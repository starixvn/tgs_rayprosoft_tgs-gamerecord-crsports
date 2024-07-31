using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;

using Hangfire;
using Hangfire.Storage;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Tgs.GameCommand.Client;
using Tgs.GameCommand.Common;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.CRSports.Core.JobApplicationServices;

internal class JobInitCommandService(
	ILogger<JobInitCommandService> logger,
	IOptions<GameRecordInfoOptions> options,
	IHangfireLock hangfireLock,
	IGameCommandClient client) : ICommandService<JobInitCommand>
{
	private readonly GameRecordInfoOptions _gameRecordInfo = options.Value;

	private readonly RecurringJobOptions _recurringJobOptions = new()
	{
		TimeZone = TimeZoneInfo.CreateCustomTimeZone(id: "eastern time", baseUtcOffset: TimeSpan.FromHours(-4), "eastern time", "eastern time")
	};

	public async ValueTask ExecuteAsync(JobInitCommand command, CancellationToken cancellationToken = default)
	{
		try
		{
			if (!await hangfireLock.LockingAsync(
				application: typeof(JobInitCommandService).Name,
				cancellationToken: cancellationToken)
				.ConfigureAwait(false))
			{
				logger.LogWarning("{logWarning}",
					JsonSerializer.Serialize(new
					{
						LogOn = DateTime.Now.ToString("u"),
						Message = "進行初始化動作，已經有其他服務進行。"
					}));
				return;
			}

			using (var connection = JobStorage.Current.GetConnection())
			{
				foreach (var recurringJob in connection.GetRecurringJobs())
				{
					RecurringJob.RemoveIfExists(recurringJob.Id);
				}
			}

			var settingConfiguration = await client.SettingGetConfigurationStringAsync(
				key: _gameRecordInfo.GameProviderConfiguration,
				cancellationToken: cancellationToken)
				.ConfigureAwait(false)
				?? throw new Exception($"This game product setting does not exist.({_gameRecordInfo.GameProviderConfiguration})");

			var settingConfigurationJson = JsonNode.Parse(settingConfiguration)
				?? throw new Exception($"This game product setting does not exist.({_gameRecordInfo.GameProviderConfiguration})");

			var gameProviderConfigs = await client.GameProviderConfigurationQueryAsync(
				gameProviderCode: _gameRecordInfo.GameProviderCode,
				cancellationToken: cancellationToken)
				.ToArrayAsync(cancellationToken: cancellationToken)
				.ConfigureAwait(false)
				?? Enumerable.Empty<GameProviderConfigInfo>();

			if (gameProviderConfigs == null || !gameProviderConfigs.Any())
			{
				return;
			}

			var gameProviderAgentMapper = new Dictionary<int, JobExecuteCommand>();

			foreach (var gameProviderConfig in gameProviderConfigs)
			{
				if (string.IsNullOrWhiteSpace(gameProviderConfig.ConfigurationData) || gameProviderConfig.Status == GameProviderConfigStatus.Inactive)
				{
					continue;
				}

				var configurationDataJson = JsonNode.Parse(gameProviderConfig.ConfigurationData)
					?? throw new Exception($"Game Provider Configuration does not exist.({gameProviderConfig.GameProviderConfigId})");

				var apiUrl = new Uri(configurationDataJson["ApiUrl"]?.ToString()
					?? settingConfigurationJson["ApiUrl"]?.ToString()
					?? throw new NullReferenceException($"{gameProviderConfig.Name} Required parameters(ApiUrl)"));
				var agentId = configurationDataJson["AgentId"]?.GetValue<int>()
					?? settingConfigurationJson["AgentId"]?.GetValue<int>()
					?? throw new NullReferenceException($"{gameProviderConfig.Name} Required parameters(AgentId)");
				var secretKey = configurationDataJson["SecretKey"]?.ToString()
					?? settingConfigurationJson["SecretKey"]?.ToString()
					?? throw new NullReferenceException($"{gameProviderConfig.Name} Required parameters(SecretKey)");
				var agentAccount = configurationDataJson["AgentAccount"]?.ToString()
					?? settingConfigurationJson["AgentAccount"]?.ToString()
					?? throw new NullReferenceException($"{gameProviderConfig.Name} Required parameters(AgentAccount)");
				var agentPassword = configurationDataJson["AgentPassword"]?.ToString()
					?? settingConfigurationJson["AgentPassword"]?.ToString()
					?? throw new NullReferenceException($"{gameProviderConfig.Name} Required parameters(AgentPassword)");

				if (gameProviderAgentMapper.ContainsKey(agentId))
				{
					continue;
				}

				var jobExecuteCommand = new JobExecuteCommand(
					ApiUrl: apiUrl,
					AgentId: agentId,
					SecretKey: secretKey,
					AgentAccount: agentAccount,
					AgentPassword: agentPassword);

				gameProviderAgentMapper.Add(agentId, jobExecuteCommand);

				var recurringJobId = $"{gameProviderConfig.Name}|{agentId}|{gameProviderConfig.GameProviderConfigId}";

				RecurringJob.AddOrUpdate<IHangfireUtensil>(
					recurringJobId: recurringJobId,
					methodCall: service => service.JobExecuteAsync(jobExecuteCommand),
					cronExpression: Cron.Minutely(),
					options: _recurringJobOptions);

				//初始化強制第一次處理
				_ = BackgroundJob.Enqueue<IHangfireUtensil>(service => service.JobExecuteAsync(jobExecuteCommand));
			}
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "{logError}",
				JsonSerializer.Serialize(new
				{
					LogOn = DateTime.Now.ToString("u"),
					Message = "遊戲廠商初始化排程發生例外"
				}));
		}
		finally
		{
			_ = BackgroundJob.Schedule<IHangfireUtensil>(service => service.JobCheckAsync(), TimeSpan.FromMinutes(1));
		}
	}
}

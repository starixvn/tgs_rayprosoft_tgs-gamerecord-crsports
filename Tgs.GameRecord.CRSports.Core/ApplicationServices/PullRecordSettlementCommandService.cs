using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tgs.GameInterface.CRSports.Client;
using Tgs.GameInterface.CRSports.Protos;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.CRSports.Core.ApplicationServices;

internal class PullRecordSettlementCommandService(
	ILogger<PullRecordSettlementCommandService> logger,
	IGameInterfaceCRSportsClient client,
	ICommandService<HandleRecordCommand> service) : ICommandService<PullRecordSettlementCommand>
{
	public async ValueTask ExecuteAsync(PullRecordSettlementCommand command, CancellationToken cancellationToken = default)
	{
		var page = 1;

		IEnumerable<GameRecordInfo> gameRecords;

		do
		{
			gameRecords = await client.GameRecordSettleAsync(
				apiUrl: command.ApiUrl,
				agentId: command.AgentId,
				secretKey: command.SecretKey,
				agentAccount: command.AgentAccount,
				agentPassword: command.AgentPassword,
				startTime: command.StartTime,
				endTime: command.EndTime,
				page: page,
				cancellationToken: cancellationToken)
				.ToArrayAsync(
				cancellationToken: cancellationToken)
				.ConfigureAwait(false)
				?? Enumerable.Empty<GameRecordInfo>();
			foreach (var gameRecord in gameRecords)
			{
				try
				{
					await service.ExecuteAsync(
						command: new(gameRecord),
						cancellationToken: cancellationToken)
						.ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "{logError}",
						JsonSerializer.Serialize(new
						{
							LogOn = DateTime.Now.ToString("u"),
							Data = new
							{
								gameRecord.PlayerCode,
								gameRecord.ExternalBetId,
								ExternalCreatedOn = gameRecord.ExternalCreatedOn.ToDateTimeOffset(),
								ExternalClosedOn = gameRecord.ExternalClosedOn.ToDateTimeOffset()
							},
							Message = "An exception occurred in this game wager."
						}));
				}
			}

			page++;
		} while (gameRecords.Any());
	}
}

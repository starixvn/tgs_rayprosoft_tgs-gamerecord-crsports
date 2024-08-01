using System.Text;
using System.Text.Json;
using Tgs.GameRecord.CRSports.Common;
using Tgs.GameRecord.CRSports.Core;
using Tgs.GameRecord.CRSports.Core.ApplicationServices;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.CRSports.Messages;

public class ScheduleMessageHandler(
	ILogger<ScheduleMessageHandler> logger,
	IPullRecordDispatcher dispatcher) : IMessageHandler<ReadOnlyMemory<byte>>
{
	public ValueTask HandleAsync(string subject, ReadOnlyMemory<byte> data, IEnumerable<MessageHeaderValue>? headerValues, CancellationToken cancellationToken = default)
	{
		var json = Encoding.UTF8.GetString(data.Span);
		var message = JsonSerializer.Deserialize<CRSportsGameRecordMessage>(json)
			?? throw new NullReferenceException("The request data resolves to a null value");

		logger.LogInformation("{logInformation}",
			JsonSerializer.Serialize(new
			{
				LogOn = DateTime.Now.ToString("u"),
				Subject = subject,
				Data = new
				{
					message.ApiUrl,
					message.AgentId,
					message.SecretKey,
					message.AgentAccount,
					message.AgentPassword,
					StartTime = message.StartTime.ToString("u"),
					EndTime = message.EndTime.ToString("u"),
				},
				Message = "This receive message handler."
			}));

		return dispatcher.PushCommandAsync(
			command: new PullRecordCommand(
			ApiUrl: message.ApiUrl,
			AgentId: message.AgentId,
			SecretKey: message.SecretKey,
			AgentAccount: message.AgentAccount,
			AgentPassword: message.AgentPassword,
			StartTime: message.StartTime,
			EndTime: message.EndTime),
			cancellationToken: cancellationToken);
	}
}

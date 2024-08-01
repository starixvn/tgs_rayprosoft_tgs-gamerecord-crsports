using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Tgs.GameRecord.CRSports.Core.ApplicationServices;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.CRSports.Core.UnitTest.ApplicationServices;
public class PullRecordCommandServiceUnitTest
{
	[Fact]
	public async Task Normal_Process()
	{
		var faceLogger = NullLogger<PullRecordCommandService>.Instance;
		var fakeUnsettlementService = Substitute.For<ICommandService<PullRecordUnsettlementCommand>>();
		var faceSettlementService = Substitute.For<ICommandService<PullRecordSettlementCommand>>();

		var command = new PullRecordCommand(
			ApiUrl: new Uri("http://localhost/"),
			AgentId: 33,
			SecretKey: "secretKey",
			AgentAccount: "agentAccount",
			AgentPassword: "agentPassword",
			StartTime: DateTimeOffset.UtcNow.AddMinutes(-10),
			EndTime: DateTimeOffset.UtcNow);

		var service = new PullRecordCommandService(
			faceLogger,
			fakeUnsettlementService,
			faceSettlementService);

		await service.ExecuteAsync(command);

		_ = fakeUnsettlementService
			.Received(1)
			.ExecuteAsync(
			command: Arg.Is(
				new PullRecordUnsettlementCommand(
					ApiUrl: command.ApiUrl,
					AgentId: command.AgentId,
					SecretKey: command.SecretKey,
					AgentAccount: command.AgentAccount,
					AgentPassword: command.AgentPassword,
					StartTime: command.StartTime,
					EndTime: command.EndTime)),
			cancellationToken: Arg.Any<CancellationToken>());

		_ = faceSettlementService
			.Received(1)
			.ExecuteAsync(
			command: Arg.Is(
				new PullRecordSettlementCommand(
					ApiUrl: command.ApiUrl,
					AgentId: command.AgentId,
					SecretKey: command.SecretKey,
					AgentAccount: command.AgentAccount,
					AgentPassword: command.AgentPassword,
					StartTime: command.StartTime,
					EndTime: command.EndTime)),
			cancellationToken: Arg.Any<CancellationToken>());
	}
}

using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Tgs.GameInterface.CRSports.Client;
using Tgs.GameRecord.CRSports.Core.ApplicationServices;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.CRSports.Core.UnitTest.ApplicationServices;

public class PullRecordUnsettlementCommandServiceUnitTest
{
	[Fact]
	public async Task Normal_Process()
	{
		var faceLogger = NullLogger<PullRecordUnsettlementCommandService>.Instance;
		var fakeClient = Substitute.For<IGameInterfaceCRSportsClient>();
		var fakeService = Substitute.For<ICommandService<HandleRecordCommand>>();

		var command = new PullRecordUnsettlementCommand(
			ApiUrl: new Uri("http://localhost/"),
			AgentId: 33,
			SecretKey: "secretKey",
			AgentAccount: "agentAccount",
			AgentPassword: "agentPassword",
			StartTime: DateTimeOffset.UtcNow.AddMinutes(-10),
			EndTime: DateTimeOffset.UtcNow);

		var service = new PullRecordUnsettlementCommandService(
			faceLogger,
			fakeClient,
			fakeService);

		await service.ExecuteAsync(command);

		_ = fakeClient
			.Received(0)
			.GameRecordSettleAsync(
				apiUrl: Arg.Is(command.ApiUrl),
				agentId: Arg.Is(command.AgentId),
				secretKey: Arg.Is(command.SecretKey),
				agentAccount: Arg.Is(command.AgentAccount),
				agentPassword: Arg.Is(command.AgentPassword),
				startTime: Arg.Is(command.StartTime),
				endTime: Arg.Is(command.EndTime),
				page: Arg.Is(1),
				cancellationToken: Arg.Any<CancellationToken>());

		_ = fakeClient
			.Received(1)
			.GameRecordBettingAsync(
				apiUrl: Arg.Is(command.ApiUrl),
				agentId: Arg.Is(command.AgentId),
				secretKey: Arg.Is(command.SecretKey),
				agentAccount: Arg.Is(command.AgentAccount),
				agentPassword: Arg.Is(command.AgentPassword),
				startTime: Arg.Is(command.StartTime),
				endTime: Arg.Is(command.EndTime),
				page: Arg.Is(1),
				cancellationToken: Arg.Any<CancellationToken>());
	}
}

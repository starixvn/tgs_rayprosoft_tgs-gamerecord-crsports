using System.Text.Json.Nodes;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Tgs.GameRecord.CRSports.Core.ApplicationServices;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.CRSports.Core.UnitTest.ApplicationServices;

public class HandleRecordCommandServiceUnitTest
{
	[Fact]
	public async Task Win()
	{
		var faceLogger = NullLogger<HandleRecordCommandService>.Instance;
		var gameRecordInfo = Options.Create(new GameRecordInfoOptions
		{
			GameProviderCode = "CR_SPORTS",
			GameProviderConfiguration = "CR_SPORTSConfiguration",
			GameExternalId = "sports_book",
			HangfirePrefix = "gamerecord-cr_sports-hangfire:",
			HangfireLocking = "gamerecord-cr_sports-locking:",
			IntervalMinutes = 10d,
			IntervalMinutesMax = 60d,
			IntervalMinutesMinimum = 1d
		});
		var fakeMessageSender = Substitute.For<IMessageSender>();

		var command = new HandleRecordCommand(
			GameRecord: new GameInterface.CRSports.Protos.GameRecordInfo
			{
				PlayerCode = "4um6m93suvtn",
				ExternalBetId = "438050329",
				ExternalCreatedOn = DateTimeOffset.Parse("2024-07-26T04:31:51-04:00").ToTimestamp(),
				ExternalClosedOn = DateTimeOffset.Parse("2024-07-26T06:19:25-04:00").ToTimestamp(),
				Status = GameInterface.CRSports.Protos.GameRecordStatus.Won,
				IsCashOut = false,
				Bet = 2.0m,
				ValidBet = 1.9m,
				Settle = 3.9m,
				WinLoss = 1.9m,
				AdditionalData = string.Empty
			});

		var service = new HandleRecordCommandService(
			faceLogger,
			gameRecordInfo,
			fakeMessageSender);

		await service.ExecuteAsync(command);

		_ = fakeMessageSender
			.Received(1)
			.PublishAsync(
			subject: Arg.Is(Handle.Common.GameRecordHandleNatsConst.GameRecordHandleRequest),
			data: Arg.Is(new Handle.Common.Protos.GameRecordHandleRequest
			{
				GameProviderCode = gameRecordInfo.Value.GameProviderCode,
				PlayerCode = command.GameRecord.PlayerCode,
				ExternalRoundId = command.GameRecord.ExternalBetId,
				ExternalBetId = command.GameRecord.ExternalBetId,
				ExternalGameId = gameRecordInfo.Value.GameExternalId,
				ExternalCreatedOn = command.GameRecord.ExternalCreatedOn,
				ExternalClosedOn = command.GameRecord.ExternalClosedOn,
				ExternalPayoffOn = command.GameRecord.ExternalClosedOn,
				ExternalResettleOn = DateTimeOffset.UnixEpoch.ToTimestamp(),
				BetState = Handle.Common.Protos.BetState.Win,
				BetType = Handle.Common.Protos.BetType.Normal,
				FreeGameState = Handle.Common.Protos.FreeGameState.None,
				GambleState = Handle.Common.Protos.GambleState.None,
				CashoutState = Handle.Common.Protos.CashoutState.None,
				ResettleState = Handle.Common.Protos.ResettleState.None,
				JackpotState = Handle.Common.Protos.JackpotState.None,
				Bet = 2.0m,
				ValidBet = 1.9m,
				Settle = 3.9m,
				Commission = 0.0m,
				Turnover = 1.9m,
				WinLoss = 1.9m,
				Jackpot = 0.0m,
				AdditionalData = command.GameRecord.AdditionalData
			}),
			cancellationToken: Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Loss()
	{
		var faceLogger = NullLogger<HandleRecordCommandService>.Instance;
		var gameRecordInfo = Options.Create(new GameRecordInfoOptions
		{
			GameProviderCode = "CR_SPORTS",
			GameProviderConfiguration = "CR_SPORTSConfiguration",
			GameExternalId = "sports_book",
			HangfirePrefix = "gamerecord-cr_sports-hangfire:",
			HangfireLocking = "gamerecord-cr_sports-locking:",
			IntervalMinutes = 10d,
			IntervalMinutesMax = 60d,
			IntervalMinutesMinimum = 1d
		});
		var fakeMessageSender = Substitute.For<IMessageSender>();

		var command = new HandleRecordCommand(
			GameRecord: new GameInterface.CRSports.Protos.GameRecordInfo
			{
				PlayerCode = "4um6m93suvtn",
				ExternalBetId = "438050329",
				ExternalCreatedOn = DateTimeOffset.Parse("2024-07-26T04:31:51-04:00").ToTimestamp(),
				ExternalClosedOn = DateTimeOffset.Parse("2024-07-26T06:19:25-04:00").ToTimestamp(),
				Status = GameInterface.CRSports.Protos.GameRecordStatus.Lose,
				IsCashOut = false,
				Bet = 2.0m,
				ValidBet = 1.0m,
				Settle = 1.0m,
				WinLoss = -1.0m,
				AdditionalData = JsonNode.Parse("""
				{
					"oddsFormat": "0.5 / 1",
					"strong": "C",
					"gtype": "FT",
					"degold": "2.000",
					"settle": "1",
					"mid": "28831508",
					"wingold_d": "0.95",
					"orderdate": "2024-07-26",
					"wingold": "1.00",
					"gold": "2.000",
					"result": "L",
					"rtype": "Running Ball-Home",
					"score": "0:0",
					"tname_home": "Real Murcia",
					"gold_d": "1.91",
					"cashoutid": "0",
					"odds": "H",
					"wtypecode": "RE",
					"currency": "HKD",
					"degold_d": "1.910",
					"id": "438050329",
					"adddate": "2024-07-26 04:31:51",
					"order": "Real Murcia",
					"cashout_d": "0.000",
					"tname_away": "Millwall",
					"handicap": "2",
					"pname": "",
					"IP": "185.196.0.16",
					"league": "Club Friendlies",
					"ioratio": "1.04",
					"rtypecode": "REH",
					"members_vgold": "1",
					"ordertime": "04:30:00",
					"cashout": "0.000",
					"report_test": "",
					"wtype": "Running Ball",
					"resultdetail": "LL",
					"vgold": "0.96",
					"resultdate": "2024-07-26 06:19:25",
					"result_score": "1:2",
					"username": "4uf6k014jp5x"
				}
				""")!.ToJsonString()
			});

		var service = new HandleRecordCommandService(
			faceLogger,
			gameRecordInfo,
			fakeMessageSender);

		await service.ExecuteAsync(command);

		_ = fakeMessageSender
			.Received(1)
			.PublishAsync(
			subject: Arg.Is(Handle.Common.GameRecordHandleNatsConst.GameRecordHandleRequest),
			data: Arg.Is(new Handle.Common.Protos.GameRecordHandleRequest
			{
				GameProviderCode = gameRecordInfo.Value.GameProviderCode,
				PlayerCode = command.GameRecord.PlayerCode,
				ExternalRoundId = command.GameRecord.ExternalBetId,
				ExternalBetId = command.GameRecord.ExternalBetId,
				ExternalGameId = gameRecordInfo.Value.GameExternalId,
				ExternalCreatedOn = command.GameRecord.ExternalCreatedOn,
				ExternalClosedOn = command.GameRecord.ExternalClosedOn,
				ExternalPayoffOn = command.GameRecord.ExternalClosedOn,
				ExternalResettleOn = DateTimeOffset.UnixEpoch.ToTimestamp(),
				BetState = Handle.Common.Protos.BetState.Loss,
				BetType = Handle.Common.Protos.BetType.Normal,
				FreeGameState = Handle.Common.Protos.FreeGameState.None,
				GambleState = Handle.Common.Protos.GambleState.None,
				CashoutState = Handle.Common.Protos.CashoutState.None,
				ResettleState = Handle.Common.Protos.ResettleState.None,
				JackpotState = Handle.Common.Protos.JackpotState.None,
				Bet = 2.0m,
				ValidBet = 1.0m,
				Settle = 1.0m,
				Commission = 0.0m,
				Turnover = 1.0m,
				WinLoss = -1.0m,
				Jackpot = 0.0m,
				AdditionalData = command.GameRecord.AdditionalData
			}),
			cancellationToken: Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Draw()
	{
		var faceLogger = NullLogger<HandleRecordCommandService>.Instance;
		var gameRecordInfo = Options.Create(new GameRecordInfoOptions
		{
			GameProviderCode = "CR_SPORTS",
			GameProviderConfiguration = "CR_SPORTSConfiguration",
			GameExternalId = "sports_book",
			HangfirePrefix = "gamerecord-cr_sports-hangfire:",
			HangfireLocking = "gamerecord-cr_sports-locking:",
			IntervalMinutes = 10d,
			IntervalMinutesMax = 60d,
			IntervalMinutesMinimum = 1d
		});
		var fakeMessageSender = Substitute.For<IMessageSender>();

		var command = new HandleRecordCommand(
			GameRecord: new GameInterface.CRSports.Protos.GameRecordInfo
			{
				PlayerCode = "4um6m93suvtn",
				ExternalBetId = "438050329",
				ExternalCreatedOn = DateTimeOffset.Parse("2024-07-26T04:31:51-04:00").ToTimestamp(),
				ExternalClosedOn = DateTimeOffset.Parse("2024-07-26T06:19:25-04:00").ToTimestamp(),
				Status = GameInterface.CRSports.Protos.GameRecordStatus.Draw,
				IsCashOut = false,
				Bet = 2.0m,
				ValidBet = 0.0m,
				Settle = 2.0m,
				WinLoss = 0.0m,
				AdditionalData = string.Empty
			});

		var service = new HandleRecordCommandService(
			faceLogger,
			gameRecordInfo,
			fakeMessageSender);

		await service.ExecuteAsync(command);

		_ = fakeMessageSender
			.Received(1)
			.PublishAsync(
			subject: Arg.Is(Handle.Common.GameRecordHandleNatsConst.GameRecordHandleRequest),
			data: Arg.Is(new Handle.Common.Protos.GameRecordHandleRequest
			{
				GameProviderCode = gameRecordInfo.Value.GameProviderCode,
				PlayerCode = command.GameRecord.PlayerCode,
				ExternalRoundId = command.GameRecord.ExternalBetId,
				ExternalBetId = command.GameRecord.ExternalBetId,
				ExternalGameId = gameRecordInfo.Value.GameExternalId,
				ExternalCreatedOn = command.GameRecord.ExternalCreatedOn,
				ExternalClosedOn = command.GameRecord.ExternalClosedOn,
				ExternalPayoffOn = command.GameRecord.ExternalClosedOn,
				ExternalResettleOn = DateTimeOffset.UnixEpoch.ToTimestamp(),
				BetState = Handle.Common.Protos.BetState.Draw,
				BetType = Handle.Common.Protos.BetType.Normal,
				FreeGameState = Handle.Common.Protos.FreeGameState.None,
				GambleState = Handle.Common.Protos.GambleState.None,
				CashoutState = Handle.Common.Protos.CashoutState.None,
				ResettleState = Handle.Common.Protos.ResettleState.None,
				JackpotState = Handle.Common.Protos.JackpotState.None,
				Bet = 2.0m,
				ValidBet = 0.0m,
				Settle = 2.0m,
				Commission = 0.0m,
				Turnover = 0.0m,
				WinLoss = 0.0m,
				Jackpot = 0.0m,
				AdditionalData = command.GameRecord.AdditionalData
			}),
			cancellationToken: Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Cashout()
	{
		var faceLogger = NullLogger<HandleRecordCommandService>.Instance;
		var gameRecordInfo = Options.Create(new GameRecordInfoOptions
		{
			GameProviderCode = "CR_SPORTS",
			GameProviderConfiguration = "CR_SPORTSConfiguration",
			GameExternalId = "sports_book",
			HangfirePrefix = "gamerecord-cr_sports-hangfire:",
			HangfireLocking = "gamerecord-cr_sports-locking:",
			IntervalMinutes = 10d,
			IntervalMinutesMax = 60d,
			IntervalMinutesMinimum = 1d
		});
		var fakeMessageSender = Substitute.For<IMessageSender>();

		var command = new HandleRecordCommand(
			GameRecord: new GameInterface.CRSports.Protos.GameRecordInfo
			{
				PlayerCode = "4uf6k014jp5x",
				ExternalBetId = "438050329",
				ExternalCreatedOn = DateTimeOffset.Parse("2024-07-26T04:31:51-04:00").ToTimestamp(),
				ExternalClosedOn = DateTimeOffset.Parse("2024-07-26T06:19:25-04:00").ToTimestamp(),
				Status = GameInterface.CRSports.Protos.GameRecordStatus.Lose,
				IsCashOut = true,
				Bet = 2.0m,
				ValidBet = 1.0m,
				Settle = 1.0m,
				WinLoss = -1.0m,
				AdditionalData = JsonNode.Parse("""
				{
					"agname": "RT706test1",
					"wager_cashout": [],
					"method": "ALLWager",
					"responeid": "22684385659",
					"page": "1",
					"agid": "3238",
					"wager_totalpage": "1",
					"respcode": "0000",
					"wager_data": [
						{
							"oddsFormat": "0.5 / 1",
							"strong": "C",
							"gtype": "FT",
							"degold": "2.000",
							"settle": "1",
							"mid": "28831508",
							"wingold_d": "0.95",
							"orderdate": "2024-07-26",
							"wingold": "1.00",
							"gold": "2.000",
							"result": "L",
							"rtype": "Running Ball-Home",
							"score": "0:0",
							"tname_home": "Real Murcia",
							"gold_d": "1.91",
							"cashoutid": "0",
							"odds": "H",
							"wtypecode": "RE",
							"currency": "HKD",
							"degold_d": "1.910",
							"id": "438050329",
							"adddate": "2024-07-26 04:31:51",
							"order": "Real Murcia",
							"cashout_d": "0.000",
							"tname_away": "Millwall",
							"handicap": "2",
							"pname": "",
							"IP": "185.196.0.16",
							"league": "Club Friendlies",
							"ioratio": "1.04",
							"rtypecode": "REH",
							"members_vgold": "1",
							"ordertime": "04:30:00",
							"cashout": "0.000",
							"report_test": "",
							"wtype": "Running Ball",
							"resultdetail": "LL",
							"vgold": "0.96",
							"resultdate": "2024-07-26 06:19:25",
							"result_score": "1:2",
							"username": "4uf6k014jp5x"
						}
					],
					"status": "success",
					"timestamp": "1722216724217"
				}
				""")!.ToJsonString()
			});

		var service = new HandleRecordCommandService(
			faceLogger,
			gameRecordInfo,
			fakeMessageSender);

		await service.ExecuteAsync(command);

		_ = fakeMessageSender
			.Received(1)
			.PublishAsync(
			subject: Arg.Is(Handle.Common.GameRecordHandleNatsConst.GameRecordHandleRequest),
			data: Arg.Is(new Handle.Common.Protos.GameRecordHandleRequest
			{
				GameProviderCode = gameRecordInfo.Value.GameProviderCode,
				PlayerCode = command.GameRecord.PlayerCode,
				ExternalRoundId = command.GameRecord.ExternalBetId,
				ExternalBetId = command.GameRecord.ExternalBetId,
				ExternalGameId = gameRecordInfo.Value.GameExternalId,
				ExternalCreatedOn = command.GameRecord.ExternalCreatedOn,
				ExternalClosedOn = command.GameRecord.ExternalClosedOn,
				ExternalPayoffOn = command.GameRecord.ExternalClosedOn,
				ExternalResettleOn = DateTimeOffset.UnixEpoch.ToTimestamp(),
				BetState = Handle.Common.Protos.BetState.Loss,
				BetType = Handle.Common.Protos.BetType.Normal,
				FreeGameState = Handle.Common.Protos.FreeGameState.None,
				GambleState = Handle.Common.Protos.GambleState.None,
				CashoutState = Handle.Common.Protos.CashoutState.Cashout,
				ResettleState = Handle.Common.Protos.ResettleState.None,
				JackpotState = Handle.Common.Protos.JackpotState.None,
				Bet = 2.0m,
				ValidBet = 1.0m,
				Settle = 1.0m,
				Commission = 0.0m,
				Turnover = 1.0m,
				WinLoss = -1.0m,
				Jackpot = 0.0m,
				AdditionalData = command.GameRecord.AdditionalData
			}),
			cancellationToken: Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Running()
	{
		var faceLogger = NullLogger<HandleRecordCommandService>.Instance;
		var gameRecordInfo = Options.Create(new GameRecordInfoOptions
		{
			GameProviderCode = "CR_SPORTS",
			GameProviderConfiguration = "CR_SPORTSConfiguration",
			GameExternalId = "sports_book",
			HangfirePrefix = "gamerecord-cr_sports-hangfire:",
			HangfireLocking = "gamerecord-cr_sports-locking:",
			IntervalMinutes = 10d,
			IntervalMinutesMax = 60d,
			IntervalMinutesMinimum = 1d
		});
		var fakeMessageSender = Substitute.For<IMessageSender>();

		var command = new HandleRecordCommand(
			GameRecord: new GameInterface.CRSports.Protos.GameRecordInfo
			{
				PlayerCode = "4uf6k014jp5x",
				ExternalBetId = "438050329",
				ExternalCreatedOn = DateTimeOffset.Parse("2024-07-26T04:31:51-04:00").ToTimestamp(),
				ExternalClosedOn = DateTime.UnixEpoch.ToTimestamp(),
				Status = GameInterface.CRSports.Protos.GameRecordStatus.Running,
				IsCashOut = false,
				Bet = 2.0m,
				ValidBet = 2.0m,
				Settle = 4.08m,
				WinLoss = 0.0m,
				AdditionalData = JsonNode.Parse("""
				{
					"agname": "RT706test1",
					"wager_cashout": [],
					"method": "ALLWager",
					"responeid": "18837809387",
					"page": "1",
					"agid": "3238",
					"wager_totalpage": "1",
					"respcode": "0000",
					"wager_data": [
						{
							"oddsFormat": "0.5 / 1",
							"strong": "C",
							"gtype": "FT",
							"degold": "2.000",
							"settle": "0",
							"mid": "28831508",
							"wingold_d": "3.90",
							"orderdate": "2024-07-26",
							"wingold": "4.08",
							"gold": "2.000",
							"result": "0",
							"rtype": "Running Ball-Home",
							"score": "0:0",
							"tname_home": "Real Murcia",
							"gold_d": "1.91",
							"cashoutid": "0",
							"odds": "H",
							"wtypecode": "RE",
							"currency": "HKD",
							"degold_d": "1.910",
							"id": "438050329",
							"adddate": "2024-07-26 04:31:51",
							"order": "Real Murcia",
							"cashout_d": "0.000",
							"tname_away": "Millwall",
							"handicap": "2",
							"pname": "",
							"IP": "185.196.0.16",
							"league": "Club Friendlies",
							"ioratio": "1.04",
							"rtypecode": "REH",
							"ordertime": "04:30:00",
							"cashout": "0.000",
							"report_test": "",
							"wtype": "Running Ball",
							"resultdetail": "0",
							"result_score": "",
							"username": "4uf6k014jp5x"
						}
					],
					"status": "success",
					"timestamp": "1721985899034"
				}
				""")!.ToJsonString()
			});

		var service = new HandleRecordCommandService(
			faceLogger,
			gameRecordInfo,
			fakeMessageSender);

		await service.ExecuteAsync(command);

		_ = fakeMessageSender
			.Received(1)
			.PublishAsync(
			subject: Arg.Is(Handle.Common.GameRecordHandleNatsConst.GameRecordHandleRequest),
			data: Arg.Is(new Handle.Common.Protos.GameRecordHandleRequest
			{
				GameProviderCode = gameRecordInfo.Value.GameProviderCode,
				PlayerCode = command.GameRecord.PlayerCode,
				ExternalRoundId = command.GameRecord.ExternalBetId,
				ExternalBetId = command.GameRecord.ExternalBetId,
				ExternalGameId = gameRecordInfo.Value.GameExternalId,
				ExternalCreatedOn = command.GameRecord.ExternalCreatedOn,
				ExternalClosedOn = DateTimeOffset.UnixEpoch.ToTimestamp(),
				ExternalPayoffOn = DateTimeOffset.UnixEpoch.ToTimestamp(),
				ExternalResettleOn = DateTimeOffset.UnixEpoch.ToTimestamp(),
				BetState = Handle.Common.Protos.BetState.Running,
				BetType = Handle.Common.Protos.BetType.Normal,
				FreeGameState = Handle.Common.Protos.FreeGameState.None,
				GambleState = Handle.Common.Protos.GambleState.None,
				CashoutState = Handle.Common.Protos.CashoutState.None,
				ResettleState = Handle.Common.Protos.ResettleState.None,
				JackpotState = Handle.Common.Protos.JackpotState.None,
				Bet = 2.0m,
				ValidBet = 2.0m,
				Settle = 0.0m,
				Commission = 0.0m,
				Turnover = 2.0m,
				WinLoss = 0.0m,
				Jackpot = 0.0m,
				AdditionalData = command.GameRecord.AdditionalData
			}),
			cancellationToken: Arg.Any<CancellationToken>());
	}
}

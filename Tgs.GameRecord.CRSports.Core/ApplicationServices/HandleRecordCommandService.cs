using System.Text.Json;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Tgs.GameRecord.Handle.Client;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.CRSports.Core.ApplicationServices;

internal class HandleRecordCommandService(
	ILogger<HandleRecordCommandService> logger,
	IOptions<GameRecordInfoOptions> gameRecordInfoOptions,
	IOptions<PolicySettingsOptions> policySettingsOptions,
	IGameRecordHandleClient client) : ICommandService<HandleRecordCommand>
{
	private readonly GameRecordInfoOptions _gameRecordInfo = gameRecordInfoOptions.Value;
	private readonly PolicySettingsOptions _policySettingsOptions = policySettingsOptions.Value;

	public async ValueTask ExecuteAsync(HandleRecordCommand command, CancellationToken cancellationToken = default)
	{
		var externalCreatedOn = command.GameRecord.ExternalCreatedOn.ToDateTimeOffset();
		var externalClosedOn = DateTimeOffset.UnixEpoch;
		var betState = command.GameRecord.Status switch
		{
			GameInterface.CRSports.Protos.GameRecordStatus.Won => Handle.Common.Protos.BetState.Win,
			GameInterface.CRSports.Protos.GameRecordStatus.Lose => Handle.Common.Protos.BetState.Loss,
			GameInterface.CRSports.Protos.GameRecordStatus.Draw => Handle.Common.Protos.BetState.Draw,
			GameInterface.CRSports.Protos.GameRecordStatus.Cancel => Handle.Common.Protos.BetState.Cancel,
			GameInterface.CRSports.Protos.GameRecordStatus.Refund => Handle.Common.Protos.BetState.Refund,
			GameInterface.CRSports.Protos.GameRecordStatus.Void => Handle.Common.Protos.BetState.Void,
			_ => Handle.Common.Protos.BetState.Running
		};
		var cashoutState = Handle.Common.Protos.CashoutState.None;
		var bet = command.GameRecord.Bet.ToDecimal().GetValueOrDefault(0);
		var validBet = command.GameRecord.Bet.ToDecimal().GetValueOrDefault(0);
		var settle = 0.0m;
		var winLoss = 0.0m;
		if (betState != Handle.Common.Protos.BetState.Running)
		{
			externalClosedOn = command.GameRecord.ExternalClosedOn.ToDateTimeOffset();
			validBet = command.GameRecord.ValidBet.ToDecimal().GetValueOrDefault(0);
			settle = command.GameRecord.Settle.ToDecimal().GetValueOrDefault(0);
			winLoss = command.GameRecord.WinLoss.ToDecimal().GetValueOrDefault(0);
			cashoutState = command.GameRecord.IsCashOut ? Handle.Common.Protos.CashoutState.Cashout : Handle.Common.Protos.CashoutState.None;
		}

		var source = new Handle.Common.ViewModels.GameRecordHandleViewModel
		{
			GameProviderCode = _gameRecordInfo.GameProviderCode,
			PlayerCode = command.GameRecord.PlayerCode,
			ExternalRoundId = command.GameRecord.ExternalBetId,
			ExternalBetId = command.GameRecord.ExternalBetId,
			ExternalGameId = _gameRecordInfo.GameExternalId,
			ExternalCreatedOn = externalCreatedOn,
			ExternalClosedOn = externalClosedOn,
			ExternalPayoffOn = externalClosedOn,
			ExternalResettleOn = DateTimeOffset.UnixEpoch,
			BetState = betState,
			BetType = Handle.Common.Protos.BetType.Normal,
			FreeGameState = Handle.Common.Protos.FreeGameState.None,
			GambleState = Handle.Common.Protos.GambleState.None,
			CashoutState = cashoutState,
			ResettleState = Handle.Common.Protos.ResettleState.None,
			JackpotState = Handle.Common.Protos.JackpotState.None,
			Bet = bet,
			ValidBet = validBet,
			Settle = settle,
			Commission = 0.0m,
			Turnover = validBet,
			WinLoss = winLoss,
			Jackpot = 0.0m,
			AdditionalData = command.GameRecord.AdditionalData
		};

		var retryCount = _policySettingsOptions.RetryCount;
		var retrySeconds = _policySettingsOptions.RetrySeconds;
		var retryPolicy = Policy
			.Handle<HttpRequestException>()
			.WaitAndRetryAsync(
				retryCount: retryCount,
				retryAttempt => TimeSpan.FromSeconds(Math.Pow(retrySeconds, retryAttempt)),
				onRetry: (exception, retryTime, retryCount, context) => logger.LogWarning(exception, "{logWarning}",
					new
					{
						LogOn = DateTime.Now.ToString("u"),
						RetryTime = retryTime,
						RetryCount = retryCount,
						Data = new
						{
							source.GameProviderCode,
							source.PlayerCode,
							source.ExternalRoundId,
							source.ExternalBetId,
							source.ExternalGameId,
							ExternalCreatedOn = source.ExternalCreatedOn.ToString("u"),
							ExternalClosedOn = source.ExternalClosedOn.ToString("u"),
							source.BetState
						},
						exception.Message
					}));

		await retryPolicy.ExecuteAsync(() => client.HandleAsync(
			source: source,
			cancellationToken: cancellationToken).AsTask())
			.ConfigureAwait(false);
	}
}

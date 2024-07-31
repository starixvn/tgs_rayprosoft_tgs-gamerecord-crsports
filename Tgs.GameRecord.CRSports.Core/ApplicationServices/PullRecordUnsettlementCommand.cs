namespace Tgs.GameRecord.CRSports.Core.ApplicationServices;

public record class PullRecordUnsettlementCommand(
	Uri ApiUrl,
	int AgentId,
	string SecretKey,
	string AgentAccount,
	string AgentPassword,
	DateTimeOffset StartTime,
	DateTimeOffset EndTime);

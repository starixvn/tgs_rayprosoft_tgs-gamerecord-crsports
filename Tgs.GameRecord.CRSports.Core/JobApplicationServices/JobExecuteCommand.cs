namespace Tgs.GameRecord.CRSports.Core.JobApplicationServices;

public record class JobExecuteCommand(
	Uri ApiUrl,
	int AgentId,
	string SecretKey,
	string AgentAccount,
	string AgentPassword);

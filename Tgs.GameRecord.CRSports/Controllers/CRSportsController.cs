using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Tgs.GameRecord.CRSports.Common;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.CRSports.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CRSportsController : ControllerBase
{
	[HttpPost("manual")]
	public ValueTask GameRecordManualAsync(
		[FromServices] IMessageSender messageSender,
		CRSportsGameRecordMessage source)
		=> messageSender.PublishAsync(
			subject: CRSportsNatsConst.ManualPublish,
			data: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(source)),
			cancellationToken: HttpContext.RequestAborted);

	[HttpPost("schedule")]
	public ValueTask GameRecordScheduleAsync(
		[FromServices] IMessageSender messageSender,
		CRSportsGameRecordMessage source)
		=> messageSender.PublishAsync(
			subject: CRSportsNatsConst.SchedulePublish,
			data: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(source)),
			cancellationToken: HttpContext.RequestAborted);
}

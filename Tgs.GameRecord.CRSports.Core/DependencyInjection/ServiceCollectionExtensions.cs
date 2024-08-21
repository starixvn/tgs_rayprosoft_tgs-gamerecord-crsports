using System.Reflection;

using Tgs.GameCommand.Client;
using Tgs.GameInterface.CRSports.Client;
using Tgs.GameRecord.CRSports.Core;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddGameRecordCore(this IServiceCollection services)
		=> services
		.RegisterAllServiceUtilityTypes(Assembly.GetExecutingAssembly(), ServiceLifetime.Transient)
		.AddSingleton(TimeProvider.System)
		.AddSingleton<IPullRecordDispatcher, PullRecordDispatcher>();
}

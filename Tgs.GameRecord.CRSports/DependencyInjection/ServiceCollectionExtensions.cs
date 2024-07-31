using Tgs.GameCommand.Client;
using Tgs.GameInterface.CRSports.Client;
using Tgs.GameRecord.CRSports;
using Tgs.GameRecord.CRSports.Core;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddGameRecord(
	   this IServiceCollection services,
	   Action<RedisOptions, IServiceProvider> redisDBconfigure,
	   Action<GameRecordInfoOptions, IServiceProvider> gameRecordInfoOptions,
	   Action<GameCommandClientOptions, IServiceProvider> gameCommandClientOptions,
	   Action<GameInterfaceCRSportsClientOptions, IServiceProvider> gameInterfaceCRSportsClientOptions)
		=> services
		.AddOptions<RedisOptions>()
		.Configure(redisDBconfigure)
		.Services
		.AddOptions<GameRecordInfoOptions>()
		.Configure(gameRecordInfoOptions)
		.Services
		.AddSingleton<RedisConnectionManager>()
		.AddGameRecordCore(
			gameCommandClientOptions,
			gameInterfaceCRSportsClientOptions)
		.AddGameRecordRepository(
			redisDatabaseResolver: (sp) => sp.GetRequiredService<RedisConnectionManager>().Database);
}

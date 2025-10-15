using System.Net;
using T1_KNative_Administrator_v03.Core.OpResult;
using T1_KNative_Administrator_v03.Core.Prometheus.Response;
using T1_KNative_Administrator_v03.Infrastructure.Repositories.FunctionsInfoRepository;

namespace T1_KNative_Administrator_v03.Infrastructure.Services.FunctionsManagerService
{
    public class FunctionsStatsManagerService
    {
		public IServiceProvider services;
		public FunctionsStatsManagerService(IServiceProvider service)
		{
			services = service;
		}

		public async Task<OpResult> WriteMetrics(string fullFunctionName, PrometheusData scrappedQueryData, string query)
		{
			using (var scope = services.CreateScope())
			{
				var functionsRepos = scope.ServiceProvider.GetRequiredService<FunctionsInfoRepository>();
				var functionEntity = functionsRepos.Get(fullFunctionName);
				if (functionEntity == null) {
					OpResult.Error("Function Not Found", HttpStatusCode.NotFound);
				}
				functionsRepos.UpdateFunctionInfo(fullFunctionName, func =>
				{
				/* TODO 
				 * Здесь будет пайплайн врайтеров
				 * 
				 */
				});
				return OpResult.Success();
			}
		}

	}
}

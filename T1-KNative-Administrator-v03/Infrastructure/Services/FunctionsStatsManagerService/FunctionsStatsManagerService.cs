using System.Net;
using T1_KNative_Administrator_v03.Core.OpResult;
using T1_KNative_Administrator_v03.Core.Prometheus.Response;
using T1_KNative_Administrator_v03.Infrastructure.Repositories.FunctionsInfoRepository;
using T1_KNative_Administrator_v03.Infrastructure.Services.FunctionsStatsManagerService.WriteDataProfiles.RAMStatsProfile;
using T1_KNative_Administrator_v03.Infrastructure.Services.FunctionsStatsManagerService.WriteDataProfiles.VCPUTimeStatsProfile;

namespace T1_KNative_Administrator_v03.Infrastructure.Services.FunctionsManagerService
{
    public class FunctionsStatsManagerService
    {
        public IServiceProvider services;
        public FunctionsStatsManagerService(IServiceProvider service)
        {
            services = service;
        }
        RAMStatsProfile RAMStatsProfile { get; set; } = new RAMStatsProfile();
        VCPUTimeStatsProfile VCPUTimeStatsProfile { get; set; } = new VCPUTimeStatsProfile();

        public async Task<OpResult> WriteMetrics(string fullFunctionName, PrometheusData scrappedQueryData, string query)
        {
            using (var scope = services.CreateScope())
            {
                var functionsRepos = scope.ServiceProvider.GetRequiredService<FunctionsInfoRepository>();
                var functionEntity = functionsRepos.Get(fullFunctionName);
                if (functionEntity == null)
                {
                    OpResult.Error("Function Not Found", HttpStatusCode.NotFound);
                }
                await functionsRepos.UpdateFunctionInfo(fullFunctionName, func =>
                {
                    if (query.StartsWith("container_memory_usage_bytes"))
                        RAMStatsProfile.WriteStatsMetric(functionEntity, scrappedQueryData, query);
                    else if (query.StartsWith("increase(container_cpu_usage_seconds_total"))
                        VCPUTimeStatsProfile.WriteStatsMetric(functionEntity, scrappedQueryData, query);
                });
                return OpResult.Success();
            }
        }

    }
}

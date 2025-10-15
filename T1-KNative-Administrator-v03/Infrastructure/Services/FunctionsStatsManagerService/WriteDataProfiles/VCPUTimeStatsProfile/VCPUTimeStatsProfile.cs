using System.Net;
using T1_KNative_Admin_v02.Core.Function;
using T1_KNative_Administrator_v03.Core.OpResult;
using T1_KNative_Administrator_v03.Core.Prometheus.Response;
using T1_KNative_Administrator_v03.Infrastructure.Services.FunctionsStatsManagerService.WriteDataProfiles.BaseProfile;

namespace T1_KNative_Administrator_v03.Infrastructure.Services.FunctionsStatsManagerService.WriteDataProfiles.VCPUTimeStatsProfile
{
    public class VCPUTimeStatsProfile : BaseStatsWriterProfile
    {
        public override OpResult WriteStatsMetric(FunctionEntity functionData, PrometheusData metricData, string query)
        {
            var cpuData = functionData.vCpuStats;
            foreach (var result in metricData.Result)
            {
                foreach (var cpuMetric in result.Values)
                {
                    bool parseRes = Decimal.TryParse(cpuMetric.Value, out Decimal cpuSeconds);
                    if (!parseRes)
                        return OpResult.Error("Parsing Failed", HttpStatusCode.InternalServerError);
                    if (cpuData.MaxRunningTimeMS == null || cpuSeconds * 1000 >= cpuData.MaxRunningTimeMS)
                    {
                        cpuData.MaxRunningTimeMS = cpuSeconds * 1000;
                    }
                    if (cpuData.AvgRunningTimeMS == null)
                    {
                        cpuData.AvgRunningTimeMS = cpuSeconds * 1000;
                    }
                    else
                    {
                        //TODO Optimisation \\\ sroki goriat
                        cpuData.AvgRunningTimeMS = (cpuData.AvgRunningTimeMS * cpuData.MetricsCounts + cpuSeconds * 1000) / (++cpuData.MetricsCounts);
                    }
                }
            }
            return OpResult.Success();
        }
    }
}

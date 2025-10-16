using System.Net;
using T1_KNative_Admin_v02.Core.Function;
using T1_KNative_Administrator_v03.Core.OpResult;
using T1_KNative_Administrator_v03.Core.Prometheus.Response;
using T1_KNative_Administrator_v03.Infrastructure.Services.FunctionsStatsManagerService.WriteDataProfiles.BaseProfile;

namespace T1_KNative_Administrator_v03.Infrastructure.Services.FunctionsStatsManagerService.WriteDataProfiles.RequestCounterStatsProfile
{
    public class RequestCounterStatsProfile : BaseStatsWriterProfile
    {
        public override OpResult WriteStatsMetric(FunctionEntity functionData, PrometheusData metricData, string query)
        {
            var requestData = functionData.requestsCounterStats;
            foreach (var result in metricData.Result)
            {
                foreach (var reqMetric in result.Values)
                {
                    bool parseRes = ulong.TryParse(reqMetric.Value, out ulong requests);
                    if (!parseRes)
                        return OpResult.Error("Parsing Failed", HttpStatusCode.InternalServerError);

                    var today = DateOnly.FromDateTime(DateTime.Today);
                    if (requestData.RequestsCountByDay.TryGetValue(today, out var value))
                        requestData.RequestsCountByDay[today] = value + requests;
                    else
                        requestData.RequestsCountByDay[today] = requests;
                }
            }
            return OpResult.Success();
        }
    }
}

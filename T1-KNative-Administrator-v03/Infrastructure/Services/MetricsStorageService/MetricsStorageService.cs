using System.Collections.Concurrent;
using T1_KNative_Administrator_v03.Core.Prometheus.Response;

namespace T1_KNative_Administrator_v03.Infrastructure.Services.MetricsStorageService
{
    public class MetricsStorageService
    {
        public ConcurrentDictionary<string, List<PrometheusData>> FunctionsMetrics { get; set; } = new ConcurrentDictionary<string, List<PrometheusData>>();
        public bool WriteMetrics(string fullFuncName, PrometheusData prometheusData)
        {
            bool res = FunctionsMetrics.TryGetValue(fullFuncName, out var datas);
            if (res)
            {
                datas.Add(prometheusData);
            }
            else
            {
                res = FunctionsMetrics.TryAdd(fullFuncName, new List<PrometheusData> { prometheusData });
            }
            return res;
        }
    }
}

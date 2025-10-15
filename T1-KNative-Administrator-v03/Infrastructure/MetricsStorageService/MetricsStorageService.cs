using System.Collections.Concurrent;
using T1_KNative_Administrator_v03.Core.Prometheus.Response;

namespace T1_KNative_Administrator_v03.Infrastructure.MetricsStorageService
{
	public class MetricsStorageService
	{
		public ConcurrentDictionary<string, PrometheusData> FunctionsMetrics { get; set; } = new ConcurrentDictionary<string, PrometheusData>();
		public bool WriteMetrics(string fullFuncName, PrometheusData prometheusData)
		{
			var res = FunctionsMetrics.TryAdd(fullFuncName, prometheusData);
			return res;
		}
	}
}

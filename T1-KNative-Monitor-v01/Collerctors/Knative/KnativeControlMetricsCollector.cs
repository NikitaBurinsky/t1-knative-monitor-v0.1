using Prometheus;
using T1_KNative_Monitor_v01.Collerctors.Prometheus.PrometheusCollectorBase;
using T1_KNative_Monitor_v01.Collerctors.Prometheus.Response;

public class KnativeControlMetricsCollector : PrometheusCollectorBase
{
	private readonly HttpClient _http;
	public KnativeControlMetricsCollector(HttpClient http) : base(http)
	{ }

	public static Dictionary<string, List<string>> servicesMetricQueries = new Dictionary<string, List<string>>
	{
		{ "autoscaler", new List<string> {
				"actual_pods",
				"desired_pods",
				"excess_burst_capacity",
				"stable_request_concurrency",
				"panic_request_concurrency\t",
				"target_concurrency_per_pod",
				"stable_requests_per_second",
				"panic_requests_per_second",
				"target_requests_per_second",
				"panic_mode",
				"requested_pods",
				"actual_pods",
				"not_ready_pods",
				"pending_pods",
				"terminating_pods",
				"scrape_time"
		}},
		{ "activator", new List<string>{
				"request_concurrency",
				"request_count",
				"request_latencies",
		}},

	};

	public async Task CollectAsync()
	{
		foreach (var metricService in servicesMetricQueries)
		{
			foreach (var metricQuery in metricService.Value)
			{
				string query = metricService.Key + "_" + metricQuery;
				var res = await GetBodyStringAsync(query, 60, new List<(string p, string v)>
				{
					new ("service_name", TestPodConfig.FullName),
				});
				var respObject = DeserialiseResponse(res);
				PrintQueryResponse(respObject, query,ConsoleColor.Green);
			}
		}
	}
}

public static class AppMetrics
{
	public static readonly Counter BusinessOps = Metrics
		.CreateCounter("app_business_operations_total", "Number of business operations executed");
}

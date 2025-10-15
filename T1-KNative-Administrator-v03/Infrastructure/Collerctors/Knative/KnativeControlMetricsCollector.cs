using Prometheus;
using T1_KNative_Administrator_v03.Core.Prometheus.Response;
using T1_KNative_Administrator_v03.Infrastructure.Services.FunctionsManagerService;
using T1_KNative_Monitor_v01.Collerctors.Prometheus.PrometheusCollectorBase;

public class KnativeControlMetricsCollector : PrometheusCollectorBase
{
    private readonly HttpClient _http;
    FunctionsStatsManagerService FunctionsStatsManagerService { get; set; }
    IConfiguration _configuration;
    public static Dictionary<string, List<string>> servicesMetricQueries;
    private string servingName;
    private string revisionName;
    private string podName;
    private string fullName;


    public KnativeControlMetricsCollector(HttpClient http, FunctionsStatsManagerService metrics, IConfiguration configuration) : base(http)
    {
        FunctionsStatsManagerService = metrics;
        _configuration = configuration;
        servingName = _configuration["Seeding:FunctionsInfo:ServingName"];
        revisionName = _configuration["Seeding:FunctionsInfo:RevisionName"];
        podName = _configuration["Seeding:FunctionsInfo:PODName"];
        fullName = servingName + "-" + revisionName + "-" + podName;

        var interval = _configuration["Collectors:Prometheus:CollectingDelaysSeconds"];

        servicesMetricQueries = new Dictionary<string, List<string>>
    {
        { "autoscaler", new List<string> {
                "actual_pods",
                "desired_pods",
                "excess_burst_capacity",
                "stable_request_concurrency",
                "panic_request_concurrency",
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
        { "container", new List<string> {
                "memory_usage_bytes{pod=" + $"\"{fullName}\"" + ", container=\"user-container\"}"
        }},
        { "", new List<string> {
                "increase(container_cpu_usage_seconds_total{pod=" + $"\"{fullName}\"" + ", container=\"user-container\"}[" + $"{interval}" + "])"
        }},
    };
    }


    public async Task CollectAsync()
    {

        foreach (var metricService in servicesMetricQueries)
        {
            foreach (var metricQuery in metricService.Value)
            {
                string query = metricService.Key == "" ? metricQuery : metricService.Key + "_" + metricQuery;
                var res = await GetBodyStringAsync(query, 60, new List<(string p, string v)>
                {
                    new ("service_name", fullName),
                });
                PrometheusQueryResponse respObject = DeserialiseResponse(res);
                if (respObject != null && respObject.Status == "success")
                {
                    await FunctionsStatsManagerService.WriteMetrics(fullName, respObject.Data, query);
                    PrintQueryResponse(respObject, query, ConsoleColor.Green);
                }
            }
        }
    }
}



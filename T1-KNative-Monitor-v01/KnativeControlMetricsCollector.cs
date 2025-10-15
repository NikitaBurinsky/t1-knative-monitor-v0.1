using Prometheus;
using System.Net.Http.Json;

public class KnativeControlMetricsCollector
{
    private readonly HttpClient _http;

    public KnativeControlMetricsCollector(HttpClient http)
    {
        _http = http;
    }

    public async Task CollectAsync()
    {
        // Example: query autoscaler desired pod count
        string query = "autoscaler_desired_pod_count";
        string url = $"http://prometheus.monitoring.svc.cluster.local:9090/api/v1/query?query={query}";

        var resp = await _http.GetFromJsonAsync<PrometheusQueryResponse>(url);

        if (resp?.Data?.Result != null)
        {
            foreach (var r in resp.Data.Result)
            {
                Console.WriteLine($"[Knative Control] {query} service={r.Metric.GetValueOrDefault("service")} value={r.Value[1]}");
            }
        }
    }
}

public static class AppMetrics
{
    public static readonly Counter BusinessOps = Metrics
        .CreateCounter("app_business_operations_total", "Number of business operations executed");
}

using System.Net.Http.Json;

public class KnativeMetricsCollector
{
    private readonly HttpClient _http;

    public KnativeMetricsCollector(HttpClient http)
    {
        _http = http;
    }

    public async Task CollectAsync()
    {
        // Example: query Knative autoscaler concurrency metric
        string query = "autoscaler_actual_pod_count";
        var url = $"http://prometheus.monitoring.svc.cluster.local:9090/api/v1/query?query={query}";

        var response = await _http.GetFromJsonAsync<PrometheusQueryResponse>(url);

        if (response?.Data?.Result != null)
        {
            foreach (var result in response.Data.Result)
            {
                Console.WriteLine($"[Knative] {query} = {result.Value[1]}");
            }
        }
    }
}

public class PrometheusQueryResponse
{
    public string Status { get; set; }
    public PrometheusData Data { get; set; }
}

public class PrometheusData
{
    public string ResultType { get; set; }
    public List<PrometheusResult> Result { get; set; }
}

public class PrometheusResult
{
    public Dictionary<string, string> Metric { get; set; }
    public List<object> Value { get; set; }
}

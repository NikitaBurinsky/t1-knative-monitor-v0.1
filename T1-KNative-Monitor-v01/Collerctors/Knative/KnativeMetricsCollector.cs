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
        var url = $"http://localhost:9090/api/v1/query?query={query}";

        var response = await _http.GetAsync(url);

		Console.ForegroundColor = ConsoleColor.Green;
		Console.WriteLine("=== KNative::MetricsCollector");
		Console.WriteLine(await response.Content.ReadAsStringAsync());
		Console.ResetColor();
		/*
        if (response?.Data?.Result != null)
        {
            foreach (var result in response.Data.Result)
            {
                Console.WriteLine($"[Knative] {query} = {result.Value[1]}");
            }
        }*/
    }
}
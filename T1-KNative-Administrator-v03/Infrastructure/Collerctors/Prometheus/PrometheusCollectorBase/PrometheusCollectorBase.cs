using System.Text.Json;
using T1_KNative_Administrator_v03.Core.Prometheus.Response;

namespace T1_KNative_Monitor_v01.Collerctors.Prometheus.PrometheusCollectorBase
{
    public class PrometheusCollectorBase
	{
		private string HostPort = "localhost:9090";
		private const int StatsCollectionDelayRangeMinutes = 5;
		private readonly HttpClient _http;

		public readonly POD TestPodConfig = new POD(
			new Revision(new Serving("echo-00001"), "deployment"), "5f657c6b6b-dkmhk");

		public record Serving(string servingName);
		public record Revision(Serving Serving, string revisionName);
		public record POD(Revision Revision, string podName)
		{
			public string FullName = $"{Revision.Serving.servingName}-{Revision.revisionName}-{podName}";
		};

		public PrometheusCollectorBase(HttpClient http)
		{
			_http = http;
		}

		public async Task<string> GetBodyStringAsync(string query, int step = 60, List<(string param, string value)> addictionalParameters = null)
		{
			var url = $"http://localhost:9090/api/v1/query_range" +
			$"?query={query}" +
			$"&step={step}" +
			$"&start={DateTimeOffset.Now.ToUnixTimeSeconds() - StatsCollectionDelayRangeMinutes * 60}" +
			$"&end={DateTimeOffset.Now.ToUnixTimeSeconds()}";
			if (addictionalParameters != null)
			{
				foreach (var param in addictionalParameters)
				{
					url += $"&{param.param}={param.value}";
				}
			}

			return await (await _http.GetAsync(url)).Content.ReadAsStringAsync();
		}
		public void PrintQueryResponse(PrometheusQueryResponse response, string query = "-", ConsoleColor color = ConsoleColor.DarkGreen)
		{
			Console.ForegroundColor = color;
			if (response == null)
			{
				Console.WriteLine("Null response");
				return;
			}
			Console.WriteLine($"Query: {query}");
			Console.WriteLine($"Status: {response.Status}, Results: {response.Data?.Result?.Count ?? 0}");

			if (response.Data?.Result == null) return;

			foreach (var result in response.Data.Result)
			{
				var serviceName = result.Metric?.GetValueOrDefault("service_name") ?? "unknown";
				var revision = result.Metric?.GetValueOrDefault("revision_name") ?? "unknown";
				var points = result.Values?.Count ?? 0;

				Console.WriteLine($"Service: {serviceName}, Revision: {revision}, Data Points: {points}");

				// Вывод первых 3 значений для примера
				if (result.Values?.Count > 0)
				{
					for (int i = 0; i < Math.Min(3, result.Values.Count); i++)
					{
						var (timestamp, value) = result.Values[i];
						var time = DateTimeOffset.FromUnixTimeSeconds(timestamp).ToString("HH:mm:ss");
						Console.WriteLine($"  {time}: {value}");
					}
					if (result.Values.Count > 3)
						Console.WriteLine($"  ... and {result.Values.Count - 3} more");
				}
				Console.WriteLine();
			}
			Console.ResetColor();
		}

		protected PrometheusQueryResponse DeserialiseResponse(string response)
		{
			JsonSerializerOptions options = new JsonSerializerOptions()
			{
				AllowTrailingCommas = true,
				PropertyNameCaseInsensitive = true,
			};
			var res = JsonSerializer.Deserialize<PrometheusQueryResponse>(response, options);
			return res;
		}

	}
};


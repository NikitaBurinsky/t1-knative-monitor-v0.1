using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace T1_KNative_Monitor_v01.Collerctors.Prometheus.Response
{
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
		[JsonPropertyName("values")]
		public List<List<JsonElement>> RawValues { get; set; }

		// Вычисляемое свойство для типизированного доступа к значениям
		[JsonIgnore]
		public List<(long Timestamp, string Value)> Values =>
		RawValues?.ConvertAll(item =>
		{
			var timestamp = item[0].GetInt64();
			var value = item[1].GetString();
			return (timestamp, value);
		}) ?? new List<(long, string)>();
	}
}
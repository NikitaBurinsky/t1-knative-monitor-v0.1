using System.Net;
using T1_KNative_Admin_v02.Core.Function;
using T1_KNative_Administrator_v03.Core.OpResult;
using T1_KNative_Administrator_v03.Core.Prometheus.Response;
using T1_KNative_Administrator_v03.Infrastructure.Services.FunctionsStatsManagerService.WriteDataProfiles.BaseProfile;

namespace T1_KNative_Administrator_v03.Infrastructure.Services.FunctionsStatsManagerService.WriteDataProfiles.RAMStatsProfile
{
	public class RAMStatsProfile : BaseStatsWriterProfile
	{
		/// <summary>
		/// Triggers on "container_memory_usage_bytes"
		/// </summary>
		public override OpResult WriteStatsMetric(FunctionEntity functionData, PrometheusData metricData, string query)
		{
			var ramData = functionData.RamStats;
			foreach (var result in metricData.Result)
			{
				foreach(var ramMetric in result.Values)
				{
					bool parseRes = ulong.TryParse(ramMetric.Value, out ulong ramUsage);
					if (!parseRes)
						return OpResult.Error("Parsing Failed", HttpStatusCode.InternalServerError);
					if(ramData.MaxRAMStatsBytes == null || ramUsage >= ramData.MaxRAMStatsBytes)
					{
						ramData.MaxRAMStatsBytes = ramUsage;
					}
					if(ramData.AvgRAMStatsBytes == null) 
					{ 
						ramData.AvgRAMStatsBytes = ramUsage;
					}
					else {
						//TODO Optimisation \\\ sroki goriat
						ramData.AvgRAMStatsBytes = (ramData.AvgRAMStatsBytes * ramData.MetricsCounts + ramUsage)/ (++ramData.MetricsCounts);
					}
				}
			}
			return OpResult.Success();
		}
	}
}

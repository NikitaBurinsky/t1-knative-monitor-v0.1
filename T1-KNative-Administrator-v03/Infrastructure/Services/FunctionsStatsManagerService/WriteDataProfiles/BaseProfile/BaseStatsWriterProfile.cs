using T1_KNative_Admin_v02.Core.Function;
using T1_KNative_Administrator_v03.Core.OpResult;
using T1_KNative_Administrator_v03.Core.Prometheus.Response;

namespace T1_KNative_Administrator_v03.Infrastructure.Services.FunctionsStatsManagerService.WriteDataProfiles.BaseProfile
{
	public abstract class BaseStatsWriterProfile<StatsClass>
	{
		public abstract OpResult WriteStatsMetric(FunctionEntity functionData, PrometheusData metricData);
	}
}

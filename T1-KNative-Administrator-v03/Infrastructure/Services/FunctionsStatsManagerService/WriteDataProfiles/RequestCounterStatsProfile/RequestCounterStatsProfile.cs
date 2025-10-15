using T1_KNative_Admin_v02.Core.Function;
using T1_KNative_Administrator_v03.Core.OpResult;
using T1_KNative_Administrator_v03.Core.Prometheus.Response;
using T1_KNative_Administrator_v03.Infrastructure.Services.FunctionsStatsManagerService.WriteDataProfiles.BaseProfile;

namespace T1_KNative_Administrator_v03.Infrastructure.Services.FunctionsStatsManagerService.WriteDataProfiles.RequestCounterStatsProfile
{
	public class RequestCounterStatsProfile : BaseStatsWriterProfile
	{
		public override OpResult WriteStatsMetric(FunctionEntity functionData, PrometheusData metricData, string query)
		{






			throw new NotImplementedException();
		}
	}
}

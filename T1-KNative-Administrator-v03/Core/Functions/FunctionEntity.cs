using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T1_KNative_Admin_v02.Core.Function
{
	public class FunctionEntity
	{
		public int Id { get; set; }

		public FunctionEntity() { }
		public string FullName { get; set; }
		public string RevisionName { get; set; }
		public string ServingName { get; set; }
		public string PODName { get; set; }

		/////// METRICS ///////
		public RunningTimeStats RunTimeStats { get; set; } = new RunningTimeStats();
		public VCPUTimeStats vCpuStats { get; set; } = new VCPUTimeStats();
		public RequestsCounterStats requestsCounterStats { get; set; } = new RequestsCounterStats();
		public RAMStats RamStats {  get; set; } = new RAMStats();

		[Owned]
		public class RunningTimeStats
		{
			public ulong? MaxRunningTimeMS { get; set; }
			public ulong? AvgRunningTimeMS { get; set; }
			public ulong MetricsCounts { get; set; } = 0;
		}

		[Owned]
		public class VCPUTimeStats
		{
			public ulong? MaxRunningTimeMS { get; set; }
			public ulong? AvgRunningTimeMS { get; set; }
			public ulong MetricsCounts { get; set; } = 0;
		}

		[Owned]
		public class RequestsCounterStats
		{
			/// <summary>
			/// Количество запросов в этот день
			/// </summary>
			public Dictionary<DateOnly, ulong> RequestsCountByDay = new Dictionary<DateOnly, ulong>(); 
		}

		[Owned]
		public class RAMStats
		{
			public ulong? MaxRAMStatsBytes { get; set; }
			public ulong? AvgRAMStatsBytes {  get; set; }
			public ulong MetricsCounts { get; set; } = 0;

		}

	}
}

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
		public FunctionEntity(string Serving, string Revision, string POD)
		{
			RevisionName = Revision;
			ServingName = Serving;
			PODName = POD;
			FullName = ServingName + "-" + RevisionName + "-" + PODName;
		}
		public string FullName { get; }
		public string RevisionName { get; }
		public string ServingName { get; }
		public string PODName { get; }

		///////METRICS///////
		public SSDReservingStats SSDStats { get; set; } = new SSDReservingStats();
		public RunningTimeStats RunTimeStats { get; set; } = new RunningTimeStats();
		public VCPUTimeStats vCpuStats { get; set; } = new VCPUTimeStats();
		public RequestsCounterStats requestsCounterStats { get; set; } = new RequestsCounterStats();
		public RAMStats RamStats {  get; set; } = new RAMStats();

		[Owned]
		public class SSDReservingStats
		{
			public ulong ReservedSpaceMB {  get; set; }
			public ulong MaxUsedSpaceMB { get; set; }
			public ulong AbgUsedSpaceMB { get; set; }
		}

		[Owned]
		public class RunningTimeStats
		{
			public ulong MaxRunningTimeMS { get; set; }
			public ulong AbgRunningTimeMS { get; set; }
		}

		[Owned]
		public class VCPUTimeStats
		{
			public ulong MaxRunningTimeMS { get; set; }
			public ulong AbgRunningTimeMS { get; set; }

		}

		[Owned]
		public class RequestsCounterStats
		{
			public Dictionary<DateOnly, ulong> RequestsCountByDay = new Dictionary<DateOnly, ulong>();
		}

		[Owned]
		public class RAMStats
		{
			public ulong MaxRAMStatsMB { get; set; }
			public ulong AvgRAMStatsMB {  get; set; }
			public ulong ReservedRAMStatsMB { get; set; }
		}

	}
}

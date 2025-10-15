using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T1_KNative_Admin_v02.Core.Function
{
	public class Function
	{
		public Function(string Serving, string Revision, string POD)
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

		///////////////////////////////
	}
}

using System.Net;
using T1_KNative_Admin_v02.Core.Function;
using T1_KNative_Administrator_v03.Core.OpResult;

namespace T1_KNative_Administrator_v03.Infrastructure.Repositories.FunctionsInfoRepository
{
	public class FunctionsInfoRepository
	{
		private ApplicationDbContext dbContext;
		public OpResult<int> AddFunctionInfo(string servingName, string revisionName, string podName)
		{
			if (dbContext.Functions.Any(x => x.RevisionName == revisionName
			&& x.ServingName == servingName
			&& x.PODName == podName))
				return OpResult<int>.Error("Similar Name Exists", HttpStatusCode.BadRequest);




	
		}
		public OpResult<int> UpdateFunctionInfo(string fullName, Action<FunctionEntity> action)
		{

		}

	}
}

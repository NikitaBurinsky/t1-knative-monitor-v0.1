using System.Collections.Concurrent;
using System.Net;
using T1_KNative_Admin_v02.Core.Function;
using T1_KNative_Administrator_v03.Core.OpResult;
using T1_KNative_Administrator_v03.Infrastructure.Repositories.FunctionsInfoRepository;

namespace T1_KNative_Administrator_v03.Infrastructure.Services.BillingService
{
	public class BillingService
	{
		FunctionCostCalculator FunctionCostCalculator { get; set; }
		public ConcurrentDictionary<int, int> users_functions = new ConcurrentDictionary<int, int>();
		FunctionsInfoRepository repository;

		public BillingService(FunctionCostCalculator functionCostCalculator, 
			FunctionsInfoRepository repository)
		{
			FunctionCostCalculator = functionCostCalculator;
			this.repository = repository;
		}

		public OpResult StartPeriod(string servName, string revisionName, string podName, int UserId = 1)
		{
			bool containsSimilar = repository.FunctionEntities
				.Any(e => e.UserId == UserId && e.FullName == servName + "-" + revisionName + "-" + podName);
			if (containsSimilar) {
				return OpResult.Error("Similar Function Exists", HttpStatusCode.BadRequest);
			}
			var res = repository.CreateFunctionInfo(servName, revisionName, podName);
			return res.Result.Succeeded ?
				OpResult.Success()
				: OpResult.Error("Cannot create period", HttpStatusCode.InternalServerError);
		}

		public OpResult<Bill> EndPeriod(string funcFullName, int UserId = 1)
		{
			var x = repository.FunctionEntities.FirstOrDefault(e => e.UserId == UserId && e.FullName == funcFullName);
			if (x == null)
				return OpResult<Bill>.Error("Function Not Found", HttpStatusCode.NotFound);
			var resBill = BillFunction(x);
			repository.DeleteFunctionInfo(funcFullName, UserId);
			return OpResult<Bill>.Success(resBill);
		}

		private Bill BillFunction(FunctionEntity func)
		{
		var res = FunctionCostCalculator.CalculateCost(func,
				DateOnly.FromDateTime(func.CreatedAt),DateOnly.FromDateTime(DateTime.Now));
			return new Bill
			{
				functionUsageInfo = func,
				resultPrice = res,
			};
		}

		public class Bill
		{
			public decimal resultPrice { get; set; }
			public FunctionEntity functionUsageInfo { get; set; }
		}
	}
}

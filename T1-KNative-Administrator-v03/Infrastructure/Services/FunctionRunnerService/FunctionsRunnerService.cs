using System.Net;
using T1_KNative_Administrator_v03.Core.OpResult;

namespace T1_KNative_Administrator_v03.Infrastructure.Services.FunctionRunnerService
{
	public class FunctionsRunnerService
	{
		HttpClient HttpClient { get; set; }

		public FunctionsRunnerService(HttpClient httpClient)
		{
			HttpClient = httpClient;
		}

		public async Task<OpResult> RunFunctions(string functionAddress, int runTimes, int runDelay)
		{
			try
			{
				for (int i = 0; i < runTimes; i++)
				{
					await HttpClient.GetAsync(functionAddress);
					if (runDelay > 0 && i < runTimes - 1)
					{
						await Task.Delay(runDelay);
					}
				}
			}
			catch (Exception ex) {
				return OpResult.Error("Function running not found : " + ex.Message, HttpStatusCode.BadRequest);
			}
			return OpResult.Success();
		}
	}



	}
}

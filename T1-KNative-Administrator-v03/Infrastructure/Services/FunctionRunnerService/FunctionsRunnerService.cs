using System.Net;
using T1_KNative_Administrator_v03.Core.OpResult;

namespace T1_KNative_Administrator_v03.Infrastructure.Services.FunctionRunnerService
{
	public class FunctionsRunnerService
	{
		
		public FunctionsRunnerService()
		{
		}

		public async Task<OpResult> RunFunctions(string functionAddress, int runTimes, int runDelay)
		{
			HttpClient httpClient = new HttpClient();
			httpClient.DefaultRequestHeaders.Add("Host", "echo.default.knative.demo.com");
			try
			{
				for (int i = 0; i < runTimes; i++)
				{
					await httpClient.GetAsync(functionAddress);
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


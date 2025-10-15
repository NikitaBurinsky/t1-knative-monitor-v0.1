using Microsoft.AspNetCore.Mvc;
using T1_KNative_Administrator_v03.Infrastructure.Repositories.FunctionsInfoRepository;
using T1_KNative_Administrator_v03.Infrastructure.Services.FunctionsManagerService;
using static T1_KNative_Monitor_v01.Collerctors.Prometheus.PrometheusCollectorBase.PrometheusCollectorBase;

namespace T1_KNative_Administrator_v03.Controllers
{
    [ApiController]
	[Route("runner")]
	public class FunctionRunnerController : ControllerBase
	{
		[HttpPost("echo")]
		public IActionResult RunEcho(
			[FromQuery] int runTimes = 1,
			[FromQuery] int runDelay = 0)
		{
			return NotFound();
		}

		[HttpGet("echo/get-metrics")]
		public IActionResult GetMetrics(
			[FromServices] FunctionsInfoRepository metricsStorageService)
		{
			var res = metricsStorageService.Get("echo-00001-deployment-5f657c6b6b-dkmhk");
			if(res != null)
			{
				return Ok(res);
			}
			else
			{
				return BadRequest();
			}


		}

	}
}

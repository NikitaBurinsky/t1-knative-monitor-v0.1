using Microsoft.AspNetCore.Mvc;
using T1_KNative_Administrator_v03.Infrastructure.MetricsStorageService;
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
			[FromServices] MetricsStorageService metricsStorageService)
		{
			bool res = metricsStorageService.FunctionsMetrics.TryGetValue("echo-00001-deployment-5f657c6b6b-dkmhk", out var value);
			if(res == true)
			{
				return Ok(value);
			}
			else
			{
				return BadRequest();
			}


		}

	}
}

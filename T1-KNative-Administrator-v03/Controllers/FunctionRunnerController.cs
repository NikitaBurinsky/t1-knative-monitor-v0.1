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
			var res = metricsStorageService.Get(GetDefaultConfigurationContainerName());
			if (res != null)
			{
				return Ok(res);
			}
			else
			{
				return BadRequest("Info was not found");
			}


		}
		IConfiguration _configuration;
		private string DefaultContainerName { get; set; }
		public FunctionRunnerController(IConfiguration configuration)
		{
			_configuration = configuration;
		}
		private string GetDefaultConfigurationContainerName()
		{
			if(DefaultContainerName != null)
				return DefaultContainerName;
			string servingName = _configuration["Seeding:FunctionsInfo:ServingName"];
			string revisionName = _configuration["Seeding:FunctionsInfo:RevisionName"];
			string podName = _configuration["Seeding:FunctionsInfo:PODName"];
			DefaultContainerName = servingName + "-" + revisionName + "-"+ podName;
			return DefaultContainerName;
		}

	}
}

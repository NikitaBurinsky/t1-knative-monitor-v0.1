using Microsoft.AspNetCore.Mvc;
using T1_KNative_Administrator_v03.Core.OpResult;
using T1_KNative_Administrator_v03.Infrastructure.Repositories.FunctionsInfoRepository;
using T1_KNative_Administrator_v03.Infrastructure.Services.FunctionRunnerService;
using T1_KNative_Administrator_v03.Infrastructure.Services.FunctionsManagerService;
using static T1_KNative_Monitor_v01.Collerctors.Prometheus.PrometheusCollectorBase.PrometheusCollectorBase;

namespace T1_KNative_Administrator_v03.Controllers
{
    [ApiController]
    [Route("runner")]
    public class FunctionRunnerController : ControllerBase
    {
        [HttpPost("echo")]
        public async Task<IActionResult> RunEcho(
			[FromServices] FunctionsRunnerService functionsRunnerService,
			[FromQuery] int runTimes = 1,
            [FromQuery] int runDelay = 0)
        {
			if(runDelay <= 0) runDelay = 0;
			if(runTimes <= 0) runTimes = 0;
			OpResult opRes = await functionsRunnerService.RunFunctions("http://127.0.0.1:8081", runTimes, runDelay);
            return opRes.Succeeded ?
				Ok(opRes) :
				BadRequest(opRes);
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

        public FunctionRunnerController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        private string DefaultContainerName { get; set; }
        private string GetDefaultConfigurationContainerName()
        {
            if (DefaultContainerName != null)
                return DefaultContainerName;
            string servingName = _configuration["Seeding:FunctionsInfo:ServingName"];
            string revisionName = _configuration["Seeding:FunctionsInfo:RevisionName"];
            string podName = _configuration["Seeding:FunctionsInfo:PODName"];
            DefaultContainerName = servingName + "-" + revisionName + "-" + podName;
            return DefaultContainerName;
        }

    }
}

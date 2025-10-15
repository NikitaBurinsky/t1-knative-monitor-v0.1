using Microsoft.AspNetCore.Mvc;
using T1_KNative_Administrator_v03.Infrastructure.Services.BillingService;

namespace T1_KNative_Administrator_v03.Controllers
{

	[ApiController]
	[Route("billings")]
	public class BillingsContoller : ControllerBase
	{
		[HttpPost("start-period")]
		public async Task<IActionResult> StartPeriod(
			[FromServices] BillingService billingService)
		{
			var res = billingService.StartPeriod(
				_configuration["Seeding:FunctionsInfo:ServingName"],
				_configuration["Seeding:FunctionsInfo:RevisionName"],
				_configuration["Seeding:FunctionsInfo:PODName"]);
			return res.Succeeded ?
				Ok(res) : BadRequest(res);
		}

		[HttpPost("end-period")]
		public async Task<IActionResult> EndPeriod(
			[FromServices] BillingService billingService)
		{
			var res = billingService.EndPeriod(GetDefaultConfigurationContainerName());
			return res.Succeeded ?
				Ok(res.Returns) : BadRequest(res);
		}

		IConfiguration _configuration;
		private string DefaultContainerName { get; set; }

		public BillingsContoller(IConfiguration configuration)
		{
			_configuration = configuration;
		}

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

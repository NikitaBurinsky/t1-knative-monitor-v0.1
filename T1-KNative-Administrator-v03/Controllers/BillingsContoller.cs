using Microsoft.AspNetCore.Mvc;

namespace T1_KNative_Administrator_v03.Controllers
{

	[ApiController]
	[Route("billings")]
	public class BillingsContoller : ControllerBase
	{
		[HttpPost("start-period")]
		public async Task<IActionResult> StartPeriod()
		{

		}


		public async Task<IActionResult> EndPeriod()
		{

		}

		public async Task<IActionResult> GetLastPeriodBill()
		{

		}





	}
}

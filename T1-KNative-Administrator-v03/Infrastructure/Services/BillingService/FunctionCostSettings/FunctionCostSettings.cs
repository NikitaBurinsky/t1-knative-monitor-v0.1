namespace T1_KNative_Admin_v02.Core.Function
{
	public class FunctionCostSettings
	{
		public decimal BaseCostPerDay { get; set; } 
		public decimal CostPerRequest { get; set; } 
		public decimal CostPerMsCpu { get; set; } 
		public decimal CostPerMsRuntime { get; set; } 
		public decimal CostPerMbRam { get; set; } 
		public decimal CostPerMbRamHour { get; set; }
		public string Currency { get; set; }
	}
}
using Microsoft.Extensions.Options;
using System;
using System.Linq;

namespace T1_KNative_Admin_v02.Core.Function
{
	public class FunctionCostCalculator
	{
		private readonly FunctionCostSettings _settings;

		public FunctionCostCalculator(IOptions<FunctionCostSettings> settings)
		{
			_settings = settings.Value;
		}

		public decimal CalculateCost(FunctionEntity function, DateOnly? startDate = null, DateOnly? endDate = null)
		{
			if (function == null) return 0;

			decimal totalCost = 0;

			// 1. Базовая стоимость содержания функции
			totalCost += CalculateBaseCost(function, startDate, endDate);

			// 2. Стоимость запросов
			totalCost += CalculateRequestsCost(function, startDate, endDate);

			// 3. Стоимость CPU времени
			totalCost += CalculateCpuCost(function);

			// 4. Стоимость времени выполнения
			totalCost += CalculateRuntimeCost(function);

			// 5. Стоимость памяти
			totalCost += CalculateMemoryCost(function);

			return Math.Round(totalCost, 4);
		}

		private decimal CalculateBaseCost(FunctionEntity function, DateOnly? startDate, DateOnly? endDate)
		{
			if (function.requestsCounterStats.RequestsCountByDay == null ||
				!function.requestsCounterStats.RequestsCountByDay.Any())
				return _settings.BaseCostPerDay;

			Dictionary<DateOnly, ulong>.KeyCollection activeDays = function.requestsCounterStats.RequestsCountByDay.Keys;
			IEnumerable<DateOnly> resActiveDays = new List<DateOnly>();
			if (startDate.HasValue)
				resActiveDays = activeDays.Where(d => d >= startDate.Value);
			if (endDate.HasValue)
				resActiveDays = activeDays.Where(d => d <= endDate.Value);

			int daysCount = resActiveDays.Count();
			return _settings.BaseCostPerDay * daysCount;
		}

		private decimal CalculateRequestsCost(FunctionEntity function, DateOnly? startDate, DateOnly? endDate)
		{
			if (function.requestsCounterStats?.RequestsCountByDay == null)
				return 0;

			ulong totalRequests = 0;

			foreach (var day in function.requestsCounterStats.RequestsCountByDay)
			{
				if (startDate.HasValue && day.Key < startDate.Value) continue;
				if (endDate.HasValue && day.Key > endDate.Value) continue;

				totalRequests += day.Value;
			}

			return _settings.CostPerRequest * totalRequests;
		}

		private decimal CalculateCpuCost(FunctionEntity function)
		{
			if (function.vCpuStats?.AvgRunningTimeMS == null || function.vCpuStats.MetricsCounts == 0)
				return 0;

			decimal totalCpuTimeMs = (decimal)function.vCpuStats.AvgRunningTimeMS.Value * function.vCpuStats.MetricsCounts;
			return _settings.CostPerMsCpu * totalCpuTimeMs;
		}

		private decimal CalculateRuntimeCost(FunctionEntity function)
		{
			if (function.RunTimeStats?.AvgRunningTimeMS == null || function.RunTimeStats.MetricsCounts == 0)
				return 0;

			decimal totalRuntimeMs = (decimal)function.RunTimeStats.AvgRunningTimeMS.Value * function.RunTimeStats.MetricsCounts;
			return _settings.CostPerMsRuntime * totalRuntimeMs;
		}

		private decimal CalculateMemoryCost(FunctionEntity function)
		{
			if (function.RamStats?.AvgRAMStatsBytes == null || function.RamStats.MetricsCounts == 0)
				return 0;

			// Переводим байты в мегабайты
			decimal avgMemoryMB = (decimal)function.RamStats.AvgRAMStatsBytes.Value / (1024 * 1024);

			if (function.RunTimeStats?.AvgRunningTimeMS != null && function.RunTimeStats.MetricsCounts > 0)
			{
				decimal totalMemoryHours = ((decimal)function.RunTimeStats.AvgRunningTimeMS.Value *
										   function.RunTimeStats.MetricsCounts) / (1000 * 60 * 60);

				return _settings.CostPerMbRamHour * avgMemoryMB * totalMemoryHours;
			}

			decimal totalMemorySeconds = ((decimal)function.RunTimeStats.AvgRunningTimeMS.Value *
										 function.RunTimeStats.MetricsCounts) / 1000;

			return _settings.CostPerMbRam * avgMemoryMB * totalMemorySeconds;
		}

		public CostBreakdown GetCostBreakdown(FunctionEntity function, DateOnly? startDate = null, DateOnly? endDate = null)
		{
			return new CostBreakdown
			{
				BaseCost = CalculateBaseCost(function, startDate, endDate),
				RequestsCost = CalculateRequestsCost(function, startDate, endDate),
				CpuCost = CalculateCpuCost(function),
				RuntimeCost = CalculateRuntimeCost(function),
				MemoryCost = CalculateMemoryCost(function),
				TotalCost = CalculateCost(function, startDate, endDate),
				Currency = _settings.Currency
			};
		}
	}

	public class CostBreakdown
	{
		public decimal BaseCost { get; set; }
		public decimal RequestsCost { get; set; }
		public decimal CpuCost { get; set; }
		public decimal RuntimeCost { get; set; }
		public decimal MemoryCost { get; set; }
		public decimal TotalCost { get; set; }
		public string Currency { get; set; }

		public void PrintBreakdown()
		{
			Console.WriteLine("=== ДЕТАЛИЗАЦИЯ СТОИМОСТИ ФУНКЦИИ ===");
			Console.WriteLine($"Базовая стоимость: {BaseCost:F4} {Currency}");
			Console.WriteLine($"Стоимость запросов: {RequestsCost:F4} {Currency}");
			Console.WriteLine($"Стоимость CPU: {CpuCost:F4} {Currency}");
			Console.WriteLine($"Стоимость времени выполнения: {RuntimeCost:F4} {Currency}");
			Console.WriteLine($"Стоимость памяти: {MemoryCost:F4} {Currency}");
			Console.WriteLine($"ОБЩАЯ СТОИМОСТЬ: {TotalCost:F4} {Currency}");
		}
	}
}
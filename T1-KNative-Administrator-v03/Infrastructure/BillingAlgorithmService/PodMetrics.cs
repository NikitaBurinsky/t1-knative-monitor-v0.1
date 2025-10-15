using System;
using System.Collections.Generic;

namespace T1_KNative_Administrator_v03.Infrastructure.BillingAlgorithmService
{
    public class PodMetrics
    {
        // Pricing constants (example values, adjust to your cluster economics)
        private const double PricePerRequest = 0.0000002; // $0.20 per million requests
        private const double PricePerGBSecond = 0.00001667; // $0.00001667 per GB-second (Lambda baseline)
        private const double PricePerVCpuSecond = 0.000011244; // $0.0405 per vCPU-hour (Fargate baseline)
        private const double PricePerGBMemorySecond = 0.000001231; // $0.004445 per GB-hour (Fargate baseline)

        /// <summary>
        /// Calculate price for a given service based on metrics.
        /// </summary>
        /// <param name="requestCount">Total number of requests handled.</param>
        /// <param name="avgDurationMs">Average request duration in milliseconds.</param>
        /// <param name="memoryMB">Memory allocated per pod (MB).</param>
        /// <param name="vCpu">vCPU allocated per pod.</param>
        /// <param name="podSeconds">Total pod runtime in seconds.</param>
        /// <returns>Total cost in USD.</returns>
        public double CalculatePrice(long requestCount, double avgDurationMs, int memoryMB, double vCpu, double podSeconds)
        {
            // Lambda-style: requests + GB-seconds
            double requestCost = requestCount * PricePerRequest;

            double durationSeconds = avgDurationMs / 1000.0 * requestCount;
            double memoryGB = memoryMB / 1024.0;
            double gbSeconds = durationSeconds * memoryGB;
            double lambdaCost = gbSeconds * PricePerGBSecond;

            // Fargate-style: vCPU-seconds + memory-seconds
            double vCpuCost = vCpu * podSeconds * PricePerVCpuSecond;
            double memoryCost = memoryGB * podSeconds * PricePerGBMemorySecond;

            // Combine models (you can weight or choose one)
            double totalCost = requestCost + lambdaCost + vCpuCost + memoryCost;

            return Math.Round(totalCost, 6);
        }
    }
}

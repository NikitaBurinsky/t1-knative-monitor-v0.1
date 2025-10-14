using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prometheus;

namespace T1_KNative_Monitor_v01
{
    public static class MetricsRegistry
    {
        public static readonly Counter CustomRequests = Metrics
            .CreateCounter("custom_requests_total", "Number of custom requests processed");

        public static readonly Gauge ActiveJobs = Metrics
            .CreateGauge("active_jobs", "Number of active background jobs");
    }
}

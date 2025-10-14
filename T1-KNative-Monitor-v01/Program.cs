using k8s;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;
using T1_KNative_Monitor_v01;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

// Load kubeconfig (works inside cluster or locally)
var config = KubernetesClientConfiguration.IsInCluster()
    ? KubernetesClientConfiguration.InClusterConfig()
    : KubernetesClientConfiguration.BuildConfigFromConfigFile();

var k8sClient = new Kubernetes(config);
var collector = new KubernetesMetricsCollector(k8sClient);

// Background task to periodically collect metrics
var app = builder.Build();

app.Lifetime.ApplicationStarted.Register(() =>
{
    Task.Run(async () =>
    {
        while (true)
        {
            try
            {
                await collector.CollectAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error collecting metrics] {ex.Message}");
            }

            await Task.Delay(TimeSpan.FromSeconds(30));
        }
    });
});

app.UseRouting();
app.UseHttpMetrics();
app.MapControllers();
app.MapMetrics();

app.MapGet("/", () => "Hello from Knative .NET service with Prometheus + Kubernetes metrics!");

app.Run();

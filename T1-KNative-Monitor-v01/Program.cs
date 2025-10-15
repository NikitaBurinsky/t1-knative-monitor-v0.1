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
var k8sCollector = new KubernetesMetricsCollector(k8sClient);
var knativeStatusCollector = new KnativeStatusCollector(k8sClient);
var knativeMetricsCollector = new KnativeMetricsCollector(new HttpClient());
var knativeControlCollector = new KnativeControlMetricsCollector(new HttpClient());

// Background task to periodically collect metrics
var app = builder.Build();

app.Lifetime.ApplicationStarted.Register(() =>
{
    _ = Task.Run(async () =>
    {
        while (true)
        {
            await k8sCollector.CollectAsync();
            await knativeStatusCollector.CollectAsync();
            await knativeMetricsCollector.CollectAsync();
            await knativeControlCollector.CollectAsync();

            await Task.Delay(TimeSpan.FromSeconds(30));
        }
    });
});

app.UseRouting();
app.UseHttpMetrics();
app.MapControllers();
app.MapMetrics();

app.MapGet("/work", () =>
{
    MetricsRegistry.CustomRequests.Inc();
    MetricsRegistry.ActiveJobs.Inc();

    Thread.Sleep(500); // simulate work

    MetricsRegistry.ActiveJobs.Dec();
    return "Work done!";
});

app.MapGet("/do-work", () =>
{
    AppMetrics.BusinessOps.Inc();
    Console.WriteLine("[App] Business operation executed");
    return "done";
});

app.Run();

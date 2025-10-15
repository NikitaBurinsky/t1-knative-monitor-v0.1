
using k8s;
using T1_KNative_Administrator_v03.Infrastructure.MetricsStorageService;

namespace T1_KNative_Administrator_v03
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);
			builder.Services.AddControllers();
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen();
			//Load kubeconfig (works inside cluster or locally)
			var config = KubernetesClientConfiguration.IsInCluster()
			? KubernetesClientConfiguration.InClusterConfig()
			: KubernetesClientConfiguration.BuildConfigFromConfigFile();

			//var k8sClient = new Kubernetes(config);
			builder.Services.AddSingleton<MetricsStorageService>();

			var app = builder.Build();
			app.UseSwagger();

			app.UseSwaggerUI();
			var metrixStorage = app.Services.GetRequiredService<MetricsStorageService>();
			var knativeControlCollector = new KnativeControlMetricsCollector(new HttpClient(), metrixStorage);

			app.Lifetime.ApplicationStarted.Register(() =>
			{
				_ = Task.Run(async () =>
				{
						while (true)
						{
							await knativeControlCollector.CollectAsync();
							await Task.Delay(TimeSpan.FromSeconds(30));
						}
				
				});
			});

			app.UseRouting();
			app.UseAuthorization();
			app.MapControllers();
			app.Run();
		}
	}
}

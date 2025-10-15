
using k8s;
using Microsoft.EntityFrameworkCore;
using T1_KNative_Admin_v02.Core.Function;
using T1_KNative_Administrator_v03.Infrastructure.Repositories;
using T1_KNative_Administrator_v03.Infrastructure.Repositories.FunctionsInfoRepository;
using T1_KNative_Administrator_v03.Infrastructure.Services.BillingService;
using T1_KNative_Administrator_v03.Infrastructure.Services.FunctionsManagerService;
using static T1_KNative_Monitor_v01.Collerctors.Prometheus.PrometheusCollectorBase.PrometheusCollectorBase;

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
			builder.Services.Configure<FunctionCostSettings>(
					builder.Configuration.GetSection("FunctionCostSettings"));
			builder.Services.AddScoped<FunctionCostCalculator>();
			builder.Services.AddScoped<BillingService>();
			builder.Services.AddScoped<FunctionsInfoRepository>();
			builder.Services.AddSingleton<FunctionsStatsManagerService>();
			builder.Services.AddDbContext<ApplicationDbContext>(o =>
			{
				o.UseInMemoryDatabase(databaseName: "ApplicationDb");
			});

			var app = builder.Build();
			app.UseSwagger();

			app.UseSwaggerUI();


			var knativeControlCollector = new KnativeControlMetricsCollector(new HttpClient(),
				app.Services.GetRequiredService<FunctionsStatsManagerService>(), app.Configuration);
			SeedTestFunctionEntity(app);
			app.Lifetime.ApplicationStarted.Register(() =>
			{
				_ = Task.Run(async () =>
				{
					while (true)
					{
						await knativeControlCollector.CollectAsync();
						await Task.Delay(TimeSpan.FromSeconds(app.Configuration.GetValue<int>("Collectors:Prometheus:CollectingDelaysSeconds")));
					}
				});
			});

			app.UseRouting();
			app.UseAuthorization();
			app.MapControllers();
			app.Run();

		}


		public static void SeedTestFunctionEntity(WebApplication app)
		{
			using (var scope = app.Services.CreateScope())
			{
				var _configuration = app.Configuration;
				string servingName = _configuration["Seeding:FunctionsInfo:ServingName"];
				string revisionName = _configuration["Seeding:FunctionsInfo:RevisionName"];
				string podName = _configuration["Seeding:FunctionsInfo:PODName"];

				var repos = scope.ServiceProvider.GetRequiredService<FunctionsInfoRepository>();
				repos.CreateFunctionInfo(servingName, revisionName, podName);
			}

		}
	}
}

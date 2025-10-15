using System.Net;
using T1_KNative_Admin_v02.Core.Function;
using T1_KNative_Administrator_v03.Core.OpResult;

namespace T1_KNative_Administrator_v03.Infrastructure.Repositories.FunctionsInfoRepository
{
	public class FunctionsInfoRepository
	{

		private ApplicationDbContext dbContext;
		public IQueryable<FunctionEntity> FunctionEntities { get; set; }
		public FunctionsInfoRepository(ApplicationDbContext dbContext)
		{
			this.dbContext = dbContext;
			FunctionEntities = dbContext.Functions;
		}

		public FunctionEntity Get(string fullName) => FunctionEntities.FirstOrDefault(x => x.FullName == fullName);
		public FunctionEntity Get(int Id) => FunctionEntities.FirstOrDefault(x => x.Id == Id);

		public async Task<OpResult<int>> CreateFunctionInfo(string servingName, string revisionName, string podName)
		{
			if (dbContext.Functions.Any(x => x.RevisionName == revisionName
			&& x.ServingName == servingName
			&& x.PODName == podName))
				return OpResult<int>.Error("Similar Name Exists", HttpStatusCode.BadRequest);
			var entry = await dbContext.Functions.AddAsync(new FunctionEntity()
			{
				ServingName = servingName,
				PODName = podName,
				RevisionName = revisionName,
				FullName = servingName + "-" + revisionName + "-" + podName,
			});
			await dbContext.SaveChangesAsync();
			return OpResult<int>.Success(entry.Entity.Id);
		}
		public async Task<OpResult<int>> UpdateFunctionInfo(string fullName, Action<FunctionEntity> action)
		{
			var entity = dbContext.Functions.FirstOrDefault(x => x.FullName == fullName);
			if (entity == null)
				return OpResult<int>.Error("Function Not Found", HttpStatusCode.NotFound);
			var entry = dbContext.Functions.Update(entity);
			action(entity);
			await dbContext.SaveChangesAsync();
			return OpResult<int>.Success(entry.Entity.Id);
		}
	}
}

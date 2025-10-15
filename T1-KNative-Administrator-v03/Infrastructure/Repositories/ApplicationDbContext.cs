using Microsoft.EntityFrameworkCore;
using T1_KNative_Admin_v02.Core.Function;

namespace T1_KNative_Administrator_v03.Infrastructure.Repositories
{
	public class ApplicationDbContext : DbContext
	{
		public DbSet<FunctionEntity> Functions { get; set; }
	}
}

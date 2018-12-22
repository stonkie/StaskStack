using System.Data.Common;
using System.Data.Entity;

namespace ByteAuthor.StaskStack
{
	public class CacheContext : DbContext
	{
		public CacheContext(DbConnection existingConnection, IDatabaseInitializer<CacheContext> initializationStrategy = null) : base(existingConnection, true)
		{
			Database.SetInitializer(initializationStrategy);
		}

		public DbSet<Task> Tasks { get; set; }
		public DbSet<Step> Steps { get; set; }
	}
}
using System.Data.Common;
using System.Data.Entity;
using System.Data.SQLite;
using ByteAuthor.StaskStack.Properties;
using Ninject.Modules;

namespace ByteAuthor.StaskStack
{
	public class CacheContextModule : NinjectModule
	{
		public override void Load()
		{
			Bind<DbConnection>().To<SQLiteConnection>()
				.WithConstructorArgument(Settings.Default.CacheConnectionString);
			Bind<IDatabaseInitializer<CacheContext>>().To<CacheContextInitializer>();
			Bind<IViewModelMapper>().To<CacheViewModelMapper>();
			Bind<CacheContext>().ToSelf();
		}
	}
}
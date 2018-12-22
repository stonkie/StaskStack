using System.Data.Entity.Infrastructure;
using Ninject;

namespace ByteAuthor.StaskStack
{
	public class CacheContextDbFactory : IDbContextFactory<CacheContext>
	{
		public CacheContext Create()
		{
			IKernel kernel = new StandardKernel(new CacheContextModule());
			return kernel.Get<CacheContext>();
		}
	}
}
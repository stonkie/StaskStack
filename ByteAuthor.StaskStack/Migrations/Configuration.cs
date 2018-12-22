using System.Data.SQLite.EF6.Migrations;

namespace ByteAuthor.StaskStack.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<ByteAuthor.StaskStack.CacheContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
	        SetSqlGenerator("System.Data.SQLite", new SQLiteMigrationSqlGenerator());
        }

        protected override void Seed(ByteAuthor.StaskStack.CacheContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data.
        }
    }
}

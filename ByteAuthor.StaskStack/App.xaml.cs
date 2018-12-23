using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ByteAuthor.StaskStack.Migrations;
using ByteAuthor.StaskStack.Properties;
using Ninject;

namespace ByteAuthor.StaskStack
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private readonly IKernel _container;

		public App()
		{
			_container = new StandardKernel(new CacheContextModule());
			
			MainWindow = _container.Get<MainWindow>();
		}

		public CacheContext Create()
		{
			return _container.Get<CacheContext>();
		}
		
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			Debug.Assert(MainWindow != null, nameof(MainWindow) + " != null");
			MainWindow.Show();
		}
	}

	internal class CacheContextInitializer : MigrateDatabaseToLatestVersion<CacheContext,Configuration>
	{
		public override void InitializeDatabase(CacheContext context)
		{
			//SQLiteConnectionStringBuilder connectionStringBuilder = new SQLiteConnectionStringBuilder(context.Database.Connection.ConnectionString);
			//string filename = connectionStringBuilder.DataSource;
			//File.Delete(filename);
			
			base.InitializeDatabase(context); 
		}
	}
}

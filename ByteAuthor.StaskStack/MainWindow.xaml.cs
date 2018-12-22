using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace ByteAuthor.StaskStack
{
	/// <summary>
	///   Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private readonly IViewModelMapper _viewModelMapper;

		public MainWindow(IViewModelMapper viewModelMapper)
		{
			_viewModelMapper = viewModelMapper;
			
			InitializeComponent();
		}

		private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
		{
			try
			{
				await _viewModelMapper.InitializeAsync();

				ControlBacklog.ViewModelMapper = _viewModelMapper;
				ControlBacklog.TasksSource = _viewModelMapper.BacklogTasks;

				ICollectionView backlogTasksView = CollectionViewSource.GetDefaultView(ControlBacklog.TasksSource);
				backlogTasksView.SortDescriptions.Add(new SortDescription(nameof(Task.Priority), ListSortDirection.Ascending));
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		private async void MenuNewTask_OnClick(object sender, RoutedEventArgs e)
		{
			try
			{
				TaskViewModel task = await _viewModelMapper.CreateTaskAsync();

				ControlBacklog.SelectTask(task);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

	}
}
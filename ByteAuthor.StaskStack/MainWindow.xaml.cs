using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace ByteAuthor.StaskStack
{
	/// <summary>
	///   Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public static readonly RoutedCommand CreateTaskCommand = new RoutedCommand("Create Task", typeof(MainWindow), new InputGestureCollection()
		{
			new KeyGesture(Key.N, ModifierKeys.Control),
		});

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
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}
		
		private async void CreateTaskCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			try
			{
				TaskViewModel task = await _viewModelMapper.CreateTaskAsync(ControlBacklog.SelectedTask);

				ControlBacklog.SelectedTask = task;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}
	}
}
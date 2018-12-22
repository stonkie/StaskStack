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

				ListBacklogTasks.ItemsSource = _viewModelMapper.BacklogTasks;

				ICollectionView backlogTasksView = CollectionViewSource.GetDefaultView(ListBacklogTasks.ItemsSource);
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

				if (ListBacklogTasks.ItemContainerGenerator.ContainerFromItem(task) is TreeViewItem taskItem)
				{
					ICollectionView taskStepsView = CollectionViewSource.GetDefaultView(taskItem.ItemsSource);
					taskStepsView.SortDescriptions.Add(new SortDescription(nameof(Step.Order), ListSortDirection.Ascending));
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		private async void ListBacklogTasks_OnKeyDown(object sender, KeyEventArgs e)
		{
			try
			{
				if (e.Key == Key.Enter)
				{
					(TaskViewModel parentTask, StepViewModel createdStep) = await CreateStepInSelectedBacklogTaskAsync();

					if (parentTask != null && ListBacklogTasks.ItemContainerGenerator.ContainerFromItem(parentTask) is TreeViewItem parentTaskItem)
					{
						if (createdStep != null &&
						    parentTaskItem.ItemContainerGenerator.ContainerFromItem(createdStep) is TreeViewItem immediateStepItem)
						{
							immediateStepItem.IsSelected = true;
						}
						else
						{
							void OnItemContainerGeneratorOnStatusChanged(object o, EventArgs args)
							{
								if (createdStep != null &&
								    parentTaskItem.ItemContainerGenerator.ContainerFromItem(createdStep) is TreeViewItem stepItem)
								{
									stepItem.IsSelected = true;
									parentTaskItem.ItemContainerGenerator.StatusChanged -= OnItemContainerGeneratorOnStatusChanged;
								}
							}

							parentTaskItem.ItemContainerGenerator.StatusChanged += OnItemContainerGeneratorOnStatusChanged;
						}

						parentTaskItem.IsExpanded = true;
					}
				}
				else if (e.Key == Key.F2)
				{
					IsInEditMode = true;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		private async Task<(TaskViewModel, StepViewModel)> CreateStepInSelectedBacklogTaskAsync()
		{
			if (ListBacklogTasks.SelectedItem is TaskViewModel taskViewModel)
			{
				return (taskViewModel, await _viewModelMapper.CreateStepAsync(taskViewModel));
			}
			else if (ListBacklogTasks.SelectedItem is StepViewModel stepViewModel)
			{
				TaskViewModel parentTaskViewModel = _viewModelMapper.BacklogTasks.Single(t => t.Steps.Contains(stepViewModel));
				return (parentTaskViewModel, await _viewModelMapper.CreateStepAsync(parentTaskViewModel, stepViewModel));
			}
			else
			{
				return (null, null);
			}
		}

		public static readonly DependencyProperty IsInEditModeProperty = DependencyProperty.Register(
			"IsInEditMode", typeof(bool), typeof(MainWindow), new PropertyMetadata(default(bool)));
		
		private string _previousText;

		public bool IsInEditMode
		{
			get => (bool) GetValue(IsInEditModeProperty);
			set => SetValue(IsInEditModeProperty, value);
		}
		
		private void TextBlockTaskHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (FindTreeItem(sender as DependencyObject)?.IsSelected ?? false)
			{
				IsInEditMode = true;
				e.Handled = true; // otherwise the newly activated control will immediately loose focus
			}
		}

		private void TextBoxTaskEditableHeader_LostFocus(object sender, RoutedEventArgs e)
		{
			IsInEditMode = false;
		}

		private void TextBoxTaskEditableHeader_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (sender is TextBox textBox && textBox.IsVisible)
			{
				textBox.Focus();
				textBox.SelectAll();
				_previousText = textBox.Text; // back up - for possible cancelling
			}
		}

		private void TextBoxTaskEditableHeader_KeyDown(object sender, KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Enter:
					IsInEditMode = false;
					e.Handled = true;
					break;
				case Key.Escape:
					if (sender is TextBox textBox)
					{
						textBox.Text = _previousText;
					}
				
					IsInEditMode = false;
					e.Handled = true;
					break;
			}
		}

		// based on http://stackoverflow.com/questions/592373/select-treeview-node-on-right-click-before-displaying-contextmenu
		private TreeViewItem FindTreeItem(DependencyObject source)
		{
			while (source != null && !(source is TreeViewItem))
			{
				source = VisualTreeHelper.GetParent(source);
			}

			return source as TreeViewItem;
		}

		private void TextBlockStepHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (FindTreeItem(sender as DependencyObject)?.IsSelected ?? false)
			{
				IsInEditMode = true;
				e.Handled = true; // otherwise the newly activated control will immediately loose focus
			}
		}

		private void TextBoxStepEditableHeader_LostFocus(object sender, RoutedEventArgs e)
		{
			IsInEditMode = false;
		}

		private void TextBoxStepEditableHeader_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (sender is TextBox textBox && textBox.IsVisible)
			{
				textBox.Focus();
				textBox.SelectAll();
				_previousText = textBox.Text; // back up - for possible cancelling
			}
		}

		private void TextBoxStepEditableHeader_KeyDown(object sender, KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Enter:
					IsInEditMode = false;
					e.Handled = true;
					break;
				case Key.Escape:
					if (sender is TextBox textBox)
					{
						textBox.Text = _previousText;
					}
				
					IsInEditMode = false;
					e.Handled = true;
					break;
			}
		}
	}
}
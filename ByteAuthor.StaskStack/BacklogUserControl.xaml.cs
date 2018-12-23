using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace ByteAuthor.StaskStack
{
	/// <summary>
	///   Interaction logic for BacklogUserControl.xaml
	/// </summary>
	public partial class BacklogUserControl : UserControl
	{
		public static readonly DependencyProperty TasksSourceProperty = DependencyProperty.Register(
			"TasksSource", typeof(IEnumerable<TaskViewModel>), typeof(BacklogUserControl), new PropertyMetadata(default(IEnumerable<TaskViewModel>), (sender, args) =>
			{
				BacklogUserControl control = sender as BacklogUserControl;
				if (control != null)
				{
					control.TasksSourceChanged?.Invoke(control, EventArgs.Empty);
				}
			}));

		public static readonly DependencyProperty IsInEditModeProperty = DependencyProperty.Register(
			"IsInEditMode", typeof(bool), typeof(BacklogUserControl), new PropertyMetadata(default(bool), (sender, args) =>
			{
				BacklogUserControl control = sender as BacklogUserControl;
				if (control != null)
				{
					control.IsInEditModeChanged?.Invoke(control, EventArgs.Empty);
				}
			}));

		public static readonly DependencyProperty ViewModelMapperProperty = DependencyProperty.Register(
			"ViewModelMapper", typeof(IViewModelMapper), typeof(BacklogUserControl),
			new PropertyMetadata(default(IViewModelMapper)));

		public event EventHandler<EventArgs> TasksSourceChanged;
		
		public event EventHandler<EventArgs> IsInEditModeChanged;
		
		private string _previousText;

		public BacklogUserControl()
		{
			TasksSourceChanged += (sender, args) =>
			{
				ListBacklogTasks.ItemsSource = this.TasksSource;
				ICollectionView backlogTasksView = CollectionViewSource.GetDefaultView(ListBacklogTasks.ItemsSource);
				backlogTasksView.SortDescriptions.Add(new SortDescription(nameof(Task.Priority), ListSortDirection.Ascending));
			};

			IsInEditModeChanged += async (sender, args) =>
			{
				if (IsInEditMode)
				{
					return;
				}

				try
				{
					if (ListBacklogTasks.SelectedItem is TaskViewModel task)
					{
						await ViewModelMapper.SaveTaskNameAsync(task);
					}
					else if (ListBacklogTasks.SelectedItem is StepViewModel step)
					{
						await ViewModelMapper.SaveStepDescriptionAsync(step);
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.ToString());
				}
			};

			InitializeComponent();
		}
		
		public bool IsInEditMode
		{
			get => (bool) GetValue(IsInEditModeProperty);
			set => SetValue(IsInEditModeProperty, value);
		}

		public IViewModelMapper ViewModelMapper
		{
			get => (IViewModelMapper) GetValue(ViewModelMapperProperty);
			set => SetValue(ViewModelMapperProperty, value);
		}

		public IEnumerable<TaskViewModel> TasksSource
		{
			get => GetValue(TasksSourceProperty) as IEnumerable<TaskViewModel>;
			set => SetValue(TasksSourceProperty, value);
		}

		public TaskViewModel SelectedTask
		{
			get
			{
				if (ListBacklogTasks.SelectedItem is TaskViewModel task)
				{
					return task;
				}
				else if (ListBacklogTasks.SelectedItem is StepViewModel step)
				{
					return ViewModelMapper.BacklogTasks.Single(t => t.Steps.Contains(step));
				}
				else
				{
					return null;
				}
			}
			set
			{
				if (ListBacklogTasks.ItemContainerGenerator.ContainerFromItem(value) is TreeViewItem taskItem)
				{
					ICollectionView taskStepsView = CollectionViewSource.GetDefaultView(taskItem.ItemsSource);
					taskStepsView.SortDescriptions.Add(new SortDescription(nameof(Step.Order), ListSortDirection.Ascending));

					taskItem.IsSelected = true;
				}
			}
		}

		private async Task<(TaskViewModel, StepViewModel)> CreateStepInSelectedBacklogTaskAsync()
		{
			if (ListBacklogTasks.SelectedItem is TaskViewModel taskViewModel)
			{
				return (taskViewModel, await ViewModelMapper.CreateStepAsync(taskViewModel));
			}

			if (ListBacklogTasks.SelectedItem is StepViewModel stepViewModel)
			{
				TaskViewModel parentTaskViewModel = ViewModelMapper.BacklogTasks.Single(t => t.Steps.Contains(stepViewModel));
				return (parentTaskViewModel, await ViewModelMapper.CreateStepAsync(parentTaskViewModel, stepViewModel));
			}

			return (null, null);
		}

		private async void ListBacklogTasks_OnKeyDown(object sender, KeyEventArgs e)
		{
			try
			{
				if (e.Key == Key.Enter)
				{
					(TaskViewModel parentTask, StepViewModel createdStep) = await CreateStepInSelectedBacklogTaskAsync();

					if (parentTask != null &&
					    ListBacklogTasks.ItemContainerGenerator.ContainerFromItem(parentTask) is TreeViewItem parentTaskItem)
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
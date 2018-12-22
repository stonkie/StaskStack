using System;
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
			"TasksSource", typeof(object), typeof(BacklogUserControl), new PropertyMetadata(default(object)));

		public static readonly DependencyProperty IsInEditModeProperty = DependencyProperty.Register(
			"IsInEditMode", typeof(bool), typeof(BacklogUserControl), new PropertyMetadata(default(bool)));

		public static readonly DependencyProperty ViewModelMapperProperty = DependencyProperty.Register(
			"ViewModelMapper", typeof(IViewModelMapper), typeof(BacklogUserControl),
			new PropertyMetadata(default(IViewModelMapper)));

		private string _previousText;

		public BacklogUserControl()
		{
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

		public object TasksSource
		{
			get => GetValue(TasksSourceProperty);
			set => SetValue(TasksSourceProperty, value);
		}

		public void SelectTask(TaskViewModel task)
		{
			if (ListBacklogTasks.ItemContainerGenerator.ContainerFromItem(task) is TreeViewItem taskItem)
			{
				ICollectionView taskStepsView = CollectionViewSource.GetDefaultView(taskItem.ItemsSource);
				taskStepsView.SortDescriptions.Add(new SortDescription(nameof(Step.Order), ListSortDirection.Ascending));
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
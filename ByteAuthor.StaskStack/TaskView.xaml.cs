using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ByteAuthor.StaskStack
{
	/// <summary>
	///   Interaction logic for TaskView.xaml
	/// </summary>
	public partial class TaskView : UserControl
	{
		public static readonly DependencyProperty TaskProperty =
			DependencyProperty.Register("Task", typeof(TaskViewModel), typeof(TaskView), new PropertyMetadata(default(TaskViewModel)));
		
		public TaskViewModel Task
		{
			get => (TaskViewModel) GetValue(TaskProperty);
			set => SetValue(TaskProperty, value);
		}
		
		public TaskView()
		{
			InitializeComponent();
		}

		private void TextboxName_OnKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				if (!ListSteps.HasItems)
				{
					Task.Steps.Add(new StepViewModel()
					{
						Description = "First Step",
					});
				}
				
				ListSteps.SelectedIndex = 0;
				System.Threading.Tasks.Task.Run( () =>
				{
					System.Threading.Tasks.Task.Delay(10);
					return Dispatcher.InvokeAsync(() =>
							{
								(ListSteps.ItemContainerGenerator.ContainerFromIndex(0) as UIElement)?.Focus();
							});
				});
				e.Handled = true;
			}
		}

		private void TextboxName_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (TextboxName.IsVisible)
			{
				TextboxName.Focus();
				TextboxName.SelectAll();
			}
		}
	}
}
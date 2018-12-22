using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using ByteAuthor.StaskStack.Annotations;

namespace ByteAuthor.StaskStack
{
	public class TaskViewModel : INotifyPropertyChanged
	{
		private int _id;
		private string _name;

		public int Id
		{
			get => _id;
			set
			{
				if (value == _id) return;
				_id = value;
				OnPropertyChanged(nameof(Id));
			}
		}

		public string Name
		{
			get => _name;
			set
			{
				if (value == _name) return;
				_name = value;
				OnPropertyChanged(nameof(Name));
			}
		}

		public string StepsBlock
		{
			get { return string.Join(Environment.NewLine, Steps.Select(step => step.Description)); }
			set
			{
				var stepDescriptions = value.Split(new string[]
				{
					Environment.NewLine
				}, StringSplitOptions.None);

				Steps.Clear();

				foreach (string description in stepDescriptions)
				{
					Steps.Add(new StepViewModel()
					{
						Description = description,
					});
				}
			}
		}

		public ObservableCollection<StepViewModel> Steps { get; } = new ObservableCollection<StepViewModel>();

		public event PropertyChangedEventHandler PropertyChanged;

		public TaskViewModel()
		{
			Steps.CollectionChanged += (sender, args) => OnPropertyChanged(nameof(Steps));
		}

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
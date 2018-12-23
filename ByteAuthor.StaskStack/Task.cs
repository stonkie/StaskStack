using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ByteAuthor.StaskStack
{
	public class Task
	{
		public int Id { get; set; }

		public string Name { get; set; }

		public double Priority { get; set; }

		public bool IsActive { get; set; }

		public bool IsDone { get; set; }

		public ObservableCollection<Step> Steps { get; set; } = new ObservableCollection<Step>();
	}
}
using System.Collections.Generic;

namespace ByteAuthor.StaskStack
{
	public class Task
	{
		public int Id { get; set; }

		public string Name { get; set; }

		public double Priority { get; set; }

		public bool IsActive { get; set; }

		public bool IsDone { get; set; }

		public ICollection<Step> Steps { get; set; }
	}
}
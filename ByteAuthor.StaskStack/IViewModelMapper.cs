using System.Collections.ObjectModel;

namespace ByteAuthor.StaskStack
{
	public interface IViewModelMapper
	{
		ObservableCollection<TaskViewModel> BacklogTasks { get; }

		System.Threading.Tasks.Task InitializeAsync();
		System.Threading.Tasks.Task<TaskViewModel> CreateTaskAsync();
		System.Threading.Tasks.Task<StepViewModel> CreateStepAsync(TaskViewModel parentTask, StepViewModel previousStep = null);
	}
}
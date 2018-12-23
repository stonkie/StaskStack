using System.Collections.ObjectModel;

namespace ByteAuthor.StaskStack
{
	public interface IViewModelMapper
	{
		ObservableCollection<TaskViewModel> BacklogTasks { get; }

		System.Threading.Tasks.Task InitializeAsync();
		System.Threading.Tasks.Task<TaskViewModel> CreateTaskAsync(TaskViewModel previousTask = null);
		System.Threading.Tasks.Task<StepViewModel> CreateStepAsync(TaskViewModel parentTask, StepViewModel previousStep = null);
		System.Threading.Tasks.Task SaveTaskNameAsync(TaskViewModel updatedTaskViewModel);
		System.Threading.Tasks.Task SaveStepDescriptionAsync(StepViewModel updatedStepViewModel);
	}
}
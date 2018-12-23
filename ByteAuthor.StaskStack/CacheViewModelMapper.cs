using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace ByteAuthor.StaskStack
{
	public class CacheViewModelMapper : IViewModelMapper
	{
		private readonly CacheContext _cacheContext;

		public ObservableCollection<TaskViewModel> BacklogTasks { get; } = new ObservableCollection<TaskViewModel>();

		public CacheViewModelMapper(CacheContext cacheContext)
		{
			_cacheContext = cacheContext;
		}

		public async System.Threading.Tasks.Task InitializeAsync()
		{
			_cacheContext.Database.Initialize(false);
			await ReloadTasksAsync();
		}

		public async Task<TaskViewModel> CreateTaskAsync(TaskViewModel previousTask = null)
		{
			Task newTask = _cacheContext.Tasks.Add(_cacheContext.Tasks.Create());
			newTask.Name = "New Task";
			newTask.Priority = await CalculateTaskPriority(previousTask);
			await _cacheContext.SaveChangesAsync();
			
			if (await IsPriorityNormalizationNeededAsync())
			{
				// Reload all tasks after normalization
				await NormalizePrioritiesAsync();
				await ReloadTasksAsync();
				return BacklogTasks.Single(t => t.Id == newTask.Id);
			}
			else
			{
				// Only add the new task if not normalizing
				TaskViewModel taskViewModel = CreateTaskViewModel(newTask);
				BacklogTasks.Add(taskViewModel);
				return taskViewModel;
			}
		}

		private async System.Threading.Tasks.Task ReloadTasksAsync()
		{
			BacklogTasks.Clear();

			await _cacheContext.Tasks.Include(task => task.Steps).ForEachAsync(task =>
			{
				TaskViewModel newTaskViewModel = CreateTaskViewModel(task);

				BacklogTasks.Add(newTaskViewModel);
			});
		}

		private async Task<double> CalculateTaskPriority(TaskViewModel previousTask)
		{
			double previousPriority;
			double nextPriority;

			if (previousTask == null)
			{
				previousPriority = await _cacheContext.Tasks
					.Where(task => !task.IsActive && !task.IsDone)
					.Select(task => task.Priority)
					.DefaultIfEmpty(0)
					.MaxAsync();

				nextPriority = 1;
			}
			else
			{
				previousPriority = previousTask.Priority;

				nextPriority = await _cacheContext.Tasks
					.Where(task => !task.IsActive && !task.IsDone && task.Priority > previousPriority)
					.Select(task => task.Priority)
					.DefaultIfEmpty(1)
					.MinAsync();
			}

			return (previousPriority + nextPriority) / 2;
		}

		private static TaskViewModel CreateTaskViewModel(Task task)
		{
			TaskViewModel newTaskViewModel = new TaskViewModel()
			{
				Id = task.Id,
				Priority = task.Priority,
				Name = task.Name,
			};

			foreach (Step step in task.Steps)
			{
				newTaskViewModel.Steps.Add(CreateStepViewModel(step));
			}

			return newTaskViewModel;
		}

		private async System.Threading.Tasks.Task<bool> IsPriorityNormalizationNeededAsync()
		{
			return await _cacheContext.Tasks.Join(_cacheContext.Tasks, task => true, task => true,
					(t1, t2) => new {Task1 = t1, Task2 = t2})
				.Where(combination => combination.Task1 != combination.Task2)
				.Where(combination => Math.Abs(combination.Task1.Priority - combination.Task2.Priority) < double.Epsilon * 10)
				.AnyAsync();
		}


		/// <summary>
		/// If any two tasks have very close priorities, we need to normalize to avoid rounding errors from messing up priorities
		/// </summary>
		/// <returns>Whether the tasks list needs to be refreshed or not</returns>
		private async System.Threading.Tasks.Task NormalizePrioritiesAsync()
		{
				List<Task> tasks = await _cacheContext.Tasks.OrderBy(task => task.Priority).ToListAsync();

				for (int index = 0; index < tasks.Count; index++)
				{
					Task task = tasks[index];
					task.Priority = (index + 1) / (double) (tasks.Count + 1);
				}

				await _cacheContext.SaveChangesAsync();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="parentTask"></param>
		/// <param name="previousStep">If null, inserts the step as the first one.</param>
		/// <returns></returns>
		public async System.Threading.Tasks.Task<StepViewModel> CreateStepAsync(TaskViewModel parentTask, StepViewModel previousStep = null)
		{
			Step newStep = _cacheContext.Steps.Add(_cacheContext.Steps.Create());
			
			newStep.Description = "New Step";

			Task persistedTask = _cacheContext.Tasks.Include(t => t.Steps).Single(t => t.Id == parentTask.Id);

			double previousOrder;
			double nextOrder;

			// If null, insert as the first one
			if (previousStep == null)
			{
				previousOrder = 0;
				nextOrder = persistedTask.Steps.FirstOrDefault()?.Order ?? 1;
			}
			else
			{
				Step persistedPreviousStep = persistedTask.Steps.SingleOrDefault(s => s.Id == previousStep.Id);
				
				if (persistedPreviousStep == null)
				{
					throw new ArgumentException("Previous step is not part of the parent task.");
				}

				previousOrder = previousStep.Order;
				nextOrder = persistedTask.Steps.OrderBy(s => s.Order).FirstOrDefault(s => s.Order > previousOrder)?.Order ?? 1;
			}

			newStep.Order = (previousOrder + nextOrder) / 2;
			persistedTask.Steps.Add(newStep);

			await _cacheContext.SaveChangesAsync();

			StepViewModel stepViewModel = CreateStepViewModel(newStep);

			parentTask.Steps.Add(stepViewModel);

			return stepViewModel;
		}

		public async System.Threading.Tasks.Task SaveTaskNameAsync(TaskViewModel updatedTaskViewModel)
		{
			int id = updatedTaskViewModel.Id;
			Task task = await _cacheContext.Tasks.SingleAsync(t => t.Id == id);

			task.Name = updatedTaskViewModel.Name;
			await _cacheContext.SaveChangesAsync();
		}
		
		public async System.Threading.Tasks.Task SaveStepDescriptionAsync(StepViewModel updatedStepViewModel)
		{
			int id = updatedStepViewModel.Id;
			Step step = await _cacheContext.Steps.SingleAsync(s => s.Id == id);

			step.Description = updatedStepViewModel.Description;
			await _cacheContext.SaveChangesAsync();
		}

		private static StepViewModel CreateStepViewModel(Step step)
		{
			StepViewModel stepViewModel = new StepViewModel()
			{
				Id = step.Id,
				Description = step.Description,
				Order = step.Order,
			};
			return stepViewModel;
		}
	}
}
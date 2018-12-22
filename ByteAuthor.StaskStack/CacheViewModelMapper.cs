using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;

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
			await _cacheContext.Tasks.LoadAsync();
		}

		public async System.Threading.Tasks.Task<TaskViewModel> CreateTaskAsync()
		{
			Task newTask = _cacheContext.Tasks.Add(_cacheContext.Tasks.Create());

			newTask.Name = "New Task";
			double largestPriority = await _cacheContext.Tasks
				.Where(task => !task.IsActive && !task.IsDone)
				.Select(task => task.Priority)
				.DefaultIfEmpty(0)
				.MaxAsync();

			newTask.Priority = (1 + largestPriority) / 2;

			await _cacheContext.SaveChangesAsync();

			TaskViewModel taskViewModel = new TaskViewModel()
			{
				Id = newTask.Id,
				Name = newTask.Name
			};

			BacklogTasks.Add(taskViewModel);

			return taskViewModel;
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

			StepViewModel stepViewModel = new StepViewModel()
			{
				Id = newStep.Id,
				Description = newStep.Description,
				Order = newStep.Order,
			};

			parentTask.Steps.Add(stepViewModel);

			return stepViewModel;
		}
	}
}
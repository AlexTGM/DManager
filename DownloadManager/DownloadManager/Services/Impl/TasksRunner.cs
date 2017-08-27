using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DownloadManager.Services.Impl
{
    public class TasksRunner : ITasksRunner
    {
        public IEnumerable<Task<T>> RunTasks<T>(IEnumerable<Func<T>> fucntions)
        {
            return fucntions.Select(Task.Run);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DownloadManager.Services
{
    public interface ITasksRunner
    {
        IEnumerable<Task<T>> RunTasks<T>(IEnumerable<Func<T>> fucntions);
    }
}
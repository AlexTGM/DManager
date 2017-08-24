using System.Collections.Generic;
using System.Threading.Tasks;

namespace DownloadManager.Services
{
    public interface IDownloadManager
    {
        List<Task> Tasks { get; }

        Task DownloadFile(string url, int threads);
    }
}
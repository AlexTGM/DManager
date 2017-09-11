using System.Collections.Generic;
using System.Threading.Tasks;
using DownloadManager.Models;

namespace DownloadManager.Services
{
    public interface IDownloadManager
    {
        List<TaskInformation> Tasks { get; }
        IEnumerable<string> DownloadFile(string url);
    }
}
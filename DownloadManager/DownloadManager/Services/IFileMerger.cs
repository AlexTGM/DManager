using System.Collections.Generic;

namespace DownloadManager.Services
{
    public interface IFileMerger
    {
        void Merge(IEnumerable<string> filesPath, string outputPath);
    }
}
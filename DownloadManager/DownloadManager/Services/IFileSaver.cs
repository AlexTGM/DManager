using SystemInterface.IO;

namespace DownloadManager.Services
{
    public interface IFileSaver
    {
        void SaveFile(IStream responseStream);
    }
}
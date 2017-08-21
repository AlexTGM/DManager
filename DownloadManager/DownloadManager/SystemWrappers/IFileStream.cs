using System;
using System.IO;

namespace DownloadManager.SystemWrappers
{
    public interface IFileStream : IDisposable
    {
        FileStream FileStreamInstance { get; }

        void Write(byte[] buffer, int offset, int count);

        void Initialize(Stream stream);
    }
}
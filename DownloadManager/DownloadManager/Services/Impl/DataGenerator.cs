using System;
using SystemInterface.IO;

namespace DownloadManager.Services.Impl
{
    public class DataGenerator : IDataGenerator
    {
        private const int BlockSize = 8192;
        private const int BlocksPerMb = 128;

        private readonly IFile _fileProvider;

        public DataGenerator(IFile fileProvider)
        {
            _fileProvider = fileProvider;
        }

        public string GenerateRandomData(int fileSize)
        {
            var data = new byte[BlockSize];
            var random = new Random();

            var fileName = $"generated_{fileSize}mb";

            using (var stream = _fileProvider.OpenWrite(fileName))
            {
                for (var i = 0; i < fileSize * BlocksPerMb; i++)
                {
                    random.NextBytes(data);
                    stream.Write(data, 0, data.Length);
                }
            }

            return fileName;
        }
    }
}
namespace DownloadManager.Models
{
    public struct FileInformation
    {
        public string Name { get; set; }
        public long ContentLength { get; set; }
        public string AcceptRanges { get; set; }
    }
}
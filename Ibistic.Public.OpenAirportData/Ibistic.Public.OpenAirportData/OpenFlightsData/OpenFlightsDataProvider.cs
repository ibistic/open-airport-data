using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Ibistic.Public.OpenAirportData.OpenFlightsData
{
    public abstract class OpenFlightsDataProvider : IDisposable
    {
        protected bool Disposed { get; private set; }

        public Uri Source { get; set; }
        public Encoding SourceEncoding { get; set; } = Encoding.UTF8;

        public TimeSpan UpdateFrequency { get; set; } = TimeSpan.FromDays(1);
        public TimeSpan TimeoutValue { get; set; } = TimeSpan.FromSeconds(30);
        public string CacheFileName { get; }
        internal long RawDownloadCount { get; private set; }

        public OpenFlightsDataProvider(string cacheFileName)
        {
            CacheFileName = cacheFileName ?? throw new ArgumentNullException(nameof(cacheFileName));
        }

        protected void EnsureNotDisposed()
        {
            if (Disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        public virtual void ClearCache()
        {
            EnsureNotDisposed();

            var file = new FileInfo(CacheFileName);
            if (file.Exists)
            {
                file.Delete();
            }
        }

        internal async Task<Stream> DownloadRawData()
        {
            RawDownloadCount++;
            var client = new HttpClient();
            return await client.GetStreamAsync(Source);
        }

        internal FileStream GetRawData()
        {
            var file = new FileInfo(CacheFileName);
            if (!file.Exists || file.LastWriteTimeUtc.Add(UpdateFrequency) <= DateTime.UtcNow || file.Length == 0)
            {
                using (FileStream fileWriteStream = file.OpenWrite())
                {
                    using (Stream downloadStream = DownloadRawData().Result)
                    {
                        var downloadTask = downloadStream.CopyToAsync(fileWriteStream);
                        var downloaded = downloadTask.Wait(TimeoutValue);

                        if (!downloaded)
                        {
                            throw new TimeoutException(
                                $"Downloading the raw data took more than the permitted timeout value: {TimeoutValue}");
                        }
                    }
                }
            }

            return file.OpenRead();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (Disposed)
            {
                return;
            }

            Disposed = true;
        }

        public void Close()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void IDisposable.Dispose()
        {
            Close();
        }
    }
}

using BubbleDownloadYoutube.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
using YoutubeExtractor;

namespace BubbleDownloadYoutube.Youtube
{
    /// <summary>
    /// Provides a method to download a video from YouTube.
    /// </summary>
    public class VideoDownloader : Downloader
    {
        public DownloadItem ItemDownload { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VideoDownloader"/> class.
        /// </summary>
        /// <param name="video">The video to download.</param>
        /// <param name="savePath">The path to save the video.</param>
        /// <param name="bytesToDownload">An optional value to limit the number of bytes to download.</param>
        /// <exception cref="ArgumentNullException"><paramref name="video"/> or <paramref name="savePath"/> is <c>null</c>.</exception>
        public VideoDownloader(VideoInfo video, DownloadItem item)
            : base(video)
        {
            ItemDownload = item;
        }

        /// <summary>
        /// Starts the video download.
        /// </summary>
        /// <exception cref="IOException">The video file could not be saved.</exception>
        /// <exception cref="WebException">An error occured while downloading the video.</exception>
        public override async Task ExecuteAsync()
        {
            await ExecuteAsync(CancellationToken.None, null);
        }

        public override async Task ExecuteAsync(CancellationToken tokenCancel)
        {
            await ExecuteAsync(tokenCancel, null);
        }

        public override async Task ExecuteAsync(IProgress<int> progress)
        {
            await ExecuteAsync(CancellationToken.None, progress);
        }

        public override async Task ExecuteAsync(CancellationToken tokenCancel, IProgress<int> progress)
        {
            var url = new System.Uri(this.Video.DownloadUrl);

            StorageFile destinationFile
              = await KnownFolders.SavedPictures.CreateFileAsync(this.Video.Title + ".mp4",
                  CreationCollisionOption.GenerateUniqueName);

            var httpClient = new Windows.Web.Http.HttpClient();
            var response = await httpClient.GetAsync(url, Windows.Web.Http.HttpCompletionOption.ResponseHeadersRead);
            var fs = await destinationFile.OpenAsync(FileAccessMode.ReadWrite);

            IInputStream inputStream = await response.Content.ReadAsInputStreamAsync();
            ulong totalBytesRead = 0;
            double totalBytes = (double)response.Content.Headers.ContentLength.Value;
            int percentualAnterior = 0;
            while (true)
            {
                // Read from the web.
                IBuffer buffer = new Windows.Storage.Streams.Buffer(1024);
                buffer = await inputStream.ReadAsync(
                    buffer,
                    buffer.Capacity,
                    InputStreamOptions.None);

                if (buffer.Length == 0)
                {
                    // There is nothing else to read.
                    break;
                }
                
                // Report progress.
                totalBytesRead += buffer.Length;
                int percentual = (int)((totalBytesRead / totalBytes) * 100);
                if (progress != null && percentual != percentualAnterior)
                    progress.Report(percentual);

                percentualAnterior = percentual;
                // Write to file.
                await fs.WriteAsync(buffer);
            }
            inputStream.Dispose();
            fs.Dispose();

        }
    }


}

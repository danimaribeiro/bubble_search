using BubbleDownloadYoutube.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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
        private CancellationTokenSource cts;

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
        /// Occurs when the downlaod progress of the video file has changed.
        /// </summary>
        public event EventHandler<ProgressEventArgs> DownloadProgressChanged;
        /// <summary>
        /// Starts the video download.
        /// </summary>
        /// <exception cref="IOException">The video file could not be saved.</exception>
        /// <exception cref="WebException">An error occured while downloading the video.</exception>
        public override async void Execute()
        {
            try
            {                
                this.OnDownloadStarted(EventArgs.Empty);
                var url = new System.Uri(this.Video.DownloadUrl);

                StorageFile destinationFile
                  = await KnownFolders.VideosLibrary.CreateFileAsync(this.Video.Title + ".mp4",
                      CreationCollisionOption.GenerateUniqueName);

                var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(url);
                
                if (response.IsSuccessStatusCode)
                {
                    var file = await response.Content.ReadAsByteArrayAsync();

                    Windows.Storage.Streams.IRandomAccessStream stream = await destinationFile.OpenAsync(FileAccessMode.ReadWrite);
                    IOutputStream output = stream.GetOutputStreamAt(0);

                    DataWriter writer = new DataWriter(output);
                    writer.WriteBytes(file);
                    await writer.StoreAsync();
                    await output.FlushAsync();
                }
            }
            catch (Exception ex)
            {
                this.OnDownloadFinished(EventArgs.Empty);
            }
            this.OnDownloadFinished(EventArgs.Empty);
        }

        private void DownloadProgress(DownloadOperation download)
        {
            double percent = 100;
            if (download.Progress.TotalBytesToReceive > 0)
            {
                percent = download.Progress.BytesReceived * 100 / download.Progress.TotalBytesToReceive;
            }
            if (this.DownloadProgressChanged != null)
                this.DownloadProgressChanged(this, new ProgressEventArgs(percent));

            if (percent>= 100)
            {
                download = null;
            }
        }

    }


}

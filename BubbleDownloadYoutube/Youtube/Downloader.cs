using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExtractor;

namespace BubbleDownloadYoutube.Youtube
{
    /// <summary>
    /// Provides the base class for the <see cref="AudioDownloader"/> and <see cref="VideoDownloader"/> class.
    /// </summary>
    public abstract class Downloader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Downloader"/> class.
        /// </summary>
        /// <param name="video">The video to download/convert.</param>
        /// <param name="savePath">The path to save the video/audio.</param>
        /// /// <param name="bytesToDownload">An optional value to limit the number of bytes to download.</param>
        /// <exception cref="ArgumentNullException"><paramref name="video"/> or <paramref name="savePath"/> is <c>null</c>.</exception>
        protected Downloader(VideoInfo video)
        {
            if (video == null)
                throw new ArgumentNullException("video");
            this.Video = video;           
        }
      
        public VideoInfo Video { get; private set; }
        /// <summary>
        /// Starts the work of the <see cref="Downloader"/>.
        /// </summary>
        public abstract Task ExecuteAsync();

        public abstract Task ExecuteAsync(CancellationToken tokenCancel);

        public abstract Task ExecuteAsync(IProgress<int> progress);

        public abstract Task ExecuteAsync(CancellationToken tokenCancel, IProgress<int> progress);
      
    }
}

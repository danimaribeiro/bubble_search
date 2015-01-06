using BubbleDownloadYoutube.Common;
using BubbleDownloadYoutube.Data;
using BubbleDownloadYoutube.Youtube;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using YoutubeExtractor;

// The Pivot Application template is documented at http://go.microsoft.com/fwlink/?LinkID=391641

namespace BubbleDownloadYoutube
{
    public sealed partial class PivotPage : Page
    {
        private const string FirstGroupName = "SearchResult";
        private const string SecondGroupName = "Downloading";
        private const string ThirdGroupName = "Finished";

        private bool _ShowVoiceSearch = false;

        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");

        public PivotPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>.
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session. The state will be null the first time a page is visited.</param>
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            var language = Windows.Media.SpeechRecognition.SpeechRecognizer.SystemSpeechLanguage;
            int total = Windows.Media.SpeechRecognition.SpeechRecognizer.SupportedTopicLanguages.Count(x => x.LanguageTag == language.LanguageTag);
            if (total == 0)
            {
                _ShowVoiceSearch = true;
                VoiceButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            YoutubeDataSource.InitializeGroups();
            await new DataModel.DatabaseRepository().Initialize();
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache. Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/>.</param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            // TODO: Save the unique state of the page here.
        }


        /// <summary>
        /// Invoked when an item within a section is clicked.
        /// </summary>
        private void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Navigate to the appropriate destination page, configuring the new page
            // by passing required information as a navigation parameter
            var itemId = ((DownloadItem)e.ClickedItem).UniqueId;
            if (!Frame.Navigate(typeof(ItemPage), itemId))
            {
                throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
            }
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void Button_Search_Click(object sender, RoutedEventArgs e)
        {
            ExecuteSearch();
        }

        private void SearchBarButton_Click(object sender, RoutedEventArgs e)
        {
            ExecuteSearch();
        }

        private async void VoiceButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var language = Windows.Media.SpeechRecognition.SpeechRecognizer.SystemSpeechLanguage;
                int total = Windows.Media.SpeechRecognition.SpeechRecognizer.SupportedTopicLanguages.Count(x => x.LanguageTag == language.LanguageTag);
                if (total > 0)
                {
                    var speechRecognizer = new Windows.Media.SpeechRecognition.SpeechRecognizer();

                    // Add a web search grammar to the recognizer.
                    var webSearchGrammar = new Windows.Media.SpeechRecognition.SpeechRecognitionTopicConstraint(
                                    Windows.Media.SpeechRecognition.SpeechRecognitionScenario.Dictation, "dictation");

                    speechRecognizer.UIOptions.AudiblePrompt = "Say what you want to search for...";
                    speechRecognizer.UIOptions.ExampleText = @"Ex. 'funny cats'";
                    speechRecognizer.Constraints.Add(webSearchGrammar);

                    // Compile the constraint.
                    await speechRecognizer.CompileConstraintsAsync();

                    // Start recognition.
                    Windows.Media.SpeechRecognition.SpeechRecognitionResult speechRecognitionResult = await speechRecognizer.RecognizeWithUIAsync();
                    //await speechRecognizer.RecognizeWithUIAsync();

                    // Do something with the recognition result.
                    txtConsulta.Text = speechRecognitionResult.Text;

                    ExecuteSearch();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }

        private async void DownloadVideoToLibrary(Data.DownloadItem item)
        {
            try
            {
                YoutubeDataSource.MarkItemAsDownloading(item);
                prgDownload.Visibility = Windows.UI.Xaml.Visibility.Visible;
                prgDownload.Value = 0;

                IEnumerable<VideoInfo> videoInfos = await DownloadUrlResolver.GetDownloadUrlsAsync(item.UrlDownload);
                foreach (VideoInfo videoInfo in videoInfos)
                {
                    if (videoInfo.VideoType == YoutubeExtractor.VideoType.Mp4 && videoInfo.Resolution < 400)
                    {
                        if (videoInfo.RequiresDecryption)
                            DownloadUrlResolver.DecryptDownloadUrl(videoInfo);

                        var videoDownloader = new VideoDownloader(videoInfo, item);
                        var progress = new Progress<int>((indice) =>
                        {
                            System.Diagnostics.Debug.WriteLine(indice);
                            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                prgDownload.Value = indice;
                            });
                        });
                        await videoDownloader.ExecuteAsync(progress);
                        break;
                    }
                }
                prgDownload.IsIndeterminate = true;
                prgDownload.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                YoutubeDataSource.MarkItemAsFinished(item);
                SaveState(item);
            }
            catch (TaskCanceledException cancel)
            {
                System.Diagnostics.Debug.WriteLine("Download cancelado");
                item.Status = Estado.Aguardando;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                item.Status = Estado.Erro;
            }
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                pivot.SelectedIndex = 2;
                DownloadItem item = (sender as Button).DataContext as DownloadItem;
                DownloadVideoToLibrary(item);
                pivot.SelectedIndex = 1;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }

        private async void ExecuteSearch()
        {
            try
            {
                prgSearch.Visibility = Windows.UI.Xaml.Visibility.Visible;
                var searchResults = await YoutubeDataSource.GetSearchResultsAsync(txtConsulta.Text);
                this.DefaultViewModel[FirstGroupName] = searchResults;
                prgSearch.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                prgDownload.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }

        private async void SaveState(DownloadItem itemToSave)
        {
            string jsonItem = Newtonsoft.Json.JsonConvert.SerializeObject(itemToSave, Newtonsoft.Json.Formatting.None);

            var video = new DataModel.VideosDataContext();
            video.UniqueId = itemToSave.UniqueId;
            video.JsonData = jsonItem;
            video.CreatedAt = DateTime.Now;

            await new DataModel.DatabaseRepository().SaveVideo(video);
        }

        private void PivotItem_Loaded(object sender, RoutedEventArgs e)
        {
            var downloadedGroup = YoutubeDataSource.GetDownloading();
            this.DefaultViewModel[SecondGroupName] = downloadedGroup;
        }

        private async void PivotItem_Loaded_1(object sender, RoutedEventArgs e)
        {
            var downloadedGroup = await YoutubeDataSource.GetDownloadedAsync();
            this.DefaultViewModel[ThirdGroupName] = downloadedGroup;
        }

        private async void ClearItemsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await new DataModel.DatabaseRepository().DeleteAll();
                YoutubeDataSource.ClearFinished();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }

        private void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (((Pivot)sender).SelectedIndex)
            {
                case 0:
                    SearchBarButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    if (_ShowVoiceSearch)
                        VoiceButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    ClearItemsButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    break;

                case 1:
                    SearchBarButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    VoiceButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    ClearItemsButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    break;
                case 2:
                    SearchBarButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    VoiceButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    ClearItemsButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    break;
            }
        }
    }
}

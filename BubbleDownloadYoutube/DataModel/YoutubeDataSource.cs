using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The data model defined by this file serves as a representative example of a strongly-typed
// model.  The property names chosen coincide with data bindings in the standard item templates.
//
// Applications may use this model as a starting point and build on it, or discard it entirely and
// replace it with something appropriate to their needs. If using this model, you might improve app 
// responsiveness by initiating the data loading task in the code behind for App.xaml when the app 
// is first launched.

namespace BubbleDownloadYoutube.Data
{
    public enum Estado
    {
        Aguardando = 0,
        Downloading = 1,
        Finalizado = 2,
        Erro = 3
    }

    /// <summary>
    /// Generic item data model.
    /// </summary>
    public class DownloadItem : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        public DownloadItem(String uniqueId, String titulo, String imagePath, String descricao, String duracao, long visualizacoes)
        {
            this.UniqueId = uniqueId;
            this.Titulo = titulo;
            this.Descricao = descricao;
            this.ImagePath = imagePath;
            this.Duracao = duracao;
            this.Visualizacoes = visualizacoes;
            this.UrlDownload = "https://www.youtube.com/watch?v=" + uniqueId;
            this.Status = Estado.Aguardando;
        }

        public string UniqueId { get; private set; }
        public string Titulo { get; private set; }
        public string Duracao { get; private set; }
        public long Visualizacoes { get; private set; }
        public string Descricao { get; private set; }
        public string ImagePath { get; private set; }
        public string UrlDownload { get; private set; }

        private Estado status;
        public Estado Status
        {
            get { return status; }
            set
            {
                status = value;
                OnPropertyChanged("Cor");
            }
        }

        public string Cor
        {
            get
            {
                if (status == Estado.Erro)
                    return "red";
                else if (status == Estado.Downloading)
                    return "lightblue";
                else if (status == Estado.Finalizado)
                    return "green";
                else
                    return "transparent";
            }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public override string ToString()
        {
            return this.Titulo;
        }
    }

    /// <summary>
    /// Generic group data model.
    /// </summary>
    public class DownloadGrupos
    {
        public DownloadGrupos(String uniqueId, String titulo, String descricao)
        {
            this.UniqueId = uniqueId;
            this.Titulo = titulo;
            this.Descricao = descricao;
            this.Items = new ObservableCollection<DownloadItem>();
        }

        public string UniqueId { get; private set; }
        public string Titulo { get; private set; }
        public string Descricao { get; private set; }
        public ObservableCollection<DownloadItem> Items { get; private set; }

        public override string ToString()
        {
            return this.Titulo;
        }
    }

    /// <summary>
    /// Creates a collection of groups and items with content read from a static json file.
    /// 
    /// SampleDataSource initializes with data read from a static json file included in the 
    /// project.  This provides sample data at both design-time and run-time.
    /// </summary>
    public sealed class YoutubeDataSource
    {
        private static YoutubeDataSource _sampleDataSource = new YoutubeDataSource();

        private ObservableCollection<DownloadGrupos> _groups = new ObservableCollection<DownloadGrupos>();
        public ObservableCollection<DownloadGrupos> Groups
        {
            get { return this._groups; }
        }

        public static async Task<IEnumerable<DownloadGrupos>> GetGroupsAsync(string consulta)
        {
            await _sampleDataSource.SearchYoutubeVideosAsync(consulta);

            return _sampleDataSource.Groups;
        }

        public static async Task<DownloadGrupos> GetGroupAsync(string uniqueId, string consulta)
        {
            await _sampleDataSource.SearchYoutubeVideosAsync(consulta);
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.Groups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static async Task<DownloadItem> GetItemAsync(string uniqueId, string consulta)
        {
            await _sampleDataSource.SearchYoutubeVideosAsync(consulta);
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.Groups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        private async Task SearchYoutubeVideosAsync(string consulta)
        {
            this.Groups.Clear();

            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = "AIzaSyA4MHKy150YypY9LzPQNXTCvEgteDrBL8U",
                ApplicationName = this.GetType().ToString()
            });
            var searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.Q = consulta; // Replace with your search term.
            searchListRequest.MaxResults = 20;
            // Call the search.list method to retrieve results matching the specified query term.
            var searchListResponse = await searchListRequest.ExecuteAsync();

            var videosSearch = youtubeService.Videos.List("statistics,contentDetails");
            videosSearch.Id = string.Join(",", searchListResponse.Items.Select(x => x.Id.VideoId).ToArray());
            var videoListResponse = await videosSearch.ExecuteAsync();

            DownloadGrupos group = new DownloadGrupos("Group-1",
                                                            "Resultados", "Resultados da pesquisa");

            int indice = 0;
            foreach (var searchResult in searchListResponse.Items)
            {
                switch (searchResult.Id.Kind)
                {
                    case "youtube#video":
                        var info = videoListResponse.Items[indice];
                        TimeSpan duracao = System.Xml.XmlConvert.ToTimeSpan(info.ContentDetails.Duration);
                        
                        group.Items.Add(new DownloadItem(searchResult.Id.VideoId,
                                                  searchResult.Snippet.Title,
                                                  searchResult.Snippet.Thumbnails.Default.Url,
                                                  searchResult.Snippet.Description,
                                                  duracao.ToString(@"hh\:mm\:ss"),
                                                  (long)info.Statistics.ViewCount.Value));

                        break;
                }
                indice++;
            }
            this.Groups.Add(group);
        }
    }
}
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BubbleDownloadYoutube.DataModel
{
    public class DatabaseRepository
    {
        public SQLiteAsyncConnection Connect = new SQLiteAsyncConnection("videos.db", true);

        public DatabaseRepository()
        {            
        }

        public async Task Initialize()
        {
            await Connect.CreateTableAsync<VideosDataContext>();            
        }

        public async Task<IEnumerable<VideosDataContext>> LoadCurrentVideos()
        {
            return await Connect.Table<VideosDataContext>().OrderBy(x=> x.CreatedAt).ToListAsync();
        }

        public async Task SaveVideo(VideosDataContext video)
        {
            await Connect.InsertAsync(video);
        }

        public async Task DeleteVideo(VideosDataContext video)
        {
            await Connect.DeleteAsync(video);
        }

        public async Task DeleteAll()
        {
            await Connect.ExecuteAsync("delete from Videos;");
        }

    }
}

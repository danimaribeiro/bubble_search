using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BubbleDownloadYoutube.DataModel
{    
    [Table("Videos")]
    public class VideosDataContext 
    { 
        [PrimaryKey]
        public string UniqueId { get; set; }
        
        public DateTime CreatedAt { get; set; }

        [MaxLength(5000)]
        public string JsonData { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.StorageClient;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Table;

namespace NewsDBLibrary
{
    [Serializable]
    [JsonObject]
    public class NewsItem : TableServiceEntity
    {
        public NewsItem()
        {
            PartitionKey = DateTime.Now.ToString("MMddyyyy");
            RowKey = string.Format("{0:10}_{1}", DateTime.MaxValue.Ticks - DateTime.Now.Ticks, Guid.NewGuid());
        }
        public string description { get; set; }
        public string publishedAt { get; set; }
        public string author { get; set; }
        public string title { get; set; }
        public string url { get; set; }
        public string urlToImage { get; set; }
    }
}
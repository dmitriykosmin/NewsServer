using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.StorageClient;

namespace NewsDBLibrary
{
    public class NewsDB_AzureDataContext : TableServiceContext
    {
        public NewsDB_AzureDataContext(string baseAddress, Microsoft.WindowsAzure.StorageCredentials credentials) : base(baseAddress, credentials)
        {
        }
        public IQueryable<NewsItem> News
        {
            get
            {
                return this.CreateQuery<NewsItem>("News");
            }
        }
    }
}

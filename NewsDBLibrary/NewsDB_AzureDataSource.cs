using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace NewsDBLibrary
{
    public class NewsDB_AzureDataSource : NewsDBInterface
    {
        private static CloudStorageAccount storageAccount;
        private NewsDB_AzureDataContext context;

        static NewsDB_AzureDataSource()
        {
            storageAccount = CloudStorageAccount.FromConfigurationSetting("DataConnectionString");
            CloudTableClient.CreateTablesFromModel(typeof(NewsDB_AzureDataContext),storageAccount.TableEndpoint.AbsoluteUri,
            storageAccount.Credentials);
        }

        public NewsDB_AzureDataSource()
        {
            context = new NewsDB_AzureDataContext(storageAccount.TableEndpoint.AbsoluteUri, storageAccount.Credentials);
            context.RetryPolicy = RetryPolicies.Retry(3, TimeSpan.FromSeconds(1));
        }

        public IEnumerable<NewsItem> GetNews(DateTime Date)
        {
            List<NewsItem> results = context.News.Where(g => g.PartitionKey == Date.ToString("MMddyyyy")).ToList();
            return results;
        }

        public void AddNews(IEnumerable<NewsItem> newNews)
        {
            foreach (NewsItem newItem in newNews)
            {
                context.AddObject("News", newItem);
            }
            context.SaveChanges();
        }

        public void AddNews(NewsItem newItem)
        {
            context.AddObject("News", newItem);
            context.SaveChanges();
        }

        public void DeleteAllNews()
        {
            foreach (NewsItem Item in context.News)
            {
                context.DeleteObject(Item);
            }
            context.SaveChanges();
        }
        
        public bool IsEmpty()
        {
            if (context.News.AsEnumerable().Count() == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}

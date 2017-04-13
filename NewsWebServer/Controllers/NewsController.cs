using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using NewsDBLibrary;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using Microsoft.WindowsAzure.ServiceRuntime;
using System.Web.Http.Results;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace NewsResponder.Controllers
{
    public class NewsController : ApiController
    {
        private static CloudStorageAccount storageAccount;
        private NewsDB_AzureDataContext context;

        public class Result
        {
            public IEnumerable<NewsItem> articles { get; set; }
            public Result(IEnumerable<NewsItem> results)
            {
                articles = results;
            }
        }

        public NewsController()
        {
            InitBd();
        }

        private void InitBd()
        {
            Microsoft.WindowsAzure.CloudStorageAccount.SetConfigurationSettingPublisher((configName, configSetter) =>
            {
                configSetter(RoleEnvironment.GetConfigurationSettingValue(configName));
            });
            storageAccount = CloudStorageAccount.FromConfigurationSetting("DataConnectionString");
            CloudTableClient.CreateTablesFromModel(typeof(NewsDB_AzureDataContext), storageAccount.TableEndpoint.AbsoluteUri,
            storageAccount.Credentials);
            context = new NewsDB_AzureDataContext(storageAccount.TableEndpoint.AbsoluteUri, storageAccount.Credentials);
            context.RetryPolicy = RetryPolicies.Retry(3, TimeSpan.FromSeconds(1));
        }

        // GET: News/Get
        public async Task<Result> Get()
        {
            var result = await Task.Run(() => 
            context.News.Where(g => g.PartitionKey == DateTime.Now.ToString("MMddyyyy")));
            return new Result(result);
        }

        // GET: News/Get/id
        public async Task<Result> Get(string id)
        {
            var result = await Task.Run(() => 
            context.News.Where(g => g.PartitionKey == id));
            return new Result(result);
        }

        // POST: News/Post
        [HttpPost]
        [Authorize]
        public async Task<IHttpActionResult> Post([FromBody]JToken NewsItems)
        {
            try
            {
                string json = NewsItems.ToString();
                IEnumerable<NewsItem> news = NewsParser.ParseToList(json);
                foreach (var item in news)
                {
                    await Task.Run(() => {
                        context.AddObject("News", item);
                        context.SaveChanges();
                    });
                }
            }
            catch(Exception e)
            {
                return new ExceptionResult(e, this);
            }
            return Ok();
        }

        // POST: News/Edit
        [HttpPost]
        [Authorize]
        public async Task<IHttpActionResult> Edit([FromBody]NewsItem NewsItem)
        {
            try
            {
                await Task.Run(() =>
                {
                    var newsItem = context.News.AsEnumerable().
                    Single(x => x.RowKey == NewsItem.RowKey);
                    newsItem.title = NewsItem.title;
                    newsItem.author = NewsItem.author;
                    newsItem.description = NewsItem.description;
                    newsItem.url = NewsItem.url;
                    newsItem.urlToImage = NewsItem.urlToImage;
                    context.UpdateObject(newsItem);
                    context.SaveChanges();
                });
            }
            catch (Exception e)
            {
                return new ExceptionResult(e, this);
            }
            return Ok();
        }

        // DELETE: News/Delete/id
        [HttpDelete]
        [Authorize]
        public async Task<IHttpActionResult> Delete(string id)
        {
            try
            {
                await Task.Run(() =>
                {
                    var newsItem = context.News.AsEnumerable().
                    Single(x => x.RowKey == id);
                    context.DeleteObject(newsItem);
                    context.SaveChanges();
                });
            }
            catch (Exception e)
            {
                return new ExceptionResult(e, this);
            }
            return Ok();
        }
    }
}

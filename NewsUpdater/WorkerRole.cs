using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.StorageClient;
using Newtonsoft.Json;
using NewsDBLibrary;

namespace NewsUpdater
{
    public class WorkerRole : RoleEntryPoint
    {
        private static NewsDBInterface Db { get; set; }
        private string JsonString { get; set; }
        private static Timer timer { get; set; }

        private string Key = "03a0da2747694f4bbc18690f6346714b";

        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        public override void Run()
        {
            Trace.TraceInformation("NewsUpdater is running");

            try
            {
                RunAsync(cancellationTokenSource.Token).Wait();
            }
            finally
            {
                runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            // Число одновременных подключений
            ServicePointManager.DefaultConnectionLimit = 12;

            bool result = base.OnStart();

            Microsoft.WindowsAzure.CloudStorageAccount.SetConfigurationSettingPublisher((configName, configSetter) =>
            {
                configSetter(RoleEnvironment.GetConfigurationSettingValue(configName));
            });

            Db = new NewsDB_AzureDataSource();

            Trace.TraceInformation("NewsUpdater has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("NewsUpdater is stopping");

            cancellationTokenSource.Cancel();
            runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("NewsUpdater has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            string query = @"https://newsapi.org/v1/articles?source=google-news&sortBy=top&apiKey=" + Key;
            TimerCallback tm = new TimerCallback(LoadJson);
            //21600000 ms = 6 hours
            timer = new Timer(tm, query, 0, 21600000);
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000);
            }
            timer.Dispose();
        }

        public async void LoadJson(object obj)
        {
            string query = (string)obj;
            try
            {
                using (HttpClient client = new HttpClient())
                using (HttpResponseMessage response = await client.GetAsync(query))
                {
                    JsonString = await response.Content.ReadAsStringAsync();
                    JsonString = JsonString.Replace("\\u0026quot;", "");
                    JsonString = JsonString.Replace("\\u0026nbsp;", "");
                    List<NewsItem> temp = NewsParser.ParseToList(JsonString);
                    Db.AddNews(temp);
                }
            }
            catch (Exception e)
            {
                Trace.TraceInformation(e.Message);
            }
        }
    }
}

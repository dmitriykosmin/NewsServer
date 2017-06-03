using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsDBLibrary
{
    class NewsDB_MSSQLDataSource : NewsDBInterface
    {
        public NewsDB_MSSQLDataContext context { get; set; }

        public NewsDB_MSSQLDataSource()
        {
            context = new NewsDB_MSSQLDataContext();
        }

        public IEnumerable<NewsItem> GetNews(DateTime Date)
        {
            List<NewsItem> results = context.News.Where(g => g.PartitionKey == Date.ToString("MMddyyyy")).ToList();
            return results;
        }

        public void AddNews(NewsItem newItem)
        {
            context.News.Add(newItem);
            context.SaveChanges();
        }

        public void AddNews(IEnumerable<NewsItem> newNews)
        {
            foreach (NewsItem newItem in newNews)
            {
                context.News.Add(newItem);
            }
            context.SaveChanges();
        }

        public void DeleteAllNews()
        {
            foreach (NewsItem Item in context.News)
            {
                context.News.Remove(Item);
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

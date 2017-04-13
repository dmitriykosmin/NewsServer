using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsDBLibrary
{
    public interface NewsDBInterface
    {
        IEnumerable<NewsItem> GetNews(DateTime Date);
        void AddNews(NewsItem newItem);
        void AddNews(IEnumerable<NewsItem> newNews);

        void DeleteAllNews();
        bool IsEmpty();
    }
}

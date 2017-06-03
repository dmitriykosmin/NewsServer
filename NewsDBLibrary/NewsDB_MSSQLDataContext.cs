using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsDBLibrary
{
    public class NewsDB_MSSQLDataContext : DbContext
    {
        public NewsDB_MSSQLDataContext() : base("DBConnection") { }
        public DbSet<NewsItem> News { get; set; }
    }
}

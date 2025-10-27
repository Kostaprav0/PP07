using System;
using System.Data.Entity;

namespace Бетон2
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() : base("name=DefaultConnection")
        {
        }

        public ApplicationDbContext(string connectionString) : base(connectionString)
        {
        }

        public string GetDatabaseInfo()
        {
            try
            {
                return Database.Connection.Database;
            }
            catch
            {
                return "Не подключено";
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
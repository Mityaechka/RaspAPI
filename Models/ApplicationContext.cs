using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace RaspAPI.Models
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Facility> Facilities { get; set; }
        public static ApplicationContext Instance
        {
            get
            {
                if (instance == null)
                {
                    var optionsBuilder = new DbContextOptionsBuilder<ApplicationContext>();

                    var options = optionsBuilder.UseSqlServer(@"
Server=ms-sql-9.in-solve.ru;Initial Catalog=1gb_raspdb;
Persist Security Info=False;User ID=1gb_mityaka;Password=ea254ae8cuiw;
MultipleActiveResultSets=False;Encrypt=False;TrustServerCertificate=True;
Connection Timeout=30;").Options;
                    instance = new ApplicationContext(options);
                }
                return instance;
            }
        }
        static ApplicationContext instance { get; set; }


        public ApplicationContext(DbContextOptions<ApplicationContext> options)
                    : base(options)
        {
            Database.EnsureCreated();   // создаем базу данных при первом обращении
        }


    }
}

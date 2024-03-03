using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TimeKeepingSystem
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<MyDbContext>
    {
        public MyDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<MyDbContext>();
            //optionsBuilder.UseSqlServer(configuration.GetConnectionString("local"));
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("online"));

            return new MyDbContext(optionsBuilder.Options, configuration);
        }
    }
}

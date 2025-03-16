using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace RepositoryLayer.Context
{
    public class AddressBookContextFactory : IDesignTimeDbContextFactory<AddressBookContext>
    {
        public AddressBookContext CreateDbContext(string[] args)
        {
            // Read configuration from appsettings.json
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<AddressBookContext>();
            var connectionString = config.GetConnectionString("Sqlserver");

            optionsBuilder.UseSqlServer(connectionString);

            return new AddressBookContext(optionsBuilder.Options);
        }
    }
}
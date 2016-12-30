using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.MongoDB.Tests.TestDomain
{
    public class MongoDbContextFixture : IDisposable
    {
        public MongoDbContextFixture()
        {
            DbContext = new ServiceCollection()
                .AddDbContext<TestMongoDbContext>(dbContextOptions => dbContextOptions.UseMongoDb(connectionString: "mongodb://localhost:27017"))
                .BuildServiceProvider()
                .GetService<TestMongoDbContext>();
        }

        public TestMongoDbContext DbContext { get; }

        public void Dispose()
        {
            DbContext?.Dispose();
        }
    }
}
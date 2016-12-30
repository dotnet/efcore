using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class MongoDbServiceCollectionExtensions
    {
        public static IServiceCollection AddEntityFrameworkMongoDb([NotNull] this IServiceCollection serviceCollection)
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));

            serviceCollection.AddEntityFramework();

            serviceCollection.TryAddEnumerable(ServiceDescriptor
                .Singleton<IDatabaseProvider, DatabaseProvider<IMongoDbDatabaseProviderServices, MongoDbOptionsExtension>>());

            serviceCollection.TryAdd(new ServiceCollection()
                .AddScoped(serviceProvider => serviceProvider.GetRequiredService<IMongoDbDatabaseProviderServices>().MongoClient)
                .AddScoped<IMongoDbConnection, MongoDbConnection>()
                .AddScoped<MongoDbConventionSetBuilder>()
                .AddScoped<MongoDbDatabase>()
                .AddScoped<MongoDbDatabaseCreator>()
                .AddScoped<IMongoDbDatabaseProviderServices, MongoDbDatabaseProviderServices>()
                .AddScoped<MongoDbModelSource>()
                .AddScoped<MongoDbModelValidator>()
                .AddScoped<MongoDbValueGeneratorCache>()
                .AddScoped<MongoDbValueGeneratorSelector>()
                .AddMongoDbQuery());

            return serviceCollection;
        }

        private static IServiceCollection AddMongoDbQuery(this IServiceCollection serviceCollection)
            => serviceCollection
                .AddScoped<MongoDbQueryContextFactory>()
                .AddScoped<MongoDbEntityQueryModelVisitorFactory>()
                .AddScoped<MongoDbEntityQueryableExpressionVisitorFactory>();
    }
}
using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using MongoDB.Driver;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    public static class MongoDbContextOptionsBuilderExtensions
    {
        public static DbContextOptionsBuilder UseMongoDb([NotNull] this DbContextOptionsBuilder optionsBuilder,
            [NotNull] string connectionString,
            [CanBeNull] Action<MongoDbContextOptionsBuilder> mongoDbOptionsAction = null)
        {
            Check.NotEmpty(connectionString, nameof(connectionString));
            return SetupMongoDb(Check.NotNull(optionsBuilder, nameof(optionsBuilder)),
                extension => extension.ConnectionString = connectionString,
                mongoDbOptionsAction);
        }

        public static DbContextOptionsBuilder UseMongoDb([NotNull] this DbContextOptionsBuilder optionsBuilder,
            [NotNull] IMongoClient mongoClient,
            [CanBeNull] Action<MongoDbContextOptionsBuilder> mongoDbOptionsAction = null)
        {
            Check.NotNull(mongoClient, nameof(mongoClient));
            return SetupMongoDb(Check.NotNull(optionsBuilder, nameof(optionsBuilder)),
                extension => extension.MongoClient = mongoClient,
                mongoDbOptionsAction);
        }

        public static DbContextOptionsBuilder UseMongoDb([NotNull] this DbContextOptionsBuilder optionsBuilder,
            [NotNull] MongoClientSettings mongoClientSettings,
            [CanBeNull] Action<MongoDbContextOptionsBuilder> mongoDbOptionsAction = null)
        {
            Check.NotNull(mongoClientSettings, nameof(mongoClientSettings));
            return SetupMongoDb(Check.NotNull(optionsBuilder, nameof(optionsBuilder)),
                extension => extension.MongoClientSettings = mongoClientSettings,
                mongoDbOptionsAction);
        }

        public static DbContextOptionsBuilder UseMongoDb([NotNull] this DbContextOptionsBuilder optionsBuilder,
            [NotNull] MongoUrl mongoUrl,
            [CanBeNull] Action<MongoDbContextOptionsBuilder> mongoDbOptionsAction = null)
        {
            Check.NotNull(mongoUrl, nameof(mongoUrl));
            return SetupMongoDb(Check.NotNull(optionsBuilder, nameof(optionsBuilder)),
                extension => extension.MongoUrl = mongoUrl,
                mongoDbOptionsAction);
        }

        private static DbContextOptionsBuilder SetupMongoDb([NotNull] DbContextOptionsBuilder optionsBuilder,
            [NotNull] Action<MongoDbOptionsExtension> mongoDbOptionsExtensionAction,
            [CanBeNull] Action<MongoDbContextOptionsBuilder> mongoDbOptionsAction)
        {
            MongoDbOptionsExtension extension = GetOrCreateExtension(optionsBuilder);
            mongoDbOptionsExtensionAction(extension);
            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

            ConfigureWarnings(optionsBuilder);

            mongoDbOptionsAction?.Invoke(new MongoDbContextOptionsBuilder(optionsBuilder));

            return optionsBuilder;
        }

        private static MongoDbOptionsExtension GetOrCreateExtension([NotNull] DbContextOptionsBuilder optionsBuilder)
        {
            var existing = optionsBuilder.Options.FindExtension<MongoDbOptionsExtension>();
            return existing != null
                ? new MongoDbOptionsExtension(existing)
                : new MongoDbOptionsExtension();
        }

        private static void ConfigureWarnings([NotNull] DbContextOptionsBuilder optionsBuilder)
            => Check.NotNull(optionsBuilder, nameof(optionsBuilder))
                .ConfigureWarnings(warningsConfigurationBuilder =>
                {
                    warningsConfigurationBuilder.Default(WarningBehavior.Log);
                });
    }
}
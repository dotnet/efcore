using Azure.Core;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Kusto.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore
{
    public static class KustoDbContextOptionsExtensions
    {
        public static DbContextOptionsBuilder<TContext> UseKusto<TContext>(
            this DbContextOptionsBuilder<TContext> optionsBuilder,
            Action<KustoDbContextOptionsBuilder> kustoOptionsAction)
            where TContext : DbContext
            => (DbContextOptionsBuilder<TContext>)UseKusto(
                (DbContextOptionsBuilder)optionsBuilder,
                kustoOptionsAction);

        public static DbContextOptionsBuilder UseKusto(
            this DbContextOptionsBuilder optionsBuilder,
            Action<KustoDbContextOptionsBuilder> kustoOptionsAction)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));
            Check.NotNull(kustoOptionsAction, nameof(kustoOptionsAction));

            ConfigureWarnings(optionsBuilder);

            kustoOptionsAction.Invoke(new KustoDbContextOptionsBuilder(optionsBuilder));

            return optionsBuilder;
        }

        public static DbContextOptionsBuilder<TContext> UseKusto<TContext>(
            this DbContextOptionsBuilder<TContext> optionsBuilder,
            string clusterEndpoint,
            string databaseName,
            string applicationClientId,
            string applicationClientSecret,
            string authority,
            Action<KustoDbContextOptionsBuilder>? kustoOptionsAction = null)
            where TContext : DbContext
            => (DbContextOptionsBuilder<TContext>)UseKusto(
                (DbContextOptionsBuilder)optionsBuilder,
                clusterEndpoint,
                databaseName,
                applicationClientId,
                applicationClientSecret,
                authority,
                kustoOptionsAction);

        public static DbContextOptionsBuilder UseKusto(
            this DbContextOptionsBuilder optionsBuilder,
            string clusterEndpoint,
            string databaseName,
            string applicationClientId,
            string applicationClientSecret,
            string authority,
            Action<KustoDbContextOptionsBuilder>? kustoOptionsAction = null)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));
            Check.NotNull(clusterEndpoint, nameof(clusterEndpoint));
            Check.NotNull(databaseName, nameof(databaseName));
            Check.NotNull(applicationClientId, nameof(applicationClientId));
            Check.NotNull(applicationClientSecret, nameof(applicationClientSecret));
            Check.NotNull(authority, nameof(authority));

            var extension = optionsBuilder.Options.FindExtension<KustoOptionsExtension>()
                ?? new KustoOptionsExtension();

            extension = extension
                .WithClusterEndpoint(clusterEndpoint)
                .WithDatabaseName(databaseName)
                .WithApplicationClientId(applicationClientId)
                .WithApplicationClientSecret(applicationClientSecret)
                .WithAuthority(authority);

            ConfigureWarnings(optionsBuilder);

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

            kustoOptionsAction?.Invoke(new KustoDbContextOptionsBuilder(optionsBuilder));

            return optionsBuilder;
        }

        public static DbContextOptionsBuilder<TContext> UseKusto<TContext>(
            this DbContextOptionsBuilder<TContext> optionsBuilder,
            string clusterEndpoint,
            TokenCredential tokenCredential,
            string databaseName,
            Action<KustoDbContextOptionsBuilder>? kustoOptionsAction = null)
            where TContext : DbContext
            => (DbContextOptionsBuilder<TContext>)UseKusto(
                (DbContextOptionsBuilder)optionsBuilder,
                clusterEndpoint,
                tokenCredential,
                databaseName,
                kustoOptionsAction);

        public static DbContextOptionsBuilder UseKusto(
            this DbContextOptionsBuilder optionsBuilder,
            string clusterEndpoint,
            TokenCredential tokenCredential,
            string databaseName,
            Action<KustoDbContextOptionsBuilder>? kustoOptionsAction = null)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));
            Check.NotNull(clusterEndpoint, nameof(clusterEndpoint));
            Check.NotNull(tokenCredential, nameof(tokenCredential));
            Check.NotNull(databaseName, nameof(databaseName));

            var extension = optionsBuilder.Options.FindExtension<KustoOptionsExtension>()
                ?? new KustoOptionsExtension();

            extension = extension
                .WithClusterEndpoint(clusterEndpoint)
                .WithTokenCredential(tokenCredential)
                .WithDatabaseName(databaseName);

            ConfigureWarnings(optionsBuilder);

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

            kustoOptionsAction?.Invoke(new KustoDbContextOptionsBuilder(optionsBuilder));

            return optionsBuilder;
        }

        public static DbContextOptionsBuilder<TContext> UseKusto<TContext>(
            this DbContextOptionsBuilder<TContext> optionsBuilder,
            string connectionString,
            string databaseName,
            Action<KustoDbContextOptionsBuilder>? kustoOptionsAction = null)
            where TContext : DbContext
            => (DbContextOptionsBuilder<TContext>)UseKusto(
                (DbContextOptionsBuilder)optionsBuilder,
                connectionString,
                databaseName,
                kustoOptionsAction);

        public static DbContextOptionsBuilder UseKusto(
            this DbContextOptionsBuilder optionsBuilder,
            string connectionString,
            string databaseName,
            Action<KustoDbContextOptionsBuilder>? kustoOptionsAction = null)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));
            Check.NotNull(connectionString, nameof(connectionString));
            Check.NotNull(databaseName, nameof(databaseName));

            var extension = optionsBuilder.Options.FindExtension<KustoOptionsExtension>()
                ?? new KustoOptionsExtension();

            extension = extension
                .WithConnectionString(connectionString)
                .WithDatabaseName(databaseName);

            ConfigureWarnings(optionsBuilder);

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

            kustoOptionsAction?.Invoke(new KustoDbContextOptionsBuilder(optionsBuilder));

            return optionsBuilder;
        }

        private static void ConfigureWarnings(DbContextOptionsBuilder optionsBuilder)
        {
            var coreOptionsExtension
                = optionsBuilder.Options.FindExtension<CoreOptionsExtension>()
                ?? new CoreOptionsExtension();

            coreOptionsExtension = coreOptionsExtension.WithWarningsConfiguration(
                coreOptionsExtension.WarningsConfiguration.TryWithExplicit(
                    KustoEventId.SyncNotSupported, WarningBehavior.Throw));

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(coreOptionsExtension);
        }
    }
}

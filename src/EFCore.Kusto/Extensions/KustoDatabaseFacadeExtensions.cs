using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Kusto.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Kusto.Storage.Internal;

namespace Microsoft.EntityFrameworkCore
{
    public static class KustoDatabaseFacadeExtensions
    {
        public static KustoClient GetKustoClient(this DatabaseFacade databaseFacade)
            => GetService<ISingletonKustoClientWrapper>(databaseFacade).Client;

        private static TService GetService<TService>(IInfrastructure<IServiceProvider> databaseFacade)
            where TService : class
        {
            var service = databaseFacade.GetService<TService>();
            if (service == null)
            {
                throw new InvalidOperationException(KustoStrings.KustoNotInUse);
            }

            return service;
        }

        public static string GetKustoDatabaseId(this DatabaseFacade databaseFacade)
        {
            var kustoOptions = databaseFacade.GetService<IDbContextOptions>().FindExtension<KustoOptionsExtension>();
            if (kustoOptions == null)
            {
                throw new InvalidOperationException(KustoStrings.KustoNotInUse);
            }

            return kustoOptions.DatabaseName;
        }

        public static bool IsKusto(this DatabaseFacade database)
            => database.ProviderName == typeof(KustoOptionsExtension).Assembly.GetName().Name;
    }
}

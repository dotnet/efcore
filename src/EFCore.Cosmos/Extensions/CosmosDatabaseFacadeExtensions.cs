// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.Cosmos.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for the <see cref="DatabaseFacade" /> returned from <see cref="DbContext.Database" />
    ///     that can be used only with the Cosmos provider.
    /// </summary>
    public static class CosmosDatabaseFacadeExtensions
    {
        /// <summary>
        ///     Gets the underlying <see cref="CosmosClient" /> for this <see cref="DbContext" />.
        /// </summary>
        /// <param name="databaseFacade"> The <see cref="DatabaseFacade" /> for the context. </param>
        /// <returns> The <see cref="CosmosClient" /> </returns>
        public static CosmosClient GetCosmosClient([NotNull] this DatabaseFacade databaseFacade)
            => GetService<SingletonCosmosClientWrapper>(databaseFacade).Client;

        private static TService GetService<TService>(IInfrastructure<IServiceProvider> databaseFacade)
        {
            Check.NotNull(databaseFacade, nameof(databaseFacade));

            var service = databaseFacade.Instance.GetService<TService>();
            if (service == null)
            {
                throw new InvalidOperationException(CosmosStrings.CosmosNotInUse);
            }

            return service;
        }

        /// <summary>
        ///     <para>
        ///         Returns <see langword="true" /> if the database provider currently in use is the Cosmos provider.
        ///     </para>
        ///     <para>
        ///         This method can only be used after the <see cref="DbContext" /> has been configured because
        ///         it is only then that the provider is known. This means that this method cannot be used
        ///         in <see cref="DbContext.OnConfiguring" /> because this is where application code sets the
        ///         provider to use as part of configuring the context.
        ///     </para>
        /// </summary>
        /// <param name="database"> The facade from <see cref="DbContext.Database" />. </param>
        /// <returns> <see langword="true" /> if the Cosmos provider is being used. </returns>
        public static bool IsCosmos([NotNull] this DatabaseFacade database)
            => database.ProviderName.Equals(
                typeof(CosmosOptionsExtension).Assembly.GetName().Name,
                StringComparison.Ordinal);
    }
}

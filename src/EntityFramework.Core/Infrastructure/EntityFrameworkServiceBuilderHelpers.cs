// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection;
using System;
using System.Linq;

namespace Microsoft.Data.Entity.Infrastructure
{
    public static class EntityFrameworkServiceBuilderHelpers
    {
        public static IServiceCollection AddDataStoreSource<TDataStoreSource>(
            [NotNull] this IServiceCollection serviceCollection,
            [NotNull]string addProviderMethodName)
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));
            Check.NotEmpty(addProviderMethodName, nameof(addProviderMethodName));

            if(serviceCollection.Any(d
                => d.ServiceType == typeof(IDataStoreSource)
                    && d.ImplementationType == typeof(TDataStoreSource)))
            {
                throw new InvalidOperationException(Strings.MultipleCallsToAddProvider(addProviderMethodName));
            }

            return serviceCollection.AddSingleton<IDataStoreSource, TDataStoreSource>();
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public static class InMemoryStoreSourceExtensions
    {
        public static IInMemoryStore GetStore([NotNull] this IInMemoryStoreSource storeSource, [NotNull] IDbContextOptions options)
            => storeSource.GetNamedStore(options.Extensions.OfType<InMemoryOptionsExtension>().FirstOrDefault()?.StoreName);

        public static IInMemoryStore GetGlobalStore([NotNull] this IInMemoryStoreSource storeSource)
            => storeSource.GetNamedStore(null);
    }
}

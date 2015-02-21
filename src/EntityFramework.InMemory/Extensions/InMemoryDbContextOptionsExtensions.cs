// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.InMemory;
using Microsoft.Data.Entity.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity
{
    public static class InMemoryDbContextOptionsExtensions
    {
        public static void UseInMemoryStore([NotNull] this DbContextOptions options, bool persist = true)
        {
            Check.NotNull(options, nameof(options));

            ((IDbContextOptions)options)
                .AddOrUpdateExtension<InMemoryOptionsExtension>(x => x.Persist = persist);
        }

        public static void UseInMemoryStore<T>([NotNull] this DbContextOptions<T> options, bool persist = true)
        {
            UseInMemoryStore((DbContextOptions)options, persist);
        }
    }
}

// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Infrastructure
{
    public class DbContextOptions<TContext> : DbContextOptions
        where TContext : DbContext
    {
        public DbContextOptions()
            : base(new Dictionary<string, string>(), new Dictionary<Type, IDbContextOptionsExtension>())
        {
        }

        public DbContextOptions(
            [NotNull] IReadOnlyDictionary<string, string> rawOptions,
            [NotNull] IReadOnlyDictionary<Type, IDbContextOptionsExtension> extensions)
            : base(rawOptions, extensions)
        {
        }

        public override DbContextOptions WithExtension<TExtension>(TExtension extension)
        {
            Check.NotNull(extension, nameof(extension));

            var extensions = Extensions.ToDictionary(p => p.GetType(), p => p);
            extensions[typeof(TExtension)] = extension;

            return new DbContextOptions<TContext>(RawOptions, extensions);
        }
    }
}

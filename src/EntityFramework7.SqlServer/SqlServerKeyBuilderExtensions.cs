// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public static class SqlServerKeyBuilderExtensions
    {
        public static KeyBuilder SqlServerKeyName([NotNull] this KeyBuilder keyBuilder, [CanBeNull] string name)
        {
            Check.NotNull(keyBuilder, nameof(keyBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            keyBuilder.Metadata.SqlServer().Name = name;

            return keyBuilder;
        }

        public static KeyBuilder SqlServerClustered([NotNull] this KeyBuilder keyBuilder, bool isClustered = true)
        {
            Check.NotNull(keyBuilder, nameof(keyBuilder));

            keyBuilder.Metadata.SqlServer().IsClustered = isClustered;

            return keyBuilder;
        }
    }
}

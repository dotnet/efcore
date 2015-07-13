// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public static class SqlServerIndexBuilderExtensions
    {
        public static IndexBuilder SqlServerIndexName([NotNull] this IndexBuilder indexBuilder, [CanBeNull] string name)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            indexBuilder.Metadata.SqlServer().Name = name;

            return indexBuilder;
        }

        public static IndexBuilder SqlServerClustered([NotNull] this IndexBuilder indexBuilder, bool isClustered = true)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));

            indexBuilder.Metadata.SqlServer().IsClustered = isClustered;

            return indexBuilder;
        }
    }
}

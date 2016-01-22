// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Data.Entity.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity
{
    public static class RelationalIndexBuilderExtensions
    {
        public static IndexBuilder HasName([NotNull] this IndexBuilder indexBuilder, [CanBeNull] string name)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            indexBuilder.Metadata.Relational().Name = name;

            return indexBuilder;
        }
    }
}

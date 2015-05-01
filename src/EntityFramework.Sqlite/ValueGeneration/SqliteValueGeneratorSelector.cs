// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Entity.ValueGeneration;

namespace Microsoft.Data.Entity.Sqlite.ValueGeneration
{
    public class SqliteValueGeneratorSelector : ValueGeneratorSelector, ISqliteValueGeneratorSelector
    {
        private readonly ISqliteValueGeneratorCache _cache;

        public SqliteValueGeneratorSelector([NotNull] ISqliteValueGeneratorCache cache)
        {
            Check.NotNull(cache, nameof(cache));

            _cache = cache;
        }

        public override ValueGenerator Select(IProperty property)
        {
            Check.NotNull(property, nameof(property));

            return _cache.GetOrAdd(property, Create);
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query.Internal
{
    public class SqlServerCompiledQueryCacheKeyGenerator : RelationalCompiledQueryCacheKeyGenerator
    {
        private readonly IDbContextOptions _dbContextOptions;

        public SqlServerCompiledQueryCacheKeyGenerator(
            [NotNull] IModel model,
            [NotNull] IDbContextOptions dbContextOptions)
            : base(model, dbContextOptions)
        {
            Check.NotNull(dbContextOptions, nameof(dbContextOptions));

            _dbContextOptions = dbContextOptions;
        }

        public override object GenerateCacheKey(Expression query, bool async)
            => new SqlServerCompiledQueryCacheKey(
                GenerateCacheKeyCore(query, async),
                _dbContextOptions.FindExtension<SqlServerOptionsExtension>()?.RowNumberPaging ?? false);

        private struct SqlServerCompiledQueryCacheKey
        {
            private readonly RelationalCompiledQueryCacheKey _relationalCompiledQueryCacheKey;
            private readonly bool _useRowNumberOffset;

            public SqlServerCompiledQueryCacheKey(
                RelationalCompiledQueryCacheKey relationalCompiledQueryCacheKey, bool useRowNumberOffset)
            {
                _relationalCompiledQueryCacheKey = relationalCompiledQueryCacheKey;
                _useRowNumberOffset = useRowNumberOffset;
            }

            public override bool Equals(object obj)
                => !ReferenceEquals(null, obj)
                   && (obj is SqlServerCompiledQueryCacheKey && Equals((SqlServerCompiledQueryCacheKey)obj));

            private bool Equals(SqlServerCompiledQueryCacheKey other)
                => _relationalCompiledQueryCacheKey.Equals(other._relationalCompiledQueryCacheKey)
                   && _useRowNumberOffset == other._useRowNumberOffset;

            public override int GetHashCode()
            {
                unchecked
                {
                    return (_relationalCompiledQueryCacheKey.GetHashCode() * 397) ^ _useRowNumberOffset.GetHashCode();
                }
            }
        }
    }
}

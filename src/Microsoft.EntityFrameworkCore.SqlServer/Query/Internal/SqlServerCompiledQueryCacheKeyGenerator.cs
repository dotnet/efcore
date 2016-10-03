// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqlServerCompiledQueryCacheKeyGenerator : RelationalCompiledQueryCacheKeyGenerator
    {
        private readonly IDbContextOptions _contextOptions;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SqlServerCompiledQueryCacheKeyGenerator(
            [NotNull] IModel model,
            [NotNull] ICurrentDbContext currentContext,
            [NotNull] IDbContextOptions contextOptions)
            : base(model, currentContext, contextOptions)
        {
            Check.NotNull(contextOptions, nameof(contextOptions));

            _contextOptions = contextOptions;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override object GenerateCacheKey(Expression query, bool async)
            => new SqlServerCompiledQueryCacheKey(
                GenerateCacheKeyCore(query, async),
                _contextOptions.FindExtension<SqlServerOptionsExtension>()?.RowNumberPaging ?? false);

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
                   && obj is SqlServerCompiledQueryCacheKey
                   && Equals((SqlServerCompiledQueryCacheKey)obj);

            private bool Equals(SqlServerCompiledQueryCacheKey other)
                => _relationalCompiledQueryCacheKey.Equals(other._relationalCompiledQueryCacheKey)
                   && (_useRowNumberOffset == other._useRowNumberOffset);

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

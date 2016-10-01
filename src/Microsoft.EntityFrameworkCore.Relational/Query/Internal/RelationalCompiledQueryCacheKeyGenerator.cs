// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class RelationalCompiledQueryCacheKeyGenerator : CompiledQueryCacheKeyGenerator
    {
        private readonly IDbContextOptions _contextOptions;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public RelationalCompiledQueryCacheKeyGenerator(
            [NotNull] IModel model,
            [NotNull] ICurrentDbContext currentContext,
            [NotNull] IDbContextOptions contextOptions)
            : base(model, currentContext)
        {
            Check.NotNull(contextOptions, nameof(contextOptions));

            _contextOptions = contextOptions;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override object GenerateCacheKey(Expression query, bool async)
            => GenerateCacheKeyCore(query, async);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected new RelationalCompiledQueryCacheKey GenerateCacheKeyCore([NotNull] Expression query, bool async)
            => new RelationalCompiledQueryCacheKey(
                base.GenerateCacheKeyCore(query, async),
                RelationalOptionsExtension.Extract(_contextOptions).UseRelationalNulls);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected struct RelationalCompiledQueryCacheKey
        {
            private readonly CompiledQueryCacheKey _compiledQueryCacheKey;
            private readonly bool _useRelationalNulls;

            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public RelationalCompiledQueryCacheKey(
                CompiledQueryCacheKey compiledQueryCacheKey, bool useRelationalNulls)
            {
                _compiledQueryCacheKey = compiledQueryCacheKey;
                _useRelationalNulls = useRelationalNulls;
            }

            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public override bool Equals(object obj)
                => !ReferenceEquals(null, obj)
                   && obj is RelationalCompiledQueryCacheKey
                   && Equals((RelationalCompiledQueryCacheKey)obj);

            private bool Equals(RelationalCompiledQueryCacheKey other)
                => _compiledQueryCacheKey.Equals(other._compiledQueryCacheKey)
                   && (_useRelationalNulls == other._useRelationalNulls);

            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public override int GetHashCode()
            {
                unchecked
                {
                    return (_compiledQueryCacheKey.GetHashCode() * 397) ^ _useRelationalNulls.GetHashCode();
                }
            }
        }
    }
}

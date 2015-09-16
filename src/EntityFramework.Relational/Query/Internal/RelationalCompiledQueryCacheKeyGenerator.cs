// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query.Internal
{
    public class RelationalCompiledQueryCacheKeyGenerator : CompiledQueryCacheKeyGenerator
    {
        private readonly IDbContextOptions _contextOptions;

        public RelationalCompiledQueryCacheKeyGenerator(
            [NotNull] IModel model, [NotNull] IDbContextOptions contextOptions)
            : base(model)
        {
            Check.NotNull(contextOptions, nameof(contextOptions));

            _contextOptions = contextOptions;
        }

        public override object GenerateCacheKey(Expression query, bool async)
            => GenerateCacheKeyCore(query, async);

        protected new RelationalCompiledQueryCacheKey GenerateCacheKeyCore([NotNull] Expression query, bool async)
            => new RelationalCompiledQueryCacheKey(
                base.GenerateCacheKeyCore(query, async),
                RelationalOptionsExtension.Extract(_contextOptions).UseRelationalNulls);

        protected struct RelationalCompiledQueryCacheKey
        {
            private readonly CompiledQueryCacheKey _compiledQueryCacheKey;
            private readonly bool _useRelationalNulls;

            public RelationalCompiledQueryCacheKey(
                CompiledQueryCacheKey compiledQueryCacheKey, bool useRelationalNulls)
            {
                _compiledQueryCacheKey = compiledQueryCacheKey;
                _useRelationalNulls = useRelationalNulls;
            }

            public override bool Equals(object obj)
                => !ReferenceEquals(null, obj)
                   && (obj is RelationalCompiledQueryCacheKey
                       && Equals((RelationalCompiledQueryCacheKey)obj));

            private bool Equals(RelationalCompiledQueryCacheKey other)
                => _compiledQueryCacheKey.Equals(other._compiledQueryCacheKey)
                   && _useRelationalNulls == other._useRelationalNulls;

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

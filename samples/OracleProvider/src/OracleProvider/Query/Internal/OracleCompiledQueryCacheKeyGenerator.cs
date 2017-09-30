// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class OracleCompiledQueryCacheKeyGenerator : RelationalCompiledQueryCacheKeyGenerator
    {
        public OracleCompiledQueryCacheKeyGenerator(
            [NotNull] CompiledQueryCacheKeyGeneratorDependencies dependencies,
            [NotNull] RelationalCompiledQueryCacheKeyGeneratorDependencies relationalDependencies)
            : base(dependencies, relationalDependencies)
        {
        }

        public override object GenerateCacheKey(Expression query, bool async)
            => new OracleCompiledQueryCacheKey(GenerateCacheKeyCore(query, async));

        private struct OracleCompiledQueryCacheKey
        {
            private readonly RelationalCompiledQueryCacheKey _relationalCompiledQueryCacheKey;

            public OracleCompiledQueryCacheKey(
                RelationalCompiledQueryCacheKey relationalCompiledQueryCacheKey)
            {
                _relationalCompiledQueryCacheKey = relationalCompiledQueryCacheKey;
            }

            public override bool Equals(object obj)
                => !ReferenceEquals(null, obj)
                   && obj is OracleCompiledQueryCacheKey
                   && Equals((OracleCompiledQueryCacheKey)obj);

            private bool Equals(OracleCompiledQueryCacheKey other)
                => _relationalCompiledQueryCacheKey.Equals(other._relationalCompiledQueryCacheKey);

            public override int GetHashCode()
            {
                return _relationalCompiledQueryCacheKey.GetHashCode();
            }
        }
    }
}

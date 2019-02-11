// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal
{
    /// <summary>
    ///     <para>
    ///         This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///         directly from your code. This API may change or be removed in future releases.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped"/>. This means that each
    ///         <see cref="DbContext"/> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public class SqlServerCompiledQueryCacheKeyGenerator : RelationalCompiledQueryCacheKeyGenerator
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SqlServerCompiledQueryCacheKeyGenerator(
            [NotNull] CompiledQueryCacheKeyGeneratorDependencies dependencies,
            [NotNull] RelationalCompiledQueryCacheKeyGeneratorDependencies relationalDependencies)
            : base(dependencies, relationalDependencies)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override object GenerateCacheKey(Expression query, bool async)
            => new SqlServerCompiledQueryCacheKey(
                GenerateCacheKeyCore(query, async),
                RelationalDependencies.ContextOptions.FindExtension<SqlServerOptionsExtension>()?.RowNumberPaging ?? false);

        private readonly struct SqlServerCompiledQueryCacheKey
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
                => !(obj is null)
                   && obj is SqlServerCompiledQueryCacheKey
                   && Equals((SqlServerCompiledQueryCacheKey)obj);

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

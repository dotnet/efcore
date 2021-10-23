// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    /// <remarks>
    ///     The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///     <see cref="DbContext" /> instance will use its own instance of this service.
    ///     The implementation may depend on other services registered with any lifetime.
    ///     The implementation does not need to be thread-safe.
    /// </remarks>
    public class SqlServerCompiledQueryCacheKeyGenerator : RelationalCompiledQueryCacheKeyGenerator
    {
        private readonly ISqlServerConnection _sqlServerConnection;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqlServerCompiledQueryCacheKeyGenerator(
            CompiledQueryCacheKeyGeneratorDependencies dependencies,
            RelationalCompiledQueryCacheKeyGeneratorDependencies relationalDependencies,
            ISqlServerConnection sqlServerConnection)
            : base(dependencies, relationalDependencies)
        {
            Check.NotNull(sqlServerConnection, nameof(sqlServerConnection));

            _sqlServerConnection = sqlServerConnection;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override object GenerateCacheKey(Expression query, bool async)
            => new SqlServerCompiledQueryCacheKey(
                GenerateCacheKeyCore(query, async),
                _sqlServerConnection.IsMultipleActiveResultSetsEnabled);

        private readonly struct SqlServerCompiledQueryCacheKey : IEquatable<SqlServerCompiledQueryCacheKey>
        {
            private readonly RelationalCompiledQueryCacheKey _relationalCompiledQueryCacheKey;
            private readonly bool _multipleActiveResultSetsEnabled;

            public SqlServerCompiledQueryCacheKey(
                RelationalCompiledQueryCacheKey relationalCompiledQueryCacheKey,
                bool multipleActiveResultSetsEnabled)
            {
                _relationalCompiledQueryCacheKey = relationalCompiledQueryCacheKey;
                _multipleActiveResultSetsEnabled = multipleActiveResultSetsEnabled;
            }

            public override bool Equals(object? obj)
                => obj is SqlServerCompiledQueryCacheKey sqlServerCompiledQueryCacheKey
                    && Equals(sqlServerCompiledQueryCacheKey);

            public bool Equals(SqlServerCompiledQueryCacheKey other)
                => _relationalCompiledQueryCacheKey.Equals(other._relationalCompiledQueryCacheKey)
                    && _multipleActiveResultSetsEnabled == other._multipleActiveResultSetsEnabled;

            public override int GetHashCode()
                => HashCode.Combine(_relationalCompiledQueryCacheKey, _multipleActiveResultSetsEnabled);
        }
    }
}

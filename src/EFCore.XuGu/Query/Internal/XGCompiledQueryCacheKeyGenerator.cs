// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.Internal
{
    public class XGCompiledQueryCacheKeyGenerator : RelationalCompiledQueryCacheKeyGenerator
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public XGCompiledQueryCacheKeyGenerator(
            [NotNull] CompiledQueryCacheKeyGeneratorDependencies dependencies,
            [NotNull] RelationalCompiledQueryCacheKeyGeneratorDependencies relationalDependencies)
            : base(dependencies, relationalDependencies)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override object GenerateCacheKey(Expression query, bool async)
        {
            var extensions = RelationalDependencies.ContextOptions.FindExtension<XGOptionsExtension>();
            return new XGCompiledQueryCacheKey(
                GenerateCacheKeyCore(query, async),
                extensions?.ServerVersion,
                extensions?.NoBackslashEscapes ?? false);
        }

        private readonly struct XGCompiledQueryCacheKey
        {
            private readonly RelationalCompiledQueryCacheKey _relationalCompiledQueryCacheKey;
            private readonly ServerVersion _serverVersion;
            private readonly bool _noBackslashEscapes;

            public XGCompiledQueryCacheKey(
                RelationalCompiledQueryCacheKey relationalCompiledQueryCacheKey,
                ServerVersion serverVersion,
                bool noBackslashEscapes)
            {
                _relationalCompiledQueryCacheKey = relationalCompiledQueryCacheKey;
                _serverVersion = serverVersion;
                _noBackslashEscapes = noBackslashEscapes;
            }

            public override bool Equals(object obj)
                => !(obj is null)
                   && obj is XGCompiledQueryCacheKey key
                   && Equals(key);

            private bool Equals(XGCompiledQueryCacheKey other)
                => _relationalCompiledQueryCacheKey.Equals(other._relationalCompiledQueryCacheKey)
                   && Equals(_serverVersion, other._serverVersion)
                   && _noBackslashEscapes == other._noBackslashEscapes
                ;

            public override int GetHashCode()
                => HashCode.Combine(_relationalCompiledQueryCacheKey,
                    _serverVersion,
                    _noBackslashEscapes);
        }
    }
}

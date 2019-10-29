// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public class DbSetFinder : IDbSetFinder
    {
        private readonly ConcurrentDictionary<Type, IReadOnlyList<DbSetProperty>> _cache
            = new ConcurrentDictionary<Type, IReadOnlyList<DbSetProperty>>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IReadOnlyList<DbSetProperty> FindSets(Type contextType)
            => _cache.GetOrAdd(contextType, FindSetsNonCached);

        private static DbSetProperty[] FindSetsNonCached(Type contextType)
        {
            var factory = new ClrPropertySetterFactory();

            return contextType.GetRuntimeProperties()
                .Where(
                    p => !p.IsStatic()
                        && !p.GetIndexParameters().Any()
                        && p.DeclaringType != typeof(DbContext)
                        && p.PropertyType.GetTypeInfo().IsGenericType
                        && (p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>)
#pragma warning disable CS0618 // Type or member is obsolete
                            || p.PropertyType.GetGenericTypeDefinition() == typeof(DbQuery<>)))
#pragma warning restore CS0618 // Type or member is obsolete
                .OrderBy(p => p.Name)
                .Select(
                    p => new DbSetProperty(
                        p.Name,
                        p.PropertyType.GetTypeInfo().GenericTypeArguments.Single(),
                        p.SetMethod == null ? null : factory.Create(p),
#pragma warning disable CS0618 // Type or member is obsolete
                        p.PropertyType.GetGenericTypeDefinition() == typeof(DbQuery<>)))
#pragma warning restore CS0618 // Type or member is obsolete
                .ToArray();
        }
    }
}

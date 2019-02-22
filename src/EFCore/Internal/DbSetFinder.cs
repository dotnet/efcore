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
    ///         This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///         directly from your code. This API may change or be removed in future releases.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton"/>. This means a single instance
    ///         is used by many <see cref="DbContext"/> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped"/>.
    ///     </para>
    /// </summary>
    public class DbSetFinder : IDbSetFinder
    {
        private readonly ConcurrentDictionary<Type, IReadOnlyList<DbSetProperty>> _cache
            = new ConcurrentDictionary<Type, IReadOnlyList<DbSetProperty>>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IReadOnlyList<DbSetProperty> FindSets(DbContext context)
            => _cache.GetOrAdd(context.GetType(), FindSets);

        private static DbSetProperty[] FindSets(Type contextType)
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

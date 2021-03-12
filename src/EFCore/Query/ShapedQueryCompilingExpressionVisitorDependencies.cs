// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         Service dependencies parameter class for <see cref="ShapedQueryCompilingExpressionVisitor" />
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    ///     <para>
    ///         Do not construct instances of this class directly from either provider or application code as the
    ///         constructor signature may change as new dependencies are added. Instead, use this type in
    ///         your constructor so that an instance will be created and injected automatically by the
    ///         dependency injection container. To create an instance with some dependent services replaced,
    ///         first resolve the object from the dependency injection container, then replace selected
    ///         services using the 'With...' methods. Do not call the constructor at any point in this process.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" /> and multiple registrations
    ///         are allowed. This means a single instance of each service is used by many <see cref="DbContext" />
    ///         instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public sealed record ShapedQueryCompilingExpressionVisitorDependencies
    {
        /// <summary>
        ///     <para>
        ///         Creates the service dependencies parameter object for a <see cref="ShapedQueryCompilingExpressionVisitor" />.
        ///     </para>
        ///     <para>
        ///         Do not call this constructor directly from either provider or application code as it may change
        ///         as new dependencies are added. Instead, use this type in your constructor so that an instance
        ///         will be created and injected automatically by the dependency injection container. To create
        ///         an instance with some dependent services replaced, first resolve the object from the dependency
        ///         injection container, then replace selected services using the 'With...' methods. Do not call
        ///         the constructor at any point in this process.
        ///     </para>
        ///     <para>
        ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///         any release. You should only use it directly in your code with extreme caution and knowing that
        ///         doing so can result in application failures when updating to a new Entity Framework Core release.
        ///     </para>
        /// </summary>
        [EntityFrameworkInternal]
        public ShapedQueryCompilingExpressionVisitorDependencies(
            [NotNull] IEntityMaterializerSource entityMaterializerSource,
            [NotNull] ITypeMappingSource typeMappingSource,
            [NotNull] IMemoryCache memoryCache,
            [NotNull] ICoreSingletonOptions coreSingletonOptions)
        {
            Check.NotNull(entityMaterializerSource, nameof(entityMaterializerSource));
            Check.NotNull(typeMappingSource, nameof(typeMappingSource));
            Check.NotNull(memoryCache, nameof(memoryCache));
            Check.NotNull(coreSingletonOptions, nameof(coreSingletonOptions));

            EntityMaterializerSource = entityMaterializerSource;
            TypeMappingSource = typeMappingSource;
            MemoryCache = memoryCache;
            CoreSingletonOptions = coreSingletonOptions;
        }

        /// <summary>
        ///     The materializer source.
        /// </summary>
        public IEntityMaterializerSource EntityMaterializerSource { get; [param: NotNull] init; }

        /// <summary>
        ///     The type mapping source.
        /// </summary>
        public ITypeMappingSource TypeMappingSource { get; [param: NotNull] init; }

        /// <summary>
        ///     The memory cache.
        /// </summary>
        public IMemoryCache MemoryCache { get; [param: NotNull] init; }

        /// <summary>
        ///     Core singleton options.
        /// </summary>
        public ICoreSingletonOptions CoreSingletonOptions { get; [param: NotNull] init; }
    }
}

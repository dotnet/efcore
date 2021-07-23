// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Update.Internal
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
    public class KeyValueIndexFactorySource : IKeyValueIndexFactorySource
    {
        private readonly ConcurrentDictionary<IKey, IKeyValueIndexFactory> _factories = new();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IKeyValueIndexFactory GetKeyValueIndexFactory(IKey key)
            => _factories.GetOrAdd(key, Create);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IKeyValueIndexFactory Create(IKey key)
            => (IKeyValueIndexFactory)typeof(KeyValueIndexFactorySource).GetTypeInfo()
                .GetDeclaredMethod(nameof(CreateFactory))!
                .MakeGenericMethod(key.GetKeyType())
                .Invoke(null, new object[] { key })!;

        [UsedImplicitly]
        private static IKeyValueIndexFactory CreateFactory<TKey>(IKey key) where TKey : notnull
            => new KeyValueIndexFactory<TKey>(key.GetPrincipalKeyValueFactory<TKey>());
    }
}

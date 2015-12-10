// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;

namespace Microsoft.Data.Entity.Update.Internal
{
    public class KeyValueIndexFactorySource : IdentityMapFactoryFactoryBase, IKeyValueIndexFactorySource
    {
        private readonly ConcurrentDictionary<IKey, IKeyValueIndexFactory> _factories
            = new ConcurrentDictionary<IKey, IKeyValueIndexFactory>();

        public virtual IKeyValueIndexFactory GetKeyValueIndexFactory(IKey key)
            => _factories.GetOrAdd(key, Create);

        [GenericMethodFactory(nameof(CreateFactory), typeof(TypeArgumentCategory.Keys))]
        public virtual IKeyValueIndexFactory Create([NotNull] IKey key)
            => (IKeyValueIndexFactory)typeof(KeyValueIndexFactorySource).GetTypeInfo()
                .GetDeclaredMethod(nameof(CreateFactory))
                .MakeGenericMethod(GetKeyType(key))
                .Invoke(null, new object[] { key });

        [UsedImplicitly]
        private static IKeyValueIndexFactory CreateFactory<TKey>(IKey key)
            => new KeyValueIndexFactory<TKey>(key.GetPrincipalKeyValueFactory<TKey>());
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public class CompositeKeyValueFactory : KeyValueFactory
    {
        private readonly int[] _indexes;

        public CompositeKeyValueFactory([NotNull] IKey key)
            : base(key)
        {
            _indexes = key.Properties.Select(p => p.GetIndex()).ToArray();
        }

        public override IKeyValue Create(ValueBuffer valueBuffer)
            => Create(_indexes, i => valueBuffer[i]);

        public override IKeyValue Create(IReadOnlyList<IProperty> properties, ValueBuffer valueBuffer)
            => Create(properties, p => valueBuffer[p.GetIndex()]);

        public override IKeyValue Create(object value)
        {
            var components = value as object[] ?? new[] { value };

            return components.Any(t => t == null)
                ? new KeyValue<object[]>(null, null)
                : new KeyValue<object[]>(Key, components);
        }

        private KeyValue<object[]> Create<T>(IReadOnlyList<T> ts, Func<T, object> reader)
        {
            var components = new object[ts.Count];

            for (var i = 0; i < ts.Count; i++)
            {
                var value = reader(ts[i]);

                if (value == null)
                {
                    return new KeyValue<object[]>(null, null);
                }

                components[i] = value;
            }

            return new KeyValue<object[]>(Key, components);
        }
    }
}

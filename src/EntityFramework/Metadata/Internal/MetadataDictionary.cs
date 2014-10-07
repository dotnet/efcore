// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class MetadataDictionary<TKey, TValue>
        where TKey : class
    {
        private readonly Dictionary<TKey, Tuple<TValue, ConfigurationSource>> _values =
            new Dictionary<TKey, Tuple<TValue, ConfigurationSource>>();

        public virtual TValue GetOrAdd(
            [NotNull] Func<TKey> getKey,
            [NotNull] Func<TKey> createKey,
            [NotNull] Func<TKey, TValue> createValue,
            ConfigurationSource configurationSource)
        {
            Check.NotNull(getKey, "getKey");
            Check.NotNull(createKey, "createKey");
            Check.NotNull(createValue, "createValue");

            TValue value;
            var key = getKey();
            if (key == null)
            {
                key = createKey();
            }
            else
            {
                if (TryGetValue(key, configurationSource, out value))
                {
                    return value;
                }
                configurationSource = ConfigurationSource.Explicit;
            }

            value = createValue(key);
            Add(key, value, configurationSource);
            return value;
        }

        private bool TryGetValue(TKey key, ConfigurationSource configurationSource, out TValue value)
        {
            Tuple<TValue, ConfigurationSource> tuple;
            if (_values.TryGetValue(key, out tuple))
            {
                if (configurationSource != tuple.Item2
                    && configurationSource.Overrides(tuple.Item2))
                {
                    _values.Remove(key);
                    tuple = new Tuple<TValue, ConfigurationSource>(tuple.Item1, configurationSource);
                    _values.Add(key, tuple);
                }

                value = tuple.Item1;
                return true;
            }

            value = default(TValue);
            return false;
        }

        public virtual void Add([NotNull] TKey key, [NotNull] TValue value, ConfigurationSource configurationSource)
        {
            Check.NotNull(key, "key");
            Check.NotNull(value, "value");

            _values.Add(key, new Tuple<TValue, ConfigurationSource>(value, configurationSource));
        }

        public virtual bool Remove([NotNull] TKey key, ConfigurationSource configurationSource)
        {
            Check.NotNull(key, "key");

            Tuple<TValue, ConfigurationSource> tuple;
            if (_values.TryGetValue(key, out tuple))
            {
                if (configurationSource.Overrides(tuple.Item2))
                {
                    _values.Remove(key);
                    return true;
                }
            }

            return configurationSource == ConfigurationSource.Explicit;
        }
    }
}

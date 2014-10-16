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
        where TValue : class
    {
        private readonly Dictionary<TKey, Tuple<TValue, ConfigurationSource>> _values =
            new Dictionary<TKey, Tuple<TValue, ConfigurationSource>>();

        public virtual TValue GetOrAdd(
            [NotNull] Func<TKey> getKey,
            [NotNull] Func<TKey> createKey,
            [NotNull] Func<TKey, bool, TValue> createValue,
            ConfigurationSource configurationSource)
        {
            return GetOrReplace(getKey, () => null, createKey, createValue, configurationSource);
        }

        public virtual TValue GetOrReplace(
            [NotNull] Func<TKey> getKey,
            [CanBeNull] Func<TKey> getKeyToReplace,
            [NotNull] Func<TKey> createKey,
            [NotNull] Func<TKey, bool, TValue> createValue,
            ConfigurationSource configurationSource)
        {
            Check.NotNull(getKey, "getKey");
            Check.NotNull(createKey, "createKey");
            Check.NotNull(createValue, "createValue");

            var isNewKey = false;
            TValue value;
            var key = getKey();
            if (key == null)
            {
                var keyToRemove = getKeyToReplace == null
                    ? null
                    : getKeyToReplace();
                if (keyToRemove != null
                    && !Remove(keyToRemove, configurationSource))
                {
                    return null;
                }
                key = createKey();
                isNewKey = true;
            }
            else
            {
                value = TryGetValue(key, configurationSource);
                if (value != null)
                {
                    return value;
                }
                configurationSource = ConfigurationSource.Explicit;
            }

            value = createValue(key, isNewKey);
            Add(key, value, configurationSource);
            return value;
        }

        public virtual TValue TryGetValue([NotNull] TKey key, ConfigurationSource configurationSource)
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

                return tuple.Item1;
            }

            return default(TValue);
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

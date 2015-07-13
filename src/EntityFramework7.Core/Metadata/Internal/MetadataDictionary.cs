// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class MetadataDictionary<TKey, TValue>
        where TKey : class
        where TValue : class
    {
        private readonly Dictionary<TKey, Tuple<TValue, ConfigurationSource>> _values =
            new Dictionary<TKey, Tuple<TValue, ConfigurationSource>>();

        private const ConfigurationSource DefaultConfigurationSource = ConfigurationSource.Explicit;

        public virtual TValue GetOrAdd(
            [NotNull] Func<TKey> getKey,
            [NotNull] Func<TKey> createKey,
            [NotNull] Func<TKey, TValue> createValue,
            ConfigurationSource configurationSource)
            => GetOrAdd(getKey, createKey, createValue, null, configurationSource);

        public virtual TValue GetOrAdd(
            [NotNull] Func<TKey> getKey,
            [NotNull] Func<TKey> createKey,
            [NotNull] Func<TKey, TValue> createValue,
            [CanBeNull] Func<TValue, TValue> onNewKeyAdded,
            ConfigurationSource configurationSource)
        {
            var isNewKey = false;
            TValue value;
            var key = getKey();
            if (key == null)
            {
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
                configurationSource = DefaultConfigurationSource;
            }

            value = createValue(key);

            Add(key, value, configurationSource);

            if (isNewKey
                && onNewKeyAdded != null)
            {
                value = onNewKeyAdded.Invoke(value);
            }

            return value;
        }

        public virtual TValue TryGetValue([NotNull] TKey key, ConfigurationSource configurationSource)
            => GetTuple(key, configurationSource).Item1;

        public virtual ConfigurationSource UpdateConfigurationSource([NotNull] TKey key, ConfigurationSource configurationSource)
            => GetTuple(key, configurationSource).Item2;

        private Tuple<TValue, ConfigurationSource> GetTuple([NotNull] TKey key, ConfigurationSource configurationSource)
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

                return tuple;
            }

            return new Tuple<TValue, ConfigurationSource>(default(TValue), DefaultConfigurationSource);
        }

        public virtual ConfigurationSource GetConfigurationSource([NotNull] TKey key)
        {
            Tuple<TValue, ConfigurationSource> tuple;
            return _values.TryGetValue(key, out tuple)
                ? tuple.Item2
                : DefaultConfigurationSource;
        }

        public virtual void Add([NotNull] TKey key, [NotNull] TValue value, ConfigurationSource configurationSource)
            => _values.Add(key, new Tuple<TValue, ConfigurationSource>(value, configurationSource));

        public virtual ConfigurationSource? Remove([NotNull] TKey key, ConfigurationSource configurationSource, bool canOverrideSameSource = true)
        {
            Tuple<TValue, ConfigurationSource> tuple;
            if (_values.TryGetValue(key, out tuple))
            {
                if (configurationSource.Overrides(tuple.Item2)
                    && (tuple.Item2 != configurationSource || canOverrideSameSource))
                {
                    _values.Remove(key);
                    return tuple.Item2;
                }
                return null;
            }

            return configurationSource.Overrides(DefaultConfigurationSource)
                   && (DefaultConfigurationSource != configurationSource || canOverrideSameSource)
                ? DefaultConfigurationSource
                : (ConfigurationSource?)null;
        }
    }
}

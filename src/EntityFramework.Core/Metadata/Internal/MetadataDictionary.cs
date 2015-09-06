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
            => GetOrAdd(getKey, createKey, createValue, null, null, configurationSource);

        public virtual TValue GetOrAdd(
            [NotNull] Func<TKey> getKey,
            [NotNull] Func<TKey> createKey,
            [NotNull] Func<TKey, TValue> createValue,
            [CanBeNull] Func<TValue, TValue> onNewKeyAdded,
            [CanBeNull] ConfigurationSource? newKeyConfigurationSource,
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
            if (isNewKey)
            {
                if (onNewKeyAdded != null)
                {
                    newKeyConfigurationSource = newKeyConfigurationSource?.Max(configurationSource) ?? configurationSource;
                    Add(key, value, newKeyConfigurationSource.Value);

                    value = onNewKeyAdded.Invoke(value);

                    Remove(key, ConfigurationSource.Explicit);
                }
                Add(key, value, configurationSource);
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

        public virtual bool CanRemove([NotNull] TKey key, ConfigurationSource configurationSource, bool canOverrideSameSource)
        {
            var currentConfigurationSource = GetConfigurationSource(key);
            return configurationSource.Overrides(currentConfigurationSource)
                   && (canOverrideSameSource || configurationSource != currentConfigurationSource);
        }

        public virtual ConfigurationSource? Remove([NotNull] TKey key, ConfigurationSource configurationSource, bool canOverrideSameSource = true)
        {
            if (!CanRemove(key, configurationSource, canOverrideSameSource))
            {
                return null;
            }

            Tuple<TValue, ConfigurationSource> tuple;
            if (_values.TryGetValue(key, out tuple))
            {
                _values.Remove(key);
                return tuple.Item2;
            }

            return DefaultConfigurationSource;
        }
    }
}

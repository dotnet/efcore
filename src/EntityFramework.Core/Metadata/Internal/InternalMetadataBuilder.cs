// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public abstract class InternalMetadataBuilder
    {
        protected InternalMetadataBuilder([NotNull] ConventionalAnnotatable metadata)
        {
            Metadata = metadata;
        }

        public virtual ConventionalAnnotatable Metadata { get; }
        public abstract InternalModelBuilder ModelBuilder { get; }

        public virtual bool HasAnnotation(
            [NotNull] string name, [CanBeNull] object value, ConfigurationSource configurationSource)
            => HasAnnotation(name, value, configurationSource, canOverrideSameSource: true);

        private bool HasAnnotation(
            string name, object value, ConfigurationSource configurationSource, bool canOverrideSameSource)
        {
            var existingAnnotation = Metadata.FindAnnotation(name);
            if (existingAnnotation != null)
            {
                if (existingAnnotation.Value.Equals(value))
                {
                    existingAnnotation.UpdateConfigurationSource(configurationSource);
                    return true;
                }

                var existingConfigurationSource = existingAnnotation.GetConfigurationSource();
                if (!configurationSource.Overrides(existingConfigurationSource)
                    || (configurationSource == existingConfigurationSource && !canOverrideSameSource))
                {
                    return false;
                }

                if (value == null)
                {
                    var removed = Metadata.RemoveAnnotation(name);
                    Debug.Assert(removed == existingAnnotation);
                }
                else
                {
                    Metadata.SetAnnotation(name, value, configurationSource);
                }

                return true;
            }

            if (value != null)
            {
                Metadata.AddAnnotation(name, value, configurationSource);
            }

            return true;
        }

        protected virtual void MergeAnnotationsFrom([NotNull] InternalMetadataBuilder annotatableBuilder)
        {
            foreach (var annotation in annotatableBuilder.Metadata.GetAnnotations())
            {
                HasAnnotation(
                    annotation.Name,
                    annotation.Value,
                    annotation.GetConfigurationSource(),
                    canOverrideSameSource: false);
            }
        }

        protected static TValue Add<TKey, TValue, TId>(
            [NotNull] TId id,
            [NotNull] Action<TKey, ConfigurationSource> updateConfigurationSource,
            [NotNull] Func<TId, ConfigurationSource, TKey> createKey,
            [NotNull] Func<TKey, TValue> createValue,
            ConfigurationSource configurationSource,
            [CanBeNull] Action<TId> onNewKeyAdding = null,
            [CanBeNull] Func<TValue, TValue> onNewKeyAdded = null)
            => GetOrAdd(id,
                i => default(TKey),
                updateConfigurationSource,
                createKey,
                createValue,
                configurationSource,
                onNewKeyAdding,
                onNewKeyAdded);

        protected static TValue GetOrAdd<TKey, TValue, TId>(
            [NotNull] TId id,
            [NotNull] Func<TId, TKey> findKey,
            [NotNull] Action<TKey, ConfigurationSource> updateConfigurationSource,
            [NotNull] Func<TId, ConfigurationSource, TKey> createKey,
            [NotNull] Func<TKey, TValue> createValue,
            ConfigurationSource configurationSource,
            [CanBeNull] Action<TId> onNewKeyAdding = null,
            [CanBeNull] Func<TValue, TValue> onNewKeyAdded = null)
        {
            var key = findKey(id);
            var isNewKey = key == null;
            if (isNewKey)
            {
                onNewKeyAdding?.Invoke(id);

                key = createKey(id, configurationSource);
            }
            else
            {
                updateConfigurationSource(key, configurationSource);
            }

            var value = createValue(key);
            if (isNewKey
                && onNewKeyAdded != null
                && value != null)
            {
                value = onNewKeyAdded(value);
            }

            return value;
        }
    }
}

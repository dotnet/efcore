// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public abstract class InternalMetadataBuilder
    {
        private readonly LazyRef<Dictionary<string, ConfigurationSource>> _annotationSources =
            new LazyRef<Dictionary<string, ConfigurationSource>>(() => new Dictionary<string, ConfigurationSource>());

        protected InternalMetadataBuilder([NotNull] Annotatable metadata)
        {
            Metadata = metadata;
        }

        public virtual Annotatable Metadata { get; }
        public abstract InternalModelBuilder ModelBuilder { get; }

        public virtual bool HasAnnotation(
            [NotNull] string name, [CanBeNull] object value, ConfigurationSource configurationSource)
            => HasAnnotation(name, value, configurationSource, canOverrideSameSource: true);

        private bool HasAnnotation(
            string name, object value, ConfigurationSource configurationSource, bool canOverrideSameSource)
        {
            var existingValue = Metadata[name];
            if (existingValue != null)
            {
                ConfigurationSource existingConfigurationSource;
                if (!_annotationSources.Value.TryGetValue(name, out existingConfigurationSource))
                {
                    existingConfigurationSource = ConfigurationSource.Explicit;
                }

                if ((value == null || !existingValue.Equals(value))
                    && (!configurationSource.Overrides(existingConfigurationSource)
                        || configurationSource == existingConfigurationSource && !canOverrideSameSource))
                {
                    return false;
                }

                configurationSource = configurationSource.Max(existingConfigurationSource);
            }

            if (value != null)
            {
                _annotationSources.Value[name] = configurationSource;
                Metadata[name] = value;
            }
            else
            {
                _annotationSources.Value.Remove(name);
                Metadata.RemoveAnnotation(name);
            }

            return true;
        }

        protected virtual void MergeAnnotationsFrom([NotNull] InternalMetadataBuilder annotatableBuilder)
        {
            foreach (var annotation in annotatableBuilder.Metadata.GetAnnotations())
            {
                ConfigurationSource annotationSource;
                if (!annotatableBuilder._annotationSources.Value.TryGetValue(annotation.Name, out annotationSource))
                {
                    annotationSource = ConfigurationSource.Explicit;
                }

                HasAnnotation(
                    annotation.Name,
                    annotation.Value,
                    annotationSource,
                    canOverrideSameSource: false);
            }
        }
    }
}

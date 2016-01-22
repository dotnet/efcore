// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
                    || ((configurationSource == existingConfigurationSource) && !canOverrideSameSource))
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
    }
}

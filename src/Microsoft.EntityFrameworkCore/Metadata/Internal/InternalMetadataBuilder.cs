// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
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

                if (!CanSetAnnotationValue(existingAnnotation, value, configurationSource, canOverrideSameSource))
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

        public virtual bool CanSetAnnotation([NotNull] string name, [CanBeNull] object value, ConfigurationSource configurationSource)
        {
            var existingAnnotation = Metadata.FindAnnotation(name);
            if (existingAnnotation != null)
            {
                return CanSetAnnotationValue(existingAnnotation, value, configurationSource, canOverrideSameSource: true);
            }

            return true;
        }

        private bool CanSetAnnotationValue(
            ConventionalAnnotation annotation, object value, ConfigurationSource configurationSource, bool canOverrideSameSource)
        {
            if (annotation.Value.Equals(value))
            {
                return true;
            }

            var existingConfigurationSource = annotation.GetConfigurationSource();
            if (!configurationSource.Overrides(existingConfigurationSource)
                || ((configurationSource == existingConfigurationSource) && !canOverrideSameSource))
            {
                return false;
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

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public abstract class InternalMetadataBuilder
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected InternalMetadataBuilder([NotNull] ConventionalAnnotatable metadata)
        {
            Metadata = metadata;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConventionalAnnotatable Metadata { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public abstract InternalModelBuilder ModelBuilder { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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

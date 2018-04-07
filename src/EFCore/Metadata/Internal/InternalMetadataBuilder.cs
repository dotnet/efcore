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
        public virtual ConventionalAnnotatable Metadata { [DebuggerStepThrough] get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public abstract InternalModelBuilder ModelBuilder { [DebuggerStepThrough] get; }

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
                if (Equals(existingAnnotation.Value, value))
                {
                    existingAnnotation.UpdateConfigurationSource(configurationSource);
                    return true;
                }

                if (!CanSetAnnotationValue(existingAnnotation, value, configurationSource, canOverrideSameSource))
                {
                    return false;
                }

                Metadata.SetAnnotation(name, value, configurationSource);

                return true;
            }

            Metadata.AddAnnotation(name, value, configurationSource);

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

        private static bool CanSetAnnotationValue(
            ConventionalAnnotation annotation, object value, ConfigurationSource configurationSource, bool canOverrideSameSource)
        {
            if (Equals(annotation.Value, value))
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
        public virtual bool RemoveAnnotation([NotNull] string name, ConfigurationSource configurationSource)
        {
            if (!CanSetAnnotation(name, null, configurationSource))
            {
                return false;
            }

            Metadata.RemoveAnnotation(name);
            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void MergeAnnotationsFrom([NotNull] ConventionalAnnotatable annotatable)
            => MergeAnnotationsFrom(annotatable, ConfigurationSource.Explicit);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void MergeAnnotationsFrom(
            [NotNull] ConventionalAnnotatable annotatable,
            ConfigurationSource minimalConfigurationSource)
        {
            foreach (var annotation in annotatable.GetAnnotations())
            {
                var configurationSource = annotation.GetConfigurationSource();
                if (configurationSource.Overrides(minimalConfigurationSource))
                {
                    HasAnnotation(
                        annotation.Name,
                        annotation.Value,
                        configurationSource,
                        canOverrideSameSource: false);
                }
            }
        }
    }
}

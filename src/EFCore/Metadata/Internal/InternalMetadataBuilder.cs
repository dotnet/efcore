// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public abstract class InternalMetadataBuilder
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected InternalMetadataBuilder([NotNull] ConventionAnnotatable metadata)
        {
            Metadata = metadata;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConventionAnnotatable Metadata { [DebuggerStepThrough] get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public abstract InternalModelBuilder ModelBuilder { [DebuggerStepThrough] get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanSetAnnotation([NotNull] string name, [CanBeNull] object value, ConfigurationSource configurationSource)
        {
            var existingAnnotation = Metadata.FindAnnotation(name);
            return existingAnnotation != null
                ? CanSetAnnotationValue(existingAnnotation, value, configurationSource, canOverrideSameSource: true)
                : true;
        }

        private static bool CanSetAnnotationValue(
            ConventionAnnotation annotation, object value, ConfigurationSource configurationSource, bool canOverrideSameSource)
        {
            if (Equals(annotation.Value, value))
            {
                return true;
            }

            var existingConfigurationSource = annotation.GetConfigurationSource();
            return !configurationSource.Overrides(existingConfigurationSource)
                   || ((configurationSource == existingConfigurationSource) && !canOverrideSameSource)
                ? false
                : true;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void MergeAnnotationsFrom([NotNull] ConventionAnnotatable annotatable)
            => MergeAnnotationsFrom(annotatable, ConfigurationSource.Explicit);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void MergeAnnotationsFrom(
            [NotNull] ConventionAnnotatable annotatable,
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

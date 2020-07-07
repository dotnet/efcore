// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class InternalNavigationBuilder : InternalPropertyBaseBuilder<Navigation>, IConventionNavigationBuilder
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public InternalNavigationBuilder([NotNull] Navigation metadata, [NotNull] InternalModelBuilder modelBuilder)
            : base(metadata, modelBuilder)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public new virtual InternalNavigationBuilder HasField([CanBeNull] string fieldName, ConfigurationSource configurationSource)
            => (InternalNavigationBuilder)base.HasField(fieldName, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public new virtual InternalNavigationBuilder UsePropertyAccessMode(
            PropertyAccessMode? propertyAccessMode, ConfigurationSource configurationSource)
            => (InternalNavigationBuilder)base.UsePropertyAccessMode(propertyAccessMode, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanSetIsEagerLoaded(bool? eagerLoaded, ConfigurationSource configurationSource)
        {
            IConventionNavigation conventionNavigation = Metadata;

            return configurationSource.Overrides(conventionNavigation.GetIsEagerLoadedConfigurationSource())
                || conventionNavigation.IsEagerLoaded == eagerLoaded;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalNavigationBuilder IsEagerLoaded(bool? eagerLoaded, ConfigurationSource configurationSource)
        {
            if (CanSetIsEagerLoaded(eagerLoaded, configurationSource))
            {
                Metadata.SetIsEagerLoaded(eagerLoaded, configurationSource);

                return this;
            }

            return null;
        }

        IConventionNavigation IConventionNavigationBuilder.Metadata
        {
            [DebuggerStepThrough]
            get => Metadata;
        }

        /// <inheritdoc />
        [DebuggerStepThrough]
        bool IConventionNavigationBuilder.CanSetPropertyAccessMode(PropertyAccessMode? propertyAccessMode, bool fromDataAnnotation)
            => CanSetPropertyAccessMode(
                propertyAccessMode, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        IConventionNavigationBuilder IConventionNavigationBuilder.UsePropertyAccessMode(
            PropertyAccessMode? propertyAccessMode, bool fromDataAnnotation)
            => UsePropertyAccessMode(
                propertyAccessMode, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        bool IConventionNavigationBuilder.CanSetField(string fieldName, bool fromDataAnnotation)
            => CanSetField(
                fieldName,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        IConventionNavigationBuilder IConventionNavigationBuilder.HasField(string fieldName, bool fromDataAnnotation)
            => HasField(
                fieldName,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        bool IConventionNavigationBuilder.CanSetIsEagerLoaded(bool? eagerLoaded, bool fromDataAnnotation)
            => CanSetIsEagerLoaded(eagerLoaded, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        IConventionNavigationBuilder IConventionNavigationBuilder.IsEagerLoaded(bool? eagerLoaded, bool fromDataAnnotation)
            => IsEagerLoaded(eagerLoaded, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
    }
}

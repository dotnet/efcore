// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
        public new virtual InternalNavigationBuilder HasField([CanBeNull] FieldInfo fieldInfo, ConfigurationSource configurationSource)
            => (InternalNavigationBuilder)base.HasField(fieldInfo, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public new virtual InternalNavigationBuilder UsePropertyAccessMode(
            PropertyAccessMode? propertyAccessMode,
            ConfigurationSource configurationSource)
            => (InternalNavigationBuilder)base.UsePropertyAccessMode(propertyAccessMode, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanSetAutoInclude(bool? autoInclude, ConfigurationSource configurationSource)
        {
            IConventionNavigation conventionNavigation = Metadata;

            return configurationSource.Overrides(conventionNavigation.GetIsEagerLoadedConfigurationSource())
                || conventionNavigation.IsEagerLoaded == autoInclude;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalNavigationBuilder AutoInclude(bool? autoInclude, ConfigurationSource configurationSource)
        {
            if (CanSetAutoInclude(autoInclude, configurationSource))
            {
                Metadata.SetIsEagerLoaded(autoInclude, configurationSource);

                return this;
            }

            return null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanSetIsRequired(bool? required, ConfigurationSource configurationSource)
        {
            var foreignKey = Metadata.ForeignKey;
            return foreignKey.IsUnique
                ? foreignKey.GetPrincipalEndConfigurationSource() == null
                    ? false
                    : Metadata.IsOnDependent
                        ? foreignKey.Builder.CanSetIsRequired(required, configurationSource)
                        : foreignKey.Builder.CanSetIsRequiredDependent(required, configurationSource)
                : Metadata.IsOnDependent
                    ? foreignKey.Builder.CanSetIsRequired(required, configurationSource)
                    : false;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalNavigationBuilder IsRequired(bool? required, ConfigurationSource configurationSource)
        {
            if (configurationSource == ConfigurationSource.Explicit
                || CanSetIsRequired(required, configurationSource))
            {
                var foreignKey = Metadata.ForeignKey;
                if (foreignKey.IsUnique)
                {
                    if (foreignKey.GetPrincipalEndConfigurationSource() == null)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.AmbiguousEndRequiredDependentNavigation(
                                Metadata.DeclaringEntityType.DisplayName(),
                                Metadata.Name,
                                foreignKey.Properties.Format()));
                    }

                    return Metadata.IsOnDependent
                        ? foreignKey.Builder.IsRequired(required, configurationSource)
                            .Metadata.DependentToPrincipal.Builder
                        : foreignKey.Builder.IsRequiredDependent(required, configurationSource)
                            .Metadata.PrincipalToDependent.Builder;
                }

                if (Metadata.IsOnDependent)
                {
                    return foreignKey.Builder.IsRequired(required, configurationSource)
                        .Metadata.DependentToPrincipal.Builder;
                }

                throw new InvalidOperationException(
                    CoreStrings.NonUniqueRequiredDependentNavigation(
                        foreignKey.PrincipalEntityType.DisplayName(), Metadata.Name));
            }

            return null;
        }

        IConventionPropertyBase IConventionPropertyBaseBuilder.Metadata
        {
            [DebuggerStepThrough]
            get => Metadata;
        }

        IConventionNavigation IConventionNavigationBuilder.Metadata
        {
            [DebuggerStepThrough]
            get => Metadata;
        }

        /// <inheritdoc />
        [DebuggerStepThrough]
        bool IConventionPropertyBaseBuilder.CanSetPropertyAccessMode(PropertyAccessMode? propertyAccessMode, bool fromDataAnnotation)
            => CanSetPropertyAccessMode(
                propertyAccessMode, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        IConventionPropertyBaseBuilder IConventionPropertyBaseBuilder.UsePropertyAccessMode(
            PropertyAccessMode? propertyAccessMode,
            bool fromDataAnnotation)
            => UsePropertyAccessMode(
                propertyAccessMode, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        IConventionNavigationBuilder IConventionNavigationBuilder.UsePropertyAccessMode(
            PropertyAccessMode? propertyAccessMode,
            bool fromDataAnnotation)
            => UsePropertyAccessMode(
                propertyAccessMode, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        bool IConventionPropertyBaseBuilder.CanSetField(string fieldName, bool fromDataAnnotation)
            => CanSetField(
                fieldName,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        IConventionPropertyBaseBuilder IConventionPropertyBaseBuilder.HasField(string fieldName, bool fromDataAnnotation)
            => HasField(
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
        bool IConventionPropertyBaseBuilder.CanSetField(FieldInfo fieldInfo, bool fromDataAnnotation)
            => CanSetField(
                fieldInfo,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        IConventionPropertyBaseBuilder IConventionPropertyBaseBuilder.HasField(FieldInfo fieldInfo, bool fromDataAnnotation)
            => HasField(
                fieldInfo,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        IConventionNavigationBuilder IConventionNavigationBuilder.HasField(FieldInfo fieldInfo, bool fromDataAnnotation)
            => HasField(
                fieldInfo,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        bool IConventionNavigationBuilder.CanSetAutoInclude(bool? autoInclude, bool fromDataAnnotation)
            => CanSetAutoInclude(autoInclude, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        IConventionNavigationBuilder IConventionNavigationBuilder.AutoInclude(bool? autoInclude, bool fromDataAnnotation)
            => AutoInclude(autoInclude, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        bool IConventionNavigationBuilder.CanSetIsRequired(bool? required, bool fromDataAnnotation)
            => CanSetIsRequired(required, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        IConventionNavigationBuilder IConventionNavigationBuilder.IsRequired(bool? required, bool fromDataAnnotation)
            => IsRequired(required, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
    }
}

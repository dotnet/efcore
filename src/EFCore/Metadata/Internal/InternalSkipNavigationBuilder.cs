// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
    public class InternalSkipNavigationBuilder : InternalPropertyBaseBuilder<SkipNavigation>, IConventionSkipNavigationBuilder
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public InternalSkipNavigationBuilder([NotNull] SkipNavigation metadata, [NotNull] InternalModelBuilder modelBuilder)
            : base(metadata, modelBuilder)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public new virtual InternalSkipNavigationBuilder HasField([CanBeNull] string fieldName, ConfigurationSource configurationSource)
            => (InternalSkipNavigationBuilder)base.HasField(fieldName, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public new virtual InternalSkipNavigationBuilder HasField([CanBeNull] FieldInfo fieldInfo, ConfigurationSource configurationSource)
            => (InternalSkipNavigationBuilder)base.HasField(fieldInfo, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public new virtual InternalSkipNavigationBuilder UsePropertyAccessMode(
            PropertyAccessMode? propertyAccessMode,
            ConfigurationSource configurationSource)
            => (InternalSkipNavigationBuilder)base.UsePropertyAccessMode(propertyAccessMode, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalSkipNavigationBuilder HasForeignKey(
            [CanBeNull] ForeignKey foreignKey,
            ConfigurationSource configurationSource)
        {
            if (!CanSetForeignKey(foreignKey, configurationSource))
            {
                return null;
            }

            if (foreignKey != null)
            {
                foreignKey.UpdateConfigurationSource(configurationSource);

                if (Metadata.Inverse?.JoinEntityType != null
                    && Metadata.Inverse.JoinEntityType
                    != (Metadata.IsOnDependent ? foreignKey.PrincipalEntityType : foreignKey.DeclaringEntityType))
                {
                    Metadata.Inverse.Builder.HasForeignKey(null, configurationSource);
                }
            }

            var oldForeignKey = Metadata.ForeignKey;

            Metadata.SetForeignKey(foreignKey, configurationSource);

            if (oldForeignKey?.Builder != null
                && oldForeignKey != foreignKey
                && oldForeignKey.ReferencingSkipNavigations?.Any() != true)
            {
                oldForeignKey.DeclaringEntityType.Builder.HasNoRelationship(oldForeignKey, ConfigurationSource.Convention);
            }

            return this;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanSetForeignKey([CanBeNull] ForeignKey foreignKey, ConfigurationSource? configurationSource)
        {
            if (!configurationSource.Overrides(Metadata.GetForeignKeyConfigurationSource()))
            {
                return Equals(Metadata.ForeignKey, foreignKey);
            }

            if (foreignKey == null)
            {
                return true;
            }

            if (Metadata.DeclaringEntityType
                != (Metadata.IsOnDependent ? foreignKey.DeclaringEntityType : foreignKey.PrincipalEntityType))
            {
                return false;
            }

            if (Metadata.Inverse?.JoinEntityType == null)
            {
                return true;
            }

            return Metadata.Inverse.JoinEntityType
                == (Metadata.IsOnDependent ? foreignKey.PrincipalEntityType : foreignKey.DeclaringEntityType)
                || Metadata.Inverse.Builder.CanSetForeignKey(null, configurationSource);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalSkipNavigationBuilder HasInverse(
            [CanBeNull] SkipNavigation inverse,
            ConfigurationSource configurationSource)
        {
            if (!CanSetInverse(inverse, configurationSource))
            {
                return null;
            }

            if (inverse != null)
            {
                inverse.UpdateConfigurationSource(configurationSource);
            }

            using (var batch = Metadata.DeclaringEntityType.Model.ConventionDispatcher.DelayConventions())
            {
                if (Metadata.Inverse != null
                    && Metadata.Inverse != inverse)
                {
                    Metadata.Inverse.SetInverse(null, configurationSource);
                }

                Metadata.SetInverse(inverse, configurationSource);

                if (inverse != null)
                {
                    inverse.SetInverse(Metadata, configurationSource);
                }
            }

            return this;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanSetInverse(
            [CanBeNull] SkipNavigation inverse,
            ConfigurationSource? configurationSource)
        {
            if (!configurationSource.Overrides(Metadata.GetInverseConfigurationSource())
                || (inverse != null
                    && !configurationSource.Overrides(inverse.GetInverseConfigurationSource())))
            {
                return Equals(Metadata.Inverse, inverse);
            }

            if (inverse == null)
            {
                return true;
            }

            return Metadata.TargetEntityType == inverse.DeclaringEntityType
                && Metadata.DeclaringEntityType == inverse.TargetEntityType
                && (Metadata.JoinEntityType == null
                    || inverse.JoinEntityType == null
                    || Metadata.JoinEntityType == inverse.JoinEntityType);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalSkipNavigationBuilder Attach(
            [CanBeNull] InternalEntityTypeBuilder entityTypeBuilder = null,
            [CanBeNull] EntityType targetEntityType = null,
            [CanBeNull] InternalSkipNavigationBuilder inverseBuilder = null)
        {
            entityTypeBuilder ??= Metadata.DeclaringEntityType.Builder;
            if (entityTypeBuilder == null)
            {
                entityTypeBuilder = Metadata.DeclaringEntityType.Model.FindEntityType(Metadata.DeclaringEntityType.Name)?.Builder;
                if (entityTypeBuilder == null)
                {
                    return null;
                }
            }

            targetEntityType ??= Metadata.TargetEntityType;
            if (targetEntityType.Builder == null)
            {
                targetEntityType = Metadata.DeclaringEntityType.Model.FindEntityType(targetEntityType.Name);
                if (targetEntityType == null)
                {
                    return null;
                }
            }

            var newSkipNavigationBuilder = entityTypeBuilder.HasSkipNavigation(
                Metadata.CreateMemberIdentity(),
                targetEntityType,
                Metadata.GetConfigurationSource(),
                Metadata.IsCollection,
                Metadata.IsOnDependent);
            if (newSkipNavigationBuilder == null)
            {
                return null;
            }

            newSkipNavigationBuilder.MergeAnnotationsFrom(Metadata);

            var foreignKeyConfigurationSource = Metadata.GetForeignKeyConfigurationSource();
            if (foreignKeyConfigurationSource.HasValue)
            {
                var foreignKey = Metadata.ForeignKey;
                if (foreignKey.Builder == null)
                {
                    foreignKey = InternalForeignKeyBuilder.FindCurrentForeignKeyBuilder(
                        foreignKey.PrincipalEntityType,
                        foreignKey.DeclaringEntityType,
                        foreignKey.DependentToPrincipal?.CreateMemberIdentity(),
                        foreignKey.PrincipalToDependent?.CreateMemberIdentity(),
                        dependentProperties: foreignKey.Properties,
                        principalProperties: foreignKey.PrincipalKey.Properties)?.Metadata;
                }

                if (foreignKey != null)
                {
                    newSkipNavigationBuilder.HasForeignKey(foreignKey, foreignKeyConfigurationSource.Value);
                }
            }

            var inverseConfigurationSource = Metadata.GetInverseConfigurationSource();
            if (inverseConfigurationSource.HasValue)
            {
                var inverse = Metadata.Inverse;
                if (inverse.Builder == null)
                {
                    inverse = inverse.DeclaringEntityType.FindSkipNavigation(inverse.Name);
                }

                if (inverseBuilder != null)
                {
                    inverse = inverseBuilder.Attach(targetEntityType.Builder, entityTypeBuilder.Metadata)?.Metadata
                        ?? inverse;
                }

                if (inverse != null)
                {
                    newSkipNavigationBuilder.HasInverse(inverse, inverseConfigurationSource.Value);
                }
            }

            var propertyAccessModeConfigurationSource = Metadata.GetPropertyAccessModeConfigurationSource();
            if (propertyAccessModeConfigurationSource.HasValue)
            {
                newSkipNavigationBuilder.UsePropertyAccessMode(
                    ((ISkipNavigation)Metadata).GetPropertyAccessMode(), propertyAccessModeConfigurationSource.Value);
            }

            var oldFieldInfoConfigurationSource = Metadata.GetFieldInfoConfigurationSource();
            if (oldFieldInfoConfigurationSource.HasValue
                && newSkipNavigationBuilder.CanSetField(Metadata.FieldInfo, oldFieldInfoConfigurationSource))
            {
                newSkipNavigationBuilder.HasField(Metadata.FieldInfo, oldFieldInfoConfigurationSource.Value);
            }

            return newSkipNavigationBuilder;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanSetAutoInclude(bool? autoInclude, ConfigurationSource configurationSource)
        {
            IConventionSkipNavigation conventionNavigation = Metadata;

            return configurationSource.Overrides(conventionNavigation.GetIsEagerLoadedConfigurationSource())
                || conventionNavigation.IsEagerLoaded == autoInclude;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalSkipNavigationBuilder AutoInclude(bool? autoInclude, ConfigurationSource configurationSource)
        {
            if (CanSetAutoInclude(autoInclude, configurationSource))
            {
                Metadata.SetIsEagerLoaded(autoInclude, configurationSource);

                return this;
            }

            return null;
        }

        IConventionPropertyBase IConventionPropertyBaseBuilder.Metadata
        {
            [DebuggerStepThrough] get => Metadata;
        }

        IConventionSkipNavigation IConventionSkipNavigationBuilder.Metadata
        {
            [DebuggerStepThrough]
            get => Metadata;
        }

        /// <inheritdoc />
        [DebuggerStepThrough]
        IConventionPropertyBaseBuilder IConventionPropertyBaseBuilder.HasField(string fieldName, bool fromDataAnnotation)
            => HasField(
                fieldName,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        IConventionPropertyBaseBuilder IConventionPropertyBaseBuilder.HasField(FieldInfo fieldInfo, bool fromDataAnnotation)
            => HasField(
                fieldInfo,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        IConventionSkipNavigationBuilder IConventionSkipNavigationBuilder.HasField(string fieldName, bool fromDataAnnotation)
            => HasField(
                fieldName,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        IConventionSkipNavigationBuilder IConventionSkipNavigationBuilder.HasField(FieldInfo fieldInfo, bool fromDataAnnotation)
            => HasField(
                fieldInfo,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        bool IConventionPropertyBaseBuilder.CanSetField(string fieldName, bool fromDataAnnotation)
            => CanSetField(
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
        IConventionPropertyBaseBuilder IConventionPropertyBaseBuilder.UsePropertyAccessMode(
            PropertyAccessMode? propertyAccessMode,
            bool fromDataAnnotation)
            => UsePropertyAccessMode(
                propertyAccessMode,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        IConventionSkipNavigationBuilder IConventionSkipNavigationBuilder.UsePropertyAccessMode(
            PropertyAccessMode? propertyAccessMode,
            bool fromDataAnnotation)
            => UsePropertyAccessMode(
                propertyAccessMode,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        bool IConventionPropertyBaseBuilder.CanSetPropertyAccessMode(
            PropertyAccessMode? propertyAccessMode,
            bool fromDataAnnotation)
            => CanSetPropertyAccessMode(
                propertyAccessMode,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        IConventionSkipNavigationBuilder IConventionSkipNavigationBuilder.HasForeignKey(
            IConventionForeignKey foreignKey,
            bool fromDataAnnotation)
            => HasForeignKey(
                (ForeignKey)foreignKey,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        bool IConventionSkipNavigationBuilder.CanSetForeignKey(
            IConventionForeignKey foreignKey,
            bool fromDataAnnotation)
            => CanSetForeignKey(
                (ForeignKey)foreignKey,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        IConventionSkipNavigationBuilder IConventionSkipNavigationBuilder.HasInverse(
            IConventionSkipNavigation inverse,
            bool fromDataAnnotation)
            => HasInverse(
                (SkipNavigation)inverse,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        bool IConventionSkipNavigationBuilder.CanSetInverse(
            IConventionSkipNavigation inverse,
            bool fromDataAnnotation)
            => CanSetInverse(
                (SkipNavigation)inverse,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        bool IConventionSkipNavigationBuilder.CanSetAutoInclude(bool? autoInclude, bool fromDataAnnotation)
            => CanSetAutoInclude(autoInclude, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        IConventionSkipNavigationBuilder IConventionSkipNavigationBuilder.AutoInclude(bool? autoInclude, bool fromDataAnnotation)
            => AutoInclude(autoInclude, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
    }
}

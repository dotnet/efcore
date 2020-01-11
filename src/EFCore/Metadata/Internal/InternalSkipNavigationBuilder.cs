// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
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
    public class InternalSkipNavigationBuilder : InternalModelItemBuilder<SkipNavigation>, IConventionSkipNavigationBuilder
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
        public virtual InternalSkipNavigationBuilder HasField([CanBeNull] string fieldName, ConfigurationSource configurationSource)
        {
            if (Metadata.FieldInfo?.GetSimpleMemberName() == fieldName
                || configurationSource.Overrides(Metadata.GetFieldInfoConfigurationSource()))
            {
                Metadata.SetField(fieldName, configurationSource);

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
        public virtual bool CanSetField([CanBeNull] string fieldName, ConfigurationSource? configurationSource)
        {
            if (configurationSource.Overrides(Metadata.GetFieldInfoConfigurationSource()))
            {
                if (fieldName == null)
                {
                    return true;
                }

                var fieldInfo = PropertyBase.GetFieldInfo(
                    fieldName, Metadata.DeclaringType, Metadata.Name,
                    shouldThrow: false);
                return fieldInfo != null
                    && PropertyBase.IsCompatible(
                        fieldInfo, Metadata.ClrType, Metadata.DeclaringType.ClrType, Metadata.Name,
                        shouldThrow: false);
            }

            return Metadata.FieldInfo?.GetSimpleMemberName() == fieldName;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalSkipNavigationBuilder HasField([CanBeNull] FieldInfo fieldInfo, ConfigurationSource configurationSource)
        {
            if (configurationSource.Overrides(Metadata.GetFieldInfoConfigurationSource())
                || Equals(Metadata.FieldInfo, fieldInfo))
            {
                Metadata.SetField(fieldInfo, configurationSource);
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
        public virtual bool CanSetField([CanBeNull] FieldInfo fieldInfo, ConfigurationSource? configurationSource)
            => (configurationSource.Overrides(Metadata.GetFieldInfoConfigurationSource())
                    && (fieldInfo == null
                        || PropertyBase.IsCompatible(
                            fieldInfo, Metadata.ClrType, Metadata.DeclaringType.ClrType, Metadata.Name,
                            shouldThrow: false)))
                || Equals(Metadata.FieldInfo, fieldInfo);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalSkipNavigationBuilder UsePropertyAccessMode(
            PropertyAccessMode? propertyAccessMode, ConfigurationSource configurationSource)
        {
            if (CanSetPropertyAccessMode(propertyAccessMode, configurationSource))
            {
                Metadata.SetPropertyAccessMode(propertyAccessMode, configurationSource);

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
        public virtual bool CanSetPropertyAccessMode(
            PropertyAccessMode? propertyAccessMode, ConfigurationSource? configurationSource)
            => configurationSource.Overrides(Metadata.GetPropertyAccessModeConfigurationSource())
                || ((ISkipNavigation)Metadata).GetPropertyAccessMode() == propertyAccessMode;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalSkipNavigationBuilder HasForeignKey([CanBeNull] ForeignKey foreignKey, ConfigurationSource configurationSource)
        {
            if (configurationSource.Overrides(Metadata.GetForeignKeyConfigurationSource())
                || Equals(Metadata.ForeignKey, foreignKey))
            {
                if (foreignKey != null)
                {
                    foreignKey.UpdateConfigurationSource(configurationSource);
                }

                Metadata.SetForeignKey(foreignKey, configurationSource);
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

            return (Metadata.DeclaringEntityType
                    == (Metadata.IsOnDependent ? foreignKey.DeclaringEntityType : foreignKey.PrincipalEntityType))
                            && (Metadata.Inverse?.AssociationEntityType == null
                                || Metadata.Inverse.AssociationEntityType
                                == (Metadata.IsOnDependent ? foreignKey.PrincipalEntityType : foreignKey.DeclaringEntityType));
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalSkipNavigationBuilder HasInverse(
            [CanBeNull] SkipNavigation inverse, ConfigurationSource configurationSource)
        {
            if (!Equals(Metadata.Inverse, inverse)
                && (!configurationSource.Overrides(Metadata.GetInverseConfigurationSource())
                    || inverse != null
                        && !configurationSource.Overrides(inverse.GetInverseConfigurationSource())))
            {
                return null;
            }

            if (inverse != null)
            {
                inverse.UpdateConfigurationSource(configurationSource);
            }

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

            return this;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanSetInverse(
            [CanBeNull] SkipNavigation inverse, ConfigurationSource? configurationSource)
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
                    && (Metadata.AssociationEntityType == null
                        || inverse.AssociationEntityType == null
                        || Metadata.AssociationEntityType == inverse.AssociationEntityType);
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
                    foreignKey = InternalRelationshipBuilder.FindCurrentRelationshipBuilder(
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

        IConventionSkipNavigation IConventionSkipNavigationBuilder.Metadata
        {
            [DebuggerStepThrough] get => Metadata;
        }

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
        bool IConventionSkipNavigationBuilder.CanSetField(string fieldName, bool fromDataAnnotation)
            => CanSetField(
                fieldName,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        bool IConventionSkipNavigationBuilder.CanSetField(FieldInfo fieldInfo, bool fromDataAnnotation)
            => CanSetField(
                fieldInfo,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        IConventionSkipNavigationBuilder IConventionSkipNavigationBuilder.UsePropertyAccessMode(
            PropertyAccessMode? propertyAccessMode, bool fromDataAnnotation)
            => UsePropertyAccessMode(
                propertyAccessMode,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        bool IConventionSkipNavigationBuilder.CanSetPropertyAccessMode(
            PropertyAccessMode? propertyAccessMode, bool fromDataAnnotation)
            => CanSetPropertyAccessMode(
                propertyAccessMode,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        IConventionSkipNavigationBuilder IConventionSkipNavigationBuilder.HasForeignKey(
            IConventionForeignKey foreignKey, bool fromDataAnnotation)
            => HasForeignKey(
                (ForeignKey)foreignKey,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        bool IConventionSkipNavigationBuilder.CanSetForeignKey(
            IConventionForeignKey foreignKey, bool fromDataAnnotation)
            => CanSetForeignKey(
                (ForeignKey)foreignKey,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        IConventionSkipNavigationBuilder IConventionSkipNavigationBuilder.HasInverse(
            IConventionSkipNavigation inverse, bool fromDataAnnotation)
            => HasInverse(
                (SkipNavigation)inverse,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        bool IConventionSkipNavigationBuilder.CanSetInverse(
            IConventionSkipNavigation inverse, bool fromDataAnnotation)
            => CanSetInverse(
                (SkipNavigation)inverse,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
    }
}

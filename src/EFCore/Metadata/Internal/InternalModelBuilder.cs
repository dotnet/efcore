// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class InternalModelBuilder : AnnotatableBuilder<Model, InternalModelBuilder>, IConventionModelBuilder
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public InternalModelBuilder([NotNull] Model metadata)
            : base(metadata, null)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override InternalModelBuilder ModelBuilder
            => this;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalEntityTypeBuilder Entity(
            [NotNull] string name,
            ConfigurationSource configurationSource,
            bool? shouldBeOwned = false)
            => Entity(new TypeIdentity(name), configurationSource, shouldBeOwned);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalEntityTypeBuilder SharedTypeEntity(
            [NotNull] string name,
            [NotNull] Type type,
            ConfigurationSource configurationSource,
            bool? shouldBeOwned = false)
            => Entity(new TypeIdentity(name, Check.NotNull(type, nameof(type))), configurationSource, shouldBeOwned);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalEntityTypeBuilder Entity(
            [NotNull] Type type,
            ConfigurationSource configurationSource,
            bool? shouldBeOwned = false)
            => Entity(new TypeIdentity(type, Metadata), configurationSource, shouldBeOwned);

        private InternalEntityTypeBuilder Entity(
            in TypeIdentity type,
            ConfigurationSource configurationSource,
            bool? shouldBeOwned)
        {
            if (IsIgnored(type, configurationSource))
            {
                return null;
            }

            using var batch = Metadata.ConventionDispatcher.DelayConventions();
            var clrType = type.Type;
            EntityType entityType;
            if (type.IsNamed)
            {
                if (type.Type != null)
                {
                    var nonSharedTypes = Metadata.GetEntityTypes(Metadata.GetDisplayName(type.Type));
                    foreach (var nonSharedType in nonSharedTypes)
                    {
                        if (configurationSource.OverridesStrictly(nonSharedType.GetConfigurationSource()))
                        {
                            continue;
                        }

                        return configurationSource == ConfigurationSource.Explicit
                            ? throw new InvalidOperationException(CoreStrings.ClashingNonSharedType(type.Name, type.Type.DisplayName()))
                            : (InternalEntityTypeBuilder)null;
                    }

                    foreach (var nonSharedType in nonSharedTypes)
                    {
                        HasNoEntityType(nonSharedType, configurationSource);
                    }
                }

                entityType = Metadata.FindEntityType(type.Name);
            }
            else
            {
                if (Metadata.IsShared(clrType))
                {
                    return configurationSource == ConfigurationSource.Explicit
                        ? throw new InvalidOperationException(CoreStrings.ClashingSharedType(clrType.DisplayName()))
                        : (InternalEntityTypeBuilder)null;
                }

                entityType = Metadata.FindEntityType(clrType);
            }

            if (shouldBeOwned == false
                && (ShouldBeOwnedType(type) // Marked in model as owned
                    || entityType != null && entityType.IsOwned())) // Created using Owns* API
            {
                // We always throw as configuring a type as owned is always comes from user (through Explicit/DataAnnotation)
                throw new InvalidOperationException(
                    CoreStrings.ClashingOwnedEntityType(
                        clrType == null ? type.Name : clrType.ShortDisplayName()));
            }

            if (shouldBeOwned == true
                && entityType != null)
            {
                if (!entityType.IsOwned()
                    && configurationSource == ConfigurationSource.Explicit
                    && entityType.GetConfigurationSource() == ConfigurationSource.Explicit)
                {
                    throw new InvalidOperationException(CoreStrings.ClashingNonOwnedEntityType(entityType.DisplayName()));
                }

                foreach (var derivedType in entityType.GetDerivedTypes())
                {
                    if (!derivedType.IsOwned()
                        && configurationSource == ConfigurationSource.Explicit
                        && derivedType.GetConfigurationSource() == ConfigurationSource.Explicit)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.ClashingNonOwnedDerivedEntityType(entityType.DisplayName(), derivedType.DisplayName()));
                    }
                }
            }

            if (entityType != null)
            {
                if (type.Type == null
                    || entityType.ClrType == type.Type)
                {
                    entityType.UpdateConfigurationSource(configurationSource);
                    return entityType.Builder;
                }

                if (configurationSource.OverridesStrictly(entityType.GetConfigurationSource()))
                {
                    HasNoEntityType(entityType, configurationSource);
                }
                else
                {
                    return configurationSource == ConfigurationSource.Explicit
                        ? throw new InvalidOperationException(
                            CoreStrings.ClashingMismatchedSharedType(type.Name, entityType.ClrType?.ShortDisplayName()))
                        : (InternalEntityTypeBuilder)null;
                }
            }

            Metadata.RemoveIgnored(type.Name);
            entityType = type.IsNamed
                ? type.Type == null
                    ? Metadata.AddEntityType(type.Name, configurationSource)
                    : Metadata.AddEntityType(type.Name, type.Type, configurationSource)
                : Metadata.AddEntityType(clrType, configurationSource);

            return entityType?.Builder;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalEntityTypeBuilder Entity(
            [NotNull] string name,
            [NotNull] string definingNavigationName,
            [NotNull] EntityType definingEntityType,
            ConfigurationSource configurationSource)
            => Entity(new TypeIdentity(name), definingNavigationName, definingEntityType, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalEntityTypeBuilder Entity(
            [NotNull] Type type,
            [NotNull] string definingNavigationName,
            [NotNull] EntityType definingEntityType,
            ConfigurationSource configurationSource)
            => Entity(new TypeIdentity(type, Metadata), definingNavigationName, definingEntityType, configurationSource);

        private InternalEntityTypeBuilder Entity(
            in TypeIdentity type,
            string definingNavigationName,
            EntityType definingEntityType,
            ConfigurationSource configurationSource)
        {
            if (IsIgnored(type, configurationSource))
            {
                return null;
            }

            var clrType = type.Type
                ?? Metadata.FindClrType(type.Name);

            var entityTypeWithDefiningNavigation = clrType == null
                ? Metadata.FindEntityType(type.Name, definingNavigationName, definingEntityType)
                : Metadata.FindEntityType(clrType, definingNavigationName, definingEntityType);
            if (entityTypeWithDefiningNavigation == null)
            {
                var entityType = clrType == null
                    ? Metadata.FindEntityType(type.Name)
                    : Metadata.FindEntityType(clrType);

                IConventionBatch batch = null;
                EntityType.Snapshot entityTypeSnapshot = null;
                if (entityType != null)
                {
                    if (!configurationSource.Overrides(entityType.GetConfigurationSource()))
                    {
                        return null;
                    }

                    batch = ModelBuilder.Metadata.ConventionDispatcher.DelayConventions();
                    entityTypeSnapshot = InternalEntityTypeBuilder.DetachAllMembers(entityType);

                    HasNoEntityType(entityType, configurationSource);
                }

                if (clrType == null)
                {
                    Metadata.RemoveIgnored(type.Name);

                    entityTypeWithDefiningNavigation = Metadata.AddEntityType(
                        type.Name, definingNavigationName, definingEntityType, configurationSource);
                }
                else
                {
                    Metadata.RemoveIgnored(type.Name);

                    entityTypeWithDefiningNavigation = Metadata.AddEntityType(
                        clrType, definingNavigationName, definingEntityType, configurationSource);
                }

                if (batch != null)
                {
                    entityTypeSnapshot.Attach(entityTypeWithDefiningNavigation.Builder);
                    batch.Dispose();
                }
            }
            else
            {
                entityTypeWithDefiningNavigation.UpdateConfigurationSource(configurationSource);
            }

            return entityTypeWithDefiningNavigation?.Builder;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalModelBuilder RemoveImplicitJoinEntity([NotNull] EntityType joinEntityType)
        {
            Check.NotNull(joinEntityType, nameof(joinEntityType));

            if (joinEntityType.Builder == null)
            {
                return this;
            }

            if (!joinEntityType.IsImplicitlyCreatedJoinEntityType)
            {
                return null;
            }

            return HasNoEntityType(joinEntityType, ConfigurationSource.Convention);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IConventionOwnedEntityTypeBuilder Owned(
            [NotNull] Type type,
            ConfigurationSource configurationSource)
        {
            if (IsIgnored(type, configurationSource))
            {
                return null;
            }

            Metadata.RemoveIgnored(type);
            Metadata.AddOwned(type);

            var entityType = Metadata.FindEntityType(type);
            if (entityType?.GetForeignKeys().Any(fk => fk.IsOwnership) == false)
            {
                if (!configurationSource.Overrides(entityType.GetConfigurationSource()))
                {
                    return null;
                }

                if (entityType.GetConfigurationSource() == ConfigurationSource.Explicit)
                {
                    throw new InvalidOperationException(CoreStrings.ClashingNonOwnedEntityType(entityType.DisplayName()));
                }

                var ownershipCandidate = entityType.GetForeignKeys().FirstOrDefault(
                    fk => fk.PrincipalToDependent != null
                        && !fk.PrincipalEntityType.IsInOwnershipPath(entityType)
                        && !fk.PrincipalEntityType.IsInDefinitionPath(type));
                if (ownershipCandidate != null)
                {
                    if (ownershipCandidate.Builder.IsOwnership(true, configurationSource) == null)
                    {
                        return null;
                    }
                }
                else
                {
                    if (!entityType.Builder.RemoveNonOwnershipRelationships(null, configurationSource))
                    {
                        return null;
                    }
                }
            }

            return new InternalOwnedEntityTypeBuilder();
        }

        private bool ShouldBeOwnedType(in TypeIdentity type)
            => type.Type != null && Metadata.IsOwned(type.Type);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool IsIgnored([NotNull] Type type, ConfigurationSource configurationSource)
            => IsIgnored(new TypeIdentity(type, Metadata), configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool IsIgnored([NotNull] string name, ConfigurationSource configurationSource)
            => IsIgnored(new TypeIdentity(name), configurationSource);

        private bool IsIgnored(in TypeIdentity type, ConfigurationSource configurationSource)
        {
            if (configurationSource == ConfigurationSource.Explicit)
            {
                return false;
            }

            var ignoredConfigurationSource = Metadata.FindIgnoredConfigurationSource(type.Name);
            return ignoredConfigurationSource.HasValue
                && ignoredConfigurationSource.Value.Overrides(configurationSource);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalModelBuilder Ignore([NotNull] Type type, ConfigurationSource configurationSource)
            => Ignore(new TypeIdentity(type, Metadata), configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalModelBuilder Ignore([NotNull] string name, ConfigurationSource configurationSource)
            => Ignore(new TypeIdentity(name), configurationSource);

        private InternalModelBuilder Ignore(in TypeIdentity type, ConfigurationSource configurationSource)
        {
            var name = type.Name;
            var ignoredConfigurationSource = Metadata.FindIgnoredConfigurationSource(name);
            if (ignoredConfigurationSource.HasValue)
            {
                if (configurationSource.Overrides(ignoredConfigurationSource)
                    && configurationSource != ignoredConfigurationSource)
                {
                    Metadata.AddIgnored(name, configurationSource);
                }

                return this;
            }

            if (!CanIgnore(type, configurationSource))
            {
                return null;
            }

            using (Metadata.ConventionDispatcher.DelayConventions())
            {
                var removed = false;
                foreach (var entityType in Metadata.GetEntityTypes(name).ToList())
                {
                    if (entityType.HasClrType())
                    {
                        if (entityType.HasSharedClrType)
                        {
                            Metadata.AddIgnored(entityType.Name, entityType.ClrType, configurationSource);
                        }
                        else
                        {
                            Metadata.AddIgnored(entityType.ClrType, configurationSource);
                        }
                    }
                    else
                    {
                        Metadata.AddIgnored(entityType.Name, configurationSource);
                    }

                    removed = true;
                    HasNoEntityType(entityType, configurationSource);
                }

                if (!removed)
                {
                    if (type.Type == null)
                    {
                        Metadata.AddIgnored(name, configurationSource);
                    }
                    else
                    {
                        Metadata.AddIgnored(type.Type, configurationSource);
                    }
                }

                if (type.Type != null)
                {
                    Metadata.RemoveOwned(type.Type);
                }

                return this;
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanIgnore([NotNull] Type type, ConfigurationSource configurationSource)
            => CanIgnore(new TypeIdentity(type, Metadata), configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanIgnore([NotNull] string name, ConfigurationSource configurationSource)
            => CanIgnore(new TypeIdentity(name), configurationSource);

        private bool CanIgnore(in TypeIdentity type, ConfigurationSource configurationSource)
        {
            var name = type.Name;
            if (Metadata.FindIgnoredConfigurationSource(name).HasValue)
            {
                return true;
            }

            if (ShouldBeOwnedType(type)
                && configurationSource != ConfigurationSource.Explicit)
            {
                return false;
            }

            if (Metadata.GetEntityTypes(name).Any(o => !configurationSource.Overrides(o.GetConfigurationSource())))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalModelBuilder HasNoEntityType([NotNull] EntityType entityType, ConfigurationSource configurationSource)
        {
            var entityTypeConfigurationSource = entityType.GetConfigurationSource();
            if (!configurationSource.Overrides(entityTypeConfigurationSource))
            {
                return null;
            }

            using (Metadata.ConventionDispatcher.DelayConventions())
            {
                var entityTypeBuilder = entityType.Builder;
                foreach (var foreignKey in entityType.GetDeclaredReferencingForeignKeys().ToList())
                {
                    var removed = foreignKey.DeclaringEntityType.Builder.HasNoRelationship(foreignKey, configurationSource);
                    Check.DebugAssert(removed != null, "removed is null");
                }

                foreach (var skipNavigation in entityType.GetDeclaredReferencingSkipNavigations().ToList())
                {
                    var removed = skipNavigation.DeclaringEntityType.Builder.HasNoSkipNavigation(skipNavigation, configurationSource);
                    Check.DebugAssert(removed != null, "removed is null");
                }

                foreach (var skipNavigation in entityType.GetDeclaredForeignKeys().SelectMany(fk => fk.GetReferencingSkipNavigations())
                    .ToList())
                {
                    var removed = skipNavigation.Builder.HasForeignKey(null, configurationSource);
                    Check.DebugAssert(removed != null, "removed is null");
                }

                foreach (var directlyDerivedType in entityType.GetDirectlyDerivedTypes().ToList())
                {
                    var derivedEntityTypeBuilder = directlyDerivedType.Builder
                        .HasBaseType(entityType.BaseType, configurationSource);
                    Check.DebugAssert(derivedEntityTypeBuilder != null, "derivedEntityTypeBuilder is null");
                }

                foreach (var definedType in Metadata.GetEntityTypes().Where(e => e.DefiningEntityType == entityType).ToList())
                {
                    HasNoEntityType(definedType, configurationSource);
                }

                Metadata.RemoveEntityType(entityType);
            }

            return this;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalModelBuilder UseChangeTrackingStrategy(
            ChangeTrackingStrategy? changeTrackingStrategy,
            ConfigurationSource configurationSource)
        {
            if (CanSetChangeTrackingStrategy(changeTrackingStrategy, configurationSource))
            {
                Metadata.SetChangeTrackingStrategy(changeTrackingStrategy, configurationSource);

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
        public virtual bool CanSetChangeTrackingStrategy(
            ChangeTrackingStrategy? changeTrackingStrategy,
            ConfigurationSource configurationSource)
            => configurationSource.Overrides(Metadata.GetChangeTrackingStrategyConfigurationSource())
                || Metadata.GetChangeTrackingStrategy() == changeTrackingStrategy;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalModelBuilder UsePropertyAccessMode(
            PropertyAccessMode? propertyAccessMode,
            ConfigurationSource configurationSource)
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
            PropertyAccessMode? propertyAccessMode,
            ConfigurationSource configurationSource)
            => configurationSource.Overrides(Metadata.GetPropertyAccessModeConfigurationSource())
                || Metadata.GetPropertyAccessMode() == propertyAccessMode;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionModel IConventionModelBuilder.Metadata
        {
            [DebuggerStepThrough]
            get => Metadata;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionEntityTypeBuilder IConventionModelBuilder.Entity(string name, bool? shouldBeOwned, bool fromDataAnnotation)
            => Entity(name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention, shouldBeOwned);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionEntityTypeBuilder IConventionModelBuilder.SharedTypeEntity(
            string name,
            Type type,
            bool? shouldBeOwned,
            bool fromDataAnnotation)
            => SharedTypeEntity(
                name, type, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention, shouldBeOwned);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionEntityTypeBuilder IConventionModelBuilder.Entity(Type type, bool? shouldBeOwned, bool fromDataAnnotation)
            => Entity(type, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention, shouldBeOwned);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionEntityTypeBuilder IConventionModelBuilder.Entity(
            string name,
            string definingNavigationName,
            IConventionEntityType definingEntityType,
            bool fromDataAnnotation)
            => Entity(
                name,
                definingNavigationName,
                (EntityType)definingEntityType,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionEntityTypeBuilder IConventionModelBuilder.Entity(
            Type type,
            string definingNavigationName,
            IConventionEntityType definingEntityType,
            bool fromDataAnnotation)
            => Entity(
                type,
                definingNavigationName,
                (EntityType)definingEntityType,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionOwnedEntityTypeBuilder IConventionModelBuilder.Owned(Type type, bool fromDataAnnotation)
            => Owned(type, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        bool IConventionModelBuilder.IsIgnored(Type type, bool fromDataAnnotation)
            => IsIgnored(type, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        bool IConventionModelBuilder.IsIgnored(string name, bool fromDataAnnotation)
            => IsIgnored(name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionModelBuilder IConventionModelBuilder.Ignore(Type type, bool fromDataAnnotation)
            => Ignore(type, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionModelBuilder IConventionModelBuilder.Ignore(string name, bool fromDataAnnotation)
            => Ignore(name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionModelBuilder IConventionModelBuilder.HasNoEntityType(IConventionEntityType entityType, bool fromDataAnnotation)
            => HasNoEntityType(
                (EntityType)entityType, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        bool IConventionModelBuilder.CanIgnore(Type type, bool fromDataAnnotation)
            => CanIgnore(type, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        bool IConventionModelBuilder.CanIgnore(string name, bool fromDataAnnotation)
            => CanIgnore(name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionModelBuilder IConventionModelBuilder.HasChangeTrackingStrategy(
            ChangeTrackingStrategy? changeTrackingStrategy,
            bool fromDataAnnotation)
            => UseChangeTrackingStrategy(
                changeTrackingStrategy, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        bool IConventionModelBuilder.CanSetChangeTrackingStrategy(ChangeTrackingStrategy? changeTrackingStrategy, bool fromDataAnnotation)
            => CanSetChangeTrackingStrategy(
                changeTrackingStrategy, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionModelBuilder IConventionModelBuilder.UsePropertyAccessMode(
            PropertyAccessMode? propertyAccessMode,
            bool fromDataAnnotation)
            => UsePropertyAccessMode(
                propertyAccessMode, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        bool IConventionModelBuilder.CanSetPropertyAccessMode(PropertyAccessMode? propertyAccessMode, bool fromDataAnnotation)
            => CanSetPropertyAccessMode(
                propertyAccessMode, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
    }
}

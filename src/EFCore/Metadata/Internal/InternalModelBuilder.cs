// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

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
    public InternalModelBuilder(Model metadata)
        : base(metadata, null!)
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
    public virtual InternalEntityTypeBuilder? Entity(
        string name,
        ConfigurationSource configurationSource,
        bool? shouldBeOwned = false)
        => Entity(new TypeIdentity(name), configurationSource, shouldBeOwned);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalEntityTypeBuilder? Entity(
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type type,
        ConfigurationSource configurationSource,
        bool? shouldBeOwned = null)
        => Entity(new TypeIdentity(type, Metadata), configurationSource, shouldBeOwned);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalEntityTypeBuilder? Entity(
        string name,
        string definingNavigationName,
        EntityType definingEntityType,
        ConfigurationSource configurationSource)
        => Entity(new TypeIdentity(name), definingNavigationName, definingEntityType, configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalEntityTypeBuilder? Entity(
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type type,
        string definingNavigationName,
        EntityType definingEntityType,
        ConfigurationSource configurationSource)
        => Entity(new TypeIdentity(type, Metadata), definingNavigationName, definingEntityType, configurationSource);

    private InternalEntityTypeBuilder? Entity(
        in TypeIdentity type,
        string definingNavigationName,
        EntityType definingEntityType,
        ConfigurationSource configurationSource)
        => SharedTypeEntity(
            definingEntityType.GetOwnedName(type.Type?.ShortDisplayName() ?? type.Name, definingNavigationName),
            type.Type, configurationSource, shouldBeOwned: true);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalEntityTypeBuilder? SharedTypeEntity(
        string name,
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type? type,
        ConfigurationSource configurationSource,
        bool? shouldBeOwned = false)
        => Entity(new TypeIdentity(name, type ?? Model.DefaultPropertyBagType), configurationSource, shouldBeOwned);

    private InternalEntityTypeBuilder? Entity(
        in TypeIdentity type,
        ConfigurationSource configurationSource,
        bool? shouldBeOwned)
    {
        if (!CanHaveEntity(type, configurationSource, shouldBeOwned, shouldThrow: configurationSource == ConfigurationSource.Explicit))
        {
            return null;
        }

        using var batch = Metadata.DelayConventions();
        var clrType = type.Type;
        EntityType? entityType;
        EntityType.Snapshot? entityTypeSnapshot = null;
        if (type.IsNamed)
        {
            if (clrType != null)
            {
                entityType = Metadata.FindEntityType(clrType);
                if (entityType != null)
                {
                    entityTypeSnapshot = InternalEntityTypeBuilder.DetachAllMembers(entityType);

                    // TODO: Use convention batch to track replaced entity type, see #15898
                    HasNoEntityType(entityType, ConfigurationSource.Explicit);
                }
            }

            entityType = Metadata.FindEntityType(type.Name);
        }
        else
        {
            clrType = type.Type!;
            var sharedConfigurationSource = Metadata.FindIsSharedConfigurationSource(clrType);
            if (sharedConfigurationSource != null)
            {
                if (!configurationSource.OverridesStrictly(sharedConfigurationSource.Value))
                {
                    return configurationSource == ConfigurationSource.Explicit
                        ? throw new InvalidOperationException(CoreStrings.ClashingSharedType(clrType.ShortDisplayName()))
                        : null;
                }

                foreach (var sharedTypeEntityType in Metadata.FindEntityTypes(clrType).ToList())
                {
                    HasNoEntityType(sharedTypeEntityType, configurationSource);
                }

                Metadata.RemoveShared(clrType);
            }

            entityType = Metadata.FindEntityType(clrType);
        }

        if (shouldBeOwned == false
            && clrType != null
            && (!configurationSource.OverridesStrictly(Metadata.FindIsOwnedConfigurationSource(clrType))
                || (Metadata.Configuration?.GetConfigurationType(clrType) == TypeConfigurationType.OwnedEntityType
                    && configurationSource != ConfigurationSource.Explicit)))
        {
            if (configurationSource == ConfigurationSource.Explicit)
            {
                throw new InvalidOperationException(
                    CoreStrings.ClashingOwnedEntityType(clrType == null ? type.Name : clrType.ShortDisplayName()));
            }

            return null;
        }

        if (entityType != null)
        {
            if (type.Type == null
                || entityType.ClrType == type.Type)
            {
                if (shouldBeOwned.HasValue)
                {
                    entityType.Builder.IsOwned(shouldBeOwned.Value, configurationSource);
                }

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
                        CoreStrings.ClashingMismatchedSharedType(type.Name, entityType.ClrType.ShortDisplayName()))
                    : null;
            }
        }
        else if (clrType != null)
        {
            var complexConfigurationSource = Metadata.FindIsComplexConfigurationSource(clrType);
            if (complexConfigurationSource != null
                && configurationSource == ConfigurationSource.Convention)
            {
                return null;
            }
        }

        if (shouldBeOwned == null)
        {
            if (type.Type == null)
            {
                return null;
            }

            var configurationType = Metadata.Configuration?.GetConfigurationType(type.Type);
            switch (configurationType)
            {
                case null:
                    break;
                case TypeConfigurationType.EntityType:
                case TypeConfigurationType.SharedTypeEntityType:
                {
                    shouldBeOwned ??= false;
                    break;
                }
                case TypeConfigurationType.OwnedEntityType:
                {
                    shouldBeOwned ??= true;
                    break;
                }
                default:
                {
                    if (configurationSource != ConfigurationSource.Explicit)
                    {
                        return null;
                    }

                    break;
                }
            }

            shouldBeOwned ??= Metadata.FindIsOwnedConfigurationSource(type.Type) != null;
        }

        if (type.IsNamed
            && clrType != null)
        {
            Metadata.AddShared(clrType, configurationSource);
        }

        Metadata.RemoveIgnored(type.Name);
        entityType = type.IsNamed
            ? clrType == null
                ? Metadata.AddEntityType(type.Name, shouldBeOwned.Value, configurationSource)
                : Metadata.AddEntityType(type.Name, clrType, shouldBeOwned.Value, configurationSource)
            : Metadata.AddEntityType(clrType!, shouldBeOwned.Value, configurationSource);

        if (entityType != null
            && entityTypeSnapshot != null)
        {
            entityTypeSnapshot.Attach(entityType.Builder);
        }

        return entityType?.Builder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanHaveEntity(
        in TypeIdentity type,
        ConfigurationSource configurationSource,
        bool? shouldBeOwned,
        bool shouldThrow = false)
    {
        if (IsIgnored(type, configurationSource))
        {
            return false;
        }

        if (type.Type != null
            && shouldBeOwned != null)
        {
            var configurationType = shouldBeOwned.Value
                ? TypeConfigurationType.OwnedEntityType
                : type.IsNamed
                    ? TypeConfigurationType.SharedTypeEntityType
                    : TypeConfigurationType.EntityType;

            if (!CanBeConfigured(type.Type, configurationType, configurationSource))
            {
                return false;
            }
        }

        var clrType = type.Type;
        EntityType? entityType;
        if (type.IsNamed)
        {
            if (clrType != null)
            {
                entityType = Metadata.FindEntityType(clrType);
                if (entityType != null)
                {
                    Check.DebugAssert(
                        entityType.Name != type.Name || !entityType.HasSharedClrType,
                        "Shared type entity types shouldn't be named the same as non-shared");

                    if (!configurationSource.OverridesStrictly(entityType.GetConfigurationSource())
                        && !entityType.IsOwned())
                    {
                        return shouldThrow
                            ? throw new InvalidOperationException(
                                CoreStrings.ClashingNonSharedType(type.Name, clrType.ShortDisplayName()))
                            : false;
                    }
                }
            }

            entityType = Metadata.FindEntityType(type.Name);
        }
        else
        {
            clrType = type.Type!;
            var sharedConfigurationSource = Metadata.FindIsSharedConfigurationSource(clrType);
            if (sharedConfigurationSource != null
                && !configurationSource.OverridesStrictly(sharedConfigurationSource.Value))
            {
                return shouldThrow
                    ? throw new InvalidOperationException(CoreStrings.ClashingSharedType(clrType.ShortDisplayName()))
                    : false;
            }

            entityType = Metadata.FindEntityType(clrType);
        }

        if (shouldBeOwned == false
            && clrType != null
            && (!configurationSource.OverridesStrictly(Metadata.FindIsOwnedConfigurationSource(clrType))
                || (Metadata.Configuration?.GetConfigurationType(clrType) == TypeConfigurationType.OwnedEntityType
                    && configurationSource != ConfigurationSource.Explicit)))
        {
            return shouldThrow
                ? throw new InvalidOperationException(
                    CoreStrings.ClashingOwnedEntityType(clrType == null ? type.Name : clrType.ShortDisplayName()))
                : false;
        }

        if (entityType != null
            && type.Type != null
            && entityType.ClrType != type.Type
            && !configurationSource.OverridesStrictly(entityType.GetConfigurationSource()))
        {
            return shouldThrow
                ? throw new InvalidOperationException(
                    CoreStrings.ClashingMismatchedSharedType(type.Name, entityType.ClrType.ShortDisplayName()))
                : false;
        }

        if (entityType == null
            && clrType != null)
        {
            var complexConfigurationSource = Metadata.FindIsComplexConfigurationSource(clrType);
            if (complexConfigurationSource != null
                && configurationSource == ConfigurationSource.Convention)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalModelBuilder? RemoveImplicitJoinEntity(
        EntityType joinEntityType, ConfigurationSource configurationSource = ConfigurationSource.Convention)
        => !Check.NotNull(joinEntityType, nameof(joinEntityType)).IsInModel
            ? this
            : !joinEntityType.IsImplicitlyCreatedJoinEntityType
                ? null
                : HasNoEntityType(joinEntityType, configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionOwnedEntityTypeBuilder? Owned(
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type type,
        ConfigurationSource configurationSource)
    {
        if (IsIgnored(type, configurationSource)
            || !CanBeConfigured(type, TypeConfigurationType.OwnedEntityType, configurationSource))
        {
            return null;
        }

        foreach (var existingEntityType in Metadata.FindEntityTypes(type))
        {
            if (!existingEntityType.Builder.CanSetIsOwned(true, configurationSource))
            {
                return null;
            }
        }

        Metadata.RemoveIgnored(type);
        Metadata.AddOwned(type, ConfigurationSource.Explicit);

        foreach (var entityType in Metadata.FindEntityTypes(type).ToList())
        {
            if (entityType.FindOwnership() != null)
            {
                continue;
            }

            var ownershipCandidates = entityType.GetForeignKeys().Where(
                fk => fk.PrincipalToDependent != null
                    && !fk.PrincipalEntityType.IsInOwnershipPath(type)).ToList();
            if (ownershipCandidates.Count >= 1)
            {
                if (ownershipCandidates[0].Builder.IsOwnership(true, configurationSource) == null)
                {
                    return null;
                }
            }
            else
            {
                // Discover the ownership when the type is added back
                HasNoEntityType(entityType, configurationSource);
            }
        }

        return new InternalOwnedEntityTypeBuilder();
    }

    private bool IsOwned(in TypeIdentity type)
        => type.Type != null && Metadata.IsOwned(type.Type);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalModelBuilder Complex(Type type, ConfigurationSource configurationSource)
    {
        var existingComplexConfiguration = Metadata.FindIsComplexConfigurationSource(type);
        if (existingComplexConfiguration == null)
        {
            Metadata.AddComplex(type, configurationSource);

            foreach (var existingEntityType in Metadata.FindEntityTypes(type).ToList())
            {
                Metadata.Builder.HasNoEntityType(existingEntityType, ConfigurationSource.Convention);
            }

            var properties = Metadata.FindProperties(type);
            if (properties != null)
            {
                foreach (var property in properties)
                {
                    property.DeclaringType.Builder.RemoveProperty(property, ConfigurationSource.Convention);
                }
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
    public virtual bool IsIgnored(
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type type,
        ConfigurationSource? configurationSource)
        => IsIgnored(new TypeIdentity(type, Metadata), configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsIgnored(string name, ConfigurationSource? configurationSource)
        => IsIgnored(new TypeIdentity(name), configurationSource);

    private bool IsIgnored(in TypeIdentity type, ConfigurationSource? configurationSource)
    {
        if (configurationSource == ConfigurationSource.Explicit)
        {
            return false;
        }

        var ignoredConfigurationSource = Metadata.FindIgnoredConfigurationSource(type.Name);
        if (type.Type != null
            && Metadata.IsIgnoredType(type.Type))
        {
            ignoredConfigurationSource = ConfigurationSource.Explicit;
        }

        return ignoredConfigurationSource.HasValue
            && ignoredConfigurationSource.Value.Overrides(configurationSource);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanBeConfigured(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type type,
        TypeConfigurationType configurationType,
        ConfigurationSource configurationSource)
    {
        if (configurationSource == ConfigurationSource.Explicit)
        {
            return true;
        }

        if (!configurationType.IsEntityType()
            && (!configurationSource.Overrides(Metadata.FindIsOwnedConfigurationSource(type))
                || Metadata.FindEntityTypes(type).Any(e => !configurationSource.Overrides(e.GetConfigurationSource()))))
        {
            return false;
        }

        var configuredType = ModelBuilder.Metadata.Configuration?.GetConfigurationType(type);
        return configuredType == null
            || configuredType == configurationType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalModelBuilder? Ignore(
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type type,
        ConfigurationSource configurationSource)
        => Ignore(new TypeIdentity(type, Metadata), configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalModelBuilder? Ignore(string name, ConfigurationSource configurationSource)
        => Ignore(new TypeIdentity(name), configurationSource);

    private InternalModelBuilder? Ignore(in TypeIdentity type, ConfigurationSource configurationSource)
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

        using (Metadata.DelayConventions())
        {
            var entityType = Metadata.FindEntityType(name);
            if (entityType != null)
            {
                if (entityType.GetConfigurationSource() == ConfigurationSource.Explicit)
                {
                    Metadata.ScopedModelDependencies?.Logger.MappedEntityTypeIgnoredWarning(entityType);
                }

                HasNoEntityType(entityType, configurationSource);
            }

            if (type.Type == null)
            {
                Metadata.AddIgnored(name, configurationSource);
            }
            else
            {
                Metadata.AddIgnored(type.Type, configurationSource);
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
    public virtual bool CanIgnore(
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type type,
        ConfigurationSource configurationSource)
        => CanIgnore(new TypeIdentity(type, Metadata), configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanIgnore(string name, ConfigurationSource configurationSource)
        => CanIgnore(new TypeIdentity(name), configurationSource);

    private bool CanIgnore(in TypeIdentity type, ConfigurationSource configurationSource)
    {
        var name = type.Name;
        if (Metadata.FindIgnoredConfigurationSource(name).HasValue)
        {
            return true;
        }

        if (IsOwned(type)
            && configurationSource != ConfigurationSource.Explicit)
        {
            return false;
        }

        if (type.Type != null
            && Metadata.FindEntityTypes(type.Type).Any(o => !configurationSource.Overrides(o.GetConfigurationSource())))
        {
            return false;
        }

        if (Metadata.FindEntityType(name)?.GetConfigurationSource().OverridesStrictly(configurationSource) == true)
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
    public virtual InternalModelBuilder? HasNoEntityType(EntityType entityType, ConfigurationSource configurationSource)
    {
        if (!entityType.IsInModel)
        {
            return this;
        }

        var entityTypeConfigurationSource = entityType.GetConfigurationSource();
        if (!configurationSource.Overrides(entityTypeConfigurationSource))
        {
            return null;
        }

        using (Metadata.DelayConventions())
        {
            foreach (var foreignKey in entityType.GetDeclaredReferencingForeignKeys().ToList())
            {
                if (foreignKey.IsOwnership
                    && configurationSource.Overrides(foreignKey.DeclaringEntityType.GetConfigurationSource()))
                {
                    HasNoEntityType(foreignKey.DeclaringEntityType, configurationSource);
                }
                else
                {
                    var removed = foreignKey.DeclaringEntityType.Builder.HasNoRelationship(foreignKey, configurationSource);
                    Check.DebugAssert(removed != null, "removed is null");
                }
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
    public virtual bool CanRemoveEntityType(EntityType entityType, ConfigurationSource configurationSource)
        => configurationSource.Overrides(entityType.GetConfigurationSource());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalModelBuilder? HasChangeTrackingStrategy(
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
    public virtual InternalModelBuilder? UsePropertyAccessMode(
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
    IConventionModelBuilder? IConventionModelBuilder.HasAnnotation(string name, object? value, bool fromDataAnnotation)
        => (IConventionModelBuilder?)base.HasAnnotation(
            name, value, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionModelBuilder? IConventionModelBuilder.HasNonNullAnnotation(string name, object? value, bool fromDataAnnotation)
        => (IConventionModelBuilder?)base.HasNonNullAnnotation(
            name, value, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionModelBuilder? IConventionModelBuilder.HasNoAnnotation(string name, bool fromDataAnnotation)
        => (IConventionModelBuilder?)base.HasNoAnnotation(
            name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionEntityTypeBuilder? IConventionModelBuilder.Entity(string name, bool? shouldBeOwned, bool fromDataAnnotation)
        => Entity(name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention, shouldBeOwned);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionEntityTypeBuilder? IConventionModelBuilder.SharedTypeEntity(
        string name,
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type type,
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
    IConventionEntityTypeBuilder? IConventionModelBuilder.Entity(
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type type,
        bool? shouldBeOwned,
        bool fromDataAnnotation)
        => Entity(type, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention, shouldBeOwned);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionEntityTypeBuilder? IConventionModelBuilder.Entity(
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
    IConventionEntityTypeBuilder? IConventionModelBuilder.Entity(
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type type,
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
    IConventionOwnedEntityTypeBuilder? IConventionModelBuilder.Owned(
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type type,
        bool fromDataAnnotation)
        => Owned(type, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionModelBuilder? IConventionModelBuilder.ComplexType(Type type, bool fromDataAnnotation)
        => Complex(type, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    bool IConventionModelBuilder.CanHaveEntity(string name, bool fromDataAnnotation)
        => CanHaveEntity(
            new TypeIdentity(name),
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention,
            shouldBeOwned: null);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    bool IConventionModelBuilder.CanHaveEntity(Type type, bool fromDataAnnotation)
        => CanHaveEntity(
            new TypeIdentity(type, Metadata),
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention,
            shouldBeOwned: null);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    bool IConventionModelBuilder.CanHaveSharedTypeEntity(string name, Type? type, bool fromDataAnnotation)
        => CanHaveEntity(
            new TypeIdentity(name, type ?? Model.DefaultPropertyBagType),
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention,
            shouldBeOwned: null);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    bool IConventionModelBuilder.CanRemoveEntity(IConventionEntityType entityType, bool fromDataAnnotation)
        => CanRemoveEntityType(
            (EntityType)entityType,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    bool IConventionModelBuilder.IsIgnored(
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type type,
        bool fromDataAnnotation)
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
    IConventionModelBuilder? IConventionModelBuilder.Ignore(
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type type,
        bool fromDataAnnotation)
        => Ignore(type, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionModelBuilder? IConventionModelBuilder.Ignore(string name, bool fromDataAnnotation)
        => Ignore(name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionModelBuilder? IConventionModelBuilder.HasNoEntityType(IConventionEntityType entityType, bool fromDataAnnotation)
        => HasNoEntityType(
            (EntityType)entityType, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    bool IConventionModelBuilder.CanIgnore(
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type type,
        bool fromDataAnnotation)
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
    IConventionModelBuilder? IConventionModelBuilder.HasChangeTrackingStrategy(
        ChangeTrackingStrategy? changeTrackingStrategy,
        bool fromDataAnnotation)
        => HasChangeTrackingStrategy(
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
    IConventionModelBuilder? IConventionModelBuilder.UsePropertyAccessMode(
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

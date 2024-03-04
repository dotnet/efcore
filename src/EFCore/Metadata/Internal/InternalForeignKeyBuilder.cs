// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class InternalForeignKeyBuilder : AnnotatableBuilder<ForeignKey, InternalModelBuilder>, IConventionForeignKeyBuilder
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public InternalForeignKeyBuilder(
        ForeignKey foreignKey,
        InternalModelBuilder modelBuilder)
        : base(foreignKey, modelBuilder)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalForeignKeyBuilder? HasNavigation(
        string? name,
        bool pointsToPrincipal,
        ConfigurationSource configurationSource)
        => pointsToPrincipal
            ? HasNavigations(
                MemberIdentity.Create(name),
                navigationToDependent: null,
                configurationSource)
            : HasNavigations(
                navigationToPrincipal: null,
                MemberIdentity.Create(name),
                configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalForeignKeyBuilder? HasNavigation(
        MemberInfo? property,
        bool pointsToPrincipal,
        ConfigurationSource configurationSource)
        => pointsToPrincipal
            ? HasNavigations(
                MemberIdentity.Create(property),
                navigationToDependent: null,
                configurationSource)
            : HasNavigations(
                navigationToPrincipal: null,
                MemberIdentity.Create(property),
                configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalForeignKeyBuilder? HasNavigations(
        string? navigationToPrincipalName,
        string? navigationToDependentName,
        ConfigurationSource configurationSource)
        => HasNavigations(
            MemberIdentity.Create(navigationToPrincipalName),
            MemberIdentity.Create(navigationToDependentName),
            configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalForeignKeyBuilder? HasNavigations(
        MemberInfo? navigationToPrincipal,
        MemberInfo? navigationToDependent,
        ConfigurationSource configurationSource)
        => HasNavigations(
            MemberIdentity.Create(navigationToPrincipal),
            MemberIdentity.Create(navigationToDependent),
            configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalForeignKeyBuilder? HasNavigations(
        MemberIdentity? navigationToPrincipal,
        MemberIdentity? navigationToDependent,
        ConfigurationSource configurationSource)
        => HasNavigations(
            navigationToPrincipal,
            navigationToDependent,
            Metadata.PrincipalEntityType,
            Metadata.DeclaringEntityType,
            configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalForeignKeyBuilder? HasNavigations(
        string? navigationToPrincipalName,
        string? navigationToDependentName,
        EntityType principalEntityType,
        EntityType dependentEntityType,
        ConfigurationSource configurationSource)
        => HasNavigations(
            MemberIdentity.Create(navigationToPrincipalName),
            MemberIdentity.Create(navigationToDependentName),
            principalEntityType,
            dependentEntityType,
            configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalForeignKeyBuilder? HasNavigations(
        MemberInfo? navigationToPrincipal,
        MemberInfo? navigationToDependent,
        EntityType principalEntityType,
        EntityType dependentEntityType,
        ConfigurationSource configurationSource)
        => HasNavigations(
            MemberIdentity.Create(navigationToPrincipal),
            MemberIdentity.Create(navigationToDependent),
            principalEntityType,
            dependentEntityType,
            configurationSource);

    private InternalForeignKeyBuilder? HasNavigations(
        MemberIdentity? navigationToPrincipal,
        MemberIdentity? navigationToDependent,
        EntityType principalEntityType,
        EntityType dependentEntityType,
        ConfigurationSource configurationSource)
    {
        var navigationToPrincipalName = navigationToPrincipal?.Name;
        var navigationToDependentName = navigationToDependent?.Name;
        if ((navigationToPrincipal == null
                || navigationToPrincipal.Value.Name == Metadata.DependentToPrincipal?.Name)
            && (navigationToDependent == null
                || navigationToDependent.Value.Name == Metadata.PrincipalToDependent?.Name))
        {
            Metadata.UpdateConfigurationSource(configurationSource);
            if (navigationToPrincipal != null)
            {
                Metadata.SetDependentToPrincipal(navigationToPrincipal.Value.Name, configurationSource);
                if (navigationToPrincipalName != null)
                {
                    dependentEntityType.RemoveIgnored(navigationToPrincipalName);
                }
            }

            if (navigationToDependent != null)
            {
                Metadata.SetPrincipalToDependent(navigationToDependent.Value.Name, configurationSource);
                if (navigationToDependentName != null)
                {
                    principalEntityType.RemoveIgnored(navigationToDependentName);
                }
            }

            return this;
        }

        var shouldThrow = configurationSource == ConfigurationSource.Explicit;

        if (navigationToPrincipalName != null
            && navigationToPrincipal!.Value.MemberInfo == null
            && dependentEntityType.ClrType != Model.DefaultPropertyBagType)
        {
            var navigationProperty = FindCompatibleClrMember(
                navigationToPrincipalName, dependentEntityType, principalEntityType, shouldThrow);
            if (navigationProperty != null)
            {
                navigationToPrincipal = MemberIdentity.Create(navigationProperty);
            }
        }

        if (navigationToDependentName != null
            && navigationToDependent!.Value.MemberInfo == null
            && principalEntityType.ClrType != Model.DefaultPropertyBagType)
        {
            var navigationProperty = FindCompatibleClrMember(
                navigationToDependentName, principalEntityType, dependentEntityType, shouldThrow);
            if (navigationProperty != null)
            {
                navigationToDependent = MemberIdentity.Create(navigationProperty);
            }
        }

        if (!CanSetNavigations(
                navigationToPrincipal,
                navigationToDependent,
                principalEntityType,
                dependentEntityType,
                configurationSource,
                shouldThrow,
                out var shouldInvert,
                out var shouldBeUnique,
                out var removeOppositeNavigation,
                out var conflictingNavigationsFound,
                out var changeRelatedTypes))
        {
            return null;
        }

        if (removeOppositeNavigation)
        {
            navigationToPrincipal ??= MemberIdentity.None;
            navigationToDependent ??= MemberIdentity.None;
        }

        IReadOnlyList<Property>? dependentProperties = null;
        IReadOnlyList<Property>? principalProperties = null;
        if (shouldInvert == true)
        {
            Check.DebugAssert(
                configurationSource.Overrides(Metadata.GetPropertiesConfigurationSource()),
                "configurationSource does not override Metadata.GetPropertiesConfigurationSource");

            Check.DebugAssert(
                configurationSource.Overrides(Metadata.GetPrincipalKeyConfigurationSource()),
                "configurationSource does not override Metadata.GetPrincipalKeyConfigurationSource");

            (principalEntityType, dependentEntityType) = (dependentEntityType, principalEntityType);
            (navigationToPrincipal, navigationToDependent) = (navigationToDependent, navigationToPrincipal);

            navigationToPrincipalName = navigationToPrincipal?.Name;
            navigationToDependentName = navigationToDependent?.Name;

            if (Metadata.GetPropertiesConfigurationSource() == configurationSource)
            {
                dependentProperties = [];
            }

            if (Metadata.GetPrincipalKeyConfigurationSource() == configurationSource)
            {
                principalProperties = [];
            }
        }

        if (navigationToPrincipalName != null
            && !dependentEntityType.FindNavigationsInHierarchy(navigationToPrincipalName).Any())
        {
            dependentEntityType.Builder.RemoveMembersInHierarchy(navigationToPrincipalName, configurationSource);
        }

        if (navigationToDependentName != null
            && !principalEntityType.FindNavigationsInHierarchy(navigationToDependentName).Any())
        {
            principalEntityType.Builder.RemoveMembersInHierarchy(navigationToDependentName, configurationSource);
        }

        InternalForeignKeyBuilder? builder;
        if (shouldInvert == true
            || conflictingNavigationsFound
            || changeRelatedTypes)
        {
            builder = ReplaceForeignKey(
                configurationSource,
                principalEntityType.Builder,
                dependentEntityType.Builder,
                navigationToPrincipal,
                navigationToDependent,
                dependentProperties,
                principalProperties: principalProperties,
                isUnique: shouldBeUnique,
                removeCurrent: shouldInvert == true || changeRelatedTypes,
                principalEndConfigurationSource: shouldInvert != null ? configurationSource : null,
                oldRelationshipInverted: shouldInvert == true);

            if (builder == null)
            {
                return null;
            }

            Check.DebugAssert(builder.Metadata.IsInModel, "builder.Metadata isn't in the model");
        }
        else
        {
            using var batch = Metadata.DeclaringEntityType.Model.DelayConventions();
            builder = this;

            if (navigationToPrincipal != null)
            {
                if (navigationToPrincipal.Value.Name == Metadata.PrincipalToDependent?.Name)
                {
                    Metadata.SetPrincipalToDependent((string?)null, configurationSource);
                }

                var navigationProperty = navigationToPrincipal.Value.MemberInfo;
                if (navigationToPrincipalName != null)
                {
                    Metadata.DeclaringEntityType.RemoveIgnored(navigationToPrincipalName);

                    if (Metadata.DeclaringEntityType.ClrType != Model.DefaultPropertyBagType
                        && navigationProperty == null)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.NoClrNavigation(navigationToPrincipalName, Metadata.DeclaringEntityType.DisplayName()));
                    }
                }

                Metadata.SetDependentToPrincipal(navigationToPrincipal, configurationSource);
            }

            if (navigationToDependent != null)
            {
                // TODO: Use layering instead, issue #15898
                IsUnique(shouldBeUnique, shouldBeUnique.HasValue ? configurationSource : ConfigurationSource.Convention);

                var navigationProperty = navigationToDependent.Value.MemberInfo;
                if (navigationToDependentName != null)
                {
                    Metadata.PrincipalEntityType.RemoveIgnored(navigationToDependentName);

                    if (Metadata.PrincipalEntityType.ClrType != Model.DefaultPropertyBagType
                        && navigationProperty == null)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.NoClrNavigation(navigationToDependentName, Metadata.PrincipalEntityType.DisplayName()));
                    }
                }

                Metadata.SetPrincipalToDependent(navigationToDependent, configurationSource);
            }

            builder = batch.Run(builder);
        }

        return builder != null
            && ((navigationToPrincipal != null
                    && builder.Metadata.DependentToPrincipal?.Name != navigationToPrincipal.Value.Name)
                || (navigationToDependent != null
                    && builder.Metadata.PrincipalToDependent?.Name != navigationToDependent.Value.Name))
            && ((navigationToDependent != null
                    && builder.Metadata.DependentToPrincipal?.Name != navigationToDependent.Value.Name)
                || (navigationToPrincipal != null
                    && builder.Metadata.PrincipalToDependent?.Name != navigationToPrincipal.Value.Name))
                ? null
                : builder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static MemberInfo? FindCompatibleClrMember(
        string navigationName,
        EntityType sourceType,
        EntityType targetType,
        bool shouldThrow = false)
    {
        var navigationProperty = sourceType.GetNavigationMemberInfo(navigationName);
        return !Navigation.IsCompatible(navigationName, navigationProperty, sourceType, targetType, null, shouldThrow)
            ? null
            : navigationProperty;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSetNavigation(
        MemberInfo? property,
        bool pointsToPrincipal,
        ConfigurationSource? configurationSource)
        => CanSetNavigation(
            MemberIdentity.Create(property),
            pointsToPrincipal,
            configurationSource,
            shouldThrow: false,
            out _);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSetNavigation(
        string? name,
        bool pointsToPrincipal,
        ConfigurationSource? configurationSource)
        => CanSetNavigation(
            MemberIdentity.Create(name),
            pointsToPrincipal,
            configurationSource,
            shouldThrow: false,
            out _);

    private bool CanSetNavigation(
        MemberIdentity navigation,
        bool pointsToPrincipal,
        ConfigurationSource? configurationSource,
        bool shouldThrow,
        out bool? shouldBeUnique)
        => pointsToPrincipal
            ? CanSetNavigations(
                navigation,
                navigationToDependent: null,
                configurationSource,
                shouldThrow,
                out shouldBeUnique)
            : CanSetNavigations(
                navigationToPrincipal: null,
                navigation,
                configurationSource,
                shouldThrow,
                out shouldBeUnique);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSetNavigations(
        MemberInfo? navigationToPrincipal,
        MemberInfo? navigationToDependent,
        ConfigurationSource? configurationSource)
        => CanSetNavigations(
            MemberIdentity.Create(navigationToPrincipal),
            MemberIdentity.Create(navigationToDependent),
            configurationSource,
            shouldThrow: false,
            out _);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSetNavigations(
        string? navigationToPrincipalName,
        string? navigationToDependentName,
        ConfigurationSource? configurationSource)
        => CanSetNavigations(
            MemberIdentity.Create(navigationToPrincipalName),
            MemberIdentity.Create(navigationToDependentName),
            configurationSource,
            shouldThrow: false,
            out _);

    private bool CanSetNavigations(
        MemberIdentity? navigationToPrincipal,
        MemberIdentity? navigationToDependent,
        ConfigurationSource? configurationSource)
        => CanSetNavigations(
            navigationToPrincipal,
            navigationToDependent,
            configurationSource,
            shouldThrow: false,
            out _);

    private bool CanSetNavigations(
        MemberIdentity? navigationToPrincipal,
        MemberIdentity? navigationToDependent,
        ConfigurationSource? configurationSource,
        bool shouldThrow,
        out bool? shouldBeUnique)
        => CanSetNavigations(
            navigationToPrincipal,
            navigationToDependent,
            Metadata.PrincipalEntityType,
            Metadata.DeclaringEntityType,
            configurationSource,
            shouldThrow,
            out _,
            out shouldBeUnique,
            out _,
            out _,
            out _);

    private bool CanSetNavigations(
        MemberIdentity? navigationToPrincipal,
        MemberIdentity? navigationToDependent,
        EntityType principalEntityType,
        EntityType dependentEntityType,
        ConfigurationSource? configurationSource,
        bool shouldThrow,
        out bool? shouldInvert,
        out bool? shouldBeUnique,
        out bool removeOppositeNavigation,
        out bool conflictingNavigationsFound,
        out bool changeRelatedTypes)
    {
        shouldInvert = null;
        shouldBeUnique = null;
        removeOppositeNavigation = false;
        conflictingNavigationsFound = false;
        changeRelatedTypes = false;

        if ((navigationToPrincipal == null
                || navigationToPrincipal.Value.Name == Metadata.DependentToPrincipal?.Name)
            && (navigationToDependent == null
                || navigationToDependent.Value.Name == Metadata.PrincipalToDependent?.Name))
        {
            return true;
        }

        if (!configurationSource.HasValue)
        {
            return false;
        }

        var navigationToPrincipalName = navigationToPrincipal?.Name;
        if (navigationToPrincipal != null
            && navigationToPrincipalName != Metadata.DependentToPrincipal?.Name)
        {
            if (!configurationSource.Overrides(Metadata.GetDependentToPrincipalConfigurationSource()))
            {
                return false;
            }

            if (navigationToPrincipalName != null)
            {
                if (navigationToDependent == null
                    && navigationToPrincipalName == Metadata.PrincipalToDependent?.Name
                    && (Metadata.DeclaringEntityType.IsAssignableFrom(Metadata.PrincipalEntityType)
                        || Metadata.PrincipalEntityType.IsAssignableFrom(Metadata.DeclaringEntityType)))
                {
                    if (!configurationSource.Value.Overrides(Metadata.GetPrincipalToDependentConfigurationSource()))
                    {
                        return false;
                    }

                    removeOppositeNavigation = true;
                }
                else if ((configurationSource != ConfigurationSource.Explicit || !shouldThrow)
                         && (!dependentEntityType.Builder.CanAddNavigation(
                             navigationToPrincipalName, navigationToPrincipal.Value.MemberInfo?.GetMemberType(),
                             configurationSource.Value)))
                {
                    return false;
                }
            }
        }

        var navigationToDependentName = navigationToDependent?.Name;
        if (navigationToDependent != null
            && navigationToDependentName != Metadata.PrincipalToDependent?.Name)
        {
            if (!configurationSource.Overrides(Metadata.GetPrincipalToDependentConfigurationSource()))
            {
                return false;
            }

            if (navigationToDependentName != null)
            {
                if (navigationToPrincipal == null
                    && navigationToDependentName == Metadata.DependentToPrincipal?.Name
                    && (Metadata.DeclaringEntityType.IsAssignableFrom(Metadata.PrincipalEntityType)
                        || Metadata.PrincipalEntityType.IsAssignableFrom(Metadata.DeclaringEntityType)))
                {
                    if (!configurationSource.Value.Overrides(Metadata.GetDependentToPrincipalConfigurationSource()))
                    {
                        return false;
                    }

                    removeOppositeNavigation = true;
                }
                else if ((configurationSource != ConfigurationSource.Explicit || !shouldThrow)
                         && (!principalEntityType.Builder.CanAddNavigation(
                             navigationToDependentName, navigationToDependent.Value.MemberInfo?.GetMemberType(),
                             configurationSource.Value)))
                {
                    return false;
                }
            }
        }

        var navigationToPrincipalProperty = navigationToPrincipal?.MemberInfo;
        var navigationToDependentProperty = navigationToDependent?.MemberInfo;

        if (!AreCompatible(
                navigationToPrincipalProperty,
                navigationToDependentProperty,
                principalEntityType,
                dependentEntityType,
                shouldThrow,
                out shouldInvert,
                out shouldBeUnique))
        {
            return false;
        }

        if (shouldBeUnique.HasValue
            && Metadata.IsUnique != shouldBeUnique
            && !configurationSource.Overrides(Metadata.GetIsUniqueConfigurationSource()))
        {
            return false;
        }

        var compatibleRelationship = FindCompatibleRelationship(
            principalEntityType,
            dependentEntityType,
            navigationToPrincipal,
            navigationToDependent,
            null,
            null,
            Metadata.GetPrincipalEndConfigurationSource(),
            configurationSource,
            out _,
            out var conflictingRelationshipsFound,
            out var resolvableRelationships);

        if (conflictingRelationshipsFound)
        {
            return false;
        }

        conflictingNavigationsFound = compatibleRelationship != null
            || resolvableRelationships.Any(
                r => (r.Resolution & (Resolution.ResetToDependent | Resolution.ResetToPrincipal | Resolution.Remove)) != 0);

        if (shouldBeUnique == null
            && (Metadata.IsUnique || configurationSource.OverridesStrictly(Metadata.GetIsUniqueConfigurationSource()))
            && ((navigationToDependentProperty != null && shouldInvert != true)
                || (navigationToPrincipalProperty != null && shouldInvert == true)))
        {
            // if new dependent can be both assume single
            shouldBeUnique = true;
        }

        if (shouldInvert == false
            && !conflictingNavigationsFound
            && (principalEntityType != Metadata.PrincipalEntityType
                || dependentEntityType != Metadata.DeclaringEntityType))
        {
            if (navigationToPrincipalProperty != null
                && !IsCompatible(
                    navigationToPrincipalProperty,
                    pointsToPrincipal: true,
                    Metadata.DeclaringEntityType,
                    Metadata.PrincipalEntityType,
                    shouldThrow: false,
                    out _))
            {
                changeRelatedTypes = true;
                return true;
            }

            if (navigationToDependentProperty != null
                && !IsCompatible(
                    navigationToDependentProperty,
                    pointsToPrincipal: false,
                    Metadata.DeclaringEntityType,
                    Metadata.PrincipalEntityType,
                    shouldThrow: false,
                    out _))
            {
                changeRelatedTypes = true;
                return true;
            }
        }

        return true;
    }

    private bool CanRemoveNavigation(bool pointsToPrincipal, ConfigurationSource? configurationSource, bool overrideSameSource = true)
        => pointsToPrincipal
            ? Metadata.DependentToPrincipal == null
            || (configurationSource.Overrides(Metadata.GetDependentToPrincipalConfigurationSource())
                && (overrideSameSource || configurationSource != Metadata.GetDependentToPrincipalConfigurationSource()))
            : Metadata.PrincipalToDependent == null
            || (!Metadata.IsOwnership
                && configurationSource.Overrides(Metadata.GetPrincipalToDependentConfigurationSource())
                && (overrideSameSource || configurationSource != Metadata.GetPrincipalToDependentConfigurationSource()));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool AreCompatible(
        MemberInfo? navigationToPrincipalProperty,
        MemberInfo? navigationToDependentProperty,
        EntityType principalEntityType,
        EntityType dependentEntityType,
        bool shouldThrow,
        out bool? shouldInvert,
        out bool? shouldBeUnique)
    {
        shouldInvert = null;
        shouldBeUnique = null;

        bool? invertedShouldBeUnique = null;
        if (navigationToPrincipalProperty != null
            && !IsCompatible(
                navigationToPrincipalProperty,
                pointsToPrincipal: false,
                principalEntityType,
                dependentEntityType,
                shouldThrow: false,
                out invertedShouldBeUnique))
        {
            shouldInvert = false;
        }

        if (navigationToDependentProperty != null
            && !IsCompatible(
                navigationToDependentProperty,
                pointsToPrincipal: true,
                principalEntityType,
                dependentEntityType,
                shouldThrow: false,
                out _))
        {
            shouldInvert = false;
        }

        if (navigationToPrincipalProperty != null
            && !IsCompatible(
                navigationToPrincipalProperty,
                pointsToPrincipal: true,
                dependentEntityType,
                principalEntityType,
                shouldThrow && shouldInvert != null,
                out _))
        {
            if (shouldInvert != null)
            {
                return false;
            }

            shouldInvert = true;
        }

        if (navigationToDependentProperty != null
            && !IsCompatible(
                navigationToDependentProperty,
                pointsToPrincipal: false,
                dependentEntityType,
                principalEntityType,
                shouldThrow && shouldInvert != null,
                out shouldBeUnique))
        {
            if (shouldInvert != null)
            {
                return false;
            }

            shouldInvert = true;
        }

        if (shouldInvert == true)
        {
            shouldBeUnique = invertedShouldBeUnique;
        }

        return true;
    }

    private static bool IsCompatible(
        MemberInfo navigationMember,
        bool pointsToPrincipal,
        EntityType dependentType,
        EntityType principalType,
        bool shouldThrow,
        out bool? shouldBeUnique)
    {
        shouldBeUnique = null;
        if (!pointsToPrincipal)
        {
            var canBeUnique = Navigation.IsCompatible(
                navigationMember.Name,
                navigationMember,
                principalType,
                dependentType,
                shouldBeCollection: false,
                shouldThrow: false);
            var canBeNonUnique = Navigation.IsCompatible(
                navigationMember.Name,
                navigationMember,
                principalType,
                dependentType,
                shouldBeCollection: true,
                shouldThrow: false);

            if (canBeUnique != canBeNonUnique)
            {
                shouldBeUnique = canBeUnique;
            }
            else if (!canBeUnique)
            {
                if (shouldThrow)
                {
                    Navigation.IsCompatible(
                        navigationMember.Name,
                        navigationMember,
                        principalType,
                        dependentType,
                        shouldBeCollection: null,
                        shouldThrow: true);
                }

                return false;
            }
        }
        else if (!Navigation.IsCompatible(
                     navigationMember.Name,
                     navigationMember,
                     dependentType,
                     principalType,
                     shouldBeCollection: false,
                     shouldThrow))
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
    public virtual InternalForeignKeyBuilder? IsRequired(bool? required, ConfigurationSource configurationSource)
    {
        if (!CanSetIsRequired(required, configurationSource))
        {
            return null;
        }

        if (required == true
            && Metadata.GetPrincipalEndConfigurationSource() == null
            && configurationSource == ConfigurationSource.Explicit)
        {
            Metadata.DeclaringEntityType.Model.ScopedModelDependencies?.Logger.AmbiguousEndRequiredWarning(Metadata);
        }

        IConventionForeignKey? foreignKey = Metadata;

        Metadata.DeclaringEntityType.Model.ConventionDispatcher.Track(
            () => Metadata.SetIsRequired(required, configurationSource), ref foreignKey);

        return foreignKey != null
            ? ((ForeignKey)foreignKey).Builder
            : this;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSetIsRequired(bool? required, ConfigurationSource? configurationSource)
        => Metadata.IsRequired == required
            || configurationSource.Overrides(Metadata.GetIsRequiredConfigurationSource());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalForeignKeyBuilder? IsRequiredDependent(bool? required, ConfigurationSource configurationSource)
    {
        if (!CanSetIsRequiredDependent(required, configurationSource))
        {
            return null;
        }

        if (required == true
            && Metadata.GetPrincipalEndConfigurationSource() == null
            && configurationSource == ConfigurationSource.Explicit)
        {
            throw new InvalidOperationException(
                CoreStrings.AmbiguousEndRequiredDependent(
                    Metadata.Properties.Format(),
                    Metadata.DeclaringEntityType.DisplayName()));
        }

        if (required == true
            && !Metadata.IsUnique)
        {
            IsUnique(null, configurationSource);
        }

        Metadata.SetIsRequiredDependent(required, configurationSource);

        return this;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSetIsRequiredDependent(bool? required, ConfigurationSource? configurationSource)
        => Metadata.IsRequiredDependent == required
            || ((required != true
                    || Metadata.IsUnique
                    || configurationSource!.Value.Overrides(Metadata.GetIsUniqueConfigurationSource()))
                && configurationSource.Overrides(Metadata.GetIsRequiredDependentConfigurationSource()));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalForeignKeyBuilder? IsOwnership(bool? ownership, ConfigurationSource configurationSource)
    {
        if (Metadata.IsOwnership == ownership)
        {
            Metadata.SetIsOwnership(ownership, configurationSource);

            return this;
        }

        if (ownership == null
            || !configurationSource.Overrides(Metadata.GetIsOwnershipConfigurationSource()))
        {
            return null;
        }

        if (!ownership.Value)
        {
            Metadata.SetIsOwnership(ownership: false, configurationSource);

            return this;
        }

        if (!Metadata.DeclaringEntityType.Builder.CanSetIsOwned(true, configurationSource))
        {
            return null;
        }

        var declaringType = Metadata.DeclaringEntityType;
        var newRelationshipBuilder = this;
        var otherOwnership = declaringType.GetDeclaredForeignKeys().SingleOrDefault(fk => fk.IsOwnership);
        var invertedOwnerships = declaringType.GetDeclaredReferencingForeignKeys()
            .Where(fk => fk.IsOwnership && fk.DeclaringEntityType.ClrType == Metadata.PrincipalEntityType.ClrType).ToList();

        if (invertedOwnerships.Any(fk => !configurationSource.Overrides(fk.GetConfigurationSource())))
        {
            return null;
        }

        if (declaringType.HasSharedClrType)
        {
            if (otherOwnership != null
                && !configurationSource.Overrides(otherOwnership.GetConfigurationSource()))
            {
                return null;
            }

            Metadata.SetIsOwnership(ownership: true, configurationSource);
            newRelationshipBuilder = newRelationshipBuilder.OnDelete(DeleteBehavior.Cascade, ConfigurationSource.Convention);

            if (newRelationshipBuilder == null)
            {
                return null;
            }

            if (otherOwnership?.IsInModel == true)
            {
                otherOwnership.DeclaringEntityType.Builder.HasNoRelationship(otherOwnership, configurationSource);
            }

            foreach (var invertedOwnership in invertedOwnerships)
            {
                if (invertedOwnership.IsInModel)
                {
                    invertedOwnership.DeclaringEntityType.Builder.HasNoRelationship(invertedOwnership, configurationSource);
                }
            }

            newRelationshipBuilder.Metadata.DeclaringEntityType.Builder.IsOwned(true, configurationSource);

            return newRelationshipBuilder;
        }

        if (otherOwnership != null)
        {
            Check.DebugAssert(
                Metadata.DeclaringEntityType.IsOwned(),
                $"Expected {Metadata.DeclaringEntityType} to be owned");

            if (!Metadata.GetConfigurationSource().Overrides(ConfigurationSource.Explicit)
                && Metadata.PrincipalEntityType.IsInOwnershipPath(Metadata.DeclaringEntityType.ClrType))
            {
                return null;
            }

            Metadata.SetIsOwnership(ownership: true, configurationSource);

            using var batch = ModelBuilder.Metadata.DelayConventions();

            newRelationshipBuilder = newRelationshipBuilder.OnDelete(DeleteBehavior.Cascade, ConfigurationSource.Convention);
            if (newRelationshipBuilder == null)
            {
                return null;
            }

            foreach (var invertedOwnership in invertedOwnerships)
            {
                invertedOwnership.DeclaringEntityType.Builder.HasNoRelationship(invertedOwnership, configurationSource);
            }

            var fk = newRelationshipBuilder.Metadata;
            fk.DeclaringEntityType.Builder.HasNoRelationship(fk, fk.GetConfigurationSource());

            if (otherOwnership.Builder.MakeDeclaringTypeShared(configurationSource) == null)
            {
                return null;
            }

            var name = Metadata.PrincipalEntityType.GetOwnedName(declaringType.ShortName(), Metadata.PrincipalToDependent!.Name);
            var newEntityType = ModelBuilder.SharedTypeEntity(
                name,
                declaringType.ClrType,
                declaringType.GetConfigurationSource(),
                shouldBeOwned: true)!.Metadata;

            newRelationshipBuilder = newRelationshipBuilder.Attach(newEntityType.Builder)!;

            ModelBuilder.Metadata.ConventionDispatcher.Tracker.Update(
                Metadata, newRelationshipBuilder.Metadata);

            return batch.Run(newRelationshipBuilder);
        }

        using (var batch = ModelBuilder.Metadata.DelayConventions())
        {
            var declaringEntityTypeBuilder =
                newRelationshipBuilder.Metadata.DeclaringEntityType.Builder.IsOwned(true, configurationSource, Metadata);

            Check.DebugAssert(declaringEntityTypeBuilder != null, "Expected declared type to be owned");

            Metadata.SetIsOwnership(true, configurationSource);
            newRelationshipBuilder = newRelationshipBuilder.OnDelete(DeleteBehavior.Cascade, ConfigurationSource.Convention)
                ?? newRelationshipBuilder;

            foreach (var invertedOwnership in invertedOwnerships)
            {
                if (configurationSource.Overrides(invertedOwnership.DeclaringEntityType.GetConfigurationSource()))
                {
                    ModelBuilder.HasNoEntityType(invertedOwnership.DeclaringEntityType, configurationSource);
                }
                else
                {
                    invertedOwnership.DeclaringEntityType.Builder.HasNoRelationship(invertedOwnership, configurationSource);
                }
            }

            return batch.Run(newRelationshipBuilder);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSetIsOwnership(bool? ownership, ConfigurationSource? configurationSource)
        => (Metadata.IsOwnership == ownership || configurationSource.Overrides(Metadata.GetIsOwnershipConfigurationSource()))
            && (ownership != true
                || Metadata.DeclaringEntityType.IsOwned()
                || configurationSource.OverridesStrictly(Metadata.GetConfigurationSource()));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalForeignKeyBuilder? MakeDeclaringTypeShared(ConfigurationSource? configurationSource)
    {
        if (Metadata.DeclaringEntityType.HasSharedClrType)
        {
            return this;
        }

        if (configurationSource == null)
        {
            return null;
        }

        Check.DebugAssert(Metadata.IsOwnership, "Expected an ownership");
        Check.DebugAssert(Metadata.PrincipalToDependent != null, "Expected a navigation to the dependent");

        var name = Metadata.PrincipalEntityType.GetOwnedName(
            Metadata.DeclaringEntityType.ShortName(), Metadata.PrincipalToDependent.Name);
        var newEntityType = ModelBuilder.SharedTypeEntity(
            name,
            Metadata.DeclaringEntityType.ClrType,
            configurationSource.Value,
            shouldBeOwned: true)!.Metadata;

        var newOwnership = newEntityType.GetForeignKeys().SingleOrDefault(fk => fk.IsOwnership);
        if (newOwnership == null)
        {
            return Metadata.IsInModel ? Metadata.Builder : null;
        }

        Check.DebugAssert(!Metadata.IsInModel, "Metadata is in the model");
        ModelBuilder.Metadata.ConventionDispatcher.Tracker.Update(Metadata, newOwnership);
        return newOwnership.Builder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalForeignKeyBuilder? OnDelete(DeleteBehavior? deleteBehavior, ConfigurationSource configurationSource)
    {
        if (!CanSetDeleteBehavior(deleteBehavior, configurationSource))
        {
            return null;
        }

        Metadata.SetDeleteBehavior(deleteBehavior, configurationSource);

        return this;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSetDeleteBehavior(DeleteBehavior? deleteBehavior, ConfigurationSource? configurationSource)
        => Metadata.DeleteBehavior == deleteBehavior || configurationSource.Overrides(Metadata.GetDeleteBehaviorConfigurationSource());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalForeignKeyBuilder? IsUnique(bool? unique, ConfigurationSource configurationSource)
    {
        if (Metadata.IsUnique == unique)
        {
            Metadata.SetIsUnique(unique, configurationSource);

            return this;
        }

        if (!CanSetIsUnique(unique, configurationSource, out var resetToDependent))
        {
            return null;
        }

        if (resetToDependent
            && Metadata.PrincipalToDependent!.GetConfigurationSource() == ConfigurationSource.Explicit)
        {
            throw new InvalidOperationException(
                CoreStrings.UnableToSetIsUnique(
                    unique,
                    Metadata.PrincipalToDependent.PropertyInfo!.Name,
                    Metadata.PrincipalEntityType.DisplayName()));
        }

        using var batch = Metadata.DeclaringEntityType.Model.DelayConventions();

        var builder = this;
        if (resetToDependent)
        {
            builder = builder.HasNavigations(navigationToPrincipal: null, MemberIdentity.None, configurationSource);
            if (builder == null)
            {
                return null;
            }
        }

        if (unique == false
            && Metadata.IsRequiredDependent)
        {
            builder.IsRequiredDependent(null, configurationSource);
        }

        builder.Metadata.SetIsUnique(unique, configurationSource);

        return batch.Run(builder);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSetIsUnique(bool? unique, ConfigurationSource? configurationSource)
        => CanSetIsUnique(unique, configurationSource, out _);

    private bool CanSetIsUnique(bool? unique, ConfigurationSource? configurationSource, out bool resetToDependent)
    {
        resetToDependent = false;
        if (Metadata.IsUnique == unique)
        {
            return true;
        }

        if (!configurationSource.HasValue
            || !configurationSource.Value.Overrides(Metadata.GetIsUniqueConfigurationSource()))
        {
            return false;
        }

        if (unique == false
            && Metadata.IsRequiredDependent
            && !configurationSource.Value.Overrides(Metadata.GetIsRequiredDependentConfigurationSource()))
        {
            return false;
        }

        var navigationMember = Metadata.PrincipalToDependent?.GetIdentifyingMemberInfo();
        if (navigationMember != null
            && !Navigation.IsCompatible(
                Metadata.PrincipalToDependent!.Name,
                navigationMember,
                Metadata.PrincipalEntityType,
                Metadata.DeclaringEntityType,
                !unique,
                shouldThrow: false))
        {
            if (!configurationSource.Value.Overrides(Metadata.GetPrincipalToDependentConfigurationSource()))
            {
                return false;
            }

            resetToDependent = true;
        }

        return true;
    }

    // Note: This will not invert relationships, use RelatedEntityTypes for that
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalForeignKeyBuilder? DependentEntityType(
        EntityType dependentEntityType,
        ConfigurationSource configurationSource)
    {
        Check.NotNull(dependentEntityType, nameof(dependentEntityType));

        var builder = this;
        if (Metadata.DeclaringEntityType.IsAssignableFrom(dependentEntityType))
        {
            if (Metadata.GetPrincipalEndConfigurationSource()?.Overrides(configurationSource) != true)
            {
                Metadata.UpdatePrincipalEndConfigurationSource(configurationSource);

                builder =
                    (InternalForeignKeyBuilder?)ModelBuilder.Metadata.ConventionDispatcher.OnForeignKeyPrincipalEndChanged(builder);
            }

            return builder;
        }

        return dependentEntityType.IsAssignableFrom(Metadata.DeclaringEntityType)
            || configurationSource == ConfigurationSource.Explicit
                ? HasEntityTypes(Metadata.PrincipalEntityType, dependentEntityType, configurationSource)
                : null;
    }

    // Note: This will not invert relationships, use RelatedEntityTypes for that
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalForeignKeyBuilder? PrincipalEntityType(
        EntityType principalEntityType,
        ConfigurationSource configurationSource)
    {
        Check.NotNull(principalEntityType, nameof(principalEntityType));

        var builder = this;
        if (Metadata.PrincipalEntityType.IsAssignableFrom(principalEntityType))
        {
            if (Metadata.GetPrincipalEndConfigurationSource()?.Overrides(configurationSource) != true)
            {
                Metadata.UpdatePrincipalEndConfigurationSource(configurationSource);

                builder =
                    (InternalForeignKeyBuilder?)ModelBuilder.Metadata.ConventionDispatcher.OnForeignKeyPrincipalEndChanged(builder);
            }

            return builder;
        }

        return principalEntityType.IsAssignableFrom(Metadata.PrincipalEntityType)
            || configurationSource == ConfigurationSource.Explicit
                ? HasEntityTypes(principalEntityType, Metadata.DeclaringEntityType, configurationSource)
                : null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalForeignKeyBuilder? HasEntityTypes(
        EntityType principalEntityType,
        EntityType dependentEntityType,
        ConfigurationSource configurationSource)
        => HasEntityTypes(principalEntityType, dependentEntityType, configurationSource, configurationSource);

    private InternalForeignKeyBuilder? HasEntityTypes(
        EntityType principalEntityType,
        EntityType dependentEntityType,
        ConfigurationSource? principalEndConfigurationSource,
        ConfigurationSource configurationSource)
    {
        if ((Metadata.PrincipalEntityType == principalEntityType
                && Metadata.DeclaringEntityType == dependentEntityType)
            || (Metadata.PrincipalEntityType == principalEntityType.LeastDerivedType(Metadata.PrincipalEntityType)
                && Metadata.DeclaringEntityType == dependentEntityType.LeastDerivedType(Metadata.DeclaringEntityType)))
        {
            if (!principalEndConfigurationSource.HasValue
                || Metadata.GetPrincipalEndConfigurationSource()?.Overrides(principalEndConfigurationSource) == true)
            {
                return this;
            }

            Metadata.UpdatePrincipalEndConfigurationSource(principalEndConfigurationSource.Value);

            principalEntityType.UpdateConfigurationSource(configurationSource);
            dependentEntityType.UpdateConfigurationSource(configurationSource);

            return (InternalForeignKeyBuilder?)ModelBuilder.Metadata.ConventionDispatcher.OnForeignKeyPrincipalEndChanged(this);
        }

        if (!CanSetRelatedTypes(
                principalEntityType,
                dependentEntityType,
                strictPrincipal: true,
                navigationToPrincipal: null,
                navigationToDependent: null,
                configurationSource,
                shouldThrow: true,
                out var shouldInvert,
                out var shouldResetToPrincipal,
                out var shouldResetToDependent,
                out var shouldResetPrincipalProperties,
                out var shouldResetDependentProperties,
                out var shouldBeUnique)
            && configurationSource != ConfigurationSource.Explicit)
        {
            return null;
        }

        var dependentProperties = (IReadOnlyList<Property>)[];
        var principalProperties = (IReadOnlyList<Property>)[];
        var builder = this;
        if (shouldInvert)
        {
            Check.DebugAssert(
                configurationSource.Overrides(Metadata.GetPropertiesConfigurationSource()),
                "configurationSource does not override Metadata.GetPropertiesConfigurationSource");

            Check.DebugAssert(
                configurationSource.Overrides(Metadata.GetPrincipalKeyConfigurationSource()),
                "configurationSource does not override Metadata.GetPrincipalKeyConfigurationSource");

            principalEntityType = principalEntityType.LeastDerivedType(Metadata.DeclaringEntityType)!;
            dependentEntityType = dependentEntityType.LeastDerivedType(Metadata.PrincipalEntityType)!;
        }
        else
        {
            principalEntityType = principalEntityType.LeastDerivedType(Metadata.PrincipalEntityType)!;
            dependentEntityType = dependentEntityType.LeastDerivedType(Metadata.DeclaringEntityType)!;

            dependentProperties = shouldResetDependentProperties
                ? dependentProperties
                : ((Metadata.GetPropertiesConfigurationSource()?.Overrides(configurationSource) ?? false)
                    ? dependentEntityType.Builder.GetActualProperties(Metadata.Properties, configurationSource)
                    : null);

            principalProperties = shouldResetPrincipalProperties
                ? principalProperties
                : ((Metadata.GetPrincipalKeyConfigurationSource()?.Overrides(configurationSource) ?? false)
                    ? principalEntityType.Builder.GetActualProperties(Metadata.PrincipalKey.Properties, configurationSource)
                    : null);
        }

        return builder.ReplaceForeignKey(
            configurationSource,
            principalEntityType.Builder,
            dependentEntityType.Builder,
            shouldResetToPrincipal ? MemberIdentity.None : null,
            shouldResetToDependent ? MemberIdentity.None : null,
            dependentProperties,
            principalProperties: principalProperties,
            isUnique: shouldBeUnique,
            removeCurrent: true,
            principalEndConfigurationSource: principalEndConfigurationSource,
            oldRelationshipInverted: shouldInvert);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSetEntityTypes(
        EntityType principalEntityType,
        EntityType dependentEntityType,
        ConfigurationSource? configurationSource)
        => CanSetEntityTypes(
            principalEntityType,
            dependentEntityType,
            configurationSource,
            out _,
            out _);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSetEntityTypes(
        EntityType principalEntityType,
        EntityType dependentEntityType,
        ConfigurationSource? configurationSource,
        out bool shouldResetToPrincipal,
        out bool shouldResetToDependent)
        => CanSetRelatedTypes(
            principalEntityType,
            dependentEntityType,
            strictPrincipal: true,
            navigationToPrincipal: null,
            navigationToDependent: null,
            configurationSource,
            shouldThrow: false,
            out _,
            out shouldResetToPrincipal,
            out shouldResetToDependent,
            out _,
            out _,
            out _);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanInvert(
        IReadOnlyList<Property>? newForeignKeyProperties,
        ConfigurationSource? configurationSource)
        => configurationSource.Overrides(Metadata.GetPrincipalEndConfigurationSource())
            && ((newForeignKeyProperties == null)
                || CanSetForeignKey(newForeignKeyProperties, Metadata.PrincipalEntityType, configurationSource));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalForeignKeyBuilder? ReuniquifyImplicitProperties(bool force)
    {
        if (!force
            && (Metadata.GetPropertiesConfigurationSource() != null
                || !Metadata.DeclaringEntityType.Builder
                    .ShouldReuniquifyTemporaryProperties(Metadata)))
        {
            return Metadata.Builder;
        }

        var relationshipBuilder = this;
        using var batch = Metadata.DeclaringEntityType.Model.DelayConventions();

        var temporaryProperties = Metadata.Properties.Where(
            p => (p.IsShadowProperty() || p.DeclaringType.IsPropertyBag && p.IsIndexerProperty())
                && ConfigurationSource.Convention.Overrides(p.GetConfigurationSource())).ToList();

        var keysToDetach = temporaryProperties.SelectMany(
                p => p.GetContainingKeys()
                    .Where(k => ConfigurationSource.Convention.Overrides(k.GetConfigurationSource())))
            .Distinct().ToList();

        List<RelationshipSnapshot>? detachedRelationships = null;
        foreach (var key in keysToDetach)
        {
            foreach (var referencingForeignKey in key.GetReferencingForeignKeys().ToList())
            {
                detachedRelationships ??= [];

                detachedRelationships.Add(InternalEntityTypeBuilder.DetachRelationship(referencingForeignKey));
            }
        }

        var detachedKeys = InternalEntityTypeBuilder.DetachKeys(keysToDetach);

        var detachedIndexes = InternalEntityTypeBuilder.DetachIndexes(
            temporaryProperties.SelectMany(p => p.GetContainingIndexes()).Distinct());

        relationshipBuilder = relationshipBuilder.HasForeignKey((IReadOnlyList<Property>?)null, ConfigurationSource.Convention)!;

        if (detachedIndexes != null)
        {
            foreach (var indexBuilderTuple in detachedIndexes)
            {
                indexBuilderTuple.Attach(indexBuilderTuple.Metadata.DeclaringEntityType.Builder);
            }
        }

        if (detachedKeys != null)
        {
            foreach (var (internalKeyBuilder, configurationSource) in detachedKeys)
            {
                internalKeyBuilder.Attach(Metadata.DeclaringEntityType.GetRootType().Builder, configurationSource);
            }
        }

        if (detachedRelationships != null)
        {
            foreach (var detachedRelationship in detachedRelationships)
            {
                detachedRelationship.Attach();
            }
        }

        return batch.Run(relationshipBuilder);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalForeignKeyBuilder? HasForeignKey(
        IReadOnlyList<MemberInfo>? properties,
        ConfigurationSource configurationSource)
        => HasForeignKey(properties, Metadata.DeclaringEntityType, configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalForeignKeyBuilder? HasForeignKey(
        IReadOnlyList<string>? propertyNames,
        ConfigurationSource configurationSource)
        => HasForeignKey(propertyNames, Metadata.DeclaringEntityType, configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalForeignKeyBuilder? HasForeignKey(
        IReadOnlyList<MemberInfo>? properties,
        EntityType dependentEntityType,
        ConfigurationSource configurationSource)
    {
        using var batch = Metadata.DeclaringEntityType.Model.DelayConventions();

        var relationship = HasForeignKey(
            dependentEntityType.Builder.GetOrCreateProperties(properties, configurationSource),
            dependentEntityType,
            configurationSource);

        if (relationship == null)
        {
            return null;
        }

        return (InternalForeignKeyBuilder?)batch.Run(relationship.Metadata)?.Builder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalForeignKeyBuilder? HasForeignKey(
        IReadOnlyList<string>? propertyNames,
        EntityType dependentEntityType,
        ConfigurationSource configurationSource)
    {
        using var batch = Metadata.DeclaringEntityType.Model.DelayConventions();

        var useDefaultType = Metadata.GetPrincipalKeyConfigurationSource() == null
            || (propertyNames != null
                && Metadata.PrincipalKey.Properties.Count != propertyNames.Count);
        var relationship = HasForeignKey(
            dependentEntityType.Builder.GetOrCreateProperties(
                propertyNames,
                configurationSource,
                Metadata.PrincipalKey.Properties,
                Metadata.GetIsRequiredConfigurationSource() != null && Metadata.IsRequired,
                useDefaultType),
            dependentEntityType,
            configurationSource);

        if (relationship == null)
        {
            return null;
        }

        return (InternalForeignKeyBuilder?)batch.Run(relationship.Metadata)?.Builder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalForeignKeyBuilder? HasForeignKey(
        IReadOnlyList<Property>? properties,
        ConfigurationSource configurationSource)
        => HasForeignKey(properties, Metadata.DeclaringEntityType, configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalForeignKeyBuilder? HasForeignKey(
        IReadOnlyList<Property>? properties,
        EntityType dependentEntityType,
        ConfigurationSource configurationSource)
    {
        if (properties == null)
        {
            return !configurationSource.Overrides(Metadata.GetPropertiesConfigurationSource())
                ? null
                : ReplaceForeignKey(
                    configurationSource,
                    dependentProperties: []);
        }

        properties = dependentEntityType.Builder.GetActualProperties(properties, configurationSource)!;
        if (Metadata.Properties.SequenceEqual(properties))
        {
            Metadata.UpdateConfigurationSource(configurationSource);
            Metadata.UpdatePropertiesConfigurationSource(configurationSource);

            var builder = this;
            if (!Metadata.IsSelfReferencing()
                && Metadata.GetPrincipalEndConfigurationSource()?.Overrides(configurationSource) != true)
            {
                Metadata.UpdatePrincipalEndConfigurationSource(configurationSource);

                builder =
                    (InternalForeignKeyBuilder?)ModelBuilder.Metadata.ConventionDispatcher.OnForeignKeyPrincipalEndChanged(builder);
            }

            return builder;
        }

        if (!CanSetForeignKey(
                properties, dependentEntityType, configurationSource, out var resetPrincipalKey))
        {
            return null;
        }

        return ReplaceForeignKey(
            configurationSource,
            dependentEntityTypeBuilder: dependentEntityType.Builder,
            dependentProperties: properties,
            principalProperties: resetPrincipalKey ? [] : null,
            principalEndConfigurationSource: configurationSource,
            removeCurrent: !Property.AreCompatible(properties, Metadata.DeclaringEntityType));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSetForeignKey(IReadOnlyList<string>? propertyNames, ConfigurationSource? configurationSource)
    {
        if (propertyNames is not null
            && ((IReadOnlyEntityType)Metadata.DeclaringEntityType).FindProperties(propertyNames) is IReadOnlyList<IReadOnlyProperty>
            properties)
        {
            return CanSetForeignKey(
                properties,
                dependentEntityType: null,
                configurationSource,
                out _);
        }

        return configurationSource.Overrides(Metadata.GetPropertiesConfigurationSource());
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSetForeignKey(IReadOnlyList<Property>? properties, ConfigurationSource? configurationSource)
        => CanSetForeignKey(
            properties,
            dependentEntityType: null,
            configurationSource,
            out _);

    private bool CanSetForeignKey(
        IReadOnlyList<IReadOnlyProperty>? properties,
        EntityType? dependentEntityType,
        ConfigurationSource? configurationSource,
        bool overrideSameSource = true)
        => CanSetForeignKey(
            properties,
            dependentEntityType,
            configurationSource,
            out _,
            overrideSameSource);

    private bool CanSetForeignKey(
        IReadOnlyList<IReadOnlyProperty>? properties,
        EntityType? dependentEntityType,
        ConfigurationSource? configurationSource,
        out bool resetPrincipalKey,
        bool overrideSameSource = true)
    {
        resetPrincipalKey = false;
        return properties != null
            && Metadata.Properties.SequenceEqual(properties)
            || CanSetForeignKey(
                properties,
                dependentEntityType,
                Metadata.PrincipalKey.Properties,
                Metadata.PrincipalEntityType,
                configurationSource,
                out resetPrincipalKey,
                overrideSameSource);
    }

    private bool CanSetForeignKey(
        IReadOnlyList<IReadOnlyProperty>? properties,
        EntityType? dependentEntityType,
        IReadOnlyList<Property> principalKeyProperties,
        EntityType principalEntityType,
        ConfigurationSource? configurationSource,
        out bool resetPrincipalKey,
        bool overrideSameSource = true)
    {
        resetPrincipalKey = false;

        if (!configurationSource.Overrides(Metadata.GetPropertiesConfigurationSource())
            || (!overrideSameSource && configurationSource == Metadata.GetPropertiesConfigurationSource()))
        {
            return false;
        }

        if (properties == null)
        {
            return true;
        }

        dependentEntityType ??= Metadata.DeclaringEntityType;

        // FKs are not allowed to use properties from inherited keys since this could result in an ambiguous value space
        if (dependentEntityType.BaseType != null
            && !principalEntityType.IsAssignableFrom(dependentEntityType)
            && configurationSource != ConfigurationSource.Explicit // let it throw for explicit
            && properties.Any(p => p.GetContainingKeys().Any(k => k.DeclaringEntityType != dependentEntityType)))
        {
            return false;
        }

        if ((dependentEntityType != Metadata.DeclaringEntityType
                && dependentEntityType == Metadata.PrincipalEntityType) // Check if inverted
            || (properties.Count != 0
                && !ForeignKey.AreCompatible(
                    principalKeyProperties,
                    properties,
                    principalEntityType,
                    dependentEntityType,
                    shouldThrow: false)))
        {
            if (!configurationSource.HasValue
                || !configurationSource.Overrides(Metadata.GetPrincipalKeyConfigurationSource()))
            {
                return false;
            }

            resetPrincipalKey = true;
        }

        return true;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalForeignKeyBuilder? HasPrincipalKey(
        IReadOnlyList<MemberInfo>? members,
        ConfigurationSource configurationSource)
    {
        using var batch = Metadata.DeclaringEntityType.Model.DelayConventions();

        var relationship = HasPrincipalKey(
            Metadata.PrincipalEntityType.Builder.GetOrCreateProperties(members, configurationSource),
            configurationSource);

        return relationship is null ? null : (InternalForeignKeyBuilder?)batch.Run(relationship.Metadata)?.Builder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalForeignKeyBuilder? HasPrincipalKey(
        IReadOnlyList<string>? propertyNames,
        ConfigurationSource configurationSource)
    {
        using var batch = Metadata.DeclaringEntityType.Model.DelayConventions();

        var relationship = HasPrincipalKey(
            Metadata.PrincipalEntityType.Builder.GetOrCreateProperties(propertyNames, configurationSource),
            configurationSource);

        return relationship is null ? null : (InternalForeignKeyBuilder?)batch.Run(relationship.Metadata)?.Builder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalForeignKeyBuilder? HasPrincipalKey(
        IReadOnlyList<Property>? properties,
        ConfigurationSource configurationSource)
    {
        if (properties == null)
        {
            return !configurationSource.Overrides(Metadata.GetPrincipalKeyConfigurationSource())
                ? null
                : ReplaceForeignKey(
                    configurationSource,
                    principalProperties: []);
        }

        properties = Metadata.PrincipalEntityType.Builder.GetActualProperties(properties, configurationSource)!;

        if (Metadata.PrincipalKey.Properties.SequenceEqual(properties))
        {
            Metadata.UpdateConfigurationSource(configurationSource);
            Metadata.UpdatePrincipalKeyConfigurationSource(configurationSource);

            var builder = this;
            if (!Metadata.IsSelfReferencing()
                && Metadata.GetPrincipalEndConfigurationSource()?.Overrides(configurationSource) != true)
            {
                Metadata.UpdatePrincipalEndConfigurationSource(configurationSource);

                builder =
                    (InternalForeignKeyBuilder?)ModelBuilder.Metadata.ConventionDispatcher.OnForeignKeyPrincipalEndChanged(builder);
            }

            return builder;
        }

        if (!CanSetPrincipalKey(properties, configurationSource, out var resetDependent, out var oldNameDependentProperties))
        {
            return null;
        }

        return ReplaceForeignKey(
            configurationSource,
            principalProperties: properties,
            dependentProperties: resetDependent ? [] : null,
            principalEndConfigurationSource: configurationSource,
            oldNameDependentProperties: oldNameDependentProperties,
            removeCurrent: oldNameDependentProperties != null);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSetPrincipalKey(IReadOnlyList<string>? propertyNames, ConfigurationSource? configurationSource)
    {
        if (propertyNames is not null
            && ((IReadOnlyEntityType)Metadata.PrincipalEntityType).FindProperties(propertyNames) is IReadOnlyList<IReadOnlyProperty>
            properties)
        {
            return CanSetPrincipalKey(
                properties,
                configurationSource,
                out _,
                out _);
        }

        return configurationSource.Overrides(Metadata.GetPrincipalKeyConfigurationSource());
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSetPrincipalKey(IReadOnlyList<Property>? properties, ConfigurationSource? configurationSource)
        => CanSetPrincipalKey(
            properties,
            configurationSource,
            out _,
            out _);

    private bool CanSetPrincipalKey(
        IReadOnlyList<IReadOnlyProperty>? properties,
        ConfigurationSource? configurationSource,
        out bool resetDependent,
        out IReadOnlyList<Property>? oldNameDependentProperties)
    {
        resetDependent = false;
        oldNameDependentProperties = null;

        if (properties is not null && Metadata.PrincipalKey.Properties.SequenceEqual(properties))
        {
            return true;
        }

        if (!configurationSource.Overrides(Metadata.GetPrincipalKeyConfigurationSource()))
        {
            return false;
        }

        if (properties == null)
        {
            return true;
        }

        if (!ForeignKey.AreCompatible(
                properties,
                Metadata.Properties,
                Metadata.PrincipalEntityType,
                Metadata.DeclaringEntityType,
                shouldThrow: false))
        {
            if (!configurationSource?.Overrides(Metadata.GetPropertiesConfigurationSource()) == true)
            {
                return false;
            }

            if (Metadata.GetPropertiesConfigurationSource().Overrides(ConfigurationSource.DataAnnotation)
                && Metadata.Properties.All(
                    p => ConfigurationSource.Convention.Overrides(p.GetTypeConfigurationSource())
                        && (p.IsShadowProperty() || p.IsIndexerProperty())))
            {
                oldNameDependentProperties = Metadata.Properties;
            }

            resetDependent = true;
        }

        return true;
    }

    private InternalForeignKeyBuilder? ReplaceForeignKey(
        ConfigurationSource configurationSource,
        InternalEntityTypeBuilder? principalEntityTypeBuilder = null,
        InternalEntityTypeBuilder? dependentEntityTypeBuilder = null,
        MemberIdentity? navigationToPrincipal = null,
        MemberIdentity? navigationToDependent = null,
        IReadOnlyList<Property>? dependentProperties = null,
        IReadOnlyList<Property>? oldNameDependentProperties = null,
        IReadOnlyList<Property>? principalProperties = null,
        bool? isUnique = null,
        bool? isRequired = null,
        bool? isRequiredDependent = null,
        bool? isOwnership = null,
        DeleteBehavior? deleteBehavior = null,
        bool removeCurrent = false,
        ConfigurationSource? principalEndConfigurationSource = null,
        bool oldRelationshipInverted = false)
    {
        if (oldRelationshipInverted
            && Metadata.IsRequired
            && Metadata.GetIsRequiredConfigurationSource() == ConfigurationSource.Explicit
            && Metadata.GetPrincipalEndConfigurationSource() == null)
        {
            throw new InvalidOperationException(
                CoreStrings.AmbiguousEndRequiredInverted(
                    Metadata.Properties.Format(),
                    Metadata.DeclaringEntityType.DisplayName(),
                    Metadata.PrincipalEntityType.DisplayName()));
        }

        principalEntityTypeBuilder ??= (oldRelationshipInverted
            ? Metadata.DeclaringEntityType.Builder
            : Metadata.PrincipalEntityType.Builder);
        dependentEntityTypeBuilder ??= (oldRelationshipInverted
            ? Metadata.PrincipalEntityType.Builder
            : Metadata.DeclaringEntityType.Builder);

        if (navigationToPrincipal == null)
        {
            if (oldRelationshipInverted)
            {
                navigationToPrincipal = Metadata.GetPrincipalToDependentConfigurationSource()?.Overrides(configurationSource)
                    ?? false
                        ? Metadata.PrincipalToDependent.CreateMemberIdentity()
                        : navigationToPrincipal;
            }
            else
            {
                navigationToPrincipal = Metadata.GetDependentToPrincipalConfigurationSource()?.Overrides(configurationSource)
                    ?? false
                        ? Metadata.DependentToPrincipal.CreateMemberIdentity()
                        : navigationToPrincipal;
            }
        }

        if (navigationToDependent == null)
        {
            if (oldRelationshipInverted)
            {
                navigationToDependent = Metadata.GetDependentToPrincipalConfigurationSource()?.Overrides(configurationSource)
                    ?? false
                        ? Metadata.DependentToPrincipal.CreateMemberIdentity()
                        : navigationToDependent;
            }
            else
            {
                navigationToDependent = Metadata.GetPrincipalToDependentConfigurationSource()?.Overrides(configurationSource)
                    ?? false
                        ? Metadata.PrincipalToDependent.CreateMemberIdentity()
                        : navigationToDependent;
            }
        }

        dependentProperties ??= ((Metadata.GetPropertiesConfigurationSource()?.Overrides(configurationSource) ?? false)
            && !oldRelationshipInverted
                ? Metadata.Properties
                : null);

        principalProperties ??= ((Metadata.GetPrincipalKeyConfigurationSource()?.Overrides(configurationSource) ?? false)
            && !oldRelationshipInverted
                ? Metadata.PrincipalKey.Properties
                : null);

        isUnique ??= ((Metadata.GetIsUniqueConfigurationSource()?.Overrides(configurationSource) ?? false)
            ? Metadata.IsUnique
            : null);

        isRequired ??= !oldRelationshipInverted
            ? ((Metadata.GetIsRequiredConfigurationSource()?.Overrides(configurationSource) ?? false)
                ? Metadata.IsRequired
                : null)
            : ((Metadata.GetIsRequiredDependentConfigurationSource()?.Overrides(ConfigurationSource.Explicit) ?? false)
                ? Metadata.IsRequiredDependent
                : null);

        isRequiredDependent ??= !oldRelationshipInverted
            ? ((Metadata.GetIsRequiredDependentConfigurationSource()?.Overrides(configurationSource) ?? false)
                ? Metadata.IsRequiredDependent
                : null)
            : ((Metadata.GetIsRequiredConfigurationSource()?.Overrides(ConfigurationSource.Explicit) ?? false)
                ? Metadata.IsRequired
                : null);

        isOwnership ??= !oldRelationshipInverted
            && (Metadata.GetIsOwnershipConfigurationSource()?.Overrides(configurationSource) ?? false)
                ? Metadata.IsOwnership
                : null;

        deleteBehavior ??= ((Metadata.GetDeleteBehaviorConfigurationSource()?.Overrides(configurationSource) ?? false)
            ? Metadata.DeleteBehavior
            : null);

        principalEndConfigurationSource ??= (principalEntityTypeBuilder.Metadata != dependentEntityTypeBuilder.Metadata
            && ((principalProperties?.Count > 0)
                || (dependentProperties?.Count > 0)
                || (navigationToDependent != null && isUnique == false)
                || isOwnership == true)
                ? configurationSource
                : null);
        principalEndConfigurationSource = principalEndConfigurationSource.Max(Metadata.GetPrincipalEndConfigurationSource());

        return ReplaceForeignKey(
            principalEntityTypeBuilder,
            dependentEntityTypeBuilder,
            navigationToPrincipal,
            navigationToDependent,
            dependentProperties,
            oldNameDependentProperties,
            principalProperties,
            isUnique,
            isRequired,
            isRequiredDependent,
            isOwnership,
            deleteBehavior,
            removeCurrent,
            oldRelationshipInverted,
            principalEndConfigurationSource,
            configurationSource);
    }

    private InternalForeignKeyBuilder? ReplaceForeignKey(
        InternalEntityTypeBuilder principalEntityTypeBuilder,
        InternalEntityTypeBuilder dependentEntityTypeBuilder,
        MemberIdentity? navigationToPrincipal,
        MemberIdentity? navigationToDependent,
        IReadOnlyList<Property>? dependentProperties,
        IReadOnlyList<Property>? oldNameDependentProperties,
        IReadOnlyList<Property>? principalProperties,
        bool? isUnique,
        bool? isRequired,
        bool? isRequiredDependent,
        bool? isOwnership,
        DeleteBehavior? deleteBehavior,
        bool removeCurrent,
        bool oldRelationshipInverted,
        ConfigurationSource? principalEndConfigurationSource,
        ConfigurationSource configurationSource)
    {
        Check.NotNull(principalEntityTypeBuilder, nameof(principalEntityTypeBuilder));
        Check.NotNull(dependentEntityTypeBuilder, nameof(dependentEntityTypeBuilder));

        if (configurationSource == ConfigurationSource.Explicit
            && principalEntityTypeBuilder.Metadata.IsKeyless)
        {
            throw new InvalidOperationException(
                CoreStrings.PrincipalKeylessType(
                    principalEntityTypeBuilder.Metadata.DisplayName(),
                    principalEntityTypeBuilder.Metadata.DisplayName()
                    + (navigationToDependent?.Name == null
                        ? ""
                        : "." + navigationToDependent.Value.Name),
                    dependentEntityTypeBuilder.Metadata.DisplayName()
                    + (navigationToPrincipal?.Name == null
                        ? ""
                        : "." + navigationToPrincipal.Value.Name)));
        }

        Check.DebugAssert(
            navigationToPrincipal?.Name == null
            || navigationToPrincipal.Value.MemberInfo != null
            || dependentEntityTypeBuilder.Metadata.ClrType == Model.DefaultPropertyBagType,
            "Principal navigation check failed");

        Check.DebugAssert(
            navigationToDependent?.Name == null
            || navigationToDependent.Value.MemberInfo != null
            || principalEntityTypeBuilder.Metadata.ClrType == Model.DefaultPropertyBagType,
            "Dependent navigation check failed");

        Check.DebugAssert(
            AreCompatible(
                principalEntityTypeBuilder.Metadata,
                dependentEntityTypeBuilder.Metadata,
                navigationToPrincipal?.MemberInfo,
                navigationToDependent?.MemberInfo,
                dependentProperties?.Count > 0 ? dependentProperties : null,
                principalProperties?.Count > 0 ? principalProperties : null,
                isUnique,
                configurationSource),
            "Compatibility check failed");

        Check.DebugAssert(
            oldNameDependentProperties == null || (dependentProperties?.Count ?? 0) == 0,
            "Dependent properties check failed");

        Check.DebugAssert(
            removeCurrent
            || !Metadata.IsInModel
            || (Metadata.PrincipalEntityType.IsAssignableFrom(principalEntityTypeBuilder.Metadata)
                && Metadata.DeclaringEntityType.IsAssignableFrom(dependentEntityTypeBuilder.Metadata)),
            "Entity type check failed");

        using var batch = Metadata.DeclaringEntityType.Model.DelayConventions();

        var referencingSkipNavigations = Metadata.ReferencingSkipNavigations
            ?.Select(n => (Navigation: n, ConfigurationSource: n.GetForeignKeyConfigurationSource()!.Value)).ToList();

        var newRelationshipBuilder = GetOrCreateRelationshipBuilder(
            principalEntityTypeBuilder.Metadata,
            dependentEntityTypeBuilder.Metadata,
            navigationToPrincipal,
            navigationToDependent,
            dependentProperties?.Count > 0 ? dependentProperties : null,
            oldNameDependentProperties,
            principalProperties?.Count > 0 ? principalProperties : null,
            isRequired,
            removeCurrent,
            principalEndConfigurationSource,
            configurationSource,
            out var existingRelationshipInverted);

        if (newRelationshipBuilder == null)
        {
            return null;
        }

        var initialPrincipalEndConfigurationSource = newRelationshipBuilder.Metadata.GetPrincipalEndConfigurationSource();

        var strictPrincipal = principalEndConfigurationSource.HasValue
            && principalEndConfigurationSource.Value.Overrides(initialPrincipalEndConfigurationSource);
        if (existingRelationshipInverted == true
            && !strictPrincipal)
        {
            oldRelationshipInverted = !oldRelationshipInverted;
            existingRelationshipInverted = false;

            (principalEntityTypeBuilder, dependentEntityTypeBuilder) = (dependentEntityTypeBuilder, principalEntityTypeBuilder);
            (navigationToPrincipal, navigationToDependent) = (navigationToDependent, navigationToPrincipal);

            dependentProperties = null;
            principalProperties = null;
        }

        var oldNavigationToPrincipal = oldRelationshipInverted
            ? Metadata.PrincipalToDependent
            : Metadata.DependentToPrincipal;
        var oldNavigationToDependent = oldRelationshipInverted
            ? Metadata.DependentToPrincipal
            : Metadata.PrincipalToDependent;

        var oldToPrincipalConfigurationSource = oldRelationshipInverted
            ? Metadata.GetPrincipalToDependentConfigurationSource()
            : Metadata.GetDependentToPrincipalConfigurationSource();
        var oldToDependentConfigurationSource = oldRelationshipInverted
            ? Metadata.GetDependentToPrincipalConfigurationSource()
            : Metadata.GetPrincipalToDependentConfigurationSource();

        var newRelationshipConfigurationSource = Metadata.GetConfigurationSource();
        if ((dependentProperties?.Count > 0)
            || navigationToPrincipal?.Name != null
            || navigationToDependent?.Name != null)
        {
            newRelationshipConfigurationSource = newRelationshipConfigurationSource.Max(configurationSource);
        }

        newRelationshipBuilder.Metadata.UpdateConfigurationSource(newRelationshipConfigurationSource);

        var resetToPrincipal = newRelationshipBuilder.Metadata.DependentToPrincipal != null
            && ((existingRelationshipInverted == false
                    && navigationToPrincipal != null
                    && navigationToPrincipal.Value.Name
                    != newRelationshipBuilder.Metadata.DependentToPrincipal.Name)
                || (existingRelationshipInverted == true
                    && navigationToDependent != null
                    && navigationToDependent.Value.Name
                    != newRelationshipBuilder.Metadata.DependentToPrincipal.Name));

        var resetToDependent = newRelationshipBuilder.Metadata.PrincipalToDependent != null
            && ((existingRelationshipInverted == false
                    && navigationToDependent != null
                    && navigationToDependent.Value.Name
                    != newRelationshipBuilder.Metadata.PrincipalToDependent.Name)
                || (existingRelationshipInverted == true
                    && navigationToPrincipal != null
                    && navigationToPrincipal.Value.Name
                    != newRelationshipBuilder.Metadata.PrincipalToDependent.Name));

        if (resetToPrincipal
            || resetToDependent)
        {
            newRelationshipBuilder = newRelationshipBuilder.HasNavigations(
                    resetToPrincipal ? MemberIdentity.None : null,
                    resetToDependent ? MemberIdentity.None : null,
                    configurationSource)
                ?? newRelationshipBuilder;
        }

        newRelationshipBuilder = newRelationshipBuilder.HasEntityTypes(
                principalEntityTypeBuilder.Metadata,
                dependentEntityTypeBuilder.Metadata,
                principalEndConfigurationSource,
                configurationSource)
            ?? newRelationshipBuilder;

        dependentProperties = oldNameDependentProperties ?? dependentProperties;
        if (dependentProperties != null
            || principalProperties != null)
        {
            var shouldSetProperties = false;
            ConfigurationSource? foreignKeyPropertiesConfigurationSource = null;
            if (dependentProperties != null)
            {
                dependentProperties = dependentEntityTypeBuilder.GetActualProperties(dependentProperties, configurationSource)!;

                foreignKeyPropertiesConfigurationSource = configurationSource;
                if (PropertyListComparer.Instance.Equals(Metadata.Properties, dependentProperties)
                    && !oldRelationshipInverted)
                {
                    foreignKeyPropertiesConfigurationSource =
                        foreignKeyPropertiesConfigurationSource.Max(Metadata.GetPropertiesConfigurationSource());
                }

                if (foreignKeyPropertiesConfigurationSource.HasValue)
                {
                    if (newRelationshipBuilder.Metadata.Properties.SequenceEqual(dependentProperties))
                    {
                        var updated = newRelationshipBuilder.HasForeignKey(
                            dependentProperties, foreignKeyPropertiesConfigurationSource.Value);

                        Check.DebugAssert(updated == newRelationshipBuilder, "updated != newRelationshipBuilder");
                    }
                    else if (dependentProperties.Count > 0
                             || (!removeCurrent
                                 && Metadata == newRelationshipBuilder.Metadata))
                    {
                        shouldSetProperties = true;
                    }
                }
            }

            ConfigurationSource? principalKeyConfigurationSource = null;
            if (principalProperties != null)
            {
                principalProperties = principalEntityTypeBuilder.GetActualProperties(principalProperties, configurationSource)!;

                principalKeyConfigurationSource = configurationSource;
                if (PropertyListComparer.Instance.Equals(
                        principalProperties, newRelationshipBuilder.Metadata.PrincipalKey.Properties)
                    && !oldRelationshipInverted)
                {
                    principalKeyConfigurationSource =
                        principalKeyConfigurationSource.Max(Metadata.GetPrincipalKeyConfigurationSource());
                }

                if (principalKeyConfigurationSource.HasValue)
                {
                    if (newRelationshipBuilder.Metadata.PrincipalKey.Properties.SequenceEqual(principalProperties))
                    {
                        var updated = newRelationshipBuilder.HasPrincipalKey(
                            principalProperties, principalKeyConfigurationSource.Value);

                        Check.DebugAssert(updated == newRelationshipBuilder, "updated != newRelationshipBuilder");
                    }
                    else if (principalProperties.Count > 0
                             || (!removeCurrent
                                 && Metadata == newRelationshipBuilder.Metadata))
                    {
                        shouldSetProperties = true;
                    }
                }
            }

            if (shouldSetProperties)
            {
                Key? principalKey = null;
                if (principalProperties != null
                    && principalProperties.Count != 0)
                {
                    principalKey = principalEntityTypeBuilder.Metadata.GetRootType().Builder
                        .HasKey(principalProperties, configurationSource)!.Metadata;
                }

                var foreignKey = newRelationshipBuilder.Metadata;
                newRelationshipBuilder = foreignKey.DeclaringEntityType.Builder.UpdateForeignKey(
                    foreignKey,
                    dependentProperties?.Count == 0 ? null : dependentProperties,
                    principalKey,
                    navigationToPrincipal?.Name
                    ?? referencingSkipNavigations?.FirstOrDefault().Navigation?.Inverse?.Name,
                    isRequired,
                    configurationSource: null);
                if (newRelationshipBuilder == null)
                {
                    return null;
                }

                if (foreignKeyPropertiesConfigurationSource != null
                    && dependentProperties?.Count != 0)
                {
                    newRelationshipBuilder.Metadata.UpdatePropertiesConfigurationSource(
                        foreignKeyPropertiesConfigurationSource.Value);
                }

                if (principalKeyConfigurationSource != null
                    && principalProperties!.Count != 0)
                {
                    newRelationshipBuilder.Metadata.UpdatePrincipalKeyConfigurationSource(
                        principalKeyConfigurationSource.Value);
                }
            }
        }

        if (isUnique.HasValue)
        {
            var isUniqueConfigurationSource = configurationSource;
            if (isUnique.Value == Metadata.IsUnique)
            {
                isUniqueConfigurationSource = isUniqueConfigurationSource.Max(Metadata.GetIsUniqueConfigurationSource());
            }

            newRelationshipBuilder = newRelationshipBuilder.IsUnique(
                    isUnique.Value,
                    isUniqueConfigurationSource)
                ?? newRelationshipBuilder;
        }
        else if (!oldRelationshipInverted
                 && Metadata.GetIsUniqueConfigurationSource() is { } isUniqueConfigurationSource
                 && !newRelationshipBuilder.Metadata.GetIsUniqueConfigurationSource().HasValue)
        {
            newRelationshipBuilder = newRelationshipBuilder.IsUnique(Metadata.IsUnique, isUniqueConfigurationSource)
                ?? newRelationshipBuilder;
        }

        if (isRequired.HasValue)
        {
            var isRequiredConfigurationSource = configurationSource;
            if (isRequired.Value == Metadata.IsRequired)
            {
                isRequiredConfigurationSource = isRequiredConfigurationSource.Max(Metadata.GetIsRequiredConfigurationSource());
            }

            newRelationshipBuilder = newRelationshipBuilder.IsRequired(
                    isRequired.Value,
                    isRequiredConfigurationSource)
                ?? newRelationshipBuilder;
        }
        else
        {
            if (!oldRelationshipInverted)
            {
                if (Metadata.GetIsRequiredConfigurationSource() is { } isRequiredConfigurationSource
                    && !newRelationshipBuilder.Metadata.GetIsRequiredConfigurationSource().HasValue)
                {
                    newRelationshipBuilder = newRelationshipBuilder.IsRequired(
                            Metadata.IsRequired,
                            isRequiredConfigurationSource)
                        ?? newRelationshipBuilder;
                }
            }
            else
            {
                if (Metadata.GetIsRequiredDependentConfigurationSource() is { } isRequiredDependentConfigurationSource
                    && isRequiredDependentConfigurationSource.Overrides(ConfigurationSource.Explicit)
                    && !newRelationshipBuilder.Metadata.GetIsRequiredConfigurationSource().HasValue)
                {
                    newRelationshipBuilder = newRelationshipBuilder.IsRequired(
                            Metadata.IsRequiredDependent,
                            isRequiredDependentConfigurationSource)
                        ?? newRelationshipBuilder;
                }
            }
        }

        if (isRequiredDependent.HasValue)
        {
            var isRequiredDependentConfigurationSource = configurationSource;
            if (isRequiredDependent.Value == Metadata.IsRequiredDependent)
            {
                isRequiredDependentConfigurationSource = isRequiredDependentConfigurationSource.Max(
                    Metadata.GetIsRequiredDependentConfigurationSource());
            }

            newRelationshipBuilder = newRelationshipBuilder.IsRequiredDependent(
                    isRequiredDependent.Value,
                    isRequiredDependentConfigurationSource)
                ?? newRelationshipBuilder;
        }
        else
        {
            if (!oldRelationshipInverted)
            {
                if (Metadata.GetIsRequiredDependentConfigurationSource() is { } isRequiredDependentConfigurationSource
                    && !newRelationshipBuilder.Metadata.GetIsRequiredDependentConfigurationSource().HasValue)
                {
                    newRelationshipBuilder = newRelationshipBuilder.IsRequiredDependent(
                            Metadata.IsRequiredDependent,
                            isRequiredDependentConfigurationSource)
                        ?? newRelationshipBuilder;
                }
            }
            else
            {
                if (Metadata.GetIsRequiredConfigurationSource() is { } isRequiredConfigurationSource
                    && isRequiredConfigurationSource.Overrides(ConfigurationSource.Explicit)
                    && !newRelationshipBuilder.Metadata.GetIsRequiredDependentConfigurationSource().HasValue)
                {
                    newRelationshipBuilder = newRelationshipBuilder.IsRequiredDependent(
                            Metadata.IsRequired,
                            isRequiredConfigurationSource)
                        ?? newRelationshipBuilder;
                }
            }
        }

        if (deleteBehavior.HasValue)
        {
            var deleteBehaviorConfigurationSource = configurationSource;
            if (deleteBehavior.Value == Metadata.DeleteBehavior)
            {
                deleteBehaviorConfigurationSource =
                    deleteBehaviorConfigurationSource.Max(Metadata.GetDeleteBehaviorConfigurationSource());
            }

            newRelationshipBuilder = newRelationshipBuilder.OnDelete(
                    deleteBehavior.Value,
                    deleteBehaviorConfigurationSource)
                ?? newRelationshipBuilder;
        }
        else if (!oldRelationshipInverted
                 && Metadata.GetDeleteBehaviorConfigurationSource() is { } deleteBehaviorConfigurationSource
                 && !newRelationshipBuilder.Metadata.GetDeleteBehaviorConfigurationSource().HasValue)
        {
            newRelationshipBuilder = newRelationshipBuilder.OnDelete(
                    Metadata.DeleteBehavior,
                    deleteBehaviorConfigurationSource)
                ?? newRelationshipBuilder;
        }

        if (navigationToPrincipal != null)
        {
            var navigationToPrincipalConfigurationSource = configurationSource;
            if (navigationToPrincipal.Value.Name == oldNavigationToPrincipal?.Name)
            {
                navigationToPrincipalConfigurationSource =
                    navigationToPrincipalConfigurationSource.Max(oldToPrincipalConfigurationSource);
            }

            newRelationshipBuilder = newRelationshipBuilder.HasNavigations(
                    navigationToPrincipal,
                    navigationToDependent: null,
                    navigationToPrincipalConfigurationSource)
                ?? newRelationshipBuilder;

            if (oldNavigationToPrincipal != null
                && newRelationshipBuilder.Metadata.DependentToPrincipal != null
                && oldNavigationToPrincipal != newRelationshipBuilder.Metadata.DependentToPrincipal)
            {
                newRelationshipBuilder = MergeFacetsFrom(
                    newRelationshipBuilder.Metadata.DependentToPrincipal, oldNavigationToPrincipal);
            }
        }
        else if (oldNavigationToPrincipal != null
                 && newRelationshipBuilder.Metadata.DependentToPrincipal == null
                 && newRelationshipBuilder.CanSetNavigations(
                     oldNavigationToPrincipal.CreateMemberIdentity(),
                     navigationToDependent: null,
                     oldToPrincipalConfigurationSource))
        {
            newRelationshipBuilder = newRelationshipBuilder.HasNavigations(
                    oldNavigationToPrincipal.CreateMemberIdentity(),
                    navigationToDependent: null,
                    oldToPrincipalConfigurationSource!.Value)
                ?? newRelationshipBuilder;

            if (newRelationshipBuilder.Metadata.DependentToPrincipal != null)
            {
                newRelationshipBuilder = MergeFacetsFrom(
                    newRelationshipBuilder.Metadata.DependentToPrincipal, oldNavigationToPrincipal);
            }
        }

        if (navigationToDependent != null)
        {
            var navigationToDependentConfigurationSource = configurationSource;
            if (navigationToDependent.Value.Name == oldNavigationToDependent?.Name)
            {
                navigationToDependentConfigurationSource =
                    navigationToDependentConfigurationSource.Max(oldToDependentConfigurationSource);
            }

            newRelationshipBuilder = newRelationshipBuilder.HasNavigations(
                    navigationToPrincipal: null,
                    navigationToDependent,
                    navigationToDependentConfigurationSource)
                ?? newRelationshipBuilder;

            if (oldNavigationToDependent != null
                && newRelationshipBuilder.Metadata.PrincipalToDependent != null
                && oldNavigationToDependent != newRelationshipBuilder.Metadata.PrincipalToDependent)
            {
                newRelationshipBuilder = MergeFacetsFrom(
                    newRelationshipBuilder.Metadata.PrincipalToDependent, oldNavigationToDependent);
            }
        }
        else if (oldNavigationToDependent != null
                 && newRelationshipBuilder.Metadata.PrincipalToDependent == null
                 && newRelationshipBuilder.CanSetNavigations(
                     navigationToPrincipal: null,
                     oldNavigationToDependent.CreateMemberIdentity(),
                     oldToDependentConfigurationSource))
        {
            newRelationshipBuilder = newRelationshipBuilder.HasNavigations(
                    navigationToPrincipal: null,
                    oldNavigationToDependent.CreateMemberIdentity(),
                    oldToDependentConfigurationSource!.Value)
                ?? newRelationshipBuilder;

            if (newRelationshipBuilder.Metadata.PrincipalToDependent != null)
            {
                newRelationshipBuilder = MergeFacetsFrom(
                    newRelationshipBuilder.Metadata.PrincipalToDependent, oldNavigationToDependent);
            }
        }

        if (isOwnership.HasValue)
        {
            var isOwnershipConfigurationSource = configurationSource;
            if (isOwnership.Value == Metadata.IsOwnership)
            {
                isOwnershipConfigurationSource = isOwnershipConfigurationSource.Max(Metadata.GetIsOwnershipConfigurationSource());
            }

            newRelationshipBuilder = newRelationshipBuilder.IsOwnership(
                    isOwnership.Value,
                    isOwnershipConfigurationSource)
                ?? newRelationshipBuilder;
        }
        else if (!oldRelationshipInverted
                 && Metadata.GetIsOwnershipConfigurationSource() is { } getIsOwnershipConfigurationSource)
        {
            newRelationshipBuilder = newRelationshipBuilder.IsOwnership(
                    Metadata.IsOwnership,
                    getIsOwnershipConfigurationSource)
                ?? newRelationshipBuilder;
        }

        if (referencingSkipNavigations != null)
        {
            foreach (var referencingNavigationTuple in referencingSkipNavigations)
            {
                var skipNavigation = referencingNavigationTuple.Navigation;
                if (!skipNavigation.IsInModel)
                {
                    var navigationEntityType = skipNavigation.DeclaringEntityType;
                    skipNavigation = !navigationEntityType.IsInModel
                        ? null
                        : navigationEntityType.FindSkipNavigation(skipNavigation.Name);
                }

                skipNavigation?.Builder.HasForeignKey(
                    newRelationshipBuilder.Metadata, referencingNavigationTuple.ConfigurationSource);
            }
        }

        if (Metadata != newRelationshipBuilder.Metadata)
        {
            newRelationshipBuilder.MergeAnnotationsFrom(Metadata);
        }

        newRelationshipBuilder = batch.Run(newRelationshipBuilder);

        return newRelationshipBuilder;
    }

    private static InternalForeignKeyBuilder MergeFacetsFrom(Navigation newNavigation, Navigation oldNavigation)
    {
        newNavigation.Builder.MergeAnnotationsFrom(oldNavigation);

        var builder = newNavigation.Builder;

        var propertyAccessModeConfigurationSource = oldNavigation.GetPropertyAccessModeConfigurationSource();
        if (propertyAccessModeConfigurationSource.HasValue
            && builder.CanSetPropertyAccessMode(
                ((IConventionNavigation)oldNavigation).GetPropertyAccessMode(),
                propertyAccessModeConfigurationSource))
        {
            builder = builder.UsePropertyAccessMode(
                ((IConventionNavigation)oldNavigation).GetPropertyAccessMode(), propertyAccessModeConfigurationSource.Value)!;
        }

        var oldFieldInfoConfigurationSource = oldNavigation.GetFieldInfoConfigurationSource();
        if (oldFieldInfoConfigurationSource.HasValue
            && builder.CanSetField(oldNavigation.FieldInfo, oldFieldInfoConfigurationSource))
        {
            builder = builder.HasField(oldNavigation.FieldInfo, oldFieldInfoConfigurationSource.Value)!;
        }

        var oldIsEagerLoadedConfigurationSource = ((IConventionNavigation)oldNavigation).GetIsEagerLoadedConfigurationSource();
        if (oldIsEagerLoadedConfigurationSource.HasValue
            && builder.CanSetAutoInclude(((IReadOnlyNavigation)oldNavigation).IsEagerLoaded, oldIsEagerLoadedConfigurationSource.Value))
        {
            builder = builder.AutoInclude(
                ((IReadOnlyNavigation)oldNavigation).IsEagerLoaded, oldIsEagerLoadedConfigurationSource.Value)!;
        }

        var oldLazyLoadingEnabledConfigurationSource = ((IConventionNavigation)oldNavigation).GetLazyLoadingEnabledConfigurationSource();
        if (oldLazyLoadingEnabledConfigurationSource.HasValue
            && builder.CanSetLazyLoadingEnabled(
                ((IReadOnlyNavigation)oldNavigation).LazyLoadingEnabled, oldLazyLoadingEnabledConfigurationSource.Value))
        {
            builder = builder.EnableLazyLoading(
                ((IReadOnlyNavigation)oldNavigation).LazyLoadingEnabled, oldLazyLoadingEnabledConfigurationSource.Value)!;
        }

        return builder.Metadata.ForeignKey.Builder;
    }

    private InternalForeignKeyBuilder? GetOrCreateRelationshipBuilder(
        EntityType principalEntityType,
        EntityType dependentEntityType,
        MemberIdentity? navigationToPrincipal,
        MemberIdentity? navigationToDependent,
        IReadOnlyList<Property>? dependentProperties,
        IReadOnlyList<Property>? oldNameDependentProperties,
        IReadOnlyList<Property>? principalProperties,
        bool? isRequired,
        bool removeCurrent,
        ConfigurationSource? principalEndConfigurationSource,
        ConfigurationSource? configurationSource,
        out bool? existingRelationshipInverted)
    {
        var newRelationshipBuilder = FindCompatibleRelationship(
            principalEntityType,
            dependentEntityType,
            navigationToPrincipal,
            navigationToDependent,
            dependentProperties,
            principalProperties,
            principalEndConfigurationSource,
            configurationSource,
            out existingRelationshipInverted,
            out var conflictingRelationshipsFound,
            out var resolvableRelationships);

        if (conflictingRelationshipsFound)
        {
            return null;
        }

        // This workaround prevents the properties to be cleaned away before the new FK is created,
        // this should be replaced with reference counting
        // Issue #15898
        var temporaryProperties = dependentProperties?.Where(
            p => p.GetConfigurationSource() == ConfigurationSource.Convention
                && ((IConventionProperty)p).IsImplicitlyCreated()).ToList();
        var tempIndex = temporaryProperties?.Count > 0
            && dependentEntityType.FindIndex(temporaryProperties) == null
                ? dependentEntityType.Builder.HasIndex(temporaryProperties, ConfigurationSource.Convention)!.Metadata
                : null;

        var temporaryKeyProperties = principalProperties?.Where(
            p => p.GetConfigurationSource() == ConfigurationSource.Convention
                && ((IConventionProperty)p).IsImplicitlyCreated()).ToList();
        var keyTempIndex = temporaryKeyProperties?.Count > 0
            && principalEntityType.FindIndex(temporaryKeyProperties) == null
                ? principalEntityType.Builder.HasIndex(temporaryKeyProperties, ConfigurationSource.Convention)!.Metadata
                : null;

        var removedForeignKeys = new List<ForeignKey>();
        var referencingSkipNavigationName = Metadata.ReferencingSkipNavigations?.FirstOrDefault()?.Inverse?.Name;
        if (!Metadata.IsInModel)
        {
            removedForeignKeys.Add(Metadata);
        }
        else
        {
            if (removeCurrent || newRelationshipBuilder != null)
            {
                if (newRelationshipBuilder == this)
                {
                    newRelationshipBuilder = null;
                }

                removedForeignKeys.Add(Metadata);
                Metadata.DeclaringEntityType.Builder.HasNoRelationship(Metadata, ConfigurationSource.Explicit);
            }
            else
            {
                existingRelationshipInverted = false;
                newRelationshipBuilder = Metadata.Builder;
            }
        }

        foreach (var relationshipWithResolution in resolvableRelationships)
        {
            var resolvableRelationship = relationshipWithResolution.Builder;
            var sameConfigurationSource = relationshipWithResolution.SameConfigurationSource;
            var resolution = relationshipWithResolution.Resolution;
            var inverseNavigationRemoved = relationshipWithResolution.InverseNavigationShouldBeRemoved;
            if (sameConfigurationSource
                && configurationSource == ConfigurationSource.Explicit
                && inverseNavigationRemoved)
            {
                var foreignKey = resolvableRelationship.Metadata;
                ThrowForConflictingNavigation(
                    foreignKey,
                    principalEntityType, dependentEntityType,
                    navigationToDependent?.Name, navigationToPrincipal?.Name);
            }

            if (resolvableRelationship == newRelationshipBuilder)
            {
                continue;
            }

            if (resolution.HasFlag(Resolution.Remove))
            {
                removedForeignKeys.Add(resolvableRelationship.Metadata);
                resolvableRelationship.Metadata.DeclaringEntityType.Builder.HasNoRelationship(
                    resolvableRelationship.Metadata, ConfigurationSource.Explicit);
                continue;
            }

            if (resolution.HasFlag(Resolution.ResetToPrincipal))
            {
                resolvableRelationship = resolvableRelationship.HasNavigations(
                    MemberIdentity.None, navigationToDependent: null, resolvableRelationship.Metadata.GetConfigurationSource())!;
            }

            if (resolution.HasFlag(Resolution.ResetToDependent))
            {
                resolvableRelationship = resolvableRelationship.HasNavigations(
                    navigationToPrincipal: null, MemberIdentity.None, resolvableRelationship.Metadata.GetConfigurationSource())!;
            }

            if (!resolvableRelationship.Metadata.IsInModel)
            {
                continue;
            }

            var navigationlessForeignKey = resolvableRelationship.Metadata;
            if (navigationlessForeignKey.DependentToPrincipal == null
                && navigationlessForeignKey.PrincipalToDependent == null
                && navigationlessForeignKey.DeclaringEntityType.Builder.HasNoRelationship(
                    navigationlessForeignKey, ConfigurationSource.Convention)
                != null)
            {
                removedForeignKeys.Add(navigationlessForeignKey);
            }

            if (resolution.HasFlag(Resolution.ResetDependentProperties))
            {
                var foreignKey = resolvableRelationship.Metadata;
                resolvableRelationship.HasForeignKey((IReadOnlyList<Property>?)null, foreignKey.GetConfigurationSource());
            }
        }

        if (newRelationshipBuilder == null)
        {
            var principalKey = principalProperties != null
                ? principalEntityType.GetRootType().Builder.HasKey(principalProperties, configurationSource)!.Metadata
                : principalEntityType.FindPrimaryKey();
            if (principalKey != null)
            {
                if (oldNameDependentProperties != null
                    || (dependentProperties != null
                        && !ForeignKey.AreCompatible(
                            principalKey.Properties,
                            dependentProperties,
                            principalEntityType,
                            dependentEntityType,
                            shouldThrow: false)
                        && dependentProperties.All(
                            p => ConfigurationSource.Convention.Overrides(p.GetTypeConfigurationSource())
                                && (p.IsShadowProperty() || p.IsIndexerProperty()))))
                {
                    dependentProperties = (oldNameDependentProperties ?? dependentProperties)!;
                    if (principalKey.Properties.Count == dependentProperties.Count)
                    {
                        var detachedProperties = InternalTypeBaseBuilder.DetachProperties(dependentProperties)!;
                        dependentProperties = dependentEntityType.Builder.GetOrCreateProperties(
                            dependentProperties.Select(p => p.Name).ToList(),
                            ConfigurationSource.Convention,
                            principalKey.Properties,
                            isRequired ?? false)!;
                        detachedProperties.Attach(dependentEntityType.Builder);
                    }
                }

                if (dependentProperties != null
                    && !ForeignKey.AreCompatible(
                        principalKey.Properties,
                        dependentProperties,
                        principalEntityType,
                        dependentEntityType,
                        shouldThrow: false))
                {
                    if (principalProperties == null)
                    {
                        principalKey = null;
                    }
                    else
                    {
                        dependentProperties = null;
                    }
                }
            }

            newRelationshipBuilder = dependentEntityType.Builder.CreateForeignKey(
                principalEntityType.Builder,
                dependentProperties,
                principalKey,
                navigationToPrincipal?.Name ?? referencingSkipNavigationName,
                isRequired,
                ConfigurationSource.Convention)!;
        }

        if (tempIndex?.IsInModel == true)
        {
            dependentEntityType.RemoveIndex(tempIndex.Properties);
        }

        if (keyTempIndex?.IsInModel == true)
        {
            keyTempIndex.DeclaringEntityType.RemoveIndex(keyTempIndex.Properties);
        }

        if (newRelationshipBuilder == null)
        {
            return null;
        }

        foreach (var removedForeignKey in removedForeignKeys)
        {
            Metadata.DeclaringEntityType.Model.ConventionDispatcher.Tracker.Update(removedForeignKey, newRelationshipBuilder.Metadata);
        }

        return newRelationshipBuilder;
    }

    private InternalForeignKeyBuilder? FindCompatibleRelationship(
        EntityType principalEntityType,
        EntityType dependentEntityType,
        MemberIdentity? navigationToPrincipal,
        MemberIdentity? navigationToDependent,
        IReadOnlyList<Property>? dependentProperties,
        IReadOnlyList<Property>? principalProperties,
        ConfigurationSource? principalEndConfigurationSource,
        ConfigurationSource? configurationSource,
        out bool? existingRelationshipInverted,
        out bool conflictingRelationshipsFound,
        out List<(
            InternalForeignKeyBuilder Builder,
            bool SameConfigurationSource,
            Resolution Resolution,
            bool InverseNavigationShouldBeRemoved)> resolvableRelationships)
    {
        existingRelationshipInverted = null;
        conflictingRelationshipsFound = false;
        resolvableRelationships = new List<(InternalForeignKeyBuilder, bool, Resolution, bool)>();

        var matchingRelationships = FindRelationships(
                principalEntityType,
                dependentEntityType,
                navigationToPrincipal,
                navigationToDependent,
                dependentProperties,
                principalProperties ?? principalEntityType.FindPrimaryKey()?.Properties)
            .Where(r => r.Metadata != Metadata)
            .Distinct();

        var unresolvableRelationships = new List<InternalForeignKeyBuilder>();
        foreach (var matchingRelationship in matchingRelationships)
        {
            var resolvable = true;
            bool? sameConfigurationSource = null;
            var inverseNavigationRemoved = false;
            var resolution = Resolution.None;
            var navigationToPrincipalName = navigationToPrincipal?.Name;
            var navigationToDependentName = navigationToDependent?.Name;
            if (navigationToPrincipalName != null)
            {
                if ((navigationToPrincipalName == matchingRelationship.Metadata.DependentToPrincipal?.Name)
                    && (dependentEntityType.IsAssignableFrom(matchingRelationship.Metadata.DeclaringEntityType)
                        || matchingRelationship.Metadata.DeclaringEntityType.IsAssignableFrom(dependentEntityType)))
                {
                    if (matchingRelationship.CanRemoveNavigation(
                            pointsToPrincipal: true, configurationSource, overrideSameSource: false))
                    {
                        resolution |= Resolution.ResetToPrincipal;
                        sameConfigurationSource ??= false;
                    }
                    else if (matchingRelationship.CanRemoveNavigation(pointsToPrincipal: true, configurationSource)
                             // Don't remove derived bi-directional navigations
                             && (matchingRelationship.Metadata.GetConfigurationSource() == ConfigurationSource.Explicit
                                 || navigationToDependentName != null
                                 || matchingRelationship.Metadata.PrincipalToDependent == null
                                 || !matchingRelationship.Metadata.DeclaringEntityType.IsStrictlyDerivedFrom(dependentEntityType)))
                    {
                        if (navigationToDependentName != null
                            && matchingRelationship.Metadata.PrincipalToDependent != null
                            && navigationToDependentName != matchingRelationship.Metadata.PrincipalToDependent.Name)
                        {
                            inverseNavigationRemoved = true;
                        }

                        resolution |= Resolution.ResetToPrincipal;
                        sameConfigurationSource = true;
                    }
                    else if (!configurationSource.HasValue
                             || !matchingRelationship.Metadata.DeclaringEntityType.Builder
                                 .CanRemoveForeignKey(matchingRelationship.Metadata, configurationSource.Value))
                    {
                        resolvable = false;
                    }
                }
                else if ((navigationToPrincipalName == matchingRelationship.Metadata.PrincipalToDependent?.Name)
                         && (dependentEntityType.IsAssignableFrom(matchingRelationship.Metadata.PrincipalEntityType)
                             || matchingRelationship.Metadata.PrincipalEntityType.IsAssignableFrom(dependentEntityType)))
                {
                    if (matchingRelationship.CanRemoveNavigation(
                            pointsToPrincipal: false, configurationSource, overrideSameSource: false))
                    {
                        resolution |= Resolution.ResetToDependent;
                        sameConfigurationSource ??= false;
                    }
                    else if (matchingRelationship.CanRemoveNavigation(pointsToPrincipal: false, configurationSource)
                             // Don't remove derived bi-directional navigations
                             && (matchingRelationship.Metadata.GetConfigurationSource() == ConfigurationSource.Explicit
                                 || navigationToDependentName != null
                                 || matchingRelationship.Metadata.DependentToPrincipal == null
                                 || !matchingRelationship.Metadata.PrincipalEntityType.IsStrictlyDerivedFrom(dependentEntityType)))
                    {
                        if (navigationToDependentName != null
                            && matchingRelationship.Metadata.DependentToPrincipal != null
                            && navigationToDependentName != matchingRelationship.Metadata.DependentToPrincipal.Name)
                        {
                            inverseNavigationRemoved = true;
                        }

                        resolution |= Resolution.ResetToDependent;
                        sameConfigurationSource = true;
                    }
                    else if (!configurationSource.HasValue
                             || !matchingRelationship.Metadata.DeclaringEntityType.Builder
                                 .CanRemoveForeignKey(matchingRelationship.Metadata, configurationSource.Value))
                    {
                        resolvable = false;
                    }
                }
            }

            if (navigationToDependentName != null)
            {
                if ((navigationToDependentName == matchingRelationship.Metadata.PrincipalToDependent?.Name)
                    && (principalEntityType.IsAssignableFrom(matchingRelationship.Metadata.PrincipalEntityType)
                        || matchingRelationship.Metadata.PrincipalEntityType.IsAssignableFrom(principalEntityType)))
                {
                    if (matchingRelationship.CanRemoveNavigation(
                            pointsToPrincipal: false, configurationSource, overrideSameSource: false))
                    {
                        resolution |= Resolution.ResetToDependent;
                        sameConfigurationSource ??= false;
                    }
                    else if (matchingRelationship.CanRemoveNavigation(pointsToPrincipal: false, configurationSource)
                             // Don't remove derived bi-directional navigations
                             && (matchingRelationship.Metadata.GetConfigurationSource() == ConfigurationSource.Explicit
                                 || navigationToPrincipalName != null
                                 || matchingRelationship.Metadata.DependentToPrincipal == null
                                 || !matchingRelationship.Metadata.PrincipalEntityType.IsStrictlyDerivedFrom(principalEntityType)))
                    {
                        if (navigationToPrincipalName != null
                            && matchingRelationship.Metadata.DependentToPrincipal != null
                            && navigationToPrincipalName != matchingRelationship.Metadata.DependentToPrincipal.Name)
                        {
                            inverseNavigationRemoved = true;
                        }

                        resolution |= Resolution.ResetToDependent;
                        sameConfigurationSource = true;
                    }
                    else if (!configurationSource.HasValue
                             || !matchingRelationship.Metadata.DeclaringEntityType.Builder
                                 .CanRemoveForeignKey(matchingRelationship.Metadata, configurationSource.Value))
                    {
                        resolvable = false;
                    }
                }
                else if ((navigationToDependentName == matchingRelationship.Metadata.DependentToPrincipal?.Name)
                         && (principalEntityType.IsAssignableFrom(matchingRelationship.Metadata.DeclaringEntityType)
                             || matchingRelationship.Metadata.DeclaringEntityType.IsAssignableFrom(principalEntityType)))
                {
                    if (matchingRelationship.CanRemoveNavigation(
                            pointsToPrincipal: true, configurationSource, overrideSameSource: false))
                    {
                        resolution |= Resolution.ResetToPrincipal;
                        sameConfigurationSource ??= false;
                    }
                    else if (matchingRelationship.CanRemoveNavigation(pointsToPrincipal: true, configurationSource)
                             // Don't remove derived bi-directional navigations
                             && (matchingRelationship.Metadata.GetConfigurationSource() == ConfigurationSource.Explicit
                                 || navigationToPrincipalName != null
                                 || matchingRelationship.Metadata.PrincipalToDependent == null
                                 || !matchingRelationship.Metadata.DeclaringEntityType.IsStrictlyDerivedFrom(principalEntityType)))
                    {
                        if (navigationToPrincipalName != null
                            && matchingRelationship.Metadata.PrincipalToDependent != null
                            && navigationToPrincipalName != matchingRelationship.Metadata.PrincipalToDependent.Name)
                        {
                            inverseNavigationRemoved = true;
                        }

                        resolution |= Resolution.ResetToPrincipal;
                        sameConfigurationSource = true;
                    }
                    else if (!configurationSource.HasValue
                             || !matchingRelationship.Metadata.DeclaringEntityType.Builder
                                 .CanRemoveForeignKey(matchingRelationship.Metadata, configurationSource.Value))
                    {
                        resolvable = false;
                    }
                }
            }

            if (dependentProperties != null
                && matchingRelationship.Metadata.Properties.SequenceEqual(dependentProperties))
            {
                if (matchingRelationship.CanSetForeignKey(
                        properties: null, dependentEntityType: null, configurationSource, overrideSameSource: false))
                {
                    resolution |= Resolution.ResetDependentProperties;
                    sameConfigurationSource ??= false;
                }
                else if (matchingRelationship.CanSetForeignKey(properties: null, configurationSource))
                {
                    resolution |= Resolution.ResetDependentProperties;
                    sameConfigurationSource = true;
                }
                else
                {
                    resolvable = false;
                }
            }

            if (resolvable)
            {
                if ((sameConfigurationSource ?? true)
                    && configurationSource.HasValue
                    && matchingRelationship.Metadata.DeclaringEntityType.Builder
                        .CanRemoveForeignKey(matchingRelationship.Metadata, configurationSource.Value))
                {
                    resolution |= Resolution.Remove;
                }

                resolvableRelationships.Add(
                    (matchingRelationship, sameConfigurationSource ?? true, resolution, inverseNavigationRemoved));
            }
            else
            {
                unresolvableRelationships.Add(matchingRelationship);
            }
        }

        InternalForeignKeyBuilder? newRelationshipBuilder = null;

        var candidates = unresolvableRelationships.Concat(
            resolvableRelationships.Where(r => r.SameConfigurationSource).Concat(
                    resolvableRelationships.Where(r => !r.SameConfigurationSource))
                .Select(r => r.Builder));
        foreach (var candidateRelationship in candidates)
        {
            var candidateFk = candidateRelationship.Metadata;
            if (principalEndConfigurationSource.OverridesStrictly(
                    candidateFk.GetDependentToPrincipalConfigurationSource())
                    && (principalEntityType != candidateFk.PrincipalEntityType
                        || dependentEntityType != candidateFk.DeclaringEntityType)
                    && (principalEntityType.IsAssignableFrom(dependentEntityType)
                        || dependentEntityType.IsAssignableFrom(principalEntityType)))
            {
                // Favor the intra-hierarchical relationship with higher configuration source
                continue;
            }

            if (!candidateRelationship.CanSetRelatedTypes(
                    principalEntityType,
                    dependentEntityType,
                    strictPrincipal: principalEndConfigurationSource.HasValue
                    && principalEndConfigurationSource.Overrides(Metadata.GetPrincipalEndConfigurationSource()),
                    navigationToPrincipal,
                    navigationToDependent,
                    configurationSource,
                    shouldThrow: false,
                    out var candidateRelationshipInverted,
                    out var shouldResetToPrincipal,
                    out var shouldResetToDependent,
                    out _,
                    out _,
                    out _))
            {
                continue;
            }

            if (configurationSource != ConfigurationSource.Explicit
                && (shouldResetToPrincipal || shouldResetToDependent)
                && (navigationToPrincipal?.Name is null || navigationToDependent?.Name is null)
                && candidateFk is { DependentToPrincipal: not null, PrincipalToDependent: not null }
                && ((!candidateRelationshipInverted
                        && principalEntityType.IsAssignableFrom(candidateFk.PrincipalEntityType)
                        && dependentEntityType.IsAssignableFrom(candidateFk.DeclaringEntityType))
                    || (candidateRelationshipInverted
                        && principalEntityType.IsAssignableFrom(candidateFk.DeclaringEntityType)
                        && dependentEntityType.IsAssignableFrom(candidateFk.PrincipalEntityType))))
            {
                // Favor derived bi-directional relationships over one-directional on base
                continue;
            }

            if (dependentProperties != null
                && !Property.AreCompatible(dependentProperties, candidateFk.DeclaringEntityType))
            {
                continue;
            }

            if (principalProperties != null
                && !Property.AreCompatible(principalProperties, candidateFk.PrincipalEntityType))
            {
                continue;
            }

            existingRelationshipInverted = candidateRelationshipInverted;
            newRelationshipBuilder ??= candidateRelationship;
            break;
        }

        if (unresolvableRelationships.Any(r => r != newRelationshipBuilder))
        {
            conflictingRelationshipsFound = true;
            return null;
        }

        return newRelationshipBuilder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static void ThrowForConflictingNavigation(
        IReadOnlyForeignKey foreignKey,
        string newInverseName,
        bool newToPrincipal)
        => ThrowForConflictingNavigation(
            foreignKey,
            foreignKey.PrincipalEntityType, foreignKey.DeclaringEntityType,
            newToPrincipal ? foreignKey.PrincipalToDependent?.Name : newInverseName,
            newToPrincipal ? newInverseName : foreignKey.DependentToPrincipal?.Name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static void ThrowForConflictingNavigation(
        IReadOnlyForeignKey foreignKey,
        IReadOnlyEntityType principalEntityType,
        IReadOnlyEntityType dependentEntityType,
        string? navigationToDependent,
        string? navigationToPrincipal)
        => throw new InvalidOperationException(
            CoreStrings.ConflictingRelationshipNavigation(
                principalEntityType.DisplayName()
                + (navigationToDependent == null
                    ? ""
                    : "." + navigationToDependent),
                dependentEntityType.DisplayName()
                + (navigationToPrincipal == null
                    ? ""
                    : "." + navigationToPrincipal),
                foreignKey.PrincipalEntityType.DisplayName()
                + (foreignKey.PrincipalToDependent == null
                    ? ""
                    : "." + foreignKey.PrincipalToDependent.Name),
                foreignKey.DeclaringEntityType.DisplayName()
                + (foreignKey.DependentToPrincipal == null
                    ? ""
                    : "." + foreignKey.DependentToPrincipal.Name)));

    private static IReadOnlyList<InternalForeignKeyBuilder> FindRelationships(
        EntityType principalEntityType,
        EntityType dependentEntityType,
        MemberIdentity? navigationToPrincipal,
        MemberIdentity? navigationToDependent,
        IReadOnlyList<Property>? dependentProperties,
        IReadOnlyList<Property>? principalProperties)
    {
        var existingRelationships = new List<InternalForeignKeyBuilder>();
        if (navigationToPrincipal?.Name != null)
        {
            existingRelationships.AddRange(
                dependentEntityType
                    .FindNavigationsInHierarchy(navigationToPrincipal.Value.Name)
                    .Select(n => n.ForeignKey.Builder));
        }

        if (navigationToDependent?.Name != null)
        {
            existingRelationships.AddRange(
                principalEntityType
                    .FindNavigationsInHierarchy(navigationToDependent.Value.Name)
                    .Select(n => n.ForeignKey.Builder));
        }

        if (dependentProperties != null)
        {
            if (principalProperties != null)
            {
                var principalKey = principalEntityType.FindKey(principalProperties);
                if (principalKey != null)
                {
                    existingRelationships.AddRange(
                        dependentEntityType
                            .FindForeignKeysInHierarchy(dependentProperties, principalKey, principalEntityType)
                            .Select(fk => fk.Builder));
                }
            }
            else
            {
                existingRelationships.AddRange(
                    dependentEntityType
                        .FindForeignKeysInHierarchy(dependentProperties)
                        .Where(fk => fk.PrincipalEntityType == principalEntityType)
                        .Select(fk => fk.Builder));
            }
        }

        return existingRelationships;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static InternalForeignKeyBuilder? FindCurrentForeignKeyBuilder(
        EntityType principalEntityType,
        EntityType dependentEntityType,
        MemberIdentity? navigationToPrincipal,
        MemberIdentity? navigationToDependent,
        IReadOnlyList<Property>? dependentProperties,
        IReadOnlyList<Property>? principalProperties)
    {
        InternalForeignKeyBuilder? currentRelationship = null;
        var matchingRelationships = FindRelationships(
            principalEntityType,
            dependentEntityType,
            navigationToPrincipal,
            navigationToDependent,
            dependentProperties,
            principalProperties).Distinct();

        foreach (var matchingRelationship in matchingRelationships)
        {
            if (!matchingRelationship.Metadata.DeclaringEntityType.IsAssignableFrom(dependentEntityType))
            {
                continue;
            }

            if (!matchingRelationship.Metadata.PrincipalEntityType.IsAssignableFrom(principalEntityType))
            {
                continue;
            }

            var matchingForeignKey = matchingRelationship.Metadata;
            var sameHierarchyInvertedNavigations =
                (principalEntityType.IsAssignableFrom(dependentEntityType)
                    || dependentEntityType.IsAssignableFrom(principalEntityType))
                && (navigationToPrincipal == null
                    || navigationToPrincipal.Value.Name == matchingForeignKey.PrincipalToDependent?.Name)
                && (navigationToDependent == null
                    || navigationToDependent.Value.Name == matchingForeignKey.DependentToPrincipal?.Name);

            if (!sameHierarchyInvertedNavigations)
            {
                if (navigationToPrincipal != null
                    && matchingForeignKey.DependentToPrincipal?.Name != navigationToPrincipal.Value.Name)
                {
                    continue;
                }

                if (navigationToDependent != null
                    && matchingForeignKey.PrincipalToDependent?.Name != navigationToDependent.Value.Name)
                {
                    continue;
                }
            }

            if (dependentProperties != null
                && !matchingForeignKey.Properties.SequenceEqual(dependentProperties))
            {
                continue;
            }

            if (principalProperties != null
                && !matchingForeignKey.PrincipalKey.Properties.SequenceEqual(principalProperties))
            {
                continue;
            }

            if (currentRelationship != null)
            {
                // More than one match, ambiguity should be dealt with later
                return null;
            }

            currentRelationship = matchingRelationship;
        }

        return currentRelationship;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalForeignKeyBuilder? Attach(InternalEntityTypeBuilder entityTypeBuilder)
    {
        var configurationSource = Metadata.GetConfigurationSource();
        var model = Metadata.DeclaringEntityType.Model;
        InternalEntityTypeBuilder principalEntityTypeBuilder;
        EntityType? principalEntityType;
        if (Metadata.PrincipalEntityType.IsInModel)
        {
            principalEntityTypeBuilder = Metadata.PrincipalEntityType.Builder;
            principalEntityType = Metadata.PrincipalEntityType;
        }
        else
        {
            if (Metadata.PrincipalEntityType.Name == entityTypeBuilder.Metadata.Name
                || Metadata.PrincipalEntityType.ClrType == entityTypeBuilder.Metadata.ClrType)
            {
                principalEntityTypeBuilder = entityTypeBuilder;
                principalEntityType = entityTypeBuilder.Metadata;
            }
            else
            {
                principalEntityType = model.FindEntityType(Metadata.PrincipalEntityType.Name);
                if (principalEntityType == null)
                {
                    var ownership = Metadata.PrincipalEntityType.FindOwnership();
                    if (Metadata.PrincipalEntityType.HasSharedClrType
                        && ownership is { PrincipalEntityType.IsInModel: true })
                    {
                        principalEntityType = model.FindEntityType(
                            Metadata.PrincipalEntityType.ClrType,
                            ownership.PrincipalToDependent!.Name,
                            ownership.PrincipalEntityType);
                        if (principalEntityType == null)
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }

                principalEntityTypeBuilder = principalEntityType.Builder;
            }
        }

        InternalEntityTypeBuilder dependentEntityTypeBuilder;
        EntityType? dependentEntityType;
        if (Metadata.DeclaringEntityType.IsInModel)
        {
            dependentEntityTypeBuilder = Metadata.DeclaringEntityType.Builder;
            dependentEntityType = Metadata.DeclaringEntityType;
        }
        else
        {
            if ((Metadata.DeclaringEntityType.Name == entityTypeBuilder.Metadata.Name
                    || Metadata.DeclaringEntityType.ClrType == entityTypeBuilder.Metadata.ClrType)
                && (!principalEntityType.HasSharedClrType
                    || principalEntityType != entityTypeBuilder.Metadata))
            {
                dependentEntityTypeBuilder = entityTypeBuilder;
                dependentEntityType = entityTypeBuilder.Metadata;
            }
            else
            {
                dependentEntityType = model.FindEntityType(Metadata.DeclaringEntityType.Name);
                if (dependentEntityType == null)
                {
                    using (ModelBuilder.Metadata.DelayConventions())
                    {
                        if (Metadata.DeclaringEntityType.HasSharedClrType
                            || model.IsShared(Metadata.DeclaringEntityType.ClrType))
                        {
                            if (Metadata is { IsOwnership: true, PrincipalToDependent: not null })
                            {
                                var name = principalEntityType.GetOwnedName(
                                    Metadata.DeclaringEntityType.ShortName(), Metadata.PrincipalToDependent.Name);
                                dependentEntityType = ModelBuilder.SharedTypeEntity(
                                    name,
                                    Metadata.DeclaringEntityType.ClrType,
                                    Metadata.DeclaringEntityType.GetConfigurationSource(),
                                    shouldBeOwned: true)!.Metadata;
                            }
                            else
                            {
                                return null;
                            }
                        }
                        else
                        {
                            dependentEntityType =
                                ModelBuilder.Entity(
                                        Metadata.DeclaringEntityType.ClrType,
                                        configurationSource,
                                        shouldBeOwned: Metadata.DeclaringEntityType.IsOwned())
                                    !.Metadata;
                        }
                    }
                }

                dependentEntityTypeBuilder = dependentEntityType.Builder;
            }
        }

        if (!Metadata.IsOwnership
            && !Metadata.GetConfigurationSource().Overrides(ConfigurationSource.Explicit))
        {
            if (principalEntityType.IsOwned()
                && Metadata.DependentToPrincipal != null
                && !dependentEntityType.IsInOwnershipPath(principalEntityType))
            {
                // An entity type can have a navigation to a principal owned type only if it's in the ownership path
                return null;
            }

            if (dependentEntityType.IsOwned()
                && Metadata.PrincipalToDependent != null
                && (dependentEntityType.FindOwnership()?.PrincipalEntityType == principalEntityType
                    || !dependentEntityType.IsInOwnershipPath(principalEntityType)))
            {
                // Only a type in the ownership path can have a navigation to an owned dependent
                return null;
            }
        }

        if (dependentEntityType.GetForeignKeys().Contains(Metadata, ReferenceEqualityComparer.Instance))
        {
            Check.DebugAssert(Metadata.IsInModel, "Metadata isn't in the model");

            return Metadata.Builder;
        }

        IReadOnlyList<Property> dependentProperties;
        IReadOnlyList<Property> principalProperties;
        if (Metadata.GetPrincipalKeyConfigurationSource()?.Overrides(configurationSource) != true)
        {
            principalProperties = new List<Property>();
        }
        else
        {
            principalProperties = principalEntityTypeBuilder.GetActualProperties(Metadata.PrincipalKey.Properties, configurationSource)
                ?? new List<Property>();
        }

        if ((principalProperties.Count == 0
                && Metadata.GetPropertiesConfigurationSource()?.Overrides(ConfigurationSource.Explicit) != true)
            || Metadata.GetPropertiesConfigurationSource()?.Overrides(configurationSource) != true)
        {
            dependentProperties = new List<Property>();
        }
        else
        {
            dependentProperties = dependentEntityTypeBuilder.GetActualProperties(Metadata.Properties, configurationSource)
                ?? new List<Property>();
        }

        if (dependentProperties.Count != 0)
        {
            if (!CanSetForeignKey(
                    dependentProperties,
                    dependentEntityType,
                    principalProperties.Count != 0 ? principalProperties : Metadata.PrincipalKey.Properties,
                    principalEntityType,
                    configurationSource,
                    out var resetPrincipalKey))
            {
                dependentProperties = new List<Property>();
            }
            else if (resetPrincipalKey)
            {
                principalProperties = new List<Property>();
            }
        }

        return ReplaceForeignKey(
            configurationSource,
            principalEntityTypeBuilder,
            dependentEntityTypeBuilder,
            dependentProperties: dependentProperties,
            principalProperties: principalProperties);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool AreCompatible(
        EntityType principalEntityType,
        EntityType dependentEntityType,
        MemberInfo? navigationToPrincipal,
        MemberInfo? navigationToDependent,
        IReadOnlyList<Property>? dependentProperties,
        IReadOnlyList<Property>? principalProperties,
        bool? isUnique,
        ConfigurationSource? configurationSource)
        => ForeignKey.AreCompatible(
            principalEntityType,
            dependentEntityType,
            navigationToPrincipal,
            navigationToDependent,
            dependentProperties,
            principalProperties,
            isUnique,
            configurationSource == ConfigurationSource.Explicit);

    private bool CanSetRelatedTypes(
        EntityType principalEntityType,
        EntityType dependentEntityType,
        bool strictPrincipal,
        MemberIdentity? navigationToPrincipal,
        MemberIdentity? navigationToDependent,
        ConfigurationSource? configurationSource,
        bool shouldThrow,
        out bool shouldInvert,
        out bool shouldResetToPrincipal,
        out bool shouldResetToDependent,
        out bool shouldResetPrincipalProperties,
        out bool shouldResetDependentProperties,
        out bool? shouldBeUnique)
    {
        shouldInvert = false;
        shouldResetToPrincipal = false;
        shouldResetToDependent = false;
        shouldResetPrincipalProperties = false;
        shouldResetDependentProperties = false;
        shouldBeUnique = null;

        var sameHierarchyInvertedNavigations =
            (principalEntityType.IsAssignableFrom(dependentEntityType)
                || dependentEntityType.IsAssignableFrom(principalEntityType))
            && (((navigationToPrincipal != null)
                    && (navigationToPrincipal.Value.Name == Metadata.PrincipalToDependent?.Name))
                || ((navigationToDependent != null)
                    && (navigationToDependent.Value.Name == Metadata.DependentToPrincipal?.Name))
                || ((navigationToPrincipal == null)
                    && (navigationToDependent == null)
                    && principalEntityType == Metadata.DeclaringEntityType
                    && dependentEntityType == Metadata.PrincipalEntityType));

        var someAspectsFitNonInverted = false;
        if (!sameHierarchyInvertedNavigations
            && CanSetRelatedTypes(
                principalEntityType,
                dependentEntityType,
                navigationToPrincipal,
                navigationToDependent,
                configurationSource,
                inverted: false,
                shouldThrow: false,
                out shouldResetToPrincipal,
                out shouldResetToDependent,
                out shouldResetPrincipalProperties,
                out shouldResetDependentProperties,
                out shouldBeUnique))
        {
            if (!shouldResetToPrincipal
                && !shouldResetToDependent)
            {
                return true;
            }

            someAspectsFitNonInverted = true;
        }

        var canInvert = configurationSource?.Overrides(Metadata.GetPrincipalEndConfigurationSource()) == true;
        if ((!strictPrincipal
                || canInvert)
            && CanSetRelatedTypes(
                dependentEntityType,
                principalEntityType,
                navigationToDependent,
                navigationToPrincipal,
                configurationSource,
                strictPrincipal,
                shouldThrow: false,
                out var invertedShouldResetToPrincipal,
                out var invertedShouldResetToDependent,
                out _,
                out _,
                out var invertedShouldBeUnique)
            && (!someAspectsFitNonInverted
                || (!invertedShouldResetToPrincipal
                    && !invertedShouldResetToDependent)))
        {
            shouldInvert = true;
            shouldResetToPrincipal = invertedShouldResetToDependent;
            shouldResetToDependent = invertedShouldResetToPrincipal;
            shouldBeUnique = invertedShouldBeUnique;
            return true;
        }

        if (!someAspectsFitNonInverted
            && shouldThrow)
        {
            if (strictPrincipal
                && principalEntityType.IsKeyless)
            {
                throw new InvalidOperationException(
                    CoreStrings.PrincipalKeylessType(
                        principalEntityType.DisplayName(),
                        Metadata.PrincipalEntityType.DisplayName()
                        + (Metadata.PrincipalToDependent == null
                            ? ""
                            : "." + Metadata.PrincipalToDependent.Name),
                        Metadata.DeclaringEntityType.DisplayName()
                        + (Metadata.DependentToPrincipal == null
                            ? ""
                            : "." + Metadata.DependentToPrincipal.Name)));
            }

            if (canInvert)
            {
                throw new InvalidOperationException(
                    CoreStrings.EntityTypesNotInRelationship(
                        dependentEntityType.DisplayName(),
                        principalEntityType.DisplayName(),
                        Metadata.DeclaringEntityType.DisplayName(),
                        Metadata.PrincipalEntityType.DisplayName()));
            }
        }

        return someAspectsFitNonInverted;
    }

    private bool CanSetRelatedTypes(
        EntityType principalEntityType,
        EntityType dependentEntityType,
        MemberIdentity? navigationToPrincipal,
        MemberIdentity? navigationToDependent,
        ConfigurationSource? configurationSource,
        bool inverted,
        bool shouldThrow,
        out bool shouldResetToPrincipal,
        out bool shouldResetToDependent,
        out bool shouldResetPrincipalProperties,
        out bool shouldResetDependentProperties,
        out bool? shouldBeUnique)
    {
        shouldResetToPrincipal = false;
        shouldResetToDependent = false;
        shouldResetPrincipalProperties = false;
        shouldResetDependentProperties = false;
        shouldBeUnique = null;

        if (!Metadata.DeclaringEntityType.IsAssignableFrom(dependentEntityType)
            && !dependentEntityType.IsAssignableFrom(Metadata.DeclaringEntityType))
        {
            return false;
        }

        if (!Metadata.PrincipalEntityType.IsAssignableFrom(principalEntityType)
            && !principalEntityType.IsAssignableFrom(Metadata.PrincipalEntityType))
        {
            return false;
        }

        if (inverted)
        {
            if (dependentEntityType.IsKeyless
                && !configurationSource.OverridesStrictly(dependentEntityType.GetIsKeylessConfigurationSource()))
            {
                return false;
            }
        }
        else
        {
            if (principalEntityType.IsKeyless
                && !configurationSource.OverridesStrictly(principalEntityType.GetIsKeylessConfigurationSource()))
            {
                return false;
            }
        }

        if (navigationToPrincipal != null)
        {
            if (!configurationSource.HasValue
                || !CanSetNavigation(
                    navigationToPrincipal.Value,
                    pointsToPrincipal: true,
                    configurationSource.Value,
                    shouldThrow,
                    shouldBeUnique: out _))
            {
                return false;
            }

            if (Metadata.DependentToPrincipal != null
                && navigationToPrincipal.Value.Name != Metadata.DependentToPrincipal.Name)
            {
                shouldResetToPrincipal = true;
            }
        }
        else
        {
            bool? invertedShouldBeUnique = null;
            var navigationToPrincipalProperty = Metadata.DependentToPrincipal?.GetIdentifyingMemberInfo();
            if (navigationToPrincipalProperty != null
                && !IsCompatible(
                    navigationToPrincipalProperty,
                    !inverted,
                    inverted ? principalEntityType : dependentEntityType,
                    inverted ? dependentEntityType : principalEntityType,
                    shouldThrow,
                    out invertedShouldBeUnique))
            {
                if (!configurationSource.HasValue
                    || !CanSetNavigation((string?)null, pointsToPrincipal: true, configurationSource.Value))
                {
                    return false;
                }

                shouldResetToPrincipal = true;
            }

            if (inverted)
            {
                shouldBeUnique = invertedShouldBeUnique;
            }
        }

        if (navigationToDependent != null)
        {
            if (!configurationSource.HasValue
                || !CanSetNavigation(
                    navigationToDependent.Value,
                    pointsToPrincipal: false,
                    configurationSource.Value,
                    shouldThrow,
                    out var toDependentShouldBeUnique))
            {
                return false;
            }

            if (Metadata.PrincipalToDependent != null
                && navigationToDependent.Value.Name != Metadata.PrincipalToDependent.Name)
            {
                shouldResetToDependent = true;
            }

            if (toDependentShouldBeUnique != null)
            {
                shouldBeUnique = toDependentShouldBeUnique;
            }
        }
        else
        {
            bool? toDependentShouldBeUnique = null;
            var navigationToDependentProperty = Metadata.PrincipalToDependent?.GetIdentifyingMemberInfo();
            if (navigationToDependentProperty != null
                && !IsCompatible(
                    navigationToDependentProperty,
                    inverted,
                    inverted ? principalEntityType : dependentEntityType,
                    inverted ? dependentEntityType : principalEntityType,
                    shouldThrow,
                    out toDependentShouldBeUnique))
            {
                if (!configurationSource.HasValue
                    || !CanSetNavigation((string?)null, pointsToPrincipal: false, configurationSource.Value))
                {
                    return false;
                }

                shouldResetToDependent = true;
            }

            if (!inverted
                && toDependentShouldBeUnique != null)
            {
                shouldBeUnique = toDependentShouldBeUnique;
            }
        }

        if (shouldBeUnique.HasValue
            && !CanSetIsUnique(shouldBeUnique.Value, configurationSource, out _))
        {
            return false;
        }

        if (!Property.AreCompatible(Metadata.PrincipalKey.Properties, principalEntityType))
        {
            if (!configurationSource.HasValue
                || !configurationSource.Value.Overrides(Metadata.GetPrincipalKeyConfigurationSource()))
            {
                return false;
            }

            shouldResetPrincipalProperties = true;
        }

        if (!Property.AreCompatible(Metadata.Properties, dependentEntityType))
        {
            if (!configurationSource.HasValue
                || !configurationSource.Value.Overrides(Metadata.GetPropertiesConfigurationSource()))
            {
                return false;
            }

            shouldResetDependentProperties = true;
        }

        return true;
    }

    [Flags]
    private enum Resolution
    {
        None = 0,
        Remove = 1 << 0,
        ResetToPrincipal = 1 << 1,
        ResetToDependent = 1 << 2,
        ResetDependentProperties = 1 << 3
    }

    /// <inheritdoc />
    IConventionForeignKey IConventionForeignKeyBuilder.Metadata
    {
        [DebuggerStepThrough]
        get => Metadata;
    }

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionForeignKeyBuilder? IConventionForeignKeyBuilder.HasAnnotation(string name, object? value, bool fromDataAnnotation)
        => (IConventionForeignKeyBuilder?)base.HasAnnotation(
            name, value, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionForeignKeyBuilder? IConventionForeignKeyBuilder.HasNonNullAnnotation(string name, object? value, bool fromDataAnnotation)
        => (IConventionForeignKeyBuilder?)base.HasNonNullAnnotation(
            name, value, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionForeignKeyBuilder? IConventionForeignKeyBuilder.HasNoAnnotation(string name, bool fromDataAnnotation)
        => (IConventionForeignKeyBuilder?)base.HasNoAnnotation(
            name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionForeignKeyBuilder? IConventionForeignKeyBuilder.HasEntityTypes(
        IConventionEntityType principalEntityType,
        IConventionEntityType dependentEntityType,
        bool fromDataAnnotation)
        => HasEntityTypes(
            (EntityType)principalEntityType, (EntityType)dependentEntityType,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionForeignKeyBuilder.CanSetEntityTypes(
        IConventionEntityType principalEntityType,
        IConventionEntityType dependentEntityType,
        bool fromDataAnnotation)
        => CanSetEntityTypes(
            (EntityType)principalEntityType, (EntityType)dependentEntityType,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionForeignKeyBuilder.CanInvert(IReadOnlyList<IConventionProperty>? newForeignKeyProperties, bool fromDataAnnotation)
        => CanInvert(
            (IReadOnlyList<Property>?)newForeignKeyProperties,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionForeignKeyBuilder? IConventionForeignKeyBuilder.HasForeignKey(
        IReadOnlyList<string>? properties,
        bool fromDataAnnotation)
        => HasForeignKey(
            properties,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionForeignKeyBuilder? IConventionForeignKeyBuilder.HasForeignKey(
        IReadOnlyList<IConventionProperty>? properties,
        bool fromDataAnnotation)
        => HasForeignKey(
            properties as IReadOnlyList<Property> ?? properties?.Cast<Property>().ToList(),
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionForeignKeyBuilder.CanSetForeignKey(
        IReadOnlyList<string>? properties,
        bool fromDataAnnotation)
        => CanSetForeignKey(
            properties,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionForeignKeyBuilder.CanSetForeignKey(IReadOnlyList<IConventionProperty>? properties, bool fromDataAnnotation)
        => CanSetForeignKey(
            properties as IReadOnlyList<Property> ?? properties?.Cast<Property>().ToList(),
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionForeignKeyBuilder? IConventionForeignKeyBuilder.HasPrincipalKey(
        IReadOnlyList<string>? properties,
        bool fromDataAnnotation)
        => HasPrincipalKey(
            properties,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionForeignKeyBuilder? IConventionForeignKeyBuilder.HasPrincipalKey(
        IReadOnlyList<IConventionProperty>? properties,
        bool fromDataAnnotation)
        => HasPrincipalKey(
            properties as IReadOnlyList<Property> ?? properties?.Cast<Property>().ToList(),
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionForeignKeyBuilder.CanSetPrincipalKey(
        IReadOnlyList<string>? properties,
        bool fromDataAnnotation)
        => CanSetPrincipalKey(
            properties,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionForeignKeyBuilder.CanSetPrincipalKey(IReadOnlyList<IConventionProperty>? properties, bool fromDataAnnotation)
        => CanSetPrincipalKey(
            properties as IReadOnlyList<Property> ?? properties?.Cast<Property>().ToList(),
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionForeignKeyBuilder? IConventionForeignKeyBuilder.HasNavigation(
        string? name,
        bool pointsToPrincipal,
        bool fromDataAnnotation)
        => HasNavigation(
            name, pointsToPrincipal,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionForeignKeyBuilder? IConventionForeignKeyBuilder.HasNavigation(
        MemberInfo? property,
        bool pointsToPrincipal,
        bool fromDataAnnotation)
        => HasNavigation(
            property, pointsToPrincipal,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionForeignKeyBuilder? IConventionForeignKeyBuilder.HasNavigations(
        string? navigationToPrincipalName,
        string? navigationToDependentName,
        bool fromDataAnnotation)
        => HasNavigations(
            navigationToPrincipalName, navigationToDependentName,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionForeignKeyBuilder? IConventionForeignKeyBuilder.HasNavigations(
        MemberInfo? navigationToPrincipal,
        MemberInfo? navigationToDependent,
        bool fromDataAnnotation)
        => HasNavigations(
            navigationToPrincipal, navigationToDependent,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionForeignKeyBuilder.CanSetNavigation(
        MemberInfo? property,
        bool pointsToPrincipal,
        bool fromDataAnnotation)
        => CanSetNavigation(
            property, pointsToPrincipal,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionForeignKeyBuilder.CanSetNavigation(string? name, bool pointsToPrincipal, bool fromDataAnnotation)
        => CanSetNavigation(
            name, pointsToPrincipal,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionForeignKeyBuilder.CanSetNavigations(
        MemberInfo? navigationToPrincipal,
        MemberInfo? navigationToDependent,
        bool fromDataAnnotation)
        => CanSetNavigations(
            navigationToPrincipal, navigationToDependent,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionForeignKeyBuilder.CanSetNavigations(
        string? navigationToPrincipalName,
        string? navigationToDependentName,
        bool fromDataAnnotation)
        => CanSetNavigations(
            navigationToPrincipalName, navigationToDependentName,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionForeignKeyBuilder? IConventionForeignKeyBuilder.IsRequired(bool? required, bool fromDataAnnotation)
        => IsRequired(required, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionForeignKeyBuilder.CanSetIsRequired(bool? required, bool fromDataAnnotation)
        => CanSetIsRequired(required, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionForeignKeyBuilder? IConventionForeignKeyBuilder.IsRequiredDependent(bool? required, bool fromDataAnnotation)
        => IsRequiredDependent(required, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionForeignKeyBuilder.CanSetIsRequiredDependent(bool? required, bool fromDataAnnotation)
        => CanSetIsRequiredDependent(
            required, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionForeignKeyBuilder? IConventionForeignKeyBuilder.IsOwnership(bool? ownership, bool fromDataAnnotation)
        => IsOwnership(ownership, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    bool IConventionForeignKeyBuilder.CanSetIsOwnership(bool? ownership, bool fromDataAnnotation)
        => CanSetIsOwnership(ownership, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionForeignKeyBuilder? IConventionForeignKeyBuilder.OnDelete(
        DeleteBehavior? deleteBehavior,
        bool fromDataAnnotation)
        => OnDelete(deleteBehavior, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    bool IConventionForeignKeyBuilder.CanSetOnDelete(DeleteBehavior? deleteBehavior, bool fromDataAnnotation)
        => CanSetDeleteBehavior(
            deleteBehavior, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionForeignKeyBuilder? IConventionForeignKeyBuilder.IsUnique(bool? unique, bool fromDataAnnotation)
        => IsUnique(unique, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    bool IConventionForeignKeyBuilder.CanSetIsUnique(bool? unique, bool fromDataAnnotation)
        => CanSetIsUnique(unique, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
}

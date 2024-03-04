// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A base type for conventions that perform configuration based on an attribute applied to a navigation.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
/// <typeparam name="TAttribute">The attribute type to look for.</typeparam>
public abstract class NavigationAttributeConventionBase<TAttribute>
    where TAttribute : Attribute
{
    /// <summary>
    ///     Creates a new instance of <see cref="NavigationAttributeConventionBase{TAttribute}" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    protected NavigationAttributeConventionBase(ProviderConventionSetBuilderDependencies dependencies)
    {
        Dependencies = dependencies;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

    /// <summary>
    ///     Called after an entity type is ignored.
    /// </summary>
    /// <param name="modelBuilder">The builder for the model.</param>
    /// <param name="name">The name of the ignored entity type.</param>
    /// <param name="type">The ignored entity type.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessTypeIgnored(
        IConventionModelBuilder modelBuilder,
        string name,
        Type? type,
        IConventionContext<string> context)
    {
        if (type == null)
        {
            return;
        }

        var navigations = new List<(PropertyInfo, Type)>();
        foreach (var navigationPropertyInfo in type.GetRuntimeProperties())
        {
            var targetClrType = FindCandidateNavigationWithAttributePropertyType(navigationPropertyInfo, modelBuilder.Metadata);
            if (targetClrType == null)
            {
                continue;
            }

            navigations.Add((navigationPropertyInfo, targetClrType));
        }

        if (navigations.Count == 0)
        {
            return;
        }

        Sort(navigations);

        foreach (var navigationTuple in navigations)
        {
            var (navigationPropertyInfo, targetClrType) = navigationTuple;
            var attributes = navigationPropertyInfo.GetCustomAttributes<TAttribute>(inherit: true);
            foreach (var attribute in attributes)
            {
                ProcessTypeIgnored(modelBuilder, type, navigationPropertyInfo, targetClrType, attribute, context);
                if (((ConventionContext<string>)context).ShouldStopProcessing())
                {
                    return;
                }
            }
        }
    }

    /// <summary>
    ///     Called after an entity type is added to the model.
    /// </summary>
    /// <param name="entityTypeBuilder">The builder for the entity type.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessEntityTypeAdded(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionContext<IConventionEntityTypeBuilder> context)
    {
        var navigations = GetNavigationsWithAttribute(entityTypeBuilder.Metadata);
        if (navigations == null)
        {
            return;
        }

        foreach (var navigationTuple in navigations)
        {
            var (navigationPropertyInfo, targetClrType) = navigationTuple;
            var attributes = navigationPropertyInfo.GetCustomAttributes<TAttribute>(inherit: true);
            foreach (var attribute in attributes)
            {
                ProcessEntityTypeAdded(entityTypeBuilder, navigationPropertyInfo, targetClrType, attribute, context);
                if (((ConventionContext<IConventionEntityTypeBuilder>)context).ShouldStopProcessing())
                {
                    return;
                }
            }
        }
    }

    /// <summary>
    ///     Called after an entity type is removed from the model.
    /// </summary>
    /// <param name="modelBuilder">The builder for the model.</param>
    /// <param name="entityType">The removed entity type.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessEntityTypeRemoved(
        IConventionModelBuilder modelBuilder,
        IConventionEntityType entityType,
        IConventionContext<IConventionEntityType> context)
    {
        var navigations = GetNavigationsWithAttribute(entityType);
        if (navigations == null)
        {
            return;
        }

        foreach (var navigationTuple in navigations)
        {
            var (navigationPropertyInfo, targetClrType) = navigationTuple;
            var attributes = navigationPropertyInfo.GetCustomAttributes<TAttribute>(inherit: true);
            foreach (var attribute in attributes)
            {
                ProcessEntityTypeRemoved(modelBuilder, entityType, navigationPropertyInfo, targetClrType, attribute, context);
                if (((ConventionContext<IConventionEntityType>)context).ShouldStopProcessing())
                {
                    return;
                }
            }
        }
    }

    /// <summary>
    ///     Called after the base type of an entity type changes.
    /// </summary>
    /// <param name="entityTypeBuilder">The builder for the entity type.</param>
    /// <param name="newBaseType">The new base entity type.</param>
    /// <param name="oldBaseType">The old base entity type.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessEntityTypeBaseTypeChanged(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionEntityType? newBaseType,
        IConventionEntityType? oldBaseType,
        IConventionContext<IConventionEntityType> context)
    {
        var entityType = entityTypeBuilder.Metadata;
        if (entityTypeBuilder.Metadata.BaseType != newBaseType)
        {
            return;
        }

        var navigations = GetNavigationsWithAttribute(entityType);
        if (navigations == null)
        {
            return;
        }

        foreach (var navigationTuple in navigations)
        {
            var (navigationPropertyInfo, targetClrType) = navigationTuple;
            var attributes = navigationPropertyInfo.GetCustomAttributes<TAttribute>(inherit: true);
            foreach (var attribute in attributes)
            {
                ProcessEntityTypeBaseTypeChanged(
                    entityTypeBuilder, newBaseType, oldBaseType, navigationPropertyInfo, targetClrType, attribute, context);
                if (((ConventionContext<IConventionEntityType>)context).ShouldStopProcessing())
                {
                    return;
                }
            }
        }
    }

    private List<(PropertyInfo, Type)>? GetNavigationsWithAttribute(IConventionEntityType entityType)
    {
        var navigations = new List<(PropertyInfo, Type)>();
        foreach (var navigationPropertyInfo in entityType.GetRuntimeProperties().Values)
        {
            var targetClrType = FindCandidateNavigationWithAttributePropertyType(navigationPropertyInfo, entityType);
            if (targetClrType == null)
            {
                continue;
            }

            navigations.Add((navigationPropertyInfo, targetClrType));
        }

        if (navigations.Count == 0)
        {
            return null;
        }

        Sort(navigations);

        return navigations;
    }

    private static void Sort(List<(PropertyInfo, Type)> navigations)
        => navigations.Sort((x, y) => StringComparer.Ordinal.Compare(x.Item1.Name, y.Item1.Name));

    /// <summary>
    ///     Called after a navigation is added to the entity type.
    /// </summary>
    /// <param name="navigationBuilder">The builder for the navigation.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessNavigationAdded(
        IConventionNavigationBuilder navigationBuilder,
        IConventionContext<IConventionNavigationBuilder> context)
    {
        var navigation = navigationBuilder.Metadata;
        var attributes = GetAttributes<TAttribute>(navigation.DeclaringEntityType, navigation);
        foreach (var attribute in attributes)
        {
            ProcessNavigationAdded(navigationBuilder, attribute, context);
            if (((IReadableConventionContext)context).ShouldStopProcessing())
            {
                break;
            }
        }
    }

    /// <summary>
    ///     Called after a skip navigation is added to the entity type.
    /// </summary>
    /// <param name="skipNavigationBuilder">The builder for the skip navigation.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessSkipNavigationAdded(
        IConventionSkipNavigationBuilder skipNavigationBuilder,
        IConventionContext<IConventionSkipNavigationBuilder> context)
    {
        var skipNavigation = skipNavigationBuilder.Metadata;
        var attributes = GetAttributes<TAttribute>(skipNavigation.DeclaringEntityType, skipNavigation);
        foreach (var attribute in attributes)
        {
            ProcessSkipNavigationAdded(skipNavigationBuilder, attribute, context);
            if (((IReadableConventionContext)context).ShouldStopProcessing())
            {
                break;
            }
        }
    }

    /// <summary>
    ///     Called after the principal end of a foreign key is changed.
    /// </summary>
    /// <param name="relationshipBuilder">The builder for the foreign key.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessForeignKeyPrincipalEndChanged(
        IConventionForeignKeyBuilder relationshipBuilder,
        IConventionContext<IConventionForeignKeyBuilder> context)
    {
        var fk = relationshipBuilder.Metadata;
        var dependentToPrincipalAttributes = fk.DependentToPrincipal == null
            ? null
            : GetAttributes<TAttribute>(fk.DeclaringEntityType, fk.DependentToPrincipal);
        var principalToDependentAttributes = fk.PrincipalToDependent == null
            ? null
            : GetAttributes<TAttribute>(fk.PrincipalEntityType, fk.PrincipalToDependent);
        ProcessForeignKeyPrincipalEndChanged(
            relationshipBuilder, dependentToPrincipalAttributes, principalToDependentAttributes, context);
    }

    /// <summary>
    ///     Called after an entity type member is ignored.
    /// </summary>
    /// <param name="entityTypeBuilder">The builder for the entity type.</param>
    /// <param name="name">The name of the ignored member.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessEntityTypeMemberIgnored(
        IConventionEntityTypeBuilder entityTypeBuilder,
        string name,
        IConventionContext<string> context)
    {
        var navigationPropertyInfo = entityTypeBuilder.Metadata.GetRuntimeProperties().Find(name);
        if (navigationPropertyInfo == null)
        {
            return;
        }

        var targetClrType = FindCandidateNavigationWithAttributePropertyType(navigationPropertyInfo, entityTypeBuilder.Metadata);
        if (targetClrType == null)
        {
            return;
        }

        var attributes = navigationPropertyInfo.GetCustomAttributes<TAttribute>(true);
        foreach (var attribute in attributes)
        {
            ProcessEntityTypeMemberIgnored(entityTypeBuilder, navigationPropertyInfo, targetClrType, attribute, context);
            if (((ConventionContext<string>)context).ShouldStopProcessing())
            {
                return;
            }
        }
    }

    private Type? FindCandidateNavigationWithAttributePropertyType(PropertyInfo propertyInfo, IConventionModel model)
    {
        var targetClrType = Dependencies.MemberClassifier.FindCandidateNavigationPropertyType(propertyInfo, model, useAttributes: true, out _);
        return targetClrType != null
            && Attribute.IsDefined(propertyInfo, typeof(TAttribute), inherit: true)
                ? targetClrType
                : null;
    }

    private Type? FindCandidateNavigationWithAttributePropertyType(PropertyInfo propertyInfo, IConventionEntityType entityType)
        => Dependencies.MemberClassifier.GetNavigationCandidates(entityType, useAttributes: true)
                .TryGetValue(propertyInfo, out var target)
            && Attribute.IsDefined(propertyInfo, typeof(TAttribute), inherit: true)
                ? target.Type
                : null;

    /// <summary>
    ///     Returns the attributes applied to the given navigation.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="navigation">The navigation.</param>
    /// <typeparam name="TCustomAttribute">The attribute type to look for.</typeparam>
    /// <returns>The attributes applied to the given navigation.</returns>
    protected static IEnumerable<TCustomAttribute> GetAttributes<TCustomAttribute>(
        IConventionEntityType entityType,
        IConventionNavigation navigation)
        where TCustomAttribute : Attribute
        => GetAttributes<TCustomAttribute>(navigation.GetIdentifyingMemberInfo());

    /// <summary>
    ///     Returns the attributes applied to the given skip navigation.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="skipNavigation">The skip navigation.</param>
    /// <typeparam name="TCustomAttribute">The attribute type to look for.</typeparam>
    /// <returns>The attributes applied to the given skip navigation.</returns>
    protected static IEnumerable<TCustomAttribute> GetAttributes<TCustomAttribute>(
        IConventionEntityType entityType,
        IConventionSkipNavigation skipNavigation)
        where TCustomAttribute : Attribute
        => GetAttributes<TCustomAttribute>(skipNavigation.GetIdentifyingMemberInfo());

    private static IEnumerable<TCustomAttribute> GetAttributes<TCustomAttribute>(MemberInfo? memberInfo)
        where TCustomAttribute : Attribute
    {
        if (memberInfo == null)
        {
            return Enumerable.Empty<TCustomAttribute>();
        }

        return Attribute.IsDefined(memberInfo, typeof(TCustomAttribute), inherit: true)
            ? memberInfo.GetCustomAttributes<TCustomAttribute>(true)
            : Enumerable.Empty<TCustomAttribute>();
    }

    /// <summary>
    ///     Called for every navigation property that has an attribute after an entity type is ignored.
    /// </summary>
    /// <param name="modelBuilder">The builder for the model.</param>
    /// <param name="type">The ignored entity type.</param>
    /// <param name="navigationMemberInfo">The navigation member info.</param>
    /// <param name="targetClrType">The CLR type of the target entity type.</param>
    /// <param name="attribute">The attribute.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessTypeIgnored(
        IConventionModelBuilder modelBuilder,
        Type type,
        MemberInfo navigationMemberInfo,
        Type targetClrType,
        TAttribute attribute,
        IConventionContext<string> context)
        => throw new NotSupportedException();

    /// <summary>
    ///     Called for every navigation property that has an attribute after an entity type is added to the model.
    /// </summary>
    /// <param name="entityTypeBuilder">The builder for the entity type.</param>
    /// <param name="navigationMemberInfo">The navigation member info.</param>
    /// <param name="targetClrType">The CLR type of the target entity type</param>
    /// <param name="attribute">The attribute.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessEntityTypeAdded(
        IConventionEntityTypeBuilder entityTypeBuilder,
        MemberInfo navigationMemberInfo,
        Type targetClrType,
        TAttribute attribute,
        IConventionContext<IConventionEntityTypeBuilder> context)
        => throw new NotSupportedException();

    /// <summary>
    ///     Called for every navigation property that has an attribute after an entity type is removed.
    /// </summary>
    /// <param name="modelBuilder">The builder for the model.</param>
    /// <param name="entityType">The ignored entity type.</param>
    /// <param name="navigationMemberInfo">The navigation member info.</param>
    /// <param name="targetClrType">The CLR type of the target entity type.</param>
    /// <param name="attribute">The attribute.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessEntityTypeRemoved(
        IConventionModelBuilder modelBuilder,
        IConventionEntityType entityType,
        MemberInfo navigationMemberInfo,
        Type targetClrType,
        TAttribute attribute,
        IConventionContext<IConventionEntityType> context)
        => throw new NotSupportedException();

    /// <summary>
    ///     Called for every navigation property that has an attribute after the base type for an entity type is changed.
    /// </summary>
    /// <param name="entityTypeBuilder">The builder for the entity type.</param>
    /// <param name="newBaseType">The new base type.</param>
    /// <param name="oldBaseType">The old base type.</param>
    /// <param name="navigationMemberInfo">The navigation member info.</param>
    /// <param name="targetClrType">The CLR type of the target entity type.</param>
    /// <param name="attribute">The attribute.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessEntityTypeBaseTypeChanged(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionEntityType? newBaseType,
        IConventionEntityType? oldBaseType,
        MemberInfo navigationMemberInfo,
        Type targetClrType,
        TAttribute attribute,
        IConventionContext<IConventionEntityType> context)
        => throw new NotSupportedException();

    /// <summary>
    ///     Called after a navigation property that has an attribute is added to an entity type.
    /// </summary>
    /// <param name="navigationBuilder">The builder for the navigation.</param>
    /// <param name="attribute">The attribute.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessNavigationAdded(
        IConventionNavigationBuilder navigationBuilder,
        TAttribute attribute,
        IConventionContext<IConventionNavigationBuilder> context)
        => throw new NotSupportedException();

    /// <summary>
    ///     Called after a skip navigation property that has an attribute is added to an entity type.
    /// </summary>
    /// <param name="skipNavigationBuilder">The builder for the navigation.</param>
    /// <param name="attribute">The attribute.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessSkipNavigationAdded(
        IConventionSkipNavigationBuilder skipNavigationBuilder,
        TAttribute attribute,
        IConventionContext<IConventionSkipNavigationBuilder> context)
        => throw new NotSupportedException();

    /// <summary>
    ///     Called after a navigation property that has an attribute is ignored.
    /// </summary>
    /// <param name="entityTypeBuilder">The builder for the entity type.</param>
    /// <param name="navigationMemberInfo">The navigation member info.</param>
    /// <param name="targetClrType">The CLR type of the target entity type.</param>
    /// <param name="attribute">The attribute.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessEntityTypeMemberIgnored(
        IConventionEntityTypeBuilder entityTypeBuilder,
        MemberInfo navigationMemberInfo,
        Type targetClrType,
        TAttribute attribute,
        IConventionContext<string> context)
        => throw new NotSupportedException();

    /// <summary>
    ///     Called after the principal end of a foreign key is changed.
    /// </summary>
    /// <param name="relationshipBuilder">The builder for the foreign key.</param>
    /// <param name="dependentToPrincipalAttributes">The attributes on the dependent to principal navigation.</param>
    /// <param name="principalToDependentAttributes">The attributes on the principal to dependent navigation.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessForeignKeyPrincipalEndChanged(
        IConventionForeignKeyBuilder relationshipBuilder,
        IEnumerable<TAttribute>? dependentToPrincipalAttributes,
        IEnumerable<TAttribute>? principalToDependentAttributes,
        IConventionContext<IConventionForeignKeyBuilder> context)
        => throw new NotSupportedException();
}

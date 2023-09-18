// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that configures relationships between entity types based on the navigation properties
///     as long as there is no ambiguity as to which is the corresponding inverse navigation.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public class ComplexPropertyDiscoveryConvention :
    IEntityTypeAddedConvention,
    IEntityTypeBaseTypeChangedConvention,
    IEntityTypeMemberIgnoredConvention,
    IComplexPropertyAddedConvention,
    INavigationRemovedConvention,
    INavigationAddedConvention,
    IPropertyRemovedConvention,
    IPropertyAddedConvention,
    ISkipNavigationRemovedConvention,
    ISkipNavigationAddedConvention,
    IModelFinalizingConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="ComplexPropertyDiscoveryConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    public ComplexPropertyDiscoveryConvention(ProviderConventionSetBuilderDependencies dependencies)
    {
        Dependencies = dependencies;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

    private void DiscoverComplexProperties(
        IConventionTypeBaseBuilder typeBaseBuilder)
    {
        var typeBase = typeBaseBuilder.Metadata;
        foreach (var candidateMember in typeBase.GetRuntimeProperties().Values)
        {
            TryConfigureComplexProperty(candidateMember, typeBase);
        }
    }

    private bool TryConfigureComplexProperty(MemberInfo? candidateMember, IConventionTypeBase typeBase)
    {
        if (candidateMember == null
            || !typeBase.IsInModel
            || typeBase.IsIgnored(candidateMember.Name)
            || typeBase.FindMember(candidateMember.Name) != null
            || (candidateMember is PropertyInfo propertyInfo && propertyInfo.GetIndexParameters().Length != 0)
            || !Dependencies.MemberClassifier.IsCandidateComplexProperty(
                candidateMember, typeBase.Model, out var elementType, out var explicitlyConfigured))
        {
            return false;
        }

        var model = (Model)typeBase.Model;
        var targetClrType = (elementType ?? candidateMember.GetMemberType()).UnwrapNullableType();
        if (typeBase.Model.Builder.IsIgnored(targetClrType)
            || (typeBase is IReadOnlyComplexType complexType
                && complexType.IsContainedBy(targetClrType)))
        {
            return false;
        }

        if (!explicitlyConfigured
            && model.FindIsComplexConfigurationSource(targetClrType) == null)
        {
            AddComplexCandidate(candidateMember, typeBase.Builder);
            return false;
        }

        RemoveComplexCandidate(candidateMember.Name, typeBase.Builder);

        return typeBase.Builder.ComplexProperty(candidateMember, targetClrType) != null;
    }

    /// <inheritdoc />
    public virtual void ProcessEntityTypeAdded(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionContext<IConventionEntityTypeBuilder> context)
        => DiscoverComplexProperties(entityTypeBuilder);

    /// <inheritdoc />
    public void ProcessComplexPropertyAdded(
        IConventionComplexPropertyBuilder propertyBuilder,
        IConventionContext<IConventionComplexPropertyBuilder> context)
        => DiscoverComplexProperties(propertyBuilder.Metadata.ComplexType.Builder);

    /// <inheritdoc />
    public virtual void ProcessEntityTypeBaseTypeChanged(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionEntityType? newBaseType,
        IConventionEntityType? oldBaseType,
        IConventionContext<IConventionEntityType> context)
    {
        if (oldBaseType?.IsInModel == true)
        {
            DiscoverComplexProperties(oldBaseType.Builder);
        }

        var entityType = entityTypeBuilder.Metadata;
        if (entityType.BaseType == newBaseType)
        {
            DiscoverComplexProperties(entityType.Builder);
        }
    }

    /// <inheritdoc />
    public void ProcessPropertyRemoved(
        IConventionTypeBaseBuilder typeBaseBuilder,
        IConventionProperty property,
        IConventionContext<IConventionProperty> context)
        => TryConfigureComplexProperty(property.GetIdentifyingMemberInfo(), typeBaseBuilder.Metadata);

    /// <inheritdoc />
    public void ProcessSkipNavigationRemoved(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionSkipNavigation navigation,
        IConventionContext<IConventionSkipNavigation> context)
        => TryConfigureComplexProperty(navigation.GetIdentifyingMemberInfo(), entityTypeBuilder.Metadata);

    /// <inheritdoc />
    public virtual void ProcessNavigationRemoved(
        IConventionEntityTypeBuilder sourceEntityTypeBuilder,
        IConventionEntityTypeBuilder targetEntityTypeBuilder,
        string navigationName,
        MemberInfo? memberInfo,
        IConventionContext<string> context)
        => TryConfigureComplexProperty(memberInfo, sourceEntityTypeBuilder.Metadata);

    /// <inheritdoc />
    public void ProcessEntityTypeMemberIgnored(
        IConventionEntityTypeBuilder entityTypeBuilder,
        string name,
        IConventionContext<string> context)
        => RemoveComplexCandidate(name, entityTypeBuilder);

    /// <inheritdoc />
    public void ProcessNavigationAdded(
        IConventionNavigationBuilder navigationBuilder,
        IConventionContext<IConventionNavigationBuilder> context)
        => RemoveComplexCandidate(navigationBuilder.Metadata.Name, navigationBuilder.Metadata.DeclaringType.Builder);

    /// <inheritdoc />
    public void ProcessPropertyAdded(
        IConventionPropertyBuilder propertyBuilder,
        IConventionContext<IConventionPropertyBuilder> context)
        => RemoveComplexCandidate(propertyBuilder.Metadata.Name, propertyBuilder.Metadata.DeclaringType.Builder);

    /// <inheritdoc />
    public void ProcessSkipNavigationAdded(
        IConventionSkipNavigationBuilder skipNavigationBuilder,
        IConventionContext<IConventionSkipNavigationBuilder> context)
        => RemoveComplexCandidate(skipNavigationBuilder.Metadata.Name, skipNavigationBuilder.Metadata.DeclaringType.Builder);

    /// <inheritdoc />
    public void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            DiscoverMissedComplexProperties(entityType);
        }
    }

    private void DiscoverMissedComplexProperties(IConventionTypeBase typeBase)
    {
        var candidates = GetComplexCandidates(typeBase);
        if (candidates != null)
        {
            foreach (var candidatePair in candidates.OrderBy(v => v.Key))
            {
                TryConfigureComplexProperty(candidatePair.Value, typeBase);
            }

            typeBase.Builder.HasNoAnnotation(CoreAnnotationNames.ComplexCandidates);
        }

        foreach (var complexProperty in typeBase.GetComplexProperties())
        {
            DiscoverMissedComplexProperties(complexProperty.ComplexType);
        }
    }

    private static Dictionary<string, MemberInfo>? GetComplexCandidates(IConventionTypeBase typeBase)
        => (Dictionary<string, MemberInfo>?)typeBase[CoreAnnotationNames.ComplexCandidates];

    private static void AddComplexCandidate(
        MemberInfo memberInfo,
        IConventionTypeBaseBuilder typeBaseBuilder)
    {
        var candidates = GetComplexCandidates(typeBaseBuilder.Metadata);
        if (candidates != null)
        {
            candidates[memberInfo.Name] = memberInfo;
            return;
        }

        typeBaseBuilder.HasAnnotation(
            CoreAnnotationNames.ComplexCandidates,
            new Dictionary<string, MemberInfo> { { memberInfo.Name, memberInfo } });
    }

    private static void RemoveComplexCandidate(
        string name,
        IConventionTypeBaseBuilder typeBaseBuilder)
    {
        var candidates = GetComplexCandidates(typeBaseBuilder.Metadata);
        if (candidates != null)
        {
            candidates.Remove(name);
        }
    }
}

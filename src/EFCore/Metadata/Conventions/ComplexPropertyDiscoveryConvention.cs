// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Internal;
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
    /// <param name="useAttributes">Whether the convention will use attributes found on the members.</param>
    public ComplexPropertyDiscoveryConvention(
        ProviderConventionSetBuilderDependencies dependencies,
        bool useAttributes = true)
    {
        Dependencies = dependencies;
        UseAttributes = useAttributes;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

    /// <summary>
    ///     A value indicating whether the convention will use attributes found on the members.
    /// </summary>
    protected virtual bool UseAttributes { get; }

    /// <summary>
    ///    Discovers complex properties on the given structural type.
    /// </summary>
    /// <param name="structuralTypeBuilder">The type for which the properties will be discovered.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    protected virtual void DiscoverComplexProperties(
        IConventionTypeBaseBuilder structuralTypeBuilder,
        IConventionContext context)
    {
        var typeBase = structuralTypeBuilder.Metadata;
        foreach (var candidateMember in GetMembers(typeBase))
        {
            TryConfigureComplexProperty(candidateMember, typeBase, context);
        }
    }

    private void TryConfigureComplexProperty(MemberInfo? candidateMember, IConventionTypeBase typeBase, IConventionContext context)
    {
        if (candidateMember == null
            || !IsCandidateComplexProperty(candidateMember, typeBase, out var targetClrType))
        {
            return;
        }

        RemoveComplexCandidate(candidateMember.Name, typeBase.Builder);

        typeBase.Builder.ComplexProperty(candidateMember, targetClrType);
    }

    /// <summary>
    ///     Returns a value indicating whether the given member is a complex property candidate.
    /// </summary>
    /// <param name="memberInfo">The member.</param>
    /// <param name="structuralType">The type for which the properties will be discovered.</param>
    /// <param name="targetClrType">The complex type.</param>
    protected virtual bool IsCandidateComplexProperty(
        MemberInfo memberInfo, IConventionTypeBase structuralType, [NotNullWhen(true)] out Type? targetClrType)
    {
        if (!structuralType.IsInModel
            || structuralType.IsIgnored(memberInfo.Name)
            || structuralType.FindMember(memberInfo.Name) != null
            || (memberInfo is PropertyInfo propertyInfo && propertyInfo.GetIndexParameters().Length != 0)
            || !Dependencies.MemberClassifier.IsCandidateComplexProperty(
                memberInfo, structuralType.Model, UseAttributes, out var elementType, out var explicitlyConfigured))
        {
            targetClrType = null;
            return false;
        }

        var model = (Model)structuralType.Model;
        targetClrType = (elementType ?? memberInfo.GetMemberType()).UnwrapNullableType();
        if (structuralType.Model.Builder.IsIgnored(targetClrType)
            || (structuralType is IReadOnlyComplexType complexType
                && complexType.IsContainedBy(targetClrType)))
        {
            return false;
        }

        if (!explicitlyConfigured
            && model.FindIsComplexConfigurationSource(targetClrType) == null)
        {
            AddComplexCandidate(memberInfo, structuralType.Builder);
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Returns the CLR members from the given type that should be considered when discovering properties.
    /// </summary>
    /// <param name="structuralType">The type for which the properties will be discovered.</param>
    /// <returns>The CLR members to be considered.</returns>
    protected virtual IEnumerable<MemberInfo> GetMembers(IConventionTypeBase structuralType)
        => structuralType is IConventionComplexType
            ? structuralType.GetRuntimeProperties().Values.Cast<MemberInfo>()
                .Concat(structuralType.GetRuntimeFields().Values)
            : structuralType.GetRuntimeProperties().Values.Cast<MemberInfo>();

    /// <inheritdoc />
    public virtual void ProcessEntityTypeAdded(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionContext<IConventionEntityTypeBuilder> context)
        => DiscoverComplexProperties(entityTypeBuilder, context);

    /// <inheritdoc />
    public void ProcessComplexPropertyAdded(
        IConventionComplexPropertyBuilder propertyBuilder,
        IConventionContext<IConventionComplexPropertyBuilder> context)
        => DiscoverComplexProperties(propertyBuilder.Metadata.ComplexType.Builder, context);

    /// <inheritdoc />
    public virtual void ProcessEntityTypeBaseTypeChanged(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionEntityType? newBaseType,
        IConventionEntityType? oldBaseType,
        IConventionContext<IConventionEntityType> context)
    {
        if (oldBaseType?.IsInModel == true)
        {
            DiscoverComplexProperties(oldBaseType.Builder, context);
        }

        var entityType = entityTypeBuilder.Metadata;
        if (entityType.BaseType == newBaseType)
        {
            DiscoverComplexProperties(entityType.Builder, context);
        }
    }

    /// <inheritdoc />
    public void ProcessPropertyRemoved(
        IConventionTypeBaseBuilder typeBaseBuilder,
        IConventionProperty property,
        IConventionContext<IConventionProperty> context)
        => TryConfigureComplexProperty(property.GetIdentifyingMemberInfo(), typeBaseBuilder.Metadata, context);

    /// <inheritdoc />
    public void ProcessSkipNavigationRemoved(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionSkipNavigation navigation,
        IConventionContext<IConventionSkipNavigation> context)
        => TryConfigureComplexProperty(navigation.GetIdentifyingMemberInfo(), entityTypeBuilder.Metadata, context);

    /// <inheritdoc />
    public virtual void ProcessNavigationRemoved(
        IConventionEntityTypeBuilder sourceEntityTypeBuilder,
        IConventionEntityTypeBuilder targetEntityTypeBuilder,
        string navigationName,
        MemberInfo? memberInfo,
        IConventionContext<string> context)
        => TryConfigureComplexProperty(memberInfo, sourceEntityTypeBuilder.Metadata, context);

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
            DiscoverMissedComplexProperties(entityType, context);
        }
    }

    private void DiscoverMissedComplexProperties(IConventionTypeBase typeBase, IConventionContext context)
    {
        var candidates = GetComplexCandidates(typeBase);
        if (candidates != null)
        {
            foreach (var candidatePair in candidates.OrderBy(v => v.Key))
            {
                TryConfigureComplexProperty(candidatePair.Value, typeBase, context);
            }
        }

        foreach (var complexProperty in typeBase.GetComplexProperties())
        {
            DiscoverMissedComplexProperties(complexProperty.ComplexType, context);
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

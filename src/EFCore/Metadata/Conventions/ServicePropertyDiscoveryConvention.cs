// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that adds service properties to entity types.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public class ServicePropertyDiscoveryConvention :
    IEntityTypeAddedConvention,
    IEntityTypeBaseTypeChangedConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="ServicePropertyDiscoveryConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    /// <param name="useAttributes">Whether the convention will use attributes found on the members.</param>
    public ServicePropertyDiscoveryConvention(
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
    ///     Called after an entity type is added to the model.
    /// </summary>
    /// <param name="entityTypeBuilder">The builder for the entity type.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessEntityTypeAdded(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionContext<IConventionEntityTypeBuilder> context)
        => DiscoverServiceProperties(entityTypeBuilder, context);

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
        if (entityTypeBuilder.Metadata.BaseType == newBaseType)
        {
            DiscoverServiceProperties(entityTypeBuilder, context);
        }
    }

    /// <summary>
    ///    Discovers properties on the given structural type.
    /// </summary>
    /// <param name="structuralTypeBuilder">The type for which the properties will be discovered.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    protected virtual void DiscoverServiceProperties(
        IConventionTypeBaseBuilder structuralTypeBuilder,
        IConventionContext context)
    {
        if (structuralTypeBuilder is not IConventionEntityTypeBuilder entityTypeBuilder)
        {
            return;
        }

        var entityType = entityTypeBuilder.Metadata;
        foreach (var memberInfo in GetMembers(entityType))
        {
            if (!IsCandidateServiceProperty(memberInfo, entityType, out var factory))
            {
                continue;
            }

            entityTypeBuilder.ServiceProperty(memberInfo)?.HasParameterBinding(
                (ServiceParameterBinding)factory.Bind(entityType, memberInfo.GetMemberType(), memberInfo.GetSimpleMemberName()));
        }
    }

    /// <summary>
    ///     Returns the CLR members from the given type that should be considered when discovering properties.
    /// </summary>
    /// <param name="structuralType">The type for which the properties will be discovered.</param>
    /// <returns>The CLR members to be considered.</returns>
    protected virtual IEnumerable<MemberInfo> GetMembers(IConventionTypeBase structuralType)
        => structuralType.GetRuntimeProperties().Values.Cast<MemberInfo>()
            .Concat(structuralType.GetRuntimeFields().Values);

    /// <summary>
    ///     Returns a value indicating whether the given member is a service property candidate.
    /// </summary>
    /// <param name="memberInfo">The member.</param>
    /// <param name="structuralType">The type for which the properties will be discovered.</param>
    /// <param name="factory">The parameter binding factory for the property.</param>
    protected virtual bool IsCandidateServiceProperty(
        MemberInfo memberInfo, IConventionTypeBase structuralType, [NotNullWhen(true)] out IParameterBindingFactory? factory)
    {
        factory = null;
        var model = (Model)structuralType.Model;
        if (structuralType is not IConventionEntityType entityType
            || !entityType.Builder.CanHaveServiceProperty(memberInfo)
            || model.FindIsComplexConfigurationSource(memberInfo.GetMemberType().UnwrapNullableType()) != null)
        {
            return false;
        }

        factory = Dependencies.MemberClassifier.FindServicePropertyCandidateBindingFactory(memberInfo, model, UseAttributes);
        if (factory == null)
        {
            return false;
        }

        var memberType = memberInfo.GetMemberType();
        return !entityType.HasServiceProperties()
            || !entityType.GetServiceProperties().Any(p => p.ClrType == memberType);
    }
}

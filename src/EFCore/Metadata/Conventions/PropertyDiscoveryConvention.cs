// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that adds properties to entity types corresponding to scalar public properties on the CLR type.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public class PropertyDiscoveryConvention :
    IEntityTypeAddedConvention,
    IEntityTypeBaseTypeChangedConvention,
    IComplexPropertyAddedConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="PropertyDiscoveryConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    /// <param name="useAttributes">Whether the convention will use attributes found on the members.</param>
    public PropertyDiscoveryConvention(
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

    /// <inheritdoc />
    public virtual void ProcessEntityTypeAdded(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionContext<IConventionEntityTypeBuilder> context)
        => DiscoverPrimitiveProperties(entityTypeBuilder, context);

    /// <inheritdoc />
    public virtual void ProcessEntityTypeBaseTypeChanged(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionEntityType? newBaseType,
        IConventionEntityType? oldBaseType,
        IConventionContext<IConventionEntityType> context)
    {
        if ((newBaseType == null
                || oldBaseType != null)
            && entityTypeBuilder.Metadata.BaseType == newBaseType)
        {
            DiscoverPrimitiveProperties(entityTypeBuilder, context);
        }
    }

    /// <inheritdoc />
    public void ProcessComplexPropertyAdded(
        IConventionComplexPropertyBuilder propertyBuilder,
        IConventionContext<IConventionComplexPropertyBuilder> context)
        => DiscoverPrimitiveProperties(propertyBuilder.Metadata.ComplexType.Builder, context);

    /// <summary>
    ///    Discovers properties on the given structural type.
    /// </summary>
    /// <param name="structuralTypeBuilder">The type for which the properties will be discovered.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    protected virtual void DiscoverPrimitiveProperties(
        IConventionTypeBaseBuilder structuralTypeBuilder,
        IConventionContext context)
    {
        var structuralType = structuralTypeBuilder.Metadata;
        foreach (var propertyInfo in GetMembers(structuralType))
        {
            if (!IsCandidatePrimitiveProperty(propertyInfo, structuralType, out var mapping))
            {
                continue;
            }

            var propertyBuilder = structuralTypeBuilder.Property(propertyInfo);
            if (mapping?.ElementTypeMapping != null)
            {
                var elementType = propertyInfo.GetMemberType().TryGetElementType(typeof(IEnumerable<>));
                if (elementType != null)
                {
                    propertyBuilder?.SetElementType(elementType);
                }
            }
        }
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

    /// <summary>
    ///     Returns a value indicating whether the given member is a primitive property candidate.
    /// </summary>
    /// <param name="memberInfo">The member.</param>
    /// <param name="structuralType">The type for which the properties will be discovered.</param>
    /// <param name="mapping">The type mapping for the property.</param>
    protected virtual bool IsCandidatePrimitiveProperty(
        MemberInfo memberInfo, IConventionTypeBase structuralType, out CoreTypeMapping? mapping)
        => Dependencies.MemberClassifier.IsCandidatePrimitiveProperty(memberInfo, structuralType.Model, UseAttributes, out mapping)
            && ((Model)structuralType.Model).FindIsComplexConfigurationSource(memberInfo.GetMemberType().UnwrapNullableType()) == null;
}

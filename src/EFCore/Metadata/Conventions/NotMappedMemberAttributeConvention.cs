// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that ignores members on entity types that have the <see cref="NotMappedAttribute" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public class NotMappedMemberAttributeConvention : IEntityTypeAddedConvention, IComplexPropertyAddedConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="NotMappedMemberAttributeConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    public NotMappedMemberAttributeConvention(ProviderConventionSetBuilderDependencies dependencies)
    {
        Dependencies = dependencies;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

    /// <inheritdoc />
    public virtual void ProcessEntityTypeAdded(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionContext<IConventionEntityTypeBuilder> context)
    {
        var entityType = entityTypeBuilder.Metadata;
        var members = entityType.GetRuntimeProperties().Values.Cast<MemberInfo>()
            .Concat(entityType.GetRuntimeFields().Values);

        foreach (var member in members)
        {
            if (Attribute.IsDefined(member, typeof(NotMappedAttribute), inherit: true)
                && ShouldIgnore(member))
            {
                entityTypeBuilder.Ignore(member.GetSimpleMemberName(), fromDataAnnotation: true);
            }
        }
    }

    /// <inheritdoc />
    public void ProcessComplexPropertyAdded(
        IConventionComplexPropertyBuilder propertyBuilder,
        IConventionContext<IConventionComplexPropertyBuilder> context)
    {
        var complexType = propertyBuilder.Metadata.ComplexType;
        var members = complexType.GetRuntimeProperties().Values.Cast<MemberInfo>()
            .Concat(complexType.GetRuntimeFields().Values);

        foreach (var member in members)
        {
            if (Attribute.IsDefined(member, typeof(NotMappedAttribute), inherit: true)
                && ShouldIgnore(member))
            {
                complexType.Builder.Ignore(member.GetSimpleMemberName(), fromDataAnnotation: true);
            }
        }
    }

    /// <summary>
    ///     Returns a value indicating whether the given CLR member should be ignored.
    /// </summary>
    /// <param name="memberInfo">The member.</param>
    /// <returns><see langword="true" /> if the member should be ignored.</returns>
    protected virtual bool ShouldIgnore(MemberInfo memberInfo)
        => true;
}

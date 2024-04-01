// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that configures types that have the <see cref="ComplexTypeAttribute" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public class ComplexTypeAttributeConvention : TypeAttributeConventionBase<ComplexTypeAttribute>,
    IComplexPropertyAddedConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="ComplexTypeAttributeConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    public ComplexTypeAttributeConvention(ProviderConventionSetBuilderDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <summary>
    ///     Called after a complex property is added to a type-like object.
    /// </summary>
    /// <param name="propertyBuilder">The builder for the complex property.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public override void ProcessComplexPropertyAdded(
        IConventionComplexPropertyBuilder propertyBuilder,
        IConventionContext<IConventionComplexPropertyBuilder> context)
    {
        var complexType = propertyBuilder.Metadata.ComplexType;
        var memberTypes = complexType.GetRuntimeProperties().Values.Select(e => e.PropertyType)
            .Concat(complexType.GetRuntimeFields().Values.Select(e => e.FieldType));

        foreach (var memberType in memberTypes)
        {
            if (Attribute.IsDefined(memberType, typeof(ComplexTypeAttribute), inherit: true))
            {
                complexType.Builder.Metadata.Model.Builder.ComplexType(memberType);
            }
        }
    }

    /// <summary>
    ///     Called after an entity type is added to the model if it has an attribute.
    /// </summary>
    /// <param name="entityTypeBuilder">The builder for the entity type.</param>
    /// <param name="attribute">The attribute.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    protected override void ProcessEntityTypeAdded(
        IConventionEntityTypeBuilder entityTypeBuilder,
        ComplexTypeAttribute attribute,
        IConventionContext<IConventionEntityTypeBuilder> context)
    {
        entityTypeBuilder.Metadata.Model.Builder.ComplexType(entityTypeBuilder.Metadata.ClrType);

        if (!entityTypeBuilder.Metadata.IsInModel)
        {
            context.StopProcessing();
        }
    }
}

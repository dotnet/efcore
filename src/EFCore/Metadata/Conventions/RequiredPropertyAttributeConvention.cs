// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that configures properties as required if they have the <see cref="RequiredAttribute" /> applied.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public class RequiredPropertyAttributeConvention : PropertyAttributeConventionBase<RequiredAttribute>,
    IComplexPropertyAddedConvention,
    IComplexPropertyFieldChangedConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="RequiredPropertyAttributeConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    public RequiredPropertyAttributeConvention(ProviderConventionSetBuilderDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <summary>
    ///     Called after a property is added to the entity type with an attribute on the associated CLR property or field.
    /// </summary>
    /// <param name="propertyBuilder">The builder for the property.</param>
    /// <param name="attribute">The attribute.</param>
    /// <param name="clrMember">The member that has the attribute.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    protected override void ProcessPropertyAdded(
        IConventionPropertyBuilder propertyBuilder,
        RequiredAttribute attribute,
        MemberInfo clrMember,
        IConventionContext context)
        => propertyBuilder.IsRequired(true, fromDataAnnotation: true);

    /// <summary>
    ///     Called after a complex property is added to a type with an attribute on the associated CLR property or field.
    /// </summary>
    /// <param name="propertyBuilder">The builder for the property.</param>
    /// <param name="attribute">The attribute.</param>
    /// <param name="clrMember">The member that has the attribute.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    protected override void ProcessPropertyAdded(
        IConventionComplexPropertyBuilder propertyBuilder,
        RequiredAttribute attribute,
        MemberInfo clrMember,
        IConventionContext context)
        => propertyBuilder.IsRequired(true, fromDataAnnotation: true);
}

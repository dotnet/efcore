// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that configures a property as having a backing field
///     based on the <see cref="BackingFieldAttribute" /> attribute.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public class BackingFieldAttributeConvention :
    PropertyAttributeConventionBase<BackingFieldAttribute>,
    IComplexPropertyAddedConvention,
    IComplexPropertyFieldChangedConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="BackingFieldAttributeConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    public BackingFieldAttributeConvention(ProviderConventionSetBuilderDependencies dependencies)
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
        BackingFieldAttribute attribute,
        MemberInfo clrMember,
        IConventionContext context)
        => propertyBuilder.HasField(attribute.Name, fromDataAnnotation: true);

    /// <summary>
    ///     Called after a complex property is added to a type with an attribute on the associated CLR property or field.
    /// </summary>
    /// <param name="propertyBuilder">The builder for the property.</param>
    /// <param name="attribute">The attribute.</param>
    /// <param name="clrMember">The member that has the attribute.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    protected override void ProcessPropertyAdded(
        IConventionComplexPropertyBuilder propertyBuilder,
        BackingFieldAttribute attribute,
        MemberInfo clrMember,
        IConventionContext context)
        => propertyBuilder.HasField(attribute.Name, fromDataAnnotation: true);
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that configures the maximum length based on the <see cref="StringLengthAttribute" /> applied on the property.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public class StringLengthAttributeConvention : PropertyAttributeConventionBase<StringLengthAttribute>
{
    /// <summary>
    ///     Creates a new instance of <see cref="StringLengthAttributeConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    public StringLengthAttributeConvention(ProviderConventionSetBuilderDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <inheritdoc />
    protected override void ProcessPropertyAdded(
        IConventionPropertyBuilder propertyBuilder,
        StringLengthAttribute attribute,
        MemberInfo clrMember,
        IConventionContext context)
    {
        if (attribute.MaximumLength > 0)
        {
            propertyBuilder.HasMaxLength(attribute.MaximumLength, fromDataAnnotation: true);
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that configures the Unicode based on the <see cref="UnicodeAttribute" /> applied on the property.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public class UnicodeAttributeConvention : PropertyAttributeConventionBase<UnicodeAttribute>
{
    /// <summary>
    ///     Creates a new instance of <see cref="UnicodeAttributeConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    public UnicodeAttributeConvention(ProviderConventionSetBuilderDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <inheritdoc />
    protected override void ProcessPropertyAdded(
        IConventionPropertyBuilder propertyBuilder,
        UnicodeAttribute attribute,
        MemberInfo clrMember,
        IConventionContext context)
        => propertyBuilder.IsUnicode(attribute.IsUnicode, fromDataAnnotation: true);
}

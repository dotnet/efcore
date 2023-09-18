// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that configures the Precision based on the <see cref="PrecisionAttribute" /> applied on the property.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public class PrecisionAttributeConvention : PropertyAttributeConventionBase<PrecisionAttribute>
{
    /// <summary>
    ///     Creates a new instance of <see cref="PrecisionAttributeConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    public PrecisionAttributeConvention(ProviderConventionSetBuilderDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <inheritdoc />
    protected override void ProcessPropertyAdded(
        IConventionPropertyBuilder propertyBuilder,
        PrecisionAttribute attribute,
        MemberInfo clrMember,
        IConventionContext context)
    {
        propertyBuilder.HasPrecision(attribute.Precision, fromDataAnnotation: true);

        if (attribute.Scale.HasValue)
        {
            propertyBuilder.HasScale(attribute.Scale, fromDataAnnotation: true);
        }
    }

    /// <inheritdoc />
    protected override void ProcessPropertyAdded(
        IConventionComplexPropertyBuilder propertyBuilder,
        PrecisionAttribute attribute,
        MemberInfo clrMember,
        IConventionContext context)
    {
        var property = propertyBuilder.Metadata;
        var member = property.GetIdentifyingMemberInfo();
        if (member != null
            && Attribute.IsDefined(member, typeof(ForeignKeyAttribute), inherit: true))
        {
            throw new InvalidOperationException(
                CoreStrings.AttributeNotOnEntityTypeProperty(
                    "Precision", property.DeclaringType.DisplayName(), property.Name));
        }
    }
}

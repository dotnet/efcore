// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that configures the property as a concurrency token if a <see cref="TimestampAttribute" /> is applied to it.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public class TimestampAttributeConvention : PropertyAttributeConventionBase<TimestampAttribute>
{
    /// <summary>
    ///     Creates a new instance of <see cref="TimestampAttributeConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    public TimestampAttributeConvention(ProviderConventionSetBuilderDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <inheritdoc />
    protected override void ProcessPropertyAdded(
        IConventionPropertyBuilder propertyBuilder,
        TimestampAttribute attribute,
        MemberInfo clrMember,
        IConventionContext context)
    {
        propertyBuilder.ValueGenerated(ValueGenerated.OnAddOrUpdate, fromDataAnnotation: true);
        propertyBuilder.IsConcurrencyToken(true, fromDataAnnotation: true);
    }
}

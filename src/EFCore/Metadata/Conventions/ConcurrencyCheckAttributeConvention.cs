// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that configures a property as a concurrency token if it has the <see cref="ConcurrencyCheckAttribute" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public class ConcurrencyCheckAttributeConvention : PropertyAttributeConventionBase<ConcurrencyCheckAttribute>
{
    /// <summary>
    ///     Creates a new instance of <see cref="ConcurrencyCheckAttributeConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    public ConcurrencyCheckAttributeConvention(ProviderConventionSetBuilderDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <inheritdoc />
    protected override void ProcessPropertyAdded(
        IConventionPropertyBuilder propertyBuilder,
        ConcurrencyCheckAttribute attribute,
        MemberInfo clrMember,
        IConventionContext context)
        => propertyBuilder.IsConcurrencyToken(true, fromDataAnnotation: true);
}

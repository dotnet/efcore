// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that configures a property as <see cref="ValueGenerated.OnAdd" /> if
///     <see cref="DatabaseGeneratedOption.Identity" /> is specified, <see cref="ValueGenerated.OnAddOrUpdate" /> if
///     <see cref="DatabaseGeneratedOption.Computed" /> is specified or <see cref="ValueGenerated.Never" /> if
///     <see cref="DatabaseGeneratedOption.None" /> is specified using a <see cref="DatabaseGeneratedAttribute" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public class DatabaseGeneratedAttributeConvention : PropertyAttributeConventionBase<DatabaseGeneratedAttribute>
{
    /// <summary>
    ///     Creates a new instance of <see cref="DatabaseGeneratedAttributeConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    public DatabaseGeneratedAttributeConvention(ProviderConventionSetBuilderDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <inheritdoc />
    protected override void ProcessPropertyAdded(
        IConventionPropertyBuilder propertyBuilder,
        DatabaseGeneratedAttribute attribute,
        MemberInfo clrMember,
        IConventionContext context)
    {
        var valueGenerated =
            attribute.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity
                ? ValueGenerated.OnAdd
                : attribute.DatabaseGeneratedOption == DatabaseGeneratedOption.Computed
                    ? ValueGenerated.OnAddOrUpdate
                    : ValueGenerated.Never;

        propertyBuilder.ValueGenerated(valueGenerated, fromDataAnnotation: true);
    }
}

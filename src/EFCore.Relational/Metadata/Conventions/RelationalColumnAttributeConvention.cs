// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that configures column name and type for a property based on the applied <see cref="ColumnAttribute" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public class RelationalColumnAttributeConvention : PropertyAttributeConventionBase<ColumnAttribute>
{
    /// <summary>
    ///     Creates a new instance of <see cref="RelationalColumnAttributeConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    /// <param name="relationalDependencies"> Parameter object containing relational dependencies for this convention.</param>
    public RelationalColumnAttributeConvention(
        ProviderConventionSetBuilderDependencies dependencies,
        RelationalConventionSetBuilderDependencies relationalDependencies)
        : base(dependencies)
    {
        RelationalDependencies = relationalDependencies;
    }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalConventionSetBuilderDependencies RelationalDependencies { get; }

    /// <inheritdoc />
    protected override void ProcessPropertyAdded(
        IConventionPropertyBuilder propertyBuilder,
        ColumnAttribute attribute,
        MemberInfo clrMember,
        IConventionContext context)
    {
        if (!string.IsNullOrWhiteSpace(attribute.Name))
        {
            propertyBuilder.HasColumnName(attribute.Name, fromDataAnnotation: true);
        }

        if (!string.IsNullOrWhiteSpace(attribute.TypeName))
        {
            propertyBuilder.HasColumnType(attribute.TypeName, fromDataAnnotation: true);
        }

        if (attribute.Order >= 0)
        {
            propertyBuilder.HasColumnOrder(attribute.Order, fromDataAnnotation: true);
        }
    }
}

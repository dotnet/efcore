// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that configures the table name and schema for entity types based on the applied <see cref="TableAttribute" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public class RelationalTableAttributeConvention : TypeAttributeConventionBase<TableAttribute>
{
    /// <summary>
    ///     Creates a new instance of <see cref="RelationalTableAttributeConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    /// <param name="relationalDependencies"> Parameter object containing relational dependencies for this convention.</param>
    public RelationalTableAttributeConvention(
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

    /// <summary>
    ///     Called after an entity type is added to the model if it has an attribute.
    /// </summary>
    /// <param name="entityTypeBuilder">The builder for the entity type.</param>
    /// <param name="attribute">The attribute.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    protected override void ProcessEntityTypeAdded(
        IConventionEntityTypeBuilder entityTypeBuilder,
        TableAttribute attribute,
        IConventionContext<IConventionEntityTypeBuilder> context)
    {
        if (!string.IsNullOrWhiteSpace(attribute.Schema))
        {
            entityTypeBuilder.ToTable(attribute.Name, attribute.Schema, fromDataAnnotation: true);
        }
        else if (!string.IsNullOrWhiteSpace(attribute.Name))
        {
            entityTypeBuilder.ToTable(attribute.Name, fromDataAnnotation: true);
        }
    }
}

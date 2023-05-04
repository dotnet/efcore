// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that configures the entity types that have the <see cref="OwnedAttribute" /> as owned.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public class OwnedEntityTypeAttributeConvention : EntityTypeAttributeConventionBase<OwnedAttribute>
{
    /// <summary>
    ///     Creates a new instance of <see cref="OwnedEntityTypeAttributeConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    public OwnedEntityTypeAttributeConvention(ProviderConventionSetBuilderDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <summary>
    ///     Called after an entity type is added to the model if it has an attribute.
    /// </summary>
    /// <param name="entityTypeBuilder">The builder for the entity type.</param>
    /// <param name="attribute">The attribute.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    protected override void ProcessEntityTypeAdded(
        IConventionEntityTypeBuilder entityTypeBuilder,
        OwnedAttribute attribute,
        IConventionContext<IConventionEntityTypeBuilder> context)
    {
        entityTypeBuilder.ModelBuilder.Owned(entityTypeBuilder.Metadata.ClrType, fromDataAnnotation: true);
        if (!entityTypeBuilder.Metadata.IsInModel)
        {
            context.StopProcessing();
        }
    }
}

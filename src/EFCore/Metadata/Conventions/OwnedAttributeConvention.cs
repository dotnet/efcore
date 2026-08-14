// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that configures the entity types that have the <see cref="OwnedAttribute" /> as owned.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public class OwnedAttributeConvention : TypeAttributeConventionBase<OwnedAttribute>,
    IComplexPropertyAddedConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="OwnedAttributeConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    public OwnedAttributeConvention(ProviderConventionSetBuilderDependencies dependencies)
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

    /// <summary>
    ///     Called after an complex type is added to the model if it has an attribute.
    /// </summary>
    /// <param name="complexTypeBuilder">The builder for the complex type.</param>
    /// <param name="attribute">The attribute.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    protected override void ProcessComplexTypeAdded(
        IConventionComplexTypeBuilder complexTypeBuilder,
        OwnedAttribute attribute,
        IConventionContext context)
    {
        var complexProperty = complexTypeBuilder.Metadata.ComplexProperty;
        var entityTypeBuilder = ReplaceWithEntityType(complexTypeBuilder, shouldBeOwned: true);
        if (entityTypeBuilder == null)
        {
            return;
        }

        context.StopProcessing();

        var memberInfo = complexProperty.GetIdentifyingMemberInfo();
        if (memberInfo != null
            && complexProperty.Builder is IConventionEntityTypeBuilder conventionEntityTypeBuilder)
        {
            conventionEntityTypeBuilder.HasOwnership(
                entityTypeBuilder.Metadata, memberInfo, fromDataAnnotation: true);
        }
    }
}

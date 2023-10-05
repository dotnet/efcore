// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that configures store value generation as <see cref="ValueGenerated.OnAdd" /> on properties that are
///     part of the primary key and not part of any foreign keys or were configured to have a database default value.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> and
///     <see href="https://aka.ms/efcore-docs-value-generation">EF Core value generation</see> for more information and examples.
/// </remarks>
public class CosmosValueGenerationConvention :
    ValueGenerationConvention,
    IEntityTypeAnnotationChangedConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="CosmosValueGenerationConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    public CosmosValueGenerationConvention(
        ProviderConventionSetBuilderDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <summary>
    ///     Called after an annotation is changed on an entity type.
    /// </summary>
    /// <param name="entityTypeBuilder">The builder for the entity type.</param>
    /// <param name="name">The annotation name.</param>
    /// <param name="annotation">The new annotation.</param>
    /// <param name="oldAnnotation">The old annotation.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessEntityTypeAnnotationChanged(
        IConventionEntityTypeBuilder entityTypeBuilder,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation,
        IConventionContext<IConventionAnnotation> context)
    {
        if (name != CosmosAnnotationNames.ContainerName
            || (annotation == null) == (oldAnnotation == null))
        {
            return;
        }

        var primaryKey = entityTypeBuilder.Metadata.FindPrimaryKey();
        if (primaryKey == null)
        {
            return;
        }

        foreach (var property in primaryKey.Properties)
        {
            property.Builder.ValueGenerated(GetValueGenerated(property));
        }
    }

    /// <summary>
    ///     Returns the store value generation strategy to set for the given property.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>The store value generation strategy to set for the given property.</returns>
    protected override ValueGenerated? GetValueGenerated(IConventionProperty property)
    {
        var entityType = property.DeclaringType as IConventionEntityType;
        var propertyType = property.ClrType.UnwrapNullableType();
        if (propertyType == typeof(int)
            && entityType != null)
        {
            var ownership = entityType.FindOwnership();
            if (ownership is { IsUnique: false }
                && !entityType.IsDocumentRoot())
            {
                var pk = property.FindContainingPrimaryKey();
                if (pk != null
                    && !property.IsForeignKey()
                    && pk.Properties.Count == ownership.Properties.Count + 1
                    && property.IsShadowProperty()
                    && ownership.Properties.All(fkProperty => pk.Properties.Contains(fkProperty)))
                {
                    return ValueGenerated.OnAddOrUpdate;
                }
            }
        }

        return propertyType != typeof(Guid)
            ? null
            : base.GetValueGenerated(property);
    }
}

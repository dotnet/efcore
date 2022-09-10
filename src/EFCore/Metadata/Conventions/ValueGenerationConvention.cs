// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that configures store value generation as <see cref="ValueGenerated.OnAdd" /> on properties that are
///     part of the primary key and not part of any foreign keys.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public class ValueGenerationConvention :
    IEntityTypePrimaryKeyChangedConvention,
    IForeignKeyAddedConvention,
    IForeignKeyRemovedConvention,
    IForeignKeyPropertiesChangedConvention,
    IEntityTypeBaseTypeChangedConvention,
    IForeignKeyOwnershipChangedConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="ValueGenerationConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    public ValueGenerationConvention(ProviderConventionSetBuilderDependencies dependencies)
    {
        Dependencies = dependencies;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

    /// <summary>
    ///     Called after a foreign key is added to the entity type.
    /// </summary>
    /// <param name="relationshipBuilder">The builder for the foreign key.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessForeignKeyAdded(
        IConventionForeignKeyBuilder relationshipBuilder,
        IConventionContext<IConventionForeignKeyBuilder> context)
    {
        var foreignKey = relationshipBuilder.Metadata;
        if (!foreignKey.IsBaseLinking())
        {
            foreach (var property in foreignKey.Properties)
            {
                property.Builder.ValueGenerated(GetValueGenerated(property));
            }
        }
    }

    /// <summary>
    ///     Called after a foreign key is removed.
    /// </summary>
    /// <param name="entityTypeBuilder">The builder for the entity type.</param>
    /// <param name="foreignKey">The removed foreign key.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessForeignKeyRemoved(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionForeignKey foreignKey,
        IConventionContext<IConventionForeignKey> context)
    {
        if (entityTypeBuilder.Metadata.IsInModel)
        {
            OnForeignKeyRemoved(foreignKey.Properties);
        }
    }

    /// <summary>
    ///     Called after the foreign key properties or principal key are changed.
    /// </summary>
    /// <param name="relationshipBuilder">The builder for the foreign key.</param>
    /// <param name="oldDependentProperties">The old foreign key properties.</param>
    /// <param name="oldPrincipalKey">The old principal key.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessForeignKeyPropertiesChanged(
        IConventionForeignKeyBuilder relationshipBuilder,
        IReadOnlyList<IConventionProperty> oldDependentProperties,
        IConventionKey oldPrincipalKey,
        IConventionContext<IReadOnlyList<IConventionProperty>> context)
    {
        var foreignKey = relationshipBuilder.Metadata;
        if (!foreignKey.Properties.SequenceEqual(oldDependentProperties))
        {
            OnForeignKeyRemoved(oldDependentProperties);

            if (relationshipBuilder.Metadata.IsInModel
                && !foreignKey.IsBaseLinking())
            {
                foreach (var property in foreignKey.Properties)
                {
                    property.Builder.ValueGenerated(GetValueGenerated(property));
                }
            }
        }
    }

    private void OnForeignKeyRemoved(IReadOnlyList<IConventionProperty> foreignKeyProperties)
    {
        foreach (var property in foreignKeyProperties)
        {
            var pk = property.FindContainingPrimaryKey();
            if (pk == null)
            {
                if (property.IsInModel)
                {
                    property.Builder.ValueGenerated(GetValueGenerated(property));
                }
            }
            else
            {
                foreach (var keyProperty in pk.Properties)
                {
                    keyProperty.Builder.ValueGenerated(GetValueGenerated(property));
                }
            }
        }
    }

    /// <summary>
    ///     Called after the primary key for an entity type is changed.
    /// </summary>
    /// <param name="entityTypeBuilder">The builder for the entity type.</param>
    /// <param name="newPrimaryKey">The new primary key.</param>
    /// <param name="previousPrimaryKey">The old primary key.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessEntityTypePrimaryKeyChanged(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionKey? newPrimaryKey,
        IConventionKey? previousPrimaryKey,
        IConventionContext<IConventionKey> context)
    {
        if (previousPrimaryKey != null)
        {
            foreach (var property in previousPrimaryKey.Properties)
            {
                if (property.IsInModel)
                {
                    property.Builder.ValueGenerated(ValueGenerated.Never);
                }
            }
        }

        if (newPrimaryKey?.IsInModel == true)
        {
            foreach (var property in newPrimaryKey.Properties)
            {
                property.Builder.ValueGenerated(GetValueGenerated(property));
            }
        }
    }

    /// <summary>
    ///     Called after the base type of an entity type changes.
    /// </summary>
    /// <param name="entityTypeBuilder">The builder for the entity type.</param>
    /// <param name="newBaseType">The new base entity type.</param>
    /// <param name="oldBaseType">The old base entity type.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessEntityTypeBaseTypeChanged(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionEntityType? newBaseType,
        IConventionEntityType? oldBaseType,
        IConventionContext<IConventionEntityType> context)
    {
        if (entityTypeBuilder.Metadata.BaseType != newBaseType)
        {
            return;
        }

        foreach (var property in entityTypeBuilder.Metadata.GetProperties())
        {
            property.Builder.ValueGenerated(GetValueGenerated(property));
        }
    }

    /// <summary>
    ///     Returns the store value generation strategy to set for the given property.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>The store value generation strategy to set for the given property.</returns>
    protected virtual ValueGenerated? GetValueGenerated(IConventionProperty property)
        => GetValueGenerated((IReadOnlyProperty)property);

    /// <summary>
    ///     Returns the store value generation strategy to set for the given property.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>The store value generation strategy to set for the given property.</returns>
    public static ValueGenerated? GetValueGenerated(IReadOnlyProperty property)
        => property.GetContainingForeignKeys().All(fk => fk.IsBaseLinking())
            && ShouldHaveGeneratedProperty(property.FindContainingPrimaryKey())
            && CanBeGenerated(property)
                ? ValueGenerated.OnAdd
                : null;

    private static bool ShouldHaveGeneratedProperty(IReadOnlyKey? key)
        => key != null
            && key.DeclaringEntityType.IsOwned() is var onOwnedType
            && (onOwnedType && key.Properties.Count(p => !p.IsForeignKey()) == 1
                || !onOwnedType && key.Properties.Count == 1);

    /// <summary>
    ///     Indicates whether the specified property can have the value generated by the store or by a non-temporary value generator
    ///     when not set.
    /// </summary>
    /// <param name="property">The key property that might be store generated.</param>
    /// <returns>A value indicating whether the specified property should have the value generated by the store.</returns>
    private static bool CanBeGenerated(IReadOnlyProperty property)
    {
        var propertyType = property.ClrType.UnwrapNullableType();
        return (propertyType.IsInteger()
                && propertyType != typeof(byte))
            || propertyType == typeof(Guid);
    }

    /// <summary>
    ///     Called after the ownership value for a foreign key is changed.
    /// </summary>
    /// <param name="relationshipBuilder">The builder for the foreign key.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessForeignKeyOwnershipChanged(
        IConventionForeignKeyBuilder relationshipBuilder,
        IConventionContext<bool?> context)
    {
        foreach (var property in relationshipBuilder.Metadata.DeclaringEntityType.GetProperties())
        {
            property.Builder.ValueGenerated(GetValueGenerated(property));
        }
    }
}

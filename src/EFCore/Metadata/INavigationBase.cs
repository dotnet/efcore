// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a navigation property which can be used to navigate a relationship.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public interface INavigationBase : IReadOnlyNavigationBase, IPropertyBase
{
    /// <summary>
    ///     Gets the entity type that this navigation property belongs to.
    /// </summary>
    new IEntityType DeclaringEntityType
    {
        [DebuggerStepThrough]
        get => (IEntityType)((IReadOnlyNavigationBase)this).DeclaringEntityType;
    }

    /// <summary>
    ///     Gets the entity type that this navigation property will hold an instance(s) of.
    /// </summary>
    new IEntityType TargetEntityType
    {
        [DebuggerStepThrough]
        get => (IEntityType)((IReadOnlyNavigationBase)this).TargetEntityType;
    }

    /// <summary>
    ///     Gets the inverse navigation.
    /// </summary>
    new INavigationBase? Inverse
    {
        [DebuggerStepThrough]
        get => (INavigationBase?)((IReadOnlyNavigationBase)this).Inverse;
    }

    /// <summary>
    ///     Gets the <see cref="IClrCollectionAccessor" /> for this navigation property, if it's a collection
    ///     navigation.
    /// </summary>
    /// <returns>The accessor.</returns>
    IClrCollectionAccessor? GetCollectionAccessor();

    /// <summary>
    ///     Calls <see cref="ILazyLoader.SetLoaded" /> for a <see cref="INavigationBase" /> to mark it as loaded
    ///     when a no-tracking query has eagerly loaded this relationship.
    /// </summary>
    /// <param name="entity">The entity for which the navigation has been loaded.</param>
    void SetIsLoadedWhenNoTracking(object entity)
    {
        Check.NotNull(entity, nameof(entity));

        var serviceProperties = DeclaringEntityType
            .GetDerivedTypesInclusive()
            .Where(t => t.ClrType.IsInstanceOfType(entity))
            .SelectMany(e => e.GetServiceProperties())
            .Where(p => p.ClrType == typeof(ILazyLoader));

        foreach (var serviceProperty in serviceProperties)
        {
            ((ILazyLoader?)serviceProperty.GetGetter().GetClrValueUsingContainingEntity(entity))?.SetLoaded(entity, Name);
        }
    }
}

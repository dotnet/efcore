// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a navigation property which can be used to navigate a relationship.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public interface INavigation : IReadOnlyNavigation, INavigationBase
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
    ///     Gets the foreign key that defines the relationship this navigation property will navigate.
    /// </summary>
    new IForeignKey ForeignKey
    {
        [DebuggerStepThrough]
        get => (IForeignKey)((IReadOnlyNavigation)this).ForeignKey;
    }

    /// <summary>
    ///     Gets the inverse navigation.
    /// </summary>
    new INavigation? Inverse
    {
        [DebuggerStepThrough]
        get => (INavigation?)((IReadOnlyNavigation)this).Inverse;
    }
}

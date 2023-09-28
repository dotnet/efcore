// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a navigation property that is part of a relationship
///     that is forwarded through a third entity type.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public interface ISkipNavigation : IReadOnlySkipNavigation, INavigationBase
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
    ///     Gets the join type used by the foreign key.
    /// </summary>
    new IEntityType JoinEntityType
    {
        [DebuggerStepThrough]
        get => (IEntityType)((IReadOnlySkipNavigation)this).JoinEntityType!;
    }

    /// <summary>
    ///     Gets the foreign key to the join type.
    /// </summary>
    new IForeignKey ForeignKey
    {
        [DebuggerStepThrough]
        get => (IForeignKey)((IReadOnlySkipNavigation)this).ForeignKey!;
    }

    /// <summary>
    ///     Gets the inverse skip navigation.
    /// </summary>
    new ISkipNavigation Inverse
    {
        [DebuggerStepThrough]
        get => (ISkipNavigation)((IReadOnlySkipNavigation)this).Inverse;
    }
}

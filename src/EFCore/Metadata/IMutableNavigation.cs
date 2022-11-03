// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a navigation property which can be used to navigate a relationship.
/// </summary>
/// <remarks>
///     <para>
///         This interface is used during model creation and allows the metadata to be modified.
///         Once the model is built, <see cref="INavigation" /> represents a read-only view of the same metadata.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and
///         examples.
///     </para>
/// </remarks>
public interface IMutableNavigation : IReadOnlyNavigation, IMutableNavigationBase
{
    /// <summary>
    ///     Gets the type that this navigation property belongs to.
    /// </summary>
    new IMutableEntityType DeclaringEntityType
    {
        [DebuggerStepThrough]
        get => (IMutableEntityType)((IReadOnlyNavigationBase)this).DeclaringEntityType;
    }

    /// <summary>
    ///     Gets the entity type that this navigation property will hold an instance(s) of.
    /// </summary>
    new IMutableEntityType TargetEntityType
    {
        [DebuggerStepThrough]
        get => (IMutableEntityType)((IReadOnlyNavigationBase)this).TargetEntityType;
    }

    /// <summary>
    ///     Gets the foreign key that defines the relationship this navigation property will navigate.
    /// </summary>
    new IMutableForeignKey ForeignKey
    {
        [DebuggerStepThrough]
        get => (IMutableForeignKey)((IReadOnlyNavigation)this).ForeignKey;
    }

    /// <summary>
    ///     Gets the inverse navigation.
    /// </summary>
    new IMutableNavigation? Inverse
    {
        [DebuggerStepThrough]
        get => (IMutableNavigation?)((IReadOnlyNavigation)this).Inverse;
    }

    /// <summary>
    ///     Sets the inverse navigation.
    /// </summary>
    /// <param name="inverseName">
    ///     The name of the inverse navigation property. Passing <see langword="null" /> will result in there being
    ///     no inverse navigation property defined.
    /// </param>
    /// <returns>The inverse navigation.</returns>
    IMutableNavigation? SetInverse(string? inverseName);

    /// <summary>
    ///     Sets the inverse navigation.
    /// </summary>
    /// <param name="inverse">
    ///     The inverse navigation property. Passing <see langword="null" /> will result in there being
    ///     no inverse navigation property defined.
    /// </param>
    /// <returns>The inverse navigation.</returns>
    IMutableNavigation? SetInverse(MemberInfo? inverse);
}

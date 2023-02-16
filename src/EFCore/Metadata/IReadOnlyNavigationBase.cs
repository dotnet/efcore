// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a navigation property which can be used to navigate a relationship.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public interface IReadOnlyNavigationBase : IReadOnlyPropertyBase
{
    /// <summary>
    ///     Gets the entity type that this navigation property belongs to.
    /// </summary>
    IReadOnlyEntityType DeclaringEntityType { get; }

    /// <summary>
    ///     Gets the entity type that this navigation property will hold an instance(s) of.
    /// </summary>
    IReadOnlyEntityType TargetEntityType { get; }

    /// <summary>
    ///     Gets the inverse navigation.
    /// </summary>
    IReadOnlyNavigationBase? Inverse { get; }

    /// <summary>
    ///     Gets a value indicating whether the navigation property is a collection property.
    /// </summary>
    bool IsCollection { get; }

    /// <summary>
    ///     Gets a value indicating whether this navigation should be eager loaded by default.
    /// </summary>
    bool IsEagerLoaded
        => (bool?)this[CoreAnnotationNames.EagerLoaded] ?? false;

    /// <summary>
    ///     Determines whether or not this navigation should lazy-load if lazy-loading is enabled and a mechanism for lazy-loading
    ///     has been configured in the model.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-lazy-loading">Lazy loading</see> for more information and examples.
    ///     </para>
    /// </remarks>
    bool LazyLoadingEnabled
        => (bool?)this[CoreAnnotationNames.LazyLoadingEnabled] ?? true;

    /// <inheritdoc />
    bool IReadOnlyPropertyBase.IsShadowProperty()
        => this.GetIdentifyingMemberInfo() == null;
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
/// <remarks>
///     Describes a reference (non-collection) Include on an entity being materialized.
///     The included materializer may itself have includes (ThenInclude), forming a tree.
/// </remarks>
public readonly struct ReferenceIncludeInfo(
    RelationalStructuralTypeMaterializer materializer,
    INavigationBase navigation,
    IClrPropertySetter navigationSetter,
    INavigationBase? inverseNavigation,
    IClrPropertySetter? inverseNavigationSetter,
    IClrCollectionAccessor? inverseNavigationCollectionAccessor,
    bool isKeylessEntityType)
{
    /// <summary>
    ///     The materializer for the included (related) entity type.
    /// </summary>
    public RelationalStructuralTypeMaterializer Materializer { get; } = materializer;

    /// <summary>
    ///     Isolated <see cref="ResultContext" /> for the included materializer's own collection-include protocol.
    ///     Each reference include needs its own context so that its state does not collide with the outer
    ///     materializer's <see cref="ResultContext" /> (which stores a different entity type in <c>Values[0]</c>).
    /// </summary>
    public ResultContext ResultContext { get; } = new();

    /// <summary>
    ///     The navigation from the principal entity to the included entity (e.g. Post.Blog).
    /// </summary>
    public INavigationBase Navigation { get; } = navigation;

    /// <summary>
    ///     The setter for the navigation property on the principal entity.
    /// </summary>
    public IClrPropertySetter NavigationSetter { get; } = navigationSetter;

    /// <summary>
    ///     The inverse navigation (e.g. Blog.Posts), or null if there is none.
    /// </summary>
    public INavigationBase? InverseNavigation { get; } = inverseNavigation;

    /// <summary>
    ///     The setter for the inverse navigation, or null if inverse is a collection or doesn't exist.
    /// </summary>
    public IClrPropertySetter? InverseNavigationSetter { get; } = inverseNavigationSetter;

    /// <summary>
    ///     The collection accessor for the inverse navigation, or null if inverse is a reference or doesn't exist.
    /// </summary>
    public IClrCollectionAccessor? InverseNavigationCollectionAccessor { get; } = inverseNavigationCollectionAccessor;

    /// <summary>
    ///     Whether the navigation's declaring entity type is keyless (has no primary key).
    /// </summary>
    public bool IsKeylessEntityType { get; } = isKeylessEntityType;
}

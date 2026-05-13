// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data.Common;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     Describes a collection Include on an entity being materialized.
/// </summary>
/// <remarks>
///     The included materializer may itself have includes (ThenInclude), forming a tree.
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </remarks>
public readonly struct CollectionIncludeInfo(
    RelationalStructuralTypeMaterializer innerMaterializer,
    INavigationBase navigation,
    INavigationBase? inverseNavigation,
    IClrPropertySetter? inverseNavigationSetter,
    IClrCollectionAccessor? inverseNavigationCollectionAccessor,
    IClrCollectionAccessor collectionAccessor,
    Func<QueryContext, DbDataReader, object[]> parentIdentifier,
    Func<QueryContext, DbDataReader, object[]> outerIdentifier,
    Func<QueryContext, DbDataReader, object[]> selfIdentifier,
    IReadOnlyList<Func<object, object, bool>> parentIdentifierValueComparers,
    IReadOnlyList<Func<object, object, bool>> outerIdentifierValueComparers,
    IReadOnlyList<Func<object, object, bool>> selfIdentifierValueComparers,
    int collectionId,
    bool isKeylessEntityType,
    bool setLoaded,
    Func<object?>? parentEntityProvider = null)
{
    /// <summary>
    ///     The materializer for the included (child) entity type.
    /// </summary>
    public RelationalStructuralTypeMaterializer InnerMaterializer { get; } = innerMaterializer;

    /// <summary>
    ///     The collection navigation from the parent entity (e.g. Blog.Posts).
    /// </summary>
    public INavigationBase Navigation { get; } = navigation;

    /// <summary>
    ///     The inverse navigation (e.g. Post.Blog), or null.
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
    ///     Accessor for getting/creating the collection on the parent entity.
    /// </summary>
    public IClrCollectionAccessor CollectionAccessor { get; } = collectionAccessor;

    /// <summary>
    ///     Extracts parent identifier values from the current DbDataReader row.
    /// </summary>
    public Func<QueryContext, DbDataReader, object[]> ParentIdentifier { get; } = parentIdentifier;

    /// <summary>
    ///     Extracts outer identifier values from the current DbDataReader row.
    /// </summary>
    public Func<QueryContext, DbDataReader, object[]> OuterIdentifier { get; } = outerIdentifier;

    /// <summary>
    ///     Extracts self (child element) identifier values from the current DbDataReader row.
    /// </summary>
    public Func<QueryContext, DbDataReader, object[]> SelfIdentifier { get; } = selfIdentifier;

    /// <summary>
    ///     Value comparers for parent identifier comparison.
    /// </summary>
    public IReadOnlyList<Func<object, object, bool>> ParentIdentifierValueComparers { get; } = parentIdentifierValueComparers;

    /// <summary>
    ///     Value comparers for outer identifier comparison.
    /// </summary>
    public IReadOnlyList<Func<object, object, bool>> OuterIdentifierValueComparers { get; } = outerIdentifierValueComparers;

    /// <summary>
    ///     Value comparers for self identifier comparison.
    /// </summary>
    public IReadOnlyList<Func<object, object, bool>> SelfIdentifierValueComparers { get; } = selfIdentifierValueComparers;

    /// <summary>
    ///     The index into <see cref="SingleQueryResultCoordinator.Collections" />.
    /// </summary>
    public int CollectionId { get; } = collectionId;

    /// <summary>
    ///     Whether the navigation's declaring entity type is keyless.
    /// </summary>
    public bool IsKeylessEntityType { get; } = isKeylessEntityType;

    /// <summary>
    ///     Whether materializing this include should mark the navigation as loaded.
    /// </summary>
    public bool SetLoaded { get; } = setLoaded;

    /// <summary>
    ///     Optional provider for the parent entity of this collection. When non-null, this collection
    ///     was flattened from a reference include and the parent entity is obtained from this delegate
    ///     (which captures the reference include's <see cref="ResultContext" />). When null, the parent
    ///     entity is the outer entity being materialized.
    /// </summary>
    public Func<object?>? ParentEntityProvider { get; } = parentEntityProvider;
}

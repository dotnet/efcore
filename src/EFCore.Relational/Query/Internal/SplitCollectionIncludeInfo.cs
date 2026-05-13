// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data.Common;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     Describes a split-query collection include.
/// </summary>
/// <remarks>
///     Unlike <see cref="CollectionIncludeInfo" /> (used for single-query), this carries its own
///     <see cref="CommandCache" /> for executing a separate SQL query to load the collection's child rows.
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </remarks>
public sealed class SplitCollectionIncludeInfo(
    Func<QueryContext, DbDataReader, ResultContext, SingleQueryResultCoordinator, object?> elementMaterializer,
    INavigationBase? navigation,
    INavigationBase? inverseNavigation,
    IClrPropertySetter? inverseNavigationSetter,
    IClrCollectionAccessor? inverseNavigationCollectionAccessor,
    IClrCollectionAccessor? collectionAccessor,
    Action<object, object?>? collectionAdd,
    Func<QueryContext, DbDataReader, object[]> parentIdentifier,
    Func<QueryContext, DbDataReader, object[]> childIdentifier,
    IReadOnlyList<Func<object, object, bool>> identifierValueComparers,
    int collectionId,
    Expression selectExpression,
    List<SplitCollectionIncludeInfo> childSplitCollections,
    bool setLoaded,
    RelationalStructuralTypeMaterializer? innerMaterializer = null,
    Func<object?>? parentEntityProvider = null)
{
    /// <summary>
    ///     The materializer delegate for elements returned from the split child query.
    ///     For include-based collections this is the structural materializer's <see cref="RelationalStructuralTypeMaterializer.Materialize" />;
    ///     for standalone collection projections this may be a general projection materializer.
    /// </summary>
    public Func<QueryContext, DbDataReader, ResultContext, SingleQueryResultCoordinator, object?> ElementMaterializer { get; }
        = elementMaterializer;

    /// <summary>
    ///     The structural materializer for include-based split collections, or null for standalone
    ///     split collection projections whose element shaper is not structural.
    /// </summary>
    public RelationalStructuralTypeMaterializer? InnerMaterializer { get; } = innerMaterializer;

    /// <summary>
    ///     The collection navigation from the parent entity.
    /// </summary>
    public INavigationBase? Navigation { get; } = navigation;

    /// <summary>
    ///     The inverse navigation, or null.
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
    public IClrCollectionAccessor? CollectionAccessor { get; } = collectionAccessor;

    /// <summary>
    ///     Adds an element to a standalone collection projection, or null for include collections.
    /// </summary>
    public Action<object, object?>? CollectionAdd { get; } = collectionAdd;

    /// <summary>
    ///     Extracts parent identifier values from the main query's DbDataReader.
    /// </summary>
    public Func<QueryContext, DbDataReader, object[]> ParentIdentifier { get; } = parentIdentifier;

    /// <summary>
    ///     Extracts child identifier values from the split query's DbDataReader.
    /// </summary>
    public Func<QueryContext, DbDataReader, object[]> ChildIdentifier { get; } = childIdentifier;

    /// <summary>
    ///     Value comparers for parent/child identifier comparison.
    /// </summary>
    public IReadOnlyList<Func<object, object, bool>> IdentifierValueComparers { get; } = identifierValueComparers;

    /// <summary>
    ///     The index into <see cref="SplitQueryResultCoordinator.Collections" />.
    /// </summary>
    public int CollectionId { get; } = collectionId;

    /// <summary>
    ///     The <see cref="SelectExpression" /> for this split collection's separate SQL query.
    /// </summary>
    public Expression SelectExpression { get; } = selectExpression;

    /// <summary>
    ///     Any nested split collections within the child entity.
    /// </summary>
    public List<SplitCollectionIncludeInfo> ChildSplitCollections { get; } = childSplitCollections;

    /// <summary>
    ///     Whether materializing this include should mark the navigation as loaded.
    /// </summary>
    public bool SetLoaded { get; } = setLoaded;

    /// <summary>
    ///     Pre-built command cache for the split collection's SQL query.
    ///     Set after construction by the factory.
    /// </summary>
    public RelationalCommandCache? CommandCache { get; set; }

    /// <summary>
    ///     Optional provider for the parent entity. When non-null, this collection was
    ///     flattened from a reference include.
    /// </summary>
    public Func<object?>? ParentEntityProvider { get; set; } = parentEntityProvider;
}

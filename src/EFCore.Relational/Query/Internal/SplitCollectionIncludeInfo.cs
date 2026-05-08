// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data.Common;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
/// <remarks>
///     Describes a split-query collection include. Unlike <see cref="CollectionIncludeInfo" /> (used for
///     single-query), this carries its own <see cref="CommandCache" /> for executing a separate SQL query
///     to load the collection's child rows.
/// </remarks>
public sealed class SplitCollectionIncludeInfo(
    RelationalStructuralTypeMaterializer innerMaterializer,
    INavigationBase? navigation,
    INavigationBase? inverseNavigation,
    IClrPropertySetter? inverseNavigationSetter,
    IClrCollectionAccessor? collectionAccessor,
    Func<QueryContext, DbDataReader, object[]> parentIdentifier,
    Func<QueryContext, DbDataReader, object[]> childIdentifier,
    IReadOnlyList<Func<object, object, bool>> identifierValueComparers,
    int collectionId,
    Expression selectExpression,
    List<SplitCollectionIncludeInfo> childSplitCollections,
    Func<object?>? parentEntityProvider = null)
{
    /// <summary>
    ///     The materializer for the included (child) entity type.
    /// </summary>
    public RelationalStructuralTypeMaterializer InnerMaterializer { get; } = innerMaterializer;

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
    ///     Accessor for getting/creating the collection on the parent entity.
    /// </summary>
    public IClrCollectionAccessor? CollectionAccessor { get; } = collectionAccessor;

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

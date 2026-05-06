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
///     Describes a JSON-mapped owned navigation Include on an entity being materialized.
///     The navigation's data is read from a JSON column in the DbDataReader rather than from
///     separate relational columns or a correlated subquery.
/// </remarks>
public readonly struct JsonIncludeInfo(
    INavigationBase navigation,
    INavigationBase? inverseNavigation,
    IClrPropertySetter? inverseNavigationSetter,
    object materializer,
    JsonProjectionInfo projectionInfo,
    Func<DbDataReader, MemoryStream?> jsonStreamReader,
    bool isCollection)
{
    /// <summary>
    ///     The navigation from the parent entity to the included JSON type.
    /// </summary>
    public INavigationBase Navigation { get; } = navigation;

    /// <summary>
    ///     The inverse navigation, or null.
    /// </summary>
    public INavigationBase? InverseNavigation { get; } = inverseNavigation;

    /// <summary>
    ///     The setter for the inverse navigation, or null if inverse is a collection or doesn't exist.
    /// </summary>
    public IClrPropertySetter? InverseNavigationSetter { get; } = inverseNavigationSetter;

    /// <summary>
    ///     The materializer for the JSON structural type.
    /// </summary>
    public object Materializer { get; } = materializer;

    /// <summary>
    ///     The JSON projection info (column index and key access info).
    /// </summary>
    public JsonProjectionInfo ProjectionInfo { get; } = projectionInfo;

    /// <summary>
    ///     A compiled delegate that reads the JSON column from the DbDataReader as a MemoryStream.
    /// </summary>
    public Func<DbDataReader, MemoryStream?> JsonStreamReader { get; } = jsonStreamReader;

    /// <summary>
    ///     Whether the navigation is a collection.
    /// </summary>
    public bool IsCollection { get; } = isCollection;
}

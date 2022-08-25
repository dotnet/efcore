// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     <para>
///         A class representing a component of JSON path used in <see cref="JsonQueryExpression"/> or <see cref="JsonScalarExpression"/>.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public class PathSegment
{
    /// <summary>
    ///     Creates a new instance of the <see cref="PathSegment" /> class.
    /// </summary>
    /// <param name="key">A key which is being accessed in the JSON.</param>
    public PathSegment(string key)
    {
        Key = key;
    }

    /// <summary>
    ///     The key which is being accessed in the JSON.
    /// </summary>
    public virtual string Key { get; }

    /// <inheritdoc />
    public override string ToString() => (Key == "$" ? "" : ".") + Key;

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is PathSegment pathSegment
                && Equals(pathSegment));

    private bool Equals(PathSegment pathSegment)
        => Key == pathSegment.Key;

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(Key);
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     <para>
///         A struct representing a component of JSON path used in <see cref="JsonQueryExpression" /> or <see cref="JsonScalarExpression" />.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public readonly struct PathSegment : IRelationalQuotableExpression
{
    private static ConstructorInfo? _pathSegmentPropertyConstructor, _pathSegmentArrayIndexConstructor;

    /// <summary>
    ///     Creates a new <see cref="PathSegment" /> struct representing JSON property access.
    /// </summary>
    /// <param name="propertyName">A name of JSON property which is being accessed.</param>
    public PathSegment(string propertyName)
    {
        PropertyName = propertyName;
        ArrayIndex = null;
    }

    /// <summary>
    ///     Creates a new <see cref="PathSegment" /> struct representing JSON array element access.
    /// </summary>
    /// <param name="arrayIndex"><see langword="abstract" />An index of an element which is being accessed in the JSON array.</param>
    public PathSegment(SqlExpression arrayIndex)
    {
        ArrayIndex = arrayIndex;
        PropertyName = null;
    }

    /// <summary>
    ///     The name of JSON property which is being accessed.
    /// </summary>
    public string? PropertyName { get; }

    /// <summary>
    ///     The index of an element which is being accessed in the JSON array.
    /// </summary>
    public SqlExpression? ArrayIndex { get; }

    /// <inheritdoc />
    public Expression Quote()
        => this switch
        {
            { PropertyName: string propertyName }
                => Expression.New(
                    _pathSegmentPropertyConstructor ??= typeof(PathSegment).GetConstructor([typeof(string)])!,
                    Expression.Constant(propertyName)),
            { ArrayIndex: SqlExpression arrayIndex }
                => Expression.New(
                    _pathSegmentArrayIndexConstructor ??= typeof(PathSegment).GetConstructor([typeof(SqlExpression)])!,
                    arrayIndex.Quote()),
            _ => throw new UnreachableException()
        };

    /// <inheritdoc />
    public override string ToString()
        => PropertyName
            ?? ArrayIndex switch
            {
                null => "",
                SqlConstantExpression { Value: not null } sqlConstant => $"[{sqlConstant.Value}]",
                SqlParameterExpression sqlParameter => $"[{sqlParameter.Name}]",
                _ => "[(...)]"
            };

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is PathSegment pathSegment && Equals(pathSegment);

    private bool Equals(PathSegment pathSegment)
        => PropertyName == pathSegment.PropertyName
            && ((ArrayIndex == null && pathSegment.ArrayIndex == null)
                || (ArrayIndex != null && ArrayIndex.Equals(pathSegment.ArrayIndex)));

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(PropertyName, ArrayIndex);
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

/// <summary>
///     An expression that represents a fragment that will be inserted verbatim into the query.
/// </summary>
/// <remarks>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </remarks>
public class FragmentExpression(string fragment) : SqlExpression(typeof(string), CosmosTypeMapping.Default)
{
    /// <summary>
    ///     The fragment.
    /// </summary>
    public virtual string Fragment { get; } = fragment;

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => this;

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
        => expressionPrinter.Append(Fragment);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual bool Equals(FragmentExpression other)
        => base.Equals(other)
            && Fragment == other.Fragment;

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => !ReferenceEquals(null, obj)
            && (ReferenceEquals(this, obj)
                || obj.GetType() == GetType()
                && Equals((FragmentExpression)obj));

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), Fragment);
}

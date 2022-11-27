// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

public sealed class TableReferenceExpression : Expression
{
    private SelectExpression _selectExpression;

    public TableReferenceExpression(SelectExpression selectExpression, string alias)
    {
        _selectExpression = selectExpression;
        Alias = alias;
    }

    public TableExpressionBase Table
        => _selectExpression.Tables.Single(
            e => string.Equals((e as JoinExpressionBase)?.Table.Alias ?? e.Alias, Alias, StringComparison.OrdinalIgnoreCase));

    public string Alias { get; internal set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public SelectExpression SelectExpression => _selectExpression;

    public override Type Type
        => typeof(object);

    public override ExpressionType NodeType
        => ExpressionType.Extension;

    public void UpdateTableReference(SelectExpression oldSelect, SelectExpression newSelect)
    {
        if (ReferenceEquals(oldSelect, _selectExpression))
        {
            _selectExpression = newSelect;
        }
    }

    internal void Verify(SelectExpression selectExpression)
    {
        if (!ReferenceEquals(selectExpression, _selectExpression))
        {
            throw new InvalidOperationException("Dangling TableReferenceExpression.");
        }
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
           && (ReferenceEquals(this, obj)
               || obj is TableReferenceExpression tableReferenceExpression
               && Equals(tableReferenceExpression));

    // Since table reference is owned by SelectExpression, the select expression should be the same reference if they are matching.
    // That means we also don't need to compute the hashcode for it.
    // This allows us to break the cycle in computation when traversing this graph.
    private bool Equals(TableReferenceExpression tableReferenceExpression)
        => string.Equals(Alias, tableReferenceExpression.Alias, StringComparison.OrdinalIgnoreCase)
           && ReferenceEquals(_selectExpression, tableReferenceExpression._selectExpression);

    /// <inheritdoc />
    public override int GetHashCode()
        => 0;
}

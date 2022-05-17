// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public sealed class DeleteExpression : Expression, IPrintableExpression
{
    public DeleteExpression(TableExpression table, SelectExpression selectExpression)
    {
        Table = table;
        SelectExpression = selectExpression;
    }

    public TableExpression Table { get; }

    public SelectExpression SelectExpression { get; }

    /// <inheritdoc />
    public override Type Type
        => typeof(object);

    /// <inheritdoc />
    public sealed override ExpressionType NodeType
        => ExpressionType.Extension;

    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var selectExpression = (SelectExpression)visitor.Visit(SelectExpression);

        return Update(selectExpression);
    }

    public DeleteExpression Update(SelectExpression selectExpression)
        => selectExpression != SelectExpression
            ? new DeleteExpression(Table, selectExpression)
            : this;

    /// <inheritdoc />
    public void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.AppendLine($"DELETE FROM {Table.Name} AS {Table.Alias}");
        expressionPrinter.Visit(SelectExpression);

    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is DeleteExpression deleteExpression
                && Equals(deleteExpression));

    private bool Equals(DeleteExpression deleteExpression)
        => Table == deleteExpression.Table
        && SelectExpression == deleteExpression.SelectExpression;

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Table, SelectExpression);
}

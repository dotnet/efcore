// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
///     <para>
///         An expression that represents a DELETE operation in a SQL tree.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally not used in application code.
///     </para>
/// </summary>
public sealed class DeleteExpression : Expression, IPrintableExpression
{
    /// <summary>
    ///     Creates a new instance of the <see cref="DeleteExpression" /> class.
    /// </summary>
    /// <param name="table">A table on which the delete operation is being applied.</param>
    /// <param name="selectExpression">A select expression which is used to determine which rows to delete.</param>
    public DeleteExpression(TableExpression table, SelectExpression selectExpression)
    {
        Table = table;
        SelectExpression = selectExpression;
    }

    /// <summary>
    ///     The table on which the delete operation is being applied.
    /// </summary>
    public TableExpression Table { get; }

    /// <summary>
    ///     The select expression which is used to determine which rows to delete.
    /// </summary>
    public SelectExpression SelectExpression { get; }

    /// <inheritdoc />
    public override Type Type
        => typeof(object);

    /// <inheritdoc />
    public sealed override ExpressionType NodeType
        => ExpressionType.Extension;

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var selectExpression = (SelectExpression)visitor.Visit(SelectExpression);

        return Update(selectExpression);
    }

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    /// <param name="selectExpression">The <see cref="SelectExpression" /> property of the result.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
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

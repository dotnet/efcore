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
public sealed class DeleteExpression : Expression, IRelationalQuotableExpression, IPrintableExpression
{
    private static ConstructorInfo? _quotingConstructor;

    /// <summary>
    ///     Creates a new instance of the <see cref="DeleteExpression" /> class.
    /// </summary>
    /// <param name="table">A table on which the delete operation is being applied.</param>
    /// <param name="selectExpression">A select expression which is used to determine which rows to delete.</param>
    public DeleteExpression(TableExpression table, SelectExpression selectExpression)
        : this(table, selectExpression, new HashSet<string>())
    {
    }

    private DeleteExpression(TableExpression table, SelectExpression selectExpression, ISet<string> tags)
    {
        Table = table;
        SelectExpression = selectExpression;
        Tags = tags;
    }

    /// <summary>
    ///     The list of tags applied to this <see cref="DeleteExpression" />.
    /// </summary>
    public ISet<string> Tags { get; }

    /// <summary>
    ///     The table on which the delete operation is being applied.
    /// </summary>
    public TableExpression Table { get; }

    /// <summary>
    ///     The select expression which is used to determine which rows to delete.
    /// </summary>
    public SelectExpression SelectExpression { get; }

    /// <summary>
    ///     Applies a given set of tags.
    /// </summary>
    /// <param name="tags">A list of tags to apply.</param>
    public DeleteExpression ApplyTags(ISet<string> tags)
        => new(Table, SelectExpression, tags);

    /// <inheritdoc />
    public override Type Type
        => typeof(object);

    /// <inheritdoc />
    public override ExpressionType NodeType
        => ExpressionType.Extension;

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var selectExpression = (SelectExpression)visitor.Visit(SelectExpression);
        var table = (TableExpression)visitor.Visit(Table);
        return Update(table, selectExpression);
    }

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    /// <param name="table">The <see cref="Table" /> property of the result.</param>
    /// <param name="selectExpression">The <see cref="SelectExpression" /> property of the result.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    public DeleteExpression Update(TableExpression table, SelectExpression selectExpression)
        => table == Table && selectExpression == SelectExpression
            ? this
            : new DeleteExpression(table, selectExpression, Tags);

    /// <inheritdoc />
    public Expression Quote()
        => New(
            _quotingConstructor ??= typeof(DeleteExpression).GetConstructor(
            [
                typeof(TableExpression),
                typeof(SelectExpression),
                typeof(ISet<string>)
            ])!,
            Table.Quote(),
            SelectExpression.Quote(),
            RelationalExpressionQuotingUtilities.QuoteTags(Tags));

    /// <inheritdoc />
    public void Print(ExpressionPrinter expressionPrinter)
    {
        foreach (var tag in Tags)
        {
            expressionPrinter.Append($"-- {tag}");
        }

        expressionPrinter.AppendLine();
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
    public override int GetHashCode()
        => HashCode.Combine(Table, SelectExpression);

}

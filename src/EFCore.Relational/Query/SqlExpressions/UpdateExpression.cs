// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
///     <para>
///         An expression that represents an UPDATE operation in a SQL tree.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public sealed class UpdateExpression : Expression, IRelationalQuotableExpression, IPrintableExpression
{
    private static ConstructorInfo? _quotingConstructor;
    private static ConstructorInfo? _columnValueSetterQuotingConstructor;

    /// <summary>
    ///     Creates a new instance of the <see cref="UpdateExpression" /> class.
    /// </summary>
    /// <param name="table">A table on which the update operation is being applied.</param>
    /// <param name="selectExpression">A select expression which is used to determine which rows to update and to get data from additional tables.</param>
    /// <param name="columnValueSetters">
    ///     A list of <see cref="ColumnValueSetter" /> which specifies columns and their corresponding values to
    ///     update.
    /// </param>
    public UpdateExpression(TableExpression table, SelectExpression selectExpression, IReadOnlyList<ColumnValueSetter> columnValueSetters)
        : this(table, selectExpression, columnValueSetters, new HashSet<string>())
    {
    }

    private UpdateExpression(
        TableExpression table,
        SelectExpression selectExpression,
        IReadOnlyList<ColumnValueSetter> columnValueSetters,
        ISet<string> tags)
    {
        Table = table;
        SelectExpression = selectExpression;
        ColumnValueSetters = columnValueSetters;
        Tags = tags;
    }

    /// <summary>
    ///     The list of tags applied to this <see cref="UpdateExpression" />.
    /// </summary>
    public ISet<string> Tags { get; }

    /// <summary>
    ///     The table on which the update operation is being applied.
    /// </summary>
    public TableExpression Table { get; }

    /// <summary>
    ///     The select expression which is used to determine which rows to update and to get data from additional tables.
    /// </summary>
    public SelectExpression SelectExpression { get; }

    /// <summary>
    ///     The list of <see cref="ColumnValueSetter" /> which specifies columns and their corresponding values to update.
    /// </summary>
    public IReadOnlyList<ColumnValueSetter> ColumnValueSetters { get; }

    /// <summary>
    ///     Applies a given set of tags.
    /// </summary>
    /// <param name="tags">A list of tags to apply.</param>
    public UpdateExpression ApplyTags(ISet<string> tags)
        => new(Table, SelectExpression, ColumnValueSetters, tags);

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
        List<ColumnValueSetter>? columnValueSetters = null;
        for (var (i, n) = (0, ColumnValueSetters.Count); i < n; i++)
        {
            var columnValueSetter = ColumnValueSetters[i];
            var newValue = (SqlExpression)visitor.Visit(columnValueSetter.Value);
            if (columnValueSetters != null)
            {
                columnValueSetters.Add(columnValueSetter with { Value = newValue });
            }
            else if (!ReferenceEquals(newValue, columnValueSetter.Value))
            {
                columnValueSetters = new List<ColumnValueSetter>(n);
                for (var j = 0; j < i; j++)
                {
                    columnValueSetters.Add(ColumnValueSetters[j]);
                }

                columnValueSetters.Add(columnValueSetter with { Value = newValue });
            }
        }

        var table = (TableExpression)visitor.Visit(Table);

        return selectExpression == SelectExpression && table == Table && columnValueSetters is null
            ? this
            : new UpdateExpression(table, selectExpression, columnValueSetters ?? ColumnValueSetters);
    }

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    /// <param name="selectExpression">The <see cref="SelectExpression" /> property of the result.</param>
    /// <param name="columnValueSetters">The <see cref="ColumnValueSetters" /> property of the result.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    public UpdateExpression Update(SelectExpression selectExpression, IReadOnlyList<ColumnValueSetter> columnValueSetters)
        => selectExpression != SelectExpression || !ColumnValueSetters.SequenceEqual(columnValueSetters)
            ? new UpdateExpression(Table, selectExpression, columnValueSetters, Tags)
            : this;

    /// <inheritdoc />
    public Expression Quote()
        => New(
            _quotingConstructor ??= typeof(UpdateExpression).GetConstructor(
            [
                typeof(TableExpression), typeof(SelectExpression), typeof(IReadOnlyList<ColumnValueSetter>), typeof(ISet<string>)
            ])!,
            Table.Quote(),
            SelectExpression.Quote(),
            NewArrayInit(
                typeof(ColumnValueSetter),
                ColumnValueSetters
                    .Select(
                        s => New(
                            _columnValueSetterQuotingConstructor ??=
                                typeof(ColumnValueSetter).GetConstructor([typeof(ColumnExpression), typeof(SqlExpression)])!,
                            s.Column.Quote(),
                            s.Value.Quote()))),
            RelationalExpressionQuotingUtilities.QuoteTags(Tags));

    /// <inheritdoc />
    public void Print(ExpressionPrinter expressionPrinter)
    {
        foreach (var tag in Tags)
        {
            expressionPrinter.Append($"-- {tag}");
        }

        expressionPrinter.AppendLine();
        expressionPrinter.AppendLine($"UPDATE {Table.Name} AS {Table.Alias}");
        expressionPrinter.AppendLine("SET ");
        expressionPrinter.Visit(ColumnValueSetters[0].Column);
        expressionPrinter.Append(" = ");
        expressionPrinter.Visit(ColumnValueSetters[0].Value);
        using (expressionPrinter.Indent())
        {
            foreach (var columnValueSetter in ColumnValueSetters.Skip(1))
            {
                expressionPrinter.AppendLine(",");
                expressionPrinter.Visit(columnValueSetter.Column);
                expressionPrinter.Append(" = ");
                expressionPrinter.Visit(columnValueSetter.Value);
            }
        }

        expressionPrinter.AppendLine();
        expressionPrinter.Visit(SelectExpression);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is UpdateExpression updateExpression
                && Equals(updateExpression));

    private bool Equals(UpdateExpression updateExpression)
        => Table == updateExpression.Table
            && SelectExpression == updateExpression.SelectExpression
            && ColumnValueSetters.SequenceEqual(updateExpression.ColumnValueSetters);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Table);
        hash.Add(SelectExpression);
        foreach (var item in ColumnValueSetters)
        {
            hash.Add(item);
        }

        return hash.ToHashCode();
    }
}

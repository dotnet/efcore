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
public sealed class UpdateExpression : Expression, IPrintableExpression
{
    /// <summary>
    ///     Creates a new instance of the <see cref="UpdateExpression" /> class.
    /// </summary>
    /// <param name="table">A table on which the update operation is being applied.</param>
    /// <param name="selectExpression">A select expression which is used to determine which rows to update and to get data from additional tables.</param>
    /// <param name="setColumnValues">A list of <see cref="SetColumnValue"/> which specifies columns and their corresponding values to update.</param>
    public UpdateExpression(TableExpression table, SelectExpression selectExpression, IReadOnlyList<SetColumnValue> setColumnValues)
        : this(table, selectExpression, setColumnValues, new HashSet<string>())
    {
    }

    private UpdateExpression(
        TableExpression table, SelectExpression selectExpression, IReadOnlyList<SetColumnValue> setColumnValues, ISet<string> tags)
    {
        Table = table;
        SelectExpression = selectExpression;
        SetColumnValues = setColumnValues;
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
    ///     The list of <see cref="SetColumnValue"/> which specifies columns and their corresponding values to update.
    /// </summary>
    public IReadOnlyList<SetColumnValue> SetColumnValues { get; }

    /// <summary>
    ///     Applies a given set of tags.
    /// </summary>
    /// <param name="tags">A list of tags to apply.</param>
    public UpdateExpression ApplyTags(ISet<string> tags)
        => new(Table, SelectExpression, SetColumnValues, tags);

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
        List<SetColumnValue>? setColumnValues = null;
        for (var (i, n) = (0, SetColumnValues.Count); i < n; i++)
        {
            var setColumnValue = SetColumnValues[i];
            var newValue = (SqlExpression)visitor.Visit(setColumnValue.Value);
            if (setColumnValues != null)
            {
                setColumnValues.Add(new SetColumnValue(setColumnValue.Column, newValue));
            }
            else if (!ReferenceEquals(newValue, setColumnValue.Value))
            {
                setColumnValues = new(n);
                for (var j = 0; j < i; j++)
                {
                    setColumnValues.Add(SetColumnValues[j]);
                }
                setColumnValues.Add(new SetColumnValue(setColumnValue.Column, newValue));
            }
        }

        return selectExpression != SelectExpression
            || setColumnValues != null
            ? new UpdateExpression(Table, selectExpression, setColumnValues ?? SetColumnValues)
            : this;
    }

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    /// <param name="selectExpression">The <see cref="SelectExpression" /> property of the result.</param>
    /// <param name="setColumnValues">The <see cref="SetColumnValues" /> property of the result.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    public UpdateExpression Update(SelectExpression selectExpression, IReadOnlyList<SetColumnValue> setColumnValues)
        => selectExpression != SelectExpression || !SetColumnValues.SequenceEqual(setColumnValues)
            ? new UpdateExpression(Table, selectExpression, setColumnValues, Tags)
            : this;

    /// <inheritdoc />
    public void Print(ExpressionPrinter expressionPrinter)
    {
        foreach (var tag in Tags)
        {
            expressionPrinter.Append($"-- {tag}");
        }
        expressionPrinter.AppendLine();
        expressionPrinter.AppendLine($"UPDATE {Table.Name} AS {Table.Alias}");
        expressionPrinter.AppendLine("SET");
        using (expressionPrinter.Indent())
        {
            var first = true;
            foreach (var setColumnValue in SetColumnValues)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    expressionPrinter.AppendLine(",");
                }
                expressionPrinter.Visit(setColumnValue.Column);
                expressionPrinter.Append(" = ");
                expressionPrinter.Visit(setColumnValue.Value);
            }
        }
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
        && SetColumnValues.SequenceEqual(updateExpression.SetColumnValues);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Table);
        hash.Add(SelectExpression);
        foreach (var item in SetColumnValues)
        {
            hash.Add(item);
        }

        return hash.ToHashCode();
    }
}


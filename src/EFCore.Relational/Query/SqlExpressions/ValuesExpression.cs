// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
///     <para>
///         An expression that represents a constant table in SQL, sometimes known as a table value constructor.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public class ValuesExpression : TableExpressionBase, IClonableTableExpressionBase
{
    /// <summary>
    ///     The row values for this table.
    /// </summary>
    public virtual IReadOnlyList<RowValueExpression> RowValues { get; }

    /// <summary>
    ///     The names of the columns contained in this table.
    /// </summary>
    public virtual IReadOnlyList<string> ColumnNames { get; }

    /// <summary>
    ///     Creates a new instance of the <see cref="ValuesExpression" /> class.
    /// </summary>
    /// <param name="alias">A string alias for the table source.</param>
    /// <param name="rowValues">The row values for this table.</param>
    /// <param name="columnNames">The names of the columns contained in this table.</param>
    /// <param name="annotations">A collection of annotations associated with this expression.</param>
    public ValuesExpression(
        string? alias,
        IReadOnlyList<RowValueExpression> rowValues,
        IReadOnlyList<string> columnNames,
        IEnumerable<IAnnotation>? annotations = null)
        : base(alias, annotations)
    {
        Check.NotEmpty(rowValues, nameof(rowValues));

#if DEBUG
        if (rowValues.Any(rv => rv.Values.Count != columnNames.Count))
        {
            throw new ArgumentException("All number of all row values doesn't match the number of column names");
        }

        if (rowValues.SelectMany(rv => rv.Values).Any(
                v => v is not SqlConstantExpression and not SqlUnaryExpression
                {
                    Operand: SqlConstantExpression,
                    OperatorType: ExpressionType.Convert
                }))
        {
            // See #30734 for non-constants
            throw new ArgumentException("Only constant expressions are supported in ValuesExpression");
        }
#endif

        RowValues = rowValues;
        ColumnNames = columnNames;
    }

    /// <summary>
    ///     The alias assigned to this table source.
    /// </summary>
    [NotNull]
    public override string? Alias
    {
        get => base.Alias!;
        internal set => base.Alias = value;
    }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        Check.NotNull(visitor, nameof(visitor));

        RowValueExpression[]? newRowValues = null;

        for (var i = 0; i < RowValues.Count; i++)
        {
            var rowValue = RowValues[i];
            var visited = (RowValueExpression)visitor.Visit(rowValue);
            if (visited != rowValue && newRowValues is null)
            {
                newRowValues = new RowValueExpression[RowValues.Count];
                for (var j = 0; j < i; j++)
                {
                    newRowValues[j] = RowValues[j];
                }
            }

            if (newRowValues is not null)
            {
                newRowValues[i] = visited;
            }
        }

        return newRowValues is null ? this : new ValuesExpression(Alias, newRowValues, ColumnNames);
    }

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    public virtual ValuesExpression Update(IReadOnlyList<RowValueExpression> rowValues)
        => rowValues.Count == RowValues.Count && rowValues.Zip(RowValues, (x, y) => (x, y)).All(tup => tup.x == tup.y)
            ? this
            : new ValuesExpression(Alias, rowValues, ColumnNames);

    /// <inheritdoc />
    protected override TableExpressionBase CreateWithAnnotations(IEnumerable<IAnnotation> annotations)
        => new ValuesExpression(Alias, RowValues, ColumnNames, annotations);

    /// <inheritdoc />
    public virtual TableExpressionBase Clone()
        => CreateWithAnnotations(GetAnnotations());

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Append("VALUES (");

        var count = RowValues.Count;
        for (var i = 0; i < count; i++)
        {
            expressionPrinter.Visit(RowValues[i]);

            if (i < count - 1)
            {
                expressionPrinter.Append(", ");
            }
        }

        expressionPrinter.Append(")");
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is ValuesExpression other && Equals(other);

    private bool Equals(ValuesExpression? other)
    {
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (other is null || !base.Equals(other) || other.RowValues.Count != RowValues.Count)
        {
            return false;
        }

        for (var i = 0; i < RowValues.Count; i++)
        {
            if (!other.RowValues[i].Equals(RowValues[i]))
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hashCode = new HashCode();

        foreach (var rowValue in RowValues)
        {
            hashCode.Add(rowValue);
        }

        return hashCode.ToHashCode();
    }
}

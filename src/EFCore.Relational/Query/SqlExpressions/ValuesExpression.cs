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
public class ValuesExpression : TableExpressionBase
{
    private static ConstructorInfo? _quotingConstructor;

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
    public ValuesExpression(
        string? alias,
        IReadOnlyList<RowValueExpression> rowValues,
        IReadOnlyList<string> columnNames)
        : base(alias, annotations: (IReadOnlyDictionary<string, IAnnotation>?)null)
    {
        Check.DebugAssert(
            rowValues.All(rv => rv.Values.Count == columnNames.Count),
            "All row values must have a value count matching the number of column names");

        RowValues = rowValues;
        ColumnNames = columnNames;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public ValuesExpression(
        string? alias,
        IReadOnlyList<RowValueExpression> rowValues,
        IReadOnlyList<string> columnNames,
        IReadOnlyDictionary<string, IAnnotation>? annotations)
        : base(alias, annotations)
    {
        Check.DebugAssert(
            rowValues.All(rv => rv.Values.Count == columnNames.Count),
            "All row values must have a value count matching the number of column names");

        RowValues = rowValues;
        ColumnNames = columnNames;
    }

    /// <summary>
    ///     The alias assigned to this table source.
    /// </summary>
    public override string Alias
        => base.Alias!;

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => visitor.VisitAndConvert(RowValues) is var newRowValues
            && ReferenceEquals(newRowValues, RowValues)
                ? this
                : new ValuesExpression(Alias, newRowValues, ColumnNames);

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    public virtual ValuesExpression Update(IReadOnlyList<RowValueExpression> rowValues)
        => rowValues.Count == RowValues.Count && rowValues.Zip(RowValues, (x, y) => (x, y)).All(tup => tup.x == tup.y)
            ? this
            : new ValuesExpression(Alias, rowValues, ColumnNames);

    /// <inheritdoc />
    protected override ValuesExpression WithAnnotations(IReadOnlyDictionary<string, IAnnotation> annotations)
        => new(Alias, RowValues, ColumnNames, annotations);

    /// <inheritdoc />
    public override ValuesExpression WithAlias(string newAlias)
        => new(newAlias, RowValues, ColumnNames, Annotations);

    /// <inheritdoc />
    public override Expression Quote()
        => New(
            _quotingConstructor ??= typeof(ValuesExpression).GetConstructor(
            [
                typeof(string),
                typeof(IReadOnlyList<RowValueExpression>),
                typeof(IReadOnlyList<string>),
                typeof(IReadOnlyDictionary<string, IAnnotation>)
            ])!,
            Constant(Alias, typeof(string)),
            NewArrayInit(typeof(RowValueExpression), RowValues.Select(rv => rv.Quote())),
            NewArrayInit(typeof(string), ColumnNames.Select(Constant)),
            RelationalExpressionQuotingUtilities.QuoteAnnotations(Annotations));

    /// <inheritdoc />
    public override TableExpressionBase Clone(string? alias, ExpressionVisitor cloningExpressionVisitor)
    {
        var newRowValues = new RowValueExpression[RowValues.Count];

        for (var i = 0; i < newRowValues.Length; i++)
        {
            newRowValues[i] = (RowValueExpression)cloningExpressionVisitor.Visit(RowValues[i]);
        }

        return new ValuesExpression(alias, newRowValues, ColumnNames, Annotations);
    }

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

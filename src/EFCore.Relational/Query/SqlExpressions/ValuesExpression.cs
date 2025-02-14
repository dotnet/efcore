// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
    public virtual IReadOnlyList<RowValueExpression>? RowValues { get; }

    /// <summary>
    ///     A parameter containing the list of values. The parameterized list get expanded to the actual value
    ///     before the query SQL is generated.
    /// </summary>
    public virtual SqlParameterExpression? ValuesParameter { get; }

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
        : this(alias, rowValues: rowValues, valuesParameter: null, columnNames: columnNames)
    {
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="ValuesExpression" /> class.
    /// </summary>
    /// <param name="alias">A string alias for the table source.</param>
    /// <param name="valuesParameter">A parameterized list of values.</param>
    /// <param name="columnNames">The names of the columns contained in this table.</param>
    public ValuesExpression(
        string? alias,
        SqlParameterExpression valuesParameter,
        IReadOnlyList<string> columnNames)
        : this(alias, rowValues: null, valuesParameter: valuesParameter, columnNames: columnNames)
    {
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
        IReadOnlyList<RowValueExpression>? rowValues,
        SqlParameterExpression? valuesParameter,
        IReadOnlyList<string> columnNames,
        IReadOnlyDictionary<string, IAnnotation>? annotations = null)
        : base(alias, annotations)
    {
        if (rowValues is not null)
        {
            Check.DebugAssert(
                rowValues.All(rv => rv.Values.Count == columnNames.Count),
                "All row values must have a value count matching the number of column names");
        }

        if (valuesParameter is not null)
        {
            Check.DebugAssert(
                columnNames.Count is 1 or 2,
                $"Column names do not match usage of {nameof(ValuesParameter)}");
        }

        if (!(rowValues is null ^ valuesParameter is null))
        {
            throw new ArgumentException(
                RelationalStrings.OneOfTwoValuesMustBeSet(nameof(rowValues), nameof(valuesParameter)));
        }

        RowValues = rowValues;
        ValuesParameter = valuesParameter;
        ColumnNames = columnNames;
    }

    /// <summary>
    ///     The alias assigned to this table source.
    /// </summary>
    public override string Alias
        => base.Alias!;

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => this switch
        {
            { RowValues: not null } => Update(visitor.VisitAndConvert(RowValues)),
            { ValuesParameter: not null } => Update((SqlParameterExpression)visitor.Visit(ValuesParameter)),
            _ => throw new UnreachableException()
        };

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    public virtual ValuesExpression Update(IReadOnlyList<RowValueExpression> rowValues)
        => Update(rowValues: rowValues, valuesParameter: null);

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    public virtual ValuesExpression Update(SqlParameterExpression valuesParameter)
        => Update(rowValues: null, valuesParameter: valuesParameter);

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    public virtual ValuesExpression Update(
        IReadOnlyList<RowValueExpression>? rowValues,
        SqlParameterExpression? valuesParameter)
        => ((rowValues is not null
                    && RowValues is not null
                    && rowValues.Count == RowValues.Count
                    && rowValues.Zip(RowValues, (x, y) => (x, y)).All(tup => tup.x == tup.y))
                || (rowValues is null && RowValues is null))
            && valuesParameter == ValuesParameter
                ? this
                : new ValuesExpression(Alias, rowValues, valuesParameter, ColumnNames, Annotations);

    /// <inheritdoc />
    protected override ValuesExpression WithAnnotations(IReadOnlyDictionary<string, IAnnotation> annotations)
        => new(Alias, RowValues, ValuesParameter, ColumnNames, annotations);

    /// <inheritdoc />
    public override ValuesExpression WithAlias(string newAlias)
        => new(newAlias, RowValues, ValuesParameter, ColumnNames, Annotations);

    /// <inheritdoc />
    public override Expression Quote()
        => New(
            _quotingConstructor ??= typeof(ValuesExpression).GetConstructor(
            [
                typeof(string),
                typeof(IReadOnlyList<RowValueExpression>),
                typeof(SqlParameterExpression),
                typeof(IReadOnlyList<string>),
                typeof(IReadOnlyDictionary<string, IAnnotation>)
            ])!,
            Constant(Alias, typeof(string)),
            RowValues is not null
                ? NewArrayInit(typeof(RowValueExpression), RowValues.Select(rv => rv.Quote()))
                : Constant(null, typeof(RowValueExpression)),
            RelationalExpressionQuotingUtilities.QuoteOrNull(ValuesParameter),
            NewArrayInit(typeof(string), ColumnNames.Select(Constant)),
            RelationalExpressionQuotingUtilities.QuoteAnnotations(Annotations));

    /// <inheritdoc />
    public override TableExpressionBase Clone(string? alias, ExpressionVisitor cloningExpressionVisitor)
    {
        switch (this)
        {
            case { RowValues: not null }:
                var newRowValues = new RowValueExpression[RowValues.Count];
                for (var i = 0; i < newRowValues.Length; i++)
                {
                    newRowValues[i] = (RowValueExpression)cloningExpressionVisitor.Visit(RowValues[i]);
                }

                return new ValuesExpression(alias, newRowValues, null, ColumnNames, Annotations);

            case { ValuesParameter: not null }:
                var newValuesParameter = (SqlParameterExpression)cloningExpressionVisitor.Visit(ValuesParameter);
                return new ValuesExpression(alias, null, newValuesParameter, ColumnNames, Annotations);

            default:
                throw new UnreachableException();
        }
    }

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Append("VALUES (");

        switch (this)
        {
            case { RowValues: not null }:
                var count = RowValues.Count;
                for (var i = 0; i < count; i++)
                {
                    expressionPrinter.Visit(RowValues[i]);

                    if (i < count - 1)
                    {
                        expressionPrinter.Append(", ");
                    }
                }

                break;

            case { ValuesParameter: not null }:
                expressionPrinter.Visit(ValuesParameter);
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        expressionPrinter.Append(")");
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is ValuesExpression valuesExpression
                && Equals(valuesExpression));

    private bool Equals(ValuesExpression? valuesExpression)
        => base.Equals(valuesExpression)
            && (ValuesParameter?.Equals(valuesExpression.ValuesParameter) ?? valuesExpression.ValuesParameter == null)
            && (ReferenceEquals(RowValues, valuesExpression.RowValues)
                || (RowValues is not null
                    && valuesExpression.RowValues is not null
                    && RowValues.SequenceEqual(valuesExpression.RowValues)));

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(base.GetHashCode());
        hashCode.Add(ValuesParameter);

        if (RowValues is not null)
        {
            for (var i = 0; i < RowValues.Count; i++)
            {
                hashCode.Add(RowValues[i]);
            }
        }

        return hashCode.ToHashCode();
    }
}

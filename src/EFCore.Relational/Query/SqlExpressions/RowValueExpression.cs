// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
///     <para>
///         An expression that represents a row value in a SQL tree.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public class RowValueExpression : SqlExpression
{
    private static readonly ISet<ExpressionType> _allowedOperators = new HashSet<ExpressionType>
    {
        ExpressionType.LessThan,
        ExpressionType.LessThanOrEqual,
        ExpressionType.GreaterThan,
        ExpressionType.GreaterThanOrEqual,
    };

    /// <summary>
    ///     Creates a new instance of the <see cref="RowValueExpression" /> class.
    /// </summary>
    /// <param name="operatorType">The operator to apply.</param>
    /// <param name="columns">The columns on which the comparison will be performed..</param>
    /// <param name="values">The values to compare with.</param>
    /// <param name="valuesTypeMappings">The <see cref="RelationalTypeMapping" />s associated with the values.</param>
    /// <param name="typeMapping">The <see cref="RelationalTypeMapping" /> associated with the expression.</param>
    public RowValueExpression(
        ExpressionType operatorType,
        IReadOnlyList<SqlExpression> columns,
        IReadOnlyList<object> values,
        IReadOnlyList<RelationalTypeMapping> valuesTypeMappings,
        RelationalTypeMapping? typeMapping)
        : base(typeof(bool), typeMapping)
    {
        if (!IsValidOperator(operatorType))
        {
            throw new InvalidOperationException(
                RelationalStrings.UnsupportedOperatorForSqlExpression(
                    operatorType, typeof(RowValueExpression).ShortDisplayName()));
        }

        if (columns.Count != values.Count)
        {
            throw new InvalidOperationException(RelationalStrings.InvalidRowValueArgumentsCount);
        }
        if (values.Count != valuesTypeMappings.Count)
        {
            throw new InvalidOperationException(RelationalStrings.InvalidRowValueArgumentsCount);
        }

        // TODO: Validate columns.

        OperatorType = operatorType;
        Columns = columns;
        Values = values;
        ValuesTypeMappings = valuesTypeMappings;
    }

    /// <summary>
    ///     The operator of this SQL row value operation.
    /// </summary>
    public virtual ExpressionType OperatorType { get; }

    /// <summary>
    ///     The columns on which the comparison will be performed.
    /// </summary>
    public virtual IReadOnlyList<SqlExpression> Columns { get; }

    /// <summary>
    ///     The values to compare with.
    /// </summary>
    public virtual IReadOnlyList<object> Values { get; }

    /// <summary>
    ///     The values type mappings.
    /// </summary>
    public virtual IReadOnlyList<RelationalTypeMapping> ValuesTypeMappings { get; }

    internal static bool IsValidOperator(ExpressionType operatorType)
        => _allowedOperators.Contains(operatorType);

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => this;

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        var count = Columns.Count;

        expressionPrinter.Append("(");
        for (var i = 0; i < count; i++)
        {
            expressionPrinter.Visit(Columns[i]);

            if (i < count - 1)
            {
                expressionPrinter.Append(", ");
            }
        }
        expressionPrinter.Append(")");

        expressionPrinter.Append(expressionPrinter.GenerateBinaryOperator(OperatorType));

        expressionPrinter.Append("(");
        for (var i = 0; i < count; i++)
        {
            expressionPrinter.Append(Values[i].ToString()!);

            if (i < count - 1)
            {
                expressionPrinter.Append(", ");
            }
        }
        expressionPrinter.Append(")");
    }
}

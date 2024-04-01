// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
///     <para>
///         An expression that represents projecting a scalar SQL value from a subquery.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public class ScalarSubqueryExpression : SqlExpression
{
    private static ConstructorInfo? _quotingConstructor;

    /// <summary>
    ///     Creates a new instance of the <see cref="ScalarSubqueryExpression" /> class.
    /// </summary>
    /// <param name="subquery">A subquery projecting single row with a single scalar projection.</param>
    public ScalarSubqueryExpression(SelectExpression subquery)
        : this(subquery, subquery.Projection[0].Expression.TypeMapping)
    {
        Subquery = subquery;
    }

    private ScalarSubqueryExpression(SelectExpression subquery, RelationalTypeMapping? typeMapping)
        : base(Verify(subquery).Projection[0].Type, typeMapping)
    {
        Subquery = subquery;
    }

    private static SelectExpression Verify(SelectExpression selectExpression)
    {
        Check.DebugAssert(!selectExpression.IsMutable, "Mutable subquery provided to ExistsExpression");

        if (selectExpression.Projection.Count != 1)
        {
            throw new InvalidOperationException(CoreStrings.TranslationFailed(selectExpression.Print()));
        }

        return selectExpression;
    }

    /// <summary>
    ///     The subquery projecting single row with single scalar projection.
    /// </summary>
    public virtual SelectExpression Subquery { get; }

    /// <summary>
    ///     Applies supplied type mapping to this expression.
    /// </summary>
    /// <param name="typeMapping">A relational type mapping to apply.</param>
    /// <returns>A new expression which has supplied type mapping.</returns>
    public virtual SqlExpression ApplyTypeMapping(RelationalTypeMapping? typeMapping)
        => new ScalarSubqueryExpression(Subquery, typeMapping);

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => Update((SelectExpression)visitor.Visit(Subquery));

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    /// <param name="subquery">The <see cref="Subquery" /> property of the result.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    public virtual ScalarSubqueryExpression Update(SelectExpression subquery)
        => subquery != Subquery
            ? new ScalarSubqueryExpression(subquery)
            : this;

    /// <inheritdoc />
    public override Expression Quote()
        => New(
            _quotingConstructor ??= typeof(ScalarSubqueryExpression).GetConstructor([typeof(SelectExpression)])!,
            Subquery.Quote());

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Append("(");
        using (expressionPrinter.Indent())
        {
            expressionPrinter.Visit(Subquery);
        }

        expressionPrinter.Append(")");
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is ScalarSubqueryExpression scalarSubqueryExpression
                && Equals(scalarSubqueryExpression));

    private bool Equals(ScalarSubqueryExpression scalarSubqueryExpression)
        => base.Equals(scalarSubqueryExpression)
            && Subquery.Equals(scalarSubqueryExpression.Subquery);

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), Subquery);
}

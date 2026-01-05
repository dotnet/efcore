// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
///     <para>
///         An expression that represents an AT TIME ZONE operation in a SQL tree.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public class AtTimeZoneExpression : SqlExpression
{
    private static ConstructorInfo? _quotingConstructor;

    /// <summary>
    ///     Creates a new instance of the <see cref="AtTimeZoneExpression" /> class.
    /// </summary>
    /// <param name="operand">The operand on which to perform the time zone conversion.</param>
    /// <param name="timeZone">The time zone to convert to.</param>
    /// <param name="type">The <see cref="Type" /> of the expression.</param>
    /// <param name="typeMapping">The <see cref="RelationalTypeMapping" /> associated with the expression.</param>
    public AtTimeZoneExpression(
        SqlExpression operand,
        SqlExpression timeZone,
        Type type,
        RelationalTypeMapping? typeMapping)
        : base(type, typeMapping)
    {
        Operand = operand;
        TimeZone = timeZone;
    }

    /// <summary>
    ///     The input operand on which to apply the time zone.
    /// </summary>
    public virtual SqlExpression Operand { get; }

    /// <summary>
    ///     The time zone to be applied.
    /// </summary>
    public virtual SqlExpression TimeZone { get; }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var operand = (SqlExpression)visitor.Visit(Operand);
        var timeZone = (SqlExpression)visitor.Visit(TimeZone);

        return Update(operand, timeZone);
    }

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    /// <param name="operand">The <see cref="Operand" /> property of the result.</param>
    /// <param name="timeZone">The <see cref="TimeZone" /> property of the result.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    public virtual AtTimeZoneExpression Update(SqlExpression operand, SqlExpression timeZone)
        => operand != Operand || timeZone != TimeZone
            ? new AtTimeZoneExpression(operand, timeZone, Type, TypeMapping)
            : this;

    /// <inheritdoc />
    public override Expression Quote()
        => New(
            _quotingConstructor ??= typeof(AtTimeZoneExpression).GetConstructor(
                [typeof(SqlExpression), typeof(SqlExpression), typeof(Type), typeof(RelationalTypeMapping)])!,
            Operand.Quote(),
            TimeZone.Quote(),
            Constant(Type),
            RelationalExpressionQuotingUtilities.QuoteTypeMapping(TypeMapping));

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Visit(Operand);

        expressionPrinter.Append(" AT TIME ZONE ");

        expressionPrinter.Visit(TimeZone);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is AtTimeZoneExpression atTimeZoneExpression
                && Equals(atTimeZoneExpression));

    private bool Equals(AtTimeZoneExpression atTimeZoneExpression)
        => base.Equals(atTimeZoneExpression)
            && Operand.Equals(atTimeZoneExpression.Operand)
            && TimeZone.Equals(atTimeZoneExpression.TimeZone);

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), Operand, TimeZone);
}

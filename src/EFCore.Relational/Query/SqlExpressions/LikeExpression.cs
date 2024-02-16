// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
///     <para>
///         An expression that represents a LIKE in a SQL tree.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public class LikeExpression : SqlExpression
{
    private static ConstructorInfo? _quotingConstructor;

    /// <summary>
    ///     Creates a new instance of the <see cref="LikeExpression" /> class.
    /// </summary>
    /// <param name="match">An expression on which LIKE is applied.</param>
    /// <param name="pattern">A pattern to search.</param>
    /// <param name="escapeChar">An optional escape character to use in LIKE.</param>
    /// <param name="typeMapping">The <see cref="RelationalTypeMapping" /> associated with the expression.</param>
    public LikeExpression(
        SqlExpression match,
        SqlExpression pattern,
        SqlExpression? escapeChar,
        RelationalTypeMapping? typeMapping)
        : base(typeof(bool), typeMapping)
    {
        Match = match;
        Pattern = pattern;
        EscapeChar = escapeChar;
    }

    /// <summary>
    ///     The expression on which LIKE is applied.
    /// </summary>
    public virtual SqlExpression Match { get; }

    /// <summary>
    ///     The pattern to search in <see cref="Match" />.
    /// </summary>
    public virtual SqlExpression Pattern { get; }

    /// <summary>
    ///     The escape character to use in LIKE.
    /// </summary>
    public virtual SqlExpression? EscapeChar { get; }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var match = (SqlExpression)visitor.Visit(Match);
        var pattern = (SqlExpression)visitor.Visit(Pattern);
        var escapeChar = (SqlExpression?)visitor.Visit(EscapeChar);

        return Update(match, pattern, escapeChar);
    }

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    /// <param name="match">The <see cref="Match" /> property of the result.</param>
    /// <param name="pattern">The <see cref="Pattern" /> property of the result.</param>
    /// <param name="escapeChar">The <see cref="EscapeChar" /> property of the result.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    public virtual LikeExpression Update(
        SqlExpression match,
        SqlExpression pattern,
        SqlExpression? escapeChar)
        => match != Match || pattern != Pattern || escapeChar != EscapeChar
            ? new LikeExpression(match, pattern, escapeChar, TypeMapping)
            : this;

    /// <inheritdoc />
    public override Expression Quote()
        => New(
            _quotingConstructor ??= typeof(LikeExpression).GetConstructor(
                [typeof(SqlExpression), typeof(SqlExpression), typeof(SqlExpression), typeof(RelationalTypeMapping)])!,
            Match.Quote(),
            Pattern.Quote(),
            RelationalExpressionQuotingUtilities.VisitOrNull(EscapeChar),
            RelationalExpressionQuotingUtilities.QuoteTypeMapping(TypeMapping));

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Visit(Match);
        expressionPrinter.Append(" LIKE ");
        expressionPrinter.Visit(Pattern);

        if (EscapeChar != null)
        {
            expressionPrinter.Append(" ESCAPE ");
            expressionPrinter.Visit(EscapeChar);
        }
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is LikeExpression likeExpression
                && Equals(likeExpression));

    private bool Equals(LikeExpression likeExpression)
        => base.Equals(likeExpression)
            && Match.Equals(likeExpression.Match)
            && Pattern.Equals(likeExpression.Pattern)
            && (EscapeChar == null
                ? likeExpression.EscapeChar == null
                : EscapeChar.Equals(likeExpression.EscapeChar));

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), Match, Pattern, EscapeChar);
}

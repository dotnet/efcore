// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
///     <para>
///         An expression that represents a SQL token.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public class SqlFragmentExpression : SqlExpression
{
    private static ConstructorInfo? _quotingConstructor;

    /// <summary>
    ///     Creates a new instance of the <see cref="SqlFragmentExpression" /> class.
    /// </summary>
    /// <param name="sql">A string token to print in SQL tree.</param>
    public SqlFragmentExpression(string sql)
        : base(typeof(string), null)
    {
        Sql = sql;
    }

    /// <summary>
    ///     The string token to print in SQL tree.
    /// </summary>
    public virtual string Sql { get; }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => this;

    /// <inheritdoc />
    public override Expression Quote()
        => New(
            _quotingConstructor ??= typeof(SqlFragmentExpression).GetConstructor([typeof(string)])!,
            Constant(Sql)); // TODO: The new type mapping once that's merged

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
        => expressionPrinter.Append(Sql);

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is SqlFragmentExpression sqlFragmentExpression
                && Equals(sqlFragmentExpression));

    private bool Equals(SqlFragmentExpression sqlFragmentExpression)
        => base.Equals(sqlFragmentExpression)
            && Sql == sqlFragmentExpression.Sql
            && Sql != "*"; // We make star projection different because it could be coming from different table.

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), Sql);
}

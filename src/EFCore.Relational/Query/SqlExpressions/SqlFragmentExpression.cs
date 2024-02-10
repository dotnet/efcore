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
    /// <summary>
    ///     Creates a new instance of the <see cref="SqlFragmentExpression" /> class.
    /// </summary>
    /// <param name="sql">A string token to print in SQL tree.</param>
    public SqlFragmentExpression(string sql)
        : this(sql, typeof(string), typeMapping: null)
    {
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="SqlFragmentExpression" /> class.
    /// </summary>
    /// <param name="sql">A string token to print in SQL tree.</param>
    /// <param name="type">The <see cref="Type" /> of the expression.</param>
    /// <param name="typeMapping">The <see cref="RelationalTypeMapping" /> associated with the expression.</param>
    public SqlFragmentExpression(string sql, Type type, RelationalTypeMapping? typeMapping = null)
        : base(type, typeMapping)
    {
        Sql = sql;
    }

    /// <summary>
    ///     The string token to print in SQL tree.
    /// </summary>
    public virtual string Sql { get; }

    /// <summary>
    ///     Applies supplied type mapping to this expression.
    /// </summary>
    /// <param name="typeMapping">A relational type mapping to apply.</param>
    /// <returns>A new expression which has supplied type mapping.</returns>
    public virtual SqlFragmentExpression ApplyTypeMapping(RelationalTypeMapping? typeMapping)
        => new(Sql, Type, typeMapping);

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => this;

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

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
///     <para>
///         An expression that represents a subquery table source with user-provided custom SQL.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public class FromSqlExpression : TableExpressionBase, IClonableTableExpressionBase
{
    /// <summary>
    ///     Creates a new instance of the <see cref="FromSqlExpression" /> class.
    /// </summary>
    /// <param name="defaultTableBase">A default table base associated with this table source.</param>
    /// <param name="sql">A user-provided custom SQL for the table source.</param>
    /// <param name="arguments">A user-provided parameters to pass to the custom SQL.</param>
    public FromSqlExpression(ITableBase defaultTableBase, string sql, Expression arguments)
        : this(defaultTableBase.Name[..1].ToLowerInvariant(), defaultTableBase, sql, arguments, annotations: null)
    {
    }

    // See issue#21660/21627
    ///// <summary>
    /////     Creates a new instance of the <see cref="FromSqlExpression" /> class.
    ///// </summary>
    ///// <param name="sqlQuery">A sql query associated with this table source.</param>
    //public FromSqlExpression(ISqlQuery sqlQuery)
    //    : this(sqlQuery, sqlQuery.Sql, Constant(Array.Empty<object>(), typeof(object[])))
    //{
    //}

    /// <summary>
    ///     Creates a new instance of the <see cref="FromSqlExpression" /> class.
    /// </summary>
    /// <param name="alias">An alias to use for this table source.</param>
    /// <param name="sql">A user-provided custom SQL for the table source.</param>
    /// <param name="arguments">A user-provided parameters to pass to the custom SQL.</param>
    public FromSqlExpression(string alias, string sql, Expression arguments)
        : this(alias, null, sql, arguments, annotations: null)
    {
    }

    private FromSqlExpression(
        string alias,
        ITableBase? tableBase,
        string sql,
        Expression arguments,
        IEnumerable<IAnnotation>? annotations)
        : base(alias, annotations)
    {
        Table = tableBase;
        Sql = sql;
        Arguments = arguments;
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

    /// <summary>
    ///     The user-provided custom SQL for the table source.
    /// </summary>
    public virtual string Sql { get; }

    /// <summary>
    ///     The user-provided parameters passed to the custom SQL.
    /// </summary>
    public virtual Expression Arguments { get; }

    /// <summary>
    ///     The <see cref="ITableBase"/> associated with given table source if any, <see langword="null" /> otherwise.
    /// </summary>
    public virtual ITableBase? Table { get; }

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    /// <param name="arguments">The <see cref="Arguments" /> property of the result.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    public virtual FromSqlExpression Update(Expression arguments)
        => arguments != Arguments
            ? new FromSqlExpression(Alias, Table, Sql, arguments, GetAnnotations())
            : this;

    /// <inheritdoc />
    protected override TableExpressionBase CreateWithAnnotations(IEnumerable<IAnnotation> annotations)
        => new FromSqlExpression(Alias, Table, Sql, Arguments, annotations);

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => this;

    /// <inheritdoc />
    public virtual TableExpressionBase Clone()
        => new FromSqlExpression(Alias, Table, Sql, Arguments, GetAnnotations());

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Append(Sql);
        PrintAnnotations(expressionPrinter);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is FromSqlExpression fromSqlExpression
                && Equals(fromSqlExpression));

    private bool Equals(FromSqlExpression fromSqlExpression)
        => base.Equals(fromSqlExpression)
            && Table == fromSqlExpression.Table
            && Sql == fromSqlExpression.Sql
            && ExpressionEqualityComparer.Instance.Equals(Arguments, fromSqlExpression.Arguments);

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), Table, Sql, Arguments);
}

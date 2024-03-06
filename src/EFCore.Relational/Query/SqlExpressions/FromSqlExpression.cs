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
public class FromSqlExpression : TableExpressionBase, ITableBasedExpression
{
    private static ConstructorInfo? _quotingConstructor;
    private static MethodInfo? _constantExpressionFactoryMethod, _parameterExpressionFactoryMethod;

    /// <summary>
    ///     Creates a new instance of the <see cref="FromSqlExpression" /> class.
    /// </summary>
    /// <param name="alias">An alias to use for this table source.</param>
    /// <param name="defaultTableBase">A default table base associated with this table source.</param>
    /// <param name="sql">A user-provided custom SQL for the table source.</param>
    /// <param name="arguments">A user-provided parameters to pass to the custom SQL.</param>
    public FromSqlExpression(string alias, ITableBase defaultTableBase, string sql, Expression arguments)
        : this(alias, defaultTableBase, sql, arguments, annotations: null)
    {
    }

    // See issue#21660/21627
    ///// <summary>
    /////     Creates a new instance of the <see cref="FromSqlExpression" /> class.
    ///// </summary>
    ///// <param name="sqlQuery">A sql query associated with this table source.</param>
    //public FromSqlExpression(ISqlQuery sqlQuery)
    //    : this(sqlQuery, sqlQuery.Sql, Constant([], typeof(object[])))
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

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public FromSqlExpression(
        string alias,
        ITableBase? tableBase,
        string sql,
        Expression arguments,
        IReadOnlyDictionary<string, IAnnotation>? annotations)
        : base(alias, annotations)
    {
        Table = tableBase;
        Sql = sql;
        Arguments = arguments;
    }

    /// <summary>
    ///     The alias assigned to this table source.
    /// </summary>
    public override string Alias
        => base.Alias!;

    /// <summary>
    ///     The user-provided custom SQL for the table source.
    /// </summary>
    public virtual string Sql { get; }

    /// <summary>
    ///     The user-provided parameters passed to the custom SQL.
    /// </summary>
    public virtual Expression Arguments { get; }

    /// <summary>
    ///     The <see cref="ITableBase" /> associated with given table source if any, <see langword="null" /> otherwise.
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
            ? new FromSqlExpression(Alias, Table, Sql, arguments, Annotations)
            : this;

    /// <inheritdoc />
    protected override FromSqlExpression WithAnnotations(IReadOnlyDictionary<string, IAnnotation> annotations)
        => new(Alias, Table, Sql, Arguments, annotations);

    /// <inheritdoc />
    public override FromSqlExpression WithAlias(string newAlias)
        => new(newAlias, Table, Sql, Arguments, Annotations);

    /// <inheritdoc />
    public override Expression Quote()
    {
        _constantExpressionFactoryMethod ??= typeof(Expression).GetMethod(nameof(Constant), [typeof(object)])!;

        return New(
            _quotingConstructor ??= typeof(FromSqlExpression).GetConstructor(
            [
                typeof(string), typeof(ITableBase), typeof(string), typeof(Expression), typeof(IReadOnlyDictionary<string, IAnnotation>)
            ])!,
            Constant(Alias, typeof(string)),
            Table is null ? Constant(null, typeof(ITableBase)) : RelationalExpressionQuotingUtilities.QuoteTableBase(Table),
            Constant(Sql),
            Arguments switch
            {
                ConstantExpression { Value: object[] arguments }
                    => NewArrayInit(
                        typeof(object),
                        arguments.Select(a => (Expression)Call(_constantExpressionFactoryMethod, Constant(a))).ToArray()),

                ParameterExpression parameter
                    when parameter.Type == typeof(object[])
                    => Call(
                        _parameterExpressionFactoryMethod ??= typeof(Expression).GetMethod(nameof(Parameter), [typeof(Type), typeof(string)])!,
                        Constant(typeof(object[])),
                        Constant(parameter.Name, typeof(string))),

                _ => throw new UnreachableException() // TODO: Confirm
            },
            RelationalExpressionQuotingUtilities.QuoteAnnotations(Annotations));
    }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => this;

    /// <inheritdoc />
    public override TableExpressionBase Clone(string? alias, ExpressionVisitor cloningVisitor)
        => new FromSqlExpression(alias!, Table, Sql, Arguments, Annotations);

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

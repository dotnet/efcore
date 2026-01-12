// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using static Microsoft.EntityFrameworkCore.SqlServerDbFunctionsExtensions;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

/// <summary>
///     An expression that represents a SQL Server full-text table-valued function (e.g., CONTAINSTABLE).
/// </summary>
public class FullTextTableExpression : TableExpressionBase
{
   /// <summary>
    ///     Creates a new instance of the <see cref="FullTextTableExpression" /> class.
    /// </summary>
    /// <param name="functionName">The name of the full-text function.</param>
    /// <param name="tableFragment">The SQL fragment representing the raw table name.</param>
    /// <param name="columnFragment">The SQL fragment representing the raw column name.</param>
    /// <param name="searchCondition">The search condition expression.</param>
    /// <param name="alias">The table alias.</param>
    public FullTextTableExpression(
        string functionName,
        SqlFragmentExpression tableFragment,
        SqlFragmentExpression columnFragment,
        SqlExpression searchCondition,
        string alias)
        : base(alias)
    {
        FunctionName = functionName;
        TableFragment = tableFragment;
        ColumnFragment = columnFragment;
        SearchCondition = searchCondition;
    }

    /// <summary>
    ///     The name of the function (e.g. CONTAINSTABLE).
    /// </summary>
    public virtual string FunctionName { get; }

    /// <summary>
    ///     The SQL fragment representing the raw table name.
    /// </summary>
    public virtual SqlFragmentExpression TableFragment { get; }

    /// <summary>
    ///     The SQL fragment representing the raw column name.
    /// </summary>
    public virtual SqlFragmentExpression ColumnFragment { get; }

    /// <summary>
    ///     The search condition.
    /// </summary>
    public virtual SqlExpression SearchCondition { get; }

    /// <inheritdoc />
    protected override TableExpressionBase WithAnnotations(IReadOnlyDictionary<string, IAnnotation> annotations)
        => new FullTextTableExpression(FunctionName, TableFragment, ColumnFragment, SearchCondition, Alias!);

    /// <inheritdoc />
    public override TableExpressionBase WithAlias(string newAlias)
        => new FullTextTableExpression(FunctionName, TableFragment, ColumnFragment, SearchCondition, newAlias);

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Append(FunctionName).Append("(");
        expressionPrinter.Visit(TableFragment);
        expressionPrinter.Append(", ");
        expressionPrinter.Visit(ColumnFragment);
        expressionPrinter.Append(", ");
        expressionPrinter.Visit(SearchCondition);
        expressionPrinter.Append(") AS ").Append(Alias!);
    }

    /// <inheritdoc />
    public override TableExpressionBase Quote()
        => this;

    /// <inheritdoc />
    public override TableExpressionBase Clone(string? alias, ExpressionVisitor visitor)
    {
        return new FullTextTableExpression(
            FunctionName,
            (SqlFragmentExpression)visitor.Visit(TableFragment),
            (SqlFragmentExpression)visitor.Visit(ColumnFragment),
            (SqlExpression)visitor.Visit(SearchCondition),
            (alias ?? Alias)!);
    }
}

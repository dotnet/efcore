// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
///     <para>
///         A reference to a column of either the existing target row or the incoming "excluded" row within the update clause of a
///         <see cref="MergeExpression" />. Unlike <see cref="ColumnExpression" />, it is not bound to a table alias, since the target and
///         excluded rows are implicit in the merge statement.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally not used in application code.
///     </para>
/// </summary>
public sealed class MergeColumnReferenceExpression : SqlExpression
{
    private static ConstructorInfo? _quotingConstructor;

    /// <summary>
    ///     Creates a new instance of the <see cref="MergeColumnReferenceExpression" /> class.
    /// </summary>
    /// <param name="columnName">The name of the referenced column.</param>
    /// <param name="fromExcluded">Whether the reference is to the incoming "excluded" row rather than the existing target row.</param>
    /// <param name="type">The <see cref="System.Type" /> of the expression.</param>
    /// <param name="typeMapping">The <see cref="RelationalTypeMapping" /> associated with the expression.</param>
    public MergeColumnReferenceExpression(string columnName, bool fromExcluded, Type type, RelationalTypeMapping? typeMapping)
        : base(type, typeMapping)
    {
        ColumnName = columnName;
        FromExcluded = fromExcluded;
    }

    /// <summary>
    ///     The name of the referenced column.
    /// </summary>
    public string ColumnName { get; }

    /// <summary>
    ///     Whether the reference is to the incoming "excluded" row rather than the existing target row.
    /// </summary>
    public bool FromExcluded { get; }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => this;

    /// <inheritdoc />
    public override Expression Quote()
        => New(
            _quotingConstructor ??= typeof(MergeColumnReferenceExpression).GetConstructor(
                [typeof(string), typeof(bool), typeof(Type), typeof(RelationalTypeMapping)])!,
            Constant(ColumnName),
            Constant(FromExcluded),
            Constant(Type),
            RelationalExpressionQuotingUtilities.QuoteTypeMapping(TypeMapping));

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
        => expressionPrinter.Append(FromExcluded ? $"excluded.{ColumnName}" : ColumnName);

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is MergeColumnReferenceExpression other
            && ColumnName == other.ColumnName
            && FromExcluded == other.FromExcluded
            && Type == other.Type;

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(ColumnName, FromExcluded, Type);
}

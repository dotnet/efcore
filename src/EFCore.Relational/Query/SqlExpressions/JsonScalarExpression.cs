// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
///     <para>
///         An expression representing a scalar extracted from a JSON column with the given path in SQL tree.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public class JsonScalarExpression : SqlExpression
{
    /// <summary>
    ///     Creates a new instance of the <see cref="JsonScalarExpression" /> class.
    /// </summary>
    /// <param name="jsonColumn">A column containg JSON value.</param>
    /// <param name="property">A property representing the result of this expression.</param>
    /// <param name="path">A list of path segments leading to the scalar from the root of the JSON stored in the column.</param>
    /// <param name="nullable">A value indicating whether the expression is nullable.</param>
    public JsonScalarExpression(
        ColumnExpression jsonColumn,
        IProperty property,
        IReadOnlyList<PathSegment> path,
        bool nullable)
        : this(jsonColumn, path, property.ClrType.UnwrapNullableType(), property.FindRelationalTypeMapping()!, nullable)
    {
    }

    internal JsonScalarExpression(
        ColumnExpression jsonColumn,
        IReadOnlyList<PathSegment> path,
        Type type,
        RelationalTypeMapping typeMapping,
        bool nullable)
        : base(type, typeMapping)
    {
        JsonColumn = jsonColumn;
        Path = path;
        IsNullable = nullable;
    }

    /// <summary>
    ///     The column containing the JSON value.
    /// </summary>
    public virtual ColumnExpression JsonColumn { get; }

    /// <summary>
    ///     The list of path segments leading to the scalar from the root of the JSON stored in the column.
    /// </summary>
    public virtual IReadOnlyList<PathSegment> Path { get; }

    /// <summary>
    ///     The value indicating whether the expression is nullable.
    /// </summary>
    public virtual bool IsNullable { get; }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var jsonColumn = (ColumnExpression)visitor.Visit(JsonColumn);
        var jsonColumnMadeNullable = jsonColumn.IsNullable && !JsonColumn.IsNullable;

        // TODO Call update: Issue#28887
        return jsonColumn != JsonColumn
            ? new JsonScalarExpression(
                jsonColumn,
                Path,
                Type,
                TypeMapping!,
                IsNullable || jsonColumnMadeNullable)
            : this;
    }

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    /// <param name="jsonColumn">The <see cref="JsonColumn" /> property of the result.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    public virtual JsonScalarExpression Update(ColumnExpression jsonColumn)
        => jsonColumn != JsonColumn
            ? new JsonScalarExpression(jsonColumn, Path, Type, TypeMapping!, IsNullable)
            : this;

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Append("JsonScalarExpression(column: ");
        expressionPrinter.Visit(JsonColumn);
        expressionPrinter.Append($", {string.Join("", Path.Select(e => e.ToString()))})");
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is JsonScalarExpression jsonScalarExpression
            && JsonColumn.Equals(jsonScalarExpression.JsonColumn)
            && Path.SequenceEqual(jsonScalarExpression.Path);

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), JsonColumn, Path);
}

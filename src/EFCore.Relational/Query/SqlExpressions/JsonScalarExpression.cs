// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    /// <summary>
    ///     Expression representing a scalar extracted from a JSON column with the given path.
    /// </summary>
    public class JsonScalarExpression : SqlExpression
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="JsonScalarExpression" /> class.
        /// </summary>
        /// <param name="jsonColumn">A column containg JSON.</param>
        /// <param name="property">A property representing the result of this expression.</param>
        /// <param name="jsonPath">A JSON path leading to the scalar from the root of the JSON stored in the column.</param>
        /// <param name="nullable">A value indicating whether the expression is nullable.</param>
        public JsonScalarExpression(
            ColumnExpression jsonColumn,
            IProperty property,
            SqlExpression jsonPath,
            bool nullable)
            : this(jsonColumn, property.ClrType, property.FindRelationalTypeMapping()!, jsonPath, nullable)
        {
        }

        internal JsonScalarExpression(
            ColumnExpression jsonColumn,
            Type type,
            RelationalTypeMapping typeMapping,
            SqlExpression jsonPath,
            bool nullable)
            : base(type, typeMapping)
        {
            JsonColumn = jsonColumn;
            JsonPath = jsonPath;
            IsNullable = nullable;
        }

        /// <summary>
        ///     The column containg JSON.
        /// </summary>
        public virtual ColumnExpression JsonColumn { get; }

        /// <summary>
        ///     The JSON path leading to the scalar from the root of the JSON stored in the column.
        /// </summary>
        public virtual SqlExpression JsonPath { get; }

        /// <summary>
        ///     The value indicating whether the expression is nullable.
        /// </summary>
        public virtual bool IsNullable { get; }

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var jsonColumn = (ColumnExpression)visitor.Visit(JsonColumn);
            var jsonColumnMadeNullable = jsonColumn.IsNullable && !JsonColumn.IsNullable;

            return jsonColumn != JsonColumn
                ? new JsonScalarExpression(
                    jsonColumn,
                    Type,
                    TypeMapping!,
                    JsonPath,
                    IsNullable || jsonColumnMadeNullable)
                : this;
        }

        /// <summary>
        ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
        ///     return this expression.
        /// </summary>
        /// <param name="jsonColumn">The <see cref="JsonColumn" /> property of the result.</param>
        /// <param name="jsonPath">The <see cref="JsonPath" /> property of the result.</param>
        /// <param name="nullable">The <see cref="IsNullable" /> property of the result.</param>
        /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
        public virtual JsonScalarExpression Update(
            ColumnExpression jsonColumn,
            SqlExpression jsonPath,
            bool nullable)
            => jsonColumn != JsonColumn
            || jsonPath != JsonPath
            || nullable != IsNullable
                ? new JsonScalarExpression(jsonColumn, Type, TypeMapping!, jsonPath, nullable)
                : this;

        /// <inheritdoc />
        protected override void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.Append("JsonScalarExpression(column: ");
            expressionPrinter.Visit(JsonColumn);
            expressionPrinter.Append("  Path: ");
            expressionPrinter.Visit(JsonPath);
            expressionPrinter.Append(")");
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
            => obj is JsonScalarExpression jsonScalarExpression
                && JsonColumn.Equals(jsonScalarExpression.JsonColumn)
                && JsonPath.Equals(jsonScalarExpression.JsonPath);

        /// <inheritdoc />
        public override int GetHashCode()
            => HashCode.Combine(base.GetHashCode(), JsonColumn, JsonPath);
    }
}

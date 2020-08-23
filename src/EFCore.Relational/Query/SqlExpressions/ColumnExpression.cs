// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    /// <summary>
    ///     <para>
    ///         An expression that represents a column in a SQL tree.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    ///     <para>
    ///         This class is not publicly constructable. If this is a problem for your application or provider, then please file
    ///         an issue at https://github.com/dotnet/efcore.
    ///     </para>
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay(),nq}")]
    // Class is sealed because there are no public/protected constructors. Can be unsealed if this is changed.
    public sealed class ColumnExpression : SqlExpression
    {
        internal ColumnExpression(IProperty property, IColumnBase column, TableExpressionBase table, bool nullable)
            : this(
                column.Name,
                table,
                property.ClrType.UnwrapNullableType(),
                column.PropertyMappings.First(m => m.Property == property).TypeMapping,
                nullable || column.IsNullable)
        {
        }

        internal ColumnExpression(ProjectionExpression subqueryProjection, TableExpressionBase table)
            : this(
                subqueryProjection.Alias, table,
                subqueryProjection.Type, subqueryProjection.Expression.TypeMapping,
                IsNullableProjection(subqueryProjection))
        {
        }

        private static bool IsNullableProjection(ProjectionExpression projectionExpression)
            => projectionExpression.Expression switch
            {
                ColumnExpression columnExpression => columnExpression.IsNullable,
                SqlConstantExpression sqlConstantExpression => sqlConstantExpression.Value == null,
                _ => true,
            };

        private ColumnExpression(string name, TableExpressionBase table, Type type, RelationalTypeMapping typeMapping, bool nullable)
            : base(type, typeMapping)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(table, nameof(table));
            Check.NotEmpty(table.Alias, $"{nameof(table)}.{nameof(table.Alias)}");

            Name = name;
            Table = table;
            IsNullable = nullable;
        }

        /// <summary>
        ///     The name of the column.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     The table from which column is being referenced.
        /// </summary>
        public TableExpressionBase Table { get; }

        /// <summary>
        ///     The bool value indicating if this column can have null values.
        /// </summary>
        public bool IsNullable { get; }

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            return this;
        }

        /// <summary>
        ///     Makes this column nullable.
        /// </summary>
        /// <returns> A new expression which has <see cref="IsNullable" /> property set to true. </returns>
        public ColumnExpression MakeNullable()
            => new ColumnExpression(Name, Table, Type, TypeMapping, true);

        /// <inheritdoc />
        protected override void Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

            expressionPrinter.Append(Table.Alias).Append(".");
            expressionPrinter.Append(Name);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is ColumnExpression columnExpression
                    && Equals(columnExpression));

        private bool Equals(ColumnExpression columnExpression)
            => base.Equals(columnExpression)
                && string.Equals(Name, columnExpression.Name)
                && Table.Equals(columnExpression.Table)
                && IsNullable == columnExpression.IsNullable;

        /// <inheritdoc />
        public override int GetHashCode()
            => HashCode.Combine(base.GetHashCode(), Name, Table, IsNullable);

        private string DebuggerDisplay()
            => $"{Table.Alias}.{Name}";
    }
}

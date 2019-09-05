// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    [DebuggerDisplay("{DebuggerDisplay(),nq}")]
    public class ColumnExpression : SqlExpression
    {
        internal ColumnExpression(IProperty property, TableExpressionBase table, bool nullable)
            : this(property.GetColumnName(), table, property.ClrType, property.GetRelationalTypeMapping(),
                  nullable || property.IsNullable || property.DeclaringEntityType.BaseType != null)
        {
        }

        internal ColumnExpression(ProjectionExpression subqueryProjection, TableExpressionBase table)
            : this(subqueryProjection.Alias, table,
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

        public string Name { get; }
        public TableExpressionBase Table { get; }
        public bool IsNullable { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor) => this;

        public ColumnExpression MakeNullable()
            => new ColumnExpression(Name, Table, Type.MakeNullable(), TypeMapping, true);

        public override void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.Append(Table.Alias).Append(".");
            expressionPrinter.Append(Name);
        }

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

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Name, Table, IsNullable);

        private string DebuggerDisplay() => $"{Table.Alias}.{Name}";
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions
{
    public class ColumnExpression : SqlExpression
    {
        public ColumnExpression(IProperty property, TableExpressionBase table)
            : base(property.ClrType, property.FindRelationalMapping(), false, true)
        {
            Name = property.Relational().ColumnName;
            Table = table;
        }

        public ColumnExpression(ProjectionExpression subqueryProjection, TableExpressionBase table)
            : base(subqueryProjection.Type, subqueryProjection.SqlExpression.TypeMapping, false, true)
        {
            Name = subqueryProjection.Alias;
            Table = table;
        }

        private ColumnExpression(string name, TableExpressionBase table,
            Type type, RelationalTypeMapping typeMapping, bool treatAsValue)
            : base(type, typeMapping, false, treatAsValue)
        {
            Name = name;
            Table = table;
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var newTable = (TableExpressionBase)visitor.Visit(Table);

            return newTable != Table
                ? new ColumnExpression(Name, newTable, Type, TypeMapping, ShouldBeValue)
                : this;
        }

        public override SqlExpression ConvertToValue(bool treatAsValue)
        {
            return new ColumnExpression(Name, Table, Type, TypeMapping, treatAsValue);
        }

        public string Name { get; }
        public TableExpressionBase Table { get; }

        public override bool Equals(object obj)
            => obj != null
            && (ReferenceEquals(this, obj)
                || obj is ColumnExpression columnExpression
                    && Equals(columnExpression));

        private bool Equals(ColumnExpression columnExpression)
            => base.Equals(columnExpression)
            && string.Equals(Name, columnExpression.Name)
            && Table.Equals(columnExpression.Table);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ Name.GetHashCode();
                hashCode = (hashCode * 397) ^ Table.GetHashCode();

                return hashCode;
            }
        }
    }
}

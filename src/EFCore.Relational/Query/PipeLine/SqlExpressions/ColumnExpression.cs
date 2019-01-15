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
        #region Fields & Constructors

        internal ColumnExpression(IProperty property, TableExpressionBase table, bool nullable)
            : this(property.Relational().ColumnName, table, property.ClrType, property.FindRelationalMapping(),
                  nullable || property.IsNullable || property.DeclaringEntityType.BaseType != null)
        {
        }

        internal ColumnExpression(ProjectionExpression subqueryProjection, TableExpressionBase table, bool nullable)
            : this(subqueryProjection.Alias, table, subqueryProjection.Type, subqueryProjection.Expression.TypeMapping, nullable)
        {
        }

        private ColumnExpression(string name, TableExpressionBase table, Type type, RelationalTypeMapping typeMapping, bool nullable)
            : base(type, typeMapping)
        {
            Name = name;
            Table = table;
            Nullable = nullable;
        }

        #endregion

        #region Public Properties

        public string Name { get; }
        public TableExpressionBase Table { get; }
        public bool Nullable { get; }

        #endregion

        #region Expression-based methods

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var newTable = (TableExpressionBase)visitor.Visit(Table);

            return newTable != Table
                ? new ColumnExpression(Name, newTable, Type, TypeMapping, Nullable)
                : this;
        }

        public ColumnExpression MakeNullable()
        {
            return new ColumnExpression(Name, Table, Type.MakeNullable(), TypeMapping, true);
        }

        #endregion

        #region Equality & HashCode

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

        #endregion
    }
}

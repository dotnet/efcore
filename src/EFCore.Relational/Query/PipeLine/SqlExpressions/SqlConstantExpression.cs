// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions
{
    public class SqlConstantExpression : SqlExpression
    {
        #region Fields & Constructors
        private readonly ConstantExpression _constantExpression;

        public SqlConstantExpression(ConstantExpression constantExpression, RelationalTypeMapping typeMapping)
            : base(constantExpression.Type, typeMapping)
        {
            _constantExpression = constantExpression;
        }
        #endregion

        #region Public Properties
        public object Value => _constantExpression.Value;
        #endregion

        #region Expression-based methods
        public SqlExpression ApplyTypeMapping(RelationalTypeMapping typeMapping)
        {
            return new SqlConstantExpression(_constantExpression, typeMapping);
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            return this;
        }
        #endregion

        #region Equality & HashCode
        public override bool Equals(object obj)
            => obj != null
            && (ReferenceEquals(this, obj)
                || obj is SqlConstantExpression sqlConstantExpression
                    && Equals(sqlConstantExpression));

        private bool Equals(SqlConstantExpression sqlConstantExpression)
            => base.Equals(sqlConstantExpression)
            && Value.Equals(sqlConstantExpression.Value);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ Value.GetHashCode();

                return hashCode;
            }
        }
        #endregion
    }
}

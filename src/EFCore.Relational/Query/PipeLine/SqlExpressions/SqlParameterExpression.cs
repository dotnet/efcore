// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions
{
    public class SqlParameterExpression : SqlExpression
    {
        #region Fields & Constructors
        private readonly ParameterExpression _parameterExpression;

        internal SqlParameterExpression(ParameterExpression parameterExpression, RelationalTypeMapping typeMapping)
            : base(parameterExpression.Type, typeMapping)
        {
            _parameterExpression = parameterExpression;
        }
        #endregion

        #region Public Properties
        public string Name => _parameterExpression.Name;
        #endregion

        #region Expression-based methods
        public SqlExpression ApplyTypeMapping(RelationalTypeMapping typeMapping)
        {
            return new SqlParameterExpression(_parameterExpression, typeMapping);
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
                || obj is SqlParameterExpression sqlParameterExpression
                    && Equals(sqlParameterExpression));

        private bool Equals(SqlParameterExpression sqlParameterExpression)
            => base.Equals(sqlParameterExpression)
            && string.Equals(Name, sqlParameterExpression.Name);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ Name.GetHashCode();

                return hashCode;
            }
        }
        #endregion
    }
}

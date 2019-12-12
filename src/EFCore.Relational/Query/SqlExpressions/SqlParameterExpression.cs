// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    // Class is sealed because there are no public/protected constructors. Can be unsealed if this is changed.
    public sealed class SqlParameterExpression : SqlExpression
    {
        private readonly ParameterExpression _parameterExpression;

        internal SqlParameterExpression(ParameterExpression parameterExpression, RelationalTypeMapping typeMapping)
            : base(parameterExpression.Type, typeMapping)
        {
            _parameterExpression = parameterExpression;
        }

        public string Name => _parameterExpression.Name;

        public SqlExpression ApplyTypeMapping([CanBeNull] RelationalTypeMapping typeMapping)
        {
            return new SqlParameterExpression(_parameterExpression, typeMapping);
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            return this;
        }

        public override void Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

            expressionPrinter.Append("@" + _parameterExpression.Name);
        }

        public override bool Equals(object obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is SqlParameterExpression sqlParameterExpression
                    && Equals(sqlParameterExpression));

        private bool Equals(SqlParameterExpression sqlParameterExpression)
            => base.Equals(sqlParameterExpression)
                && string.Equals(Name, sqlParameterExpression.Name);

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Name);
    }
}

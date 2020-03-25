// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    public abstract class SqlExpression : Expression, IPrintableExpression
    {
        protected SqlExpression([NotNull] Type type, [CanBeNull] RelationalTypeMapping typeMapping)
        {
            Type = type;
            TypeMapping = typeMapping;
        }

        public override Type Type { get; }
        public virtual RelationalTypeMapping TypeMapping { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
            => throw new InvalidOperationException(CoreStrings.VisitChildrenMustBeOverridden);

        public sealed override ExpressionType NodeType => ExpressionType.Extension;
        public abstract void Print(ExpressionPrinter expressionPrinter);

        public override bool Equals(object obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is SqlExpression sqlExpression
                    && Equals(sqlExpression));

        private bool Equals(SqlExpression sqlExpression)
            => Type == sqlExpression.Type
               && ((TypeMapping == null && sqlExpression.TypeMapping == null)
                || TypeMapping?.Equals(sqlExpression.TypeMapping) == true);

        public override int GetHashCode() => HashCode.Combine(Type, TypeMapping);
    }
}

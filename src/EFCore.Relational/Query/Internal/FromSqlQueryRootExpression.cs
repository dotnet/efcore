// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class FromSqlQueryRootExpression : QueryRootExpression
    {
        public FromSqlQueryRootExpression(
            [NotNull] IAsyncQueryProvider queryProvider, [NotNull] IEntityType entityType, [NotNull] string sql, [NotNull] Expression argument)
            : base(queryProvider, entityType)
        {
            Check.NotEmpty(sql, nameof(sql));
            Check.NotNull(argument, nameof(argument));

            Sql = sql;
            Argument = argument;
        }

        public FromSqlQueryRootExpression(
            [NotNull] IEntityType entityType, [NotNull] string sql, [NotNull] Expression argument)
            : base(entityType)
        {
            Check.NotEmpty(sql, nameof(sql));
            Check.NotNull(argument, nameof(argument));

            Sql = sql;
            Argument = argument;
        }

        public virtual string Sql { get; }
        public virtual Expression Argument { get; }

        public override Expression DetachQueryProvider() => new FromSqlQueryRootExpression(EntityType, Sql, Argument);

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var argument = visitor.Visit(Argument);

            return argument != Argument
                ? new FromSqlQueryRootExpression(EntityType, Sql, argument)
                : this;
        }

        public override void Print(ExpressionPrinter expressionPrinter)
        {
            base.Print(expressionPrinter);
            expressionPrinter.Append($".FromSql({Sql}, ");
            expressionPrinter.Visit(Argument);
            expressionPrinter.AppendLine(")");
        }

        public override bool Equals(object obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is FromSqlQueryRootExpression queryRootExpression
                    && Equals(queryRootExpression));

        private bool Equals(FromSqlQueryRootExpression queryRootExpression)
            => base.Equals(queryRootExpression)
                && string.Equals(Sql, queryRootExpression.Sql, StringComparison.OrdinalIgnoreCase)
                && ExpressionEqualityComparer.Instance.Equals(Argument, queryRootExpression.Argument);

        public override int GetHashCode()
            => HashCode.Combine(base.GetHashCode(), Sql, ExpressionEqualityComparer.Instance.GetHashCode(Argument));
    }
}

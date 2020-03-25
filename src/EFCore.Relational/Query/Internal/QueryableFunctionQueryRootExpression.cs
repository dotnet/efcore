// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class QueryableFunctionQueryRootExpression : QueryRootExpression
    {
        //Since this is always generated while compiling there is no query provider associated
        public QueryableFunctionQueryRootExpression(
            [NotNull] IEntityType entityType, [NotNull] IDbFunction function, [NotNull] IReadOnlyCollection<Expression> arguments)
            : base(entityType)
        {
            Check.NotNull(function, nameof(function));
            Check.NotNull(arguments, nameof(arguments));

            Function = function;
            Arguments = arguments;
        }

        public virtual IDbFunction Function { get; }
        public virtual IReadOnlyCollection<Expression> Arguments { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var arguments = new List<Expression>();
            var changed = false;
            foreach (var argument in Arguments)
            {
                var newArgument = visitor.Visit(argument);
                arguments.Add(newArgument);
                changed |= argument != newArgument;
            }

            return changed
                ? new QueryableFunctionQueryRootExpression(EntityType, Function, arguments)
                : this;
        }

        public override void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.Append(Function.MethodInfo.DisplayName());
            expressionPrinter.Append("(");
            expressionPrinter.VisitCollection(Arguments);
            expressionPrinter.Append(")");
        }

        public override bool Equals(object obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is QueryableFunctionQueryRootExpression queryRootExpression
                    && Equals(queryRootExpression));

        private bool Equals(QueryableFunctionQueryRootExpression queryRootExpression)
            => base.Equals(queryRootExpression)
                && Equals(Function, queryRootExpression.Function)
                && Arguments.SequenceEqual(queryRootExpression.Arguments, ExpressionEqualityComparer.Instance);

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(base.GetHashCode());
            hashCode.Add(Function);
            foreach (var item in Arguments)
            {
                hashCode.Add(item);
            }

            return hashCode.ToHashCode();
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Query.Expressions
{
    public class PropertyEvaluationPreventingExpression : ExtensionExpression
    {
        public PropertyEvaluationPreventingExpression([NotNull] MemberExpression argument)
            : base(argument.Type)
        {
            MemberExpression = argument;
        }

        public virtual MemberExpression MemberExpression { get; private set; }

        public override bool CanReduce
        {
            get { return true; }
        }

        public override Expression Reduce()
        {
            return MemberExpression;
        }

        protected override Expression VisitChildren(ExpressionTreeVisitor visitor)
        {
            var newExpression = visitor.VisitExpression(MemberExpression.Expression);

            if (newExpression != MemberExpression.Expression)
            {
                return new PropertyEvaluationPreventingExpression(
                    Property(newExpression, MemberExpression.Member.Name));
            }

            return this;
        }
    }
}

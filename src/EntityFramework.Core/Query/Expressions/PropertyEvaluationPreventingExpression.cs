// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Query.Expressions
{
    public class PropertyEvaluationPreventingExpression : Expression
    {
        private readonly MemberExpression _memberExpression;

        public PropertyEvaluationPreventingExpression([NotNull] MemberExpression argument)
        {
            _memberExpression = argument;
        }

        public virtual MemberExpression MemberExpression => _memberExpression;

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override Type Type => _memberExpression.Type;

        public override bool CanReduce
        {
            get { return true; }
        }

        public override Expression Reduce()
        {
            return MemberExpression;
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var newExpression = visitor.Visit(MemberExpression.Expression);

            if (newExpression != MemberExpression.Expression)
            {
                return new PropertyEvaluationPreventingExpression(
                    Property(newExpression, MemberExpression.Member.Name));
            }

            return this;
        }
    }
}

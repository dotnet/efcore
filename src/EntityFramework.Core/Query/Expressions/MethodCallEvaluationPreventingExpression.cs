// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query.Expressions
{
    public class MethodCallEvaluationPreventingExpression : Expression
    {
        private readonly MethodCallExpression _methodCall;

        public MethodCallEvaluationPreventingExpression([NotNull] MethodCallExpression argument)
        {
            Check.NotNull(argument, nameof(argument));

            _methodCall = argument;
        }

        public virtual MethodCallExpression MethodCall => _methodCall;

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override Type Type => _methodCall.Type;

        public override bool CanReduce
        {
            get { return true; }
        }

        public override Expression Reduce()
        {
            return MethodCall;
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var newObject = visitor.Visit(MethodCall.Object);
            var newArguments = visitor.VisitAndConvert(MethodCall.Arguments, "VisitChildren");

            if (newObject != MethodCall.Object
                || newArguments != MethodCall.Arguments)
            {
                return new MethodCallEvaluationPreventingExpression(
                    Call(newObject, MethodCall.Method, newArguments));
            }

            return this;
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.Data.Entity.Utilities;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Query.Expressions
{
    public class MethodCallEvaluationPreventingExpression : ExtensionExpression
    {
        public MethodCallEvaluationPreventingExpression([NotNull] MethodCallExpression argument)
            : base(argument.Type)
        {
            Check.NotNull(argument, nameof(argument));

            MethodCall = argument;
        }

        public virtual MethodCallExpression MethodCall { get; private set; }

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

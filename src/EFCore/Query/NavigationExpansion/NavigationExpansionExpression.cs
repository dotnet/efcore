// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Query.NavigationExpansion
{
    public class NavigationExpansionExpression : Expression, IPrintable
    {
        private Type _returnType;

        public NavigationExpansionExpression(
            Expression operand,
            NavigationExpansionExpressionState state,
            Type returnType)
        {
            Operand = operand;
            State = state;
            _returnType = returnType;
        }

        public virtual Expression Operand { get; }
        public virtual NavigationExpansionExpressionState State { get; private set; }

        public override ExpressionType NodeType => ExpressionType.Extension;
        public override Type Type => _returnType;
        public override bool CanReduce => false;

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var newOperand = visitor.Visit(Operand);
            var newState = ReplaceNavigationExpansionExpressionState(visitor);

            return Update(newOperand, newState);
        }

        public virtual NavigationExpansionExpression Update(Expression operand, NavigationExpansionExpressionState state)
            => operand != Operand || state != State
            ? new NavigationExpansionExpression(operand, state, Type)
            : this;

        private NavigationExpansionExpressionState ReplaceNavigationExpansionExpressionState(ExpressionVisitor visitor)
        {
            var newCurrentParameter = (ParameterExpression)visitor.Visit(State.CurrentParameter);
            var newPendingSelector = (LambdaExpression)visitor.Visit(State.PendingSelector);
            var pendingOrderingsChanged = false;
            var newPendingOrderings = new List<(MethodInfo method, LambdaExpression keySelector)>();

            foreach (var pendingOrdering in State.PendingOrderings)
            {
                var newPendingOrderingKeySelector = (LambdaExpression)visitor.Visit(pendingOrdering.keySelector);
                if (newPendingOrderingKeySelector != pendingOrdering.keySelector)
                {
                    newPendingOrderings.Add((pendingOrdering.method, keySelector: newPendingOrderingKeySelector));
                    pendingOrderingsChanged = true;
                }
                else
                {
                    newPendingOrderings.Add(pendingOrdering);
                }
            }

            var newPendingIncludeChain = (NavigationBindingExpression)visitor.Visit(State.PendingIncludeChain);

            if (newCurrentParameter != State.CurrentParameter
                || newPendingSelector != State.PendingSelector
                || pendingOrderingsChanged
                || newPendingIncludeChain != State.PendingIncludeChain)
            {
                return new NavigationExpansionExpressionState(
                    newCurrentParameter,
                    State.SourceMappings,
                    newPendingSelector,
                    State.ApplyPendingSelector,
                    newPendingOrderings,
                    newPendingIncludeChain,
                    State.PendingCardinalityReducingOperator,
                    State.PendingTags,
                    State.CustomRootMappings,
                    State.MaterializeCollectionNavigation);
            }

            return State;
        }

        public virtual void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.Visit(Operand);

            if (State.ApplyPendingSelector)
            {
                expressionPrinter.StringBuilder.Append(".PendingSelect(");
                expressionPrinter.Visit(State.PendingSelector);
                expressionPrinter.StringBuilder.Append(")");
            }

            if (State.PendingCardinalityReducingOperator != null)
            {
                expressionPrinter.StringBuilder.Append(".Pending" + State.PendingCardinalityReducingOperator.Name);
            }
        }
    }
}

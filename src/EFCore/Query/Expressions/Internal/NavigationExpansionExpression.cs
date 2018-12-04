// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Query.Expressions.Internal
{
    public class NavigationExpansionExpression : Expression, IPrintable
    {
        private MethodInfo _selectMethodInfo
            = typeof(Queryable).GetMethods().Where(m => m.Name == nameof(Queryable.Select) && m.GetParameters()[1].ParameterType.GetGenericArguments()[0].GetGenericArguments().Count() == 2).Single();

        private Type _returnType;

        public override ExpressionType NodeType => ExpressionType.Extension;
        public override Type Type => _returnType;
        public override bool CanReduce => true;
        public override Expression Reduce()
        {
            var parameter = Parameter(Operand.Type.GetGenericArguments()[0]);
            var body = (Expression)parameter;
            foreach (var finalProjectionPathElement in FinalProjectionPath)
            {
                body = Field(body, finalProjectionPathElement);
            }

            var lambda = Lambda(body, parameter);
            var method =_selectMethodInfo.MakeGenericMethod(parameter.Type, body.Type);

            return Call(method, Operand, lambda);
        }
            
        public Expression Operand { get; }

        public ParameterExpression ParameterExpression { get; }

        public List<(List<string> from, List<string> to)> TransparentIdentifierAccessorMapping { get; }

        public List<NavigationPathNode> FoundNavigations { get; }

        public List<string> FinalProjectionPath { get; }

        public NavigationExpansionExpression(
            Expression operand,
            ParameterExpression parameterExpression,
            List<(List<string> from, List<string> to)> transparentIdentifierAccessorMapping,
            List<NavigationPathNode> foundNavigations,
            List<string> finalProjectionPath,
            Type returnType)
        {
            Operand = operand;
            ParameterExpression = parameterExpression;
            TransparentIdentifierAccessorMapping = transparentIdentifierAccessorMapping;
            FoundNavigations = foundNavigations;
            FinalProjectionPath = finalProjectionPath;
            _returnType = returnType;
        }

        public void Print([NotNull] ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.Print(Operand);
        }
    }
}

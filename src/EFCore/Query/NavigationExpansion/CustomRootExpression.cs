// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Query.NavigationExpansion
{
    public class CustomRootExpression : Expression, IPrintable
    {
        public virtual ParameterExpression RootParameter { get; }
        public virtual List<string> Mapping { get; }
        public override ExpressionType NodeType => ExpressionType.Extension;
        public override bool CanReduce => false;
        public override Type Type { get; }

        public CustomRootExpression(ParameterExpression rootParameter, List<string> mapping, Type type)
        {
            RootParameter = rootParameter;
            Mapping = mapping;
            Type = type;
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var newRootParameter = (ParameterExpression)visitor.Visit(RootParameter);

            return Update(newRootParameter);
        }

        public virtual CustomRootExpression Update(ParameterExpression rootParameter)
            => rootParameter != RootParameter
            ? new CustomRootExpression(rootParameter, Mapping, Type)
            : this;

        public virtual void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.StringBuilder.Append("CUSTOM_ROOT([" + Type.ShortDisplayName() + "] | ");
            expressionPrinter.Visit(RootParameter);
            if (Mapping.Count > 0)
            {
                expressionPrinter.StringBuilder.Append(".");
                expressionPrinter.StringBuilder.Append(string.Join(".", Mapping));
            }

            expressionPrinter.StringBuilder.Append(")");
        }
    }
}

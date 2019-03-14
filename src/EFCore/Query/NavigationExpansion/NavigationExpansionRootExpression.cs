
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
    public class NavigationExpansionRootExpression : Expression, IPrintable
    {
        public NavigationExpansionRootExpression(NavigationExpansionExpression navigationExpansion, List<string> mapping)
        {
            NavigationExpansion = navigationExpansion;
            Mapping = mapping;
        }

        public virtual NavigationExpansionExpression NavigationExpansion { get; }
        public virtual List<string> Mapping { get; }
        public override ExpressionType NodeType => ExpressionType.Extension;
        public override bool CanReduce => false;
        public override Type Type => NavigationExpansion.Type;

        public virtual Expression Unwrap()
        {
            if (Mapping.Count == 0)
            {
                return NavigationExpansion;
            }

            var newOperand = NavigationExpansion.Operand.BuildPropertyAccess(Mapping);

            return new NavigationExpansionExpression(newOperand, NavigationExpansion.State, NavigationExpansion.Type);
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var newNavigationExpansion = (NavigationExpansionExpression)visitor.Visit(NavigationExpansion);

            return Update(newNavigationExpansion);
        }

        public virtual NavigationExpansionRootExpression Update(NavigationExpansionExpression navigationExpansion)
            => navigationExpansion != NavigationExpansion
            ? new NavigationExpansionRootExpression(navigationExpansion, Mapping)
            : this;

        public virtual void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.StringBuilder.Append("EXPANSION_ROOT([" + Type.ShortDisplayName() + "] | ");
            expressionPrinter.Visit(Unwrap());
            expressionPrinter.StringBuilder.Append(")");
        }
    }
}

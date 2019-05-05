// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Query.NavigationExpansion
{
    public class IncludeExpression : Expression, IPrintable
    {
        public IncludeExpression(Expression entityExpression, Expression navigationExpression, INavigation navigation)
        {
            EntityExpression = entityExpression;
            NavigationExpression = navigationExpression;
            Navigation = navigation;
            Type = EntityExpression.Type;
        }

        public virtual Expression EntityExpression { get; set; }
        public virtual Expression NavigationExpression { get; set; }
        public virtual INavigation Navigation { get; set; }

        public override ExpressionType NodeType => ExpressionType.Extension;
        public override bool CanReduce => false;
        public override Type Type { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var newEntityExpression = visitor.Visit(EntityExpression);
            var newNavigationExpression = visitor.Visit(NavigationExpression);

            return Update(newEntityExpression, newNavigationExpression);
        }

        public virtual IncludeExpression Update(Expression entityExpression, Expression navigationExpression)
            => entityExpression != EntityExpression || navigationExpression != NavigationExpression
            ? new IncludeExpression(entityExpression, navigationExpression, Navigation)
            : this;

        public virtual void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.StringBuilder.AppendLine("Include(");
            expressionPrinter.StringBuilder.IncrementIndent();
            expressionPrinter.Visit(EntityExpression);
            expressionPrinter.StringBuilder.AppendLine(", ");
            expressionPrinter.Visit(NavigationExpression);
            expressionPrinter.StringBuilder.AppendLine(")");
            expressionPrinter.StringBuilder.DecrementIndent();
        }
    }
}

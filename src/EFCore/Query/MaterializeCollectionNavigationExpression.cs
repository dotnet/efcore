// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class MaterializeCollectionNavigationExpression : Expression, IPrintableExpression
    {
        public MaterializeCollectionNavigationExpression(Expression subquery, INavigation navigation)
        {
            Subquery = subquery;
            Navigation = navigation;
        }

        public virtual Expression Subquery { get; }
        public virtual INavigation Navigation { get; }

        public sealed override ExpressionType NodeType => ExpressionType.Extension;
        public override Type Type => Navigation.ClrType;

        protected override Expression VisitChildren(ExpressionVisitor visitor)
            => Update(visitor.Visit(Subquery));

        public virtual MaterializeCollectionNavigationExpression Update(Expression subquery)
            => subquery != Subquery
                ? new MaterializeCollectionNavigationExpression(subquery, Navigation)
                : this;

        public virtual void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.Append($"MaterializeCollectionNavigation({Navigation}, ");
            expressionPrinter.Visit(Subquery);
            expressionPrinter.Append(")");
        }
    }
}

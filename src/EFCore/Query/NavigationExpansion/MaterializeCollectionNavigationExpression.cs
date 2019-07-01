// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Query.NavigationExpansion
{
    public class MaterializeCollectionNavigationExpression : Expression, IPrintable
    {
        public MaterializeCollectionNavigationExpression(Expression subquery, INavigation navigation)
        {
            Subquery = subquery;
            Navigation = navigation;
        }

        public virtual Expression Subquery { get; }
        public virtual INavigation Navigation { get; }

        public override ExpressionType NodeType => ExpressionType.Extension;
        public override Type Type => Navigation.ClrType;

        public virtual void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.StringBuilder.Append($"MaterializeCollectionNavigation({Navigation}, ");
            expressionPrinter.Visit(Subquery);
            expressionPrinter.StringBuilder.Append(")");
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var subquery = visitor.Visit(Subquery);

            return subquery != Subquery
                ? new MaterializeCollectionNavigationExpression(subquery, Navigation)
                : this;
        }


    }
}

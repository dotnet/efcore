// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class MaterializeCollectionNavigationExpression : Expression, IPrintableExpression
    {
        public MaterializeCollectionNavigationExpression([NotNull] Expression subquery, [NotNull] INavigation navigation)
        {
            Check.NotNull(subquery, nameof(subquery));
            Check.NotNull(navigation, nameof(navigation));

            Subquery = subquery;
            Navigation = navigation;
        }

        public virtual Expression Subquery { get; }
        public virtual INavigation Navigation { get; }

        public sealed override ExpressionType NodeType => ExpressionType.Extension;
        public override Type Type => Navigation.ClrType;

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            return Update(visitor.Visit(Subquery));
        }

        public virtual MaterializeCollectionNavigationExpression Update([NotNull] Expression subquery)
        {
            Check.NotNull(subquery, nameof(subquery));

            return subquery != Subquery
                ? new MaterializeCollectionNavigationExpression(subquery, Navigation)
                : this;
        }

        public virtual void Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

            expressionPrinter.AppendLine("MaterializeCollectionNavigation(");
            using (expressionPrinter.Indent())
            {
                expressionPrinter.AppendLine($"navigation: Navigation: {Navigation.DeclaringEntityType.DisplayName()}.{Navigation.Name},");
                expressionPrinter.Append("subquery: ");
                expressionPrinter.Visit(Subquery);
            }
        }
    }
}

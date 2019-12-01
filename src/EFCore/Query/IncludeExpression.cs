// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class IncludeExpression : Expression, IPrintableExpression
    {
        private Expression _entityExpression;
        private Expression _navigationExpression;
        private INavigation _navigation;

        public IncludeExpression(
            [NotNull] Expression entityExpression,
            [NotNull] Expression navigationExpression,
            [NotNull] INavigation navigation)
        {
            Check.NotNull(entityExpression, nameof(entityExpression));
            Check.NotNull(navigationExpression, nameof(navigationExpression));
            Check.NotNull(navigation, nameof(navigation));

            EntityExpression = entityExpression;
            NavigationExpression = navigationExpression;
            Navigation = navigation;
            Type = EntityExpression.Type;
        }

        public virtual Expression EntityExpression
        {
            get => _entityExpression;
            [param: NotNull] set => _entityExpression = Check.NotNull(value, nameof(value));
        }

        public virtual Expression NavigationExpression
        {
            get => _navigationExpression;
            [param: NotNull] set => _navigationExpression = Check.NotNull(value, nameof(value));
        }

        public virtual INavigation Navigation
        {
            get => _navigation;
            [param: NotNull] set => _navigation = Check.NotNull(value, nameof(value));
        }

        public sealed override ExpressionType NodeType => ExpressionType.Extension;
        public override Type Type { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var newEntityExpression = visitor.Visit(EntityExpression);
            var newNavigationExpression = visitor.Visit(NavigationExpression);

            return Update(newEntityExpression, newNavigationExpression);
        }

        public virtual IncludeExpression Update([NotNull] Expression entityExpression, [NotNull] Expression navigationExpression)
        {
            Check.NotNull(entityExpression, nameof(entityExpression));
            Check.NotNull(navigationExpression, nameof(navigationExpression));

            return entityExpression != EntityExpression || navigationExpression != NavigationExpression
                ? new IncludeExpression(entityExpression, navigationExpression, Navigation)
                : this;
        }

        public virtual void Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

            expressionPrinter.AppendLine("IncludeExpression(");
            using (expressionPrinter.Indent())
            {
                expressionPrinter.Visit(EntityExpression);
                expressionPrinter.AppendLine(", ");
                expressionPrinter.Visit(NavigationExpression);
                expressionPrinter.AppendLine($", {Navigation.Name})");
            }
        }
    }
}

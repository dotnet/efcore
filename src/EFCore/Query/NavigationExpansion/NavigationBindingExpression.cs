// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Query.NavigationExpansion
{
    public class NavigationBindingExpression : Expression, IPrintable
    {
        public NavigationBindingExpression(
            ParameterExpression rootParameter,
            NavigationTreeNode navigationTreeNode,
            IEntityType entityType,
            SourceMapping sourceMapping,
            Type type)
        {
            RootParameter = rootParameter;
            NavigationTreeNode = navigationTreeNode;
            EntityType = entityType;
            SourceMapping = sourceMapping;
            Type = type;
        }

        public virtual ParameterExpression RootParameter { get; }
        public virtual IEntityType EntityType { get; }
        public virtual NavigationTreeNode NavigationTreeNode { get; }
        public virtual SourceMapping SourceMapping { get; }

        public override ExpressionType NodeType => ExpressionType.Extension;
        public override bool CanReduce => false;
        public override Type Type { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var newRootParameter = (ParameterExpression)visitor.Visit(RootParameter);

            return Update(newRootParameter);
        }

        public virtual NavigationBindingExpression Update(ParameterExpression rootParameter)
            => rootParameter != RootParameter
            ? new NavigationBindingExpression(rootParameter, NavigationTreeNode, EntityType, SourceMapping, Type)
            : this;

        public virtual void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.StringBuilder.Append("BINDING([" + EntityType.ClrType.ShortDisplayName() + "] | from: ");
            expressionPrinter.StringBuilder.Append(string.Join(".", NavigationTreeNode.FromMappings.First()) + " to: ");
            expressionPrinter.Visit(RootParameter);
            if (NavigationTreeNode.ToMapping.Count > 0)
            {
                expressionPrinter.StringBuilder.Append(".");
                expressionPrinter.StringBuilder.Append(string.Join(".", NavigationTreeNode.ToMapping));
            }

            expressionPrinter.StringBuilder.Append(")");
        }
    }
}

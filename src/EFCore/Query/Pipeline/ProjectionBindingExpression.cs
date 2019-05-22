// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Query.Pipeline
{
    public class ProjectionBindingExpression : Expression, IPrintable
    {
        public ProjectionBindingExpression(Expression queryExpression, ProjectionMember projectionMember, Type type)
        {
            QueryExpression = queryExpression;
            ProjectionMember = projectionMember;
            Type = type;
        }

        public ProjectionBindingExpression(Expression queryExpression, int index, Type type)
        {
            QueryExpression = queryExpression;
            Index = index;
            Type = type;
        }

        public Expression QueryExpression { get; }
        public ProjectionMember ProjectionMember { get; }
        public int Index { get; }
        public override Type Type { get; }
        public override ExpressionType NodeType => ExpressionType.Extension;

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            return this;
        }

        public void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.StringBuilder.Append(nameof(ProjectionBindingExpression) + ": " + ProjectionMember + "/" + Index);
        }
    }
}

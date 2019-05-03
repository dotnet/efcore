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
        public ProjectionBindingExpression(ProjectionMember projectionMember, Type type)
        {
            ProjectionMember = projectionMember;
            Type = type;
        }

        public ProjectionMember ProjectionMember { get; }
        public override Type Type { get; }
        public override ExpressionType NodeType => ExpressionType.Extension;

        public void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.StringBuilder.Append(nameof(ProjectionBindingExpression) + ": " + ProjectionMember);
        }
    }
}

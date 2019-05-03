// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.Pipeline
{
    public class ProjectionBindingExpression : Expression
    {
        public ProjectionBindingExpression(ProjectionMember projectionMember, Type type)
        {
            ProjectionMember = projectionMember;
            Type = type;
        }

        public ProjectionMember ProjectionMember { get; }
        public override Type Type { get; }
        public override ExpressionType NodeType => ExpressionType.Extension;
    }

}

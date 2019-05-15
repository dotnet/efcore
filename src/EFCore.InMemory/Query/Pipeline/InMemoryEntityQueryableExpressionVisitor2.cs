// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Pipeline;

namespace Microsoft.EntityFrameworkCore.InMemory.Query.Pipeline
{
    public class InMemoryEntityQueryableExpressionVisitor2 : EntityQueryableExpressionVisitor2
    {
        private readonly IModel _model;

        public InMemoryEntityQueryableExpressionVisitor2(IModel model)
        {
            _model = model;
        }

        protected override ShapedQueryExpression CreateShapedQueryExpression(Type elementType)
        {
            return new InMemoryShapedQueryExpression(_model.FindEntityType(elementType));
        }
    }
}

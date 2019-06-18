// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.InMemory.Query.Pipeline
{
    public class InMemoryEntityQueryableTranslator : EntityQueryableTranslator
    {
        private readonly IModel _model;

        public InMemoryEntityQueryableTranslator(IModel model)
        {
            _model = model;
        }

        protected override ShapedQueryExpression CreateShapedQueryExpression(Type elementType)
        {
            var entityType = _model.FindEntityType(elementType);
            var queryExpression = new InMemoryQueryExpression(entityType);

            return new ShapedQueryExpression(
                queryExpression,
                new EntityShaperExpression(
                    entityType,
                    new ProjectionBindingExpression(
                        queryExpression,
                        new ProjectionMember(),
                        typeof(ValueBuffer)),
                    false));
        }
    }
}

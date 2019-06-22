// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Pipeline
{
    public class CosmosEntityQueryableTranslatorFactory : EntityQueryableTranslatorFactory
    {
        private readonly IModel _model;
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public CosmosEntityQueryableTranslatorFactory(IModel model,
            ISqlExpressionFactory sqlExpressionFactory)
        {
            _model = model;
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public override EntityQueryableTranslator Create(QueryCompilationContext queryCompilationContext)
        {
            return new CosmosEntityQueryableTranslator(_model, _sqlExpressionFactory);
        }
    }

    public class CosmosEntityQueryableTranslator : EntityQueryableTranslator
    {
        private readonly IModel _model;
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public CosmosEntityQueryableTranslator(IModel model,
            ISqlExpressionFactory sqlExpressionFactory)
        {
            _model = model;
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        protected override ShapedQueryExpression CreateShapedQueryExpression(Type elementType)
        {
            var entityType = _model.FindEntityType(elementType);
            var selectExpression = _sqlExpressionFactory.Select(entityType);

            return new ShapedQueryExpression(
                selectExpression,
                new EntityShaperExpression(
                entityType,
                new ProjectionBindingExpression(
                    selectExpression,
                    new ProjectionMember(),
                    typeof(ValueBuffer)),
                false));
        }
    }
}

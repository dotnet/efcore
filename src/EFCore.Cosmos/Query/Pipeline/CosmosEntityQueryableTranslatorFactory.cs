// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Pipeline;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Pipeline
{
    public class CosmosEntityQueryableTranslatorFactory : EntityQueryableTranslatorFactory
    {
        private readonly IModel _model;

        public CosmosEntityQueryableTranslatorFactory(IModel model)
        {
            _model = model;
        }

        public override EntityQueryableTranslator Create(QueryCompilationContext2 queryCompilationContext)
        {
            throw new NotImplementedException();
        }
    }

    public class CosmosQueryableMethodTranslatingExpressionVisitorFactory : IQueryableMethodTranslatingExpressionVisitorFactory
    {
        public QueryableMethodTranslatingExpressionVisitor Create(IModel model)
        {
            throw new NotImplementedException();
        }
    }

    public class CosmosShapedQueryCompilingExpressionVisitorFactory : IShapedQueryCompilingExpressionVisitorFactory
    {
        private readonly IEntityMaterializerSource _entityMaterializerSource;

        public CosmosShapedQueryCompilingExpressionVisitorFactory(IEntityMaterializerSource entityMaterializerSource)
        {
            _entityMaterializerSource = entityMaterializerSource;
        }

        public ShapedQueryCompilingExpressionVisitor Create(QueryCompilationContext2 queryCompilationContext)
        {
            throw new NotImplementedException();
            //return new CosmosShapedQueryCompilingExpressionVisitor(
            //    _entityMaterializerSource,
            //    queryCompilationContext.TrackQueryResults,
            //    queryCompilationContext.Async);
        }
    }
}

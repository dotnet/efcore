// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Remotion.Linq.Clauses;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class DocumentDbEntityQueryableExpressionVisitorFactory : IEntityQueryableExpressionVisitorFactory
    {
        private readonly IDocumentDbClientService _documentDbClientService;
        private readonly IModel _model;
        private readonly IEntityMaterializerSource _entityMaterializerSource;

        public DocumentDbEntityQueryableExpressionVisitorFactory(
            IDocumentDbClientService documentDbClientService,
            IModel model,
            IEntityMaterializerSource entityMaterializerSource)
        {
            _documentDbClientService = documentDbClientService;
            _model = model;
            _entityMaterializerSource = entityMaterializerSource;
        }
        public ExpressionVisitor Create(EntityQueryModelVisitor entityQueryModelVisitor, IQuerySource querySource)
        {
            return new DocumentDbEntityQueryableExpressionVisitor(
                _documentDbClientService,
                _model,
                entityQueryModelVisitor,
                querySource,
                _entityMaterializerSource);
        }
    }
}

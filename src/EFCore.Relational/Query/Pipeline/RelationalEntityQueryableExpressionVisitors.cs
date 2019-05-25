// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Pipeline;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
{
    public class RelationalEntityQueryableTranslator : EntityQueryableTranslator
    {
        private readonly IModel _model;
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public RelationalEntityQueryableTranslator(IModel model, ISqlExpressionFactory sqlExpressionFactory)
        {
            _model = model;
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public override Expression Visit(Expression query)
        {
            return new RelationalEntityQueryableExpressionVisitor2(_model, _sqlExpressionFactory).Visit(query);
        }
    }
}

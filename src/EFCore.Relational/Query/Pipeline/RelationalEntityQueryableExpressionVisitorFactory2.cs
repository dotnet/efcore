// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Pipeline;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
{
    public class RelationalEntityQueryableTranslatorFactory : EntityQueryableTranslatorFactory
    {
        private readonly IModel _model;
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public RelationalEntityQueryableTranslatorFactory(IModel model, ISqlExpressionFactory sqlExpressionFactory)
        {
            _model = model;
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public override EntityQueryableTranslator Create(QueryCompilationContext2 queryCompilationContext)
        {
            return new RelationalEntityQueryableTranslator(_model, _sqlExpressionFactory);
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Pipeline;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
{
    public class RelationalEntityQueryableTranslatorFactory : EntityQueryableTranslatorFactory
    {
        private readonly IModel _model;

        public RelationalEntityQueryableTranslatorFactory(IModel model)
        {
            _model = model;
        }

        public override EntityQueryableTranslator Create(QueryCompilationContext2 queryCompilationContext)
        {
            return new RelationalEntityQueryableTranslator(_model);
        }
    }
}

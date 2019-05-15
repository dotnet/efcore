// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Pipeline;

namespace Microsoft.EntityFrameworkCore.InMemory.Query.Pipeline
{
    public class InMemoryEntityQueryableTranslatorFactory : EntityQueryableTranslatorFactory
    {
        private readonly IModel _model;

        public InMemoryEntityQueryableTranslatorFactory(IModel model)
        {
            _model = model;
        }

        public override EntityQueryableTranslator Create(QueryCompilationContext2 queryCompilationContext)
        {
            return new InMemoryEntityQueryableTranslator(_model);
        }
    }
}

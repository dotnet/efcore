// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
{
    public class QuerySqlGeneratorFactory : IQuerySqlGeneratorFactory
    {
        private readonly IRelationalCommandBuilderFactory _commandBuilderFactory;
        private readonly ISqlGenerationHelper _sqlGenerationHelper;

        public QuerySqlGeneratorFactory(
            IRelationalCommandBuilderFactory commandBuilderFactory,
            ISqlGenerationHelper sqlGenerationHelper)
        {
            _commandBuilderFactory = commandBuilderFactory;
            _sqlGenerationHelper = sqlGenerationHelper;
        }

        public virtual QuerySqlGenerator Create()
        {
            return new QuerySqlGenerator(
                _commandBuilderFactory,
                _sqlGenerationHelper);
        }
    }
}

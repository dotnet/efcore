// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class QuerySqlGeneratorFactory : IQuerySqlGeneratorFactory
    {
        private readonly QuerySqlGeneratorDependencies _dependencies;

        public QuerySqlGeneratorFactory(QuerySqlGeneratorDependencies dependencies)
        {
            _dependencies = dependencies;
        }

        public virtual QuerySqlGenerator Create()
            => new QuerySqlGenerator(_dependencies);
    }
}

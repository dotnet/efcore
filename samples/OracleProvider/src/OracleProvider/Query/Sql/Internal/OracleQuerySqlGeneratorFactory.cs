// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Oracle.Query.Sql.Internal
{
    public class OracleQuerySqlGeneratorFactory : QuerySqlGeneratorFactoryBase
    {
        public OracleQuerySqlGeneratorFactory(
            [NotNull] QuerySqlGeneratorDependencies dependencies)
            : base(dependencies)
        {
        }

        public override IQuerySqlGenerator CreateDefault(SelectExpression selectExpression)
            => new OracleQuerySqlGenerator(
                Dependencies,
                Check.NotNull(selectExpression, nameof(selectExpression)));
    }
}

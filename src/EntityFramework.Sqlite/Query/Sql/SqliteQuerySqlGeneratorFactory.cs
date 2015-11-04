// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.Expressions;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query.Sql
{
    public class SqliteQuerySqlGeneratorFactory : QuerySqlGeneratorFactoryBase
    {
        public SqliteQuerySqlGeneratorFactory(
            [NotNull] IRelationalCommandBuilderFactory commandBuilderFactory,
            [NotNull] ISqlGenerator sqlGenerator,
            [NotNull] IParameterNameGeneratorFactory parameterNameGeneratorFactory)
            : base(
                Check.NotNull(commandBuilderFactory, nameof(commandBuilderFactory)),
                Check.NotNull(sqlGenerator, nameof(sqlGenerator)),
                Check.NotNull(parameterNameGeneratorFactory, nameof(parameterNameGeneratorFactory)))
        {
        }

        public override IQuerySqlGenerator CreateDefault(SelectExpression selectExpression)
            => new SqliteQuerySqlGenerator(
                CommandBuilderFactory,
                SqlGenerator,
                ParameterNameGeneratorFactory,
                Check.NotNull(selectExpression, nameof(selectExpression)));
    }
}

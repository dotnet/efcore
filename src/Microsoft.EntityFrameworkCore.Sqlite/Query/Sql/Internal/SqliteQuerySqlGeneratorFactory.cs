// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Sql.Internal
{
    public class SqliteQuerySqlGeneratorFactory : QuerySqlGeneratorFactoryBase
    {
        public SqliteQuerySqlGeneratorFactory(
            [NotNull] IRelationalCommandBuilderFactory commandBuilderFactory,
            [NotNull] ISqlGenerationHelper sqlGenerationHelper,
            [NotNull] IParameterNameGeneratorFactory parameterNameGeneratorFactory,
            [NotNull] IRelationalTypeMapper relationalTypeMapper)
            : base(
                Check.NotNull(commandBuilderFactory, nameof(commandBuilderFactory)),
                Check.NotNull(sqlGenerationHelper, nameof(sqlGenerationHelper)),
                Check.NotNull(parameterNameGeneratorFactory, nameof(parameterNameGeneratorFactory)),
                Check.NotNull(relationalTypeMapper, nameof(relationalTypeMapper)))
        {
        }

        public override IQuerySqlGenerator CreateDefault(SelectExpression selectExpression)
            => new SqliteQuerySqlGenerator(
                CommandBuilderFactory,
                SqlGenerationHelper,
                ParameterNameGeneratorFactory,
                RelationalTypeMapper,
                Check.NotNull(selectExpression, nameof(selectExpression)));
    }
}

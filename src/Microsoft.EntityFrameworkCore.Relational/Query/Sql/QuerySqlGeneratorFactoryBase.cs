// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.Sql.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Sql
{
    public abstract class QuerySqlGeneratorFactoryBase : IQuerySqlGeneratorFactory
    {
        protected QuerySqlGeneratorFactoryBase(
            [NotNull] IRelationalCommandBuilderFactory commandBuilderFactory,
            [NotNull] ISqlGenerationHelper sqlGenerationHelper,
            [NotNull] IParameterNameGeneratorFactory parameterNameGeneratorFactory,
            [NotNull] IRelationalTypeMapper relationalTypeMapper)
        {
            Check.NotNull(commandBuilderFactory, nameof(commandBuilderFactory));
            Check.NotNull(sqlGenerationHelper, nameof(sqlGenerationHelper));
            Check.NotNull(parameterNameGeneratorFactory, nameof(parameterNameGeneratorFactory));
            Check.NotNull(relationalTypeMapper, nameof(relationalTypeMapper));

            CommandBuilderFactory = commandBuilderFactory;
            SqlGenerationHelper = sqlGenerationHelper;
            ParameterNameGeneratorFactory = parameterNameGeneratorFactory;
            RelationalTypeMapper = relationalTypeMapper;
        }

        protected virtual IRelationalCommandBuilderFactory CommandBuilderFactory { get; }
        protected virtual ISqlGenerationHelper SqlGenerationHelper { get; }
        protected virtual IParameterNameGeneratorFactory ParameterNameGeneratorFactory { get; }
        protected virtual IRelationalTypeMapper RelationalTypeMapper { get; }

        public abstract IQuerySqlGenerator CreateDefault(SelectExpression selectExpression);

        public virtual IQuerySqlGenerator CreateFromSql(
            SelectExpression selectExpression,
            string sql,
            Expression arguments)
            => new FromSqlNonComposedQuerySqlGenerator(
                CommandBuilderFactory,
                SqlGenerationHelper,
                ParameterNameGeneratorFactory,
                RelationalTypeMapper,
                Check.NotNull(selectExpression, nameof(selectExpression)),
                Check.NotEmpty(sql, nameof(sql)),
                Check.NotNull(arguments, nameof(arguments)));
    }
}

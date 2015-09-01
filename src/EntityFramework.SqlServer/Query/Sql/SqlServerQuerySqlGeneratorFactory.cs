// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.Expressions;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query.Sql
{
    public class SqlServerQuerySqlGeneratorFactory : ISqlQueryGeneratorFactory
    {
        private readonly IParameterNameGeneratorFactory _parameterNameGeneratorFactory;
        private readonly IRelationalCommandBuilderFactory _commandBuilderFactory;

        public SqlServerQuerySqlGeneratorFactory(
            [NotNull] IParameterNameGeneratorFactory parameterNameGeneratorFactory,
            [NotNull] IRelationalCommandBuilderFactory commandBuilderFactory)
        {
            Check.NotNull(parameterNameGeneratorFactory, nameof(parameterNameGeneratorFactory));
            Check.NotNull(commandBuilderFactory, nameof(commandBuilderFactory));

            _parameterNameGeneratorFactory = parameterNameGeneratorFactory;
            _commandBuilderFactory = commandBuilderFactory;
        }

        public virtual ISqlQueryGenerator CreateGenerator(SelectExpression selectExpression)
            => new SqlServerQuerySqlGenerator(
                _parameterNameGeneratorFactory,
                _commandBuilderFactory,
                Check.NotNull(selectExpression, nameof(selectExpression)));

        public virtual ISqlQueryGenerator CreateRawCommandGenerator(
            SelectExpression selectExpression,
            string sql,
            object[] parameters)
            => new RawSqlQueryGenerator(
                _parameterNameGeneratorFactory,
                _commandBuilderFactory,
                Check.NotNull(selectExpression, nameof(selectExpression)),
                Check.NotNull(sql, nameof(sql)),
                Check.NotNull(parameters, nameof(parameters)));
    }
}

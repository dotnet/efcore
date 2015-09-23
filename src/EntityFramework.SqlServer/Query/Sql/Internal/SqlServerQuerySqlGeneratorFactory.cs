// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.Expressions;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query.Sql.Internal
{
    public class SqlServerQuerySqlGeneratorFactory : ISqlQueryGeneratorFactory
    {
        private readonly IRelationalCommandBuilderFactory _commandBuilderFactory;
        private readonly IParameterNameGeneratorFactory _parameterNameGeneratorFactory;

        public SqlServerQuerySqlGeneratorFactory(
            [NotNull] IRelationalCommandBuilderFactory commandBuilderFactory,
            [NotNull] IParameterNameGeneratorFactory parameterNameGeneratorFactory)
        {
            Check.NotNull(commandBuilderFactory, nameof(commandBuilderFactory));
            Check.NotNull(parameterNameGeneratorFactory, nameof(parameterNameGeneratorFactory));

            _commandBuilderFactory = commandBuilderFactory;
            _parameterNameGeneratorFactory = parameterNameGeneratorFactory;
        }

        public virtual ISqlQueryGenerator CreateGenerator(SelectExpression selectExpression)
            => new SqlServerQuerySqlGenerator(
                _commandBuilderFactory,
                _parameterNameGeneratorFactory,
                Check.NotNull(selectExpression, nameof(selectExpression)));

        public virtual ISqlQueryGenerator CreateRawCommandGenerator(
            SelectExpression selectExpression,
            string sql,
            object[] parameters)
            => new RawSqlQueryGenerator(
                _commandBuilderFactory,
                _parameterNameGeneratorFactory,
                Check.NotNull(selectExpression, nameof(selectExpression)),
                Check.NotNull(sql, nameof(sql)),
                Check.NotNull(parameters, nameof(parameters)));
    }
}

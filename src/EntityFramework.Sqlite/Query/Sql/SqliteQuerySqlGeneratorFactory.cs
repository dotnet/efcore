// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.Expressions;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query.Sql
{
    public class SqliteQuerySqlGeneratorFactory : ISqlQueryGeneratorFactory
    {
        private readonly IParameterNameGeneratorFactory _parameterNameGeneratorFactory;
        private readonly IRelationalCommandBuilderFactory _commandBuilderFactory;

        public SqliteQuerySqlGeneratorFactory(
            [NotNull] IParameterNameGeneratorFactory parameterNameGeneratorFactory,
            [NotNull] IRelationalCommandBuilderFactory commandBuilderFactory)
        {
            Check.NotNull(parameterNameGeneratorFactory, nameof(parameterNameGeneratorFactory));
            Check.NotNull(commandBuilderFactory, nameof(commandBuilderFactory));

            _parameterNameGeneratorFactory = parameterNameGeneratorFactory;
            _commandBuilderFactory = commandBuilderFactory;
        }

        public virtual ISqlQueryGenerator CreateGenerator([NotNull] SelectExpression selectExpression)
            => new SqliteQuerySqlGenerator(
                _parameterNameGeneratorFactory,
                _commandBuilderFactory,
                Check.NotNull(selectExpression, nameof(selectExpression)));

        public virtual ISqlQueryGenerator CreateRawCommandGenerator(
            [NotNull] SelectExpression selectExpression,
            [NotNull] string sql,
            [NotNull] object[] parameters)
            => new RawSqlQueryGenerator(
                _parameterNameGeneratorFactory,
                _commandBuilderFactory,
                Check.NotNull(selectExpression, nameof(selectExpression)),
                Check.NotNull(sql, nameof(sql)),
                Check.NotNull(parameters, nameof(parameters)));
    }
}

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
        private readonly ISqlGenerator _sqlGenerator;
        private readonly IParameterNameGeneratorFactory _parameterNameGeneratorFactory;
        private readonly ISqlCommandBuilder _sqlCommandBuilder;

        public SqlServerQuerySqlGeneratorFactory(
            [NotNull] IRelationalCommandBuilderFactory commandBuilderFactory,
            [NotNull] ISqlGenerator sqlGenerator,
            [NotNull] IParameterNameGeneratorFactory parameterNameGeneratorFactory,
            [NotNull] ISqlCommandBuilder sqlCommandBuilder)
        {
            Check.NotNull(commandBuilderFactory, nameof(commandBuilderFactory));
            Check.NotNull(sqlGenerator, nameof(sqlGenerator));
            Check.NotNull(parameterNameGeneratorFactory, nameof(parameterNameGeneratorFactory));
            Check.NotNull(sqlCommandBuilder, nameof(sqlCommandBuilder));

            _commandBuilderFactory = commandBuilderFactory;
            _sqlGenerator = sqlGenerator;
            _parameterNameGeneratorFactory = parameterNameGeneratorFactory;
            _sqlCommandBuilder = sqlCommandBuilder;
        }

        public virtual ISqlQueryGenerator CreateGenerator(SelectExpression selectExpression)
            => new SqlServerQuerySqlGenerator(
                _commandBuilderFactory,
                _sqlGenerator,
                _parameterNameGeneratorFactory,
                Check.NotNull(selectExpression, nameof(selectExpression)));

        public virtual ISqlQueryGenerator CreateRawCommandGenerator(
            SelectExpression selectExpression,
            string sql,
            object[] parameters)
            => new RawSqlQueryGenerator(
                _sqlCommandBuilder,
                Check.NotNull(selectExpression, nameof(selectExpression)),
                Check.NotNull(sql, nameof(sql)),
                Check.NotNull(parameters, nameof(parameters)));
    }
}

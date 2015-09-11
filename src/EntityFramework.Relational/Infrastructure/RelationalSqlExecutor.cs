// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Infrastructure
{
    public class RelationalSqlExecutor
    {
        private readonly IParameterNameGeneratorFactory _parameterNameGeneratorFactory;
        private readonly ISqlStatementExecutor _statementExecutor;
        private readonly IRelationalConnection _connection;
        private readonly IRelationalTypeMapper _typeMapper;

        public RelationalSqlExecutor(
            [NotNull] IParameterNameGeneratorFactory parameterNameGeneratorFactory,
            [NotNull] ISqlStatementExecutor statementExecutor,
            [NotNull] IRelationalConnection connection,
            [NotNull] IRelationalTypeMapper typeMapper)
        {
            Check.NotNull(parameterNameGeneratorFactory, nameof(parameterNameGeneratorFactory));
            Check.NotNull(statementExecutor, nameof(statementExecutor));
            Check.NotNull(connection, nameof(connection));
            Check.NotNull(typeMapper, nameof(typeMapper));

            _parameterNameGeneratorFactory = parameterNameGeneratorFactory;
            _statementExecutor = statementExecutor;
            _connection = connection;
            _typeMapper = typeMapper;
        }

        public virtual void ExecuteSqlCommand([NotNull] string sql, [NotNull] params object[] parameters)
        {
            Check.NotNull(sql, nameof(sql));
            Check.NotNull(parameters, nameof(parameters));

            var builder = new RelationalCommandBuilder();

            var parameterNameGenerator = _parameterNameGeneratorFactory.Create();

            var substitutions = new string[parameters.Length];

            for (var index = 0; index < substitutions.Length; index++)
            {
                substitutions[index] = parameterNameGenerator.GenerateNext();
                builder.RelationalParameterList.GetOrAdd(
                    substitutions[index],
                    parameters[index]);
            }

            builder.AppendLines(string.Format(sql, substitutions));

            _statementExecutor.ExecuteNonQuery(_connection, new[] { builder.RelationalCommand } );
        }
    }
}

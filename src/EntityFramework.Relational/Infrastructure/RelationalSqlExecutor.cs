// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Infrastructure
{
    public class RelationalSqlExecutor
    {
        private readonly IRelationalCommandBuilderFactory _relationalCommandBuilderFactory;
        private readonly IParameterNameGeneratorFactory _parameterNameGeneratorFactory;
        private readonly ISqlStatementExecutor _statementExecutor;
        private readonly IRelationalConnection _connection;

        public RelationalSqlExecutor(
            [NotNull] IRelationalCommandBuilderFactory relationalCommandBuilderFactory,
            [NotNull] IParameterNameGeneratorFactory parameterNameGeneratorFactory,
            [NotNull] ISqlStatementExecutor statementExecutor,
            [NotNull] IRelationalConnection connection,
            [NotNull] IRelationalTypeMapper typeMapper)
        {
            Check.NotNull(relationalCommandBuilderFactory, nameof(relationalCommandBuilderFactory));
            Check.NotNull(parameterNameGeneratorFactory, nameof(parameterNameGeneratorFactory));
            Check.NotNull(statementExecutor, nameof(statementExecutor));
            Check.NotNull(connection, nameof(connection));
            Check.NotNull(typeMapper, nameof(typeMapper));

            _relationalCommandBuilderFactory = relationalCommandBuilderFactory;
            _parameterNameGeneratorFactory = parameterNameGeneratorFactory;
            _statementExecutor = statementExecutor;
            _connection = connection;
        }

        public virtual void ExecuteSqlCommand([NotNull] string sql, [NotNull] params object[] parameters)
        {
            Check.NotNull(sql, nameof(sql));
            Check.NotNull(parameters, nameof(parameters));

            _statementExecutor.ExecuteNonQuery(
                _connection,
                new[] { CreateCommand(sql, parameters) });
        }

        public virtual async Task ExecuteSqlCommandAsync(
            [NotNull] string sql,
            CancellationToken cancellationToken = default(CancellationToken),
            [NotNull] params object[] parameters)
        {
            Check.NotNull(sql, nameof(sql));
            Check.NotNull(parameters, nameof(parameters));

            await _statementExecutor.ExecuteNonQueryAsync(
                _connection,
                new[] { CreateCommand(sql, parameters) },
                cancellationToken);
        }

        private RelationalCommand CreateCommand(
            string sql,
            object[] parameters)
        {
            var builder = _relationalCommandBuilderFactory.Create();

            var parameterNameGenerator = _parameterNameGeneratorFactory.Create();

            var substitutions = new string[parameters.Length];

            for (var index = 0; index < substitutions.Length; index++)
            {
                substitutions[index] = parameterNameGenerator.GenerateNext();
                builder.AddParameter(
                    substitutions[index],
                    parameters[index]);
            }

            builder.AppendLines(string.Format(sql, substitutions));

            return builder.BuildRelationalCommand();
        }
    }
}

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
        private readonly IParameterNameGeneratorFactory _parameterNameGeneratorFactory;
        private readonly IRelationalCommandBuilderFactory _commandBuilderFactory;
        private readonly IRelationalConnection _connection;

        public RelationalSqlExecutor(
            [NotNull] IParameterNameGeneratorFactory parameterNameGeneratorFactory,
            [NotNull] IRelationalCommandBuilderFactory commandBuilderFactory,
            [NotNull] IRelationalConnection connection)
        {
            Check.NotNull(parameterNameGeneratorFactory, nameof(parameterNameGeneratorFactory));
            Check.NotNull(commandBuilderFactory, nameof(commandBuilderFactory));
            Check.NotNull(connection, nameof(connection));

            _commandBuilderFactory = commandBuilderFactory;
            _parameterNameGeneratorFactory = parameterNameGeneratorFactory;
            _connection = connection;
        }

        public virtual void ExecuteSqlCommand(
            [NotNull] string sql,
            [NotNull] params object[] parameters)
            => CreateCommand(
                Check.NotNull(sql, nameof(sql)),
                Check.NotNull(parameters, nameof(parameters)))
                    .ExecuteNonQuery(_connection);

        public virtual Task ExecuteSqlCommandAsync(
            [NotNull] string sql,
            CancellationToken cancellationToken = default(CancellationToken),
            [NotNull] params object[] parameters)
            => CreateCommand(
                Check.NotNull(sql, nameof(sql)),
                Check.NotNull(parameters, nameof(parameters)))
                    .ExecuteNonQueryAsync(_connection, cancellationToken);

        private IRelationalCommand CreateCommand(string sql, object[] parameters)
        {
            var builder = _commandBuilderFactory.Create();

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

            return builder.BuildRelationalCommand();
        }
    }
}

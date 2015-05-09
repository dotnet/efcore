// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Query.Sql;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Query
{
    public class CommandBuilder
    {
        private readonly IRelationalValueBufferFactoryFactory _valueBufferFactoryFactory;
        private readonly ISqlQueryGenerator _sqlGenerator;

        private IRelationalValueBufferFactory _valueBufferFactory;

        public CommandBuilder(
            [NotNull] ISqlQueryGenerator sqlGenerator,
            [NotNull] IRelationalValueBufferFactoryFactory valueBufferFactoryFactory)
        {
            Check.NotNull(sqlGenerator, nameof(sqlGenerator));
            Check.NotNull(valueBufferFactoryFactory, nameof(valueBufferFactoryFactory));

            _sqlGenerator = sqlGenerator;
            _valueBufferFactoryFactory = valueBufferFactoryFactory;
        }

        public virtual IRelationalValueBufferFactory ValueBufferFactory => _valueBufferFactory;

        public virtual DbCommand Build(
            [NotNull] IRelationalConnection connection,
            [NotNull] IDictionary<string, object> parameterValues)
        {
            Check.NotNull(connection, nameof(connection));

            // TODO: Cache command...

            var command = connection.DbConnection.CreateCommand();

            if (connection.Transaction != null)
            {
                command.Transaction = connection.Transaction.DbTransaction;
            }

            if (connection.CommandTimeout != null)
            {
                command.CommandTimeout = (int)connection.CommandTimeout;
            }

            command.CommandText = _sqlGenerator.GenerateSql(parameterValues);

            foreach (var commandParameter in _sqlGenerator.Parameters)
            {
                var parameter = command.CreateParameter();

                parameter.ParameterName = commandParameter.Name;
                parameter.Value = commandParameter.Value;

                // TODO: Parameter facets?

                command.Parameters.Add(parameter);
            }

            return command;
        }

        public virtual void NotifyReaderCreated([NotNull] DbDataReader dataReader)
        {
            Check.NotNull(dataReader, nameof(dataReader));

            LazyInitializer
                .EnsureInitialized(
                    ref _valueBufferFactory,
                    () => _sqlGenerator.CreateValueBufferFactory(_valueBufferFactoryFactory, dataReader));
        }
    }
}

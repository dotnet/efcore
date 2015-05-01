// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Query.Sql;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Query
{
    public class CommandBuilder
    {
        private readonly ISqlQueryGenerator _sqlGenerator;

        public CommandBuilder([NotNull] ISqlQueryGenerator sqlGenerator)
        {
            Check.NotNull(sqlGenerator, nameof(sqlGenerator));

            _sqlGenerator = sqlGenerator;
        }

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
    }
}

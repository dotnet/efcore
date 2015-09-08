// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Infrastructure
{
    public class SqlBatch
    {
        public SqlBatch([NotNull] string sql, [NotNull] params CommandParameter[] commandParameters)
        {
            Check.NotNull(sql, nameof(sql));
            Check.NotNull(commandParameters, nameof(commandParameters));

            Sql = sql;
            CommandParameters = commandParameters;
        }

        public virtual string Sql { get; }

        public virtual IReadOnlyList<CommandParameter> CommandParameters { get; }

        public virtual bool SuppressTransaction { get; set; }

        public virtual DbCommand CreateCommand(
            [NotNull] IRelationalConnection connection,
            [CanBeNull] DbTransaction transaction)
        {
            Check.NotNull(connection, nameof(connection));

            var command = connection.DbConnection.CreateCommand();
            command.CommandText = Sql;
            command.Transaction = transaction;

            if (connection.CommandTimeout != null)
            {
                command.CommandTimeout = (int)connection.CommandTimeout;
            }

            foreach(var commandParameter in CommandParameters)
            {
                command.Parameters.Add(
                    commandParameter.TypeMapping.CreateParameter(
                        command,
                        commandParameter.Name,
                        commandParameter.Value));
            }

            return command;
        }
    }
}

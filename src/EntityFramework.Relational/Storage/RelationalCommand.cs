// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Storage
{
    public class RelationalCommand
    {
        public RelationalCommand(
            [NotNull] string commandText,
            [NotNull] params RelationalParameter[] parameters)
        {
            Check.NotNull(commandText, nameof(commandText));
            Check.NotNull(parameters, nameof(parameters));

            CommandText = commandText;
            Parameters = parameters;
        }

        public virtual string CommandText { get; }

        public virtual IReadOnlyList<RelationalParameter> Parameters { get; }

        public virtual DbCommand CreateDbCommand(
            [NotNull] IRelationalConnection connection,
            [NotNull] IRelationalTypeMapper typeMapper)
        {
            Check.NotNull(connection, nameof(connection));
            Check.NotNull(typeMapper, nameof(typeMapper));

            var command = connection.DbConnection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = CommandText;

            if (connection.Transaction != null)
            {
                command.Transaction = connection.Transaction.GetService();
            }

            if (connection.CommandTimeout != null)
            {
                command.CommandTimeout = (int)connection.CommandTimeout;
            }

            foreach (var parameter in Parameters)
            {
                command.Parameters.Add(
                    parameter.CreateDbParameter(command, typeMapper));
            }

            return command;
        }
    }
}

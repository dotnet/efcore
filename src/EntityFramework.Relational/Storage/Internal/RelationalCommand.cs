// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Storage.Internal
{
    public class RelationalCommand : IRelationalCommand
    {
        public RelationalCommand(
            [NotNull] string commandText,
            [NotNull] IReadOnlyList<RelationalParameter> parameters)
        {
            Check.NotNull(commandText, nameof(commandText));
            Check.NotNull(parameters, nameof(parameters));

            CommandText = commandText;
            Parameters = parameters;
        }

        public virtual string CommandText { get; }

        public virtual IReadOnlyList<RelationalParameter> Parameters { get; }

        public virtual DbCommand CreateCommand([NotNull] IRelationalConnection connection)
        {
            Check.NotNull(connection, nameof(connection));

            var command = connection.DbConnection.CreateCommand();
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
                    parameter.RelationalTypeMapping.CreateParameter(
                        command,
                        parameter.Name,
                        parameter.Value,
                        parameter.Nullable));
            }

            return command;
        }
    }
}

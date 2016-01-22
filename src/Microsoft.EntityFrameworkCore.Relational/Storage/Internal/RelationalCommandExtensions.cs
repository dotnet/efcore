// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public static class RelationalCommandExtensions
    {
        public static void ExecuteNonQuery(
            [NotNull] this IEnumerable<IRelationalCommand> commands,
            [NotNull] IRelationalConnection connection)
        {
            Check.NotNull(commands, nameof(commands));
            Check.NotNull(connection, nameof(connection));

            connection.Open();

            try
            {
                foreach (var command in commands)
                {
                    command.ExecuteNonQuery(
                        connection,
                        manageConnection: false);
                }
            }
            finally
            {
                connection.Close();
            }
        }

        public static async Task ExecuteNonQueryAsync(
            [NotNull] this IEnumerable<IRelationalCommand> commands,
            [NotNull] IRelationalConnection connection,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(commands, nameof(commands));
            Check.NotNull(connection, nameof(connection));

            await connection.OpenAsync(cancellationToken);

            try
            {
                foreach (var command in commands)
                {
                    await command.ExecuteNonQueryAsync(
                        connection,
                        manageConnection: false,
                        cancellationToken: cancellationToken);
                }
            }
            finally
            {
                connection.Close();
            }
        }
    }
}

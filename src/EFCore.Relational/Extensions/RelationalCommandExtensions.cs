// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     Extension methods typically used by internal code and database providers to execute
    ///     commands on the low-level <see cref="IRelationalCommand" /> abstraction.
    /// </summary>
    public static class RelationalCommandExtensions
    {
        /// <summary>
        ///     Executes the command with no results.
        /// </summary>
        /// <param name="command"> The command to be executed. </param>
        /// <param name="connection"> The connection to execute against. </param>
        /// <returns> The number of rows affected. </returns>
        public static int ExecuteNonQuery(
            [NotNull] this IRelationalCommand command,
            [NotNull] IRelationalConnection connection)
            => command.ExecuteNonQuery(connection, parameterValues: null);

        /// <summary>
        ///     Asynchronously executes the command with no results.
        /// </summary>
        /// <param name="command"> The command to be executed. </param>
        /// <param name="connection"> The connection to execute against. </param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous operation. The task result contains the number of rows affected.
        /// </returns>
        public static Task<int> ExecuteNonQueryAsync(
            [NotNull] this IRelationalCommand command,
            [NotNull] IRelationalConnection connection,
            CancellationToken cancellationToken = default)
            => command.ExecuteNonQueryAsync(connection, parameterValues: null, cancellationToken: cancellationToken);

        /// <summary>
        ///     Executes the command with a single scalar result.
        /// </summary>
        /// <param name="command"> The command to be executed. </param>
        /// <param name="connection"> The connection to execute against. </param>
        /// <returns> The result of the command. </returns>
        public static object ExecuteScalar(
            [NotNull] this IRelationalCommand command,
            [NotNull] IRelationalConnection connection)
            => command.ExecuteScalar(connection, parameterValues: null);

        /// <summary>
        ///     Asynchronously executes the command with a single scalar result.
        /// </summary>
        /// <param name="command"> The command to be executed. </param>
        /// <param name="connection"> The connection to execute against. </param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous operation. The task result contains the result of the command.
        /// </returns>
        public static Task<object> ExecuteScalarAsync(
            [NotNull] this IRelationalCommand command,
            [NotNull] IRelationalConnection connection,
            CancellationToken cancellationToken = default)
            => command.ExecuteScalarAsync(connection, parameterValues: null, cancellationToken: cancellationToken);

        /// <summary>
        ///     Executes the command with a <see cref="RelationalDataReader" /> result.
        /// </summary>
        /// <param name="command"> The command to be executed. </param>
        /// <param name="connection"> The connection to execute against. </param>
        /// <returns> The result of the command. </returns>
        public static RelationalDataReader ExecuteReader(
            [NotNull] this IRelationalCommand command,
            [NotNull] IRelationalConnection connection)
            => command.ExecuteReader(connection, parameterValues: null);

        /// <summary>
        ///     Asynchronously executes the command with a <see cref="RelationalDataReader" /> result.
        /// </summary>
        /// <param name="command"> The command to be executed. </param>
        /// <param name="connection"> The connection to execute against. </param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous operation. The task result contains the result of the command.
        /// </returns>
        public static Task<RelationalDataReader> ExecuteReaderAsync(
            [NotNull] this IRelationalCommand command,
            [NotNull] IRelationalConnection connection,
            CancellationToken cancellationToken = default)
            => command.ExecuteReaderAsync(connection, parameterValues: null, cancellationToken: cancellationToken);
    }
}

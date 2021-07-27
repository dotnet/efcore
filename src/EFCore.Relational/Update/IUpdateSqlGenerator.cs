// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Update
{
    /// <summary>
    ///     <para>
    ///         A service used to generate SQL for insert, update, and delete commands, and related SQL
    ///         operations needed for <see cref="DbContext.SaveChanges()" />
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers; it is generally not used in application code.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public interface IUpdateSqlGenerator
    {
        /// <summary>
        ///     Generates SQL that will obtain the next value in the given sequence.
        /// </summary>
        /// <param name="name"> The name of the sequence. </param>
        /// <param name="schema"> The schema that contains the sequence, or <see langword="null" /> to use the default schema. </param>
        /// <returns> The SQL. </returns>
        string GenerateNextSequenceValueOperation(string name, string? schema);

        /// <summary>
        ///     Generates a SQL fragment that will get the next value from the given sequence and appends it to
        ///     the full command being built by the given <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="commandStringBuilder"> The builder to which the SQL fragment should be appended. </param>
        /// <param name="name"> The name of the sequence. </param>
        /// <param name="schema"> The schema that contains the sequence, or <see langword="null" /> to use the default schema. </param>
        void AppendNextSequenceValueOperation(
            StringBuilder commandStringBuilder,
            string name,
            string? schema);

        /// <summary>
        ///     Appends a SQL fragment for the start of a batch to
        ///     the full command being built by the given <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="commandStringBuilder"> The builder to which the SQL fragment should be appended. </param>
        void AppendBatchHeader(StringBuilder commandStringBuilder);

        /// <summary>
        ///     Appends a SQL command for deleting a row to the commands being built.
        /// </summary>
        /// <param name="commandStringBuilder"> The builder to which the SQL should be appended. </param>
        /// <param name="command"> The command that represents the delete operation. </param>
        /// <param name="commandPosition"> The ordinal of this command in the batch. </param>
        /// <returns> The <see cref="ResultSetMapping" /> for the command. </returns>
        ResultSetMapping AppendDeleteOperation(
            StringBuilder commandStringBuilder,
            IModificationCommand command,
            int commandPosition);

        /// <summary>
        ///     Appends a SQL command for inserting a row to the commands being built.
        /// </summary>
        /// <param name="commandStringBuilder"> The builder to which the SQL should be appended. </param>
        /// <param name="command"> The command that represents the delete operation. </param>
        /// <param name="commandPosition"> The ordinal of this command in the batch. </param>
        /// <returns> The <see cref="ResultSetMapping" /> for the command. </returns>
        ResultSetMapping AppendInsertOperation(
            StringBuilder commandStringBuilder,
            IModificationCommand command,
            int commandPosition);

        /// <summary>
        ///     Appends a SQL command for updating a row to the commands being built.
        /// </summary>
        /// <param name="commandStringBuilder"> The builder to which the SQL should be appended. </param>
        /// <param name="command"> The command that represents the delete operation. </param>
        /// <param name="commandPosition"> The ordinal of this command in the batch. </param>
        /// <returns> The <see cref="ResultSetMapping" /> for the command. </returns>
        ResultSetMapping AppendUpdateOperation(
            StringBuilder commandStringBuilder,
            IModificationCommand command,
            int commandPosition);
    }
}

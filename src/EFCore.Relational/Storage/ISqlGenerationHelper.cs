// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Provides services to help with generation of SQL commands.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public interface ISqlGenerationHelper
    {
        /// <summary>
        ///     The terminator to be used for SQL statements.
        /// </summary>
        string StatementTerminator { get; }

        /// <summary>
        ///     The terminator to be used for batches of SQL statements.
        /// </summary>
        string BatchTerminator { get; }

        /// <summary>
        ///     Gets the SQL for a START TRANSACTION statement.
        /// </summary>
        string StartTransactionStatement { get; }

        /// <summary>
        ///     Gets the SQL for a COMMIT statement.
        /// </summary>
        string CommitTransactionStatement { get; }

        /// <summary>
        ///     The default single-line comment prefix.
        /// </summary>
        string SingleLineCommentToken { get; }

        /// <summary>
        ///     Generates a valid parameter name for the given candidate name.
        /// </summary>
        /// <param name="name">
        ///     The candidate name for the parameter.
        /// </param>
        /// <returns> A valid name based on the candidate name. </returns>
        string GenerateParameterName([NotNull] string name);

        /// <summary>
        ///     Writes a valid parameter name for the given candidate name.
        /// </summary>
        /// <param name="builder"> The <see cref="StringBuilder" /> to write generated string to. </param>
        /// <param name="name">
        ///     The candidate name for the parameter.
        /// </param>
        void GenerateParameterName([NotNull] StringBuilder builder, [NotNull] string name);

        /// <summary>
        ///     Generates a valid parameter placeholder name for the given candidate name.
        /// </summary>
        /// <param name="name">
        ///     The candidate name for the parameter placeholder.
        /// </param>
        /// <returns> A valid placeholder name based on the candidate name. </returns>
        string GenerateParameterNamePlaceholder([NotNull] string name);

        /// <summary>
        ///     Writes a valid parameter placeholder name for the given candidate name.
        /// </summary>
        /// <param name="builder"> The <see cref="StringBuilder" /> to write generated string to. </param>
        /// <param name="name">
        ///     The candidate name for the parameter placeholder.
        /// </param>
        void GenerateParameterNamePlaceholder([NotNull] StringBuilder builder, [NotNull] string name);

        /// <summary>
        ///     Generates the delimited SQL representation of an identifier (column name, table name, etc.).
        /// </summary>
        /// <param name="identifier"> The identifier to delimit. </param>
        /// <returns> The generated string. </returns>
        string DelimitIdentifier([NotNull] string identifier);

        /// <summary>
        ///     Writes the delimited SQL representation of an identifier (column name, table name, etc.).
        /// </summary>
        /// <param name="builder"> The <see cref="StringBuilder" /> to write generated string to. </param>
        /// <param name="identifier"> The identifier to delimit. </param>
        void DelimitIdentifier([NotNull] StringBuilder builder, [NotNull] string identifier);

        /// <summary>
        ///     Generates the delimited SQL representation of an identifier (column name, table name, etc.).
        /// </summary>
        /// <param name="name"> The identifier to delimit. </param>
        /// <param name="schema"> The schema of the identifier. </param>
        /// <returns> The generated string. </returns>
        string DelimitIdentifier([NotNull] string name, [CanBeNull] string schema);

        /// <summary>
        ///     Writes the delimited SQL representation of an identifier (column name, table name, etc.).
        /// </summary>
        /// <param name="builder"> The <see cref="StringBuilder" /> to write generated string to. </param>
        /// <param name="name"> The identifier to delimit. </param>
        /// <param name="schema"> The schema of the identifier. </param>
        void DelimitIdentifier([NotNull] StringBuilder builder, [NotNull] string name, [CanBeNull] string schema);

        /// <summary>
        ///     Generates a SQL comment.
        /// </summary>
        /// <param name="text"> The comment text. </param>
        /// <returns> The generated SQL. </returns>
        string GenerateComment([NotNull] string text);
    }
}

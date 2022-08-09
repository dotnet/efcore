// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     <para>
///         Provides services to help with generation of SQL commands.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///         for more information and examples.
///     </para>
/// </remarks>
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
    /// <returns>A valid name based on the candidate name.</returns>
    string GenerateParameterName(string name);

    /// <summary>
    ///     Writes a valid parameter name for the given candidate name.
    /// </summary>
    /// <param name="builder">The <see cref="StringBuilder" /> to write generated string to.</param>
    /// <param name="name">
    ///     The candidate name for the parameter.
    /// </param>
    void GenerateParameterName(StringBuilder builder, string name);

    /// <summary>
    ///     Generates a valid parameter placeholder name for the given candidate name.
    /// </summary>
    /// <param name="name">
    ///     The candidate name for the parameter placeholder.
    /// </param>
    /// <returns>A valid placeholder name based on the candidate name.</returns>
    string GenerateParameterNamePlaceholder(string name);

    /// <summary>
    ///     Writes a valid parameter placeholder name for the given candidate name.
    /// </summary>
    /// <param name="builder">The <see cref="StringBuilder" /> to write generated string to.</param>
    /// <param name="name">
    ///     The candidate name for the parameter placeholder.
    /// </param>
    void GenerateParameterNamePlaceholder(StringBuilder builder, string name);

    /// <summary>
    ///     Generates the delimited SQL representation of an identifier (column name, table name, etc.).
    /// </summary>
    /// <param name="identifier">The identifier to delimit.</param>
    /// <returns>The generated string.</returns>
    string DelimitIdentifier(string identifier);

    /// <summary>
    ///     Writes the delimited SQL representation of an identifier (column name, table name, etc.).
    /// </summary>
    /// <param name="builder">The <see cref="StringBuilder" /> to write generated string to.</param>
    /// <param name="identifier">The identifier to delimit.</param>
    void DelimitIdentifier(StringBuilder builder, string identifier);

    /// <summary>
    ///     Generates the delimited SQL representation of an identifier (column name, table name, etc.).
    /// </summary>
    /// <param name="name">The identifier to delimit.</param>
    /// <param name="schema">The schema of the identifier.</param>
    /// <returns>The generated string.</returns>
    string DelimitIdentifier(string name, string? schema);

    /// <summary>
    ///     Writes the delimited SQL representation of an identifier (column name, table name, etc.).
    /// </summary>
    /// <param name="builder">The <see cref="StringBuilder" /> to write generated string to.</param>
    /// <param name="name">The identifier to delimit.</param>
    /// <param name="schema">The schema of the identifier.</param>
    void DelimitIdentifier(StringBuilder builder, string name, string? schema);

    /// <summary>
    ///     Generates a SQL comment.
    /// </summary>
    /// <param name="text">The comment text.</param>
    /// <returns>The generated SQL.</returns>
    string GenerateComment(string text);

    /// <summary>
    ///     Generates an SQL statement which creates a savepoint with the given name.
    /// </summary>
    /// <param name="name">The name of the savepoint to be created.</param>
    /// <returns>An SQL string to create the savepoint.</returns>
    string GenerateCreateSavepointStatement(string name);

    /// <summary>
    ///     Generates an SQL statement which rolls back to a savepoint with the given name.
    /// </summary>
    /// <param name="name">The name of the savepoint to be rolled back to.</param>
    /// <returns>An SQL string to roll back the savepoint.</returns>
    string GenerateRollbackToSavepointStatement(string name);

    /// <summary>
    ///     Generates an SQL statement which releases a savepoint with the given name.
    /// </summary>
    /// <param name="name">The name of the savepoint to be released.</param>
    /// <returns>An SQL string to release the savepoint.</returns>
    string GenerateReleaseSavepointStatement(string name);
}

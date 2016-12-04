// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;
using JetBrains.Annotations;

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
        ///     Generates the SQL representation of a literal value.
        /// </summary>
        /// <param name="value"> The literal value. </param>
        /// <param name="typeMapping"> An optional type mapping that is used for this value. </param>
        /// <returns> The generated string. </returns>
        string GenerateLiteral([CanBeNull] object value, [CanBeNull] RelationalTypeMapping typeMapping = null);

        /// <summary>
        ///     Writes the SQL representation of a literal value.
        /// </summary>
        /// <param name="builder"> The <see cref="StringBuilder" /> to write generated string to. </param>
        /// <param name="value"> The literal value. </param>
        /// <param name="typeMapping"> An optional type mapping that is used for this value. </param>
        void GenerateLiteral([NotNull] StringBuilder builder, [CanBeNull] object value, [CanBeNull] RelationalTypeMapping typeMapping = null);

        /// <summary>
        ///     Generates the escaped SQL representation of a literal value.
        /// </summary>
        /// <param name="literal"> The value to be escaped. </param>
        /// <returns> The generated string. </returns>
        string EscapeLiteral([NotNull] string literal);

        /// <summary>
        ///     Writes the escaped SQL representation of a literal value.
        /// </summary>
        /// <param name="builder"> The <see cref="StringBuilder" /> to write generated string to. </param>
        /// <param name="literal"> The value to be escaped. </param>
        void EscapeLiteral([NotNull] StringBuilder builder, [NotNull] string literal);

        /// <summary>
        ///     Generates the escaped SQL representation of an identifier (column name, table name, etc.).
        /// </summary>
        /// <param name="identifier"> The identifier to be escaped. </param>
        /// <returns> The generated string. </returns>
        string EscapeIdentifier([NotNull] string identifier);

        /// <summary>
        ///     Writes the escaped SQL representation of an identifier (column name, table name, etc.).
        /// </summary>
        /// <param name="builder"> The <see cref="StringBuilder" /> to write generated string to. </param>
        /// <param name="identifier"> The identifier to be escaped. </param>
        void EscapeIdentifier([NotNull] StringBuilder builder, [NotNull] string identifier);

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
    }
}

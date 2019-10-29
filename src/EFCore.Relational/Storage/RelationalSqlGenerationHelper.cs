// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
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
    public class RelationalSqlGenerationHelper : ISqlGenerationHelper
    {
        /// <summary>
        ///     Initializes a new instance of the this class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        public RelationalSqlGenerationHelper([NotNull] RelationalSqlGenerationHelperDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));
        }

        /// <summary>
        ///     The terminator to be used for SQL statements.
        /// </summary>
        public virtual string StatementTerminator => ";";

        /// <summary>
        ///     The terminator to be used for batches of SQL statements.
        /// </summary>
        public virtual string BatchTerminator => string.Empty;

        /// <summary>
        ///     The default single-line comment prefix.
        /// </summary>
        public virtual string SingleLineCommentToken => "--";

        /// <summary>
        ///     Generates a valid parameter name for the given candidate name.
        /// </summary>
        /// <param name="name">The candidate name for the parameter.</param>
        /// <returns>
        ///     A valid name based on the candidate name.
        /// </returns>
        public virtual string GenerateParameterName(string name)
            => name.StartsWith("@")
                ? name
                : "@" + name;

        /// <summary>
        ///     Writes a valid parameter name for the given candidate name.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder" /> to write generated string to.</param>
        /// <param name="name">The candidate name for the parameter.</param>
        public virtual void GenerateParameterName(StringBuilder builder, string name)
            => builder.Append("@").Append(name);

        /// <summary>
        ///     Generates a valid parameter placeholder name for the given candidate name.
        /// </summary>
        /// <param name="name">The candidate name for the parameter placeholder.</param>
        /// <returns>
        ///     A valid name based on the candidate name.
        /// </returns>
        public virtual string GenerateParameterNamePlaceholder(string name)
            => GenerateParameterName(name);

        /// <summary>
        ///     Writes a valid parameter placeholder name for the given candidate name.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder" /> to write generated string to.</param>
        /// <param name="name">The candidate name for the parameter placeholder.</param>
        public virtual void GenerateParameterNamePlaceholder(StringBuilder builder, string name)
            => GenerateParameterName(builder, name);

        /// <summary>
        ///     Generates the escaped SQL representation of an identifier (column name, table name, etc.).
        /// </summary>
        /// <param name="identifier">The identifier to be escaped.</param>
        /// <returns>
        ///     The generated string.
        /// </returns>
        public virtual string EscapeIdentifier([NotNull] string identifier)
            => Check.NotEmpty(identifier, nameof(identifier)).Replace("\"", "\"\"");

        /// <summary>
        ///     Writes the escaped SQL representation of an identifier (column name, table name, etc.).
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder" /> to write generated string to.</param>
        /// <param name="identifier">The identifier to be escaped.</param>
        public virtual void EscapeIdentifier([NotNull] StringBuilder builder, [NotNull] string identifier)
        {
            Check.NotEmpty(identifier, nameof(identifier));

            var initialLength = builder.Length;
            builder.Append(identifier);
            builder.Replace("\"", "\"\"", initialLength, identifier.Length);
        }

        /// <summary>
        ///     Generates the delimited SQL representation of an identifier (column name, table name, etc.).
        /// </summary>
        /// <param name="identifier">The identifier to delimit.</param>
        /// <returns>
        ///     The generated string.
        /// </returns>
        public virtual string DelimitIdentifier(string identifier)
            => $"\"{EscapeIdentifier(Check.NotEmpty(identifier, nameof(identifier)))}\""; // Interpolation okay; strings

        /// <summary>
        ///     Writes the delimited SQL representation of an identifier (column name, table name, etc.).
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder" /> to write generated string to.</param>
        /// <param name="identifier">The identifier to delimit.</param>
        public virtual void DelimitIdentifier(StringBuilder builder, string identifier)
        {
            Check.NotEmpty(identifier, nameof(identifier));

            builder.Append('"');
            EscapeIdentifier(builder, identifier);
            builder.Append('"');
        }

        /// <summary>
        ///     Generates the delimited SQL representation of an identifier (column name, table name, etc.).
        /// </summary>
        /// <param name="name">The identifier to delimit.</param>
        /// <param name="schema">The schema of the identifier.</param>
        /// <returns>
        ///     The generated string.
        /// </returns>
        public virtual string DelimitIdentifier(string name, string schema)
            => (!string.IsNullOrEmpty(schema)
                    ? DelimitIdentifier(schema) + "."
                    : string.Empty)
                + DelimitIdentifier(Check.NotEmpty(name, nameof(name)));

        /// <summary>
        ///     Writes the delimited SQL representation of an identifier (column name, table name, etc.).
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder" /> to write generated string to.</param>
        /// <param name="name">The identifier to delimit.</param>
        /// <param name="schema">The schema of the identifier.</param>
        public virtual void DelimitIdentifier(StringBuilder builder, string name, string schema)
        {
            if (!string.IsNullOrEmpty(schema))
            {
                DelimitIdentifier(builder, schema);
                builder.Append(".");
            }

            DelimitIdentifier(builder, name);
        }

        /// <summary>
        ///     Generates a SQL comment.
        /// </summary>
        /// <param name="text"> The comment text. </param>
        /// <returns> The generated SQL. </returns>
        public virtual string GenerateComment(string text)
        {
            Check.NotEmpty(text, nameof(text));

            var builder = new StringBuilder();
            using (var reader = new StringReader(text))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    builder.Append(SingleLineCommentToken).Append(" ").AppendLine(line);
                }
            }

            return builder.ToString();
        }
    }
}

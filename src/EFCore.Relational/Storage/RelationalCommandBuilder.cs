// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Builds a command to be executed against a relational database.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
    ///     for more information.
    /// </remarks>
    public class RelationalCommandBuilder : IRelationalCommandBuilder
    {
        private readonly List<IRelationalParameter> _parameters = new();
        private readonly IndentedStringBuilder _commandTextBuilder = new();

        /// <summary>
        ///     <para>
        ///         Constructs a new <see cref="RelationalCommand" />.
        ///     </para>
        ///     <para>
        ///         This type is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="dependencies">Parameter object containing dependencies for this service.</param>
        public RelationalCommandBuilder(
            RelationalCommandBuilderDependencies dependencies)
        {
            Dependencies = dependencies;
        }

        /// <summary>
        ///     Relational provider-specific dependencies for this service.
        /// </summary>
        protected virtual RelationalCommandBuilderDependencies Dependencies { get; }

        /// <summary>
        ///     The source for <see cref="RelationalTypeMapping" />s to use.
        /// </summary>
        public virtual IRelationalTypeMappingSource TypeMappingSource
            => Dependencies.TypeMappingSource;

        /// <summary>
        ///     Creates the command.
        /// </summary>
        /// <returns>The newly created command.</returns>
        public virtual IRelationalCommand Build()
            => new RelationalCommand(Dependencies, _commandTextBuilder.ToString(), Parameters);

        /// <summary>
        ///     Gets the command text.
        /// </summary>
        public override string ToString()
            => _commandTextBuilder.ToString();

        /// <summary>
        ///     The collection of parameters.
        /// </summary>
        public virtual IReadOnlyList<IRelationalParameter> Parameters
            => _parameters;

        /// <summary>
        ///     Adds the given parameter to this command.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public virtual IRelationalCommandBuilder AddParameter(IRelationalParameter parameter)
        {
            _parameters.Add(parameter);

            return this;
        }

        /// <summary>
        ///     Appends an object to the command text.
        /// </summary>
        /// <param name="value">The object to be written.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public virtual IRelationalCommandBuilder Append(string value)
        {
            _commandTextBuilder.Append(value);

            return this;
        }

        /// <summary>
        ///     Appends a blank line to the command text.
        /// </summary>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public virtual IRelationalCommandBuilder AppendLine()
        {
            _commandTextBuilder.AppendLine();

            return this;
        }

        /// <summary>
        ///     Increments the indent of subsequent lines.
        /// </summary>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public virtual IRelationalCommandBuilder IncrementIndent()
        {
            _commandTextBuilder.IncrementIndent();

            return this;
        }

        /// <summary>
        ///     Decrements the indent of subsequent lines.
        /// </summary>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public virtual IRelationalCommandBuilder DecrementIndent()
        {
            _commandTextBuilder.DecrementIndent();

            return this;
        }

        /// <summary>
        ///     Gets the length of the command text.
        /// </summary>
        public virtual int CommandTextLength
            => _commandTextBuilder.Length;
    }
}

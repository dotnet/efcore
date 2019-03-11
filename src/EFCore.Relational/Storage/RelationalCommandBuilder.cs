// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

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
    public class RelationalCommandBuilder : IRelationalCommandBuilder
    {
        private readonly List<IRelationalParameter> _parameters = new List<IRelationalParameter>();
        private readonly IndentedStringBuilder _commandTextBuilder = new IndentedStringBuilder();

        /// <summary>
        ///     <para>
        ///         Constructs a new <see cref="RelationalCommand"/>.
        ///     </para>
        ///     <para>
        ///         This type is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        public RelationalCommandBuilder(
            [NotNull] RelationalCommandBuilderDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
        }

        /// <summary>
        ///     Dependencies for this service.
        /// </summary>
        public virtual RelationalCommandBuilderDependencies Dependencies { get; }

        /// <summary>
        ///     The source for <see cref="RelationalTypeMapping"/>s to use.
        /// </summary>
        public virtual IRelationalTypeMappingSource TypeMappingSource => Dependencies.TypeMappingSource;

        /// <summary>
        ///     Creates the command.
        /// </summary>
        /// <returns> The newly created command. </returns>
        public virtual IRelationalCommand Build()
            => new RelationalCommand(Dependencies, _commandTextBuilder.ToString(), Parameters);

        /// <summary>
        ///     Gets the command text.
        /// </summary>
        public override string ToString() => _commandTextBuilder.ToString();

        /// <summary>
        ///     The collection of parameters.
        /// </summary>
        public virtual IReadOnlyList<IRelationalParameter> Parameters => _parameters;

        /// <summary>
        ///     Adds the given parameter to this command.
        /// </summary>
        /// <param name="parameter"> The parameter. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public virtual IRelationalCommandBuilder AddParameter(IRelationalParameter parameter)
        {
            Check.NotNull(parameter, nameof(parameter));

            _parameters.Add(parameter);

            return this;
        }

        /// <summary>
        ///     Appends an object to the command text.
        /// </summary>
        /// <param name="value"> The object to be written. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public virtual IRelationalCommandBuilder Append(object value)
        {
            Check.NotNull(value, nameof(value));

            _commandTextBuilder.Append(value);

            return this;
        }

        /// <summary>
        ///     Appends a blank line to the command text.
        /// </summary>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public virtual IRelationalCommandBuilder AppendLine()
        {
            _commandTextBuilder.AppendLine();

            return this;
        }

        /// <summary>
        ///     Increments the indent of subsequent lines.
        /// </summary>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public virtual IRelationalCommandBuilder IncrementIndent()
        {
            _commandTextBuilder.IncrementIndent();

            return this;
        }

        /// <summary>
        ///     Decrements the indent of subsequent lines.
        /// </summary>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public virtual IRelationalCommandBuilder DecrementIndent()
        {
            _commandTextBuilder.DecrementIndent();

            return this;
        }

        /// <summary>
        ///     Gets the length of the command text.
        /// </summary>
        public virtual int CommandTextLength => _commandTextBuilder.Length;
    }
}

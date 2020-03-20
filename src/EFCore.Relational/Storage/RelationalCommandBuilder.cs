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
#pragma warning disable EF1001 // Internal EF Core API usage.
        private readonly IndentedStringBuilder _commandTextBuilder = new IndentedStringBuilder();
#pragma warning restore EF1001 // Internal EF Core API usage.

        /// <summary>
        ///     <para>
        ///         Constructs a new <see cref="RelationalCommand" />.
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
        ///     The source for <see cref="RelationalTypeMapping" />s to use.
        /// </summary>
        public virtual IRelationalTypeMappingSource TypeMappingSource => Dependencies.TypeMappingSource;

        /// <summary>
        ///     Creates the command.
        /// </summary>
        /// <returns> The newly created command. </returns>
        public virtual IRelationalCommand Build()
#pragma warning disable EF1001 // Internal EF Core API usage.
            => new RelationalCommand(Dependencies, _commandTextBuilder.ToString(), Parameters);
#pragma warning restore EF1001 // Internal EF Core API usage.

        /// <summary>
        ///     Gets the command text.
        /// </summary>
#pragma warning disable EF1001 // Internal EF Core API usage.
        public override string ToString() => _commandTextBuilder.ToString();
#pragma warning restore EF1001 // Internal EF Core API usage.

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

#pragma warning disable EF1001 // Internal EF Core API usage.
            _commandTextBuilder.Append(value);
#pragma warning restore EF1001 // Internal EF Core API usage.

            return this;
        }

        /// <summary>
        ///     Appends a blank line to the command text.
        /// </summary>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public virtual IRelationalCommandBuilder AppendLine()
        {
#pragma warning disable EF1001 // Internal EF Core API usage.
            _commandTextBuilder.AppendLine();
#pragma warning restore EF1001 // Internal EF Core API usage.

            return this;
        }

        /// <summary>
        ///     Increments the indent of subsequent lines.
        /// </summary>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public virtual IRelationalCommandBuilder IncrementIndent()
        {
#pragma warning disable EF1001 // Internal EF Core API usage.
            _commandTextBuilder.IncrementIndent();
#pragma warning restore EF1001 // Internal EF Core API usage.

            return this;
        }

        /// <summary>
        ///     Decrements the indent of subsequent lines.
        /// </summary>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public virtual IRelationalCommandBuilder DecrementIndent()
        {
#pragma warning disable EF1001 // Internal EF Core API usage.
            _commandTextBuilder.DecrementIndent();
#pragma warning restore EF1001 // Internal EF Core API usage.

            return this;
        }

        /// <summary>
        ///     Gets the length of the command text.
        /// </summary>
#pragma warning disable EF1001 // Internal EF Core API usage.
        public virtual int CommandTextLength => _commandTextBuilder.Length;
#pragma warning restore EF1001 // Internal EF Core API usage.
    }
}

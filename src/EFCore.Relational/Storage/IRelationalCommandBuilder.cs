// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

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
    public interface IRelationalCommandBuilder
    {
        /// <summary>
        ///     The collection of parameters.
        /// </summary>
        IReadOnlyList<IRelationalParameter> Parameters { get; }

        /// <summary>
        ///     Adds the given parameter to this command.
        /// </summary>
        /// <param name="parameter"> The parameter. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        IRelationalCommandBuilder AddParameter([NotNull] IRelationalParameter parameter);

        /// <summary>
        ///     The source for <see cref="RelationalTypeMapping"/>s to use.
        /// </summary>
        IRelationalTypeMappingSource TypeMappingSource { get; }

        /// <summary>
        ///     Creates the command.
        /// </summary>
        /// <returns> The newly created command. </returns>
        IRelationalCommand Build();

        /// <summary>
        ///     Appends an object to the command text.
        /// </summary>
        /// <param name="value"> The object to be written. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        IRelationalCommandBuilder Append([NotNull] object value);

        /// <summary>
        ///     Appends a blank line to the command text.
        /// </summary>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        IRelationalCommandBuilder AppendLine();

        /// <summary>
        ///     Increments the indent of subsequent lines.
        /// </summary>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        IRelationalCommandBuilder IncrementIndent();

        /// <summary>
        ///     Decrements the indent of subsequent lines.
        /// </summary>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        IRelationalCommandBuilder DecrementIndent();

        /// <summary>
        ///     Gets the length of the command text.
        /// </summary>
        int CommandTextLength { get; }
    }
}

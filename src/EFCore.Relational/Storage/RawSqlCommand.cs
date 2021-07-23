// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Represents a raw SQL command to be executed against a relational database.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class RawSqlCommand
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RawSqlCommand" /> class.
        /// </summary>
        /// <param name="relationalCommand"> The command to be executed. </param>
        /// <param name="parameterValues"> The values to be assigned to parameters. </param>
        public RawSqlCommand(
            IRelationalCommand relationalCommand,
            IReadOnlyDictionary<string, object?> parameterValues)
        {
            Check.NotNull(relationalCommand, nameof(relationalCommand));
            Check.NotNull(parameterValues, nameof(parameterValues));

            RelationalCommand = relationalCommand;
            ParameterValues = parameterValues;
        }

        /// <summary>
        ///     Gets the command to be executed.
        /// </summary>
        public virtual IRelationalCommand RelationalCommand { get; }

        /// <summary>
        ///     Gets the values to be assigned to parameters.
        /// </summary>
        public virtual IReadOnlyDictionary<string, object?> ParameterValues { get; }
    }
}

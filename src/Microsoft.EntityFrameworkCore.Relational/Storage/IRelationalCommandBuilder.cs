// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;

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
    public interface IRelationalCommandBuilder : IInfrastructure<IndentedStringBuilder>
    {
        /// <summary>
        ///     Builds the parameters associated with this command.
        /// </summary>
        IRelationalParameterBuilder ParameterBuilder { get; }

        /// <summary>
        ///     Creates the command.
        /// </summary>
        /// <returns> The newly created command. </returns>
        IRelationalCommand Build();
    }
}

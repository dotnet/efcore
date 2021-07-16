// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data.Common;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         Implemented by database providers to generate the query string for <see cref="EntityFrameworkQueryableExtensions.ToQueryString" />.
    ///     </para>
    ///     <para>
    ///         This interface is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public interface IRelationalQueryStringFactory
    {
        /// <summary>
        ///     Returns a formatted query string for the given command.
        /// </summary>
        /// <param name="command"> The command that represents the query. </param>
        /// <returns> The formatted string. </returns>
        string Create(DbCommand command);
    }
}

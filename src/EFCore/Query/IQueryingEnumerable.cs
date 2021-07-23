// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         Interface that can be implemented by a database provider's <see cref="IEnumerable" /> implementation to
    ///         provide the query string for debugging purposes.
    ///     </para>
    ///     <para>
    ///         This interface is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public interface IQueryingEnumerable
    {
        /// <summary>
        ///     <para>
        ///         A string representation of the query used.
        ///     </para>
        ///     <para>
        ///         Warning: this string may not be suitable for direct execution is intended only for use in debugging.
        ///     </para>
        /// </summary>
        /// <returns> The query string. </returns>
        string ToQueryString();
    }
}

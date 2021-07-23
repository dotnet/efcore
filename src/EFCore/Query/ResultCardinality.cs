// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         An enum that represents the cardinality of query result.
    ///     </para>
    ///     <para>
    ///         This enum is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public enum ResultCardinality
    {
        /// <summary>
        ///     Indicates that the query returns multiple results.
        /// </summary>
        Enumerable,

        /// <summary>
        ///     Indicates that the query returns a single result. Throws if there is no result or more than one result.
        /// </summary>
        Single,

        /// <summary>
        ///     Indicates that the query returns a single or default result. Throws if there is more than one result.
        /// </summary>
        SingleOrDefault
    }
}

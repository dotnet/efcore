// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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

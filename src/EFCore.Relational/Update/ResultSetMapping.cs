// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Update
{
    /// <summary>
    ///     <para>
    ///         Indicates what kind of impact on the result set a given command will have.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers; it is generally not used in application code.
    ///     </para>
    /// </summary>
    public enum ResultSetMapping
    {
        /// <summary>
        ///     The command does not have any result set mapping.
        /// </summary>
        NoResultSet,

        /// <summary>
        ///     The command maps to a result in the result set, but this is not the last result.
        /// </summary>
        NotLastInResultSet,

        /// <summary>
        ///     The command maps to the last result in the result set.
        /// </summary>
        LastInResultSet
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     The table-like store object type.
    /// </summary>
    public enum StoreObjectType
    {
        /// <summary>
        ///     A table.
        /// </summary>
        Table,

        /// <summary>
        ///     A view.
        /// </summary>
        View,

        /// <summary>
        ///     A SQL query.
        /// </summary>
        SqlQuery,

        /// <summary>
        ///     A table-valued function.
        /// </summary>
        Function
    }
}

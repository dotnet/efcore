// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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

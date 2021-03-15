// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Indicates how the related collections in a query should be loaded from database.
    /// </summary>
    public enum QuerySplittingBehavior
    {
        /// <summary>
        ///     <para>
        ///         The related collections will be loaded in same database query as parent query.
        ///     </para>
        ///     <para>
        ///         This behavior generally guarantees result consistency in the face of concurrent updates
        ///         (but details may vary based on the database and transaction isolation level in use).
        ///         However, this can cause performance issues when the query loads multiple related collections.
        ///     </para>
        /// </summary>
        SingleQuery = 0,

        /// <summary>
        ///     <para>
        ///         The related collections will be loaded in separate database queries from the parent query.
        ///     </para>
        ///     <para>
        ///         This behavior can significantly improve performance when the query loads multiple collections.
        ///         However, since separate queries are used, this can result in inconsistent results when concurrent updates occur.
        ///         Serializable or snapshot transactions can be used to mitigate this
        ///         and achieve consistency with split queries, but that may bring other performance costs and behavioral difference.
        ///     </para>
        /// </summary>
        SplitQuery
    }
}

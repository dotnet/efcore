// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Indicates how the related collections in a query should be loaded from database.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-split-queries">EF Core split queries</see> for more information and examples.
/// </remarks>
public enum QuerySplittingBehavior
{
    /// <summary>
    ///     The related collections will be loaded in same database query as parent query.
    /// </summary>
    /// <remarks>
    ///     This behavior generally guarantees result consistency in the face of concurrent updates
    ///     (but details may vary based on the database and transaction isolation level in use).
    ///     However, this can cause performance issues when the query loads multiple related collections.
    /// </remarks>
    SingleQuery = 0,

    /// <summary>
    ///     The related collections will be loaded in separate database queries from the parent query.
    /// </summary>
    /// <remarks>
    ///     This behavior can significantly improve performance when the query loads multiple collections.
    ///     However, since separate queries are used, this can result in inconsistent results when concurrent updates occur.
    ///     Serializable or snapshot transactions can be used to mitigate this
    ///     and achieve consistency with split queries, but that may bring other performance costs and behavioral difference.
    /// </remarks>
    SplitQuery
}

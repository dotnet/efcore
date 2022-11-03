// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     The table-like store object type.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
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
    Function,

    /// <summary>
    ///     An insert stored procedure.
    /// </summary>
    InsertStoredProcedure,

    /// <summary>
    ///     A delete stored procedure.
    /// </summary>
    DeleteStoredProcedure,

    /// <summary>
    ///     An update stored procedure.
    /// </summary>
    UpdateStoredProcedure,
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     SQLite specific extension methods for <see cref="ITable" />.
/// </summary>
public static class SqliteTableExtensions
{
    /// <summary>
    ///     Returns a value indicating whether to use the SQL RETURNING clause when saving changes to the table.
    ///     The RETURNING clause is incompatible with certain Sqlite features, such as virtual tables or tables with AFTER triggers.
    /// </summary>
    /// <param name="table">The table.</param>
    /// <returns><see langword="true" /> if the SQL RETURNING clause is used to save changes to the table.</returns>
    public static bool IsSqlReturningClauseUsed(this ITable table)
    {
        if (table.FindRuntimeAnnotation(SqliteAnnotationNames.UseSqlReturningClause) is { Value: bool isSqlOutputClauseUsed })
        {
            return isSqlOutputClauseUsed;
        }

        isSqlOutputClauseUsed = table.EntityTypeMappings.All(
            e => ((IEntityType)e.TypeBase).IsSqlReturningClauseUsed(StoreObjectIdentifier.Table(table.Name, table.Schema)));

        table.SetRuntimeAnnotation(SqliteAnnotationNames.UseSqlReturningClause, isSqlOutputClauseUsed);

        return isSqlOutputClauseUsed;
    }
}

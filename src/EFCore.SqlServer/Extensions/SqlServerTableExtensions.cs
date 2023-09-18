// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     SQL Server specific extension methods for <see cref="ITable" />.
/// </summary>
public static class SqlServerTableExtensions
{
    /// <summary>
    ///     Returns a value indicating whether to use the SQL OUTPUT clause when saving changes to the table. The OUTPUT clause is
    ///     incompatible with certain SQL Server features, such as tables with triggers.
    /// </summary>
    /// <param name="table">The table.</param>
    /// <returns><see langword="true" /> if the SQL OUTPUT clause is used to save changes to the table.</returns>
    public static bool IsSqlOutputClauseUsed(this ITable table)
    {
        if (table.FindRuntimeAnnotation(SqlServerAnnotationNames.UseSqlOutputClause) is { Value: bool isSqlOutputClauseUsed })
        {
            return isSqlOutputClauseUsed;
        }

        isSqlOutputClauseUsed = table.EntityTypeMappings.All(
            e => ((IEntityType)e.TypeBase).IsSqlOutputClauseUsed(StoreObjectIdentifier.Table(table.Name, table.Schema)));

        table.SetRuntimeAnnotation(SqlServerAnnotationNames.UseSqlOutputClause, isSqlOutputClauseUsed);

        return isSqlOutputClauseUsed;
    }
}

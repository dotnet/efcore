// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Update.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public static class ColumnAccessorsFactory
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static ColumnAccessors Create(IColumn column)
        => (ColumnAccessors)GenericCreate
            .MakeGenericMethod(column.ProviderClrType)
            .Invoke(null, [column])!;

    private static readonly MethodInfo GenericCreate
        = typeof(ColumnAccessorsFactory).GetTypeInfo().GetDeclaredMethod(nameof(CreateGeneric))!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static ColumnAccessors CreateGeneric<TColumn>(IColumn column)
        => new(
            CreateCurrentValueGetter<TColumn>(column),
            CreateOriginalValueGetter<TColumn>(column));

    private static Func<IReadOnlyModificationCommand, (TColumn?, bool)> CreateCurrentValueGetter<TColumn>(IColumn column)
        => c =>
        {
            if (c.Entries.Count > 0)
            {
                var value = default(TColumn);
                var valueFound = false;
                for (var i = 0; i < c.Entries.Count; i++)
                {
                    var entry = c.Entries[i];
                    var property = column.FindColumnMapping(entry.EntityType)?.Property;
                    if (property == null)
                    {
                        continue;
                    }

                    var providerValue = entry.GetCurrentProviderValue(property);
                    if (providerValue == null)
                    {
                        return (value!, valueFound);
                    }

                    value = (TColumn)providerValue!;
                    valueFound = true;
                    if (entry.EntityState == EntityState.Added
                        || entry.IsModified(property))
                    {
                        return (value, valueFound);
                    }
                }

                return (value, valueFound);
            }

            var modification = c.ColumnModifications.FirstOrDefault(m => m.ColumnName == column.Name);
            return modification == null
                ? (default, false)
                : modification.Value == null
                    ? (default, false)
                    : ((TColumn)modification.Value!, true);
        };

    private static Func<IReadOnlyModificationCommand, (TColumn, bool)> CreateOriginalValueGetter<TColumn>(IColumn column)
        => c =>
        {
            if (c.Entries.Count > 0)
            {
                var value = default(TColumn)!;
                var valueFound = false;
                for (var i = 0; i < c.Entries.Count; i++)
                {
                    var entry = c.Entries[i];
                    var property = column.FindColumnMapping(entry.EntityType)?.Property;
                    if (property == null)
                    {
                        continue;
                    }

                    var providerValue = entry.GetOriginalProviderValue(property);
                    if (providerValue == null)
                    {
                        return (value!, valueFound);
                    }

                    value = (TColumn)providerValue!;
                    valueFound = true;
                    if (entry.EntityState == EntityState.Unchanged
                        || (entry.EntityState == EntityState.Modified && !entry.IsModified(property)))
                    {
                        return (value, valueFound);
                    }
                }

                return (value, valueFound);
            }

            var modification = c.ColumnModifications.FirstOrDefault(m => m.ColumnName == column.Name);
            return modification == null
                ? (default!, false)
                : modification.OriginalValue == null
                    ? (default!, false)
                    : ((TColumn)modification.OriginalValue!, true);
        };
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a database trigger on a table.
/// </summary>
/// <remarks>
///     <para>
///         Since triggers features vary across databases, this is mainly an extension point for providers to add their own annotations.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-triggers">Database triggers</see> for more information and examples.
///     </para>
/// </remarks>
public interface IReadOnlyTrigger : IReadOnlyAnnotatable
{
    /// <summary>
    ///     Gets the name of the trigger in the model.
    /// </summary>
    string ModelName { get; }

    /// <summary>
    ///     Gets the database name of the trigger.
    /// </summary>
    string? Name { get; }

    /// <summary>
    ///     Gets the name of the table on which this trigger is defined.
    /// </summary>
    string TableName { get; }

    /// <summary>
    ///     Gets the schema of the table on which this trigger is defined.
    /// </summary>
    string? TableSchema { get; }

    /// <summary>
    ///     Returns the default database name that would be used for this trigger.
    /// </summary>
    /// <returns>The default name that would be used for this trigger.</returns>
    string? GetDefaultName()
    {
        var table = StoreObjectIdentifier.Create(EntityType, StoreObjectType.Table);
        return !table.HasValue ? null : GetDefaultName(table.Value);
    }

    /// <summary>
    ///     Gets the database name of the trigger.
    /// </summary>
    /// <param name="storeObject">The identifier of the store object.</param>
    /// <returns>The database name of the trigger for the given store object.</returns>
    string? GetName(in StoreObjectIdentifier storeObject);

    /// <summary>
    ///     Returns the default database name that would be used for this trigger.
    /// </summary>
    /// <param name="storeObject">The identifier of the store object.</param>
    /// <returns>The default name that would be used for this trigger.</returns>
    string? GetDefaultName(in StoreObjectIdentifier storeObject)
        => storeObject.StoreObjectType == StoreObjectType.Table
            ? Uniquifier.Truncate(ModelName, EntityType.Model.GetMaxIdentifierLength())
            : null;

    /// <summary>
    ///     Gets the entity type on which this trigger is defined.
    /// </summary>
    IReadOnlyEntityType EntityType { get; }
}

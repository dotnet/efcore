// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a database trigger on a table.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-triggers">Database triggers</see> for more information and examples.
/// </remarks>
public class RuntimeTrigger : AnnotatableBase, ITrigger
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public RuntimeTrigger(
        RuntimeEntityType entityType,
        string modelName,
        string? name,
        string tableName,
        string? tableSchema)
    {
        EntityType = entityType;
        ModelName = modelName;
        Name = name;
        TableName = tableName;
        TableSchema = tableSchema;
    }

    /// <inheritdoc />
    public virtual string ModelName { get; }

    /// <summary>
    ///     Gets the database name of the trigger.
    /// </summary>
    public virtual string? Name { get; }

    /// <inheritdoc />
    public virtual string? GetName(in StoreObjectIdentifier storeObject)
        => Name;

    /// <inheritdoc />
    public virtual string TableName { get; }

    /// <inheritdoc />
    public virtual string? TableSchema { get; }

    /// <inheritdoc />
    public virtual IEntityType EntityType { get; }

    /// <inheritdoc />
    IReadOnlyEntityType IReadOnlyTrigger.EntityType
        => EntityType;
}

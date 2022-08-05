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
    ///    Initializes a new instance of the <see cref="RuntimeTrigger" /> class.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="modelName">The name in the model.</param>
    /// <param name="name">The name in the database.</param>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="tableSchema">The schema of the table.</param>
    public RuntimeTrigger(
        RuntimeEntityType entityType,
        string modelName,
        string name,
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
    
    /// <inheritdoc />
    public virtual string Name { get; }

    /// <inheritdoc />
    public virtual string? GetName(in StoreObjectIdentifier storeObject)
        => storeObject.StoreObjectType == StoreObjectType.Table
                && TableName == storeObject.Name
                && TableSchema == storeObject.Schema
            ? Name
            : null;

    /// <inheritdoc />
    public virtual string TableName { get; }

    /// <inheritdoc />
    public virtual string? TableSchema { get; }

    /// <inheritdoc />
    public virtual IEntityType EntityType { get; }

    /// <inheritdoc />
    public override string ToString()
        => ((ITrigger)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual DebugView DebugView
        => new(
            () => ((ITrigger)this).ToDebugString(),
            () => ((ITrigger)this).ToDebugString(MetadataDebugStringOptions.LongDefault));

    /// <inheritdoc />
    IReadOnlyEntityType IReadOnlyTrigger.EntityType
        => EntityType;
}

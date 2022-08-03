// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;

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

    /// <summary>
    ///     <para>
    ///         Creates a human-readable representation of the given metadata.
    ///     </para>
    ///     <para>
    ///         Warning: Do not rely on the format of the returned string.
    ///         It is designed for debugging only and may change arbitrarily between releases.
    ///     </para>
    /// </summary>
    /// <param name="options">Options for generating the string.</param>
    /// <param name="indent">The number of indent spaces to use before each new line.</param>
    /// <returns>A human-readable representation.</returns>
    string ToDebugString(MetadataDebugStringOptions options = MetadataDebugStringOptions.ShortDefault, int indent = 0)
    {
        var builder = new StringBuilder();
        var indentString = new string(' ', indent);

        builder
            .Append(indentString)
            .Append("Trigger: ")
            .Append(ModelName);

        if (Name != ModelName)
        {
            builder.Append('*');
        }

        builder.Append(" ");

        if (TableSchema != null)
        {
            builder
                .Append('.')
                .Append(TableSchema);
        }

        builder.Append(TableName);

        if ((options & MetadataDebugStringOptions.SingleLine) == 0)
        {
            if ((options & MetadataDebugStringOptions.IncludeAnnotations) != 0)
            {
                builder.Append(AnnotationsToDebugString(indent: indent + 2));
            }
        }

        return builder.ToString();
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Trigger extension methods for relational database metadata.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-triggers">Database triggers</see> for more information and examples.
/// </remarks>
public static class RelationalTriggerExtensions
{
    /// <summary>
    ///     Gets the name of the trigger in the database.
    /// </summary>
    /// <param name="trigger">The trigger.</param>
    /// <returns>The name of the trigger in the database.</returns>
    public static string? GetDatabaseName(this IReadOnlyTrigger trigger)
    {
        if (trigger.EntityType.GetTableName() == null)
        {
            return null;
        }

        var annotation = trigger.FindAnnotation(RelationalAnnotationNames.Name);
        return annotation != null ? (string?)annotation.Value : trigger.GetDefaultDatabaseName();
    }

    /// <summary>
    ///     Returns the default name that would be used for this trigger in the database.
    /// </summary>
    /// <param name="trigger">The trigger.</param>
    /// <returns>The default name that would be used for this trigger in the database.</returns>
    public static string? GetDefaultDatabaseName(this IReadOnlyTrigger trigger)
    {
        var table = StoreObjectIdentifier.Create(trigger.EntityType, StoreObjectType.Table);
        return !table.HasValue ? null : trigger.GetDefaultDatabaseName(table.Value);
    }

    /// <summary>
    ///     Gets the database name of the trigger.
    /// </summary>
    /// <param name="trigger">The trigger.</param>
    /// <param name="storeObject">The identifier of the store object.</param>
    /// <returns>The database name of the trigger for the given store object.</returns>
    public static string? GetDatabaseName(this IReadOnlyTrigger trigger, in StoreObjectIdentifier storeObject)
    {
        var triggerTable = trigger.GetTableName();
        if (storeObject.StoreObjectType != StoreObjectType.Table
            || (triggerTable != null
                && (trigger.GetTableName() != storeObject.Name
                    || trigger.GetTableSchema() != storeObject.Schema)))
        {
            return null;
        }

        var annotation = trigger.FindAnnotation(RelationalAnnotationNames.Name);
        return annotation != null ? (string?)annotation.Value : trigger.GetDefaultDatabaseName(storeObject);
    }

    /// <summary>
    ///     Returns the default database name that would be used for this trigger.
    /// </summary>
    /// <param name="trigger">The trigger.</param>
    /// <param name="storeObject">The identifier of the store object.</param>
    /// <returns>The default name that would be used for this trigger.</returns>
    public static string? GetDefaultDatabaseName(this IReadOnlyTrigger trigger, in StoreObjectIdentifier storeObject)
        => storeObject.StoreObjectType == StoreObjectType.Table
            ? Uniquifier.Truncate(trigger.ModelName, trigger.EntityType.Model.GetMaxIdentifierLength())
            : null;

    /// <summary>
    ///     Sets the name of the trigger in the database.
    /// </summary>
    /// <param name="trigger">The trigger.</param>
    /// <param name="name">The name of the trigger in the database.</param>
    public static void SetDatabaseName(this IMutableTrigger trigger, string? name)
        => trigger.SetOrRemoveAnnotation(
            RelationalAnnotationNames.Name,
            Check.NullButNotEmpty(name, nameof(name)));

    /// <summary>
    ///     Sets the name of the trigger in the database.
    /// </summary>
    /// <param name="trigger">The trigger.</param>
    /// <param name="name">The name of the trigger in the database.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static string? SetDatabaseName(this IConventionTrigger trigger, string? name, bool fromDataAnnotation = false)
        => (string?)trigger.SetOrRemoveAnnotation(
            RelationalAnnotationNames.Name,
            Check.NullButNotEmpty(name, nameof(name)),
            fromDataAnnotation)?.Value;

    /// <summary>
    ///     Gets the configuration source for the database name.
    /// </summary>
    /// <param name="trigger">The trigger.</param>
    /// <returns>The configuration source for the database name.</returns>
    public static ConfigurationSource? GetDatabaseNameConfigurationSource(this IConventionTrigger trigger)
        => trigger.FindAnnotation(RelationalAnnotationNames.Name)?.GetConfigurationSource();

    /// <summary>
    ///     Gets the name of the table on which this trigger is defined.
    /// </summary>
    /// <param name="trigger">The trigger.</param>
    /// <returns>The name of the table on which this trigger is defined.</returns>
    public static string GetTableName(this IReadOnlyTrigger trigger)
    {
        if (trigger.FindAnnotation(RelationalAnnotationNames.TableName) is { Value: string tableName })
        {
            return tableName;
        }

        var mainTableName = trigger.EntityType.GetTableName();

        Check.DebugAssert(mainTableName is not null, "Trigger defined on entity not mapped to a table");

        return mainTableName;
    }

    /// <summary>
    ///     Sets the name of the table on which this trigger is defined.
    /// </summary>
    /// <param name="trigger">The trigger.</param>
    /// <param name="name">The name of the table on which this trigger is defined.</param>
    public static void SetTableName(this IMutableTrigger trigger, string? name)
        => trigger.SetOrRemoveAnnotation(
            RelationalAnnotationNames.TableName,
            Check.NullButNotEmpty(name, nameof(name)));

    /// <summary>
    ///     Sets the name of the table on which this trigger is defined.
    /// </summary>
    /// <param name="trigger">The trigger.</param>
    /// <param name="name">The name of the table on which this trigger is defined.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static string? SetTableName(this IConventionTrigger trigger, string? name, bool fromDataAnnotation = false)
        => (string?)trigger.SetOrRemoveAnnotation(
            RelationalAnnotationNames.TableName,
            Check.NullButNotEmpty(name, nameof(name)),
            fromDataAnnotation)?.Value;

    /// <summary>
    ///     Gets the configuration source for the table name.
    /// </summary>
    /// <param name="trigger">The trigger.</param>
    /// <returns>The configuration source for the database name.</returns>
    public static ConfigurationSource? GetTableNameConfigurationSource(this IConventionTrigger trigger)
        => trigger.FindAnnotation(RelationalAnnotationNames.TableName)?.GetConfigurationSource();

    /// <summary>
    ///     Gets the schema of the table on which this trigger is defined.
    /// </summary>
    /// <param name="trigger">The trigger.</param>
    /// <returns>The schema of the table on which this trigger is defined.</returns>
    public static string? GetTableSchema(this IReadOnlyTrigger trigger)
        => (string?)trigger.FindAnnotation(RelationalAnnotationNames.Schema)?.Value
            ?? trigger.EntityType.GetSchema();

    /// <summary>
    ///     Sets the schema of the table on which this trigger is defined.
    /// </summary>
    /// <param name="trigger">The trigger.</param>
    /// <param name="schema">The schema of the table on which this trigger is defined.</param>
    public static void SetTableSchema(this IMutableTrigger trigger, string? schema)
        => trigger.SetOrRemoveAnnotation(
            RelationalAnnotationNames.Schema,
            Check.NullButNotEmpty(schema, nameof(schema)));

    /// <summary>
    ///     Sets the schema of the table on which this trigger is defined.
    /// </summary>
    /// <param name="trigger">The trigger.</param>
    /// <param name="schema">The schema of the table on which this trigger is defined.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static string? SetTableSchema(this IConventionTrigger trigger, string? schema, bool fromDataAnnotation = false)
        => (string?)trigger.SetOrRemoveAnnotation(
            RelationalAnnotationNames.Schema,
            Check.NullButNotEmpty(schema, nameof(schema)),
            fromDataAnnotation)?.Value;

    /// <summary>
    ///     Gets the configuration source for the table schema.
    /// </summary>
    /// <param name="trigger">The trigger.</param>
    /// <returns>The configuration source for the database name.</returns>
    public static ConfigurationSource? GetTableSchemaConfigurationSource(this IConventionTrigger trigger)
        => trigger.FindAnnotation(RelationalAnnotationNames.Schema)?.GetConfigurationSource();
}

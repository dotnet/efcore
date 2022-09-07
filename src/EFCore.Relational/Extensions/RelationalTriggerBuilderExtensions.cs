// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Relational database specific extension methods for <see cref="TriggerBuilder" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-triggers">Database triggers</see> for more information and examples.
/// </remarks>
public static class RelationalTriggerBuilderExtensions
{
    /// <summary>
    ///     Sets the database name of the trigger.
    /// </summary>
    /// <param name="triggerBuilder">The builder for the trigger being configured.</param>
    /// <param name="name">The database name of the trigger.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The same builder instance if the configuration was applied, <see langword="null" /> otherwise.</returns>
    public static IConventionTriggerBuilder? HasDatabaseName(
        this IConventionTriggerBuilder triggerBuilder,
        string? name,
        bool fromDataAnnotation = false)
    {
        if (!triggerBuilder.CanSetDatabaseName(name, fromDataAnnotation))
        {
            return null;
        }

        triggerBuilder.Metadata.SetDatabaseName(name, fromDataAnnotation);
        return triggerBuilder;
    }

    /// <summary>
    ///     Returns a value indicating whether the given name can be set for the trigger.
    /// </summary>
    /// <param name="triggerBuilder">The builder for the trigger being configured.</param>
    /// <param name="name">The database name of the trigger.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the database name can be set for the trigger.</returns>
    public static bool CanSetDatabaseName(this IConventionTriggerBuilder triggerBuilder, string? name, bool fromDataAnnotation = false)
        => triggerBuilder.CanSetAnnotation(RelationalAnnotationNames.Name, name, fromDataAnnotation);

    /// <summary>
    ///     Sets name of the table on which this trigger is defined.
    /// </summary>
    /// <param name="triggerBuilder">The builder for the trigger being configured.</param>
    /// <param name="name">The name of the table on which this trigger is defined.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The same builder instance if the configuration was applied, <see langword="null" /> otherwise.</returns>
    public static IConventionTriggerBuilder? HasTableName(
        this IConventionTriggerBuilder triggerBuilder,
        string? name,
        bool fromDataAnnotation = false)
    {
        if (!triggerBuilder.CanSetTableName(name, fromDataAnnotation))
        {
            return null;
        }

        triggerBuilder.Metadata.SetTableName(name, fromDataAnnotation);
        return triggerBuilder;
    }

    /// <summary>
    ///     Returns a value indicating whether the given table name can be set for the trigger.
    /// </summary>
    /// <param name="triggerBuilder">The builder for the trigger being configured.</param>
    /// <param name="name">The name of the table on which this trigger is defined.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the table name can be set for the trigger.</returns>
    public static bool CanSetTableName(this IConventionTriggerBuilder triggerBuilder, string? name, bool fromDataAnnotation = false)
        => triggerBuilder.CanSetAnnotation(RelationalAnnotationNames.TableName, name, fromDataAnnotation);

    /// <summary>
    ///     Sets the schema of the table on which this trigger is defined.
    /// </summary>
    /// <param name="triggerBuilder">The builder for the trigger being configured.</param>
    /// <param name="schema">The schema of the table on which this trigger is defined.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The same builder instance if the configuration was applied, <see langword="null" /> otherwise.</returns>
    public static IConventionTriggerBuilder? HasTableSchema(
        this IConventionTriggerBuilder triggerBuilder,
        string? schema,
        bool fromDataAnnotation = false)
    {
        if (!triggerBuilder.CanSetTableSchema(schema, fromDataAnnotation))
        {
            return null;
        }

        triggerBuilder.Metadata.SetTableSchema(schema, fromDataAnnotation);
        return triggerBuilder;
    }

    /// <summary>
    ///     Returns a value indicating whether the given table schema can be set for the trigger.
    /// </summary>
    /// <param name="triggerBuilder">The builder for the trigger being configured.</param>
    /// <param name="schema">The schema of the table on which this trigger is defined.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the database name can be set for the trigger.</returns>
    public static bool CanSetTableSchema(this IConventionTriggerBuilder triggerBuilder, string? schema, bool fromDataAnnotation = false)
        => triggerBuilder.CanSetAnnotation(RelationalAnnotationNames.Schema, schema, fromDataAnnotation);
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class InternalTriggerBuilder : AnnotatableBuilder<Trigger, IConventionModelBuilder>, IConventionTriggerBuilder
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public InternalTriggerBuilder(Trigger trigger, IConventionModelBuilder modelBuilder)
        : base(trigger, modelBuilder)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionTriggerBuilder? HasName(string? name, ConfigurationSource configurationSource)
    {
        if (CanSetName(name, configurationSource))
        {
            Metadata.SetName(name, configurationSource);
            return this;
        }

        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSetName(string? name, ConfigurationSource configurationSource)
        => configurationSource.Overrides(Metadata.GetNameConfigurationSource())
            || Metadata.Name == name;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IConventionTrigger? HasTrigger(
        IConventionEntityType entityType,
        string name,
        string? tableName,
        string? tableSchema,
        ConfigurationSource configurationSource)
    {
        List<IConventionTrigger>? triggersToBeDetached = null;
        var trigger = entityType.FindTrigger(name);
        if (trigger != null)
        {
            if ((tableName == null && tableSchema == null)
                || (trigger.TableName == tableName && trigger.TableSchema == tableSchema))
            {
                ((Trigger)trigger).UpdateConfigurationSource(configurationSource);
                return trigger;
            }

            if (!configurationSource.Overrides(trigger.GetConfigurationSource()))
            {
                return null;
            }

            entityType.RemoveTrigger(name);
        }
        else
        {
            foreach (var derivedType in entityType.GetDerivedTypes())
            {
                var derivedTrigger = (IConventionTrigger?)Trigger.FindDeclaredTrigger(derivedType, name);
                if (derivedTrigger == null)
                {
                    continue;
                }

                if ((tableName != null || tableSchema != null)
                    && (derivedTrigger.TableName != tableName || derivedTrigger.TableSchema != tableSchema)
                    && !configurationSource.Overrides(derivedTrigger.GetConfigurationSource()))
                {
                    return null;
                }

                triggersToBeDetached ??= new List<IConventionTrigger>();

                triggersToBeDetached.Add(derivedTrigger);
            }
        }

        List<IConventionTrigger>? detachedTriggers = null;
        if (triggersToBeDetached != null)
        {
            detachedTriggers = new List<IConventionTrigger>();
            foreach (var triggerToBeDetached in triggersToBeDetached)
            {
                detachedTriggers.Add(
                    triggerToBeDetached.EntityType.RemoveTrigger(triggerToBeDetached.ModelName)!);
            }
        }

        trigger = new Trigger((IMutableEntityType)entityType, name, tableName, tableSchema, configurationSource);

        if (detachedTriggers != null)
        {
            foreach (var detachedTrigger in detachedTriggers)
            {
                Trigger.MergeInto(detachedTrigger, trigger);
            }
        }

        return trigger;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool CanHaveTrigger(
        IConventionEntityType entityType,
        string name,
        string? tableName,
        string? tableSchema,
        ConfigurationSource configurationSource)
    {
        if (tableName == null
            && tableSchema == null)
        {
            return true;
        }

        if (entityType.FindTrigger(name) is IConventionTrigger trigger)
        {
            return (trigger.TableName == tableName
                    && trigger.TableSchema == tableSchema)
                || configurationSource.Overrides(trigger.GetConfigurationSource());
        }

        foreach (var derivedType in entityType.GetDerivedTypes())
        {
            var derivedTrigger = (IConventionTrigger?)Trigger.FindDeclaredTrigger(derivedType, name);
            if (derivedTrigger == null)
            {
                continue;
            }

            if ((derivedTrigger.TableName != tableName
                || derivedTrigger.TableSchema != tableSchema)
                && !configurationSource.Overrides(derivedTrigger.GetConfigurationSource()))
            {
                return false;
            }
        }

        return true;
    }

    IConventionTrigger IConventionTriggerBuilder.Metadata
    {
        [DebuggerStepThrough]
        get => Metadata;
    }

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionTriggerBuilder? IConventionTriggerBuilder.HasName(string? name, bool fromDataAnnotation)
        => HasName(name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionTriggerBuilder.CanSetName(string? name, bool fromDataAnnotation)
        => CanSetName(name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
}

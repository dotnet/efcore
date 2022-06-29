// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that ensures that the triggers on the derived types are compatible with the triggers on the base type.
///     And also ensures that the declaring type is current.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public class TriggerConvention : IEntityTypeBaseTypeChangedConvention, IEntityTypeAddedConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="TriggerConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    /// <param name="relationalDependencies"> Parameter object containing relational dependencies for this convention.</param>
    public TriggerConvention(
        ProviderConventionSetBuilderDependencies dependencies,
        RelationalConventionSetBuilderDependencies relationalDependencies)
    {
        Dependencies = dependencies;
        RelationalDependencies = relationalDependencies;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalConventionSetBuilderDependencies RelationalDependencies { get; }

    /// <inheritdoc />
    public virtual void ProcessEntityTypeAdded(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionContext<IConventionEntityTypeBuilder> context)
    {
        var entityType = entityTypeBuilder.Metadata;
        if (!entityType.HasSharedClrType)
        {
            return;
        }

        List<IConventionTrigger>? triggersToReattach = null;
        foreach (var trigger in entityType.GetDeclaredTriggers())
        {
            if (trigger.EntityType == entityType)
            {
                continue;
            }

            triggersToReattach ??= new();

            triggersToReattach.Add(trigger);
        }

        if (triggersToReattach == null)
        {
            return;
        }

        foreach (var trigger in triggersToReattach)
        {
            var removedTrigger = entityType.RemoveTrigger(trigger.ModelName);
            if (removedTrigger != null)
            {
                Trigger.Attach(entityType, removedTrigger);
            }
        }
    }

    /// <inheritdoc />
    public virtual void ProcessEntityTypeBaseTypeChanged(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionEntityType? newBaseType,
        IConventionEntityType? oldBaseType,
        IConventionContext<IConventionEntityType> context)
    {
        var entityType = entityTypeBuilder.Metadata;
        if (newBaseType != null)
        {
            var configurationSource = entityType.GetBaseTypeConfigurationSource();
            var baseTriggers = newBaseType.GetTriggers().ToDictionary(c => c.ModelName);
            List<IConventionTrigger>? triggersToBeDetached = null;
            List<IConventionTrigger>? triggersToBeRemoved = null;
            foreach (var trigger in entityType.GetDerivedTypesInclusive().SelectMany(et => et.GetDeclaredTriggers()))
            {
                if (baseTriggers.TryGetValue(trigger.ModelName, out var baseTrigger)
                    && baseTrigger.GetConfigurationSource().Overrides(trigger.GetConfigurationSource())
                    && !AreCompatible(trigger, baseTrigger))
                {
                    if (baseTrigger.GetConfigurationSource() == ConfigurationSource.Explicit
                        && configurationSource == ConfigurationSource.Explicit
                        && trigger.GetConfigurationSource() == ConfigurationSource.Explicit)
                    {
                        throw new InvalidOperationException(
                            RelationalStrings.DuplicateTrigger(
                                trigger.ModelName,
                                trigger.EntityType.DisplayName(),
                                baseTrigger.EntityType.DisplayName()));
                    }

                    triggersToBeRemoved ??= new List<IConventionTrigger>();

                    triggersToBeRemoved.Add(trigger);
                    continue;
                }

                if (baseTrigger != null)
                {
                    triggersToBeDetached ??= new List<IConventionTrigger>();

                    triggersToBeDetached.Add(trigger);
                }
            }

            if (triggersToBeRemoved != null)
            {
                foreach (var checkConstraintToBeRemoved in triggersToBeRemoved)
                {
                    checkConstraintToBeRemoved.EntityType.RemoveTrigger(checkConstraintToBeRemoved.ModelName);
                }
            }

            if (triggersToBeDetached != null)
            {
                foreach (var triggerToBeDetached in triggersToBeDetached)
                {
                    var baseTrigger = baseTriggers[triggerToBeDetached.ModelName];
                    Trigger.MergeInto(triggerToBeDetached, baseTrigger);

                    triggerToBeDetached.EntityType.RemoveTrigger(triggerToBeDetached.ModelName);
                }
            }
        }
    }

    private static bool AreCompatible(IConventionTrigger checkConstraint, IConventionTrigger baseTrigger)
    {
        var baseTable = StoreObjectIdentifier.Create(baseTrigger.EntityType, StoreObjectType.Table);
        if (baseTable == null)
        {
            return true;
        }

        if (checkConstraint.GetName(baseTable.Value) != baseTrigger.GetName(baseTable.Value)
            && checkConstraint.GetNameConfigurationSource() is ConfigurationSource nameConfigurationSource
            && !nameConfigurationSource.OverridesStrictly(baseTrigger.GetNameConfigurationSource()))
        {
            return false;
        }

        return true;
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that ensures that the check constraints on the derived types are compatible with the check constraints on the base
///     type. And also ensures that the declaring type is current.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public class CheckConstraintConvention : IEntityTypeBaseTypeChangedConvention, IEntityTypeAddedConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="CheckConstraintConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    /// <param name="relationalDependencies"> Parameter object containing relational dependencies for this convention.</param>
    public CheckConstraintConvention(
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

    /// <summary>
    ///     Called after an entity type is added to the model.
    /// </summary>
    /// <param name="entityTypeBuilder">The builder for the entity type.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessEntityTypeAdded(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionContext<IConventionEntityTypeBuilder> context)
    {
        var entityType = entityTypeBuilder.Metadata;
        if (!entityType.HasSharedClrType)
        {
            return;
        }

        List<IConventionCheckConstraint>? constraintsToReattach = null;
        foreach (var checkConstraint in entityType.GetCheckConstraints())
        {
            if (checkConstraint.EntityType == entityType)
            {
                continue;
            }

            constraintsToReattach ??= [];

            constraintsToReattach.Add(checkConstraint);
        }

        if (constraintsToReattach == null)
        {
            return;
        }

        foreach (var checkConstraint in constraintsToReattach)
        {
            var removedCheckConstraint = entityType.RemoveCheckConstraint(checkConstraint.ModelName);
            if (removedCheckConstraint != null)
            {
                CheckConstraint.Attach(entityType, removedCheckConstraint);
            }
        }
    }

    /// <summary>
    ///     Called after the base type of an entity type changes.
    /// </summary>
    /// <param name="entityTypeBuilder">The builder for the entity type.</param>
    /// <param name="newBaseType">The new base entity type.</param>
    /// <param name="oldBaseType">The old base entity type.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
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
            var baseCheckConstraints = newBaseType.GetCheckConstraints().ToDictionary(c => c.ModelName);
            List<IConventionCheckConstraint>? checkConstraintsToBeDetached = null;
            List<IConventionCheckConstraint>? checkConstraintsToBeRemoved = null;
            foreach (var checkConstraint in entityType.GetDerivedTypesInclusive().SelectMany(et => et.GetDeclaredCheckConstraints()))
            {
                if (baseCheckConstraints.TryGetValue(checkConstraint.ModelName, out var baseCheckConstraint)
                    && baseCheckConstraint.GetConfigurationSource().Overrides(checkConstraint.GetConfigurationSource())
                    && !AreCompatible(checkConstraint, baseCheckConstraint))
                {
                    if (baseCheckConstraint.GetConfigurationSource() == ConfigurationSource.Explicit
                        && configurationSource == ConfigurationSource.Explicit
                        && checkConstraint.GetConfigurationSource() == ConfigurationSource.Explicit)
                    {
                        throw new InvalidOperationException(
                            RelationalStrings.DuplicateCheckConstraint(
                                checkConstraint.ModelName,
                                checkConstraint.EntityType.DisplayName(),
                                baseCheckConstraint.EntityType.DisplayName()));
                    }

                    checkConstraintsToBeRemoved ??= [];

                    checkConstraintsToBeRemoved.Add(checkConstraint);
                    continue;
                }

                if (baseCheckConstraint != null)
                {
                    checkConstraintsToBeDetached ??= [];

                    checkConstraintsToBeDetached.Add(checkConstraint);
                }
            }

            if (checkConstraintsToBeRemoved != null)
            {
                foreach (var checkConstraintToBeRemoved in checkConstraintsToBeRemoved)
                {
                    checkConstraintToBeRemoved.EntityType.RemoveCheckConstraint(checkConstraintToBeRemoved.ModelName);
                }
            }

            if (checkConstraintsToBeDetached != null)
            {
                foreach (var checkConstraintToBeDetached in checkConstraintsToBeDetached)
                {
                    var baseCheckConstraint = baseCheckConstraints[checkConstraintToBeDetached.ModelName];
                    CheckConstraint.MergeInto(checkConstraintToBeDetached, baseCheckConstraint);

                    checkConstraintToBeDetached.EntityType.RemoveCheckConstraint(checkConstraintToBeDetached.ModelName);
                }
            }
        }
    }

    private static bool AreCompatible(IConventionCheckConstraint checkConstraint, IConventionCheckConstraint baseCheckConstraint)
    {
        var baseTable = StoreObjectIdentifier.Create(baseCheckConstraint.EntityType, StoreObjectType.Table);
        if (baseTable == null)
        {
            return true;
        }

        if (checkConstraint.GetName(baseTable.Value) != baseCheckConstraint.GetName(baseTable.Value)
            && checkConstraint.GetNameConfigurationSource() is ConfigurationSource nameConfigurationSource
            && !nameConfigurationSource.OverridesStrictly(baseCheckConstraint.GetNameConfigurationSource()))
        {
            return false;
        }

        return CheckConstraint.AreCompatible(
            checkConstraint,
            baseCheckConstraint,
            baseTable.Value,
            shouldThrow: false);
    }
}

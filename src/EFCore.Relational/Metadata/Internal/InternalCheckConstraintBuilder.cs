// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class InternalCheckConstraintBuilder :
    AnnotatableBuilder<CheckConstraint, IConventionModelBuilder>,
    IConventionCheckConstraintBuilder
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public InternalCheckConstraintBuilder(CheckConstraint checkConstraint, IConventionModelBuilder modelBuilder)
        : base(checkConstraint, modelBuilder)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionCheckConstraintBuilder? HasName(string? name, ConfigurationSource configurationSource)
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
    public static IConventionCheckConstraint? HasCheckConstraint(
        IConventionEntityType entityType,
        string name,
        string? sql,
        ConfigurationSource configurationSource)
    {
        List<IConventionCheckConstraint>? checkConstraintsToBeDetached = null;
        var constraint = entityType.FindCheckConstraint(name);
        if (constraint != null)
        {
            if (constraint.Sql == sql)
            {
                ((CheckConstraint)constraint).UpdateConfigurationSource(configurationSource);
                return constraint;
            }

            if (!configurationSource.Overrides(constraint.GetConfigurationSource()))
            {
                return null;
            }

            entityType.RemoveCheckConstraint(name);
            constraint = null;
        }
        else
        {
            foreach (var derivedType in entityType.GetDerivedTypes())
            {
                var derivedCheckConstraint =
                    (IConventionCheckConstraint?)CheckConstraint.FindDeclaredCheckConstraint(derivedType, name);
                if (derivedCheckConstraint == null)
                {
                    continue;
                }

                if (derivedCheckConstraint.Sql != sql
                    && !configurationSource.Overrides(derivedCheckConstraint.GetConfigurationSource()))
                {
                    return null;
                }

                checkConstraintsToBeDetached ??= [];

                checkConstraintsToBeDetached.Add(derivedCheckConstraint);
            }
        }

        List<IConventionCheckConstraint>? detachedCheckConstraints = null;
        if (checkConstraintsToBeDetached != null)
        {
            detachedCheckConstraints = [];
            foreach (var checkConstraintToBeDetached in checkConstraintsToBeDetached)
            {
                detachedCheckConstraints.Add(
                    checkConstraintToBeDetached.EntityType.RemoveCheckConstraint(checkConstraintToBeDetached.ModelName)!);
            }
        }

        if (sql != null)
        {
            constraint = new CheckConstraint((IMutableEntityType)entityType, name, sql, configurationSource);

            if (detachedCheckConstraints != null)
            {
                foreach (var detachedCheckConstraint in detachedCheckConstraints)
                {
                    CheckConstraint.MergeInto(detachedCheckConstraint, constraint);
                }
            }
        }

        return constraint;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool CanHaveCheckConstraint(
        IConventionEntityType entityType,
        string name,
        string? sql,
        ConfigurationSource configurationSource)
    {
        var constraint = entityType.FindCheckConstraint(name);
        if (constraint != null)
        {
            return constraint.Sql == sql
                || configurationSource.Overrides(constraint.GetConfigurationSource());
        }

        foreach (var derivedType in entityType.GetDerivedTypes())
        {
            var derivedCheckConstraint = (IConventionCheckConstraint?)CheckConstraint.FindDeclaredCheckConstraint(derivedType, name);
            if (derivedCheckConstraint == null)
            {
                continue;
            }

            if (derivedCheckConstraint.Sql != sql
                && !configurationSource.Overrides(derivedCheckConstraint.GetConfigurationSource()))
            {
                return false;
            }
        }

        return true;
    }

    IConventionCheckConstraint IConventionCheckConstraintBuilder.Metadata
    {
        [DebuggerStepThrough]
        get => Metadata;
    }

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionCheckConstraintBuilder? IConventionCheckConstraintBuilder.HasAnnotation(string name, object? value, bool fromDataAnnotation)
        => (IConventionCheckConstraintBuilder?)base.HasAnnotation(
            name, value, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionCheckConstraintBuilder? IConventionCheckConstraintBuilder.HasNonNullAnnotation(
        string name,
        object? value,
        bool fromDataAnnotation)
        => (IConventionCheckConstraintBuilder?)base.HasNonNullAnnotation(
            name, value, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionCheckConstraintBuilder? IConventionCheckConstraintBuilder.HasNoAnnotation(string name, bool fromDataAnnotation)
        => (IConventionCheckConstraintBuilder?)base.HasNoAnnotation(
            name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionCheckConstraintBuilder? IConventionCheckConstraintBuilder.HasName(string? name, bool fromDataAnnotation)
        => HasName(name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionCheckConstraintBuilder.CanSetName(string? name, bool fromDataAnnotation)
        => CanSetName(name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
}

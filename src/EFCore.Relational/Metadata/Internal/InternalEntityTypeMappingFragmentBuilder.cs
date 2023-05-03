// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class InternalEntityTypeMappingFragmentBuilder :
    AnnotatableBuilder<EntityTypeMappingFragment, IConventionModelBuilder>,
    IConventionEntityTypeMappingFragmentBuilder
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public InternalEntityTypeMappingFragmentBuilder(
        EntityTypeMappingFragment fragment,
        IConventionModelBuilder modelBuilder)
        : base(fragment, modelBuilder)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalEntityTypeMappingFragmentBuilder? ExcludeTableFromMigrations(
        bool? excludedFromMigrations,
        ConfigurationSource configurationSource)
    {
        if (!CanExcludeTableFromMigrations(excludedFromMigrations, configurationSource))
        {
            return null;
        }

        Metadata.SetIsTableExcludedFromMigrations(excludedFromMigrations, configurationSource);
        return this;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanExcludeTableFromMigrations(
        bool? excludedFromMigrations,
        ConfigurationSource configurationSource)
        => configurationSource.Overrides(Metadata.GetIsTableExcludedFromMigrationsConfigurationSource())
            || Metadata.IsTableExcludedFromMigrations == excludedFromMigrations;

    /// <inheritdoc />
    IConventionEntityTypeMappingFragment IConventionEntityTypeMappingFragmentBuilder.Metadata
    {
        [DebuggerStepThrough]
        get => Metadata;
    }
}

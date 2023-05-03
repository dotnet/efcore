// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class InternalRelationalPropertyOverridesBuilder :
    AnnotatableBuilder<RelationalPropertyOverrides, IConventionModelBuilder>,
    IConventionRelationalPropertyOverridesBuilder
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public InternalRelationalPropertyOverridesBuilder(
        RelationalPropertyOverrides overrides,
        IConventionModelBuilder modelBuilder)
        : base(overrides, modelBuilder)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalRelationalPropertyOverridesBuilder? HasColumnName(
        string? name,
        ConfigurationSource configurationSource)
    {
        if (!CanSetColumnName(name, configurationSource))
        {
            return null;
        }

        Metadata.SetColumnName(name, configurationSource);

        return this;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSetColumnName(
        string? name,
        ConfigurationSource configurationSource)
        => configurationSource.Overrides(Metadata.GetColumnNameConfigurationSource())
            || Metadata.ColumnName == name;

    /// <inheritdoc />
    IConventionRelationalPropertyOverrides IConventionRelationalPropertyOverridesBuilder.Metadata
    {
        [DebuggerStepThrough]
        get => Metadata;
    }
}

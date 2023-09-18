// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class InternalStoredProcedureResultColumnBuilder :
    AnnotatableBuilder<StoredProcedureResultColumn, IConventionModelBuilder>,
    IConventionStoredProcedureResultColumnBuilder
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public InternalStoredProcedureResultColumnBuilder(
        StoredProcedureResultColumn resultColumn,
        IConventionModelBuilder modelBuilder)
        : base(resultColumn, modelBuilder)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalStoredProcedureResultColumnBuilder? HasName(
        string name,
        ConfigurationSource configurationSource)
    {
        if (!CanSetName(name, configurationSource))
        {
            return null;
        }

        Metadata.SetName(name, configurationSource);

        return this;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSetName(
        string? name,
        ConfigurationSource configurationSource)
        => configurationSource.Overrides(Metadata.GetNameConfigurationSource())
            || Metadata.Name == name;

    /// <inheritdoc />
    IConventionStoredProcedureResultColumn IConventionStoredProcedureResultColumnBuilder.Metadata
    {
        [DebuggerStepThrough]
        get => Metadata;
    }

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionStoredProcedureResultColumnBuilder? IConventionStoredProcedureResultColumnBuilder.HasAnnotation(
        string name,
        object? value,
        bool fromDataAnnotation)
        => (IConventionStoredProcedureResultColumnBuilder?)base.HasAnnotation(
            name, value, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionStoredProcedureResultColumnBuilder? IConventionStoredProcedureResultColumnBuilder.HasNonNullAnnotation(
        string name,
        object? value,
        bool fromDataAnnotation)
        => (IConventionStoredProcedureResultColumnBuilder?)base.HasNonNullAnnotation(
            name, value, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionStoredProcedureResultColumnBuilder? IConventionStoredProcedureResultColumnBuilder.HasNoAnnotation(
        string name,
        bool fromDataAnnotation)
        => (IConventionStoredProcedureResultColumnBuilder?)base.HasNoAnnotation(
            name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionStoredProcedureResultColumnBuilder? IConventionStoredProcedureResultColumnBuilder.HasName(
        string name,
        bool fromDataAnnotation)
        => HasName(name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionStoredProcedureResultColumnBuilder.CanSetName(string? name, bool fromDataAnnotation)
        => CanSetName(name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
}

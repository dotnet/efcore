// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerFullTextCatalog(
    string name,
    IReadOnlyModel model,
    ConfigurationSource configurationSource)
    : ConventionAnnotatable, IMutableSqlServerFullTextCatalog, IConventionSqlServerFullTextCatalog, ISqlServerFullTextCatalog
{
    private bool? _isDefault;
    private bool? _isAccentSensitive;

    private ConfigurationSource _configurationSource = configurationSource;
    private ConfigurationSource? _isDefaultConfigurationSource;
    private ConfigurationSource? _isAccentSensitiveConfigurationSource;


    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IEnumerable<SqlServerFullTextCatalog> GetFullTextCatalogs(IReadOnlyAnnotatable model)
        => ((Dictionary<string, SqlServerFullTextCatalog>?)model[SqlServerAnnotationNames.FullTextCatalogs])
            ?.OrderBy(t => t.Key).Select(t => t.Value)
            ?? [];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static SqlServerFullTextCatalog? FindFullTextCatalog(IReadOnlyAnnotatable model, string name)
    {
        var catalogs = (Dictionary<string, SqlServerFullTextCatalog>?)model[SqlServerAnnotationNames.FullTextCatalogs];

        return catalogs == null || !catalogs.TryGetValue(name, out var catalog)
            ? null
            : catalog;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static SqlServerFullTextCatalog AddFullTextCatalog(
        IMutableModel model,
        string name,
        ConfigurationSource configurationSource)
    {
        var catalog = new SqlServerFullTextCatalog(name, model, configurationSource);
        var catalogs = (Dictionary<string, SqlServerFullTextCatalog>?)model[SqlServerAnnotationNames.FullTextCatalogs];
        if (catalogs == null)
        {
            catalogs = [];
            model[SqlServerAnnotationNames.FullTextCatalogs] = catalogs;
        }

        catalogs.Add(name, catalog);
        return catalog;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static SqlServerFullTextCatalog? RemoveFullTextCatalog(IMutableModel model, string name)
    {
        var catalogs = (Dictionary<string, SqlServerFullTextCatalog>?)model[SqlServerAnnotationNames.FullTextCatalogs];
        if (catalogs == null || !catalogs.TryGetValue(name, out var catalog))
        {
            return null;
        }

        catalogs.Remove(name);
        return catalog;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyModel Model { get; } = model;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override bool IsReadOnly
        => Model is Annotatable annotatable && annotatable.IsReadOnly;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Name { get; } = name;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource GetConfigurationSource()
        => _configurationSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void UpdateConfigurationSource(ConfigurationSource configurationSource)
        => _configurationSource = _configurationSource.Max(configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsDefault
    {
        get => _isDefault ?? false;
        set => SetIsDefault(value, ConfigurationSource.Explicit);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool? SetIsDefault(bool? isDefault, ConfigurationSource configurationSource)
    {
        EnsureMutable();

        _isDefault = isDefault;

        _isDefaultConfigurationSource = isDefault == null
            ? null
            : configurationSource.Max(_isDefaultConfigurationSource);

        return isDefault;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? GetIsDefaultConfigurationSource()
        => _isDefaultConfigurationSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsAccentSensitive
    {
        get => _isAccentSensitive ?? true;
        set => SetIsAccentSensitive(value, ConfigurationSource.Explicit);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool? SetIsAccentSensitive(bool? isAccentSensitive, ConfigurationSource configurationSource)
    {
        EnsureMutable();

        _isAccentSensitive = isAccentSensitive;

        _isAccentSensitiveConfigurationSource = isAccentSensitive == null
            ? null
            : configurationSource.Max(_isAccentSensitiveConfigurationSource);

        return isAccentSensitive;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? GetIsAccentSensitiveConfigurationSource()
        => _isAccentSensitiveConfigurationSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IMutableModel IMutableSqlServerFullTextCatalog.Model
    {
        [DebuggerStepThrough]
        get => (IMutableModel)Model;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionModel IConventionSqlServerFullTextCatalog.Model
    {
        [DebuggerStepThrough]
        get => (IConventionModel)Model;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IModel ISqlServerFullTextCatalog.Model
    {
        [DebuggerStepThrough]
        get => (IModel)Model;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    bool? IConventionSqlServerFullTextCatalog.SetIsDefault(bool? isDefault, bool fromDataAnnotation)
        => SetIsDefault(isDefault, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    bool? IConventionSqlServerFullTextCatalog.SetIsAccentSensitive(bool? isAccentSensitive, bool fromDataAnnotation)
        => SetIsAccentSensitive(isAccentSensitive, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
}

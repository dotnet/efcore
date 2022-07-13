// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class Trigger : ConventionAnnotatable, IMutableTrigger, IConventionTrigger, ITrigger
{
    private string? _name;
    private string? _tableName;
    private string? _tableSchema;
    private InternalTriggerBuilder? _builder;

    private ConfigurationSource _configurationSource;
    private ConfigurationSource? _nameConfigurationSource;
    private ConfigurationSource? _tableNameConfigurationSource;
    private ConfigurationSource? _tableSchemaConfigurationSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public Trigger(
        IMutableEntityType entityType,
        string name,
        string? tableName,
        string? tableSchema,
        ConfigurationSource configurationSource)
    {
        EntityType = entityType;
        ModelName = name;
        _tableName = tableName;
        _tableSchema = tableSchema;
        _configurationSource = configurationSource;

        var triggers = GetTriggersDictionary(entityType);
        if (triggers == null)
        {
            triggers = new SortedDictionary<string, ITrigger>(StringComparer.Ordinal);
            entityType.SetOrRemoveAnnotation(RelationalAnnotationNames.Triggers, triggers);
        }

        if (triggers.ContainsKey(name))
        {
            throw new InvalidOperationException(
                RelationalStrings.DuplicateTrigger(
                    name, entityType.DisplayName(), entityType.DisplayName()));
        }

        var baseTrigger = entityType.BaseType?.FindTrigger(name);
        if (baseTrigger != null)
        {
            throw new InvalidOperationException(
                RelationalStrings.DuplicateTrigger(
                    name, entityType.DisplayName(), baseTrigger.EntityType.DisplayName()));
        }

        foreach (var derivedType in entityType.GetDerivedTypes())
        {
            var derivedTrigger = FindTrigger(derivedType, name);
            if (derivedTrigger != null)
            {
                throw new InvalidOperationException(
                    RelationalStrings.DuplicateTrigger(
                        name, entityType.DisplayName(), derivedTrigger.EntityType.DisplayName()));
            }
        }

        if (entityType.GetTableName() is null)
        {
            throw new InvalidOperationException(RelationalStrings.TriggerOnUnmappedEntityType(name, entityType.DisplayName()));
        }

        EnsureMutable();

        triggers.Add(name, this);

        _builder = new InternalTriggerBuilder(this, ((IConventionModel)entityType.Model).Builder);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IEnumerable<IReadOnlyTrigger> GetDeclaredTriggers(IReadOnlyEntityType entityType)
        => GetTriggersDictionary(entityType)?.Values ?? Enumerable.Empty<ITrigger>();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IEnumerable<IReadOnlyTrigger> GetTriggers(IReadOnlyEntityType entityType)
        => entityType.BaseType != null
            ? GetTriggers(entityType.BaseType).Concat(GetDeclaredTriggers(entityType))
            : GetDeclaredTriggers(entityType);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IReadOnlyTrigger? FindTrigger(
        IReadOnlyEntityType entityType,
        string name)
        => entityType.BaseType?.FindTrigger(name) ?? FindDeclaredTrigger(entityType, name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IReadOnlyTrigger? FindDeclaredTrigger(IReadOnlyEntityType entityType, string name)
    {
        var triggers = (SortedDictionary<string, ITrigger>?)entityType[RelationalAnnotationNames.Triggers];

        return triggers is not null && triggers.TryGetValue(name, out var trigger)
            ? trigger
            : null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static Trigger? RemoveTrigger(IMutableEntityType entityType, string name)
    {
        var triggers = (SortedDictionary<string, ITrigger>?)entityType[RelationalAnnotationNames.Triggers];
        if (triggers == null
            || !triggers.TryGetValue(name, out var trigger))
        {
            return null;
        }

        var mutableTrigger = (Trigger)trigger;
        triggers.Remove(name);
        mutableTrigger.SetRemovedFromModel();

        return mutableTrigger;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static void Attach(IConventionEntityType entityType, IConventionTrigger detachedTrigger)
    {
        var newTrigger = new Trigger(
            (IMutableEntityType)entityType,
            detachedTrigger.ModelName,
            detachedTrigger.TableName,
            detachedTrigger.TableSchema,
            detachedTrigger.GetConfigurationSource());

        MergeInto(detachedTrigger, newTrigger);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static void MergeInto(IConventionTrigger detachedTrigger, IConventionTrigger existingTrigger)
    {
        var nameConfigurationSource = detachedTrigger.GetNameConfigurationSource();
        if (nameConfigurationSource != null)
        {
            ((InternalTriggerBuilder)existingTrigger.Builder).HasName(
                detachedTrigger.Name, nameConfigurationSource.Value);
        }

        ((InternalTriggerBuilder)existingTrigger.Builder).MergeAnnotationsFrom(
            (Trigger)detachedTrigger);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalTriggerBuilder Builder
    {
        [DebuggerStepThrough]
        get => _builder ?? throw new InvalidOperationException(CoreStrings.ObjectRemovedFromModel);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsInModel
        => _builder is not null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void SetRemovedFromModel()
        => _builder = null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyEntityType EntityType { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string ModelName { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? Name
    {
        get => EntityType.GetTableName() == null
            ? null
            : _name ?? ((IReadOnlyTrigger)this).GetDefaultName();
        set => SetName(value, ConfigurationSource.Explicit);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? GetName(in StoreObjectIdentifier storeObject)
        => storeObject.StoreObjectType == StoreObjectType.Table
                && TableName == storeObject.Name
                && TableSchema == storeObject.Schema
            ? _name ?? ((IReadOnlyTrigger)this).GetDefaultName(storeObject)
            : null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? SetName(string? name, ConfigurationSource configurationSource)
    {
        EnsureMutable();

        _name = name;

        _nameConfigurationSource = configurationSource.Max(_nameConfigurationSource);

        return name;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? GetNameConfigurationSource()
        => _nameConfigurationSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string TableName
    {
        get => _tableName ?? EntityType.GetTableName()!;
        set => SetTableName(value, ConfigurationSource.Explicit);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? SetTableName(string? tableName, ConfigurationSource configurationSource)
    {
        EnsureMutable();

        _tableName = tableName;

        _tableNameConfigurationSource = configurationSource.Max(_tableNameConfigurationSource);

        return tableName;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? GetTableNameConfigurationSource()
        => _tableNameConfigurationSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? TableSchema
    {
        get => _tableSchema ?? EntityType.GetSchema();
        set => SetTableSchema(value, ConfigurationSource.Explicit);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? SetTableSchema(string? tableSchema, ConfigurationSource configurationSource)
    {
        EnsureMutable();

        _tableSchema = tableSchema;

        _tableSchemaConfigurationSource = configurationSource.Max(_tableSchemaConfigurationSource);

        return tableSchema;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? GetTableSchemaConfigurationSource()
        => _tableSchemaConfigurationSource;

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

    private static SortedDictionary<string, ITrigger>? GetTriggersDictionary(IReadOnlyEntityType entityType)
        => (SortedDictionary<string, ITrigger>?)entityType[RelationalAnnotationNames.Triggers];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string ToString()
        => ((ITrigger)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual DebugView DebugView
        => new(
            () => ((ITrigger)this).ToDebugString(),
            () => ((ITrigger)this).ToDebugString(MetadataDebugStringOptions.LongDefault));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionEntityType IConventionTrigger.EntityType
    {
        [DebuggerStepThrough]
        get => (IConventionEntityType)EntityType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IMutableEntityType IMutableTrigger.EntityType
    {
        [DebuggerStepThrough]
        get => (IMutableEntityType)EntityType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IEntityType ITrigger.EntityType
    {
        [DebuggerStepThrough]
        get => (IEntityType)EntityType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    string ITrigger.Name
    {
        [DebuggerStepThrough]
        get => Name!;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionTriggerBuilder IConventionTrigger.Builder
    {
        [DebuggerStepThrough]
        get => Builder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    string? IConventionTrigger.SetName(string? name, bool fromDataAnnotation)
        => SetName(name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
}

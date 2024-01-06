// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class StoredProcedure :
    ConventionAnnotatable,
    IRuntimeStoredProcedure,
    IMutableStoredProcedure,
    IConventionStoredProcedure
{
    private readonly List<StoredProcedureParameter> _parameters = [];
    private readonly Dictionary<string, StoredProcedureParameter> _currentValueParameters = new();
    private readonly Dictionary<string, StoredProcedureParameter> _originalValueParameters = new();
    private StoredProcedureParameter? _rowsAffectedParameter;
    private readonly List<StoredProcedureResultColumn> _resultColumns = [];
    private StoredProcedureResultColumn? _rowsAffectedResultColumn;
    private readonly Dictionary<string, StoredProcedureResultColumn> _propertyResultColumns = new();
    private string? _schema;
    private string? _name;
    private InternalStoredProcedureBuilder? _builder;
    private bool _isRowsAffectedReturned;
    private IStoreStoredProcedure? _storeStoredProcedure;

    private ConfigurationSource _configurationSource;
    private ConfigurationSource? _schemaConfigurationSource;
    private ConfigurationSource? _nameConfigurationSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public StoredProcedure(
        IMutableEntityType entityType,
        ConfigurationSource configurationSource)
    {
        EntityType = entityType;
        _configurationSource = configurationSource;
        _builder = new InternalStoredProcedureBuilder(this, ((IConventionEntityType)entityType).Model.Builder);
    }

    /// <inheritdoc />
    public virtual IMutableEntityType EntityType { get; set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalStoredProcedureBuilder Builder
    {
        [DebuggerStepThrough]
        get => _builder ?? throw new InvalidOperationException(CoreStrings.ObjectRemovedFromModel(Name));
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
    ///     Indicates whether the function is read-only.
    /// </summary>
    public override bool IsReadOnly
        => ((Annotatable)EntityType).IsReadOnly;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IStoredProcedure? FindStoredProcedure(
        IReadOnlyEntityType entityType,
        StoreObjectType sprocType)
    {
        var storedProcedure = FindDeclaredStoredProcedure(entityType, sprocType);
        if (storedProcedure != null)
        {
            return storedProcedure;
        }

        if ((entityType.GetMappingStrategy() ?? RelationalAnnotationNames.TphMappingStrategy)
            == RelationalAnnotationNames.TphMappingStrategy
            && entityType.BaseType != null)
        {
            return FindStoredProcedure(entityType.GetRootType(), sprocType);
        }

        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IStoredProcedure? FindDeclaredStoredProcedure(
        IReadOnlyEntityType entityType,
        StoreObjectType sprocType)
    {
        var sprocAnnotation = entityType.FindAnnotation(GetAnnotationName(sprocType));
        return sprocAnnotation != null ? (IStoredProcedure?)sprocAnnotation.Value : null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static StoredProcedure SetStoredProcedure(
        IMutableEntityType entityType,
        StoreObjectType sprocType)
        => SetStoredProcedure(entityType, sprocType, null, null);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static StoredProcedure SetStoredProcedure(
        IMutableEntityType entityType,
        StoreObjectType sprocType,
        string? name,
        string? schema)
    {
        var oldId = FindDeclaredStoredProcedure(entityType, sprocType)?.GetStoreIdentifier();
        var sproc = new StoredProcedure(entityType, ConfigurationSource.Explicit);
        entityType.SetAnnotation(GetAnnotationName(sprocType), sproc);

        if (name != null)
        {
            sproc.SetName(name, schema, ConfigurationSource.Explicit, skipOverrides: true);
        }

        if (oldId != null)
        {
            UpdateOverrides(oldId.Value, ((IReadOnlyStoredProcedure)sproc).GetStoreIdentifier(), (IConventionEntityType)entityType);
        }

        return sproc;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static StoredProcedure? SetStoredProcedure(
        IConventionEntityType entityType,
        StoreObjectType sprocType,
        bool fromDataAnnotation)
        => SetStoredProcedure(entityType, sprocType, null, null, fromDataAnnotation);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static StoredProcedure? SetStoredProcedure(
        IConventionEntityType entityType,
        StoreObjectType sprocType,
        string? name,
        string? schema,
        bool fromDataAnnotation)
    {
        var oldId = FindDeclaredStoredProcedure(entityType, sprocType)?.GetStoreIdentifier();
        var configurationSource = fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention;
        var sproc = new StoredProcedure((IMutableEntityType)entityType, configurationSource);
        sproc = (StoredProcedure?)entityType.SetAnnotation(GetAnnotationName(sprocType), sproc, fromDataAnnotation)?.Value;

        if (name != null)
        {
            sproc?.SetName(name, schema, configurationSource, skipOverrides: true);
        }

        if (oldId != null
            && sproc != null)
        {
            UpdateOverrides(oldId.Value, ((IReadOnlyStoredProcedure)sproc).GetStoreIdentifier(), entityType);
        }

        return sproc;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IMutableStoredProcedure? RemoveStoredProcedure(IMutableEntityType entityType, StoreObjectType sprocType)
    {
        var oldId = FindDeclaredStoredProcedure(entityType, sprocType)?.GetStoreIdentifier();
        var sproc = (IMutableStoredProcedure?)entityType.RemoveAnnotation(GetAnnotationName(sprocType))?.Value;

        if (oldId != null
            && sproc != null)
        {
            UpdateOverrides(oldId.Value, null, (IConventionEntityType)entityType);
        }

        return sproc;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IConventionStoredProcedure? RemoveStoredProcedure(IConventionEntityType entityType, StoreObjectType sprocType)
    {
        var oldId = FindDeclaredStoredProcedure(entityType, sprocType)?.GetStoreIdentifier();
        var sproc = (IConventionStoredProcedure?)entityType.RemoveAnnotation(GetAnnotationName(sprocType))?.Value;

        if (oldId != null
            && sproc != null)
        {
            UpdateOverrides(oldId.Value, null, entityType);
        }

        return sproc;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static ConfigurationSource? GetStoredProcedureConfigurationSource(
        IConventionEntityType entityType,
        StoreObjectType sprocType)
        => entityType.FindAnnotation(GetAnnotationName(sprocType))
            ?.GetConfigurationSource();

    private static string GetAnnotationName(StoreObjectType sprocType)
        => sprocType switch
        {
            StoreObjectType.InsertStoredProcedure => RelationalAnnotationNames.InsertStoredProcedure,
            StoreObjectType.DeleteStoredProcedure => RelationalAnnotationNames.DeleteStoredProcedure,
            StoreObjectType.UpdateStoredProcedure => RelationalAnnotationNames.UpdateStoredProcedure,
            _ => throw new InvalidOperationException("Unsopported sproc type " + sprocType)
        };

    /// <inheritdoc />
    [DebuggerStepThrough]
    public virtual ConfigurationSource GetConfigurationSource()
        => _configurationSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    public virtual void UpdateConfigurationSource(ConfigurationSource configurationSource)
        => _configurationSource = configurationSource.Max(_configurationSource);

    /// <inheritdoc />
    public virtual string? Schema
    {
        get => _schema ?? EntityType.GetDefaultSchema();
        set => SetSchema(value, ConfigurationSource.Explicit);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? SetSchema(string? schema, ConfigurationSource configurationSource)
    {
        EnsureMutable();

        var oldId = ((IReadOnlyStoredProcedure)this).GetStoreIdentifier();

        _schema = schema;

        _schemaConfigurationSource = configurationSource.Max(_schemaConfigurationSource);

        if (oldId != null)
        {
            UpdateOverrides(oldId.Value, ((IReadOnlyStoredProcedure)this).GetStoreIdentifier(), (IConventionEntityType)EntityType);
        }

        return schema;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? GetSchemaConfigurationSource()
        => _schemaConfigurationSource;

    /// <inheritdoc />
    public virtual string? Name
    {
        get => _name ?? GetDefaultName();
        set => SetName(value, ConfigurationSource.Explicit);
    }

    private string? GetDefaultName()
    {
        var tableName = EntityType.GetTableName() ?? EntityType.GetDefaultTableName();
        if (tableName == null)
        {
            if (_configurationSource == ConfigurationSource.Convention)
            {
                return null;
            }

            tableName = Uniquifier.Truncate(EntityType.ShortName(), EntityType.Model.GetMaxIdentifierLength());
        }

        string? suffix;
        if (EntityType.GetInsertStoredProcedure() == this)
        {
            suffix = "_Insert";
        }
        else if (EntityType.GetDeleteStoredProcedure() == this)
        {
            suffix = "_Delete";
        }
        else if (EntityType.GetUpdateStoredProcedure() == this)
        {
            suffix = "_Update";
        }
        else
        {
            return null;
        }

        return tableName + suffix;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? SetName(string? name, ConfigurationSource configurationSource)
    {
        EnsureMutable();

        var oldId = ((IReadOnlyStoredProcedure)this).GetStoreIdentifier();

        _name = name;

        _nameConfigurationSource = name == null
            ? null
            : configurationSource.Max(_nameConfigurationSource);

        if (oldId != null)
        {
            UpdateOverrides(oldId.Value, ((IReadOnlyStoredProcedure)this).GetStoreIdentifier(), (IConventionEntityType)EntityType);
        }

        return name;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void SetName(string? name, string? schema, ConfigurationSource configurationSource, bool skipOverrides = false)
    {
        EnsureMutable();

        var oldId = ((IReadOnlyStoredProcedure)this).GetStoreIdentifier();

        _name = name;

        _nameConfigurationSource = name == null
            ? null
            : configurationSource.Max(_nameConfigurationSource);

        _schema = schema;

        _schemaConfigurationSource = configurationSource.Max(_schemaConfigurationSource);

        if (!skipOverrides
            && oldId != null)
        {
            UpdateOverrides(oldId.Value, ((IReadOnlyStoredProcedure)this).GetStoreIdentifier(), (IConventionEntityType)EntityType);
        }
    }

    /// <inheritdoc />
    public virtual ConfigurationSource? GetNameConfigurationSource()
        => _nameConfigurationSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsRowsAffectedReturned
    {
        get => _isRowsAffectedReturned;
        set => SetIsRowsAffectedReturned(value);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool SetIsRowsAffectedReturned(bool rowsAffectedReturned)
    {
        EnsureMutable();

        if (_rowsAffectedParameter != null || _rowsAffectedResultColumn != null)
        {
            throw new InvalidOperationException(
                RelationalStrings.StoredProcedureRowsAffectedReturnConflictingParameter(
                    ((IReadOnlyStoredProcedure)this).GetStoreIdentifier()?.DisplayName()));
        }

        _isRowsAffectedReturned = rowsAffectedReturned;

        return rowsAffectedReturned;
    }

    private static void UpdateOverrides(
        StoreObjectIdentifier oldId,
        StoreObjectIdentifier? newId,
        IConventionEntityType entityType)
    {
        if (oldId == newId)
        {
            return;
        }

        var properties = (entityType.GetMappingStrategy() ?? RelationalAnnotationNames.TphMappingStrategy)
            == RelationalAnnotationNames.TphMappingStrategy
                ? entityType.GetProperties().Concat(entityType.GetDerivedProperties())
                : entityType.GetProperties();

        foreach (var property in properties)
        {
            var removedOverrides = property.RemoveOverrides(oldId);
            if (removedOverrides != null
                && newId != null)
            {
                RelationalPropertyOverrides.Attach(property, removedOverrides, newId.Value);
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyList<StoredProcedureParameter> Parameters
    {
        [DebuggerStepThrough]
        get => _parameters;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual StoredProcedureParameter? FindParameter(string propertyName)
        => _currentValueParameters.TryGetValue(propertyName, out var parameter)
            ? parameter
            : null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual StoredProcedureParameter AddParameter(string propertyName)
    {
        if (_currentValueParameters.ContainsKey(propertyName))
        {
            throw new InvalidOperationException(
                RelationalStrings.StoredProcedureDuplicateParameter(
                    propertyName, ((IReadOnlyStoredProcedure)this).GetStoreIdentifier()?.DisplayName()));
        }

        var parameter = new StoredProcedureParameter(this, rowsAffected: false, propertyName, originalValue: false);
        _parameters.Add(parameter);
        _currentValueParameters.Add(propertyName, parameter);

        return parameter;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual StoredProcedureParameter? FindOriginalValueParameter(string propertyName)
        => _originalValueParameters.TryGetValue(propertyName, out var parameter)
            ? parameter
            : null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual StoredProcedureParameter AddOriginalValueParameter(string propertyName)
    {
        if (_originalValueParameters.ContainsKey(propertyName))
        {
            throw new InvalidOperationException(
                RelationalStrings.StoredProcedureDuplicateOriginalValueParameter(
                    propertyName, ((IReadOnlyStoredProcedure)this).GetStoreIdentifier()?.DisplayName()));
        }

        var parameter = new StoredProcedureParameter(this, rowsAffected: false, propertyName, originalValue: true);
        _parameters.Add(parameter);
        _originalValueParameters.Add(propertyName, parameter);

        return parameter;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual StoredProcedureParameter? FindRowsAffectedParameter()
        => _rowsAffectedParameter;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual StoredProcedureParameter AddRowsAffectedParameter()
    {
        if (_rowsAffectedParameter != null
            || _rowsAffectedResultColumn != null
            || _isRowsAffectedReturned)
        {
            throw new InvalidOperationException(
                RelationalStrings.StoredProcedureDuplicateRowsAffectedParameter(
                    ((IReadOnlyStoredProcedure)this).GetStoreIdentifier()?.DisplayName()));
        }

        var parameter = new StoredProcedureParameter(this, rowsAffected: true, propertyName: null, originalValue: null);
        _parameters.Add(parameter);
        _rowsAffectedParameter = parameter;

        return parameter;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyList<StoredProcedureResultColumn> ResultColumns
    {
        [DebuggerStepThrough]
        get => _resultColumns;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual StoredProcedureResultColumn? FindResultColumn(string propertyName)
        => _propertyResultColumns.TryGetValue(propertyName, out var resultColumn)
            ? resultColumn
            : null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual StoredProcedureResultColumn AddResultColumn(string propertyName)
    {
        if (_propertyResultColumns.ContainsKey(propertyName))
        {
            throw new InvalidOperationException(
                RelationalStrings.StoredProcedureDuplicateResultColumn(
                    propertyName, ((IReadOnlyStoredProcedure)this).GetStoreIdentifier()?.DisplayName()));
        }

        var resultColumn = new StoredProcedureResultColumn(this, forRowsAffected: false, propertyName);
        _resultColumns.Add(resultColumn);
        _propertyResultColumns.Add(propertyName, resultColumn);

        return resultColumn;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual StoredProcedureResultColumn? FindRowsAffectedResultColumn()
        => _rowsAffectedResultColumn;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual StoredProcedureResultColumn AddRowsAffectedResultColumn()
    {
        if (_rowsAffectedResultColumn != null
            || _rowsAffectedParameter != null
            || _isRowsAffectedReturned)
        {
            throw new InvalidOperationException(
                RelationalStrings.StoredProcedureDuplicateRowsAffectedResultColumn(
                    ((IReadOnlyStoredProcedure)this).GetStoreIdentifier()?.DisplayName()));
        }

        var resultColumn = new StoredProcedureResultColumn(this, forRowsAffected: true, propertyName: null);
        _resultColumns.Add(resultColumn);
        _rowsAffectedResultColumn = resultColumn;

        return resultColumn;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string ToString()
        => ((IStoredProcedure)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual DebugView DebugView
        => new(
            () => ((IStoredProcedure)this).ToDebugString(),
            () => ((IStoredProcedure)this).ToDebugString(MetadataDebugStringOptions.LongDefault));

    /// <inheritdoc />
    IConventionStoredProcedureBuilder IConventionStoredProcedure.Builder
    {
        [DebuggerStepThrough]
        get => Builder;
    }

    /// <inheritdoc />
    string IStoredProcedure.Name
    {
        [DebuggerStepThrough]
        get => Name!;
    }

    /// <inheritdoc />
    IReadOnlyEntityType IReadOnlyStoredProcedure.EntityType
    {
        [DebuggerStepThrough]
        get => (IConventionEntityType)EntityType;
    }

    /// <inheritdoc />
    IConventionEntityType IConventionStoredProcedure.EntityType
    {
        [DebuggerStepThrough]
        get => (IConventionEntityType)EntityType;
    }

    /// <inheritdoc />
    IEntityType IStoredProcedure.EntityType
    {
        [DebuggerStepThrough]
        get => (IEntityType)EntityType;
    }

    /// <inheritdoc />
    IStoreStoredProcedure IStoredProcedure.StoreStoredProcedure
        => _storeStoredProcedure!; // Relational model creation ensures StoreStoredProcedure is populated

    /// <inheritdoc />
    IStoreStoredProcedure IRuntimeStoredProcedure.StoreStoredProcedure
    {
        get => _storeStoredProcedure!;
        set => _storeStoredProcedure = value;
    }

    /// <inheritdoc />
    IReadOnlyList<IReadOnlyStoredProcedureParameter> IReadOnlyStoredProcedure.Parameters
    {
        [DebuggerStepThrough]
        get => Parameters;
    }

    /// <inheritdoc />
    IReadOnlyList<IMutableStoredProcedureParameter> IMutableStoredProcedure.Parameters
    {
        [DebuggerStepThrough]
        get => Parameters;
    }

    /// <inheritdoc />
    IReadOnlyList<IConventionStoredProcedureParameter> IConventionStoredProcedure.Parameters
    {
        [DebuggerStepThrough]
        get => Parameters;
    }

    /// <inheritdoc />
    IReadOnlyList<IStoredProcedureParameter> IStoredProcedure.Parameters
    {
        [DebuggerStepThrough]
        get => Parameters;
    }

    /// <inheritdoc />
    IReadOnlyList<IReadOnlyStoredProcedureResultColumn> IReadOnlyStoredProcedure.ResultColumns
    {
        [DebuggerStepThrough]
        get => ResultColumns;
    }

    /// <inheritdoc />
    IReadOnlyList<IMutableStoredProcedureResultColumn> IMutableStoredProcedure.ResultColumns
    {
        [DebuggerStepThrough]
        get => ResultColumns;
    }

    /// <inheritdoc />
    IReadOnlyList<IConventionStoredProcedureResultColumn> IConventionStoredProcedure.ResultColumns
    {
        [DebuggerStepThrough]
        get => ResultColumns;
    }

    /// <inheritdoc />
    IReadOnlyList<IStoredProcedureResultColumn> IStoredProcedure.ResultColumns
    {
        [DebuggerStepThrough]
        get => ResultColumns;
    }

    /// <inheritdoc />
    [DebuggerStepThrough]
    string? IConventionStoredProcedure.SetName(string? name, bool fromDataAnnotation)
        => SetName(name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    string? IConventionStoredProcedure.SetSchema(string? schema, bool fromDataAnnotation)
        => SetSchema(schema, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionStoredProcedure.SetIsRowsAffectedReturned(bool rowsAffectedReturned, bool fromDataAnnotation)
        => SetIsRowsAffectedReturned(rowsAffectedReturned);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IReadOnlyStoredProcedureParameter? IReadOnlyStoredProcedure.FindParameter(string propertyName)
        => FindParameter(propertyName);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IMutableStoredProcedureParameter? IMutableStoredProcedure.FindParameter(string propertyName)
        => FindParameter(propertyName);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionStoredProcedureParameter? IConventionStoredProcedure.FindParameter(string propertyName)
        => FindParameter(propertyName);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IStoredProcedureParameter? IStoredProcedure.FindParameter(string propertyName)
        => FindParameter(propertyName);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionStoredProcedureParameter? IConventionStoredProcedure.AddParameter(string propertyName, bool fromDataAnnotation)
        => AddParameter(propertyName);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IMutableStoredProcedureParameter IMutableStoredProcedure.AddParameter(string propertyName)
        => AddParameter(propertyName);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IReadOnlyStoredProcedureParameter? IReadOnlyStoredProcedure.FindOriginalValueParameter(string propertyName)
        => FindOriginalValueParameter(propertyName);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IMutableStoredProcedureParameter? IMutableStoredProcedure.FindOriginalValueParameter(string propertyName)
        => FindOriginalValueParameter(propertyName);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionStoredProcedureParameter? IConventionStoredProcedure.FindOriginalValueParameter(string propertyName)
        => FindOriginalValueParameter(propertyName);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IStoredProcedureParameter? IStoredProcedure.FindOriginalValueParameter(string propertyName)
        => FindOriginalValueParameter(propertyName);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IMutableStoredProcedureParameter IMutableStoredProcedure.AddOriginalValueParameter(string propertyName)
        => AddOriginalValueParameter(propertyName);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionStoredProcedureParameter? IConventionStoredProcedure.AddOriginalValueParameter(
        string propertyName,
        bool fromDataAnnotation)
        => AddOriginalValueParameter(propertyName);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IReadOnlyStoredProcedureParameter? IReadOnlyStoredProcedure.FindRowsAffectedParameter()
        => FindRowsAffectedParameter();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IMutableStoredProcedureParameter? IMutableStoredProcedure.FindRowsAffectedParameter()
        => FindRowsAffectedParameter();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionStoredProcedureParameter? IConventionStoredProcedure.FindRowsAffectedParameter()
        => FindRowsAffectedParameter();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IStoredProcedureParameter? IStoredProcedure.FindRowsAffectedParameter()
        => FindRowsAffectedParameter();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IMutableStoredProcedureParameter IMutableStoredProcedure.AddRowsAffectedParameter()
        => AddRowsAffectedParameter();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionStoredProcedureParameter? IConventionStoredProcedure.AddRowsAffectedParameter(bool fromDataAnnotation)
        => AddRowsAffectedParameter();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IReadOnlyStoredProcedureResultColumn? IReadOnlyStoredProcedure.FindResultColumn(string propertyName)
        => FindResultColumn(propertyName);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IMutableStoredProcedureResultColumn? IMutableStoredProcedure.FindResultColumn(string propertyName)
        => FindResultColumn(propertyName);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionStoredProcedureResultColumn? IConventionStoredProcedure.FindResultColumn(string propertyName)
        => FindResultColumn(propertyName);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IStoredProcedureResultColumn? IStoredProcedure.FindResultColumn(string propertyName)
        => FindResultColumn(propertyName);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IMutableStoredProcedureResultColumn IMutableStoredProcedure.AddResultColumn(string propertyName)
        => AddResultColumn(propertyName);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionStoredProcedureResultColumn? IConventionStoredProcedure.AddResultColumn(
        string propertyName,
        bool fromDataAnnotation)
        => AddResultColumn(propertyName);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IReadOnlyStoredProcedureResultColumn? IReadOnlyStoredProcedure.FindRowsAffectedResultColumn()
        => FindRowsAffectedResultColumn();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IMutableStoredProcedureResultColumn? IMutableStoredProcedure.FindRowsAffectedResultColumn()
        => FindRowsAffectedResultColumn();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionStoredProcedureResultColumn? IConventionStoredProcedure.FindRowsAffectedResultColumn()
        => FindRowsAffectedResultColumn();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IStoredProcedureResultColumn? IStoredProcedure.FindRowsAffectedResultColumn()
        => FindRowsAffectedResultColumn();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IMutableStoredProcedureResultColumn IMutableStoredProcedure.AddRowsAffectedResultColumn()
        => AddRowsAffectedResultColumn();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionStoredProcedureResultColumn? IConventionStoredProcedure.AddRowsAffectedResultColumn(bool fromDataAnnotation)
        => AddRowsAffectedResultColumn();
}

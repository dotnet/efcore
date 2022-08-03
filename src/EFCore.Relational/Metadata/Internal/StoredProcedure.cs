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
    ConventionAnnotatable, IRuntimeStoredProcedure, IMutableStoredProcedure, IConventionStoredProcedure
{
    private readonly List<string> _parameters = new();
    private readonly HashSet<string> _parametersSet = new();
    private readonly List<string> _resultColumns = new();
    private readonly HashSet<string> _resultColumnsSet = new();
    private string? _schema;
    private string? _name;
    private InternalStoredProcedureBuilder? _builder;
    private bool _areTransactionsSuppressed;
    private IStoreStoredProcedure? _storeStoredProcedure;

    private ConfigurationSource _configurationSource;
    private ConfigurationSource? _schemaConfigurationSource;
    private ConfigurationSource? _nameConfigurationSource;
    private ConfigurationSource? _areTransactionsSuppressedConfigurationSource;

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
        _builder = new(this, ((IConventionEntityType)entityType).Model.Builder);
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
    {
        var oldId = FindDeclaredStoredProcedure(entityType, sprocType)?.GetStoreIdentifier();
        var sproc = new StoredProcedure(entityType, ConfigurationSource.Explicit);
        entityType.SetAnnotation(GetAnnotationName(sprocType), sproc);

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
    public static StoredProcedure SetStoredProcedure(
        IMutableEntityType entityType,
        StoreObjectType sprocType,
        string name,
        string? schema)
    {
        var oldId = FindDeclaredStoredProcedure(entityType, sprocType)?.GetStoreIdentifier();
        var sproc = new StoredProcedure(entityType, ConfigurationSource.Explicit);
        entityType.SetAnnotation(GetAnnotationName(sprocType), sproc);
        sproc.SetName(name, schema, ConfigurationSource.Explicit, skipOverrides: true);

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
    {
        var oldId = FindDeclaredStoredProcedure(entityType, sprocType)?.GetStoreIdentifier();
        var sproc = new StoredProcedure(
            (IMutableEntityType)entityType,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
        sproc = (StoredProcedure?)entityType.SetAnnotation(GetAnnotationName(sprocType), sproc)?.Value;

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
    public static StoredProcedure? SetStoredProcedure(
        IConventionEntityType entityType,
        StoreObjectType sprocType,
        string name,
        string? schema,
        bool fromDataAnnotation)
    {
        var oldId = FindDeclaredStoredProcedure(entityType, sprocType)?.GetStoreIdentifier();
        var configurationSource = fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention;
        var sproc = new StoredProcedure((IMutableEntityType)entityType, configurationSource);
        sproc = (StoredProcedure?)entityType.SetAnnotation(GetAnnotationName(sprocType), sproc)?.Value;

        sproc?.SetName(name, schema, configurationSource, skipOverrides: true);

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
        IConventionEntityType entityType, StoreObjectType sprocType)
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
            return null;
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

    /// <inheritdoc />
    public virtual bool AreTransactionsSuppressed
    {
        get => _areTransactionsSuppressed;
        set => SetAreTransactionsSuppressed(value, ConfigurationSource.Explicit);
    }
    
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool SetAreTransactionsSuppressed(bool areTransactionsSuppressed, ConfigurationSource configurationSource)
    {
        EnsureMutable();

        _areTransactionsSuppressed = areTransactionsSuppressed;

        _areTransactionsSuppressedConfigurationSource = configurationSource.Max(_areTransactionsSuppressedConfigurationSource);

        return areTransactionsSuppressed;
    }
    
    /// <inheritdoc />
    public virtual ConfigurationSource? GetAreTransactionsSuppressedConfigurationSource()
        => _areTransactionsSuppressedConfigurationSource;

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

    /// <inheritdoc />
    public virtual IReadOnlyList<string> Parameters
    {
        [DebuggerStepThrough]
        get => _parameters;
    }

    /// <inheritdoc />
    public virtual bool ContainsParameter(string propertyName)
        => _parametersSet.Contains(propertyName);

    /// <inheritdoc />
    public virtual bool AddParameter(string propertyName)
    {
        if (!_parametersSet.Contains(propertyName))
        {
            _parameters.Add(propertyName);
            _parametersSet.Add(propertyName);

            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public virtual IReadOnlyList<string> ResultColumns
    {
        [DebuggerStepThrough]
        get => _resultColumns;
    }

    /// <inheritdoc />
    public virtual bool ContainsResultColumn(string propertyName)
        => _resultColumnsSet.Contains(propertyName);

    /// <inheritdoc />
    public virtual bool AddResultColumn(string propertyName)
    {
        if (!_resultColumnsSet.Contains(propertyName))
        {
            _resultColumns.Add(propertyName);
            _resultColumnsSet.Add(propertyName);
            
            return true;
        }

        return false;
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
    [EntityFrameworkInternal]
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
    [DebuggerStepThrough]
    string? IConventionStoredProcedure.SetName(string? name, bool fromDataAnnotation)
        => SetName(name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    string? IConventionStoredProcedure.SetSchema(string? schema, bool fromDataAnnotation)
        => SetSchema(schema, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    string? IConventionStoredProcedure.AddParameter(string propertyName, bool fromDataAnnotation)
        => AddParameter(propertyName) ? propertyName : null;

    /// <inheritdoc />
    [DebuggerStepThrough]
    string? IConventionStoredProcedure.AddResultColumn(string propertyName, bool fromDataAnnotation)
        => AddResultColumn(propertyName) ? propertyName : null;

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionStoredProcedure.SetAreTransactionsSuppressed(bool areTransactionsSuppressed, bool fromDataAnnotation)
        => SetAreTransactionsSuppressed(
            areTransactionsSuppressed, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
}

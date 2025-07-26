// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Data;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using IColumnMapping = Microsoft.EntityFrameworkCore.Metadata.IColumnMapping;
using ITableMapping = Microsoft.EntityFrameworkCore.Metadata.ITableMapping;

namespace Microsoft.EntityFrameworkCore.Update;

/// <summary>
///     <para>
///         Represents a conceptual command to the database to insert/update/delete a row.
///     </para>
///     <para>
///         This type is typically used by database providers; it is generally not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
public class ModificationCommand : IModificationCommand, INonTrackedModificationCommand
{
    private readonly Func<string>? _generateParameterName;
    private readonly bool _sensitiveLoggingEnabled;
    private readonly bool _detailedErrorsEnabled;
    private readonly IComparer<IUpdateEntry>? _comparer;
    private readonly List<IUpdateEntry> _entries = [];
    private List<IColumnModification>? _columnModifications;
    private bool _mainEntryAdded;
    private EntityState _entityState;
    private readonly IDiagnosticsLogger<DbLoggerCategory.Update>? _logger;

    /// <summary>
    ///     Initializes a new <see cref="ModificationCommand" /> instance.
    /// </summary>
    /// <param name="modificationCommandParameters">Creation parameters.</param>
    public ModificationCommand(in ModificationCommandParameters modificationCommandParameters)
    {
        Table = modificationCommandParameters.Table;
        TableName = modificationCommandParameters.TableName;
        Schema = modificationCommandParameters.Schema;
        StoreStoredProcedure = modificationCommandParameters.StoreStoredProcedure;
        _generateParameterName = modificationCommandParameters.GenerateParameterName;
        _sensitiveLoggingEnabled = modificationCommandParameters.SensitiveLoggingEnabled;
        _detailedErrorsEnabled = modificationCommandParameters.DetailedErrorsEnabled;
        _comparer = modificationCommandParameters.Comparer;
        _logger = modificationCommandParameters.Logger;
        EntityState = EntityState.Modified;
    }

    /// <summary>
    ///     Initializes a new <see cref="ModificationCommand" /> instance.
    /// </summary>
    /// <param name="modificationCommandParameters">Creation parameters.</param>
    public ModificationCommand(in NonTrackedModificationCommandParameters modificationCommandParameters)
    {
        Table = modificationCommandParameters.Table;
        TableName = modificationCommandParameters.TableName;
        Schema = modificationCommandParameters.Schema;
        _sensitiveLoggingEnabled = modificationCommandParameters.SensitiveLoggingEnabled;
        EntityState = EntityState.Modified;
    }

    /// <inheritdoc />
    public virtual ITable? Table { get; }

    /// <inheritdoc />
    public virtual IStoreStoredProcedure? StoreStoredProcedure { get; }

    /// <inheritdoc />
    public virtual string TableName { get; }

    /// <inheritdoc />
    public virtual string? Schema { get; }

    /// <inheritdoc />
    public virtual IReadOnlyList<IUpdateEntry> Entries
        => _entries;

    /// <inheritdoc />
    public virtual EntityState EntityState
    {
        get => _entityState;
        set => _entityState = value;
    }

    /// <inheritdoc />
    public virtual IColumnBase? RowsAffectedColumn { get; private set; }

    /// <summary>
    ///     The list of <see cref="IColumnModification" /> needed to perform the insert, update, or delete.
    /// </summary>
    public virtual IReadOnlyList<IColumnModification> ColumnModifications
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _columnModifications, this, static command => command.GenerateColumnModifications());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [Conditional("DEBUG")]
    [EntityFrameworkInternal]
    public virtual void AssertColumnsNotInitialized()
    {
        if (_columnModifications != null
            && !Debugger.IsAttached)
        {
            throw new Exception("_columnModifications have been initialized prematurely");
        }
    }

    /// <inheritdoc />
    public virtual void AddEntry(IUpdateEntry entry, bool mainEntry)
    {
        AssertColumnsNotInitialized();

        switch (entry.EntityState)
        {
            case EntityState.Deleted:
            case EntityState.Modified:
            case EntityState.Added:
                break;
            default:
                if (_sensitiveLoggingEnabled)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.ModificationCommandInvalidEntityStateSensitive(
                            entry.EntityType.DisplayName(),
                            entry.BuildCurrentValuesString(entry.EntityType.FindPrimaryKey()!.Properties),
                            entry.EntityState));
                }

                throw new InvalidOperationException(
                    RelationalStrings.ModificationCommandInvalidEntityState(
                        entry.EntityType.DisplayName(),
                        entry.EntityState));
        }

        if (mainEntry)
        {
            Check.DebugAssert(!_mainEntryAdded, "Only expected a single main entry");

            for (var i = 0; i < _entries.Count; i++)
            {
                ValidateState(entry, _entries[i]);
            }

            _mainEntryAdded = true;
            _entries.Insert(0, entry);

            _entityState = entry.SharedIdentityEntry == null
                ? entry.EntityState
                : entry.SharedIdentityEntry.EntityType == entry.EntityType
                || entry.SharedIdentityEntry.EntityType.GetTableMappings()
                    .Any(m => m.Table.Name == TableName && m.Table.Schema == Schema)
                    ? EntityState.Modified
                    : entry.EntityState;
        }
        else
        {
            if (_mainEntryAdded)
            {
                ValidateState(_entries[0], entry);
            }

            _entries.Add(entry);
        }
    }

    private void ValidateState(IUpdateEntry mainEntry, IUpdateEntry entry)
    {
        var mainEntryState = mainEntry.SharedIdentityEntry == null
            ? mainEntry.EntityState
            : EntityState.Modified;
        if (mainEntryState == EntityState.Modified)
        {
            return;
        }

        var entryState = entry.SharedIdentityEntry == null
            ? entry.EntityState
            : EntityState.Modified;
        if (mainEntryState != entryState)
        {
            if (_sensitiveLoggingEnabled)
            {
                throw new InvalidOperationException(
                    RelationalStrings.ConflictingRowUpdateTypesSensitive(
                        entry.EntityType.DisplayName(),
                        entry.BuildCurrentValuesString(entry.EntityType.FindPrimaryKey()!.Properties),
                        entryState,
                        mainEntry.EntityType.DisplayName(),
                        mainEntry.BuildCurrentValuesString(mainEntry.EntityType.FindPrimaryKey()!.Properties),
                        mainEntryState));
            }

            throw new InvalidOperationException(
                RelationalStrings.ConflictingRowUpdateTypes(
                    entry.EntityType.DisplayName(),
                    entryState,
                    mainEntry.EntityType.DisplayName(),
                    mainEntryState));
        }
    }

    /// <summary>
    ///     Creates a new <see cref="IColumnModification" /> and add it to this command.
    /// </summary>
    /// <param name="columnModificationParameters">Creation parameters.</param>
    /// <returns>The new <see cref="IColumnModification" /> instance.</returns>
    public virtual IColumnModification AddColumnModification(in ColumnModificationParameters columnModificationParameters)
    {
        var modification = CreateColumnModification(columnModificationParameters);

        _columnModifications ??= [];

        _columnModifications.Add(modification);

        return modification;
    }

    /// <summary>
    ///     Creates a new instance that implements <see cref="IColumnModification" /> interface.
    /// </summary>
    /// <param name="columnModificationParameters">Creation parameters.</param>
    /// <returns>The new instance that implements <see cref="IColumnModification" /> interface.</returns>
    protected virtual IColumnModification CreateColumnModification(in ColumnModificationParameters columnModificationParameters)
        => new ColumnModification(columnModificationParameters);

    private record struct JsonPartialUpdatePathEntry(string PropertyName, int? Ordinal, IUpdateEntry ParentEntry, IPropertyBase Property);

    private List<IColumnModification> GenerateColumnModifications()
    {
        var state = EntityState;
        var adding = state == EntityState.Added;
        var updating = state == EntityState.Modified;
        var deleting = state == EntityState.Deleted;
        var columnModifications = new List<IColumnModification>();
        Dictionary<string, ColumnValuePropagator>? sharedTableColumnMap = null;

        // Detect table-splitting and populate shared columns to propagate values back to the sharing entries.
        if (_entries.Count > 1
            || (_entries is [var singleEntry]
                && (singleEntry.SharedIdentityEntry is not null
                    || singleEntry.EntityType.GetComplexProperties()
                        .Any(cp => !cp.IsCollection && !cp.ComplexType.IsMappedToJson()))))
        {
            Check.DebugAssert(StoreStoredProcedure is null, "Multiple entries/shared identity not supported with stored procedures");

            sharedTableColumnMap = [];

            if (_comparer != null
                && _entries.Count > 1)
            {
                _entries.Sort(_comparer);
            }

            foreach (var entry in _entries)
            {
                var tableMapping = GetTableMapping(entry.EntityType);
                if (tableMapping == null)
                {
                    continue;
                }

                if (entry.SharedIdentityEntry != null)
                {
                    var sharedTableMapping = entry.EntityType != entry.SharedIdentityEntry.EntityType
                        ? GetTableMapping(entry.SharedIdentityEntry.EntityType)
                        : tableMapping;
                    if (sharedTableMapping != null)
                    {
                        HandleSharedColumns(
                            entry.SharedIdentityEntry.EntityType, entry.SharedIdentityEntry, sharedTableMapping, deleting,
                            sharedTableColumnMap);
                    }
                }

                HandleSharedColumns(entry.EntityType, entry, tableMapping, deleting, sharedTableColumnMap);
            }
        }

        if (_entries.Any(e => e.EntityType is IEntityType entityType
            && (entityType.IsMappedToJson()
                || entityType.GetFlattenedComplexProperties().Any(cp => cp.ComplexType.IsMappedToJson())
                || entityType.GetNavigations().Any(e => e.IsCollection && e.TargetEntityType.IsMappedToJson()))))
        {
            HandleJson(columnModifications);
        }

        foreach (var entry in _entries)
        {
            if (entry.EntityType.IsMappedToJson())
            {
                continue;
            }

            var nonMainEntry = !_mainEntryAdded || entry != _entries[0];

            var optionalDependentWithAllNull = false;
            if (StoreStoredProcedure is null)
            {
                var tableMapping = GetTableMapping(entry.EntityType);
                if (tableMapping is null)
                {
                    continue;
                }

                optionalDependentWithAllNull =
                    entry.EntityState is EntityState.Modified or EntityState.Added
                    && tableMapping.Table.IsOptional(entry.EntityType)
                    && tableMapping.Table.GetRowInternalForeignKeys(entry.EntityType).Any();

                HandleNonJson(entry, entry.EntityType, tableMapping, nonMainEntry, ref optionalDependentWithAllNull);
            }
            else
            {
                HandleSprocs(entry, nonMainEntry, ref optionalDependentWithAllNull);
            }

            if (optionalDependentWithAllNull && _logger != null)
            {
                if (_sensitiveLoggingEnabled)
                {
                    _logger.OptionalDependentWithAllNullPropertiesWarningSensitive(entry);
                }
                else
                {
                    _logger.OptionalDependentWithAllNullPropertiesWarning(entry);
                }
            }
        }

        return columnModifications;

        void HandleNonJson(
            IUpdateEntry entry, ITypeBase structuralType, ITableMapping tableMapping, bool nonMainEntry, ref bool optionalDependentWithAllNull)
        {
            foreach (var columnMapping in tableMapping.ColumnMappings)
            {
                HandleColumn(entry, columnMapping, nonMainEntry, ref optionalDependentWithAllNull);
            }

            foreach (var complexProperty in structuralType.GetComplexProperties())
            {
                var complexTableMapping = GetTableMapping(complexProperty.ComplexType);
                if (complexTableMapping != null)
                {
                    HandleNonJson(entry, complexProperty.ComplexType, complexTableMapping, nonMainEntry, ref optionalDependentWithAllNull);
                }
            }
        }

        void HandleColumn(
            IUpdateEntry entry, IColumnMappingBase columnMapping, bool nonMainEntry, ref bool optionalDependentWithAllNull)
        {
            var property = columnMapping.Property;
            var column = columnMapping.Column;
            var storedProcedureParameter = columnMapping is IStoredProcedureParameterMapping parameterMapping
                ? parameterMapping.Parameter
                : null;
            var isKey = property.IsPrimaryKey();
            var isCondition = !adding
                && (isKey
                    || storedProcedureParameter is { ForOriginalValue: true }
                    || (property.IsConcurrencyToken && storedProcedureParameter is null));

            // Store-generated properties generally need to be read back (unless we're deleting).
            // One exception is if the property is mapped to a non-output parameter.
            var readValue = state != EntityState.Deleted
                && ColumnModification.IsStoreGenerated(entry, property)
                && (storedProcedureParameter is null || storedProcedureParameter.Direction.HasFlag(ParameterDirection.Output));

            ColumnValuePropagator? columnPropagator = null;
            sharedTableColumnMap?.TryGetValue(column.Name, out columnPropagator);

            var writeValue = false;
            if (!readValue)
            {
                if (adding)
                {
                    writeValue = property.GetBeforeSaveBehavior() == PropertySaveBehavior.Save
                        || entry.HasStoreGeneratedValue(property);

                    columnPropagator?.TryPropagate(columnMapping, entry);
                }
                else if (storedProcedureParameter is not { ForOriginalValue: true }
                         && !deleting
                         && ((updating && property.GetAfterSaveBehavior() == PropertySaveBehavior.Save)
                             || (!isKey && nonMainEntry)
                             || entry.SharedIdentityEntry != null))
                {
                    // Note that for stored procedures we always need to send all parameters, regardless of whether the property
                    // actually changed.
                    writeValue = columnPropagator?.TryPropagate(columnMapping, entry)
                        ?? (entry.EntityState == EntityState.Added
                            || entry.EntityState == EntityState.Deleted
                            || ColumnModification.IsModified(entry, property)
                            || StoreStoredProcedure is not null);
                }
            }

            if (readValue
                || writeValue
                || isCondition)
            {
                var columnModificationParameters = new ColumnModificationParameters(
                    entry,
                    property,
                    column,
                    _generateParameterName!,
                    columnMapping.TypeMapping,
                    readValue,
                    writeValue,
                    isKey,
                    isCondition,
                    _sensitiveLoggingEnabled);

                var columnModification = CreateColumnModification(columnModificationParameters);

                if (columnPropagator != null
                    && column.PropertyMappings.Count != 1)
                {
                    if (columnPropagator.ColumnModification != null)
                    {
                        columnPropagator.ColumnModification.AddSharedColumnModification(columnModification);

                        return;
                    }

                    columnPropagator.ColumnModification = columnModification;
                }

                columnModifications.Add(columnModification);

                if (optionalDependentWithAllNull
                    && (columnModification.IsWrite
                        || (columnModification.IsCondition && !isKey))
                    && columnModification.Value is not null)
                {
                    optionalDependentWithAllNull = false;
                }
            }
            else if (optionalDependentWithAllNull
                     && state == EntityState.Modified
                     && property.DeclaringType == entry.EntityType
                     && entry.GetCurrentValue(property) is not null)
            {
                optionalDependentWithAllNull = false;
            }
        }

        void HandleSprocs(IUpdateEntry entry, bool nonMainEntry, ref bool optionalDependentWithAllNull)
        {
            var storedProcedureMapping = GetStoredProcedureMapping(entry.EntityType, EntityState);
            Check.DebugAssert(storedProcedureMapping is not null, "No sproc mapping but StoredProcedure is not null");
            var storedProcedure = storedProcedureMapping.StoredProcedure;

            // Stored procedures may have an additional rows affected result column or return value, which does not have a
            // property/column mapping but still needs to have be represented via a column modification.
            // Note that for rows affected parameters/result columns, we add column modifications below along with regular parameters/
            // result columns; for return value we do that here.
            if (storedProcedure.FindRowsAffectedParameter() is { } rowsAffectedParameter)
            {
                RowsAffectedColumn = rowsAffectedParameter.StoreParameter;
            }
            else if (storedProcedure.FindRowsAffectedResultColumn() is { } rowsAffectedResultColumn)
            {
                RowsAffectedColumn = rowsAffectedResultColumn.StoreResultColumn;
            }
            else if (storedProcedureMapping.StoreStoredProcedure.ReturnValue is { } rowsAffectedReturnValue)
            {
                RowsAffectedColumn = rowsAffectedReturnValue;

                columnModifications.Add(
                    CreateColumnModification(
                        new ColumnModificationParameters(
                            entry: null,
                            property: null,
                            rowsAffectedReturnValue,
                            _generateParameterName!,
                            rowsAffectedReturnValue.StoreTypeMapping,
                            valueIsRead: true,
                            valueIsWrite: false,
                            columnIsKey: false,
                            columnIsCondition: false,
                            _sensitiveLoggingEnabled)));
            }

            // In TPH, the sproc has parameters for all entity types in the hierarchy; we must generate null column modifications
            // for parameters for unrelated entity types.
            // Enumerate over the sproc parameters in order, trying to match a corresponding parameter mapping.
            // Note that we produce the column modifications in the same order as their sproc parameters; this is important and assumed
            // later in the pipeline.
            foreach (var parameter in StoreStoredProcedure.Parameters)
            {
                if (parameter.FindParameterMapping(entry.EntityType) is { } parameterMapping)
                {
                    HandleColumn(entry, parameterMapping, nonMainEntry, ref optionalDependentWithAllNull);
                    continue;
                }

                // The parameter has no corresponding mapping; this is either a sibling property in a TPH hierarchy or a rows affected
                // output parameter. Note that we set IsRead to false since we don't propagate the output parameter.
                columnModifications.Add(
                    CreateColumnModification(
                        new ColumnModificationParameters(
                            entry: null,
                            property: null,
                            parameter,
                            _generateParameterName!,
                            parameter.StoreTypeMapping,
                            valueIsRead: false,
                            valueIsWrite: parameter.Direction.HasFlag(ParameterDirection.Input),
                            columnIsKey: false,
                            columnIsCondition: false,
                            _sensitiveLoggingEnabled)));
            }

            foreach (var resultColumn in StoreStoredProcedure.ResultColumns)
            {
                if (resultColumn.FindColumnMapping(entry.EntityType) is { } resultColumnMapping)
                {
                    HandleColumn(entry, resultColumnMapping, nonMainEntry, ref optionalDependentWithAllNull);
                    continue;
                }

                // The result column has no corresponding mapping; this is either a sibling property in a TPH hierarchy or a rows
                // affected result column. Note that we set IsRead to false since we don't propagate the result column.
                columnModifications.Add(
                    CreateColumnModification(
                        new ColumnModificationParameters(
                            entry: null,
                            property: null,
                            resultColumn,
                            _generateParameterName!,
                            resultColumn.StoreTypeMapping,
                            valueIsRead: false,
                            valueIsWrite: false,
                            columnIsKey: false,
                            columnIsCondition: false,
                            _sensitiveLoggingEnabled)));
            }
        }

        void HandleSharedColumns(
            ITypeBase structuralType,
            IUpdateEntry entry,
            ITableMapping tableMapping,
            bool deleting,
            Dictionary<string, ColumnValuePropagator> sharedTableColumnMap)
        {
            InitializeSharedColumns(entry, tableMapping, deleting, sharedTableColumnMap);

            foreach (var complexProperty in structuralType.GetComplexProperties())
            {
                var complexTableMapping = GetTableMapping(complexProperty.ComplexType);
                if (complexTableMapping != null)
                {
                    HandleSharedColumns(
                        complexProperty.ComplexType, entry, complexTableMapping, deleting, sharedTableColumnMap);
                }
            }
        }

        static List<JsonPartialUpdatePathEntry>? FindJsonPartialUpdateInfo(IUpdateEntry entry, List<IUpdateEntry> processedEntries)
        {
            var result = new List<JsonPartialUpdatePathEntry>();
            var currentEntry = entry;
            var currentOwnership = currentEntry.EntityType.FindOwnership()!;

            while (currentEntry.EntityType.IsMappedToJson())
            {
                var jsonPropertyName = currentEntry.EntityType.GetJsonPropertyName()!;
                currentOwnership = currentEntry.EntityType.FindOwnership()!;
                var previousEntry = currentEntry;
#pragma warning disable EF1001 // Internal EF Core API usage.
                currentEntry = ((InternalEntityEntry)currentEntry).StateManager.FindPrincipal(
                    (InternalEntityEntry)currentEntry, currentOwnership)!;
#pragma warning restore EF1001 // Internal EF Core API usage.

                if (processedEntries.Contains(currentEntry))
                {
                    return null;
                }

                var ordinal = default(int?);
                if (!currentOwnership.IsUnique
                    && previousEntry.EntityState != EntityState.Added
                    && previousEntry.EntityState != EntityState.Deleted)
                {
                    var ordinalProperty = previousEntry.EntityType.FindPrimaryKey()!.Properties.Last();
                    ordinal = (int)previousEntry.GetCurrentProviderValue(ordinalProperty)! - 1;
                }

                var pathEntry = new JsonPartialUpdatePathEntry(
                    currentOwnership.PrincipalEntityType.IsMappedToJson() ? jsonPropertyName : "$",
                    ordinal,
                    currentEntry,
                    currentOwnership.GetNavigation(pointsToPrincipal: false)!); // TODO: Handle complex properties, Issue #36429

                result.Insert(0, pathEntry);
            }

            var modifiedMembers = entry.EntityType.GetFlattenedProperties().Where(entry.IsModified).ToList();
            if (modifiedMembers.Count == 1)
            {
                result.Add(new JsonPartialUpdatePathEntry(
                    modifiedMembers[0].GetJsonPropertyName()!,
                    null,
                    entry,
                    modifiedMembers[0]));
            }
            else
            {
                // only add to processed entries list if we are planning to update the entire entity
                // (rather than just a single property on that entity)
                processedEntries.Add(entry);
            }

            // parent entity got deleted, no need to do any json-specific processing
            if (currentEntry.EntityState == EntityState.Deleted)
            {
                return null;
            }

            return result;
        }

        static List<JsonPartialUpdatePathEntry> FindCommonJsonPartialUpdateInfo(
            List<JsonPartialUpdatePathEntry> first,
            List<JsonPartialUpdatePathEntry> second)
        {
            var commonPath = new List<JsonPartialUpdatePathEntry>();
            for (var i = 0; i < Math.Min(first.Count, second.Count); i++)
            {
                if (first[i].PropertyName != second[i].PropertyName)
                {
                    break;
                }

                if (first[i].Ordinal == second[i].Ordinal)
                {
                    commonPath.Add(first[i]);
                    continue;
                }

                var common = new JsonPartialUpdatePathEntry(
                    first[i].PropertyName,
                    null,
                    first[i].ParentEntry,
                    Property: first[i].Property);

                commonPath.Add(common);

                break;
            }

            Check.DebugAssert(commonPath.Count > 0, "Common denominator should always have at least one node - the root.");

            return commonPath;
        }

        void HandleJson(List<IColumnModification> columnModifications)
        {
            var jsonColumnsUpdateMap = new Dictionary<IColumn, List<JsonPartialUpdatePathEntry>>();
            var processedEntries = new List<IUpdateEntry>();
            foreach (var entry in _entries)
            {
                if (!entry.EntityType.IsMappedToJson())
                {
                    continue;
                }

                var jsonColumn = GetTableMapping(entry.EntityType)!.Table.FindColumn(entry.EntityType.GetContainerColumnName()!)!;
                var jsonPartialUpdateInfo = FindJsonPartialUpdateInfo(entry, processedEntries);
                if (jsonPartialUpdateInfo == null)
                {
                    continue;
                }

                if (jsonColumnsUpdateMap.TryGetValue(jsonColumn, out var currentJsonPartialUpdateInfo))
                {
                    jsonPartialUpdateInfo = FindCommonJsonPartialUpdateInfo(
                        currentJsonPartialUpdateInfo,
                        jsonPartialUpdateInfo);
                }

                jsonColumnsUpdateMap[jsonColumn] = jsonPartialUpdateInfo;
            }

            foreach (var entry in _entries)
            {
                if (entry.EntityType.IsMappedToJson())
                {
                    continue;
                }

                foreach (var jsonCollectionNavigation in entry.EntityType.GetNavigations()
                             .Where(
                                 n => n.IsCollection
                                     && n.TargetEntityType.IsMappedToJson()
                                     && (entry.GetCurrentValue(n) as IEnumerable)?.Any() == false))
                {
                    var jsonCollectionEntityType = jsonCollectionNavigation.TargetEntityType;
                    var jsonCollectionColumn =
                        GetTableMapping(jsonCollectionEntityType)!.Table.FindColumn(
                            jsonCollectionEntityType.GetContainerColumnName()!)!;

                    if (!jsonColumnsUpdateMap.ContainsKey(jsonCollectionColumn))
                    {
                        var jsonPartialUpdateInfo = new List<JsonPartialUpdatePathEntry>
                        {
                            new("$", null, entry, jsonCollectionNavigation)
                        };
                        jsonColumnsUpdateMap[jsonCollectionColumn] = jsonPartialUpdateInfo;
                    }
                }

                foreach (var complexProperty in entry.EntityType.GetFlattenedComplexProperties())
                {
                    var complexType = complexProperty.ComplexType;
                    if (!complexType.IsMappedToJson()
                        || complexProperty.DeclaringType.IsMappedToJson()
                        || (entry.EntityState != EntityState.Added
                            && !entry.IsModified(complexProperty)))
                    {
                        continue;
                    }

                    var jsonColumn = GetTableMapping(entry.EntityType)!.Table.FindColumn(complexType.GetContainerColumnName()!)!;
                    if (!jsonColumnsUpdateMap.ContainsKey(jsonColumn))
                    {
                        var jsonPartialUpdateInfo = new List<JsonPartialUpdatePathEntry>
                        {
                            new("$", null, entry, complexProperty)
                        };
                        jsonColumnsUpdateMap[jsonColumn] = jsonPartialUpdateInfo;
                    }
                }
            }

            foreach (var (jsonColumn, updateInfo) in jsonColumnsUpdateMap)
            {
                var finalUpdatePathElement = updateInfo.Last();
                var jsonColumnTypeMapping = jsonColumn.StoreTypeMapping;
                var jsonProperty = finalUpdatePathElement.Property;
                var propertyValue = finalUpdatePathElement.ParentEntry.GetCurrentValue(jsonProperty);

                // TODO: Change JSON path to be structured, issue #32185
                var jsonPathString = string.Join(
                    ".", updateInfo.Select(x => x.PropertyName + (x.Ordinal != null ? "[" + x.Ordinal + "]" : "")));
                if (jsonProperty is IProperty property)
                {
                    var columnModificationParameters = new ColumnModificationParameters(
                        jsonColumn.Name,
                        value: propertyValue,
                        property: property,
                        columnType: jsonColumnTypeMapping.StoreType,
                        jsonColumnTypeMapping,
                        jsonPath: jsonPathString,
                        read: false,
                        write: true,
                        key: false,
                        condition: false,
                        _sensitiveLoggingEnabled) { GenerateParameterName = _generateParameterName };

                    ProcessSinglePropertyJsonUpdate(ref columnModificationParameters);

                    columnModifications.Add(new ColumnModification(columnModificationParameters));
                }
                else
                {
                    var stream = new MemoryStream();
                    var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = false });
                    if (finalUpdatePathElement.Ordinal != null && propertyValue != null)
                    {
                        var i = 0;
                        foreach (var navigationValueElement in (IEnumerable)propertyValue)
                        {
                            if (i == finalUpdatePathElement.Ordinal)
                            {
                                WriteJson(
                                    writer,
                                    navigationValueElement,
                                    (IInternalEntry)finalUpdatePathElement.ParentEntry,
                                    jsonProperty,
                                    ordinal: null,
                                    isCollection: false,
                                    isTopLevel: true);

                                break;
                            }

                            i++;
                        }
                    }
                    else
                    {
                        WriteJson(
                            writer,
                            propertyValue,
                            (IInternalEntry)finalUpdatePathElement.ParentEntry,
                            jsonProperty,
                            ordinal: null,
                            isCollection: jsonProperty.IsCollection,
                            isTopLevel: true);
                    }

                    writer.Flush();

                    var value = writer.BytesCommitted > 0
                        ? Encoding.UTF8.GetString(stream.ToArray())
                        : null;

                    columnModifications.Add(
                        new ColumnModification(
                            new ColumnModificationParameters(
                                jsonColumn.Name,
                                value: value,
                                property: null,
                                columnType: jsonColumnTypeMapping.StoreType,
                                jsonColumnTypeMapping,
                                jsonPath: jsonPathString,
                                read: false,
                                write: true,
                                key: false,
                                condition: false,
                                _sensitiveLoggingEnabled) { GenerateParameterName = _generateParameterName }));
                }
            }
        }
    }

    /// <summary>
    ///     Performs processing specifically needed for column modifications that correspond to single-property JSON updates.
    /// </summary>
    /// <remarks>
    ///     By default, strings, numeric types and bool and sent as a regular relational parameter, since database functions responsible for
    ///     patching JSON documents support this. Other types get converted to JSON via the normal means and sent as a string parameter.
    /// </remarks>
    protected virtual void ProcessSinglePropertyJsonUpdate(ref ColumnModificationParameters parameters)
    {
        var property = parameters.Property!;
        var mapping = property.GetRelationalTypeMapping();
        var propertyProviderClrType = (mapping.Converter?.ProviderClrType ?? property.ClrType).UnwrapNullableType();
        var value = parameters.Value;

        // On most databases, the function which patches a JSON document (e.g. SQL Server JSON_MODIFY) accepts relational string, numeric
        // and bool types directly, without serializing it to a JSON string. So by default, for those cases simply return the value as-is,
        // with the property's type mapping which will take care of sending the parameter with the relational value.
        // Note that we haven't yet applied a value converter if one is configured, in order to allow for it to get applied later with
        // the regular parameter flow.
        if (value == null
            || propertyProviderClrType == typeof(string)
            || propertyProviderClrType == typeof(bool)
            || propertyProviderClrType.IsNumeric())
        {
            parameters = parameters with { Value = value, TypeMapping = mapping };
        }
        else
        {
            var jsonValueReaderWriter = mapping.JsonValueReaderWriter;
            value = jsonValueReaderWriter?.ToJsonString(value)[1..^1] // The JSON string contains enclosing quotes, remove these.
                ?? (mapping.Converter == null ? value : mapping.Converter.ConvertToProvider(value));

            parameters = parameters with { Value = value };
        }
    }

    private void WriteJson(
        Utf8JsonWriter writer,
        object? value,
#pragma warning disable EF1001 // Internal EF Core API usage.
        IInternalEntry parentEntry,
#pragma warning restore EF1001 // Internal EF Core API usage.
        IPropertyBase property,
        int? ordinal,
        bool isCollection,
        bool isTopLevel)
    {
        var structuralType = property is INavigation navigation
            ? (ITypeBase)navigation.TargetEntityType
            : ((IComplexProperty)property).ComplexType;
        if (value is null)
        {
            if (!isTopLevel)
            {
                writer.WriteNullValue();
            }

            return;
        }

        if (isCollection)
        {
            var i = 0;
            writer.WriteStartArray();
            foreach (var collectionElement in (IEnumerable)value)
            {
                WriteJson(
                    writer,
                    collectionElement,
                    parentEntry,
                    property,
                    i++,
                    isCollection: false,
                    isTopLevel: false);
            }

            writer.WriteEndArray();
            return;
        }

        writer.WriteStartObject();

#pragma warning disable EF1001 // Internal EF Core API usage.
        var entry = structuralType is IComplexType complexType
            ? complexType.ComplexProperty.IsCollection
                ? parentEntry.GetComplexCollectionEntry(complexType.ComplexProperty, ordinal!.Value)
                : parentEntry
            : ((InternalEntityEntry)parentEntry).StateManager.TryGetEntry(value, (IEntityType)structuralType)!;
#pragma warning restore EF1001 // Internal EF Core API usage.
        WriteJsonObject(writer, parentEntry, entry, structuralType, ordinal);

        writer.WriteEndObject();
    }

#pragma warning disable EF1001 // Internal EF Core API usage.
    private void WriteJsonObject(Utf8JsonWriter writer, IInternalEntry parentEntry, IInternalEntry entry, ITypeBase structuralType, int? ordinal)
#pragma warning restore EF1001 // Internal EF Core API usage.
    {
        foreach (var property in structuralType.GetProperties())
        {
            if (property.IsKey())
            {
                if (property.IsOrdinalKeyProperty() && ordinal != null)
                {
#pragma warning disable EF1001 // Internal EF Core API usage.
                    entry.SetStoreGeneratedValue(property, ordinal.Value + 1, setModified: false);
#pragma warning disable EF1001 // Internal EF Core API usage.
                }

                continue;
            }

            // jsonPropertyName can only be null for key properties
            var jsonPropertyName = property.GetJsonPropertyName()!;
#pragma warning disable EF1001 // Internal EF Core API usage.
            var propertyValue = entry.GetCurrentValue(property);
#pragma warning disable EF1001 // Internal EF Core API usage.
            writer.WritePropertyName(jsonPropertyName);

            if (propertyValue is not null)
            {
                var jsonValueReaderWriter = property.GetJsonValueReaderWriter() ?? property.GetTypeMapping().JsonValueReaderWriter;
                Check.DebugAssert(jsonValueReaderWriter is not null, "Missing JsonValueReaderWriter on JSON property");
                jsonValueReaderWriter.ToJson(writer, propertyValue);
            }
            else
            {
                writer.WriteNullValue();
            }
        }

        foreach (var complexProperty in structuralType.GetComplexProperties())
        {
            var jsonPropertyName = complexProperty.GetJsonPropertyName()!;
#pragma warning disable EF1001 // Internal EF Core API usage.
            var complexPropertyValue = entry.GetCurrentValue(complexProperty);
#pragma warning disable EF1001 // Internal EF Core API usage.
            writer.WritePropertyName(jsonPropertyName);

            WriteJson(
                writer,
                complexPropertyValue,
                entry,
                complexProperty,
                ordinal: null,
                isCollection: complexProperty.IsCollection,
                isTopLevel: false);
        }

        if (structuralType is IEntityType entityType)
        {
            foreach (var navigation in entityType.GetNavigations())
            {
                // skip back-references to the parent
                if (navigation.IsOnDependent)
                {
                    continue;
                }

                var jsonPropertyName = navigation.TargetEntityType.GetJsonPropertyName()!;
#pragma warning disable EF1001 // Internal EF Core API usage.
                var ownedNavigationValue = entry.GetCurrentValue(navigation)!;
#pragma warning disable EF1001 // Internal EF Core API usage.

                writer.WritePropertyName(jsonPropertyName);
                WriteJson(
                    writer,
                    ownedNavigationValue,
                    entry,
                    navigation,
                    ordinal: null,
                    isCollection: navigation.IsCollection,
                    isTopLevel: false);
            }
        }
    }

    private ITableMapping? GetTableMapping(ITypeBase structuralType)
    {
        foreach (var mapping in structuralType.GetTableMappings())
        {
            var table = mapping.Table;
            if (table.Name == TableName
                && table.Schema == Schema)
            {
                return mapping;
            }
        }

        return null;
    }

    private IStoredProcedureMapping? GetStoredProcedureMapping(IEntityType entityType, EntityState entityState)
    {
        var sprocMappings = entityState switch
        {
            EntityState.Added => entityType.GetInsertStoredProcedureMappings(),
            EntityState.Modified => entityType.GetUpdateStoredProcedureMappings(),
            EntityState.Deleted => entityType.GetDeleteStoredProcedureMappings(),

            _ => throw new ArgumentOutOfRangeException(nameof(entityState), entityState, "Invalid EntityState value")
        };

        foreach (var mapping in sprocMappings)
        {
            if (mapping.StoreStoredProcedure == StoreStoredProcedure)
            {
                return mapping;
            }
        }

        return null;
    }

    private static void InitializeSharedColumns(
        IUpdateEntry entry,
        ITableMapping tableMapping,
        bool deleting,
        Dictionary<string, ColumnValuePropagator> columnMap)
    {
        foreach (var columnMapping in tableMapping.ColumnMappings)
        {
            if (columnMapping.Property.DeclaringType.IsMappedToJson()
                || (columnMapping.Column.PropertyMappings.Select(p => p.Property).Distinct().Count() == 1
                    && entry.SharedIdentityEntry == null))
            {
                continue;
            }

            var columnName = columnMapping.Column.Name;
            if (!columnMap.TryGetValue(columnName, out var columnPropagator))
            {
                columnPropagator = new ColumnValuePropagator();
                columnMap.Add(columnName, columnPropagator);
            }

            if (!deleting)
            {
                columnPropagator.RecordValue(columnMapping, entry);
            }
        }
    }

    /// <inheritdoc />
    public virtual void PropagateResults(RelationalDataReader relationalReader)
    {
        var (seenRegularResultColumn, seenStoredProcedureResultColumn) = (false, false);

        var columnCount = ColumnModifications.Count;

        var readerIndex = -1;
        for (var columnIndex = 0; columnIndex < columnCount; columnIndex++)
        {
            var columnModification = ColumnModifications[columnIndex];

            if (!columnModification.IsRead)
            {
                continue;
            }

            switch (columnModification.Column)
            {
                case IColumn:
                    // If we're reading a regular result set, then we generated SQL which projects columns out in the order in which they're
                    // listed in ColumnModifications.
                    readerIndex++;
#if DEBUG
                    Check.DebugAssert(!seenStoredProcedureResultColumn);
                    seenRegularResultColumn = true;
#endif
                    break;

                case IStoreStoredProcedureResultColumn resultColumn:
                    if (ReferenceEquals(RowsAffectedColumn, resultColumn))
                    {
                        continue;
                    }

                    // For stored procedure result sets, we need to get the column ordering from metadata.
                    readerIndex = resultColumn.Position;
#if DEBUG
                    Check.DebugAssert(!seenRegularResultColumn);
                    seenStoredProcedureResultColumn = true;
#endif
                    break;

                case IStoreStoredProcedureParameter or IStoreStoredProcedureReturnValue:
                    // Stored procedure output parameters (and return values) are only propagated later, since they're populated only when
                    // the reader is closed.
                    continue;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            Check.DebugAssert(
                columnModification.Property is not null, "No property when propagating results to a readable column modification");

            columnModification.Value =
                columnModification.Property.GetReaderFieldValue(relationalReader, readerIndex, _detailedErrorsEnabled);
        }
    }

    /// <inheritdoc />
    public virtual void PropagateOutputParameters(DbParameterCollection parameterCollection, int baseParameterIndex)
    {
        var columnCount = ColumnModifications.Count;

        for (var columnIndex = 0; columnIndex < columnCount; columnIndex++)
        {
            var columnModification = ColumnModifications[columnIndex];
            if (columnModification.Property is null
                || !columnModification.IsRead
                || columnModification.Column is not IStoreStoredProcedureParameter storedProcedureParameter)
            {
                continue;
            }

            Check.DebugAssert(
                storedProcedureParameter.Direction != ParameterDirection.Input,
                "Readable column modification has a stored procedure parameter with direction Input");
            Check.DebugAssert(
                columnModification.ParameterName is not null,
                "Readable column modification has a stored procedure parameter without a name");

            columnModification.Value = parameterCollection[baseParameterIndex + storedProcedureParameter.Position].Value;
        }
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var result = $"{EntityState}: {TableName}";
        if (_columnModifications == null)
        {
            return result;
        }

        result += "(" + string.Join(", ", _columnModifications.Where(m => m.IsKey).Select(m => m.OriginalValue?.ToString())) + ")";
        return result;
    }

    private sealed class ColumnValuePropagator
    {
        private bool _write;
        private object? _originalValue;
        private object? _currentValue;
        private bool _originalValueInitialized;

        public IColumnModification? ColumnModification { get; set; }

        public void RecordValue(IColumnMapping mapping, IUpdateEntry entry)
        {
            var property = mapping.Property;
            switch (entry.EntityState)
            {
                case EntityState.Modified:
                    if (!_write
                        && Update.ColumnModification.IsModified(entry, property))
                    {
                        _write = true;
                        _currentValue = Update.ColumnModification.GetCurrentProviderValue(entry, property);
                        _originalValue = Update.ColumnModification.GetOriginalProviderValue(entry, property);
                        _originalValueInitialized = true;
                    }

                    break;
                case EntityState.Added:
                    if (_currentValue == null
                        || !property.GetValueComparer().Equals(
                            Update.ColumnModification.GetCurrentValue(entry, property),
                            property.Sentinel))
                    {
                        _currentValue = Update.ColumnModification.GetCurrentProviderValue(entry, property);
                    }

                    _write = !_originalValueInitialized
                        || !mapping.Column.ProviderValueComparer.Equals(_originalValue, _currentValue);

                    break;
                case EntityState.Deleted:
                    _originalValue = Update.ColumnModification.GetOriginalProviderValue(entry, property);
                    _originalValueInitialized = true;
                    if (!_write
                        && !property.IsPrimaryKey())
                    {
                        _write = true;
                        _currentValue = null;
                    }

                    break;
            }
        }

        public bool TryPropagate(IColumnMappingBase mapping, IUpdateEntry entry)
        {
            var property = mapping.Property;
            if (_write
                && (entry.EntityState == EntityState.Unchanged
                    || (entry.EntityState == EntityState.Modified && !Update.ColumnModification.IsModified(entry, property))
                    || (entry.EntityState == EntityState.Added
                        && ((!_originalValueInitialized
                                && property.GetValueComparer().Equals(
                                    Update.ColumnModification.GetCurrentValue(entry, property),
                                    property.Sentinel))
                            || (_originalValueInitialized
                                && mapping.Column.ProviderValueComparer.Equals(
                                    Update.ColumnModification.GetCurrentProviderValue(entry, property),
                                    _originalValue))))))
            {
                if ((property.GetAfterSaveBehavior() == PropertySaveBehavior.Save
                        || entry.EntityState == EntityState.Added)
                    && property.ValueGenerated != ValueGenerated.Never)
                {
                    var value = _currentValue;
                    var converter = property.GetTypeMapping().Converter;
                    if (converter != null)
                    {
                        value = converter.ConvertFromProvider(value);
                    }

                    Update.ColumnModification.SetStoreGeneratedValue(entry, property, value);
                }

                return false;
            }

            return _write;
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class StoreStoredProcedure : TableBase, IStoreStoredProcedure
{
    private readonly SortedDictionary<string, IStoreStoredProcedureParameter> _parametersSet;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public StoreStoredProcedure(IRuntimeStoredProcedure sproc, RelationalModel model)
        : base(sproc.Name, sproc.Schema, model)
    {
        StoredProcedures = new(StoredProcedureComparer.Instance) { { sproc } };

        sproc.StoreStoredProcedure = this;

        _parametersSet = new(StringComparer.Ordinal);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SortedSet<IStoredProcedure> StoredProcedures { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual List<IStoreStoredProcedureParameter> Parameters { get; protected set; }
        = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void AddParameter(IStoreStoredProcedureParameter parameter)
    {
        _parametersSet[parameter.Name] = parameter;
        Parameters.Add(parameter);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IStoreStoredProcedureParameter? FindParameter(string name)
        => _parametersSet.TryGetValue(name, out var parameter)
            ? parameter
            : null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IStoreStoredProcedureParameter? FindParameter(IProperty property)
        => property.GetInsertStoredProcedureParameterMappings()
            .Concat(property.GetDeleteStoredProcedureParameterMappings())
            .Concat(property.GetUpdateStoredProcedureParameterMappings())
            .FirstOrDefault(cm => cm.StoredProcedureMapping.StoreStoredProcedure == this)
            ?.Parameter;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual List<IStoreStoredProcedureResultColumn> ResultColumns { get; protected set; }
        = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void AddResultColumn(IStoreStoredProcedureResultColumn column)
    {
        Columns[column.Name] = column;
        ResultColumns.Add(column);
    }

    /// <inheritdoc />
    public override IColumnBase? FindColumn(IProperty property)
        => property.GetInsertStoredProcedureResultColumnMappings()
            .Concat(property.GetUpdateStoredProcedureResultColumnMappings())
            .FirstOrDefault(cm => cm.StoredProcedureMapping.StoreStoredProcedure == this)
            ?.Column;

    /// <inheritdoc />
    [DebuggerStepThrough]
    public virtual IStoreStoredProcedureResultColumn? FindResultColumn(string name)
        => (IStoreStoredProcedureResultColumn?)base.FindColumn(name);

    /// <inheritdoc />
    [DebuggerStepThrough]
    public virtual IStoreStoredProcedureResultColumn? FindResultColumn(IProperty property)
        => (IStoreStoredProcedureResultColumn?)FindColumn(property);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string ToString()
        => ((IStoreStoredProcedure)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

    /// <inheritdoc />
    IEnumerable<IStoredProcedure> IStoreStoredProcedure.StoredProcedures
    {
        [DebuggerStepThrough]
        get => StoredProcedures;
    }

    /// <inheritdoc />
    IEnumerable<IStoredProcedureMapping> IStoreStoredProcedure.EntityTypeMappings
    {
        [DebuggerStepThrough]
        get => EntityTypeMappings.Cast<IStoredProcedureMapping>();
    }

    /// <inheritdoc />
    IEnumerable<IStoreStoredProcedureParameter> IStoreStoredProcedure.Parameters
    {
        [DebuggerStepThrough]
        get => Parameters;
    }

    /// <inheritdoc />
    IEnumerable<IStoreStoredProcedureResultColumn> IStoreStoredProcedure.ResultColumns
    {
        [DebuggerStepThrough]
        get => ResultColumns;
    }
}

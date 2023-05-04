// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class StoreFunction : TableBase, IStoreFunction
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public StoreFunction(IRuntimeDbFunction dbFunction, RelationalModel model)
        : base(dbFunction.Name, dbFunction.Schema, model)
    {
        DbFunctions = new SortedDictionary<string, IDbFunction>(StringComparer.Ordinal) { { dbFunction.ModelName, dbFunction } };
        IsBuiltIn = dbFunction.IsBuiltIn;
        ReturnType = dbFunction.StoreType;

        Parameters = new StoreFunctionParameter[dbFunction.Parameters.Count];
        for (var i = 0; i < dbFunction.Parameters.Count; i++)
        {
            Parameters[i] = new StoreFunctionParameter(this, (IRuntimeDbFunctionParameter)dbFunction.Parameters[i]);
        }

        dbFunction.StoreFunction = this;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SortedDictionary<string, IDbFunction> DbFunctions { get; }

    /// <inheritdoc />
    public virtual bool IsBuiltIn { get; }

    /// <inheritdoc />
    public virtual string? ReturnType { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual StoreFunctionParameter[] Parameters { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void AddDbFunction(IRuntimeDbFunction dbFunction)
    {
        dbFunction.StoreFunction = this;
        for (var i = 0; i < dbFunction.Parameters.Count; i++)
        {
            Parameters[i].DbFunctionParameters.Add(dbFunction.Parameters[i]);
        }

        DbFunctions.Add(dbFunction.ModelName, dbFunction);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual StoreFunctionParameter? FindParameter(string propertyName)
        => Parameters.FirstOrDefault(p => p.Name == propertyName);

    /// <inheritdoc />
    public override IColumnBase? FindColumn(IProperty property)
        => property.GetFunctionColumnMappings()
            .FirstOrDefault(cm => cm.TableMapping.Table == this)
            ?.Column;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public new virtual FunctionColumn? FindColumn(string name)
        => (FunctionColumn?)base.FindColumn(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string ToString()
        => ((IStoreFunction)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

    /// <inheritdoc />
    IEnumerable<IFunctionMapping> IStoreFunction.EntityTypeMappings
    {
        [DebuggerStepThrough]
        get => EntityTypeMappings.Cast<IFunctionMapping>();
    }

    /// <inheritdoc />
    IEnumerable<IFunctionColumn> IStoreFunction.Columns
    {
        [DebuggerStepThrough]
        get => Columns.Values.Cast<IFunctionColumn>();
    }

    /// <inheritdoc />
    IEnumerable<IStoreFunctionParameter> IStoreFunction.Parameters
    {
        [DebuggerStepThrough]
        get => Parameters;
    }

    /// <inheritdoc />
    IEnumerable<IDbFunction> IStoreFunction.DbFunctions
    {
        [DebuggerStepThrough]
        get => DbFunctions.Values;
    }

    /// <inheritdoc />
    [DebuggerStepThrough]
    IFunctionColumn? IStoreFunction.FindColumn(string name)
        => (IFunctionColumn?)base.FindColumn(name);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IFunctionColumn? IStoreFunction.FindColumn(IProperty property)
        => (IFunctionColumn?)FindColumn(property);
}

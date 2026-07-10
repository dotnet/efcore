// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class StoredProcedureMapping : TableMappingBase<IStoredProcedureResultColumnMapping>, IStoredProcedureMapping
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public StoredProcedureMapping(
        IEntityType entityType,
        StoreStoredProcedure storeStoredProcedure,
        IStoredProcedure storedProcedure,
        ITableMapping? tableMapping,
        bool? includesDerivedTypes)
        : base(entityType, storeStoredProcedure, includesDerivedTypes)
    {
        StoredProcedure = storedProcedure;
        StoredProcedureIdentifier = storedProcedure.GetStoreIdentifier();
        TableMapping = tableMapping;
    }

    /// <inheritdoc />
    public virtual IStoreStoredProcedure StoreStoredProcedure
        => (StoreStoredProcedure)base.Table;

    /// <inheritdoc />
    public virtual IStoredProcedure StoredProcedure { get; }

    /// <inheritdoc />
    public virtual StoreObjectIdentifier StoredProcedureIdentifier { get; }

    /// <inheritdoc />
    public virtual ITableMapping? TableMapping { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual List<IStoredProcedureParameterMapping> ParameterMappings { get; }
        = [];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool AddParameterMapping(IStoredProcedureParameterMapping parameterMapping)
    {
        if (ParameterMappings.IndexOf(parameterMapping, ColumnMappingBaseComparer.Instance) != -1)
        {
            return false;
        }

        ParameterMappings.Add(parameterMapping);
        ParameterMappings.Sort(ColumnMappingBaseComparer.Instance);

        return true;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string ToString()
        => ((IStoredProcedureMapping)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual DebugView DebugView
        => new(
            () => ((IStoredProcedureMapping)this).ToDebugString(),
            () => ((IStoredProcedureMapping)this).ToDebugString(MetadataDebugStringOptions.LongDefault));

    /// <inheritdoc />
    IEnumerable<IStoredProcedureResultColumnMapping> IStoredProcedureMapping.ResultColumnMappings
    {
        [DebuggerStepThrough]
        get => ColumnMappings;
    }

    /// <inheritdoc />
    IEnumerable<IStoredProcedureParameterMapping> IStoredProcedureMapping.ParameterMappings
    {
        [DebuggerStepThrough]
        get => ParameterMappings;
    }
}

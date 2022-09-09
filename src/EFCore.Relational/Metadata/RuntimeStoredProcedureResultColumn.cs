// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     Represents a stored procedure result column.
/// </summary>
public class RuntimeStoredProcedureResultColumn : AnnotatableBase, IRuntimeStoredProcedureResultColumn
{
    private IStoreStoredProcedureResultColumn? _storeResultColumn;
    private readonly string? _propertyName;
    private readonly bool _forRowsAffected;
    private readonly string _name;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public RuntimeStoredProcedureResultColumn(
        RuntimeStoredProcedure storedProcedure,
        string name,
        bool forRowsAffected,
        string? propertyName)
    {
        StoredProcedure = storedProcedure;
        _propertyName = propertyName;
        _forRowsAffected = forRowsAffected;
        _name = name;
    }

    /// <summary>
    ///     Gets the stored procedure to which this parameter belongs.
    /// </summary>
    public virtual RuntimeStoredProcedure StoredProcedure { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string ToString()
        => ((IStoredProcedureResultColumn)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual DebugView DebugView
        => new(
            () => ((IStoredProcedureResultColumn)this).ToDebugString(),
            () => ((IStoredProcedureResultColumn)this).ToDebugString(MetadataDebugStringOptions.LongDefault));

    /// <inheritdoc />
    IReadOnlyStoredProcedure IReadOnlyStoredProcedureResultColumn.StoredProcedure
    {
        [DebuggerStepThrough]
        get => StoredProcedure;
    }

    /// <inheritdoc />
    IStoredProcedure IStoredProcedureResultColumn.StoredProcedure
    {
        [DebuggerStepThrough]
        get => StoredProcedure;
    }

    /// <inheritdoc />
    string IReadOnlyStoredProcedureResultColumn.Name
    {
        [DebuggerStepThrough]
        get => _name;
    }

    /// <inheritdoc />
    string? IReadOnlyStoredProcedureResultColumn.PropertyName
    {
        [DebuggerStepThrough]
        get => _propertyName;
    }

    /// <inheritdoc />
    bool IReadOnlyStoredProcedureResultColumn.ForRowsAffected
    {
        [DebuggerStepThrough]
        get => _forRowsAffected;
    }

    /// <inheritdoc />
    IStoreStoredProcedureResultColumn IStoredProcedureResultColumn.StoreResultColumn
    {
        [DebuggerStepThrough]
        get => _storeResultColumn!;
    }

    /// <inheritdoc />
    IStoreStoredProcedureResultColumn IRuntimeStoredProcedureResultColumn.StoreResultColumn
    {
        [DebuggerStepThrough]
        get => _storeResultColumn!;
        set => _storeResultColumn = value;
    }
}

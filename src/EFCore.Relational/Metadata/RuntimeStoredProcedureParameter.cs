// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     Represents a stored procedure parameter.
/// </summary>
public class RuntimeStoredProcedureParameter : AnnotatableBase, IRuntimeStoredProcedureParameter
{
    private IStoreStoredProcedureParameter? _storeParameter;
    private readonly string? _propertyName;
    private readonly bool _forRowsAffected;
    private readonly bool? _forOriginalValue;
    private readonly string _name;
    private readonly ParameterDirection _direction;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public RuntimeStoredProcedureParameter(
        RuntimeStoredProcedure storedProcedure,
        string name,
        ParameterDirection direction,
        bool forRowsAffected,
        string? propertyName,
        bool? forOriginalValue)
    {
        StoredProcedure = storedProcedure;
        _propertyName = propertyName;
        _forOriginalValue = forOriginalValue;
        _forRowsAffected = forRowsAffected;
        _name = name;
        _direction = direction;
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
        => ((IStoredProcedureParameter)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual DebugView DebugView
        => new(
            () => ((IStoredProcedureParameter)this).ToDebugString(),
            () => ((IStoredProcedureParameter)this).ToDebugString(MetadataDebugStringOptions.LongDefault));

    /// <inheritdoc />
    IReadOnlyStoredProcedure IReadOnlyStoredProcedureParameter.StoredProcedure
    {
        [DebuggerStepThrough]
        get => StoredProcedure;
    }

    /// <inheritdoc />
    IStoredProcedure IStoredProcedureParameter.StoredProcedure
    {
        [DebuggerStepThrough]
        get => StoredProcedure;
    }

    /// <inheritdoc />
    string IReadOnlyStoredProcedureParameter.Name
    {
        [DebuggerStepThrough]
        get => _name;
    }

    /// <inheritdoc />
    string? IReadOnlyStoredProcedureParameter.PropertyName
    {
        [DebuggerStepThrough]
        get => _propertyName;
    }

    /// <inheritdoc />
    ParameterDirection IReadOnlyStoredProcedureParameter.Direction
    {
        [DebuggerStepThrough]
        get => _direction;
    }

    /// <inheritdoc />
    bool? IReadOnlyStoredProcedureParameter.ForOriginalValue
    {
        [DebuggerStepThrough]
        get => _forOriginalValue;
    }

    /// <inheritdoc />
    bool IReadOnlyStoredProcedureParameter.ForRowsAffected
    {
        [DebuggerStepThrough]
        get => _forRowsAffected;
    }

    /// <inheritdoc />
    IStoreStoredProcedureParameter IStoredProcedureParameter.StoreParameter
    {
        [DebuggerStepThrough]
        get => _storeParameter!;
    }

    /// <inheritdoc />
    IStoreStoredProcedureParameter IRuntimeStoredProcedureParameter.StoreParameter
    {
        [DebuggerStepThrough]
        get => _storeParameter!;
        set => _storeParameter = value;
    }
}

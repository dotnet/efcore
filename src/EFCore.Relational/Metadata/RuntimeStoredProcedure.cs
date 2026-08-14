// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class RuntimeStoredProcedure : AnnotatableBase, IRuntimeStoredProcedure
{
    private readonly List<RuntimeStoredProcedureParameter> _parameters = [];
    private readonly List<RuntimeStoredProcedureResultColumn> _resultColumns = [];
    private readonly string? _schema;
    private readonly string _name;
    private readonly bool _isRowsAffectedReturned;
    private IStoreStoredProcedure? _storeStoredProcedure;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RuntimeStoredProcedure" /> class.
    /// </summary>
    /// <param name="entityType">The mapped entity type.</param>
    /// <param name="name">The name.</param>
    /// <param name="schema">The schema.</param>
    /// <param name="rowsAffectedReturned">Whether this stored procedure returns the number of rows affected.</param>
    public RuntimeStoredProcedure(
        RuntimeEntityType entityType,
        string name,
        string? schema,
        bool rowsAffectedReturned)
    {
        EntityType = entityType;
        _name = name;
        _schema = schema;
        _isRowsAffectedReturned = rowsAffectedReturned;
    }

    /// <summary>
    ///     Gets the entity type in which this stored procedure is defined.
    /// </summary>
    public virtual RuntimeEntityType EntityType { get; set; }

    /// <summary>
    ///     Adds a new parameter mapped to the property with the given name.
    /// </summary>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="direction">The direction.</param>
    /// <param name="forRowsAffected">Whether the parameter holds the rows affected.</param>
    /// <param name="propertyName">The name of the corresponding property.</param>
    /// <param name="forOriginalValue">Whether the parameter holds the original value.</param>
    public virtual RuntimeStoredProcedureParameter AddParameter(
        string name,
        ParameterDirection direction,
        bool forRowsAffected,
        string? propertyName,
        bool? forOriginalValue)
    {
        var parameter = new RuntimeStoredProcedureParameter(
            this,
            name,
            direction,
            forRowsAffected,
            propertyName,
            forOriginalValue);
        _parameters.Add(parameter);
        return parameter;
    }

    /// <summary>
    ///     Adds a new column of the result for this stored procedure mapped to the property with the given name
    /// </summary>
    /// <param name="name">The name of the result column.</param>
    /// <param name="forRowsAffected">Whether the column holds the rows affected.</param>
    /// <param name="propertyName">The name of the corresponding property.</param>
    public virtual RuntimeStoredProcedureResultColumn AddResultColumn(
        string name,
        bool forRowsAffected,
        string? propertyName)
    {
        var resultColumn = new RuntimeStoredProcedureResultColumn(
            this,
            name,
            forRowsAffected,
            propertyName);
        _resultColumns.Add(resultColumn);
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
    [EntityFrameworkInternal]
    public virtual DebugView DebugView
        => new(
            () => ((IStoredProcedure)this).ToDebugString(),
            () => ((IStoredProcedure)this).ToDebugString(MetadataDebugStringOptions.LongDefault));

    /// <inheritdoc />
    IReadOnlyEntityType IReadOnlyStoredProcedure.EntityType
    {
        [DebuggerStepThrough]
        get => EntityType;
    }

    /// <inheritdoc />
    IEntityType IStoredProcedure.EntityType
    {
        [DebuggerStepThrough]
        get => EntityType;
    }

    /// <inheritdoc />
    string? IReadOnlyStoredProcedure.Name
    {
        [DebuggerStepThrough]
        get => _name;
    }

    /// <inheritdoc />
    string IStoredProcedure.Name
    {
        [DebuggerStepThrough]
        get => _name;
    }

    /// <inheritdoc />
    string? IReadOnlyStoredProcedure.Schema
    {
        [DebuggerStepThrough]
        get => _schema;
    }

    /// <inheritdoc />
    bool IReadOnlyStoredProcedure.IsRowsAffectedReturned
    {
        [DebuggerStepThrough]
        get => _isRowsAffectedReturned;
    }

    /// <inheritdoc />
    IReadOnlyList<IReadOnlyStoredProcedureParameter> IReadOnlyStoredProcedure.Parameters
    {
        [DebuggerStepThrough]
        get => _parameters;
    }

    /// <inheritdoc />
    IReadOnlyList<IStoredProcedureParameter> IStoredProcedure.Parameters
    {
        [DebuggerStepThrough]
        get => _parameters;
    }

    /// <inheritdoc />
    IReadOnlyStoredProcedureParameter? IReadOnlyStoredProcedure.FindParameter(string propertyName)
        => _parameters.FirstOrDefault(
            (IReadOnlyStoredProcedureParameter p)
                => p.ForOriginalValue == false && p.PropertyName == propertyName);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IStoredProcedureParameter? IStoredProcedure.FindParameter(string propertyName)
        => (IStoredProcedureParameter?)((IReadOnlyStoredProcedure)this).FindParameter(propertyName);

    /// <inheritdoc />
    IReadOnlyStoredProcedureParameter? IReadOnlyStoredProcedure.FindOriginalValueParameter(string propertyName)
        => _parameters.FirstOrDefault(
            (IReadOnlyStoredProcedureParameter p)
                => p.ForOriginalValue == true && p.PropertyName == propertyName);

    /// <inheritdoc />
    IStoredProcedureParameter? IStoredProcedure.FindOriginalValueParameter(string propertyName)
        => (IStoredProcedureParameter?)((IReadOnlyStoredProcedure)this).FindOriginalValueParameter(propertyName);

    /// <inheritdoc />
    IReadOnlyStoredProcedureParameter? IReadOnlyStoredProcedure.FindRowsAffectedParameter()
        => _parameters.FirstOrDefault(
            (IStoredProcedureParameter p)
                => p.ForRowsAffected);

    /// <inheritdoc />
    IStoredProcedureParameter? IStoredProcedure.FindRowsAffectedParameter()
        => (IStoredProcedureParameter?)((IReadOnlyStoredProcedure)this).FindRowsAffectedParameter();

    /// <inheritdoc />
    IReadOnlyList<IReadOnlyStoredProcedureResultColumn> IReadOnlyStoredProcedure.ResultColumns
    {
        [DebuggerStepThrough]
        get => _resultColumns;
    }

    /// <inheritdoc />
    IReadOnlyList<IStoredProcedureResultColumn> IStoredProcedure.ResultColumns
    {
        [DebuggerStepThrough]
        get => _resultColumns;
    }

    /// <inheritdoc />
    IReadOnlyStoredProcedureResultColumn? IReadOnlyStoredProcedure.FindResultColumn(string propertyName)
        => _resultColumns.FirstOrDefault(
            (IReadOnlyStoredProcedureResultColumn c)
                => c.PropertyName == propertyName);

    /// <inheritdoc />
    IStoredProcedureResultColumn? IStoredProcedure.FindResultColumn(string propertyName)
        => (IStoredProcedureResultColumn?)((IReadOnlyStoredProcedure)this).FindResultColumn(propertyName);

    /// <inheritdoc />
    IReadOnlyStoredProcedureResultColumn? IReadOnlyStoredProcedure.FindRowsAffectedResultColumn()
        => _resultColumns.FirstOrDefault(
            (IReadOnlyStoredProcedureResultColumn c)
                => c.ForRowsAffected);

    /// <inheritdoc />
    IStoredProcedureResultColumn? IStoredProcedure.FindRowsAffectedResultColumn()
        => (IStoredProcedureResultColumn?)((IReadOnlyStoredProcedure)this).FindRowsAffectedResultColumn();

    /// <inheritdoc />
    IStoreStoredProcedure IStoredProcedure.StoreStoredProcedure
    {
        [DebuggerStepThrough]
        get => _storeStoredProcedure!;
    }

    /// <inheritdoc />
    IStoreStoredProcedure IRuntimeStoredProcedure.StoreStoredProcedure
    {
        get => _storeStoredProcedure!;
        set => _storeStoredProcedure = value;
    }
}

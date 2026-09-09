// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class StoredProcedureParameter :
    ConventionAnnotatable,
    IMutableStoredProcedureParameter,
    IConventionStoredProcedureParameter,
    IRuntimeStoredProcedureParameter
{
    private string? _name;
    private ParameterDirection? _direction;

    private ConfigurationSource? _nameConfigurationSource;
    private ConfigurationSource? _directionConfigurationSource;
    private InternalStoredProcedureParameterBuilder? _builder;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public StoredProcedureParameter(
        StoredProcedure storedProcedure,
        bool rowsAffected,
        string? propertyName,
        bool? originalValue)
    {
        StoredProcedure = storedProcedure;
        ForRowsAffected = rowsAffected;
        PropertyName = propertyName;
        ForOriginalValue = originalValue;
        if (rowsAffected)
        {
            _direction = ParameterDirection.Output;
            _directionConfigurationSource = ConfigurationSource.Explicit;
        }

        _builder = new InternalStoredProcedureParameterBuilder(this, storedProcedure.Builder.ModelBuilder);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalStoredProcedureParameterBuilder Builder
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
        => _builder is not null
            && StoredProcedure.IsInModel;

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
    public override bool IsReadOnly
        => ((Annotatable)StoredProcedure.EntityType).IsReadOnly;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual StoredProcedure StoredProcedure { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IStoreStoredProcedureParameter StoreParameter { get; set; } = default!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? PropertyName { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool? ForOriginalValue { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool ForRowsAffected { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Name
    {
        get
        {
            if (_name != null)
            {
                return _name;
            }

            if (ForRowsAffected)
            {
                return "RowsAffected";
            }

            var baseName = GetProperty().GetDefaultColumnName(
                ((IReadOnlyStoredProcedure)StoredProcedure).GetStoreIdentifier()!.Value)!;
            return ForOriginalValue ?? false
                ? Uniquifier.Truncate(baseName + "_Original", GetProperty().DeclaringType.Model.GetMaxIdentifierLength())
                : baseName;
        }
        set => SetName(value, ConfigurationSource.Explicit);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string SetName(string name, ConfigurationSource configurationSource)
    {
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
    public virtual ParameterDirection Direction
    {
        get => _direction ?? ParameterDirection.Input;
        set => SetDirection(value, ConfigurationSource.Explicit);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ParameterDirection SetDirection(ParameterDirection direction, ConfigurationSource configurationSource)
    {
        if (ForRowsAffected)
        {
            throw new InvalidOperationException(
                RelationalStrings.StoredProcedureParameterInvalidConfiguration(
                    nameof(Direction), Name, ((IReadOnlyStoredProcedure)StoredProcedure).GetStoreIdentifier()?.DisplayName()));
        }

        if (!IsValid(direction))
        {
            throw new InvalidOperationException(
                RelationalStrings.StoredProcedureParameterInvalidDirection(
                    direction, Name, ((IReadOnlyStoredProcedure)StoredProcedure).GetStoreIdentifier()?.DisplayName()));
        }

        _direction = direction;

        _directionConfigurationSource = configurationSource.Max(_directionConfigurationSource);

        return direction;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsValid(ParameterDirection direction)
        => direction switch
        {
            ParameterDirection.Output => ForOriginalValue != true,
            ParameterDirection.ReturnValue => false,
            _ => true
        };

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? GetDirectionConfigurationSource()
        => _directionConfigurationSource;

    private IMutableProperty GetProperty()
        => StoredProcedure.EntityType.FindProperty(PropertyName!)
            ?? StoredProcedure.EntityType.GetDerivedTypes().Select(t => t.FindDeclaredProperty(PropertyName!)!)
                .First(n => n != null);

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
    IMutableStoredProcedure IMutableStoredProcedureParameter.StoredProcedure
    {
        [DebuggerStepThrough]
        get => StoredProcedure;
    }

    /// <inheritdoc />
    IConventionStoredProcedure IConventionStoredProcedureParameter.StoredProcedure
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
    IConventionStoredProcedureParameterBuilder IConventionStoredProcedureParameter.Builder
    {
        [DebuggerStepThrough]
        get => Builder;
    }

    /// <inheritdoc />
    string IConventionStoredProcedureParameter.SetName(string name, bool fromDataAnnotation)
        => SetName(name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    ParameterDirection IConventionStoredProcedureParameter.SetDirection(ParameterDirection direction, bool fromDataAnnotation)
        => SetDirection(direction, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
}

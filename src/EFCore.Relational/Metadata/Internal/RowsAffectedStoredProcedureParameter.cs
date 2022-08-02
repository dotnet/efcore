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
public class RowsAffectedStoredProcedureParameter :
    ConventionAnnotatable,
    IMutableStoredProcedureParameter,
    IConventionStoredProcedureParameter,
    IRuntimeStoredProcedureParameter
{
    private string _name = "RowsAffected";
    
    private ConfigurationSource? _nameConfigurationSource;
    private InternalRowsAffectedStoredProcedureParameterBuilder? _builder;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public RowsAffectedStoredProcedureParameter(StoredProcedure storedProcedure)
    {
        StoredProcedure = storedProcedure;
        _builder = new(this, storedProcedure.Builder.ModelBuilder);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalRowsAffectedStoredProcedureParameterBuilder Builder
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
    public virtual bool? ForOriginalValue
        => null;
    
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool ForRowsAffected
        => true;
    
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? PropertyName => null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Name
    {
        get => _name;
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
        get => ParameterDirection.Output;
        set => throw new InvalidOperationException(
            RelationalStrings.StoredProcedureParameterInvalidConfiguration(
                nameof(Direction), Name, ((IReadOnlyStoredProcedure)StoredProcedure).GetStoreIdentifier()?.DisplayName()));
    }

    /// <summary>
    ///     Returns the configuration source for <see cref="IReadOnlyStoredProcedureParameter.Direction" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IReadOnlyStoredProcedureParameter.Direction" />.</returns>
    public virtual ConfigurationSource? GetDirectionConfigurationSource()
        => ConfigurationSource.Explicit;

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
        => Direction = direction;
}

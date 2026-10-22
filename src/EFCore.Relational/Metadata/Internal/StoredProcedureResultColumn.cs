// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class StoredProcedureResultColumn :
    ConventionAnnotatable,
    IMutableStoredProcedureResultColumn,
    IConventionStoredProcedureResultColumn,
    IRuntimeStoredProcedureResultColumn
{
    private string _name = "RowsAffected";

    private ConfigurationSource? _nameConfigurationSource;
    private InternalStoredProcedureResultColumnBuilder? _builder;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public StoredProcedureResultColumn(
        StoredProcedure storedProcedure,
        bool forRowsAffected,
        string? propertyName)
    {
        StoredProcedure = storedProcedure;
        ForRowsAffected = forRowsAffected;
        PropertyName = propertyName;
        _builder = new InternalStoredProcedureResultColumnBuilder(this, storedProcedure.Builder.ModelBuilder);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalStoredProcedureResultColumnBuilder Builder
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
    public virtual IStoreStoredProcedureResultColumn StoreResultColumn { get; set; } = default!;

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
    public virtual bool ForRowsAffected { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Name
    {
        get => ForRowsAffected
            ? _name
            : GetProperty().GetColumnName(
                ((IReadOnlyStoredProcedure)StoredProcedure).GetStoreIdentifier()!.Value)!;
        set => SetName(value, ConfigurationSource.Explicit);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? SetName(string name, ConfigurationSource configurationSource)
    {
        if (ForRowsAffected)
        {
            _name = name;

            _nameConfigurationSource = configurationSource.Max(_nameConfigurationSource);

            return name;
        }

        if (configurationSource == ConfigurationSource.Explicit)
        {
            GetProperty().SetColumnName(name, ((IReadOnlyStoredProcedure)StoredProcedure).GetStoreIdentifier()!.Value);
            return name;
        }

        return ((IConventionProperty)GetProperty()).SetColumnName(
            name, ((IReadOnlyStoredProcedure)StoredProcedure).GetStoreIdentifier()!.Value,
            fromDataAnnotation: configurationSource == ConfigurationSource.DataAnnotation);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? GetNameConfigurationSource()
        => ForRowsAffected
            ? _nameConfigurationSource
            : ((IConventionProperty)GetProperty()!)
            .GetColumnNameConfigurationSource(((IReadOnlyStoredProcedure)StoredProcedure).GetStoreIdentifier()!.Value);

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
        => ((IStoredProcedureResultColumn)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
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
    IMutableStoredProcedure IMutableStoredProcedureResultColumn.StoredProcedure
    {
        [DebuggerStepThrough]
        get => StoredProcedure;
    }

    /// <inheritdoc />
    IConventionStoredProcedure IConventionStoredProcedureResultColumn.StoredProcedure
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
    IConventionStoredProcedureResultColumnBuilder IConventionStoredProcedureResultColumn.Builder
    {
        [DebuggerStepThrough]
        get => Builder;
    }

    /// <inheritdoc />
    string? IConventionStoredProcedureResultColumn.SetName(string name, bool fromDataAnnotation)
        => SetName(name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
}

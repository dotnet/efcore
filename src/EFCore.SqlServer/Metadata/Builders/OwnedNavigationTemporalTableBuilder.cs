// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
///     and it is not designed to be directly constructed in your application code.
/// </summary>
public class OwnedNavigationTemporalTableBuilder
{
    private readonly OwnedNavigationBuilder _referenceOwnershipBuilder;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public OwnedNavigationTemporalTableBuilder(OwnedNavigationBuilder referenceOwnershipBuilder)
    {
        _referenceOwnershipBuilder = referenceOwnershipBuilder;
    }

    /// <summary>
    ///     Configures a history table for the entity mapped to a temporal table.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-temporal">Using SQL Server temporal tables with EF Core</see>
    ///     for more information.
    /// </remarks>
    /// <param name="name">The name of the history table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public virtual OwnedNavigationTemporalTableBuilder UseHistoryTable(string name)
        => UseHistoryTable(name, null);

    /// <summary>
    ///     Configures a history table for the entity mapped to a temporal table.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-temporal">Using SQL Server temporal tables with EF Core</see>
    ///     for more information.
    /// </remarks>
    /// <param name="name">The name of the history table.</param>
    /// <param name="schema">The schema of the history table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public virtual OwnedNavigationTemporalTableBuilder UseHistoryTable(string name, string? schema)
    {
        _referenceOwnershipBuilder.OwnedEntityType.SetHistoryTableName(name);
        _referenceOwnershipBuilder.OwnedEntityType.SetHistoryTableSchema(schema);

        return this;
    }

    /// <summary>
    ///     Returns an object that can be used to configure a period start property of the entity type mapped to a temporal table.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-temporal">Using SQL Server temporal tables with EF Core</see>
    ///     for more information.
    /// </remarks>
    /// <param name="propertyName">The name of the period start property.</param>
    /// <returns>An object that can be used to configure the period start property.</returns>
    public virtual OwnedNavigationTemporalPeriodPropertyBuilder HasPeriodStart(string propertyName)
    {
        _referenceOwnershipBuilder.OwnedEntityType.SetPeriodStartPropertyName(propertyName);
        var property = ConfigurePeriodProperty(propertyName);

#pragma warning disable EF1001 // Internal EF Core API usage.
        return new OwnedNavigationTemporalPeriodPropertyBuilder(new PropertyBuilder(property));
#pragma warning restore EF1001 // Internal EF Core API usage.
    }

    /// <summary>
    ///     Returns an object that can be used to configure a period end property of the entity type mapped to a temporal table.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-temporal">Using SQL Server temporal tables with EF Core</see>
    ///     for more information.
    /// </remarks>
    /// <param name="propertyName">The name of the period end property.</param>
    /// <returns>An object that can be used to configure the period end property.</returns>
    public virtual OwnedNavigationTemporalPeriodPropertyBuilder HasPeriodEnd(string propertyName)
    {
        _referenceOwnershipBuilder.OwnedEntityType.SetPeriodEndPropertyName(propertyName);
        var property = ConfigurePeriodProperty(propertyName);

#pragma warning disable EF1001 // Internal EF Core API usage.
        return new OwnedNavigationTemporalPeriodPropertyBuilder(new PropertyBuilder(property));
#pragma warning restore EF1001 // Internal EF Core API usage.
    }

    private IMutableProperty ConfigurePeriodProperty(string propertyName)
    {
        // TODO: Configure the property explicitly, but remove it if it's no longer used, issue #15898
        var conventionPropertyBuilder = _referenceOwnershipBuilder.GetInfrastructure().Property(
            typeof(DateTime),
            propertyName,
            setTypeConfigurationSource: false);

        // if convention builder is null, it means the property with this name exists, but it has incorrect type
        // we will throw in the model validation
        if (conventionPropertyBuilder != null)
        {
            conventionPropertyBuilder.ValueGenerated(ValueGenerated.OnAddOrUpdate);
        }

        return _referenceOwnershipBuilder.OwnedEntityType.FindProperty(propertyName)!;
    }

    #region Hidden System.Object members

    /// <summary>
    ///     Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string? ToString()
        => base.ToString();

    /// <summary>
    ///     Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns><see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override bool Equals(object? obj)
        => base.Equals(obj);

    /// <summary>
    ///     Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override int GetHashCode()
        => base.GetHashCode();

    #endregion
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
///     and it is not designed to be directly constructed in your application code.
/// </summary>
public class TableBuilder
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public TableBuilder(string? name, string? schema, EntityTypeBuilder entityTypeBuilder)
    {
        Name = name;
        Schema = schema;
        EntityTypeBuilder = entityTypeBuilder;
    }

    /// <summary>
    ///     The specified table name.
    /// </summary>
    public virtual string? Name { get; }

    /// <summary>
    ///     The specified table schema.
    /// </summary>
    public virtual string? Schema { get; }

    /// <summary>
    ///     The entity type being configured.
    /// </summary>
    public virtual IMutableEntityType Metadata => EntityTypeBuilder.Metadata;

    /// <summary>
    ///     The entity type builder.
    /// </summary>
    public virtual EntityTypeBuilder EntityTypeBuilder { get; }

    /// <summary>
    ///     Configures the table to be ignored by migrations.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
    /// </remarks>
    /// <param name="excluded">A value indicating whether the table should be managed by migrations.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public virtual TableBuilder ExcludeFromMigrations(bool excluded = true)
    {
        Metadata.SetIsTableExcludedFromMigrations(excluded);

        return this;
    }

    /// <summary>
    ///     Configures a database trigger on the table.
    /// </summary>
    /// <param name="name">The name of the trigger.</param>
    /// <returns>A builder that can be used to configure the database trigger.</returns>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-triggers">Database triggers</see> for more information and examples.
    /// </remarks>
    public virtual TriggerBuilder HasTrigger(string name)
        => new((Trigger)InternalTriggerBuilder.HasTrigger(
            (IConventionEntityType)Metadata,
            name,
            Name,
            Schema,
            ConfigurationSource.Explicit)!);

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

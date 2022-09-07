// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Provides a simple API for configuring a <see cref="IStoredProcedure" /> result column.
/// </summary>
/// <remarks>
///     Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
///     and it is not designed to be directly constructed in your application code.
/// </remarks>
public class StoredProcedureResultColumnBuilder : IInfrastructure<PropertyBuilder?>
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public StoredProcedureResultColumnBuilder(
        InternalStoredProcedureResultColumnBuilder builder,
        PropertyBuilder? propertyBuilder)
    {
        Builder = builder;
        PropertyBuilder = propertyBuilder;
    }

    /// <summary>
    ///     The stored procedure result column being configured.
    /// </summary>
    public virtual IMutableStoredProcedureResultColumn Metadata
        => Builder.Metadata;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual InternalStoredProcedureResultColumnBuilder Builder { get; }

    private PropertyBuilder? PropertyBuilder { get; }

    /// <summary>
    ///     Sets the name of the stored procedure result column.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> and
    ///     <see href="https://aka.ms/efcore-docs-saving-data">Saving data with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="name">The store type of the function parameter in the database.</param>
    /// <returns>The same builder instance so that further configuration calls can be chained.</returns>
    public virtual StoredProcedureResultColumnBuilder HasName(string name)
    {
        Check.NullButNotEmpty(name, nameof(name));

        Builder.HasName(name, ConfigurationSource.Explicit);

        return this;
    }

    /// <summary>
    ///     Adds or updates an annotation on the property for a specific stored procedure.
    ///     If an annotation with the key specified in <paramref name="annotation" />
    ///     already exists, its value will be updated.
    /// </summary>
    /// <param name="annotation">The key of the annotation to be added or updated.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual StoredProcedureResultColumnBuilder HasAnnotation(string annotation, object? value)
    {
        Check.NotEmpty(annotation, nameof(annotation));

        Builder.HasAnnotation(annotation, value, ConfigurationSource.Explicit);

        return this;
    }

    PropertyBuilder? IInfrastructure<PropertyBuilder?>.Instance
        => PropertyBuilder;

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
    // ReSharper disable once BaseObjectEqualsIsObjectEquals
    public override bool Equals(object? obj)
        => base.Equals(obj);

    /// <summary>
    ///     Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
    public override int GetHashCode()
        => base.GetHashCode();

    #endregion
}

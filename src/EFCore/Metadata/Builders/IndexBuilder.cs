// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Index = Microsoft.EntityFrameworkCore.Metadata.Internal.Index;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Provides a simple API for configuring an <see cref="IMutableIndex" />.
/// </summary>
/// <remarks>
///     <para>
///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
///         and it is not designed to be directly constructed in your application code.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and
///         examples.
///     </para>
/// </remarks>
public class IndexBuilder : IInfrastructure<IConventionIndexBuilder>
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public IndexBuilder(IMutableIndex index)
    {
        Builder = ((Index)index).Builder;
    }

    /// <summary>
    ///     The internal builder being used to configure the index.
    /// </summary>
    IConventionIndexBuilder IInfrastructure<IConventionIndexBuilder>.Instance
        => Builder;

    private InternalIndexBuilder Builder { get; }

    /// <summary>
    ///     The index being configured.
    /// </summary>
    public virtual IMutableIndex Metadata
        => Builder.Metadata;

    /// <summary>
    ///     Adds or updates an annotation on the index. If an annotation with the key specified in
    ///     <paramref name="annotation" />
    ///     already exists its value will be updated.
    /// </summary>
    /// <param name="annotation">The key of the annotation to be added or updated.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual IndexBuilder HasAnnotation(string annotation, object? value)
    {
        Check.NotEmpty(annotation, nameof(annotation));

        Builder.HasAnnotation(annotation, value, ConfigurationSource.Explicit);

        return this;
    }

    /// <summary>
    ///     Configures whether this index is unique (i.e. the value(s) for each instance must be unique).
    /// </summary>
    /// <param name="unique">A value indicating whether this index is unique.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual IndexBuilder IsUnique(bool unique = true)
    {
        Builder.IsUnique(unique, ConfigurationSource.Explicit);

        return this;
    }

    /// <summary>
    ///     Configures the sort order(s) for the columns of this index (ascending or descending).
    /// </summary>
    /// <param name="descending">
    ///     A set of values indicating whether each corresponding index column has descending sort order.
    ///     An empty list indicates that all index columns will have descending sort order.
    /// </param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual IndexBuilder IsDescending(params bool[] descending)
    {
        Builder.IsDescending(descending, ConfigurationSource.Explicit);

        return this;
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

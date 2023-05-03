// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Provides a simple API surface for configuring conventions.
/// </summary>
/// <remarks>
///     Instances of this class are returned from methods when using the <see cref="ModelConfigurationBuilder" /> API
///     and it is not designed to be directly constructed in your application code.
/// </remarks>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public class ConventionSetBuilder
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConventionSet _conventionSet;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public ConventionSetBuilder(ConventionSet conventionSet, IServiceProvider serviceProvider)
    {
        Check.NotNull(conventionSet, nameof(conventionSet));

        _conventionSet = conventionSet;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    ///     Replaces an existing convention with a derived convention. Also registers the new convention for any
    ///     convention types not implemented by the existing convention.
    /// </summary>
    /// <typeparam name="TImplementation">The type of the old convention.</typeparam>
    /// <param name="conventionFactory">The factory that creates the new convention.</param>
    public virtual void Replace<TImplementation>(Func<IServiceProvider, TImplementation> conventionFactory)
        where TImplementation : IConvention
    {
        var convention = conventionFactory(_serviceProvider);
        _conventionSet.Replace(convention);
    }

    /// <summary>
    ///     Adds a convention to the set.
    /// </summary>
    /// <param name="conventionFactory">The factory that creates the convention.</param>
    public virtual void Add(Func<IServiceProvider, IConvention> conventionFactory)
    {
        var convention = conventionFactory(_serviceProvider);
        _conventionSet.Add(convention);
    }

    /// <summary>
    ///     Removes the convention of the given type.
    /// </summary>
    /// <param name="conventionType">The convention type to remove.</param>
    public virtual void Remove(Type conventionType)
        => _conventionSet.Remove(conventionType);

    /// <summary>
    ///     Remove the convention of the given type.
    /// </summary>
    /// <typeparam name="TImplementaion">The type of convention to remove</typeparam>
    public virtual void Remove<TImplementaion>()
        where TImplementaion : IConvention
        => Remove(typeof(TImplementaion));

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

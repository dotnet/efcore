// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     A parameter object passed to <see cref="IMaterializationInterceptor" /> methods containing data about the instance
///     being materialized.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
/// </remarks>
public readonly struct MaterializationInterceptionData
{
    private readonly MaterializationContext _materializationContext;
    private readonly Dictionary<IPropertyBase, (object TypedAccessor, Func<MaterializationContext, object?> Accessor)> _valueAccessor;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    [UsedImplicitly]
    public MaterializationInterceptionData(
        MaterializationContext materializationContext,
        IEntityType entityType,
        QueryTrackingBehavior? queryTrackingBehavior,
        Dictionary<IPropertyBase, (object TypedAccessor, Func<MaterializationContext, object?> Accessor)> valueAccessor)
    {
        _materializationContext = materializationContext;
        _valueAccessor = valueAccessor;
        EntityType = entityType;
        QueryTrackingBehavior = queryTrackingBehavior;
    }

    /// <summary>
    ///     The current <see cref="DbContext" /> instance being used.
    /// </summary>
    public DbContext Context
        => _materializationContext.Context;

    /// <summary>
    ///     The type of the entity being materialized.
    /// </summary>
    public IEntityType EntityType { get; }

    /// <summary>
    ///     The query tracking behavior, or <see langword="null" /> if this materialization is not from a query.
    /// </summary>
    public QueryTrackingBehavior? QueryTrackingBehavior { get; }

    /// <summary>
    ///     Gets the property value for the property with the given name.
    /// </summary>
    /// <remarks>
    ///     This generic overload of this method will not cause a primitive or value-type property value to be boxed into
    ///     a heap-allocated object.
    /// </remarks>
    /// <param name="propertyName">The property name.</param>
    /// <returns>The property value.</returns>
    public T GetPropertyValue<T>(string propertyName)
        => GetPropertyValue<T>(GetProperty(propertyName));

    /// <summary>
    ///     Gets the property value for the property with the given name.
    /// </summary>
    /// <remarks>
    ///     This non-generic overload of this method will always cause a primitive or value-type property value to be boxed into
    ///     a heap-allocated object.
    /// </remarks>
    /// <param name="propertyName">The property name.</param>
    /// <returns>The property value.</returns>
    public object? GetPropertyValue(string propertyName)
        => GetPropertyValue(GetProperty(propertyName));

    private IPropertyBase GetProperty(string propertyName)
    {
        var property = (IPropertyBase?)EntityType.FindProperty(propertyName)
            ?? EntityType.FindServiceProperty(propertyName);

        if (property == null)
        {
            throw new ArgumentException(CoreStrings.PropertyNotFound(propertyName, EntityType.DisplayName()), nameof(propertyName));
        }

        return property;
    }

    /// <summary>
    ///     Gets the property value for the given property.
    /// </summary>
    /// <remarks>
    ///     This generic overload of this method will not cause a primitive or value-type property value to be boxed into
    ///     a heap-allocated object.
    /// </remarks>
    /// <param name="property">The property.</param>
    /// <returns>The property value.</returns>
    public T GetPropertyValue<T>(IPropertyBase property)
        => ((Func<MaterializationContext, T>)_valueAccessor[property].TypedAccessor)(_materializationContext);

    /// <summary>
    ///     Gets the property value for the given property.
    /// </summary>
    /// <remarks>
    ///     This non-generic overload of this method will always cause a primitive or value-type property value to be boxed into
    ///     a heap-allocated object.
    /// </remarks>
    /// <param name="property">The property.</param>
    /// <returns>The property value.</returns>
    public object? GetPropertyValue(IPropertyBase property)
        => _valueAccessor[property].Accessor(_materializationContext);
}

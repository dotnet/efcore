// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking;

/// <summary>
///     A collection of all property values for an entity.
/// </summary>
/// <remarks>
///     <para>
///         Objects of this type can be obtained from <see cref="EntityEntry.CurrentValues" />,
///         <see cref="EntityEntry.OriginalValues" />,  <see cref="EntityEntry.GetDatabaseValues" />,
///         or <see cref="EntityEntry.GetDatabaseValuesAsync" />.
///         Once obtained, the objects are usually used in various combinations to resolve optimistic
///         concurrency exceptions signaled by the throwing of a <see cref="DbUpdateConcurrencyException" />.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see> for more information and
///         examples.
///     </para>
/// </remarks>
public abstract class PropertyValues
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected PropertyValues(InternalEntityEntry internalEntry)
    {
        InternalEntry = internalEntry;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual InternalEntityEntry InternalEntry { [DebuggerStepThrough] get; }

    /// <summary>
    ///     Creates an instance of the entity type and sets all its properties using the
    ///     values from this object.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see> for more information and
    ///     examples.
    /// </remarks>
    /// <returns>The values of this object copied into a new entity instance.</returns>
    public abstract object ToObject();

    /// <summary>
    ///     Sets the values of this object by copying values from the given object.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The given object can be of any type.  Any property on the object with a name that
    ///         matches a property name in the entity type and can be read will be copied.  Other
    ///         properties will be ignored.  This allows, for example, copying of properties from
    ///         simple Data Transfer Objects (DTOs).
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see> for more information and
    ///         examples.
    ///     </para>
    /// </remarks>
    /// <param name="obj">The object to read values from.</param>
    public abstract void SetValues(object obj);

    /// <summary>
    ///     Creates a clone of the values in this object. Changes made to the new object will not be
    ///     reflected in this object and vice versa.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see> for more information and
    ///     examples.
    /// </remarks>
    /// <returns>A clone of this object.</returns>
    public abstract PropertyValues Clone();

    /// <summary>
    ///     Sets the values of this object by reading values from another <see cref="PropertyValues" />
    ///     object.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The other object must be based on the same type as this object, or a type derived
    ///         from the type for this object.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see> for more information and
    ///         examples.
    ///     </para>
    /// </remarks>
    /// <param name="propertyValues">The object from which values should be copied.</param>
    public abstract void SetValues(PropertyValues propertyValues);

    /// <summary>
    ///     Sets the values of this object by copying values from the given dictionary.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The keys of the dictionary must match property names. Any key in the dictionary
    ///         that does not match the name of a property in the entity type will be ignored.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see> for more information and
    ///         examples.
    ///     </para>
    /// </remarks>
    /// <param name="values">The dictionary to read values from.</param>
    public virtual void SetValues<TProperty>(IDictionary<string, TProperty> values)
    {
        Check.NotNull(values, nameof(values));

        foreach (var property in Properties)
        {
            if (values.TryGetValue(property.Name, out var value))
            {
                this[property] = value;
            }
        }
    }

    /// <summary>
    ///     Gets the properties for which this object is storing values.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see> for more information and
    ///     examples.
    /// </remarks>
    /// <value> The properties. </value>
    public abstract IReadOnlyList<IProperty> Properties { get; }

    /// <summary>
    ///     Gets the underlying entity type for which this object is storing values.
    /// </summary>
    public virtual IEntityType EntityType
    {
        [DebuggerStepThrough]
        get => InternalEntry.EntityType;
    }

    /// <summary>
    ///     Gets or sets the value of the property with the specified property name.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <returns>The value of the property.</returns>
    public abstract object? this[string propertyName] { get; set; }

    /// <summary>
    ///     Gets or sets the value of the property.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>The value of the property.</returns>
    public abstract object? this[IProperty property] { get; set; }

    /// <summary>
    ///     Gets the value of the property just like using the indexed property getter but
    ///     typed to the type of the generic parameter.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see> for more information and
    ///     examples.
    /// </remarks>
    /// <typeparam name="TValue">The type of the property.</typeparam>
    /// <param name="propertyName">The property name.</param>
    /// <returns>The value of the property.</returns>
    public abstract TValue GetValue<TValue>(string propertyName);

    /// <summary>
    ///     Try to gets the value of the property just like using the indexed property getter but
    ///     typed to the type of the generic parameter.
    ///     If property exists it return the value into the out parameter, otherwise the default value of TValue
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see> for more information and
    ///     examples.
    /// </remarks>
    /// <typeparam name="TValue">The type of the property.</typeparam>
    /// <param name="propertyName">The property name.</param>
    /// <param name="value">The property value if any.</param>
    /// <returns>True if the property exists, otherwise false.</returns>
    public virtual bool TryGetValue<TValue>(string propertyName, out TValue value)
    {
        var property = Properties.FirstOrDefault(p => p.Name == propertyName);
        if (property != null)
        {
            value = GetValue<TValue>(propertyName);
            return true;
        }

        value = default!;
        return false;
    }

    /// <summary>
    ///     Gets the value of the property just like using the indexed property getter but
    ///     typed to the type of the generic parameter.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see> for more information and
    ///     examples.
    /// </remarks>
    /// <typeparam name="TValue">The type of the property.</typeparam>
    /// <param name="property">The property.</param>
    /// <returns>The value of the property.</returns>
    public abstract TValue GetValue<TValue>(IProperty property);

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

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking;

/// <summary>
///     Provides access to change tracking information and operations for a given property of a complex type.
/// </summary>
/// <remarks>
///     <para>
///         Instances of this class are returned from methods when using the <see cref="ChangeTracker" /> API and it is
///         not designed to be directly constructed in your application code.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see> for more information and
///         examples.
///     </para>
/// </remarks>
public class ComplexPropertyEntry : MemberEntry
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public ComplexPropertyEntry(InternalEntityEntry internalEntry, IComplexProperty complexProperty)
        : base(internalEntry, complexProperty)
    {
    }

    /// <summary>
    ///     Gets or sets a value indicating whether any of the properties of the complex type have been modified
    ///     and should be updated in the database when <see cref="DbContext.SaveChanges()" /> is called.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Setting this value causes all of the properties of the complex type to be marked as modified or not as appropriate.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see> for more information and
    ///         examples.
    ///     </para>
    /// </remarks>
    public override bool IsModified
    {
        get => Metadata.ComplexType.GetFlattenedProperties().Any(property => InternalEntry.IsModified(property));
        set
        {
            foreach (var property in Metadata.ComplexType.GetFlattenedProperties())
            {
                InternalEntry.SetPropertyModified(property, isModified: value);
            }
        }
    }

    /// <summary>
    ///     Gets the metadata that describes the facets of this property and how it maps to the database.
    /// </summary>
    public new virtual IComplexProperty Metadata
        => (IComplexProperty)base.Metadata;

    /// <summary>
    ///     Provides access to change tracking information and operations for a given property of this complex type.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see> for more information and
    ///     examples.
    /// </remarks>
    /// <param name="property">The property to access information and operations for.</param>
    /// <returns>An object that exposes change tracking information and operations for the given property.</returns>
    public virtual PropertyEntry Property(IProperty property)
    {
        Check.NotNull(property, nameof(property));

        return new PropertyEntry(InternalEntry, property);
    }

    /// <summary>
    ///     Provides access to change tracking information and operations for a given property of this complex type.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see> for more information and
    ///     examples.
    /// </remarks>
    /// <param name="propertyName">The property to access information and operations for.</param>
    /// <returns>An object that exposes change tracking information and operations for the given property.</returns>
    public virtual PropertyEntry Property(string propertyName)
    {
        Check.NotEmpty(propertyName, nameof(propertyName));

        return new PropertyEntry(InternalEntry, Metadata.ComplexType.GetProperty(propertyName));
    }

    /// <summary>
    ///     Provides access to change tracking information and operations for all properties of this complex type.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see> for more information and
    ///     examples.
    /// </remarks>
    public virtual IEnumerable<PropertyEntry> Properties
        => Metadata.ComplexType.GetProperties().Select(property => new PropertyEntry(InternalEntry, property));

    /// <summary>
    ///     Provides access to change tracking information and operations for a given property of a nested complex type on this
    ///     complex type.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see> for more information and
    ///     examples.
    /// </remarks>
    /// <param name="property">The property to access information and operations for.</param>
    /// <returns>An object that exposes change tracking information and operations for the given property.</returns>
    public virtual ComplexPropertyEntry ComplexProperty(IComplexProperty property)
    {
        Check.NotNull(property, nameof(property));

        return new ComplexPropertyEntry(InternalEntry, property);
    }

    /// <summary>
    ///     Provides access to change tracking information and operations for a given property of a nested complex type on this
    ///     complex type.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see> for more information and
    ///     examples.
    /// </remarks>
    /// <param name="propertyName">The property to access information and operations for.</param>
    /// <returns>An object that exposes change tracking information and operations for the given property.</returns>
    public virtual ComplexPropertyEntry ComplexProperty(string propertyName)
    {
        Check.NotEmpty(propertyName, nameof(propertyName));

        return new ComplexPropertyEntry(InternalEntry, Metadata.ComplexType.GetComplexProperty(propertyName));
    }

    /// <summary>
    ///     Provides access to change tracking information and operations for all properties of nested complex types on this complex type.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see> for more information and
    ///     examples.
    /// </remarks>
    public virtual IEnumerable<ComplexPropertyEntry> ComplexProperties
        => Metadata.ComplexType.GetComplexProperties().Select(property => new ComplexPropertyEntry(InternalEntry, property));
}

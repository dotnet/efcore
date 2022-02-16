// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Pass a value from this enum to <see cref="ModelBuilder.UsePropertyAccessMode" />,
///     <see cref="EntityTypeBuilder.UsePropertyAccessMode" />, or
///     <see cref="PropertyBuilder.UsePropertyAccessMode" /> to change whether the property
///     or backing field will be used when reading and writing to a property or field.
/// </summary>
/// <remarks>
///     <para>
///         The default behavior is <see cref="PreferField" />. Prior to EF Core 3.0,
///         the default behavior was <see cref="PreferFieldDuringConstruction" />.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-property-access">Property versus field access in EF Core</see> for more information and
///         examples.
///     </para>
/// </remarks>
public enum PropertyAccessMode
{
    /// <summary>
    ///     Enforces that all accesses to the property must go through the field.
    /// </summary>
    /// <remarks>
    ///     An exception will be thrown if this mode is set and it is not possible to read from or write to the field.
    /// </remarks>
    Field,

    /// <summary>
    ///     Enforces that all accesses to the property must go through the field when new instances are being constructed. New instances are
    ///     typically constructed when entities are queried from the database. An exception will be thrown if this mode is set and it is not
    ///     possible to write to the field.
    /// </summary>
    /// <remarks>
    ///     All other uses of the property will go through the property getters and setters, unless this is not possible because, for
    ///     example, the property is read-only, in which case these accesses will also use the field.
    /// </remarks>
    FieldDuringConstruction,

    /// <summary>
    ///     Enforces that all accesses to the property must go through the property getters and setters, even when new objects are being
    ///     constructed.
    /// </summary>
    /// <remarks>
    ///     An exception will be thrown if this mode is set and it is not possible to read from or write to the property, for example
    ///     because it is read-only.
    /// </remarks>
    Property,

    /// <summary>
    ///     All accesses to the property goes directly to the field, unless the field is not known, in which case access goes through the
    ///     property.
    /// </summary>
    PreferField,

    /// <summary>
    ///     All accesses to the property when constructing new entity instances goes directly to the field, unless the field is not known,
    ///     in which case access goes through the property. All other uses of the property will go through the property getters and setters,
    ///     unless this is not possible because, for example, the property is read-only, in which case these accesses will also use the
    ///     field.
    /// </summary>
    PreferFieldDuringConstruction,

    /// <summary>
    ///     All accesses to the property go through the property, unless there is no property or it is missing a setter/getter, in which
    ///     case access goes directly to the field.
    /// </summary>
    PreferProperty
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     <para>
    ///         Pass a value from this enum to <see cref="ModelBuilder.UsePropertyAccessMode" />,
    ///         <see cref="EntityTypeBuilder.UsePropertyAccessMode" />, or
    ///         <see cref="PropertyBuilder.UsePropertyAccessMode" /> to change whether the property
    ///         or backing field will be used when reading and writing to a property or field.
    ///     </para>
    ///     <para>
    ///         The default behavior is <see cref="PreferField" />. Prior to EF Core 3.0,
    ///         the default behavior was <see cref="PreferFieldDuringConstruction" />.
    ///     </para>
    /// </summary>
    public enum PropertyAccessMode
    {
        /// <summary>
        ///     <para>
        ///         Enforces that all accesses to the property must go through the field.
        ///     </para>
        ///     <para>
        ///         An exception will be thrown if this mode is set and it is not possible to read
        ///         from or write to the field.
        ///     </para>
        /// </summary>
        Field,

        /// <summary>
        ///     <para>
        ///         Enforces that all accesses to the property must go through the field when
        ///         new instances are being constructed. New instances are typically constructed when
        ///         entities are queried from the database.
        ///         An exception will be thrown if this mode is set and it is not possible to
        ///         write to the field.
        ///     </para>
        ///     <para>
        ///         All other uses of the property will go through the property getters and setters,
        ///         unless this is not possible because, for example, the property is read-only, in which
        ///         case these accesses will also use the field.
        ///     </para>
        /// </summary>
        FieldDuringConstruction,

        /// <summary>
        ///     <para>
        ///         Enforces that all accesses to the property must go through the property
        ///         getters and setters, even when new objects are being constructed.
        ///     </para>
        ///     <para>
        ///         An exception will be thrown if this mode is set and it is not possible to read
        ///         from or write to the property, for example because it is read-only.
        ///     </para>
        /// </summary>
        Property,

        /// <summary>
        ///     <para>
        ///         All accesses to the property goes directly to the field, unless the field is
        ///         not known, in which as access goes through the property.
        ///     </para>
        /// </summary>
        PreferField,

        /// <summary>
        ///     <para>
        ///         All accesses to the property when constructing new entity instances goes directly
        ///         to the field, unless the field is not known, in which as access goes through the property.
        ///         All other uses of the property will go through the property getters and setters,
        ///         unless this is not possible because, for example, the property is read-only, in which
        ///         case these accesses will also use the field.
        ///     </para>
        /// </summary>
        PreferFieldDuringConstruction,

        /// <summary>
        ///     <para>
        ///         All accesses to the property go through the property, unless there is no property or
        ///         it is missing a setter/getter, in which as access goes directly to the field.
        ///     </para>
        /// </summary>
        PreferProperty
    }
}

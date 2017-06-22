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
    ///         If no access mode is set, then the backing field for a property will be used if possible
    ///         when constructing new instances of the entity. The property getter or setter will be used,
    ///         if possible, for all other accesses of the property. Note that when it is not possible
    ///         to use the field because it could not be found by convention and was not specified using
    ///         <see cref="PropertyBuilder.HasField" />, then the property will be used instead. Likewise,
    ///         when it is not possible to use the property getter or setter, for example when the
    ///         property is read-only, then the field will be used instead.
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
        ///     <para>
        ///         This access mode is similar to the default mode used if none has been set except
        ///         that it will throw an exception if it is not possible to write to the field for
        ///         entity construction. The default access mode will fall back to using the property
        ///         instead.
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
        Property
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Represents the mapping between a .NET type and a database type.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class CoreTypeMapping
    {
        private ValueComparer _comparer;
        private ValueComparer _keyComparer;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CoreTypeMapping" /> class.
        /// </summary>
        /// <param name="clrType"> The .NET type used in the EF model. </param>
        /// <param name="converter"> Converts types to and from the store whenever this mapping is used. </param>
        /// <param name="comparer"> Supports custom value snapshotting and comparisons. </param>
        /// <param name="keyComparer"> Supports custom comparisons between keys--e.g. PK to FK comparison. </param>
        public CoreTypeMapping(
            [NotNull] Type clrType,
            [CanBeNull] ValueConverter converter = null,
            [CanBeNull] ValueComparer comparer = null,
            [CanBeNull] ValueComparer keyComparer = null)
        {
            Check.NotNull(clrType, nameof(clrType));

            clrType = converter?.ModelClrType ?? clrType;
            ClrType = clrType;
            Converter = converter;

            if (comparer?.Type == clrType)
            {
                _comparer = comparer;
            }

            if (keyComparer?.Type == clrType)
            {
                _keyComparer = keyComparer;
            }
        }

        /// <summary>
        ///     Gets the .NET type used in the EF model.
        /// </summary>
        public virtual Type ClrType { get; }

        /// <summary>
        ///     Converts types to and from the store whenever this mapping is used.
        ///     May be null if no conversion is needed.
        /// </summary>
        public virtual ValueConverter Converter { get; }

        /// <summary>
        ///     A <see cref="ValueComparer" /> adds custom value snapshotting and comparison for
        ///     CLR types that cannot be compared with <see cref="object.Equals(object, object)" />
        ///     and/or need a deep copy when taking a snapshot.
        /// </summary>
        public virtual ValueComparer Comparer
            => NonCapturingLazyInitializer.EnsureInitialized(
                ref _comparer,
                this,
                c => CreateComparer(c.ClrType, favorStructuralComparisons: false));

        /// <summary>
        ///     A <see cref="ValueComparer" /> adds custom value comparison for use when
        ///     comparing key values to each other. For example, when comparing a PK to and FK.
        /// </summary>
        public virtual ValueComparer KeyComparer
            => NonCapturingLazyInitializer.EnsureInitialized(
                ref _keyComparer,
                this,
                c => CreateComparer(c.ClrType, favorStructuralComparisons: true));

        private static ValueComparer CreateComparer(Type clrType, bool favorStructuralComparisons)
            => (ValueComparer)Activator.CreateInstance(
                typeof(ValueComparer<>).MakeGenericType(clrType),
                new object[] { favorStructuralComparisons });

        /// <summary>
        ///     Returns a new copy of this type mapping with the given <see cref="ValueConverter" />
        ///     added.
        /// </summary>
        /// <param name="converter"> The converter to use. </param>
        /// <returns> A new type mapping </returns>
        public virtual CoreTypeMapping Clone([CanBeNull] ValueConverter converter)
            => new CoreTypeMapping(ClrType, ComposeConverter(converter));

        /// <summary>
        ///     Composes the given <see cref="ValueConverter" /> with any already in this mapping
        ///     and returns a new <see cref="ValueConverter" /> combining them together.
        /// </summary>
        /// <param name="converter"> The new converter. </param>
        /// <returns> The composed converter. </returns>
        protected virtual ValueConverter ComposeConverter([CanBeNull] ValueConverter converter)
            => converter == null ? Converter : converter.ComposeWith(Converter);
    }
}

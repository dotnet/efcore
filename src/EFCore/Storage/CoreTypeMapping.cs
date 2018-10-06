// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;

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
    public abstract class CoreTypeMapping
    {
        /// <summary>
        ///     Parameter object for use in the <see cref="CoreTypeMapping" /> hierarchy.
        /// </summary>
        protected readonly struct CoreTypeMappingParameters
        {
            /// <summary>
            ///     Creates a new <see cref="CoreTypeMappingParameters" /> parameter object.
            /// </summary>
            /// <param name="clrType"> The .NET type used in the EF model. </param>
            /// <param name="converter"> Converts types to and from the store whenever this mapping is used. </param>
            /// <param name="comparer"> Supports custom value snapshotting and comparisons. </param>
            /// <param name="keyComparer"> Supports custom comparisons between keys--e.g. PK to FK comparison. </param>
            /// <param name="valueGeneratorFactory"> An optional factory for creating a specific <see cref="ValueGenerator" />. </param>
            public CoreTypeMappingParameters(
                [NotNull] Type clrType,
                [CanBeNull] ValueConverter converter = null,
                [CanBeNull] ValueComparer comparer = null,
                [CanBeNull] ValueComparer keyComparer = null,
                [CanBeNull] Func<IProperty, IEntityType, ValueGenerator> valueGeneratorFactory = null)
                : this(clrType, converter, comparer, keyComparer, null, valueGeneratorFactory)
            {
            }

            /// <summary>
            ///     Creates a new <see cref="CoreTypeMappingParameters" /> parameter object.
            /// </summary>
            /// <param name="clrType"> The .NET type used in the EF model. </param>
            /// <param name="converter"> Converts types to and from the store whenever this mapping is used. </param>
            /// <param name="comparer"> Supports custom value snapshotting and comparisons. </param>
            /// <param name="keyComparer"> Supports custom comparisons between keys--e.g. PK to FK comparison. </param>
            /// <param name="structuralComparer"> Supports structural snapshotting needed for mutable reference types. </param>
            /// <param name="valueGeneratorFactory"> An optional factory for creating a specific <see cref="ValueGenerator" />. </param>
            public CoreTypeMappingParameters(
                [NotNull] Type clrType,
                [CanBeNull] ValueConverter converter,
                [CanBeNull] ValueComparer comparer,
                [CanBeNull] ValueComparer keyComparer,
                [CanBeNull] ValueComparer structuralComparer,
                [CanBeNull] Func<IProperty, IEntityType, ValueGenerator> valueGeneratorFactory)
            {
                Check.NotNull(clrType, nameof(clrType));

                ClrType = clrType;
                Converter = converter;
                Comparer = comparer;
                KeyComparer = keyComparer;
                StructuralComparer = structuralComparer;
                ValueGeneratorFactory = valueGeneratorFactory;
            }

            /// <summary>
            ///     The mapping CLR type.
            /// </summary>
            public Type ClrType { get; }

            /// <summary>
            ///     The mapping converter.
            /// </summary>
            public ValueConverter Converter { get; }

            /// <summary>
            ///     The mapping comparer.
            /// </summary>
            public ValueComparer Comparer { get; }

            /// <summary>
            ///     The mapping key comparer.
            /// </summary>
            public ValueComparer KeyComparer { get; }

            /// <summary>
            ///     The mapping structural comparer.
            /// </summary>
            public ValueComparer StructuralComparer { get; }

            /// <summary>
            ///     An optional factory for creating a specific <see cref="ValueGenerator" /> to use with
            ///     this mapping.
            /// </summary>
            public Func<IProperty, IEntityType, ValueGenerator> ValueGeneratorFactory { get; }

            /// <summary>
            ///     Creates a new <see cref="CoreTypeMappingParameters" /> parameter object with the given
            ///     converter composed with any existing converter and set on the new parameter object.
            /// </summary>
            /// <param name="converter"> The converter. </param>
            /// <returns> The new parameter object. </returns>
            public CoreTypeMappingParameters WithComposedConverter([CanBeNull] ValueConverter converter)
                => new CoreTypeMappingParameters(
                    ClrType,
                    converter == null ? Converter : converter.ComposeWith(Converter),
                    Comparer,
                    KeyComparer,
                    StructuralComparer,
                    ValueGeneratorFactory);
        }

        private ValueComparer _comparer;
        private ValueComparer _keyComparer;
        private ValueComparer _structuralComparer;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CoreTypeMapping" /> class.
        /// </summary>
        /// <param name="parameters"> The parameters for this mapping. </param>
        protected CoreTypeMapping(CoreTypeMappingParameters parameters)
        {
            Parameters = parameters;

            var converter = parameters.Converter;

            var clrType = converter?.ModelClrType ?? parameters.ClrType;
            ClrType = clrType;

            if (parameters.Comparer?.Type == clrType)
            {
                _comparer = parameters.Comparer;
            }

            if (parameters.KeyComparer?.Type == clrType)
            {
                _keyComparer = parameters.KeyComparer;
            }

            if (parameters.StructuralComparer?.Type == clrType)
            {
                _structuralComparer = parameters.StructuralComparer;
            }

            ValueGeneratorFactory = parameters.ValueGeneratorFactory
                                    ?? converter?.MappingHints?.ValueGeneratorFactory;
        }

        /// <summary>
        ///     Returns the parameters used to create this type mapping.
        /// </summary>
        protected virtual CoreTypeMappingParameters Parameters { get; }

        /// <summary>
        ///     Gets the .NET type used in the EF model.
        /// </summary>
        public virtual Type ClrType { get; }

        /// <summary>
        ///     Converts types to and from the store whenever this mapping is used.
        ///     May be null if no conversion is needed.
        /// </summary>
        public virtual ValueConverter Converter => Parameters.Converter;

        /// <summary>
        ///     An optional factory for creating a specific <see cref="ValueGenerator" /> to use with
        ///     this mapping.
        /// </summary>
        public virtual Func<IProperty, IEntityType, ValueGenerator> ValueGeneratorFactory { get; }

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

        /// <summary>
        ///     A <see cref="ValueComparer" /> adds custom value comparison for use when
        ///     a deep/structural copy and/or comparison is needed.
        /// </summary>
        public virtual ValueComparer StructuralComparer
            => _structuralComparer ?? KeyComparer;

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
        public abstract CoreTypeMapping Clone([CanBeNull] ValueConverter converter);

        /// <summary>
        ///     Creates a an expression tree that can be used to generate code for the literal value.
        ///     Currently, only very basic expressions such as constructor calls and factory methods taking
        ///     simple constants are supported.
        /// </summary>
        /// <param name="value"> The value for which a literal is needed. </param>
        /// <returns> An expression tree that can be used to generate code for the literal value. </returns>
        public virtual Expression GenerateCodeLiteral([NotNull] object value)
            => throw new NotSupportedException(CoreStrings.LiteralGenerationNotSupported(ClrType.ShortDisplayName()));
    }
}

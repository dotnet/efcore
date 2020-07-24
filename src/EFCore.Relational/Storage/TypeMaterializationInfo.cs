// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     Associates a <see cref="RelationalTypeMapping" /> with an optional <see cref="IProperty" />
    ///     and an index into the data reader for use when reading and converting values from the database.
    /// </summary>
    public class TypeMaterializationInfo
    {
        /// <summary>
        ///     Creates a new <see cref="TypeMaterializationInfo" /> instance.
        /// </summary>
        /// <param name="modelClrType"> The type that is needed in the model after conversion. </param>
        /// <param name="property"> The property associated with the type, or <see langword="null" /> if none. </param>
        /// <param name="typeMappingSource"> The type mapping source to use to find a mapping if the property does not have one already bound. </param>
        /// <param name="index">
        ///     The index of the underlying result set that should be used for this type,
        ///     or -1 if no index mapping is needed.
        /// </param>
        [Obsolete]
        public TypeMaterializationInfo(
            [NotNull] Type modelClrType,
            [CanBeNull] IProperty property,
            [CanBeNull] IRelationalTypeMappingSource typeMappingSource,
            int index = -1)
            : this(modelClrType, property, typeMappingSource, null, index)
        {
        }

        /// <summary>
        ///     Creates a new <see cref="TypeMaterializationInfo" /> instance.
        /// </summary>
        /// <param name="modelClrType"> The type that is needed in the model after conversion. </param>
        /// <param name="property"> The property associated with the type, or <see langword="null" /> if none. </param>
        /// <param name="typeMappingSource"> The type mapping source to use to find a mapping if the property does not have one already bound. </param>
        /// <param name="fromLeftOuterJoin"> Whether or not the value is coming from a LEFT OUTER JOIN operation. </param>
        /// <param name="index">
        ///     The index of the underlying result set that should be used for this type,
        ///     or -1 if no index mapping is needed.
        /// </param>
        [Obsolete]
        public TypeMaterializationInfo(
            [NotNull] Type modelClrType,
            [CanBeNull] IProperty property,
            [CanBeNull] IRelationalTypeMappingSource typeMappingSource,
            bool? fromLeftOuterJoin,
            int index)
            : this(modelClrType, property, typeMappingSource, fromLeftOuterJoin, index, mapping: null)
        {
        }

        /// <summary>
        ///     Creates a new <see cref="TypeMaterializationInfo" /> instance.
        /// </summary>
        /// <param name="modelClrType"> The type that is needed in the model after conversion. </param>
        /// <param name="property"> The property associated with the type, or <see langword="null" /> if none. </param>
        /// <param name="mapping"> The type mapping to use or <see langword="null" /> to infer one. </param>
        /// <param name="nullable"> A value indicating whether the value could be null. </param>
        public TypeMaterializationInfo(
            [NotNull] Type modelClrType,
            [CanBeNull] IProperty property,
            [NotNull] RelationalTypeMapping mapping,
            bool? nullable = null)
        {
            Check.NotNull(modelClrType, nameof(modelClrType));

            ProviderClrType = mapping?.Converter?.ProviderClrType
                ?? modelClrType;

            ModelClrType = modelClrType;
            Mapping = mapping;
            Property = property;
            IsNullable = nullable;
        }

        /// <summary>
        ///     Creates a new <see cref="TypeMaterializationInfo" /> instance.
        /// </summary>
        /// <param name="modelClrType"> The type that is needed in the model after conversion. </param>
        /// <param name="property"> The property associated with the type, or <see langword="null" /> if none. </param>
        /// <param name="typeMappingSource"> The type mapping source to use to find a mapping if the property does not have one already bound. </param>
        /// <param name="fromLeftOuterJoin"> Whether or not the value is coming from a LEFT OUTER JOIN operation. </param>
        /// <param name="index">
        ///     The index of the underlying result set that should be used for this type,
        ///     or -1 if no index mapping is needed.
        /// </param>
        /// <param name="mapping"> The type mapping to use or <see langword="null" /> to infer one. </param>
        [Obsolete]
        public TypeMaterializationInfo(
            [NotNull] Type modelClrType,
            [CanBeNull] IProperty property,
            [CanBeNull] IRelationalTypeMappingSource typeMappingSource,
            bool? fromLeftOuterJoin,
            int index = -1,
            [CanBeNull] RelationalTypeMapping mapping = null)
        {
            Check.NotNull(modelClrType, nameof(modelClrType));

            if (mapping == null)
            {
                mapping = property?.GetRelationalTypeMapping()
                    ?? typeMappingSource?.GetMapping(modelClrType);
            }

            ProviderClrType = mapping?.Converter?.ProviderClrType
                ?? modelClrType;

            ModelClrType = modelClrType;
            Mapping = mapping;
            Property = property;
            Index = index;
            IsFromLeftOuterJoin = fromLeftOuterJoin;
        }

        /// <summary>
        ///     The type that will be read from the database provider.
        /// </summary>
        public virtual Type ProviderClrType { get; }

        /// <summary>
        ///     The type that is needed in the model after conversion.
        /// </summary>
        public virtual Type ModelClrType { get; }

        /// <summary>
        ///     The type mapping for the value to be read.
        /// </summary>
        public virtual RelationalTypeMapping Mapping { get; }

        /// <summary>
        ///     The property associated with the type, or <see langword="null" /> if none.
        /// </summary>
        public virtual IProperty Property { get; }

        /// <summary>
        ///     The index of the underlying result set that should be used for this type,
        ///     or -1 if no index mapping is needed.
        /// </summary>
        [Obsolete]
        public virtual int Index { get; } = -1;

        /// <summary>
        ///     Whether or not the value is coming from a LEFT OUTER JOIN operation.
        /// </summary>
        [Obsolete]
        public virtual bool? IsFromLeftOuterJoin { get; }

        /// <summary>
        ///     Whether or not the value can be null.
        /// </summary>
        public virtual bool? IsNullable { get; }

        /// <summary>
        ///     Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other"> The object to compare with the current object. </param>
        /// <returns> <see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />. </returns>
        protected virtual bool Equals([NotNull] TypeMaterializationInfo other)
            => ProviderClrType == other.ProviderClrType
                && ModelClrType == other.ModelClrType
                && Equals(Mapping, other.Mapping)
                && Equals(Property, other.Property)
#pragma warning disable CS0612 // Type or member is obsolete
                && Index == other.Index
                && IsFromLeftOuterJoin == other.IsFromLeftOuterJoin
#pragma warning restore CS0612 // Type or member is obsolete
                && IsNullable == other.IsNullable;

        /// <summary>
        ///     Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj"> The object to compare with the current object. </param>
        /// <returns> <see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />. </returns>
        public override bool Equals(object obj)
            => !(obj is null)
                && (ReferenceEquals(this, obj)
                    || obj.GetType() == GetType()
                    && Equals((TypeMaterializationInfo)obj));

        /// <summary>
        ///     Serves as the default hash function.
        /// </summary>
        /// <returns> A hash code for the current object. </returns>
        public override int GetHashCode()
#pragma warning disable CS0612 // Type or member is obsolete
            => HashCode.Combine(ProviderClrType, ModelClrType, Mapping, Property, Index, IsFromLeftOuterJoin, IsNullable);
#pragma warning restore CS0612 // Type or member is obsolete
    }
}

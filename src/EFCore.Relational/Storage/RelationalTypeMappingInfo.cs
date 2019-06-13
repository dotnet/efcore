// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     Describes metadata needed to decide on a relational type mapping for
    ///     a property, type, or provider-specific relational type name.
    /// </summary>
    public readonly struct RelationalTypeMappingInfo : IEquatable<RelationalTypeMappingInfo>
    {
        private readonly TypeMappingInfo _coreTypeMappingInfo;

        /// <summary>
        ///     Creates a new instance of <see cref="RelationalTypeMappingInfo" />.
        /// </summary>
        /// <param name="property"> The property for which mapping is needed. </param>
        public RelationalTypeMappingInfo([NotNull] IProperty property)
            : this(property.FindPrincipals())
        {
        }

        /// <summary>
        ///     Creates a new instance of <see cref="RelationalTypeMappingInfo" />.
        /// </summary>
        /// <param name="principals"> The principal property chain for the property for which mapping is needed. </param>
        /// <param name="storeTypeName"> The provider-specific relational type name for which mapping is needed. </param>
        /// <param name="storeTypeNameBase"> The provider-specific relational type name, with any facets removed. </param>
        /// <param name="fallbackUnicode"> Specifies a fallback Specifies Unicode or ANSI mapping for the mapping, in case one isn't found at the core level, or <c>null</c> for default. </param>
        /// <param name="fixedLength"> Specifies a fixed length mapping, or <c>null</c> for default. </param>
        /// <param name="fallbackSize"> Specifies a fallback size for the mapping, in case one isn't found at the core level, or <c>null</c> for default. </param>
        /// <param name="fallbackPrecision"> Specifies a fallback precision for the mapping, in case one isn't found at the core level, or <c>null</c> for default. </param>
        /// <param name="fallbackScale"> Specifies a fallback scale for the mapping, in case one isn't found at the core level, or <c>null</c> for default. </param>
        public RelationalTypeMappingInfo(
            [NotNull] IReadOnlyList<IProperty> principals,
            [CanBeNull] string storeTypeName = null,
            [CanBeNull] string storeTypeNameBase = null,
            bool? fallbackUnicode = null,
            bool? fixedLength = null,
            int? fallbackSize = null,
            int? fallbackPrecision = null,
            int? fallbackScale = null)
        {
            _coreTypeMappingInfo = new TypeMappingInfo(principals, fallbackUnicode, fallbackSize, fallbackPrecision, fallbackScale);

            IsFixedLength = fixedLength;
            StoreTypeName = storeTypeName;
            StoreTypeNameBase = storeTypeNameBase;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="RelationalTypeMappingInfo" />.
        /// </summary>
        /// <param name="storeTypeName"> The provider-specific relational type name for which mapping is needed. </param>
        /// <param name="storeTypeNameBase"> The provider-specific relational type name, with any facets removed. </param>
        /// <param name="unicode"> Specifies Unicode or ANSI mapping, or <c>null</c> for default. </param>
        /// <param name="size"> Specifies a size for the mapping, or <c>null</c> for default. </param>
        /// <param name="precision"> Specifies a precision for the mapping, or <c>null</c> for default. </param>
        /// <param name="scale"> Specifies a scale for the mapping, or <c>null</c> for default. </param>
        public RelationalTypeMappingInfo(
            [NotNull] string storeTypeName,
            [NotNull] string storeTypeNameBase,
            bool? unicode,
            int? size,
            int? precision,
            int? scale)
        {
            // Note: Empty string is allowed for store type name because SQLite
            Check.NotNull(storeTypeName, nameof(storeTypeName));
            Check.NotNull(storeTypeNameBase, nameof(storeTypeNameBase));

            _coreTypeMappingInfo = new TypeMappingInfo(null, false, unicode, size, null, precision, scale);
            StoreTypeName = storeTypeName;
            StoreTypeNameBase = storeTypeNameBase;
            IsFixedLength = null;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="RelationalTypeMappingInfo" />.
        /// </summary>
        /// <param name="member"> The property or field for which mapping is needed. </param>
        /// <param name="storeTypeName"> The provider-specific relational type name for which mapping is needed. </param>
        /// <param name="storeTypeNameBase"> The provider-specific relational type name, with any facets removed. </param>
        /// <param name="unicode"> Specifies Unicode or ANSI mapping, or <c>null</c> for default. </param>
        /// <param name="size"> Specifies a size for the mapping, or <c>null</c> for default. </param>
        /// <param name="precision"> Specifies a precision for the mapping, or <c>null</c> for default. </param>
        /// <param name="scale"> Specifies a scale for the mapping, or <c>null</c> for default. </param>
        public RelationalTypeMappingInfo(
            [NotNull] MemberInfo member,
            [CanBeNull] string storeTypeName = null,
            [CanBeNull] string storeTypeNameBase = null,
            bool? unicode = null,
            int? size = null,
            int? precision = null,
            int? scale = null)
        {
            Check.NotNull(member, nameof(member));

            _coreTypeMappingInfo = new TypeMappingInfo(member, unicode, size, precision, scale);

            StoreTypeName = storeTypeName;
            StoreTypeNameBase = storeTypeNameBase;
            IsFixedLength = null;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="RelationalTypeMappingInfo" /> with the given <see cref="ValueConverterInfo" />.
        /// </summary>
        /// <param name="source"> The source info. </param>
        /// <param name="converter"> The converter to apply. </param>
        public RelationalTypeMappingInfo(
            in RelationalTypeMappingInfo source,
            in ValueConverterInfo converter)
        {
            _coreTypeMappingInfo = new TypeMappingInfo(
                source._coreTypeMappingInfo,
                converter,
                source.IsUnicode,
                source.Size,
                source.Precision,
                source.Scale);

            var mappingHints = converter.MappingHints;

            StoreTypeName = source.StoreTypeName;
            StoreTypeNameBase = source.StoreTypeNameBase;
            IsFixedLength = source.IsFixedLength ?? (mappingHints as RelationalConverterMappingHints)?.IsFixedLength;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="TypeMappingInfo" />.
        /// </summary>
        /// <param name="type"> The CLR type in the model for which mapping is needed. </param>
        /// <param name="storeTypeName"> The database type name. </param>
        /// <param name="storeTypeNameBase"> The provider-specific relational type name, with any facets removed. </param>
        /// <param name="keyOrIndex"> If <c>true</c>, then a special mapping for a key or index may be returned. </param>
        /// <param name="unicode"> Specifies Unicode or ANSI mapping, or <c>null</c> for default. </param>
        /// <param name="size"> Specifies a size for the mapping, or <c>null</c> for default. </param>
        /// <param name="rowVersion"> Specifies a row-version, or <c>null</c> for default. </param>
        /// <param name="fixedLength"> Specifies a fixed length mapping, or <c>null</c> for default. </param>
        /// <param name="precision"> Specifies a precision for the mapping, or <c>null</c> for default. </param>
        /// <param name="scale"> Specifies a scale for the mapping, or <c>null</c> for default. </param>
        public RelationalTypeMappingInfo(
            [NotNull] Type type,
            [CanBeNull] string storeTypeName = null,
            [CanBeNull] string storeTypeNameBase = null,
            bool keyOrIndex = false,
            bool? unicode = null,
            int? size = null,
            bool? rowVersion = null,
            bool? fixedLength = null,
            int? precision = null,
            int? scale = null)
        {
            _coreTypeMappingInfo = new TypeMappingInfo(type, keyOrIndex, unicode, size, rowVersion, precision, scale);

            IsFixedLength = fixedLength;
            StoreTypeName = storeTypeName;
            StoreTypeNameBase = storeTypeNameBase;
        }

        /// <summary>
        ///     The provider-specific relational type name for which mapping is needed.
        /// </summary>
        public string StoreTypeName { get; }

        /// <summary>
        ///     The provider-specific relational type name, with any facets removed.
        /// </summary>
        public string StoreTypeNameBase { get; }

        /// <summary>
        ///     Indicates the store-size to use for the mapping, or null if none.
        /// </summary>
        public int? Size => _coreTypeMappingInfo.Size;

        /// <summary>
        ///     The suggested precision of the mapped data type.
        /// </summary>
        public int? Precision => _coreTypeMappingInfo.Precision;

        /// <summary>
        ///     The suggested scale of the mapped data type.
        /// </summary>
        public int? Scale => _coreTypeMappingInfo.Scale;

        /// <summary>
        ///     Whether or not the mapped data type is fixed length.
        /// </summary>
        public bool? IsFixedLength { get; }

        /// <summary>
        ///     Indicates whether or not the mapping is part of a key or index.
        /// </summary>
        public bool IsKeyOrIndex => _coreTypeMappingInfo.IsKeyOrIndex;

        /// <summary>
        ///     Indicates whether or not the mapping supports Unicode, or null if not defined.
        /// </summary>
        public bool? IsUnicode => _coreTypeMappingInfo.IsUnicode;

        /// <summary>
        ///     Indicates whether or not the mapping will be used for a row version, or null if not defined.
        /// </summary>
        public bool? IsRowVersion => _coreTypeMappingInfo.IsRowVersion;

        /// <summary>
        ///     The CLR type in the model.
        /// </summary>
        public Type ClrType => _coreTypeMappingInfo.ClrType;

        /// <summary>
        ///     Returns a new <see cref="TypeMappingInfo" /> with the given converter applied.
        /// </summary>
        /// <param name="converterInfo"> The converter to apply. </param>
        /// <returns> The new mapping info. </returns>
        public RelationalTypeMappingInfo WithConverter(in ValueConverterInfo converterInfo)
            => new RelationalTypeMappingInfo(this, converterInfo);

        /// <summary>
        ///     Compares this <see cref="RelationalTypeMappingInfo" /> to another to check if they represent the same mapping.
        /// </summary>
        /// <param name="other"> The other object. </param>
        /// <returns> <c>True</c> if they represent the same mapping; <c>false</c> otherwise. </returns>
        public bool Equals(RelationalTypeMappingInfo other)
            => _coreTypeMappingInfo.Equals(other._coreTypeMappingInfo)
               && IsFixedLength == other.IsFixedLength
               && StoreTypeName == other.StoreTypeName;

        /// <summary>
        ///     Compares this <see cref="RelationalTypeMappingInfo" /> to another to check if they represent the same mapping.
        /// </summary>
        /// <param name="obj"> The other object. </param>
        /// <returns> <c>True</c> if they represent the same mapping; <c>false</c> otherwise. </returns>
        public override bool Equals(object obj)
            => obj != null
               && obj.GetType() == GetType()
               && Equals((RelationalTypeMappingInfo)obj);

        /// <summary>
        ///     Returns a hash code for this object.
        /// </summary>
        /// <returns> The hash code. </returns>
        public override int GetHashCode() => HashCode.Combine(_coreTypeMappingInfo, StoreTypeName, IsFixedLength);
    }
}

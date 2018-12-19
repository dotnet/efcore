// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
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
        private readonly int? _parsedSize;
        private readonly int? _parsedPrecision;
        private readonly int? _parsedScale;
        private readonly bool _isMax;

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
        public RelationalTypeMappingInfo([NotNull] IReadOnlyList<IProperty> principals)
        {
            _coreTypeMappingInfo = new TypeMappingInfo(principals);

            string storeTypeName = null;
            bool? fixedLength = null;
            for (var i = 0; i < principals.Count; i++)
            {
                var principal = principals[i];
                if (storeTypeName == null)
                {
                    var columnType = (string)principal[RelationalAnnotationNames.ColumnType];
                    if (columnType != null)
                    {
                        storeTypeName = columnType;
                    }
                }

                if (fixedLength == null)
                {
                    fixedLength = principal.Relational().IsFixedLength;
                }
            }

            IsFixedLength = fixedLength;
            StoreTypeName = storeTypeName;
            StoreTypeNameBase = ParseStoreTypeName(storeTypeName, out _parsedSize, out _parsedPrecision, out _parsedScale, out _isMax);
        }

        /// <summary>
        ///     Creates a new instance of <see cref="RelationalTypeMappingInfo" />.
        /// </summary>
        /// <param name="type"> The CLR type in the model for which mapping is needed. </param>
        public RelationalTypeMappingInfo([NotNull] Type type)
        {
            _coreTypeMappingInfo = new TypeMappingInfo(type);
            StoreTypeName = null;
            StoreTypeNameBase = null;
            IsFixedLength = null;
            _parsedSize = null;
            _parsedPrecision = null;
            _parsedScale = null;
            _isMax = false;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="RelationalTypeMappingInfo" />.
        /// </summary>
        /// <param name="storeTypeName"> The provider-specific relational type name for which mapping is needed. </param>
        public RelationalTypeMappingInfo([NotNull] string storeTypeName)
        {
            // Note: Empty string is allowed for store type name because SQLite
            Check.NotNull(storeTypeName, nameof(storeTypeName));

            _coreTypeMappingInfo = new TypeMappingInfo();
            StoreTypeName = storeTypeName;
            StoreTypeNameBase = ParseStoreTypeName(storeTypeName, out _parsedSize, out _parsedPrecision, out _parsedScale, out _isMax);
            IsFixedLength = null;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="RelationalTypeMappingInfo" />.
        /// </summary>
        /// <param name="member"> The property or field for which mapping is needed. </param>
        public RelationalTypeMappingInfo([NotNull] MemberInfo member)
        {
            Check.NotNull(member, nameof(member));

            _coreTypeMappingInfo = new TypeMappingInfo(member);

            if (Attribute.IsDefined(member, typeof(ColumnAttribute), inherit: true))
            {
                var attribute = member.GetCustomAttributes<ColumnAttribute>(inherit: true).First();
                StoreTypeName = attribute.TypeName;
                StoreTypeNameBase = ParseStoreTypeName(
                    attribute.TypeName, out _parsedSize, out _parsedPrecision, out _parsedScale, out _isMax);
            }
            else
            {
                StoreTypeName = null;
                StoreTypeNameBase = null;
                _parsedSize = null;
                _parsedPrecision = null;
                _parsedScale = null;
                _isMax = false;
            }

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
            StoreTypeNameBase = ParseStoreTypeName(source.StoreTypeName, out _parsedSize, out _parsedPrecision, out _parsedScale, out _isMax);
            IsFixedLength = source.IsFixedLength ?? (mappingHints as RelationalConverterMappingHints)?.IsFixedLength;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="TypeMappingInfo" />.
        /// </summary>
        /// <param name="type"> The CLR type in the model for which mapping is needed. </param>
        /// <param name="storeTypeName"> The database type name. </param>
        /// <param name="keyOrIndex"> If <c>true</c>, then a special mapping for a key or index may be returned. </param>
        /// <param name="unicode"> Specifies Unicode or ANSI mapping, or <c>null</c> for default. </param>
        /// <param name="size"> Specifies a size for the mapping, or <c>null</c> for default. </param>
        /// <param name="rowVersion"> Specifies a row-version, or <c>null</c> for default. </param>
        /// <param name="fixedLength"> Specifies a fixed length mapping, or <c>null</c> for default. </param>
        /// <param name="precision"> Specifies a precision for the mapping, or <c>null</c> for default. </param>
        /// <param name="scale"> Specifies a scale for the mapping, or <c>null</c> for default. </param>
        public RelationalTypeMappingInfo(
            [NotNull] Type type,
            [CanBeNull] string storeTypeName,
            bool keyOrIndex,
            bool? unicode,
            int? size,
            bool? rowVersion,
            bool? fixedLength,
            int? precision,
            int? scale)
        {
            _coreTypeMappingInfo = new TypeMappingInfo(type, keyOrIndex, unicode, size, rowVersion, precision, scale);

            IsFixedLength = fixedLength;
            StoreTypeName = storeTypeName;
            StoreTypeNameBase = ParseStoreTypeName(storeTypeName, out _parsedSize, out _parsedPrecision, out _parsedScale, out _isMax);
        }

        private static string ParseStoreTypeName(
            string storeTypeName,
            out int? size,
            out int? precision,
            out int? scale,
            out bool isMax)
        {
            size = null;
            precision = null;
            scale = null;
            isMax = false;

            if (storeTypeName != null)
            {
                var openParen = storeTypeName.IndexOf("(", StringComparison.Ordinal);
                if (openParen > 0)
                {
                    var closeParen = storeTypeName.IndexOf(")", openParen + 1, StringComparison.Ordinal);
                    if (closeParen > openParen)
                    {
                        var comma = storeTypeName.IndexOf(",", openParen + 1, StringComparison.Ordinal);
                        if (comma > openParen
                            && comma < closeParen)
                        {
                            if (int.TryParse(storeTypeName.Substring(openParen + 1, comma - openParen - 1), out var parsedPrecision))
                            {
                                precision = parsedPrecision;
                            }

                            if (int.TryParse(storeTypeName.Substring(comma + 1, closeParen - comma - 1), out var parsedScale))
                            {
                                scale = parsedScale;
                            }
                        }
                        else
                        {
                            var sizeString = storeTypeName.Substring(openParen + 1, closeParen - openParen - 1).Trim();
                            if (sizeString.Equals("max", StringComparison.OrdinalIgnoreCase))
                            {
                                isMax = true;
                            }
                            else if (int.TryParse(sizeString, out var parsedSize))
                            {
                                size = parsedSize;
                                precision = parsedSize;
                            }
                        }

                        return storeTypeName.Substring(0, openParen);
                    }
                }
            }

            return storeTypeName;
        }

        /// <summary>
        ///     The provider-specific relational type name for which mapping is needed.
        /// </summary>
        public string StoreTypeName { get; }

        /// <summary>
        ///     The provider-specific relational type name, with any size/precision/scale removed.
        /// </summary>
        public string StoreTypeNameBase { get; }

        /// <summary>
        ///     <c>True</c> if the store type name ends in "(max)".
        /// </summary>
        public bool StoreTypeNameSizeIsMax => _isMax;

        /// <summary>
        ///     Indicates the store-size to use for the mapping, or null if none.
        /// </summary>
        public int? Size => _coreTypeMappingInfo.Size ?? _parsedSize;

        /// <summary>
        ///     The suggested precision of the mapped data type.
        /// </summary>
        public int? Precision => _coreTypeMappingInfo.Precision ?? _parsedPrecision;

        /// <summary>
        ///     The suggested scale of the mapped data type.
        /// </summary>
        public int? Scale => _coreTypeMappingInfo.Scale ?? _parsedScale;

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
        public override int GetHashCode()
        {
            var hashCode = _coreTypeMappingInfo.GetHashCode();
            hashCode = (hashCode * 397) ^ (StoreTypeName?.GetHashCode() ?? 0);
            hashCode = (hashCode * 397) ^ (IsFixedLength?.GetHashCode() ?? 0);
            return hashCode;
        }
    }
}

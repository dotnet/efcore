// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.Converters;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     Describes metadata needed to decide on a relational type mapping for
    ///     a property, type, or provider-specific relational type name.
    /// </summary>
    public abstract class RelationalTypeMappingInfo : TypeMappingInfo
    {
        private readonly int? _parsedSize;
        private readonly int? _parsedPrecision;
        private readonly int? _parsedScale;
        private readonly bool _isMax;

        /// <summary>
        ///     Creates a new instance of <see cref="RelationalTypeMappingInfo" />.
        /// </summary>
        /// <param name="property"> The property for which mapping is needed. </param>
        protected RelationalTypeMappingInfo([NotNull] IProperty property)
            : base(property)
        {
            var storeTypeName = property
                .FindPrincipals()
                .Select(p => (string)p[RelationalAnnotationNames.ColumnType])
                .FirstOrDefault(t => t != null);

            StoreTypeName = storeTypeName;
            StoreTypeNameBase = ParseStoreTypeName(storeTypeName, out _parsedSize, out _parsedPrecision, out _parsedScale, out _isMax);
        }

        /// <summary>
        ///     Creates a new instance of <see cref="RelationalTypeMappingInfo" />.
        /// </summary>
        /// <param name="type"> The CLR type in the model for which mapping is needed. </param>
        protected RelationalTypeMappingInfo([NotNull] Type type)
            : base(type)
        {
        }

        /// <summary>
        ///     Creates a new instance of <see cref="RelationalTypeMappingInfo" />.
        /// </summary>
        /// <param name="storeTypeName"> The provider-specific relational type name for which mapping is needed. </param>
        protected RelationalTypeMappingInfo([NotNull] string storeTypeName)
        {
            Check.NotEmpty(storeTypeName, nameof(storeTypeName));

            StoreTypeName = storeTypeName;
            StoreTypeNameBase = ParseStoreTypeName(storeTypeName, out _parsedSize, out _parsedPrecision, out _parsedScale, out _isMax);
        }

        /// <summary>
        ///     Creates a new instance of <see cref="RelationalTypeMappingInfo" />.
        /// </summary>
        /// <param name="member"> The property or field for which mapping is needed. </param>
        protected RelationalTypeMappingInfo([NotNull] MemberInfo member)
            : base(member)
        {
            Check.NotNull(member, nameof(member));

            var attribute = member.GetCustomAttributes<ColumnAttribute>(true)?.FirstOrDefault();
            if (attribute != null)
            {
                StoreTypeName = attribute.TypeName;
                StoreTypeNameBase = ParseStoreTypeName(attribute.TypeName, out _parsedSize, out _parsedPrecision, out _parsedScale, out _isMax);
            }
        }

        /// <summary>
        ///     Creates a new instance of <see cref="RelationalTypeMappingInfo" /> with the given <see cref="ValueConverterInfo" />.
        /// </summary>
        /// <param name="source"> The source info. </param>
        /// <param name="builtInConverter"> The converter to apply. </param>
        protected RelationalTypeMappingInfo(
            [NotNull] RelationalTypeMappingInfo source,
            ValueConverterInfo builtInConverter)
            : base(source, builtInConverter)
        {
            StoreTypeName = source.StoreTypeName;
            StoreTypeNameBase = ParseStoreTypeName(source.StoreTypeName, out _parsedSize, out _parsedPrecision, out _parsedScale, out _isMax);
        }

        /// <summary>
        ///     Creates a new instance of <see cref="TypeMappingInfo" />.
        /// </summary>
        /// <param name="type"> The CLR type in the model for which mapping is needed. </param>
        /// <param name="keyOrIndex"> If <c>true</c>, then a special mapping for a key or index may be returned. </param>
        /// <param name="unicode"> Specifies Unicode or Ansi mapping, or <c>null</c> for default. </param>
        /// <param name="size"> Specifies a size for the mapping, or <c>null</c> for default. </param>
        /// <param name="rowVersion"> Specifies a row-version, or <c>null</c> for default. </param>
        /// <param name="fixedLength"> Specifies a fixed length mapping, or <c>null</c> for default. </param>
        /// <param name="precision"> Specifies a precision for the mapping, or <c>null</c> for default. </param>
        /// <param name="scale"> Specifies a scale for the mapping, or <c>null</c> for default. </param>
        protected RelationalTypeMappingInfo(
            [NotNull] Type type,
            bool keyOrIndex,
            bool? unicode = null,
            int? size = null,
            bool? rowVersion = null,
            bool? fixedLength = null,
            int? precision = null,
            int? scale = null)
            : base(type, keyOrIndex, unicode, size, rowVersion, fixedLength, precision, scale)
        {
        }

        private string ParseStoreTypeName(
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
        public virtual string StoreTypeName { get; }

        /// <summary>
        ///     The provider-specific relational type name, with any size/precision/scale removed.
        /// </summary>
        public virtual string StoreTypeNameBase { get; }

        /// <summary>
        ///     <c>True</c> if the store type name ends in "(max)".
        /// </summary>
        public virtual bool StoreTypeNameSizeIsMax => _isMax;

        /// <summary>
        ///     Indicates the store-size to use for the mapping, or null if none.
        /// </summary>
        public override int? Size => base.Size ?? _parsedSize;

        /// <summary>
        ///     The suggested precision of the mapped data type.
        /// </summary>
        public override int? Precision => base.Precision ?? _parsedPrecision;

        /// <summary>
        ///     The suggested scale of the mapped data type.
        /// </summary>
        public override int? Scale => base.Scale ?? _parsedScale;

        /// <summary>
        ///     Compares this <see cref="RelationalTypeMappingInfo" /> to another to check if they represent the same mapping.
        /// </summary>
        /// <param name="other"> The other object. </param>
        /// <returns> <c>True</c> if they represent the same mapping; <c>false</c> otherwise. </returns>
        protected virtual bool Equals([NotNull] RelationalTypeMappingInfo other)
            => Equals((TypeMappingInfo)other)
               && StoreTypeName == other.StoreTypeName;

        /// <summary>
        ///     Compares this <see cref="RelationalTypeMappingInfo" /> to another to check if they represent the same mapping.
        /// </summary>
        /// <param name="obj"> The other object. </param>
        /// <returns> <c>True</c> if they represent the same mapping; <c>false</c> otherwise. </returns>
        public override bool Equals(object obj)
            => obj != null
               && (ReferenceEquals(this, obj)
                   || obj.GetType() == GetType()
                   && Equals((RelationalTypeMappingInfo)obj));

        /// <summary>
        ///     Returns a hash code for this object.
        /// </summary>
        /// <returns> The hash code. </returns>
        public override int GetHashCode()
            => (base.GetHashCode() * 397) ^ (StoreTypeName?.GetHashCode() ?? 0);
    }
}

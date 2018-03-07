// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
    ///     Describes metadata needed to decide on a type mapping for a property or type.
    /// </summary>
    public abstract class TypeMappingInfo
    {
        /// <summary>
        ///     Creates a new instance of <see cref="TypeMappingInfo" />.
        /// </summary>
        protected TypeMappingInfo()
        {
        }

        /// <summary>
        ///     Creates a new instance of <see cref="TypeMappingInfo" />.
        /// </summary>
        /// <param name="property"> The property for which mapping is needed. </param>
        protected TypeMappingInfo([NotNull] IProperty property)
        {
            Check.NotNull(property, nameof(property));

            var principals = property.FindPrincipals().ToList();

            Property = property;
            MemberInfo = property.GetIdentifyingMemberInfo();
            IsKeyOrIndex = property.IsKeyOrForeignKey() || property.IsIndex();
            Size = principals.Select(p => p.GetMaxLength()).FirstOrDefault(t => t != null);
            IsUnicode = principals.Select(p => p.IsUnicode()).FirstOrDefault(t => t != null);
            IsRowVersion = property.IsConcurrencyToken && property.ValueGenerated == ValueGenerated.OnAddOrUpdate;
            ClrType = property.ClrType.UnwrapNullableType();
        }

        /// <summary>
        ///     Creates a new instance of <see cref="TypeMappingInfo" />.
        /// </summary>
        /// <param name="type"> The CLR type in the model for which mapping is needed. </param>
        protected TypeMappingInfo([NotNull] Type type)
        {
            Check.NotNull(type, nameof(type));

            ClrType = type.UnwrapNullableType();
        }

        /// <summary>
        ///     Creates a new instance of <see cref="TypeMappingInfo" />.
        /// </summary>
        /// <param name="member"> The property or field for which mapping is needed. </param>
        protected TypeMappingInfo([NotNull] MemberInfo member)
        {
            Check.NotNull(member, nameof(member));

            ClrType = member.GetMemberType().UnwrapNullableType();
            MemberInfo = member;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="TypeMappingInfo" />.
        /// </summary>
        /// <param name="type"> The CLR type in the model for which mapping is needed. </param>
        /// <param name="keyOrIndex"> If <c>true</c>, then a special mapping for a key or index may be returned. </param>
        /// <param name="unicode"> Specifies Unicode or ANSI mapping, or <c>null</c> for default. </param>
        /// <param name="size"> Specifies a size for the mapping, or <c>null</c> for default. </param>
        /// <param name="rowVersion"> Specifies a row-version, or <c>null</c> for default. </param>
        /// <param name="precision"> Specifies a precision for the mapping, or <c>null</c> for default. </param>
        /// <param name="scale"> Specifies a scale for the mapping, or <c>null</c> for default. </param>
        protected TypeMappingInfo(
            [NotNull] Type type,
            bool keyOrIndex,
            bool? unicode = null,
            int? size = null,
            bool? rowVersion = null,
            int? precision = null,
            int? scale = null)
            : this(type)
        {
            IsKeyOrIndex = keyOrIndex;
            Size = size;
            IsUnicode = unicode;
            IsRowVersion = rowVersion;
            Precision = precision;
            Scale = scale;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="TypeMappingInfo" /> with the given <see cref="ValueConverterInfo" />.
        /// </summary>
        /// <param name="source"> The source info. </param>
        /// <param name="converter"> The converter to apply. </param>
        protected TypeMappingInfo(
            [NotNull] TypeMappingInfo source,
            ValueConverterInfo converter)
        {
            Check.NotNull(source, nameof(source));

            Property = source.Property;
            IsRowVersion = source.IsRowVersion;
            IsKeyOrIndex = source.IsKeyOrIndex;
            MemberInfo = source.MemberInfo;

            var mappingHints = converter.MappingHints;

            Size = source.Size ?? mappingHints?.Size;
            IsUnicode = source.IsUnicode ?? mappingHints?.IsUnicode;
            Scale = source.Scale ?? mappingHints?.Scale;
            Precision = source.Precision ?? mappingHints?.Precision;

            ClrType = converter.ProviderClrType.UnwrapNullableType();
        }

        /// <summary>
        ///     Returns a new <see cref="TypeMappingInfo" /> with the given converter applied.
        /// </summary>
        /// <param name="converterInfo"> The converter to apply. </param>
        /// <returns> The new mapping info. </returns>
        public abstract TypeMappingInfo WithConverter(ValueConverterInfo converterInfo);

        /// <summary>
        ///     The property for which mapping is needed.
        /// </summary>
        public virtual IProperty Property { get; }

        /// <summary>
        ///     Indicates whether or not the mapping is part of a key or index.
        /// </summary>
        public virtual bool IsKeyOrIndex { get; }

        /// <summary>
        ///     Indicates the store-size to use for the mapping, or null if none.
        /// </summary>
        public virtual int? Size { get; }

        /// <summary>
        ///     Indicates whether or not the mapping supports Unicode, or null if not defined.
        /// </summary>
        public virtual bool? IsUnicode { get; }

        /// <summary>
        ///     Indicates whether or not the mapping will be used for a row version, or null if not defined.
        /// </summary>
        public virtual bool? IsRowVersion { get; }

        /// <summary>
        ///     The suggested precision of the mapped data type.
        /// </summary>
        public virtual int? Precision { get; }

        /// <summary>
        ///     The suggested scale of the mapped data type.
        /// </summary>
        public virtual int? Scale { get; }

        /// <summary>
        ///     The field or property info for the property.
        /// </summary>
        public virtual MemberInfo MemberInfo { get; }

        /// <summary>
        ///     The CLR type in the model.
        /// </summary>
        public virtual Type ClrType { get; }

        /// <summary>
        ///     Compares this <see cref="TypeMappingInfo" /> to another to check if they represent the same mapping.
        /// </summary>
        /// <param name="other"> The other object. </param>
        /// <returns> <c>True</c> if they represent the same mapping; <c>false</c> otherwise. </returns>
        protected virtual bool Equals([NotNull] TypeMappingInfo other)
            => ClrType == other.ClrType
               && MemberInfo == other.MemberInfo
               && IsKeyOrIndex == other.IsKeyOrIndex
               && Size == other.Size
               && IsUnicode == other.IsUnicode
               && IsRowVersion == other.IsRowVersion
               && Precision == other.Precision
               && Scale == other.Scale;

        /// <summary>
        ///     Compares this <see cref="TypeMappingInfo" /> to another to check if they represent the same mapping.
        /// </summary>
        /// <param name="obj"> The other object. </param>
        /// <returns> <c>True</c> if they represent the same mapping; <c>false</c> otherwise. </returns>
        public override bool Equals(object obj)
            => obj != null
               && (ReferenceEquals(this, obj)
                   || obj.GetType() == GetType()
                   && Equals((TypeMappingInfo)obj));

        /// <summary>
        ///     Returns a hash code for this object.
        /// </summary>
        /// <returns> The hash code. </returns>
        public override int GetHashCode()
        {
            var hashCode = ClrType?.GetHashCode() ?? 0;
            hashCode = (hashCode * 397) ^ IsKeyOrIndex.GetHashCode();
            hashCode = (hashCode * 397) ^ (Size?.GetHashCode() ?? 0);
            hashCode = (hashCode * 397) ^ (MemberInfo?.GetHashCode() ?? 0);
            hashCode = (hashCode * 397) ^ (IsUnicode?.GetHashCode() ?? 0);
            hashCode = (hashCode * 397) ^ (IsRowVersion?.GetHashCode() ?? 0);
            hashCode = (hashCode * 397) ^ (Scale?.GetHashCode() ?? 0);
            hashCode = (hashCode * 397) ^ (Precision?.GetHashCode() ?? 0);
            return hashCode;
        }
    }
}

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
        private readonly ValueConverter _customConverter;

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

            Property = property;

            MemberInfo = property.GetIdentifyingMemberInfo();

            var principals = property.FindPrincipals().ToList();

            ConfiguredProviderClrType = principals
                .Select(p => p.GetProviderClrType())
                .FirstOrDefault(t => t != null)
                ?.UnwrapNullableType();

            _customConverter = principals
                .Select(p => p.GetValueConverter())
                .FirstOrDefault(c => c != null);

            var mappingHints = _customConverter?.MappingHints;

            if (_customConverter != null)
            {
                ValueConverterInfo = new ValueConverterInfo(
                    _customConverter.ModelClrType,
                    _customConverter.ProviderClrType,
                    i => _customConverter,
                    mappingHints);
            }

            IsKeyOrIndex = property.IsKeyOrForeignKey() || property.IsIndex();

            Size = principals.Select(p => p.GetMaxLength()).FirstOrDefault(t => t != null) ?? mappingHints?.Size;

            IsUnicode = principals.Select(p => p.IsUnicode()).FirstOrDefault(t => t != null)
                        ?? mappingHints?.IsUnicode;

            IsRowVersion = property.IsConcurrencyToken && property.ValueGenerated == ValueGenerated.OnAddOrUpdate;

            Precision = mappingHints?.Precision;

            Scale = mappingHints?.Scale;

            ModelClrType = property.ClrType.UnwrapNullableType();

            ProviderClrType = CreateProviderClrType();
        }

        private Type CreateProviderClrType()
            => ValueConverterInfo?.ProviderClrType.UnwrapNullableType() ?? ConfiguredProviderClrType ?? ModelClrType;

        /// <summary>
        ///     Creates a new instance of <see cref="TypeMappingInfo" />.
        /// </summary>
        /// <param name="type"> The CLR type in the model for which mapping is needed. </param>
        protected TypeMappingInfo([NotNull] Type type)
        {
            Check.NotNull(type, nameof(type));

            ModelClrType = type.UnwrapNullableType();
            ProviderClrType = CreateProviderClrType();
        }

        /// <summary>
        ///     Creates a new instance of <see cref="TypeMappingInfo" />.
        /// </summary>
        /// <param name="member"> The property or field for which mapping is needed. </param>
        protected TypeMappingInfo([NotNull] MemberInfo member)
        {
            Check.NotNull(member, nameof(member));

            ModelClrType = member.GetMemberType().UnwrapNullableType();
            MemberInfo = member;
            ProviderClrType = CreateProviderClrType();
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
        /// <param name="builtInConverter"> The converter to apply. </param>
        protected TypeMappingInfo(
            [NotNull] TypeMappingInfo source,
            ValueConverterInfo builtInConverter)
        {
            Check.NotNull(source, nameof(source));

            Property = source.Property;
            ModelClrType = source.ModelClrType;
            ConfiguredProviderClrType = source.ConfiguredProviderClrType;
            IsRowVersion = source.IsRowVersion;
            IsKeyOrIndex = source.IsKeyOrIndex;

            if (source._customConverter != null)
            {
                _customConverter = source._customConverter;

                ValueConverterInfo = new ValueConverterInfo(
                    _customConverter.ModelClrType,
                    builtInConverter.ProviderClrType,
                    i => _customConverter.ComposeWith(builtInConverter.Create()),
                    builtInConverter.MappingHints == null
                        ? _customConverter.MappingHints
                        : builtInConverter.MappingHints.With(_customConverter.MappingHints));
            }
            else
            {
                ValueConverterInfo = builtInConverter;
            }

            // ReSharper disable once VirtualMemberCallInConstructor
            var mappingHints = ValueConverterInfo?.MappingHints;

            Size = source.Size ?? mappingHints?.Size;
            IsUnicode = source.IsUnicode ?? mappingHints?.IsUnicode;
            Scale = source.Scale ?? mappingHints?.Scale;
            Precision = source.Precision ?? mappingHints?.Precision;
            ProviderClrType = CreateProviderClrType();
        }

        /// <summary>
        ///     Returns a new <see cref="TypeMappingInfo" /> with the given converter applied.
        /// </summary>
        /// <param name="converterInfo"> The converter to apply. </param>
        /// <returns> The new mapping info. </returns>
        public abstract TypeMappingInfo WithBuiltInConverter(ValueConverterInfo converterInfo);

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
        ///     The CLR type set to use when reading/writing to/from the store.
        /// </summary>
        public virtual Type ConfiguredProviderClrType { get; }

        /// <summary>
        ///     The field or property info for the property.
        /// </summary>
        public virtual MemberInfo MemberInfo { get; }

        /// <summary>
        ///     The <see cref="ValueConverter" /> to use when reading/writing to/from the database provider.
        /// </summary>
        public virtual ValueConverterInfo? ValueConverterInfo { get; }

        /// <summary>
        ///     The CLR type in the model.
        /// </summary>
        public virtual Type ModelClrType { get; }

        /// <summary>
        ///     The CLR type targeted by the type mapping when reading/writing to/from the databaseProvider.
        /// </summary>
        public virtual Type ProviderClrType { get; }

        /// <summary>
        ///     Compares this <see cref="TypeMappingInfo" /> to another to check if they represent the same mapping.
        /// </summary>
        /// <param name="other"> The other object. </param>
        /// <returns> <c>True</c> if they represent the same mapping; <c>false</c> otherwise. </returns>
        protected virtual bool Equals([NotNull] TypeMappingInfo other)
            => ModelClrType == other.ModelClrType
               && MemberInfo == other.MemberInfo
               && ConfiguredProviderClrType == other.ConfiguredProviderClrType
               && IsKeyOrIndex == other.IsKeyOrIndex
               && Size == other.Size
               && IsUnicode == other.IsUnicode
               && IsRowVersion == other.IsRowVersion
               && Precision == other.Precision
               && Scale == other.Scale
               && Equals(_customConverter, other._customConverter);

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
            var hashCode = (ConfiguredProviderClrType != null ? ConfiguredProviderClrType.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ IsKeyOrIndex.GetHashCode();
            hashCode = (hashCode * 397) ^ (Size?.GetHashCode() ?? 0);
            hashCode = (hashCode * 397) ^ (MemberInfo?.GetHashCode() ?? 0);
            hashCode = (hashCode * 397) ^ (IsUnicode?.GetHashCode() ?? 0);
            hashCode = (hashCode * 397) ^ (IsRowVersion?.GetHashCode() ?? 0);
            hashCode = (hashCode * 397) ^ (Scale?.GetHashCode() ?? 0);
            hashCode = (hashCode * 397) ^ (Precision?.GetHashCode() ?? 0);
            hashCode = (hashCode * 397) ^ (_customConverter?.GetHashCode() ?? 0);
            hashCode = (hashCode * 397) ^ (ModelClrType?.GetHashCode() ?? 0);
            return hashCode;
        }
    }
}

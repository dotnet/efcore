// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Maps .NET types to their corresponding relational database types.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public abstract class RelationalTypeMapper : CoreTypeMapper, IRelationalTypeMapper
    {
        private static readonly IReadOnlyDictionary<string, Func<Type, RelationalTypeMapping>> _emptyNamedMappings
            = new Dictionary<string, Func<Type, RelationalTypeMapping>>();

        private static readonly MethodInfo _getFieldValueMethod
            = typeof(DbDataReader).GetTypeInfo().GetDeclaredMethod(nameof(DbDataReader.GetFieldValue));

        private static readonly IDictionary<Type, MethodInfo> _getXMethods
            = new Dictionary<Type, MethodInfo>
            {
                { typeof(bool), typeof(DbDataReader).GetTypeInfo().GetDeclaredMethod(nameof(DbDataReader.GetBoolean)) },
                { typeof(byte), typeof(DbDataReader).GetTypeInfo().GetDeclaredMethod(nameof(DbDataReader.GetByte)) },
                { typeof(char), typeof(DbDataReader).GetTypeInfo().GetDeclaredMethod(nameof(DbDataReader.GetChar)) },
                { typeof(DateTime), typeof(DbDataReader).GetTypeInfo().GetDeclaredMethod(nameof(DbDataReader.GetDateTime)) },
                { typeof(decimal), typeof(DbDataReader).GetTypeInfo().GetDeclaredMethod(nameof(DbDataReader.GetDecimal)) },
                { typeof(double), typeof(DbDataReader).GetTypeInfo().GetDeclaredMethod(nameof(DbDataReader.GetDouble)) },
                { typeof(float), typeof(DbDataReader).GetTypeInfo().GetDeclaredMethod(nameof(DbDataReader.GetFloat)) },
                { typeof(Guid), typeof(DbDataReader).GetTypeInfo().GetDeclaredMethod(nameof(DbDataReader.GetGuid)) },
                { typeof(short), typeof(DbDataReader).GetTypeInfo().GetDeclaredMethod(nameof(DbDataReader.GetInt16)) },
                { typeof(int), typeof(DbDataReader).GetTypeInfo().GetDeclaredMethod(nameof(DbDataReader.GetInt32)) },
                { typeof(long), typeof(DbDataReader).GetTypeInfo().GetDeclaredMethod(nameof(DbDataReader.GetInt64)) },
                { typeof(string), typeof(DbDataReader).GetTypeInfo().GetDeclaredMethod(nameof(DbDataReader.GetString)) }
            };

        private readonly ConcurrentDictionary<(string StoreType, Type ClrType), RelationalTypeMapping> _explicitMappings
            = new ConcurrentDictionary<(string StoreType, Type ClrType), RelationalTypeMapping>();

        /// <summary>
        ///     Initializes a new instance of the this class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        [Obsolete("Use RelationalTypeMapper(CoreTypeMapperDependencies RelationalTypeMapperDependencies) instead.")]
        protected RelationalTypeMapper([NotNull] RelationalTypeMapperDependencies dependencies)
            : this(new CoreTypeMapperDependencies(), dependencies)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the this class.
        /// </summary>
        /// <param name="coreDependencies"> Parameter object containing dependencies for this service. </param>
        /// <param name="relationalDependencies"> Parameter object containing relational-specific dependencies for this service. </param>
        protected RelationalTypeMapper(
            [NotNull] CoreTypeMapperDependencies coreDependencies,
            [NotNull] RelationalTypeMapperDependencies relationalDependencies)
            : base(coreDependencies)
        {
            Check.NotNull(relationalDependencies, nameof(relationalDependencies));
        }

        /// <summary>
        ///     Gets the mappings from .NET types to database types.
        /// </summary>
        /// <returns> The type mappings. </returns>
        protected abstract IReadOnlyDictionary<Type, RelationalTypeMapping> GetClrTypeMappings();

        /// <summary>
        ///     Gets the mappings from .NET type names to database types.
        /// </summary>
        /// <returns> The type mappings. </returns>
        protected virtual IReadOnlyDictionary<string, Func<Type, RelationalTypeMapping>> GetClrTypeNameMappings()
            => _emptyNamedMappings;

        /// <summary>
        ///     Gets the mappings from database types to .NET types.
        /// </summary>
        /// <returns> The type mappings. </returns>
        [Obsolete("Override GetMultipleStoreTypeMappings instead.")]
        protected virtual IReadOnlyDictionary<string, RelationalTypeMapping> GetStoreTypeMappings()
            => throw new NotImplementedException("This method was abstract and is now obsolete. Override GetMultipleStoreTypeMappings instead.");

        /// <summary>
        ///     Gets the mappings from database types to .NET types.
        /// </summary>
        /// <returns> The type mappings. </returns>
        protected virtual IReadOnlyDictionary<string, IList<RelationalTypeMapping>> GetMultipleStoreTypeMappings()
           => null;

        /// <summary>
        ///     Gets column type for the given property.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The name of the database type. </returns>
        protected virtual string GetColumnType([NotNull] IProperty property) 
            => (string)Check.NotNull(property, nameof(property))[RelationalAnnotationNames.ColumnType];

        /// <summary>
        ///     Ensures that the given type name is a valid type for the relational database.
        ///     An exception is thrown if it is not a valid type.
        /// </summary>
        /// <param name="storeType">The type to be validated.</param>
        public virtual void ValidateTypeName(string storeType)
        {
        }

        /// <summary>
        ///     Gets a value indicating whether the given .NET type is mapped.
        /// </summary>
        /// <param name="clrType"> The .NET type. </param>
        /// <returns> True if the type can be mapped; otherwise false. </returns>
        public override bool IsTypeMapped(Type clrType)
        {
            Check.NotNull(clrType, nameof(clrType));

            return FindMapping(clrType) != null;
        }

        /// <summary>
        ///     Gets mapping for the given <see cref="RelationalTypeMappingInfo"/>.
        ///     Returns null if no mapping is found.
        /// </summary>
        /// <param name="typeMappingInfo"> The input data to the mapping process. </param>
        /// <returns> The type mapping to be used. </returns>
        protected virtual RelationalTypeMapping FindMapping([NotNull] RelationalTypeMappingInfo typeMappingInfo)
        {
            Check.NotNull(typeMappingInfo, nameof(typeMappingInfo));

            return (typeMappingInfo.StoreTypeName != null
                       ? _explicitMappings.GetOrAdd(
                           (typeMappingInfo.StoreTypeName, typeMappingInfo.ModelClrType?.UnwrapNullableType() ?? typeof(DBNull)),
                           k => CreateMappingFromStoreType(typeMappingInfo))
                       : null)
                   ?? FindCustomMapping(typeMappingInfo)
                   ?? (typeMappingInfo.Property == null ? null : FindCustomMapping(typeMappingInfo.Property))
                   ?? (typeMappingInfo.ModelClrType == null ? null : FindMapping(typeMappingInfo.ModelClrType));
        }

        /// <summary>
        ///     Gets the relational database type for the given property.
        ///     Returns null if no mapping is found.
        /// </summary>
        /// <param name="property">The property to get the mapping for.</param>
        /// <returns>
        ///     The type mapping to be used.
        /// </returns>
        public virtual RelationalTypeMapping FindMapping(IProperty property)
        {
            Check.NotNull(property, nameof(property));

            return FindMapping(new RelationalTypeMappingInfo(property: property));
        }

        /// <summary>
        ///     Gets the relational database type for a given .NET type.
        ///     Returns null if no mapping is found.
        /// </summary>
        /// <param name="clrType">The type to get the mapping for.</param>
        /// <returns>
        ///     The type mapping to be used.
        /// </returns>
        public virtual RelationalTypeMapping FindMapping(Type clrType)
        {
            Check.NotNull(clrType, nameof(clrType));

            return _explicitMappings.GetOrAdd(
                ("", clrType.UnwrapNullableType()),
                k =>
                    {
                        var underlyingType = k.ClrType.UnwrapEnumType();

                        if (!GetClrTypeMappings().TryGetValue(underlyingType, out var mapping)
                            && GetClrTypeNameMappings().TryGetValue(underlyingType.FullName, out var mappingFunc))
                        {
                            mapping = mappingFunc(underlyingType);
                        }

                        return mapping != null
                               && k.ClrType.IsEnum
                            ? (RelationalTypeMapping)mapping.Clone(CreateEnumToNumberConverter(k.ClrType))
                            : mapping;
                    });
        }

        private static ValueConverter CreateEnumToNumberConverter(Type enumType)
            => (ValueConverter)Activator.CreateInstance(
                typeof(EnumToNumberConveter<,>).MakeGenericType(enumType, enumType.UnwrapEnumType()));

        /// <summary>
        ///     <para>
        ///         Gets the mapping that represents the given database type.
        ///         Returns null if no mapping is found.
        ///     </para>
        ///     <para>
        ///         Note that sometimes the same store type can have different mappings; this method returns the default.
        ///     </para>
        /// </summary>
        /// <param name="storeType">The type to get the mapping for.</param>
        /// <returns>
        ///     The type mapping to be used.
        /// </returns>
        public virtual RelationalTypeMapping FindMapping(string storeType)
        {
            Check.NotEmpty(storeType, nameof(storeType));

            return FindMapping(new RelationalTypeMappingInfo(storeTypeName: storeType));
        }

        /// <summary>
        ///     Creates the mapping for the given database type.
        /// </summary>
        /// <param name="storeType">The type to create the mapping for.</param>
        /// <returns> The type mapping to be used. </returns>
        protected virtual RelationalTypeMapping CreateMappingFromStoreType([NotNull] string storeType)
        {
            Check.NotEmpty(storeType, nameof(storeType));

            return CreateMappingFromStoreType(new RelationalTypeMappingInfo(storeTypeName: storeType));
        }

        /// <summary>
        ///     Creates the mapping for the given database type.
        /// </summary>
        /// <param name="typeMappingInfo"> The input data to the mapping process. </param>
        /// <returns> The type mapping to be used. </returns>
        protected virtual RelationalTypeMapping CreateMappingFromStoreType([NotNull] RelationalTypeMappingInfo typeMappingInfo)
        {
            Check.NotNull(typeMappingInfo, nameof(typeMappingInfo));

            var storeType = typeMappingInfo.StoreTypeName;
            if (storeType == null)
            {
                return null;
            }
            
            var clrType = typeMappingInfo.ModelClrType?.UnwrapNullableType();

            if (TryFindExactMapping(storeType, clrType, out var mapping))
            {
                return mapping;
            }

            var isEnum = clrType != null && clrType.IsEnum;

            if (mapping == null
                && isEnum)
            {
                if (TryFindExactMapping(storeType, clrType.UnwrapEnumType(), out mapping))
                {
                    return (RelationalTypeMapping)mapping.Clone(CreateEnumToNumberConverter(clrType));
                }

                if (mapping == null
                    && TryFindExactMapping(storeType, typeof(string), out mapping))
                {
                    return (RelationalTypeMapping)mapping.Clone(CreateEnumToStringConverter(clrType));
                }
            }

            var size = mapping?.Size;

            var openParen = storeType.IndexOf("(", StringComparison.Ordinal);
            if (openParen > 0)
            {
                var fragment = storeType.Substring(0, openParen);
                if (!TryFindStoreMapping(fragment, clrType, out mapping) 
                    && isEnum)
                {
                    if (TryFindStoreMapping(fragment, clrType.UnwrapEnumType(), out mapping))
                    {
                        mapping = (RelationalTypeMapping)mapping.Clone(CreateEnumToNumberConverter(clrType));
                    }
                    else if (TryFindStoreMapping(fragment, typeof(string), out mapping))
                    {
                        mapping = (RelationalTypeMapping)mapping.Clone(CreateEnumToStringConverter(clrType));
                    }
                }

                if (mapping?.ClrType == typeof(string)
                    || mapping?.ClrType == typeof(byte[]))
                {
                    var closeParen = storeType.IndexOf(")", openParen + 1, StringComparison.Ordinal);

                    if (closeParen > openParen
                        && int.TryParse(storeType.Substring(openParen + 1, closeParen - openParen - 1), out var newSize)
                        && mapping.Size != newSize)
                    {
                        size = newSize;
                    }
                }
            }

            return mapping?.Clone(storeType, size ?? mapping.Size);
        }

        private static ValueConverter CreateEnumToStringConverter(Type enumType)
            => (ValueConverter)Activator.CreateInstance(
                typeof(EnumToStringConveter<>).MakeGenericType(enumType));

        private bool TryFindExactMapping(string storeType, Type clrType, out RelationalTypeMapping mapping)
            => TryFindStoreMapping(storeType, clrType, out mapping)
               && mapping.StoreType.Equals(storeType, StringComparison.OrdinalIgnoreCase);

        private bool TryFindStoreMapping(
            string storeTypeFragment, 
            Type clrType, 
            out RelationalTypeMapping mapping)
        {
            var mappings = GetMultipleStoreTypeMappings();
            if (mappings == null)
            {
                // Only look in obsolete collection if new collection returned null
#pragma warning disable 618
                if (GetStoreTypeMappings().TryGetValue(storeTypeFragment, out mapping))
#pragma warning restore 618
                {
                    return clrType == null || mapping.ClrType == clrType;
                }
            }
            else if (mappings.TryGetValue(storeTypeFragment, out var mappingList))
            {
                mapping = mappingList.FirstOrDefault(m => clrType == null || m.ClrType == clrType);
                if (mapping != null)
                {
                    return true;
                }
            }

            mapping = null;
            return false;
        }

        /// <summary>
        ///     Gets the relational database type for the given property, using a separate type mapper if needed.
        ///     This base implementation uses custom mappers for string and byte array properties.
        ///     Returns null if no mapping is found.
        /// </summary>
        /// <param name="property">The property to get the mapping for.</param>
        /// <returns>
        ///     The type mapping to be used.
        /// </returns>
        protected virtual RelationalTypeMapping FindCustomMapping([NotNull] IProperty property)
        {
            Check.NotNull(property, nameof(property));

            return FindCustomMapping(new RelationalTypeMappingInfo(property: property));
        }

        /// <summary>
        ///     Gets the relational database type for the given property, using a separate type mapper if needed.
        ///     This base implementation uses custom mappers for string and byte array properties.
        ///     Returns null if no mapping is found.
        /// </summary>
        /// <param name="typeMappingInfo"> The input data to the mapping process. </param>
        /// <returns>
        ///     The type mapping to be used.
        /// </returns>
        protected virtual RelationalTypeMapping FindCustomMapping([NotNull] RelationalTypeMappingInfo typeMappingInfo)
        {
            Check.NotNull(typeMappingInfo, nameof(typeMappingInfo));

            var clrType = typeMappingInfo.ModelClrType?.UnwrapNullableType();

            return clrType == typeof(string)
                ? GetStringMapping(typeMappingInfo)
                : clrType == typeof(byte[])
                    ? GetByteArrayMapping(typeMappingInfo)
                    : null;
        }

        /// <summary>
        ///     Gets the mapper to be used for byte array properties.
        /// </summary>
        public virtual IByteArrayRelationalTypeMapper ByteArrayMapper => null;

        /// <summary>
        ///     Gets the mapper to be used for string properties.
        /// </summary>
        public virtual IStringRelationalTypeMapper StringMapper => null;

        /// <summary>
        ///     Gets the relational database type for the given string property.
        /// </summary>
        /// <param name="property"> The property to get the mapping for. </param>
        /// <returns> The type mapping to be used. </returns>
        protected virtual RelationalTypeMapping GetStringMapping([NotNull] IProperty property)
        {
            Check.NotNull(property, nameof(property));

            return GetStringMapping(new RelationalTypeMappingInfo(property: property));
        }

        /// <summary>
        ///     Gets the relational database type for the given byte array property.
        /// </summary>
        /// <param name="property"> The property to get the mapping for. </param>
        /// <returns> The type mapping to be used. </returns>
        protected virtual RelationalTypeMapping GetByteArrayMapping([NotNull] IProperty property)
        {
            Check.NotNull(property, nameof(property));

            return GetByteArrayMapping(new RelationalTypeMappingInfo(property: property));
        }

        /// <summary>
        ///     Gets the relational database type for the given string property.
        /// </summary>
        /// <param name="typeMappingInfo"> The input data to the mapping process. </param>
        /// <returns> The type mapping to be used. </returns>
        protected virtual RelationalTypeMapping GetStringMapping([NotNull] RelationalTypeMappingInfo typeMappingInfo)
        {
            Check.NotNull(typeMappingInfo, nameof(typeMappingInfo));

            var property = typeMappingInfo.Property;
            if (property == null)
            {
                return null;
            }

            var principal = property.FindPrincipal();

            return StringMapper?.FindMapping(
                property.IsUnicode() ?? principal?.IsUnicode() ?? true,
                RequiresKeyMapping(property),
                property.GetMaxLength() ?? principal?.GetMaxLength());
        }

        /// <summary>
        ///     Gets the relational database type for the given byte array property.
        /// </summary>
        /// <param name="typeMappingInfo"> The input data to the mapping process. </param>
        /// <returns> The type mapping to be used. </returns>
        protected virtual RelationalTypeMapping GetByteArrayMapping([NotNull] RelationalTypeMappingInfo typeMappingInfo)
        {
            Check.NotNull(typeMappingInfo, nameof(typeMappingInfo));

            var property = typeMappingInfo.Property;
            if (property == null)
            {
                return null;
            }

            return ByteArrayMapper?.FindMapping(
                property.IsConcurrencyToken && property.ValueGenerated == ValueGenerated.OnAddOrUpdate,
                RequiresKeyMapping(property),
                property.GetMaxLength() ?? property.FindPrincipal()?.GetMaxLength());
        }

        /// <summary>
        ///     Gets a value indicating whether the given property should use a database type that is suitable for key properties.
        /// </summary>
        /// <param name="property"> The property to get the mapping for. </param>
        /// <returns> True if the property is a key, otherwise false. </returns>
        protected virtual bool RequiresKeyMapping([NotNull] IProperty property)
            => property.IsKey() || property.IsForeignKey();

        /// <summary>
        ///     The method to use when reading values of the given type. The method must be defined
        ///     on <see cref="DbDataReader" /> or one of its subclasses.
        /// </summary>
        /// <param name="type"> The type of the value to be read. </param>
        /// <returns> The method to use to read the value. </returns>
        public virtual MethodInfo GetDataReaderMethod(Type type)
        {
            Check.NotNull(type, nameof(type));

            return _getXMethods.TryGetValue(type, out var method)
                ? method
                : _getFieldValueMethod.MakeGenericMethod(type);
        }

        /// <summary>
        ///     Describes metadata needed to decide on a relational type mapping for
        ///     a property, type, or provider-specific relational type name.
        /// </summary>
        protected class RelationalTypeMappingInfo : CoreTypeMapper.TypeMappingInfo
        {
            /// <summary>
            ///     Creates a new instance of <see cref="RelationalTypeMappingInfo" />.
            /// </summary>
            /// <param name="property"> The property for which mapping is needed. </param>
            /// <param name="modelClrType"> The CLR type in the model for which mapping is needed. </param>
            /// <param name="storeTypeName"> The provider-specific relational type name for which mapping is needed. </param>
            public RelationalTypeMappingInfo(
                [CanBeNull] IProperty property = null,
                [CanBeNull] Type modelClrType = null,
                [CanBeNull] string storeTypeName = null)
                : base(property, modelClrType)
            {
                storeTypeName = storeTypeName ?? GetColumnType(property);

                if (storeTypeName == null)
                {
                    var principalProperty = property?.FindPrincipal();
                    if (principalProperty != null)
                    {
                        storeTypeName = GetColumnType(principalProperty);
                    }
                }

                StoreTypeName = storeTypeName;
            }

            private static string GetColumnType(IProperty property)
                => (string)property?[RelationalAnnotationNames.ColumnType];

            /// <summary>
            ///     The provider-specific relational type name for which mapping is needed.
            /// </summary>
            public virtual string StoreTypeName { get; }
        }
    }
}

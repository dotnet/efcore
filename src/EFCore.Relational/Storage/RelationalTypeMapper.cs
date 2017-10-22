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
using Microsoft.EntityFrameworkCore.Storage.Converters;
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

        private readonly ConcurrentDictionary<RelationalTypeMappingInfo, RelationalTypeMapping> _explicitMappings
            = new ConcurrentDictionary<RelationalTypeMappingInfo, RelationalTypeMapping>();

        /// <summary>
        ///     Initializes a new instance of the this class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        [Obsolete("Use RelationalTypeMapper(CoreTypeMapperDependencies RelationalTypeMapperDependencies) instead.")]
        protected RelationalTypeMapper([NotNull] RelationalTypeMapperDependencies dependencies)
            : this(
                new CoreTypeMapperDependencies(
                    new ValueConverterSelector(
                        new ValueConverterSelectorDependencies())), dependencies)
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

            RelationalDependencies = relationalDependencies;
        }

        /// <summary>
        ///     Dependencies used to create this <see cref="RelationalTypeMapper" />
        /// </summary>
        protected virtual RelationalTypeMapperDependencies RelationalDependencies { get; }

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

            return FindMapping(new RelationalTypeMappingInfo(modelClrType: clrType)) != null;
        }

        private RelationalTypeMapping FindMapping(
            [NotNull] RelationalTypeMappingInfo typeMappingInfo,
            bool blockClrTypeFallback = false)
        {
            Check.NotNull(typeMappingInfo, nameof(typeMappingInfo));

            return _explicitMappings.GetOrAdd(
                typeMappingInfo,
                k => (typeMappingInfo.StoreTypeName == null
                         ? null
                         : FindMappingsWithConversions(typeMappingInfo, blockClrTypeFallback, CreateMappingFromStoreType))
                     ?? FindMappingsWithConversions(typeMappingInfo, blockClrTypeFallback, CreateMappingFromClrType)
                     ?? FindMappingsWithConversions(typeMappingInfo, blockClrTypeFallback, CreateMappingFromFallbacks));
        }

        private RelationalTypeMapping FindMappingsWithConversions(
            RelationalTypeMappingInfo typeMappingInfo,
            bool blockClrTypeFallback,
            Func<RelationalTypeMappingInfo, bool, RelationalTypeMapping> mappingFunc)
        {
            var typeMappingUsed = typeMappingInfo;

            var mapping = mappingFunc(typeMappingUsed, blockClrTypeFallback);

            if (mapping == null
                && typeMappingInfo.TargetClrType != null)
            {
                foreach (var converterInfo in Dependencies
                    .ValueConverterSelector
                    .ForTypes(typeMappingInfo.TargetClrType, typeMappingInfo.StoreClrType))
                {
                    typeMappingUsed = (RelationalTypeMappingInfo)typeMappingInfo.WithBuiltInConverter(converterInfo);
                    mapping = mappingFunc(typeMappingUsed, blockClrTypeFallback);

                    if (mapping != null)
                    {
                        break;
                    }
                }
            }

            if (mapping != null
                && typeMappingUsed.ValueConverterInfo != null)
            {
                mapping = (RelationalTypeMapping)mapping.Clone(typeMappingUsed.ValueConverterInfo?.Create());
            }

            return mapping;
        }

        private RelationalTypeMapping CreateMappingFromClrType(
            RelationalTypeMappingInfo typeMappingInfo,
            bool blockClrTypeFallback)
            => FindCustomMapping(typeMappingInfo)
               ?? FindClrTypeMapping(typeMappingInfo);

        private RelationalTypeMapping CreateMappingFromFallbacks(
            RelationalTypeMappingInfo typeMappingInfo,
            bool blockClrTypeFallback)
            => FindClrTypeMapping(typeMappingInfo)
               ?? (typeMappingInfo.Property == null ? null : FindCustomMapping(typeMappingInfo.Property))
               ?? (blockClrTypeFallback || typeMappingInfo.TargetClrType == null ? null : FindMapping(typeMappingInfo.TargetClrType));

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

            return FindMapping(new RelationalTypeMappingInfo(property));
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
            => FindMapping(new RelationalTypeMappingInfo(modelClrType: Check.NotNull(clrType, nameof(clrType))), blockClrTypeFallback: true);

        private RelationalTypeMapping FindClrTypeMapping(RelationalTypeMappingInfo typeMappingInfo)
        {
            if (typeMappingInfo.TargetClrType == null)
            {
                return null;
            }

            if (!GetClrTypeMappings().TryGetValue(typeMappingInfo.TargetClrType, out var mapping)
                && GetClrTypeNameMappings().TryGetValue(typeMappingInfo.TargetClrType.FullName, out var mappingFunc))
            {
                mapping = mappingFunc(typeMappingInfo.TargetClrType);
            }

            if (mapping != null
                && (typeMappingInfo.Precision != null
                    || typeMappingInfo.Scale != null))
            {
                var newStoreName = mapping.StoreType;
                var openParen = newStoreName.IndexOf("(", StringComparison.Ordinal);
                if (openParen > 0)
                {
                    newStoreName = mapping.StoreType.Substring(0, openParen);
                }

                newStoreName += typeMappingInfo.Precision != null
                                && typeMappingInfo.Scale != null
                    ? "(" + typeMappingInfo.Precision + "," + typeMappingInfo.Scale + ")"
                    : "(" + (typeMappingInfo.Precision ?? typeMappingInfo.Scale) + ")";

                mapping = mapping.Clone(newStoreName, mapping.Size);
            }

            return mapping;
        }

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

        private RelationalTypeMapping CreateMappingFromStoreType(
            [NotNull] RelationalTypeMappingInfo typeMappingInfo,
            bool blockClrTypeFallback = false)
        {
            Check.NotNull(typeMappingInfo, nameof(typeMappingInfo));

            var storeTypeName = typeMappingInfo.StoreTypeName;
            if (storeTypeName == null)
            {
                return null;
            }

            if (TryFindExactMapping(typeMappingInfo, typeMappingInfo.TargetClrType, out var mapping))
            {
                return mapping;
            }

            int? size = null;

            var openParen = storeTypeName.IndexOf("(", StringComparison.Ordinal);
            if (openParen > 0)
            {
                if (TryFindStoreMapping(storeTypeName.Substring(0, openParen), typeMappingInfo.TargetClrType, out mapping))
                {
                    var closeParen = storeTypeName.IndexOf(")", openParen + 1, StringComparison.Ordinal);
                    if (closeParen > openParen)
                    {
                        var comma = storeTypeName.IndexOf(",", openParen + 1, StringComparison.Ordinal);
                        if (comma > openParen
                            && comma < closeParen)
                        {
                            // TODO: Parse precision/scale when supported
                        }
                        else if (int.TryParse(storeTypeName.Substring(openParen + 1, closeParen - openParen - 1), out var newSize) && mapping.Size != newSize)
                        {
                            size = newSize;
                        }
                    }
                }
            }

            return mapping?.Clone(storeTypeName, size ?? mapping.Size);
        }

        private bool TryFindExactMapping(RelationalTypeMappingInfo typeMappingInfo, Type clrType, out RelationalTypeMapping mapping)
            => TryFindStoreMapping(typeMappingInfo.StoreTypeName, clrType, out mapping)
               && mapping.StoreType.Equals(typeMappingInfo.StoreTypeName, StringComparison.OrdinalIgnoreCase);

        private bool TryFindStoreMapping(string storeTypeFragment, Type clrType,  out RelationalTypeMapping mapping)
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

            return FindCustomMapping(new RelationalTypeMappingInfo(property));
        }

        /// <summary>
        ///     Gets the relational database type for the given property, using a separate type mapper if needed.
        ///     This base implementation uses custom mappers for string and byte array properties.
        ///     Returns null if no mapping is found.
        /// </summary>
        /// <param name="typeMappingInfo"> The input data to the mapping process. </param>
        /// <returns> The type mapping to be used. </returns>
        protected virtual RelationalTypeMapping FindCustomMapping([NotNull] RelationalTypeMappingInfo typeMappingInfo)
        {
            Check.NotNull(typeMappingInfo, nameof(typeMappingInfo));

            return typeMappingInfo.TargetClrType == typeof(string)
                ? (GetStringMapping(typeMappingInfo)
                   ?? (typeMappingInfo.Property == null ? null : GetStringMapping(typeMappingInfo.Property)))
                : typeMappingInfo.TargetClrType == typeof(byte[])
                    ? (GetByteArrayMapping(typeMappingInfo)
                       ?? (typeMappingInfo.Property == null ? null : GetByteArrayMapping(typeMappingInfo.Property)))
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

            return GetStringMapping(new RelationalTypeMappingInfo(property));
        }

        /// <summary>
        ///     Gets the relational database type for the given byte array property.
        /// </summary>
        /// <param name="property"> The property to get the mapping for. </param>
        /// <returns> The type mapping to be used. </returns>
        protected virtual RelationalTypeMapping GetByteArrayMapping([NotNull] IProperty property)
        {
            Check.NotNull(property, nameof(property));

            return GetByteArrayMapping(new RelationalTypeMappingInfo(property));
        }

        /// <summary>
        ///     Gets the relational database type for the given string property.
        /// </summary>
        /// <param name="typeMappingInfo"> The input data to the mapping process. </param>
        /// <returns> The type mapping to be used. </returns>
        protected virtual RelationalTypeMapping GetStringMapping(
            [NotNull] RelationalTypeMappingInfo typeMappingInfo)
        {
            Check.NotNull(typeMappingInfo, nameof(typeMappingInfo));

            return StringMapper?.FindMapping(
                typeMappingInfo.IsUnicode != false,
                typeMappingInfo.IsKeyOrIndex,
                typeMappingInfo.Size);
        }

        /// <summary>
        ///     Gets the relational database type for the given byte array property.
        /// </summary>
        /// <param name="typeMappingInfo"> The input data to the mapping process. </param>
        /// <returns> The type mapping to be used. </returns>
        protected virtual RelationalTypeMapping GetByteArrayMapping(
            [NotNull] RelationalTypeMappingInfo typeMappingInfo)
        {
            Check.NotNull(typeMappingInfo, nameof(typeMappingInfo));

            return ByteArrayMapper?.FindMapping(
                typeMappingInfo.IsRowVersion == true,
                typeMappingInfo.IsKeyOrIndex,
                typeMappingInfo.Size);
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
        protected class RelationalTypeMappingInfo : TypeMappingInfo
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
                if (storeTypeName == null
                    && property != null)
                {
                    storeTypeName = property
                        .FindPrincipals()
                        .Select(p => (string)p[RelationalAnnotationNames.ColumnType])
                        .FirstOrDefault(t => t != null);

                }

                StoreTypeName = storeTypeName;
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
            }

            /// <summary>
            ///     Returns a new <see cref="RelationalTypeMappingInfo" /> with the given converter applied.
            /// </summary>
            /// <param name="converterInfo"> The converter to apply. </param>
            /// <returns> The new mapping info. </returns>
            public override TypeMappingInfo WithBuiltInConverter(ValueConverterInfo converterInfo)
                => new RelationalTypeMappingInfo(this, converterInfo);

            /// <summary>
            ///     The provider-specific relational type name for which mapping is needed.
            /// </summary>
            public virtual string StoreTypeName { get; }

            /// <summary>
            /// Compares this <see cref="RelationalTypeMappingInfo"/> to another to check if they represent the same mapping.
            /// </summary>
            /// <param name="other"> The other object. </param>
            /// <returns> <c>True</c> if they represent the same mapping; <c>false</c> otherwise. </returns>
            protected bool Equals(RelationalTypeMappingInfo other)
                => Equals((TypeMappingInfo)other)
                   && StoreTypeName == other.StoreTypeName;

            /// <summary>
            /// Compares this <see cref="RelationalTypeMappingInfo"/> to another to check if they represent the same mapping.
            /// </summary>
            /// <param name="obj"> The other object. </param>
            /// <returns> <c>True</c> if they represent the same mapping; <c>false</c> otherwise. </returns>
            public override bool Equals(object obj)
                => !ReferenceEquals(null, obj)
                   && (ReferenceEquals(this, obj)
                       || obj.GetType() == GetType()
                       && Equals((RelationalTypeMappingInfo)obj));

            /// <summary>
            /// Returns a hash code for this object.
            /// </summary>
            /// <returns> The hash code. </returns>
            public override int GetHashCode()
                => (base.GetHashCode() * 397) ^ (StoreTypeName?.GetHashCode() ?? 0);
        }
    }
}

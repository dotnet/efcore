// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Reflection;
using JetBrains.Annotations;
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
    public abstract class RelationalTypeMapping : CoreTypeMapping
    {
        /// <summary>
        ///     Parameter object for use in the <see cref="RelationalTypeMapping" /> hierarchy.
        /// </summary>
        protected readonly struct RelationalTypeMappingParameters
        {
            /// <summary>
            ///     Creates a new <see cref="RelationalTypeMappingParameters" /> parameter object.
            /// </summary>
            /// <param name="coreParameters"> Parameters for the <see cref="CoreTypeMapping"/> base class. </param>
            /// <param name="storeType"> The name of the database type. </param>
            /// <param name="storeTypePostfix"> Indicates which values should be appended to the store type name. </param>
            /// <param name="dbType"> The <see cref="System.Data.DbType" /> to be used. </param>
            /// <param name="unicode"> A value indicating whether the type should handle Unicode data or not. </param>
            /// <param name="size"> The size of data the property is configured to store, or null if no size is configured. </param>
            /// <param name="fixedLength"> A value indicating whether the type is constrained to fixed-length data. </param>
            /// <param name="precision"> The precision of data the property is configured to store, or null if no size is configured. </param>
            /// <param name="scale"> The scale of data the property is configured to store, or null if no size is configured. </param>
            public RelationalTypeMappingParameters(
                CoreTypeMappingParameters coreParameters,
                [NotNull] string storeType,
                StoreTypePostfix storeTypePostfix = StoreTypePostfix.None,
                DbType? dbType = null,
                bool unicode = false,
                int? size = null,
                bool fixedLength = false,
                int? precision = null,
                int? scale = null)
            {
                Check.NotEmpty(storeType, nameof(storeType));

                CoreParameters = coreParameters;
                StoreType = storeType;
                StoreTypePostfix = storeTypePostfix;
                DbType = dbType;
                Unicode = unicode;
                Size = size;
                Precision = precision;
                Scale = scale;
                FixedLength = fixedLength;
            }

            /// <summary>
            ///     Parameters for the <see cref="CoreTypeMapping"/> base class.
            /// </summary>
            public CoreTypeMappingParameters CoreParameters { get; }

            /// <summary>
            ///     The mapping store type.
            /// </summary>
            public string StoreType { get; }

            /// <summary>
            ///     The mapping DbType.
            /// </summary>
            public DbType? DbType { get; }

            /// <summary>
            ///     The mapping Unicode flag.
            /// </summary>
            public bool Unicode { get; }

            /// <summary>
            ///     The mapping size.
            /// </summary>
            public int? Size { get; }

            /// <summary>
            ///     The mapping precision.
            /// </summary>
            public int? Precision { get; }

            /// <summary>
            ///     The mapping scale.
            /// </summary>
            public int? Scale { get; }

            /// <summary>
            ///     The mapping fixed-length flag.
            /// </summary>
            public bool FixedLength { get; }

            /// <summary>
            ///     Indicates which values should be appended to the store type name.
            /// </summary>
            public StoreTypePostfix StoreTypePostfix { get; }

            /// <summary>
            ///     Creates a new <see cref="RelationalTypeMappingParameters" /> parameter object with the given
            ///     store type and size.
            /// </summary>
            /// <param name="storeType"> The new store type name. </param>
            /// <param name="size"> The new size. </param>
            /// <param name="storeTypePostfix"> The new postfix, or null to leave unchanged. </param>
            /// <returns> The new parameter object. </returns>
            public RelationalTypeMappingParameters WithStoreTypeAndSize(
                [NotNull] string storeType,
                int? size,
                StoreTypePostfix? storeTypePostfix = null)
                => new RelationalTypeMappingParameters(
                    CoreParameters,
                    storeType,
                    storeTypePostfix ?? StoreTypePostfix,
                    DbType,
                    Unicode,
                    size,
                    FixedLength,
                    Precision,
                    Scale);

            /// <summary>
            ///     Creates a new <see cref="RelationalTypeMappingParameters" /> parameter object with the given
            ///     converter composed with any existing converter and set on the new parameter object.
            /// </summary>
            /// <param name="converter"> The converter. </param>
            /// <returns> The new parameter object. </returns>
            public RelationalTypeMappingParameters WithComposedConverter([CanBeNull] ValueConverter converter)
                => new RelationalTypeMappingParameters(
                    CoreParameters.WithComposedConverter(converter),
                    StoreType,
                    StoreTypePostfix,
                    DbType,
                    Unicode,
                    Size,
                    FixedLength,
                    Precision,
                    Scale);
        }

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

        /// <summary>
        ///     Gets the mapping to be used when the only piece of information is that there is a null value.
        /// </summary>
        public static readonly RelationalTypeMapping NullMapping = new NullTypeMapping("NULL");

        private class NullTypeMapping : RelationalTypeMapping
        {
            public NullTypeMapping(string storeType)
                : base(storeType, typeof(object))
            {
            }

            public override RelationalTypeMapping Clone(string storeType, int? size)
                => this;

            public override CoreTypeMapping Clone(ValueConverter converter)
                => this;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelationalTypeMapping" /> class.
        /// </summary>
        /// <param name="parameters"> The parameters for this mapping. </param>
        protected RelationalTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters.CoreParameters)
        {
            Parameters = parameters;

            var converter = parameters.CoreParameters.Converter;

            var size = parameters.Size ?? converter?.MappingHints?.Size;
            Size = size;

            var storeType = parameters.StoreType;

            if (storeType != null)
            {
                if (size != null
                    && parameters.StoreTypePostfix == StoreTypePostfix.Size)
                {
                    storeType = GetBaseName(storeType) + "(" + size + ")";
                }
                else if (parameters.StoreTypePostfix == StoreTypePostfix.PrecisionAndScale
                         || parameters.StoreTypePostfix == StoreTypePostfix.Precision)
                {
                    var precision = converter?.MappingHints?.Precision;

                    if (precision != null)
                    {
                        var scale = converter?.MappingHints?.Scale;

                        storeType = GetBaseName(storeType)
                                    + "("
                                    + (scale == null || parameters.StoreTypePostfix == StoreTypePostfix.Precision
                                        ? precision.ToString()
                                        : precision + "," + scale)
                                    + ")";
                    }
                }
            }

            StoreType = storeType;
        }

        private static string GetBaseName(string storeType)
        {
            var openParen = storeType.IndexOf("(", StringComparison.Ordinal);
            if (openParen >= 0)
            {
                storeType = storeType.Substring(0, openParen);
            }

            return storeType;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelationalTypeMapping" /> class.
        /// </summary>
        /// <param name="storeType"> The name of the database type. </param>
        /// <param name="clrType"> The .NET type. </param>
        /// <param name="dbType"> The <see cref="System.Data.DbType" /> to be used. </param>
        /// <param name="unicode"> A value indicating whether the type should handle Unicode data or not. </param>
        /// <param name="size"> The size of data the property is configured to store, or null if no size is configured. </param>
        protected RelationalTypeMapping(
            [NotNull] string storeType,
            [NotNull] Type clrType,
            DbType? dbType = null,
            bool unicode = false,
            int? size = null)
            : this(
                new RelationalTypeMappingParameters(
                    new CoreTypeMappingParameters(clrType), storeType, StoreTypePostfix.None, dbType, unicode, size))
        {
        }

        /// <summary>
        ///     Returns the parameters used to create this type mapping.
        /// </summary>
        protected new virtual RelationalTypeMappingParameters Parameters { get; }

        /// <summary>
        ///     Creates a copy of this mapping.
        /// </summary>
        /// <param name="storeType"> The name of the database type. </param>
        /// <param name="size"> The size of data the property is configured to store, or null if no size is configured. </param>
        /// <returns> The newly created mapping. </returns>
        public abstract RelationalTypeMapping Clone([NotNull] string storeType, int? size);

        /// <summary>
        ///    Returns a new copy of this type mapping with the given <see cref="ValueConverter"/>
        ///    added.
        /// </summary>
        /// <param name="converter"> The converter to use. </param>
        /// <returns> A new type mapping </returns>
        public override CoreTypeMapping Clone(ValueConverter converter)
            => throw new NotImplementedException(CoreStrings.ConverterCloneNotImplemented(GetType().ShortDisplayName()));

        /// <summary>
        ///     Clones the type mapping to update the name and size from the mapping info, if needed.
        /// </summary>
        /// <param name="mappingInfo"> The mapping info containing the facets to use. </param>
        /// <returns> The cloned mapping, or the original mapping if no clone was needed. </returns>
        public virtual RelationalTypeMapping Clone(in RelationalTypeMappingInfo mappingInfo)
            => (mappingInfo.Size != null
                && mappingInfo.Size != Size)
               || (mappingInfo.StoreTypeName != null
                   && !string.Equals(mappingInfo.StoreTypeName, StoreType, StringComparison.Ordinal))
                ? Clone(
                    mappingInfo.StoreTypeName ?? StoreType,
                    mappingInfo.Size ?? Size)
                : this;

        /// <summary>
        ///     Gets the name of the database type.
        /// </summary>
        public virtual StoreTypePostfix StoreTypePostfix => Parameters.StoreTypePostfix;

        /// <summary>
        ///     Gets the name of the database type.
        /// </summary>
        public virtual string StoreType { get; }

        /// <summary>
        ///     Gets the <see cref="System.Data.DbType" /> to be used.
        /// </summary>
        public virtual DbType? DbType => Parameters.DbType;

        /// <summary>
        ///     Gets a value indicating whether the type should handle Unicode data or not.
        /// </summary>
        public virtual bool IsUnicode => Parameters.Unicode;

        /// <summary>
        ///     Gets the size of data the property is configured to store, or null if no size is configured.
        /// </summary>
        public virtual int? Size { get; }

        /// <summary>
        ///     Gets a value indicating whether the type is constrained to fixed-length data.
        /// </summary>
        public virtual bool IsFixedLength => Parameters.FixedLength;

        /// <summary>
        ///     Gets the string format to be used to generate SQL literals of this type.
        /// </summary>
        protected virtual string SqlLiteralFormatString { get; } = "{0}";

        /// <summary>
        ///     Creates a <see cref="DbParameter" /> with the appropriate type information configured.
        /// </summary>
        /// <param name="command"> The command the parameter should be created on. </param>
        /// <param name="name"> The name of the parameter. </param>
        /// <param name="value"> The value to be assigned to the parameter. </param>
        /// <param name="nullable"> A value indicating whether the parameter should be a nullable type. </param>
        /// <returns> The newly created parameter. </returns>
        public virtual DbParameter CreateParameter(
            [NotNull] DbCommand command,
            [NotNull] string name,
            [CanBeNull] object value,
            bool? nullable = null)
        {
            Check.NotNull(command, nameof(command));

            var parameter = command.CreateParameter();
            parameter.Direction = ParameterDirection.Input;
            parameter.ParameterName = name;

            if (Converter != null
                && ((AppContext.TryGetSwitch("Microsoft.EntityFrameworkCore.Issue12557", out var isEnabled) && isEnabled)
                    || !IsLegacyEnumValue(Converter, value)))
            {
                value = Converter.ConvertToProvider(value);
            }

            parameter.Value = value ?? DBNull.Value;

            if (nullable.HasValue)
            {
                parameter.IsNullable = nullable.Value;
            }

            if (DbType.HasValue)
            {
                parameter.DbType = DbType.Value;
            }

            if (Size.HasValue
                && Size.Value != -1)
            {
                parameter.Size = Size.Value;
            }

            ConfigureParameter(parameter);

            return parameter;
        }

        // Avoid converting value from enum to integer if it is already an integer; preserves 2.0 behavior
        private static bool IsLegacyEnumValue(ValueConverter converter, object value)
            => value != null
               && IsLegacyInteger(value.GetType())
               && converter.GetType().IsGenericType
               && converter.GetType().GetGenericTypeDefinition() == typeof(EnumToNumberConverter<,>);

        private static bool IsLegacyInteger(Type type)
        {
            type = type.UnwrapNullableType();

            return type == typeof(int)
                   || type == typeof(long)
                   || type == typeof(short)
                   || type == typeof(byte);
        }

        /// <summary>
        ///     Configures type information of a <see cref="DbParameter" />.
        /// </summary>
        /// <param name="parameter"> The parameter to be configured. </param>
        protected virtual void ConfigureParameter([NotNull] DbParameter parameter)
        {
        }

        /// <summary>
        ///     Generates the SQL representation of a literal value.
        /// </summary>
        /// <param name="value">The literal value.</param>
        /// <returns>
        ///     The generated string.
        /// </returns>
        public virtual string GenerateSqlLiteral([CanBeNull] object value)
        {
            if (Converter != null)
            {
                value = Converter.ConvertToProvider(value);
            }

            return GenerateProviderValueSqlLiteral(value);
        }

        /// <summary>
        ///     Generates the SQL representation of a literal value without conversion.
        /// </summary>
        /// <param name="value">The literal value.</param>
        /// <returns>
        ///     The generated string.
        /// </returns>
        public virtual string GenerateProviderValueSqlLiteral([CanBeNull] object value)
            => value == null
                ? "NULL"
                : GenerateNonNullSqlLiteral(value);

        /// <summary>
        ///     Generates the SQL representation of a non-null literal value.
        /// </summary>
        /// <param name="value">The literal value.</param>
        /// <returns>
        ///     The generated string.
        /// </returns>
        protected virtual string GenerateNonNullSqlLiteral([NotNull] object value)
            => string.Format(CultureInfo.InvariantCulture, SqlLiteralFormatString, Check.NotNull(value, nameof(value)));

        /// <summary>
        ///     The method to use when reading values of the given type. The method must be defined
        ///     on <see cref="DbDataReader" /> or one of its subclasses.
        /// </summary>
        /// <returns> The method to use to read the value. </returns>
        public virtual MethodInfo GetDataReaderMethod()
        {
            var type = (Converter?.ProviderClrType ?? ClrType).UnwrapNullableType();

            return _getXMethods.TryGetValue(type, out var method)
                ? method
                : _getFieldValueMethod.MakeGenericMethod(type);
        }
    }
}

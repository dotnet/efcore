// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Utilities;

#pragma warning disable 618
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
            /// <param name="coreParameters"> Parameters for the <see cref="CoreTypeMapping" /> base class. </param>
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

                var converterHints = coreParameters.Converter?.MappingHints;

                CoreParameters = coreParameters;
                StoreType = storeType;
                StoreTypePostfix = storeTypePostfix;
                DbType = dbType;
                Unicode = unicode;
                Size = size ?? converterHints?.Size;
                Precision = precision ?? converterHints?.Precision;
                Scale = scale ?? converterHints?.Scale;
                FixedLength = fixedLength;
                PrecisionAndScaleOverriden = false;
            }

            // #12405
            private RelationalTypeMappingParameters(
                CoreTypeMappingParameters coreParameters,
                [NotNull] string storeType,
                StoreTypePostfix storeTypePostfix = StoreTypePostfix.None,
                DbType? dbType = null,
                bool unicode = false,
                int? size = null,
                bool fixedLength = false,
                int? precision = null,
                int? scale = null,
                bool precisionAndScaleOverriden = false)
                : this(coreParameters, storeType, storeTypePostfix, dbType, unicode, size, fixedLength, precision, scale)
            {
                PrecisionAndScaleOverriden = precisionAndScaleOverriden;
            }

            /// <summary>
            ///     Parameters for the <see cref="CoreTypeMapping" /> base class.
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
            ///     This is provided for compatibility with 2.1 providers and shouldn't be used
            /// </summary>
            // If not set fallback to 2.1 behavior by using Precision and Scale from the converter
            // #12405
            [Obsolete("This is provided for compatibility with 2.1 providers and shouldn't be used")]
            public bool PrecisionAndScaleOverriden { get; }

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
            ///     mapping info.
            /// </summary>
            /// <param name="mappingInfo"> The mapping info containing the facets to use. </param>
            /// <returns> The new parameter object. </returns>
            public RelationalTypeMappingParameters WithTypeMappingInfo(in RelationalTypeMappingInfo mappingInfo)
                => new RelationalTypeMappingParameters(
                    CoreParameters,
                    mappingInfo.StoreTypeName ?? StoreType,
                    StoreTypePostfix,
                    DbType,
                    mappingInfo.IsUnicode ?? Unicode,
                    mappingInfo.Size ?? Size,
                    mappingInfo.IsFixedLength ?? FixedLength,
                    mappingInfo.Precision ?? Precision,
                    mappingInfo.Scale ?? Scale,
                    PrecisionAndScaleOverriden);

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
                    Scale,
                    PrecisionAndScaleOverriden);

            /// <summary>
            ///     Creates a new <see cref="RelationalTypeMappingParameters" /> parameter object with the given
            ///     store type and size.
            /// </summary>
            /// <param name="precision"> The precision of data the property is configured to store, or null if no size is configured. </param>
            /// <param name="scale"> The scale of data the property is configured to store, or null if no size is configured. </param>
            /// <returns> The new parameter object. </returns>
            public RelationalTypeMappingParameters WithPrecisionAndScale(
                int? precision,
                int? scale)
                => new RelationalTypeMappingParameters(
                    CoreParameters,
                    StoreType,
                    StoreTypePostfix,
                    DbType,
                    Unicode,
                    Size,
                    FixedLength,
                    precision,
                    scale,
                    precisionAndScaleOverriden: true);

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
                    Scale,
                    PrecisionAndScaleOverriden);
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

        private readonly bool _precisionAndScaleOverriden;

        private class NullTypeMapping : RelationalTypeMapping
        {
            public NullTypeMapping(string storeType)
                : base(storeType, typeof(object))
            {
            }

            protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
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
            _precisionAndScaleOverriden = parameters.PrecisionAndScaleOverriden;

            var size = parameters.Size;
            var storeType = parameters.StoreType;

            if (storeType != null)
            {
                StoreTypeNameBase = GetBaseName(storeType);
                if (size != null
                    && parameters.StoreTypePostfix == StoreTypePostfix.Size)
                {
                    storeType = StoreTypeNameBase + "(" + size + ")";
                }
                else if (parameters.StoreTypePostfix == StoreTypePostfix.PrecisionAndScale
                         || parameters.StoreTypePostfix == StoreTypePostfix.Precision)
                {
                    var precision = parameters.Precision;
                    var converter = parameters.CoreParameters.Converter;
                    // Fallback to 2.1 behavior
                    // #12405
                    var oldBehavior = !_precisionAndScaleOverriden;
                    if (oldBehavior)
                    {
                        precision = converter?.MappingHints?.Precision;
                    }

                    if (precision != null)
                    {
                        var scale = parameters.Scale;
                        if (oldBehavior)
                        {
                            scale = converter.MappingHints?.Scale;
                        }

                        storeType = StoreTypeNameBase
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
            : this(new RelationalTypeMappingParameters(
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
        /// <param name="parameters"> The parameters for this mapping. </param>
        /// <returns> The newly created mapping. </returns>
        protected virtual RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => throw new NotImplementedException(CoreStrings.ConverterCloneNotImplemented(GetType().ShortDisplayName()));

        /// <summary>
        ///     Creates a copy of this mapping.
        /// </summary>
        /// <param name="storeType"> The name of the database type. </param>
        /// <param name="size"> The size of data the property is configured to store, or null if no size is configured. </param>
        /// <returns> The newly created mapping. </returns>
        public virtual RelationalTypeMapping Clone([NotNull] string storeType, int? size)
            => Clone(Parameters.WithStoreTypeAndSize(storeType, size));

        /// <summary>
        ///     Creates a copy of this mapping.
        /// </summary>
        /// <param name="precision"> The precision of data the property is configured to store, or null if no size is configured. </param>
        /// <param name="scale"> The scale of data the property is configured to store, or null if no size is configured. </param>
        /// <returns> The newly created mapping. </returns>
        public virtual RelationalTypeMapping Clone(int? precision, int? scale)
            => Clone(Parameters.WithPrecisionAndScale(precision, scale));

        /// <summary>
        ///     Returns a new copy of this type mapping with the given <see cref="ValueConverter" />
        ///     added.
        /// </summary>
        /// <param name="converter"> The converter to use. </param>
        /// <returns> A new type mapping </returns>
        public override CoreTypeMapping Clone(ValueConverter converter)
            => Clone(Parameters.WithComposedConverter(converter));

        /// <summary>
        ///     Clones the type mapping to update facets from the mapping info, if needed.
        /// </summary>
        /// <param name="mappingInfo"> The mapping info containing the facets to use. </param>
        /// <returns> The cloned mapping, or the original mapping if no clone was needed. </returns>
        public virtual RelationalTypeMapping Clone(in RelationalTypeMappingInfo mappingInfo)
        {
            var checkStoreTypeAndSize = true;
            RelationalTypeMapping clone = null;
            if ((mappingInfo.Scale != null
                 && mappingInfo.Scale != Parameters.Scale
                 && StoreTypePostfix == StoreTypePostfix.PrecisionAndScale)
                || (mappingInfo.Precision != null
                    && mappingInfo.Precision != Parameters.Precision
                    && (StoreTypePostfix == StoreTypePostfix.PrecisionAndScale
                        || StoreTypePostfix == StoreTypePostfix.Precision)))
            {
                var oldBehavior = !_precisionAndScaleOverriden;
                if (!oldBehavior)
                {
                    var storeTypeChanged = mappingInfo.StoreTypeNameBase != null
                                            && !string.Equals(mappingInfo.StoreTypeNameBase, StoreTypeNameBase, StringComparison.OrdinalIgnoreCase);

                    clone = storeTypeChanged
                        ? Clone(Parameters.WithTypeMappingInfo(mappingInfo))
                        : Clone(
                            mappingInfo.Precision ?? Parameters.Precision,
                            mappingInfo.Scale ?? Parameters.Scale);

                    // Fallback to 2.1 behavior if Clone is not overriden
                    // #12405
                    oldBehavior = clone.GetType() != GetType();
                }

                checkStoreTypeAndSize = oldBehavior;
            }

            if (checkStoreTypeAndSize)
            {
                var storeTypeOrSizeChanged = (mappingInfo.Size != null
                                              && mappingInfo.Size != Size
                                              && StoreTypePostfix == StoreTypePostfix.Size)
                                             || (mappingInfo.StoreTypeName != null
                                                 && !string.Equals(mappingInfo.StoreTypeName, StoreType, StringComparison.OrdinalIgnoreCase));

                clone = storeTypeOrSizeChanged
                    ? Clone(
                        mappingInfo.StoreTypeName ?? StoreType,
                        mappingInfo.Size ?? Size)
                    : this;
            }

            if (clone.GetType() != GetType())
            {
                throw new NotImplementedException(CoreStrings.ConverterCloneNotImplemented(GetType().ShortDisplayName()));
            }

            return clone;
        }

        /// <summary>
        ///     Gets the name of the database type.
        /// </summary>
        public virtual StoreTypePostfix StoreTypePostfix => Parameters.StoreTypePostfix;

        /// <summary>
        ///     Gets the name of the database type.
        /// </summary>
        public virtual string StoreType { get; }

        /// <summary>
        ///     Gets the base name of the database type.
        /// </summary>
        public virtual string StoreTypeNameBase { get; }

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
        public virtual int? Size => Parameters.Size;

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
                && !IsLegacyEnumValue(Converter, value))
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

        /// <summary>
        ///     Gets a custom expression tree for reading the value from the input data reader
        ///     expression that contains the database value.
        /// </summary>
        /// <param name="expression"> The input expression, containing the database value. </param>
        /// <returns> The expression with customization added. </returns>
        public virtual Expression CustomizeDataReaderExpression([NotNull] Expression expression)
            => expression;
    }
}

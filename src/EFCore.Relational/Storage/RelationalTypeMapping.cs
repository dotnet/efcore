// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using System.Globalization;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     <para>
///         Represents the mapping between a .NET type and a database type.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
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
        /// <param name="coreParameters">Parameters for the <see cref="CoreTypeMapping" /> base class.</param>
        /// <param name="storeType">The name of the database type.</param>
        /// <param name="storeTypePostfix">Indicates which values should be appended to the store type name.</param>
        /// <param name="dbType">The <see cref="System.Data.DbType" /> to be used.</param>
        /// <param name="unicode">A value indicating whether the type should handle Unicode data or not.</param>
        /// <param name="size">The size of data the property is configured to store, or null if no size is configured.</param>
        /// <param name="fixedLength">A value indicating whether the type is constrained to fixed-length data.</param>
        /// <param name="precision">The precision of data the property is configured to store, or null if no size is configured.</param>
        /// <param name="scale">The scale of data the property is configured to store, or null if no size is configured.</param>
        public RelationalTypeMappingParameters(
            CoreTypeMappingParameters coreParameters,
            string storeType,
            StoreTypePostfix storeTypePostfix = StoreTypePostfix.None,
            DbType? dbType = null,
            bool unicode = false,
            int? size = null,
            bool fixedLength = false,
            int? precision = null,
            int? scale = null)
        {
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
        /// <param name="mappingInfo">The mapping info containing the facets to use.</param>
        /// <returns>The new parameter object.</returns>
        public RelationalTypeMappingParameters WithTypeMappingInfo(in RelationalTypeMappingInfo mappingInfo)
            => new(
                CoreParameters,
                mappingInfo.StoreTypeName ?? StoreType,
                StoreTypePostfix,
                DbType,
                mappingInfo.IsUnicode ?? Unicode,
                mappingInfo.Size ?? Size,
                mappingInfo.IsFixedLength ?? FixedLength,
                mappingInfo.Precision ?? Precision,
                mappingInfo.Scale ?? Scale);

        /// <summary>
        ///     Creates a new <see cref="RelationalTypeMappingParameters" /> parameter object with the given
        ///     store type and size.
        /// </summary>
        /// <param name="storeType">The new store type name.</param>
        /// <param name="size">The new size.</param>
        /// <param name="storeTypePostfix">The new postfix, or null to leave unchanged.</param>
        /// <returns>The new parameter object.</returns>
        public RelationalTypeMappingParameters WithStoreTypeAndSize(
            string storeType,
            int? size,
            StoreTypePostfix? storeTypePostfix = null)
            => new(
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
        ///     Creates a new <see cref="RelationalTypeMappingParameters" /> parameter object with the given precision and scale
        /// </summary>
        /// <param name="precision">The precision of data the property is configured to store, or null if no size is configured.</param>
        /// <param name="scale">The scale of data the property is configured to store, or null if no size is configured.</param>
        /// <returns>The new parameter object.</returns>
        public RelationalTypeMappingParameters WithPrecisionAndScale(
            int? precision,
            int? scale)
            => new(
                CoreParameters,
                StoreType,
                StoreTypePostfix,
                DbType,
                Unicode,
                Size,
                FixedLength,
                precision,
                scale);

        /// <summary>
        ///     Creates a new <see cref="RelationalTypeMappingParameters" /> parameter object with the given precision.
        /// </summary>
        /// <param name="precision">The precision of data the property is configured to store, or null if no size is configured.</param>
        /// <returns>The new parameter object.</returns>
        public RelationalTypeMappingParameters WithPrecision(int? precision)
            => new(
                CoreParameters,
                StoreType,
                StoreTypePostfix,
                DbType,
                Unicode,
                Size,
                FixedLength,
                precision,
                Scale);

        /// <summary>
        ///     Creates a new <see cref="RelationalTypeMappingParameters" /> parameter object with the given scale.
        /// </summary>
        /// <param name="scale">The scale of data the property is configured to store, or null if no size is configured.</param>
        /// <returns>The new parameter object.</returns>
        public RelationalTypeMappingParameters WithScale(int? scale)
            => new(
                CoreParameters,
                StoreType,
                StoreTypePostfix,
                DbType,
                Unicode,
                Size,
                FixedLength,
                Precision,
                scale);

        /// <summary>
        ///     Creates a new <see cref="RelationalTypeMappingParameters" /> parameter object with the given
        ///     converter composed with any existing converter and set on the new parameter object.
        /// </summary>
        /// <param name="converter">The converter.</param>
        /// <returns>The new parameter object.</returns>
        public RelationalTypeMappingParameters WithComposedConverter(ValueConverter? converter)
            => new(
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

    private static readonly MethodInfo GetFieldValueMethod
        = GetDataReaderMethod(nameof(DbDataReader.GetFieldValue));

    private static readonly IDictionary<Type, MethodInfo> GetXMethods
        = new Dictionary<Type, MethodInfo>
        {
            { typeof(bool), GetDataReaderMethod(nameof(DbDataReader.GetBoolean)) },
            { typeof(byte), GetDataReaderMethod(nameof(DbDataReader.GetByte)) },
            { typeof(char), GetDataReaderMethod(nameof(DbDataReader.GetChar)) },
            { typeof(DateTime), GetDataReaderMethod(nameof(DbDataReader.GetDateTime)) },
            { typeof(decimal), GetDataReaderMethod(nameof(DbDataReader.GetDecimal)) },
            { typeof(double), GetDataReaderMethod(nameof(DbDataReader.GetDouble)) },
            { typeof(float), GetDataReaderMethod(nameof(DbDataReader.GetFloat)) },
            { typeof(Guid), GetDataReaderMethod(nameof(DbDataReader.GetGuid)) },
            { typeof(short), GetDataReaderMethod(nameof(DbDataReader.GetInt16)) },
            { typeof(int), GetDataReaderMethod(nameof(DbDataReader.GetInt32)) },
            { typeof(long), GetDataReaderMethod(nameof(DbDataReader.GetInt64)) },
            { typeof(string), GetDataReaderMethod(nameof(DbDataReader.GetString)) }
        };

    private static MethodInfo GetDataReaderMethod(string name)
        => typeof(DbDataReader).GetRuntimeMethod(name, new[] { typeof(int) })!;

    /// <summary>
    ///     Gets the mapping to be used when the only piece of information is that there is a null value.
    /// </summary>
    public static readonly RelationalTypeMapping NullMapping = new NullTypeMapping("NULL");

    private sealed class NullTypeMapping : RelationalTypeMapping
    {
        public NullTypeMapping(string storeType)
            : base(storeType, typeof(object))
        {
        }

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => this;
    }

    private ValueComparer? _comparer;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RelationalTypeMapping" /> class.
    /// </summary>
    /// <param name="parameters">The parameters for this mapping.</param>
    protected RelationalTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters.CoreParameters)
    {
        Parameters = parameters;

        var storeType = parameters.StoreType;
        var storeTypeNameBase = GetBaseName(storeType);

        StoreTypeNameBase = storeTypeNameBase;
        StoreType = ProcessStoreType(parameters, storeType, storeTypeNameBase);

        static string GetBaseName(string storeType)
        {
            var openParen = storeType.IndexOf("(", StringComparison.Ordinal);
            if (openParen >= 0)
            {
                storeType = storeType[..openParen];
            }

            return storeType;
        }
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="RelationalTypeMapping" /> class.
    /// </summary>
    /// <param name="storeType">The name of the database type.</param>
    /// <param name="clrType">The .NET type.</param>
    /// <param name="dbType">The <see cref="System.Data.DbType" /> to be used.</param>
    /// <param name="unicode">A value indicating whether the type should handle Unicode data or not.</param>
    /// <param name="size">The size of data the property is configured to store, or null if no size is configured.</param>
    /// <param name="fixedLength">A value indicating whether the type has fixed length data or not.</param>
    /// <param name="precision">The precision of data the property is configured to store, or null if no precision is configured.</param>
    /// <param name="scale">The scale of data the property is configured to store, or null if no scale is configured.</param>
    protected RelationalTypeMapping(
        string storeType,
        Type clrType,
        DbType? dbType = null,
        bool unicode = false,
        int? size = null,
        bool fixedLength = false,
        int? precision = null,
        int? scale = null)
        : this(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(clrType), storeType, StoreTypePostfix.None, dbType, unicode, size, fixedLength, precision,
                scale))
    {
    }

    /// <summary>
    ///     Returns the parameters used to create this type mapping.
    /// </summary>
    protected new virtual RelationalTypeMappingParameters Parameters { get; }

    /// <summary>
    ///     Gets the name of the database type.
    /// </summary>
    public virtual StoreTypePostfix StoreTypePostfix
        => Parameters.StoreTypePostfix;

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
    public virtual DbType? DbType
        => Parameters.DbType;

    /// <summary>
    ///     Gets a value indicating whether the type should handle Unicode data or not.
    /// </summary>
    public virtual bool IsUnicode
        => Parameters.Unicode;

    /// <summary>
    ///     Gets the size of data the property is configured to store, or null if no size is configured.
    /// </summary>
    public virtual int? Size
        => Parameters.Size;

    /// <summary>
    ///     Gets the precision of data the property is configured to store, or null if no precision is configured.
    /// </summary>
    public virtual int? Precision
        => Parameters.Precision;

    /// <summary>
    ///     Gets the scale of data the property is configured to store, or null if no scale is configured.
    /// </summary>
    public virtual int? Scale
        => Parameters.Scale;

    /// <summary>
    ///     Gets a value indicating whether the type is constrained to fixed-length data.
    /// </summary>
    public virtual bool IsFixedLength
        => Parameters.FixedLength;

    /// <summary>
    ///     Gets the string format to be used to generate SQL literals of this type.
    /// </summary>
    protected virtual string SqlLiteralFormatString
        => "{0}";

    /// <summary>
    ///     A <see cref="ValueComparer" /> for the provider CLR type values.
    /// </summary>
    public virtual ValueComparer ProviderComparer
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _comparer,
            this,
            static c => ValueComparer.CreateDefault(c.Converter?.ProviderClrType ?? c.ClrType, favorStructuralComparisons: true));

    /// <summary>
    ///     Creates a copy of this mapping.
    /// </summary>
    /// <param name="parameters">The parameters for this mapping.</param>
    /// <returns>The newly created mapping.</returns>
    protected abstract RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters);

    /// <summary>
    ///     Creates a copy of this mapping.
    /// </summary>
    /// <param name="storeType">The name of the database type.</param>
    /// <param name="size">The size of data the property is configured to store, or null if no size is configured.</param>
    /// <returns>The newly created mapping.</returns>
    public virtual RelationalTypeMapping Clone(string storeType, int? size)
        => Clone(Parameters.WithStoreTypeAndSize(storeType, size));

    /// <summary>
    ///     Creates a copy of this mapping.
    /// </summary>
    /// <param name="precision">The precision of data the property is configured to store, or null if no size is configured.</param>
    /// <param name="scale">The scale of data the property is configured to store, or null if no size is configured.</param>
    /// <returns>The newly created mapping.</returns>
    public virtual RelationalTypeMapping Clone(int? precision, int? scale)
        => Clone(Parameters.WithPrecisionAndScale(precision, scale));

    /// <summary>
    ///     Returns a new copy of this type mapping with the given <see cref="ValueConverter" />
    ///     added.
    /// </summary>
    /// <param name="converter">The converter to use.</param>
    /// <returns>A new type mapping</returns>
    public override CoreTypeMapping Clone(ValueConverter? converter)
        => Clone(Parameters.WithComposedConverter(converter));

    /// <summary>
    ///     Clones the type mapping to update facets from the mapping info, if needed.
    /// </summary>
    /// <param name="mappingInfo">The mapping info containing the facets to use.</param>
    /// <returns>The cloned mapping, or the original mapping if no clone was needed.</returns>
    public virtual RelationalTypeMapping Clone(in RelationalTypeMappingInfo mappingInfo)
        => Clone(Parameters.WithTypeMappingInfo(mappingInfo));

    /// <summary>
    ///     Processes the store type name to add appropriate postfix/prefix text as needed.
    /// </summary>
    /// <param name="parameters">The parameters for this mapping.</param>
    /// <param name="storeType">The specified store type name.</param>
    /// <param name="storeTypeNameBase">The calculated based name</param>
    /// <returns>The store type name to use.</returns>
    protected virtual string ProcessStoreType(
        RelationalTypeMappingParameters parameters,
        string storeType,
        string storeTypeNameBase)
    {
        var size = parameters.Size;

        if (size != null
            && parameters.StoreTypePostfix == StoreTypePostfix.Size)
        {
            storeType = storeTypeNameBase + "(" + size + ")";
        }
        else if (parameters.StoreTypePostfix == StoreTypePostfix.PrecisionAndScale
                 || parameters.StoreTypePostfix == StoreTypePostfix.Precision)
        {
            var precision = parameters.Precision;
            if (precision != null)
            {
                var scale = parameters.Scale;
                storeType = storeTypeNameBase
                    + "("
                    + (scale == null || parameters.StoreTypePostfix == StoreTypePostfix.Precision
                        ? precision.ToString()
                        : precision + "," + scale)
                    + ")";
            }
        }

        return storeType;
    }

    /// <summary>
    ///     Creates a <see cref="DbParameter" /> with the appropriate type information configured.
    /// </summary>
    /// <param name="command">The command the parameter should be created on.</param>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="value">The value to be assigned to the parameter.</param>
    /// <param name="nullable">A value indicating whether the parameter should be a nullable type.</param>
    /// <returns>The newly created parameter.</returns>
    public virtual DbParameter CreateParameter(
        DbCommand command,
        string name,
        object? value,
        bool? nullable = null)
    {
        var parameter = command.CreateParameter();
        parameter.Direction = ParameterDirection.Input;
        parameter.ParameterName = name;

        value = NormalizeEnumValue(value);

        if (Converter != null)
        {
            value = Converter.ConvertToProvider(value);
        }

        parameter.Value = value ?? DBNull.Value;

        if (nullable.HasValue)
        {
            Check.DebugAssert(nullable.Value || value != null, "Null value in a non-nullable parameter");

            parameter.IsNullable = nullable.Value;
        }

        if (DbType.HasValue)
        {
            parameter.DbType = DbType.Value;
        }

        ConfigureParameter(parameter);

        return parameter;
    }

    private object? NormalizeEnumValue(object? value)
    {
        // When Enum column is compared to constant the C# compiler put a constant of integer there
        // In some unknown cases for parameter we also see integer value.
        // So if CLR type is enum we need to convert integer value to enum value
        if (value?.GetType().IsInteger() == true
            && ClrType.UnwrapNullableType().IsEnum)
        {
            return Enum.ToObject(ClrType.UnwrapNullableType(), value);
        }

        // When Enum is cast manually our logic of removing implicit convert gives us enum value here
        // So if CLR type is integer we need to convert enum value to integer value
        if (value?.GetType().IsEnum == true
            && ClrType.UnwrapNullableType().IsInteger())
        {
            return Convert.ChangeType(value, ClrType);
        }

        return value;
    }

    /// <summary>
    ///     Configures type information of a <see cref="DbParameter" />.
    /// </summary>
    /// <param name="parameter">The parameter to be configured.</param>
    protected virtual void ConfigureParameter(DbParameter parameter)
    {
    }

    /// <summary>
    ///     Generates the SQL representation of a literal value.
    /// </summary>
    /// <param name="value">The literal value.</param>
    /// <returns>
    ///     The generated string.
    /// </returns>
    public virtual string GenerateSqlLiteral(object? value)
    {
        value = NormalizeEnumValue(value);

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
    public virtual string GenerateProviderValueSqlLiteral(object? value)
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
    protected virtual string GenerateNonNullSqlLiteral(object value)
        => string.Format(CultureInfo.InvariantCulture, SqlLiteralFormatString, value);

    /// <summary>
    ///     The method to use when reading values of the given type. The method must be defined
    ///     on <see cref="DbDataReader" /> or one of its subclasses.
    /// </summary>
    /// <returns>The method to use to read the value.</returns>
    public virtual MethodInfo GetDataReaderMethod()
    {
        var type = (Converter?.ProviderClrType ?? ClrType).UnwrapNullableType();

        return GetDataReaderMethod(type);
    }

    /// <summary>
    ///     The method to use when reading values of the given type. The method must be defined
    ///     on <see cref="DbDataReader" />.
    /// </summary>
    /// <returns>The method to use to read the value.</returns>
    public static MethodInfo GetDataReaderMethod(Type type)
        => GetXMethods.TryGetValue(type, out var method)
            ? method
            : GetFieldValueMethod.MakeGenericMethod(type);

    /// <summary>
    ///     Gets a custom expression tree for reading the value from the input data reader
    ///     expression that contains the database value.
    /// </summary>
    /// <param name="expression">The input expression, containing the database value.</param>
    /// <returns>The expression with customization added.</returns>
    public virtual Expression CustomizeDataReaderExpression(Expression expression)
        => expression;
}

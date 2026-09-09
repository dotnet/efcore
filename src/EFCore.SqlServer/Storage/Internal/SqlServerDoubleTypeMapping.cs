// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerDoubleTypeMapping : DoubleTypeMapping
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static new SqlServerDoubleTypeMapping Default { get; } = new("float");

    private static readonly MethodInfo GetFloatMethod
        = typeof(DbDataReader).GetRuntimeMethod(nameof(DbDataReader.GetFloat), [typeof(int)])!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerDoubleTypeMapping(
        string storeType,
        DbType? dbType = System.Data.DbType.Double,
        StoreTypePostfix storeTypePostfix = StoreTypePostfix.Precision)
        : base(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(typeof(double), jsonValueReaderWriter: JsonDoubleReaderWriter.Instance),
                storeType,
                storeTypePostfix,
                dbType))
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected SqlServerDoubleTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    /// <summary>
    ///     Creates a copy of this mapping.
    /// </summary>
    /// <param name="parameters">The parameters for this mapping.</param>
    /// <returns>The newly created mapping.</returns>
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new SqlServerDoubleTypeMapping(parameters);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string GenerateNonNullSqlLiteral(object value)
    {
        var literal = base.GenerateNonNullSqlLiteral(value);

        var doubleValue = Convert.ToDouble(value);
        return !literal.Contains('E')
            && !literal.Contains('e')
            && !double.IsNaN(doubleValue)
            && !double.IsInfinity(doubleValue)
                ? literal + "E0"
                : literal;
    }

    /// <summary>
    ///     The method to use when reading values of the given type. The method must be defined
    ///     on <see cref="DbDataReader" /> or one of its subclasses.
    /// </summary>
    /// <returns>The method to use to read the value.</returns>
    public override MethodInfo GetDataReaderMethod()
        => Precision is <= 24 ? GetFloatMethod : base.GetDataReaderMethod();

    /// <summary>
    ///     Gets a custom expression tree for reading the value from the input data reader
    ///     expression that contains the database value.
    /// </summary>
    /// <param name="expression">The input expression, containing the database value.</param>
    /// <returns>The expression with customization added.</returns>
    public override Expression CustomizeDataReaderExpression(Expression expression)
    {
        if (Precision is <= 24)
        {
            expression = Expression.Convert(expression, typeof(double));
        }

        return base.CustomizeDataReaderExpression(expression);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void ConfigureParameter(DbParameter parameter)
    {
        base.ConfigureParameter(parameter);

        if (Precision.HasValue
            && Precision.Value != -1)
        {
            // SqlClient wants this set as "size"
            parameter.Size = Precision.Value;
        }
    }
}

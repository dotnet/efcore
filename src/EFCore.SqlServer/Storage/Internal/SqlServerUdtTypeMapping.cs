// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;

namespace Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerUdtTypeMapping : RelationalTypeMapping
{
    private static Action<DbParameter, string>? _udtTypeNameSetter;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerUdtTypeMapping(
        Type clrType,
        string storeType,
        Func<object, Expression> literalGenerator,
        StoreTypePostfix storeTypePostfix = StoreTypePostfix.None,
        string? udtTypeName = null,
        ValueConverter? converter = null,
        ValueComparer? comparer = null,
        ValueComparer? keyComparer = null,
        DbType? dbType = null,
        bool unicode = false,
        int? size = null,
        bool fixedLength = false,
        int? precision = null,
        int? scale = null)
        : base(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(
                    clrType, converter, comparer, keyComparer), storeType, storeTypePostfix, dbType, unicode, size, fixedLength,
                precision, scale))

    {
        LiteralGenerator = literalGenerator;
        UdtTypeName = udtTypeName ?? storeType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected SqlServerUdtTypeMapping(
        RelationalTypeMappingParameters parameters,
        Func<object, Expression> literalGenerator,
        string? udtTypeName)
        : base(parameters)
    {
        LiteralGenerator = literalGenerator;
        UdtTypeName = udtTypeName ?? parameters.StoreType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string UdtTypeName { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Func<object, Expression> LiteralGenerator { get; }

    /// <summary>
    ///     Creates a copy of this mapping.
    /// </summary>
    /// <param name="parameters">The parameters for this mapping.</param>
    /// <returns>The newly created mapping.</returns>
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new SqlServerUdtTypeMapping(parameters, LiteralGenerator, UdtTypeName);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void ConfigureParameter(DbParameter parameter)
        => SetUdtTypeName(parameter);

    private void SetUdtTypeName(DbParameter parameter)
    {
        LazyInitializer.EnsureInitialized(
            ref _udtTypeNameSetter,
            () => CreateUdtTypeNameAccessor(parameter.GetType()));

        if (parameter.Value != null
            && parameter.Value != DBNull.Value)
        {
            _udtTypeNameSetter(parameter, UdtTypeName);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression GenerateCodeLiteral(object value)
        => LiteralGenerator(value);

    private static Action<DbParameter, string> CreateUdtTypeNameAccessor(Type paramType)
    {
        var paramParam = Expression.Parameter(typeof(DbParameter), "parameter");
        var valueParam = Expression.Parameter(typeof(string), "value");

        return Expression.Lambda<Action<DbParameter, string>>(
            Expression.Call(
                Expression.Convert(paramParam, paramType),
                paramType.GetProperty("UdtTypeName")!.SetMethod!,
                valueParam),
            paramParam,
            valueParam).Compile();
    }
}

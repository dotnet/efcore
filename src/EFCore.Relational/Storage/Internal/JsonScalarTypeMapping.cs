// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;

namespace Microsoft.EntityFrameworkCore.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class JsonScalarTypeMapping : RelationalTypeMapping
{
    private readonly IRelationalTypeMappingSource _typeMappingSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public JsonScalarTypeMapping(IRelationalTypeMappingSource typeMappingSource)
        : base("json-scalar", typeof(object))
        => _typeMappingSource = typeMappingSource;

    /// <inheritdoc />
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => this;

    internal static RelationalTypeMapping GetMapping(IRelationalTypeMappingSource typeMappingSource, object? value)
        // JSON numbers are not limited to a relational column's default precision and scale.
        => typeMappingSource.GetMapping(value?.GetType() ?? typeof(string)).WithPrecisionAndScale(null, null);

    /// <inheritdoc />
    public override DbParameter CreateParameter(
        DbCommand command,
        string name,
        object? value,
        bool? nullable = null,
        ParameterDirection direction = ParameterDirection.Input)
        // The serialized token's CLR type is only known at execution time. Use native parameter configuration
        // without applying the property's converter or document reader/writer a second time.
        => GetMapping(_typeMappingSource, value)
            .CreateParameter(command, name, value, nullable, direction);
}

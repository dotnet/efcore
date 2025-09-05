// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.SqlServer.Update.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerModificationCommand : ModificationCommand
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerModificationCommand(in ModificationCommandParameters modificationCommandParameters)
        : base(modificationCommandParameters)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerModificationCommand(in NonTrackedModificationCommandParameters modificationCommandParameters)
        : base(modificationCommandParameters)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void ProcessSinglePropertyJsonUpdate(ref ColumnModificationParameters parameters)
    {
        // TODO: Move more of this logic to the type mapping. Issue #34432
        var property = parameters.Property!;
        var mapping = property.GetRelationalTypeMapping();
        var propertyProviderClrType = (mapping.Converter?.ProviderClrType ?? property.ClrType).UnwrapNullableType();
        var value = parameters.Value;

        // JSON-compatible non-string values (bool, numeric, null) are sent directly as non-string parameters.
        if (value is null
            || ((propertyProviderClrType == typeof(bool)
                || propertyProviderClrType.IsNumeric())
                && !property.IsPrimitiveCollection))
        {
            parameters = parameters with { Value = value, TypeMapping = mapping };

            return;
        }

        var jsonValueReaderWriter = mapping.JsonValueReaderWriter;
        if (jsonValueReaderWriter != null)
        {
            if (property.IsPrimitiveCollection)
            {
                // This is a JSON array, so send with the original type mapping, which may indicate the column type is JSON.
                parameters = parameters with { Value = jsonValueReaderWriter.ToJsonString(value) };

                return;
            }

            // Otherwise, wrap the value in a simple JSON object to avoid double escaping.
            parameters = parameters with
            {
                Value = jsonValueReaderWriter.ToJsonObjectString("", value),
                TypeMapping = parameters.TypeMapping is SqlServerStructuralJsonTypeMapping
                    ? parameters.TypeMapping
                    : SqlServerStructuralJsonTypeMapping.Default
            };

            return;
        }
        else if (mapping.Converter != null)
        {
            value = mapping.Converter.ConvertToProvider(value);
        }

        parameters = parameters with
        {
            Value = value,
            TypeMapping = parameters.TypeMapping is SqlServerStructuralJsonTypeMapping
                ? SqlServerStringTypeMapping.UnicodeDefault
                : parameters.TypeMapping
        };
    }
}

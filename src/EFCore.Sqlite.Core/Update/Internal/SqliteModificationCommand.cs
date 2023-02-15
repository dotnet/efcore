// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json.Nodes;

namespace Microsoft.EntityFrameworkCore.Sqlite.Update.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqliteModificationCommand : ModificationCommand
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqliteModificationCommand(in ModificationCommandParameters modificationCommandParameters)
        : base(modificationCommandParameters)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqliteModificationCommand(in NonTrackedModificationCommandParameters modificationCommandParameters)
        : base(modificationCommandParameters)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override JsonNode? GenerateJsonForSinglePropertyUpdate(IProperty property, object? propertyValue)
    {
        if (propertyValue is bool boolPropertyValue
            && (property.GetTypeMapping().Converter?.ProviderClrType ?? property.ClrType).UnwrapNullableType() == typeof(bool))
        {
            // Sqlite converts true/false into native 0/1 when using json_extract
            // so we convert those values to strings so that they stay as true/false
            // which is what we want to store in json object in the end
            var modifiedPropertyValue = boolPropertyValue
                ? "true"
                : "false";

#pragma warning disable EF1001 // Internal EF Core API usage.
            return base.GenerateJsonForSinglePropertyUpdate(property, modifiedPropertyValue);
#pragma warning restore EF1001 // Internal EF Core API usage.
        }

#pragma warning disable EF1001 // Internal EF Core API usage.
        return base.GenerateJsonForSinglePropertyUpdate(property, propertyValue);
#pragma warning restore EF1001 // Internal EF Core API usage.
    }
}

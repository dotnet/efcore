// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json.Nodes;

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
    protected override object? GenerateValueForSinglePropertyUpdate(IProperty property, object? propertyValue)
    {
        var propertyProviderClrType = (property.GetTypeMapping().Converter?.ProviderClrType ?? property.ClrType).UnwrapNullableType();

        // when we generate SqlParameter when updating single property in JSON entity
        // we always use SqlServerJsonTypeMapping as type mapping for the parameter
        // (since we don't have dedicated type mapping for individual JSON properties)
        // later, when we generate DbParameter we assign the value and then the DbType from the type mapping.
        // in case of byte value, when we assign the value to the DbParameter it sets its type to Byte and its size to 1
        // then, we change DbType to String, but keep size as is
        // so, if value was, say, 15 we initially generate DbParameter of type Byte, value 25 and size 1
        // but when we change the type we end up with type String, value 25 and size 1, which effectively is "2"
        // to mitigate this, we convert the value to string, to guarantee the correct parameter size.
        // this can be removed when we have dedicated JSON type mapping for individual (leaf) properties
        if (propertyProviderClrType == typeof(byte))
        {
            return JsonValue.Create(propertyValue)?.ToJsonString().Replace("\"", "");
        }

#pragma warning disable EF1001 // Internal EF Core API usage.
        return base.GenerateValueForSinglePropertyUpdate(property, propertyValue);
#pragma warning restore EF1001 // Internal EF Core API usage.
    }
}

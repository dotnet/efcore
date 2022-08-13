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
public class RawRelationalParameter : RelationalParameterBase
{
    private readonly DbParameter _parameter;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public RawRelationalParameter(
        string invariantName,
        DbParameter parameter)
        : base(invariantName)
    {
        _parameter = parameter;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override void AddDbParameter(DbCommand command, IReadOnlyDictionary<string, object?>? parameterValues)
        => AddDbParameter(command, _parameter);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override void AddDbParameter(DbCommand command, object? value)
    {
        Check.DebugAssert(
            value is DbParameter,
            $"{nameof(value)} isn't a DbParameter in {nameof(RawRelationalParameter)}.{nameof(AddDbParameter)}");

        if (value is DbParameter dbParameter)
        {
            if (command.Parameters.Contains(dbParameter.ParameterName))
            {
                return;
            }

            if (dbParameter.Direction == ParameterDirection.Input
                && value is ICloneable cloneable)
            {
                value = cloneable.Clone();
            }
        }

        command.Parameters.Add(value);
    }
}

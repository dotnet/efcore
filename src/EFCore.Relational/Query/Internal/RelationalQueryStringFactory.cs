// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using Microsoft.EntityFrameworkCore.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class RelationalQueryStringFactory : IRelationalQueryStringFactory
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Create(DbCommand command)
    {
        if (command.Parameters.Count == 0)
        {
            return command.CommandText;
        }

        var builder = new StringBuilder();
        foreach (DbParameter parameter in command.Parameters)
        {
            builder.Append("-- ").AppendLine(parameter.FormatParameter(logParameterValues: true));
        }

        return builder.Append(command.CommandText).ToString();
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqliteQueryStringFactory : IRelationalQueryStringFactory
{
    private readonly IRelationalTypeMappingSource _typeMapper;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqliteQueryStringFactory(IRelationalTypeMappingSource typeMapper)
    {
        _typeMapper = typeMapper;
    }

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
            var value = parameter.Value;
            builder
                .Append(".param set ")
                .Append(parameter.ParameterName)
                .Append(' ')
                .AppendLine(
                    value == null || value == DBNull.Value
                        ? "NULL"
                        : _typeMapper.FindMapping(value.GetType())?.GenerateSqlLiteral(value)
                        ?? value.ToString());
        }

        return builder
            .AppendLine()
            .Append(command.CommandText).ToString();
    }
}

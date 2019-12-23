// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal
{
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
        public SqliteQueryStringFactory([NotNull] IRelationalTypeMappingSource typeMapper)
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
}

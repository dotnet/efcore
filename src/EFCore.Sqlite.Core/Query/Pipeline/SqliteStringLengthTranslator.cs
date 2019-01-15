// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Pipeline
{
    public class SqliteStringLengthTranslator : IMemberTranslator
    {
        private readonly IRelationalTypeMappingSource _typeMappingSource;

        public SqliteStringLengthTranslator(IRelationalTypeMappingSource typeMappingSource)
        {
            _typeMappingSource = typeMappingSource;
        }

        public SqlExpression Translate(SqlExpression instance, MemberInfo member, Type returnType)
        {
            return instance.Type == typeof(string)
                   && member.Name == nameof(string.Length)
                   ? new SqlFunctionExpression(
                       "length",
                       new[] { instance },
                       returnType,
                       _typeMappingSource.FindMapping(returnType),
                       false)
                    : null;
        }
    }
}

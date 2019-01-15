// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Pipeline
{
    public class SqlServerStringMemberTranslator : IMemberTranslator
    {
        private readonly IRelationalTypeMappingSource _typeMappingSource;

        public SqlServerStringMemberTranslator(IRelationalTypeMappingSource typeMappingSource)
        {
            _typeMappingSource = typeMappingSource;
        }

        public SqlExpression Translate(SqlExpression instance, MemberInfo member, Type returnType)
        {
            if (member.Name == nameof(string.Length)
                && instance?.Type == typeof(string))
            {
                return new SqlCastExpression(
                    new SqlFunctionExpression(
                        "LEN",
                        new[] { instance },
                        typeof(long),
                        _typeMappingSource.FindMapping(typeof(long)),
                        false),
                    returnType,
                    _typeMappingSource.FindMapping(returnType));
            }

            return null;
        }
    }
}

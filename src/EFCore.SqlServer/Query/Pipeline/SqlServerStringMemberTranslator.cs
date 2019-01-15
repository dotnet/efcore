// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Pipeline
{
    public class SqlServerStringMemberTranslator : IMemberTranslator
    {
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public SqlServerStringMemberTranslator(ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public SqlExpression Translate(SqlExpression instance, MemberInfo member, Type returnType)
        {
            if (member.Name == nameof(string.Length)
                && instance?.Type == typeof(string))
            {
                return _sqlExpressionFactory.Convert(
                    _sqlExpressionFactory.Function("LEN", new[] { instance }, typeof(long)),
                    returnType);
            }

            return null;
        }
    }
}

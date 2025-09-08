// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.Internal
{
    public class XGStringMemberTranslator : IMemberTranslator
    {
        private readonly XGSqlExpressionFactory _sqlExpressionFactory;

        public XGStringMemberTranslator(XGSqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public virtual SqlExpression Translate(
            SqlExpression instance,
            MemberInfo member,
            Type returnType,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            if (member.Name == nameof(string.Length)
                && member.DeclaringType == typeof(string))
            {
                return _sqlExpressionFactory.NullableFunction(
                    "CHAR_LENGTH",
                    new[] { instance },
                    returnType);
            }

            return null;
        }
    }
}

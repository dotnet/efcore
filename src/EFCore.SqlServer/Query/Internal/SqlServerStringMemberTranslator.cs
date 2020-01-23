// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal
{
    public class SqlServerStringMemberTranslator : IMemberTranslator
    {
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public SqlServerStringMemberTranslator([NotNull] ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public virtual SqlExpression Translate(SqlExpression instance, MemberInfo member, Type returnType)
        {
            Check.NotNull(member, nameof(member));
            Check.NotNull(returnType, nameof(returnType));

            if (member.Name == nameof(string.Length)
                && instance?.Type == typeof(string))
            {
                return _sqlExpressionFactory.Convert(
                    _sqlExpressionFactory.Function(
                        "LEN",
                        new[] { instance },
                        nullResultAllowed: true,
                        argumentsPropagateNullability: new[] { true },
                        typeof(long)),
                    returnType);
            }

            return null;
        }
    }
}

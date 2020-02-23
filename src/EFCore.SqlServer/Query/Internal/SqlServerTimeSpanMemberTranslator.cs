// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal
{
    public class SqlServerTimeSpanMemberTranslator : IMemberTranslator
    {
        private static readonly Dictionary<string, string> _datePartMappings = new Dictionary<string, string>
            {
                { nameof(TimeSpan.Hours), "hour" },
                { nameof(TimeSpan.Minutes), "minute" },
                { nameof(TimeSpan.Seconds), "second" },
                { nameof(TimeSpan.Milliseconds), "millisecond" }
            };

        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public SqlServerTimeSpanMemberTranslator([NotNull] ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public virtual SqlExpression Translate(SqlExpression instance, MemberInfo member, Type returnType)
        {
            Check.NotNull(member, nameof(member));
            Check.NotNull(returnType, nameof(returnType));

            if (member.DeclaringType == typeof(TimeSpan) && _datePartMappings.TryGetValue(member.Name, out string value))
            {
                return _sqlExpressionFactory.Function("DATEPART", new []
                    {
                        _sqlExpressionFactory.Fragment(value),
                        instance
                    },
                    nullable: true,
                    argumentsPropagateNullability: new [] { false, true },
                    returnType);
            }

            return null;
        }
    }
}

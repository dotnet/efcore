// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Pipeline
{
    public class SqlServerDateTimeMemberTranslator : IMemberTranslator
    {
        private readonly IRelationalTypeMappingSource _typeMappingSource;
        private static readonly Dictionary<string, string> _datePartMapping
            = new Dictionary<string, string>
            {
                { nameof(DateTime.Year), "year" },
                { nameof(DateTime.Month), "month" },
                { nameof(DateTime.DayOfYear), "dayofyear" },
                { nameof(DateTime.Day), "day" },
                { nameof(DateTime.Hour), "hour" },
                { nameof(DateTime.Minute), "minute" },
                { nameof(DateTime.Second), "second" },
                { nameof(DateTime.Millisecond), "millisecond" }
            };

        public SqlServerDateTimeMemberTranslator(IRelationalTypeMappingSource typeMappingSource)
        {
            _typeMappingSource = typeMappingSource;
        }

        public SqlExpression Translate(SqlExpression instance, MemberInfo member, Type returnType)
        {
            var declaringType = member.DeclaringType;

            if (declaringType == typeof(DateTime)
                || declaringType == typeof(DateTimeOffset))
            {
                var memberName = member.Name;

                if (_datePartMapping.TryGetValue(memberName, out var datePart))
                {
                    return new SqlFunctionExpression(
                        "DATEPART",
                        new[]
                        {
                            new SqlFragmentExpression(datePart),
                            instance
                        },
                        returnType,
                        _typeMappingSource.FindMapping(typeof(int)),
                        false);
                }

                switch (memberName)
                {
                    case nameof(DateTime.Date):
                        return new SqlFunctionExpression(
                        "CONVERT",
                        new[]{
                                new SqlFragmentExpression("date"),
                                instance
                        },
                        returnType,
                        instance.TypeMapping,
                        false);

                    case nameof(DateTime.TimeOfDay):
                        return new SqlCastExpression(
                            instance,
                            returnType,
                            null);

                    case nameof(DateTime.Now):
                        return new SqlFunctionExpression(
                            declaringType == typeof(DateTime) ? "GETDATE" : "SYSDATETIMEOFFSET",
                            null,
                            returnType,
                            _typeMappingSource.FindMapping(returnType),
                            false);

                    case nameof(DateTime.UtcNow):
                        var serverTranslation = new SqlFunctionExpression(
                            declaringType == typeof(DateTime) ? "GETUTCDATE" : "SYSUTCDATETIME",
                            null,
                            returnType,
                            _typeMappingSource.FindMapping(returnType),
                            false);

                        return declaringType == typeof(DateTime)
                            ? (SqlExpression)serverTranslation
                            : new SqlCastExpression(
                                serverTranslation,
                                returnType,
                                serverTranslation.TypeMapping);

                    case nameof(DateTime.Today):
                        return new SqlFunctionExpression(
                            "CONVERT",
                            new SqlExpression[]
                            {
                                new SqlFragmentExpression("date"),
                                new SqlFunctionExpression(
                                    "GETDATE",
                                    null,
                                    typeof(DateTime),
                                    _typeMappingSource.FindMapping(typeof(DateTime)),
                                    false)
                            },
                            returnType,
                            _typeMappingSource.FindMapping(returnType),
                            false);
                }
            }

            return null;
        }
    }
}

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

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Pipeline
{
    public class SqliteDateTimeMemberTranslator : IMemberTranslator
    {
        private static readonly Dictionary<string, string> _datePartMapping
            = new Dictionary<string, string>
            {
                { nameof(DateTime.Year), "%Y" },
                { nameof(DateTime.Month), "%m" },
                { nameof(DateTime.DayOfYear), "%j" },
                { nameof(DateTime.Day), "%d" },
                { nameof(DateTime.Hour), "%H" },
                { nameof(DateTime.Minute), "%M" },
                { nameof(DateTime.Second), "%S" },
                { nameof(DateTime.DayOfWeek), "%w" }
            };

        private readonly IRelationalTypeMappingSource _typeMappingSource;
        private readonly RelationalTypeMapping _stringTypeMapping;
        private readonly RelationalTypeMapping _intTypeMapping;
        private readonly RelationalTypeMapping _doubleTypeMapping;

        public SqliteDateTimeMemberTranslator(IRelationalTypeMappingSource typeMappingSource)
        {
            _typeMappingSource = typeMappingSource;
            _stringTypeMapping = typeMappingSource.FindMapping(typeof(string));
            _intTypeMapping = typeMappingSource.FindMapping(typeof(int));
            _doubleTypeMapping = typeMappingSource.FindMapping(typeof(double));
        }

        public SqlExpression Translate(SqlExpression instance, MemberInfo member, Type returnType)
        {
            if (member.DeclaringType == typeof(DateTime))
            {
                var memberName = member.Name;

                if (_datePartMapping.TryGetValue(memberName, out var datePart))
                {
                    return new SqlCastExpression(
                        SqliteExpression.Strftime(
                            typeof(string),
                            _stringTypeMapping,
                            CreateStringConstant(datePart),
                            instance),
                        returnType,
                        _typeMappingSource.FindMapping(returnType));
                }

                if (string.Equals(memberName, nameof(DateTime.Ticks)))
                {
                    return new SqlCastExpression(
                        new SqlBinaryExpression(
                            ExpressionType.Multiply,
                            new SqlBinaryExpression(
                                ExpressionType.Subtract,
                                new SqlFunctionExpression(
                                    "julianday",
                                    new[] { instance },
                                    typeof(double),
                                    _doubleTypeMapping,
                                    false),
                                new SqlConstantExpression(
                                    Expression.Constant(1721425.5), // NB: Result of julianday('0001-01-01 00:00:00')
                                    _doubleTypeMapping),
                                typeof(double),
                                _doubleTypeMapping),
                            new SqlConstantExpression(
                                Expression.Constant(TimeSpan.TicksPerDay),
                                _typeMappingSource.FindMapping(typeof(long))),
                            typeof(double),
                            _doubleTypeMapping),
                        typeof(long),
                        _typeMappingSource.FindMapping(typeof(long)));
                }

                if (string.Equals(memberName, nameof(DateTime.Millisecond)))
                {
                    return new SqlBinaryExpression(
                        ExpressionType.Modulo,
                        new SqlBinaryExpression(
                            ExpressionType.Multiply,
                            new SqlCastExpression(
                                SqliteExpression.Strftime(
                                    typeof(string),
                                    _stringTypeMapping,
                                    CreateStringConstant("%f"),
                                    instance),
                                typeof(double),
                                _doubleTypeMapping),
                            new SqlConstantExpression(Expression.Constant(1000), _intTypeMapping),
                            typeof(double),
                            _doubleTypeMapping),
                        new SqlConstantExpression(Expression.Constant(1000), _intTypeMapping),
                        typeof(int),
                        _intTypeMapping);
                }

                var format = "%Y-%m-%d %H:%M:%f";
                SqlExpression timestring;
                var modifiers = new List<SqlExpression>();

                switch (memberName)
                {
                    case nameof(DateTime.Now):
                        timestring = CreateStringConstant("now");
                        modifiers.Add(CreateStringConstant("localtime"));
                        break;

                    case nameof(DateTime.UtcNow):
                        timestring = CreateStringConstant("now");
                        break;

                    case nameof(DateTime.Date):
                        timestring = instance;
                        modifiers.Add(CreateStringConstant("start of day"));
                        break;

                    case nameof(DateTime.Today):
                        timestring = CreateStringConstant("now");
                        modifiers.Add(CreateStringConstant("localtime"));
                        modifiers.Add(CreateStringConstant("start of day"));
                        break;

                    case nameof(DateTime.TimeOfDay):
                        format = "%H:%M:%f";
                        timestring = instance;
                        break;

                    default:
                        return null;
                }

                Debug.Assert(timestring != null);

                var typeMapping = _typeMappingSource.FindMapping(returnType);

                return new SqlFunctionExpression(
                    "rtrim",
                    new SqlExpression[]
                    {
                        new SqlFunctionExpression(
                            "rtrim",
                            new SqlExpression[]
                            {
                                SqliteExpression.Strftime(
                                    returnType,
                                    typeMapping,
                                    CreateStringConstant(format),
                                    timestring,
                                    modifiers),
                                CreateStringConstant("0")
                            },
                            returnType,
                            typeMapping,
                            false),
                        CreateStringConstant(".")
                    },
                    returnType,
                    typeMapping,
                    false);
            }

            return null;
        }

        private SqlConstantExpression CreateStringConstant(string value)
        {
            return new SqlConstantExpression(Expression.Constant(value), _stringTypeMapping);
        }
    }
}

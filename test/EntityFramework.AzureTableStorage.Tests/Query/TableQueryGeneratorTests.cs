// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.Data.Entity.AzureTableStorage.Query;
using Microsoft.Data.Entity.AzureTableStorage.Query.Expressions;
using Microsoft.Data.Entity.AzureTableStorage.Tests.Helpers;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests.Query
{
    public class TableQueryGeneratorTests
    {
        private readonly TableQueryGenerator _generator = new TableQueryGenerator();

        [Fact]
        public void Generates_take_value()
        {
            var count = 38974;
            var expression = new SelectExpression(typeof(PocoTestType))
                {
                    Take = new TakeExpression(typeof(PocoTestType), count)
                };
            Assert.Equal(count, _generator.GenerateTableQuery(expression).TakeCount);
        }

        [Fact]
        public void Generates_ignores_missing_take_value()
        {
            var expression = new SelectExpression(typeof(PocoTestType));
            Assert.Null(_generator.GenerateTableQuery(expression).TakeCount);
        }

        [Theory]
        [MemberData("ComparisonTypeExpressions")]
        [MemberData("DataTypeWhereExpressions")]
        [MemberData("ComplexLogicExpression")]
        public void Generates_filter_string(string expected, Expression expression)
        {
            var select = new SelectExpression(typeof(PocoTestType));
            select.Predicate = expression;

            Assert.Equal(expected, _generator.GenerateTableQuery(select).FilterString);
        }

        public static readonly DateTime ConstantDateTime = new DateTime(2014, 5, 23, 18, 0, 0, DateTimeKind.Utc);
        public static readonly byte[] TestByteArray = { 5, 8, 15, 16, 23, 42 };

        public static object[][] DataTypeWhereExpressions
        {
            get
            {
                return new[]
                    {
                        new[] { "Count eq 15", CreateFilterExpr("Count", 15) },
                        new[] { "Name eq 'Unicorn'", CreateFilterExpr("Name", "Unicorn") },
                        new[] { "Name eq ' has '' quotes '' \" '", CreateFilterExpr("Name", " has ' quotes ' \" ") },
                        new[] { "Name eq 'rtl أنا لا أتكلم العربية rtl'", CreateFilterExpr("Name", "rtl أنا لا أتكلم العربية rtl") },
                        new[] { "Name eq '獨角獸'", CreateFilterExpr("Name", "獨角獸") },
                        new[] { "Count eq 2147483647", CreateFilterExpr("Count", 2147483647) },
                        new[] { "BigCount eq 2147483648L", CreateFilterExpr("BigCount", 2147483648) },
                        new[] { "Price eq 100.75", CreateFilterExpr("Price", 100.75) },
                        new[] { "CustomerSince eq datetime'2014-05-23T18:00:00.0000000Z'", CreateFilterExpr("CustomerSince", ConstantDateTime) },
                        new[] { "IsEnchanted eq true", CreateFilterExpr("IsEnchanted", true) },
                        new[] { "IsEnchanted eq false", CreateFilterExpr("IsEnchanted", false) },
                        new[] { "Buffer eq X'05080f10172a'", CreateFilterExpr("Buffer", TestByteArray) },
                        new[] { "Guid eq guid'ca761232-ed42-11ce-bacd-00aa0057b223'", CreateFilterExpr("Guid", new Guid("ca761232ed4211cebacd00aa0057b223")) }
                    };
            }
        }

        public static object[][] ComparisonTypeExpressions
        {
            get
            {
                return new[]
                    {
                        new[] { "Count eq 5", CreateFilterExpr("Count", 5) },
                        new[] { "Count ne 10", CreateFilterExpr("Count", 10, ExpressionType.NotEqual) },
                        new[] { "Count lt 15", CreateFilterExpr("Count", 15, ExpressionType.LessThan) },
                        new[] { "Count le 20", CreateFilterExpr("Count", 20, ExpressionType.LessThanOrEqual) },
                        new[] { "Count gt 25", CreateFilterExpr("Count", 25, ExpressionType.GreaterThan) },
                        new[] { "Count ge 30", CreateFilterExpr("Count", 30, ExpressionType.GreaterThanOrEqual) }
                    };
            }
        }

        public static object[][] ComplexLogicExpression
        {
            get
            {
                var expr = (Expression)CreateFilterExpr("Count", 15);
                var data = new object[5][];

                data[0] = new object[]
                    {
                        "(Count eq 15) and (Count eq 15)",
                        Expression.MakeBinary(
                            ExpressionType.AndAlso, expr, expr)
                    };
                data[1] = new object[]
                    {
                        "((Count eq 15) and (Count eq 15)) and ((Count eq 15) and (Count eq 15))",
                        Expression.MakeBinary(
                            ExpressionType.AndAlso,
                            Expression.MakeBinary(
                                ExpressionType.AndAlso, expr, expr),
                            Expression.MakeBinary(
                                ExpressionType.AndAlso, expr, expr))
                    };
                data[2] = new object[]
                    {
                        "(Count eq 15) or ((Count eq 15) and (Count eq 15))",
                        Expression.MakeBinary(
                            ExpressionType.OrElse,
                            expr,
                            Expression.MakeBinary(
                                ExpressionType.AndAlso, expr, expr))
                    };
                data[3] = new object[]
                    {
                        "(Count eq 15) or ((Count eq 15) and (Count eq 15))",
                        Expression.MakeBinary(
                            ExpressionType.OrElse,
                            expr,
                            Expression.MakeBinary(
                                ExpressionType.AndAlso, expr, expr))
                    };
                data[4] = new object[]
                    {
                        "(Count eq 15) and ((Count eq 15) or (Count eq 15))",
                        Expression.MakeBinary(
                            ExpressionType.AndAlso,
                            expr,
                            Expression.MakeBinary(
                                ExpressionType.OrElse, expr, expr))
                    };
                return data;
            }
        }

        private static object CreateFilterExpr(string propertyName, object constant, ExpressionType expressionType = ExpressionType.Equal)
        {
            return Expression.MakeBinary(
                expressionType,
                new PropertyExpression(new Property(propertyName, constant.GetType(), new EntityType(typeof(PocoTestType)))),
                new QueryableConstantExpression(constant.GetType(), constant));
        }
    }
}

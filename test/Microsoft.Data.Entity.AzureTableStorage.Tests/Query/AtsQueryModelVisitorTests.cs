// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.Data.Entity.AzureTableStorage.Query;
using Microsoft.Data.Entity.AzureTableStorage.Tests.Helpers;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests.Query
{
    public class AtsQueryModelVisitorTests : AtsQueryModelVisitor, IClassFixture<TestModelFixture>
    {
        private Model _model;
        private readonly TestModelFixture _fixture;

        public AtsQueryModelVisitorTests(TestModelFixture fixture)
            : base(new AtsQueryCompilationContext(SetupModel(fixture), new TableFilterFactory()))
        {
            _fixture = fixture;
        }

        private static IModel SetupModel(TestModelFixture fixture)
        {
            var model = fixture.CreateTestModel("TestModel");
            model.AddEntityType(PocoTestType.EntityType());
            return model;
        }

        private static Expression MakePredicate<TSource>(Expression<Func<TSource, bool>> wherePredicate)
        {
            return Expression.Quote(wherePredicate);
        }

        [Fact]
        public void It_creates_table_query()
        {
            var querySource = _fixture.CreateWithEntityQueryable<PocoTestType>();
            var visitor = CreateQueryingExpressionTreeVisitor(querySource);
            AtsTableQuery query;

            visitor.VisitExpression(querySource.FromExpression);

            Assert.True(TryGetTableQuery(querySource, out query));
            Assert.IsType<AtsTableQuery>(query);
            Assert.NotNull(query);
        }

        public static IEnumerable<object[]> SimpleWhereExpressions
        {
            get
            {
                return new[]
                    {
                        new object[] { MakePredicate<PocoTestType>(s => s.Count == 5), "Count eq 5" },
                        new object[] { MakePredicate<PocoTestType>(s => 5 == s.Count), "Count eq 5" },
                        new object[] { MakePredicate<PocoTestType>(s => s.Count != 10), "Count ne 10" },
                        new object[] { MakePredicate<PocoTestType>(s => 10 != s.Count), "Count ne 10" },
                        new object[] { MakePredicate<PocoTestType>(s => s.Count < 15), "Count lt 15" },
                        new object[] { MakePredicate<PocoTestType>(s => 15 > s.Count), "Count lt 15" },
                        new object[] { MakePredicate<PocoTestType>(s => s.Count <= 20), "Count le 20" },
                        new object[] { MakePredicate<PocoTestType>(s => 20 >= s.Count), "Count le 20" },
                        new object[] { MakePredicate<PocoTestType>(s => 25 <= s.Count), "Count ge 25" },
                        new object[] { MakePredicate<PocoTestType>(s => s.Count >= 25), "Count ge 25" },
                        new object[] { MakePredicate<PocoTestType>(s => 30 < s.Count), "Count gt 30" },
                        new object[] { MakePredicate<PocoTestType>(s => s.Count > 30), "Count gt 30" },
                    };
            }
        }

        public static readonly DateTime ConstantDateTime = new DateTime(2014, 5, 23, 18, 0, 0, DateTimeKind.Utc);
        public static readonly byte[] TestByteArray = { 5, 8, 15, 16, 23, 42 };

        public static IEnumerable<object[]> DataTypeWhereExpressions
        {
            get
            {
                return new[]
                    {
                        new object[] { MakePredicate<PocoTestType>(s => s.Name == "Unicorn"), "Name eq 'Unicorn'" },
                        new object[] { MakePredicate<PocoTestType>(s => s.Name == " has ' quotes ' \" "), "Name eq ' has '' quotes '' \" '" },
                        new object[] { MakePredicate<PocoTestType>(s => s.Name == "rtl أنا لا أتكلم العربية rtl"), "Name eq 'rtl أنا لا أتكلم العربية rtl'" },
                        new object[] { MakePredicate<PocoTestType>(s => s.Name == "獨角獸"), "Name eq '獨角獸'" },
                        new object[] { MakePredicate<PocoTestType>(s => s.Count == 2147483647), "Count eq 2147483647" },
                        new object[] { MakePredicate<PocoTestType>(s => s.BigCount == 2147483648), "BigCount eq 2147483648L" },
                        new object[] { MakePredicate<PocoTestType>(s => s.Price == 100.75), "Price eq 100.75" },
                        new object[]
                            {
                                MakePredicate<PocoTestType>(s =>
                                    s.CustomerSince == ConstantDateTime),
                                "CustomerSince eq datetime'2014-05-23T18:00:00.0000000Z'"
                            },
                        new object[] { MakePredicate<PocoTestType>(s => s.IsEnchanted), "IsEnchanted eq true" },
                        new object[] { MakePredicate<PocoTestType>(s => !s.IsEnchanted), "IsEnchanted eq false" },
                        new object[] { MakePredicate<PocoTestType>(s => s.Buffer == TestByteArray), "Buffer eq X'05080f10172a'" },
                        new object[] { MakePredicate<PocoTestType>(s => s.Guid == new Guid("ca761232ed4211cebacd00aa0057b223")), "Guid eq guid'ca761232-ed42-11ce-bacd-00aa0057b223'" },
                        new object[] { MakePredicate<PocoTestType>(s => s.NestedObj.Count == 3), "" },
                    };
            }
        }

        public static IEnumerable<object[]> CompositeLogicWhereExpressions
        {
            get
            {
                return new[]
                    {
                        new object[] { MakePredicate<PocoTestType>(s => s.Count == 10 && s.Count == 20), "(Count eq 10) and (Count eq 20)" },
                        //new object[] { MakePredicate<PocoTestType>(s => s.Name=="Dijkstra" && (s.Count == 10 || s.Count == 20)), "(Name eq 'Dijkstra' and ((Count eq 10) and (Count eq 20))" },
                        //new object[] { MakePredicate<PocoTestType>(s => s.Count == 10 || s.Count == 20), "(Count eq 10) or (Count eq 20)" },
                        //new object[] { MakePredicate<PocoTestType>(s => s.Count < -10 && s.Name == "The Real Deal"), "(Count lt -10) and (Name eq 'The Real Deal')" },
                    };
            }
        }

        [Theory]
        [MemberData("SimpleWhereExpressions")]
        [MemberData("DataTypeWhereExpressions")]
        [MemberData("CompositeLogicWhereExpressions")]
        public void It_generates_where_queries(Expression expression, string expectedQuery)
        {
            var querySource = _fixture.CreateWithEntityQueryable<PocoTestType>();
            var visitor = CreateQueryingExpressionTreeVisitor(querySource);
            visitor.VisitExpression(querySource.FromExpression);
            var query = GetTableQuery(querySource);

            visitor.VisitExpression(expression);
            Assert.Equal(expectedQuery, query.ToString());
        }
    }
}

// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Data.Entity.AzureTableStorage.Query;
using Microsoft.Data.Entity.AzureTableStorage.Query.Expressions;
using Microsoft.Data.Entity.AzureTableStorage.Tests.Helpers;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Moq;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests.Query
{
    public class QueryGenerationTests : AtsQueryModelVisitor
    {
        private readonly TableQueryGenerator _generator = new TableQueryGenerator();

        public QueryGenerationTests()
            : base(new AtsQueryCompilationContext(SetupModel()))
        {
        }

        [Fact]
        public void It_creates_table_query()
        {
            var querySource = CreateWithEntityQueryable<PocoTestType>();
            var visitor = CreateQueryingExpressionTreeVisitor(querySource);
            SelectExpression query;

            visitor.VisitExpression(querySource.FromExpression);

            Assert.True(TryGetSelectExpression(querySource, out query));
            Assert.Equal(typeof(PocoTestType), query.Type);
            Assert.NotNull(query);
        }

        [Theory]
        [MemberData("SimpleWhereExpressions")]
        [MemberData("DataTypeWhereExpressions")]
        [MemberData("CompositeLogicWhereExpressions")]
        public void It_generates_where_queries(QueryModel queryModel, string expectedQuery)
        {
            var selectExpression = GetSelectExpression(queryModel);
            var tableQuery = _generator.GenerateTableQuery(selectExpression);
            Assert.Equal(expectedQuery, tableQuery.FilterString);
        }

        private SelectExpression GetSelectExpression(QueryModel queryModel)
        {
            VisitQueryModel(queryModel);

            return GetSelectExpression(queryModel.MainFromClause);
        }

        public static IEnumerable<object[]> SimpleWhereExpressions
        {
            get
            {
                return new[]
                    {
                        new object[] { Query(q => q.Where(s => s.Count == 5)), "Count eq 5" },
                        new object[] { Query(q => q.Where(s => 5 == s.Count)), "Count eq 5" },
                        new object[] { Query(q => q.Where(s => s.Count != 10)), "Count ne 10" },
                        new object[] { Query(q => q.Where(s => 10 != s.Count)), "Count ne 10" },
                        new object[] { Query(q => q.Where(s => s.Count < 15)), "Count lt 15" },
                        new object[] { Query(q => q.Where(s => 15 > s.Count)), "Count lt 15" },
                        new object[] { Query(q => q.Where(s => s.Count <= 20)), "Count le 20" },
                        new object[] { Query(q => q.Where(s => 20 >= s.Count)), "Count le 20" },
                        new object[] { Query(q => q.Where(s => 25 <= s.Count)), "Count ge 25" },
                        new object[] { Query(q => q.Where(s => s.Count >= 25)), "Count ge 25" },
                        new object[] { Query(q => q.Where(s => 30 < s.Count)), "Count gt 30" },
                        new object[] { Query(q => q.Where(s => s.Count > 30)), "Count gt 30" },
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
                        new object[] { Query(q => q.Where(s => s.Name == "Unicorn")), "Name eq 'Unicorn'" },
                        new object[] { Query(q => q.Where(s => s.Name == " has ' quotes ' \" ")), "Name eq ' has '' quotes '' \" '" },
                        new object[] { Query(q => q.Where(s => s.Name == "rtl أنا لا أتكلم العربية rtl")), "Name eq 'rtl أنا لا أتكلم العربية rtl'" },
                        new object[] { Query(q => q.Where(s => s.Name == "獨角獸")), "Name eq '獨角獸'" },
                        new object[] { Query(q => q.Where(s => s.Count == 2147483647)), "Count eq 2147483647" },
                        new object[] { Query(q => q.Where(s => s.BigCount == 2147483648)), "BigCount eq 2147483648L" },
                        new object[] { Query(q => q.Where(s => s.Price == 100.75)), "Price eq 100.75" },
                        new object[]
                            {
                                Query(q => q.Where(s =>
                                    s.CustomerSince == ConstantDateTime)),
                                "CustomerSince eq datetime'2014-05-23T18:00:00.0000000Z'"
                            },
                        new object[] { Query(q => q.Where(s => s.IsEnchanted)), "IsEnchanted eq true" },
                        new object[] { Query(q => q.Where(s => !s.IsEnchanted)), "IsEnchanted eq false" },
                        new object[] { Query(q => q.Where(s => s.Buffer == TestByteArray)), "Buffer eq X'05080f10172a'" },
                        new object[] { Query(q => q.Where(s => s.Guid == new Guid("ca761232ed4211cebacd00aa0057b223"))), "Guid eq guid'ca761232-ed42-11ce-bacd-00aa0057b223'" },
                        new object[] { Query(q => q.Where(s => s.NestedObj.Count == 3)), "" },
                    };
            }
        }

        public static IEnumerable<object[]> CompositeLogicWhereExpressions
        {
            get
            {
                return new[]
                    {
                        new object[] { Query(q => q.Where(s => s.Count == 10 && s.Count == 20)), "(Count eq 10) and (Count eq 20)" },
                        new object[] { Query(q => q.Where(s => s.Name == "Dijkstra" && (s.Count == 10 || s.Count == 20))), "(Name eq 'Dijkstra') and ((Count eq 10) or (Count eq 20))" },
                        new object[] { Query(q => q.Where(s => (s.Name == "Dijkstra" && !s.IsEnchanted) || (s.IsEnchanted && s.Count == 20))), "((Name eq 'Dijkstra') and (IsEnchanted eq false)) or ((IsEnchanted eq true) and (Count eq 20))" },
                        new object[] { Query(q => q.Where(s => s.Count == 10 || s.Count == 20)), "(Count eq 10) or (Count eq 20)" },
                        new object[] { Query(q => q.Where(s => s.Count < -10 && s.Name == "The Real Deal")), "(Count lt -10) and (Name eq 'The Real Deal')" },
                    };
            }
        }

        private static IModel SetupModel()
        {
            var model = new Model { StorageName = "TestModel" };
            model.AddEntityType(PocoTestType.EntityType());
            return model;
        }

        private static QueryModel Query(Expression<Func<DbSet<PocoTestType>, IQueryable>> expression)
        {
            var query = expression.Compile()(new DbSet<PocoTestType>(Mock.Of<DbContext>()));
            return new EntityQueryProvider(new EntityQueryExecutor(Mock.Of<DbContext>())).GenerateQueryModel(query.Expression);
        }

        private MainFromClause CreateWithEntityQueryable<T>()
        {
            var queryable = new EntityQueryable<T>(new EntityQueryExecutor(Mock.Of<DbContext>()));
            return new MainFromClause("s", typeof(T), Expression.Constant(queryable));
        }
    }
}

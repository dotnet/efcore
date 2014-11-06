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
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;
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
            : base(
                new AtsQueryCompilationContext(SetupModel(),
                    new LoggerFactory().Create("Fake"),
                    new EntityMaterializerSource(new MemberMapper(new FieldMatcher()))))
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
        [MemberData("PartitionAndRowKeyExpression")]
        public void It_generates_where_queries(string expectedQuery, QueryModel queryModel)
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
                        new object[] { "Count eq 5", Query<PocoTestType>(q => q.Where(s => s.Count == 5)) },
                        new object[] { "Count eq 5", Query<PocoTestType>(q => q.Where(s => 5 == s.Count)) },
                        new object[] { "Count ne 10", Query<PocoTestType>(q => q.Where(s => s.Count != 10)) },
                        new object[] { "Count ne 10", Query<PocoTestType>(q => q.Where(s => 10 != s.Count)) },
                        new object[] { "Count lt 15", Query<PocoTestType>(q => q.Where(s => s.Count < 15)) },
                        new object[] { "Count lt 15", Query<PocoTestType>(q => q.Where(s => 15 > s.Count)) },
                        new object[] { "Count le 20", Query<PocoTestType>(q => q.Where(s => s.Count <= 20)) },
                        new object[] { "Count le 20", Query<PocoTestType>(q => q.Where(s => 20 >= s.Count)) },
                        new object[] { "Count ge 25", Query<PocoTestType>(q => q.Where(s => 25 <= s.Count)) },
                        new object[] { "Count ge 25", Query<PocoTestType>(q => q.Where(s => s.Count >= 25)) },
                        new object[] { "Count gt 30", Query<PocoTestType>(q => q.Where(s => 30 < s.Count)) },
                        new object[] { "Count gt 30", Query<PocoTestType>(q => q.Where(s => s.Count > 30)) }
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
                        new object[] { "Name eq 'Unicorn'", Query<PocoTestType>(q => q.Where(s => s.Name == "Unicorn")) },
                        new object[] { "Name eq ' has '' quotes '' \" '", Query<PocoTestType>(q => q.Where(s => s.Name == " has ' quotes ' \" ")) },
                        new object[] { "Name eq 'rtl أنا لا أتكلم العربية rtl'", Query<PocoTestType>(q => q.Where(s => s.Name == "rtl أنا لا أتكلم العربية rtl")) },
                        new object[] { "Name eq '獨角獸'", Query<PocoTestType>(q => q.Where(s => s.Name == "獨角獸")) },
                        new object[] { "Count eq 2147483647", Query<PocoTestType>(q => q.Where(s => s.Count == 2147483647)) },
                        new object[] { "BigCount eq 2147483648L", Query<PocoTestType>(q => q.Where(s => s.BigCount == 2147483648)) },
                        new object[] { "Price eq 100.75", Query<PocoTestType>(q => q.Where(s => s.Price == 100.75)) },
                        new object[]
                            {
                                "CustomerSince eq datetime'2014-05-23T18:00:00.0000000Z'",
                                Query<PocoTestType>(q => q.Where(s =>
                                    s.CustomerSince == ConstantDateTime))
                            },
                        new object[] { "IsEnchanted eq true", Query<PocoTestType>(q => q.Where(s => s.IsEnchanted)) },
                        new object[] { "IsEnchanted eq false", Query<PocoTestType>(q => q.Where(s => !s.IsEnchanted)) },
                        new object[] { "Buffer eq X'05080f10172a'", Query<PocoTestType>(q => q.Where(s => s.Buffer == TestByteArray)) },
                        new object[] { "Guid eq guid'ca761232-ed42-11ce-bacd-00aa0057b223'", Query<PocoTestType>(q => q.Where(s => s.Guid == new Guid("ca761232ed4211cebacd00aa0057b223"))) },
                        new object[] { "", Query<PocoTestType>(q => q.Where(s => s.NestedObj.Count == 3)) },
                        new object[] { "NullInt eq 9", Query<NullablePoco>(q => q.Where(s => s.NullInt == 9)) },
                        new object[] { "NullDouble eq 3.149", Query<NullablePoco>(q => q.Where(s => s.NullDouble == 3.149)) },
                        new object[] { "", Query<NullablePoco>(q => q.Where(s => s.NullInt.HasValue)) }
                    };
            }
        }

        public static IEnumerable<object[]> CompositeLogicWhereExpressions
        {
            get
            {
                return new[]
                    {
                        new object[] { "(Count eq 10) and (Count eq 20)", Query<PocoTestType>(q => q.Where(s => s.Count == 10 && s.Count == 20)) },
                        new object[] { "(Name eq 'Dijkstra') and ((Count eq 10) or (Count eq 20))", Query<PocoTestType>(q => q.Where(s => s.Name == "Dijkstra" && (s.Count == 10 || s.Count == 20))) },
                        new object[] { "((Name eq 'Dijkstra') and (IsEnchanted eq false)) or ((IsEnchanted eq true) and (Count eq 20))", Query<PocoTestType>(q => q.Where(s => (s.Name == "Dijkstra" && !s.IsEnchanted) || (s.IsEnchanted && s.Count == 20))) },
                        new object[] { "(Count eq 10) or (Count eq 20)", Query<PocoTestType>(q => q.Where(s => s.Count == 10 || s.Count == 20)) },
                        new object[] { "(Count lt -10) and (Name eq 'The Real Deal')", Query<PocoTestType>(q => q.Where(s => s.Count < -10 && s.Name == "The Real Deal")) }
                    };
            }
        }

        public static IEnumerable<object[]> PartitionAndRowKeyExpression
        {
            get
            {
                return new[]
                    {
                        new object[] { "(RowKey eq '15') and (PartitionKey eq '16')", Query<IntKeysPoco>(s => s.Where(t => t.RowID == 15 && t.PartitionID == 16)) }
                    };
            }
        }

        private static IModel SetupModel()
        {
            var model = new Model { StorageName = "TestModel" };
            PocoTestType.EntityType(model);
            IntKeysPoco.EntityType(model);
            NullablePoco.EntityType(model);
            return model;
        }

        private static QueryModel Query<T>(Expression<Func<DbSet<T>, IQueryable>> expression) where T : class
        {
            var query = expression.Compile()(new DbSet<T>(Mock.Of<DbContext>()));
            return new EntityQueryProvider(new EntityQueryExecutor(Mock.Of<DbContext>(), new LazyRef<ILoggerFactory>(new LoggerFactory()))).GenerateQueryModel(query.Expression);
        }

        private MainFromClause CreateWithEntityQueryable<T>()
        {
            var queryable = new EntityQueryable<T>(new EntityQueryExecutor(Mock.Of<DbContext>(), new LazyRef<ILoggerFactory>(new LoggerFactory())));
            return new MainFromClause("s", typeof(T), Expression.Constant(queryable));
        }
    }
}

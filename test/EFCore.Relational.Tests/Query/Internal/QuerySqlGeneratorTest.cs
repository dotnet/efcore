// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

public class QuerySqlGeneratorTest
{
    [Theory,
     InlineData("INSERT something"),
     InlineData("SELECTANDSOMEOTHERSTUFF"),
     InlineData("SELECT"),
     InlineData("SELEC"),
     InlineData("- bad comment\nSELECT something"),
     InlineData("SELECT-\n1"),
     InlineData(""),
     InlineData("--SELECT")]
    public void CheckComposableSql_throws(string sql)
    {
        Assert.Equal(
            RelationalStrings.FromSqlNonComposable,
            Assert.Throws<InvalidOperationException>(() => CreateDummyQuerySqlGenerator().CheckComposableSql(sql)).Message);

        Assert.Equal(
            RelationalStrings.FromSqlNonComposable,
            Assert.Throws<InvalidOperationException>(() => CreateDummyQuerySqlGenerator().CheckComposableSql(sql.Replace("SELECT", "WITH")))
                .Message);
    }

    [Theory,
     InlineData("SELECT something"),
     InlineData("   SELECT something"),
     InlineData("-- comment\n SELECT something"),
     InlineData("-- comment1\r\n --\t\rcomment2\r\nSELECT something"),
     InlineData("SELECT--\n1"),
     InlineData("  /* comment */ SELECT--\n1"),
     InlineData("  /* multi\n*line\r\n * comment */ \nSELECT--\n1"),
     InlineData("SELECT/* comment */1")]
    public void CheckComposableSql_does_not_throw(string sql)
    {
        CreateDummyQuerySqlGenerator().CheckComposableSql(sql);

        CreateDummyQuerySqlGenerator().CheckComposableSql(sql.Replace("SELECT", "WITH"));
    }

    [Fact]
    public void AggregateFunction_without_orderings()
    {
        var generator = CreateDummyQuerySqlGenerator();

        var sql = generator.GetCommand(CreateAggregateFunction([])).CommandText;

        Assert.Equal("string_agg(a, ', ')", sql);
        Assert.Empty(generator.OrderingsHookCalls);
    }

    [Fact]
    public void AggregateFunction_with_orderings_generates_order_by_inside_argument_list()
    {
        var generator = CreateDummyQuerySqlGenerator();

        var sql = generator.GetCommand(
            CreateAggregateFunction(
            [
                new OrderingExpression(new SqlFragmentExpression("b"), ascending: true),
                new OrderingExpression(new SqlFragmentExpression("c"), ascending: false)
            ])).CommandText;

        Assert.Equal("string_agg(a, ', ' ORDER BY b, c DESC)", sql);
        Assert.Equal([(true, "string_agg(a, ', '"), (false, "string_agg(a, ', ' ORDER BY b, c DESC)")], generator.OrderingsHookCalls);
    }

    private static AggregateFunctionExpression CreateAggregateFunction(IReadOnlyList<OrderingExpression> orderings)
        => new(
            "string_agg",
            [new SqlFragmentExpression("a"), new SqlFragmentExpression("', '")],
            orderings,
            nullable: true,
            argumentsPropagateNullability: [false, false],
            typeof(string),
            typeMapping: null);

    private DummyQuerySqlGenerator CreateDummyQuerySqlGenerator()
        => new(
            new QuerySqlGeneratorDependencies(
                new RelationalCommandBuilderFactory(
                    new RelationalCommandBuilderDependencies(
                        new TestRelationalTypeMappingSource(
                            TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                            TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>()),
                        new ExceptionDetector(),
                        new LoggingOptions())),
                new RelationalSqlGenerationHelper(
                    new RelationalSqlGenerationHelperDependencies())));

    private class DummyQuerySqlGenerator(QuerySqlGeneratorDependencies dependencies) : QuerySqlGenerator(dependencies)
    {
        public List<(bool InsideArgumentList, string SqlBefore)> OrderingsHookCalls { get; } = [];

        public new void CheckComposableSql(string sql)
            => base.CheckComposableSql(sql);

        protected override void GenerateRootCommand(Expression queryExpression)
            => Visit(queryExpression);

        protected override void GenerateAggregateFunctionOrderings(
            AggregateFunctionExpression aggregateFunctionExpression,
            bool insideArgumentList)
        {
            var sqlBefore = Sql.Build().CommandText;

            base.GenerateAggregateFunctionOrderings(aggregateFunctionExpression, insideArgumentList);

            OrderingsHookCalls.Add((insideArgumentList, sqlBefore));
        }
    }
}

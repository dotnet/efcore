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
    public void Clr_type_change_preserves_operand_parentheses()
    {
        var intMapping = new IntTypeMapping("integer");
        var longMapping = new LongTypeMapping("integer");
        var sum = new SqlBinaryExpression(
            ExpressionType.Add,
            new SqlConstantExpression(1, intMapping),
            new SqlConstantExpression(2, intMapping),
            typeof(int),
            intMapping);
        var product = new SqlBinaryExpression(
            ExpressionType.Multiply,
            new ClrTypeChangeExpression(sum, typeof(long), longMapping),
            new SqlConstantExpression(3L, longMapping),
            typeof(long),
            longMapping);
        var select = new SelectExpression(
            alias: null,
            tables: [],
            predicate: null,
            groupBy: [],
            having: null,
            projections: [new ProjectionExpression(product, "")],
            distinct: false,
            orderings: [],
            offset: null,
            limit: null,
            sqlAliasManager: new SqlAliasManager());

        Assert.Equal("SELECT (1 + 2) * 3", CreateDummyQuerySqlGenerator().GetCommand(select).CommandText);
    }

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
        public new void CheckComposableSql(string sql)
            => base.CheckComposableSql(sql);
    }
}

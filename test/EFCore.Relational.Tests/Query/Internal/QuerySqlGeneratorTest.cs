// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class QuerySqlGeneratorTest
    {
        [Theory]
        [InlineData("INSERT something")]
        [InlineData("SELECTANDSOMEOTHERSTUFF")]
        [InlineData("SELECT")]
        [InlineData("SELEC")]
        [InlineData("- bad comment\nSELECT something")]
        [InlineData("SELECT-\n1")]
        [InlineData("")]
        [InlineData("--SELECT")]
        public void CheckComposableSql_throws(string sql)
            => Assert.Equal(
                RelationalStrings.FromSqlNonComposable,
                Assert.Throws<InvalidOperationException>(
                    () => CreateDummyQuerySqlGenerator().CheckComposableSql(sql)).Message);

        [Theory]
        [InlineData("SELECT something")]
        [InlineData("   SELECT something")]
        [InlineData("-- comment\n SELECT something")]
        [InlineData("-- comment1\r\n --\t\rcomment2\r\nSELECT something")]
        [InlineData("SELECT--\n1")]
        [InlineData("  /* comment */ SELECT--\n1")]
        [InlineData("  /* multi\n*line\r\n * comment */ \nSELECT--\n1")]
        [InlineData("SELECT/* comment */1")]
        public void CheckComposableSql_does_not_throw(string sql)
            => CreateDummyQuerySqlGenerator().CheckComposableSql(sql);

        private DummyQuerySqlGenerator CreateDummyQuerySqlGenerator()
            => new(
                new QuerySqlGeneratorDependencies(
                    new RelationalCommandBuilderFactory(
                        new RelationalCommandBuilderDependencies(
                            new TestRelationalTypeMappingSource(
                                TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                                TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>()))),
                    new RelationalSqlGenerationHelper(
                        new RelationalSqlGenerationHelperDependencies())));

        private class DummyQuerySqlGenerator : QuerySqlGenerator
        {
            public DummyQuerySqlGenerator(QuerySqlGeneratorDependencies dependencies)
                : base(dependencies)
            {
            }

            public new void CheckComposableSql(string sql)
                => base.CheckComposableSql(sql);
        }
    }
}

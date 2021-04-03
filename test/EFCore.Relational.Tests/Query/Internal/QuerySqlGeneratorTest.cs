// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
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

        [Theory]
        [InlineData(true, "SELECT 1\r\nFROM CustomFunction() AS \"c\"")]
        [InlineData(false, "SELECT 1\r\nFROM \"CustomFunction\"() AS \"c\"")]
        public void VisitTableValuedFunction_should_take_IsBuiltIn_option_into_account(bool isBuiltIn, string sql)
        {
            var entityType = CreateFunctionMappingEntityType(isBuiltIn);
            var selectExpression = CreateSelectExpression(entityType);
            var generator = CreateDummyQuerySqlGenerator();

            generator.GetCommand(selectExpression);
            var commandText = generator.Sql.Build().CommandText;

            // Transform new lines for Unix platforms.
            var expected = sql.Replace("\r\n", Environment.NewLine);

            Assert.Equal(expected, commandText);
        }

        private DummyQuerySqlGenerator CreateDummyQuerySqlGenerator()
            => new DummyQuerySqlGenerator(
                new QuerySqlGeneratorDependencies(
                    new RelationalCommandBuilderFactory(
                        new RelationalCommandBuilderDependencies(
                            new TestRelationalTypeMappingSource(
                                TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                                TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>()))),
                    new RelationalSqlGenerationHelper(
                        new RelationalSqlGenerationHelperDependencies())));

        private SelectExpression CreateSelectExpression(IEntityType entityType)
            => new SelectExpression(entityType,
                new SqlExpressionFactory(
                    new SqlExpressionFactoryDependencies(
                        new TestRelationalTypeMappingSource(
                            TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                            TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>()))));

        private static EntityType CreateFunctionMappingEntityType(bool isBuiltIn)
        {
            var model = new Model();
            var methodInfo = typeof(DummyDbFunctions).GetMethod(nameof(DummyDbFunctions.CustomFunction));
            var dbFunction = new DbFunction(methodInfo, model, ConfigurationSource.Convention)
            {
                IsBuiltIn = isBuiltIn
            };
            var storeFunction = new StoreFunction(dbFunction, new RelationalModel(model));
            var entityType = new EntityType(typeof(object), model, ConfigurationSource.Convention);
            var functionMapping = new FunctionMapping(entityType, storeFunction, dbFunction, false)
            {
                IsDefaultFunctionMapping = true
            };
            entityType[RelationalAnnotationNames.FunctionMappings] = new[] { functionMapping };
            return entityType;
        }

        private class DummyQuerySqlGenerator : QuerySqlGenerator
        {
            public DummyQuerySqlGenerator([NotNull] QuerySqlGeneratorDependencies dependencies)
                : base(dependencies)
            {
            }

            public new void CheckComposableSql(string sql)
                => base.CheckComposableSql(sql);

            public new IRelationalCommandBuilder Sql
                => base.Sql;
        }

        private static class DummyDbFunctions
        {
            public static int CustomFunction() => throw new NotImplementedException();
        }
    }
}

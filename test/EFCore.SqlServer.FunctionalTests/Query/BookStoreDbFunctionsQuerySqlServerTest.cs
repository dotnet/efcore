// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public class BookStoreDbFunctionsQuerySqlServerTest : BookStoreTestBase<BookStoreDbFunctionsQuerySqlServerTest.BookStoreSqlServerFixture>
    {
        public BookStoreDbFunctionsQuerySqlServerTest(BookStoreSqlServerFixture fixture)
            : base(fixture)
        {
        }

        public class BookStoreSqlServerFixture : BookStoreFixtureBase
        {
            public BookStoreSqlServerFixture()
            {
                var fullTextEnabled = ((SqlServerTestStore)TestStore).ExecuteScalar<int>(@"SELECT FULLTEXTSERVICEPROPERTY('IsFullTextInstalled')") == 1;
                if (fullTextEnabled)
                {
                    ((SqlServerTestStore)TestStore).ExecuteNonQuery(
                        @"IF EXISTS (SELECT * FROM sys.fulltext_catalogs WHERE name = 'BookStore_FTC')
                            BEGIN
                                DROP FULLTEXT CATALOG BookStore_FTC  
                            END");

                    ((SqlServerTestStore)TestStore).ExecuteNonQuery(
                        @"CREATE FULLTEXT CATALOG BookStore_FTC AS DEFAULT;
                             CREATE FULLTEXT INDEX ON Files(Data TYPE COLUMN FileExtension LANGUAGE 0 /* 0 = Neutral, 1033 = American English  */)  
                             KEY INDEX PK_Files WITH (STOPLIST = OFF); /* Or use (stoplist = Off) for no stoplist */");

                    ((SqlServerTestStore)TestStore).ExecuteNonQuery(
                        @"IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'[dbo].[WaitForFullTextIndexing]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1)
                            BEGIN
                                DROP PROCEDURE dbo.WaitForFullTextIndexing
                            END");

                    ((SqlServerTestStore)TestStore).ExecuteNonQuery(
                        @"CREATE PROCEDURE WaitForFullTextIndexing
                            @CatalogName VARCHAR(MAX)
                            AS
                            BEGIN
                                DECLARE @status int;
                                SET @status = 1;
                                DECLARE @waitLoops int;
                                SET @waitLoops = 0;

                                WHILE @status > 0 AND @waitLoops < 100
                                BEGIN       
                                    SELECT @status = FULLTEXTCATALOGPROPERTY(@CatalogName,'PopulateStatus')
                                    FROM sys.fulltext_catalogs AS cat;

                                    IF @status > 0
                                    BEGIN
                                        -- prevent thrashing
                                        WAITFOR DELAY '00:00:00.1';
                                    END
                                    SET @waitLoops = @waitLoops + 1;
                                END
                            END");

                    ((SqlServerTestStore)TestStore).ExecuteNonQuery(@"WaitForFullTextIndexing 'BookStore_FTC'");
                }
            }

            protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
        public void FreeText_literal()
        {
            using var context = CreateContext();
            var result =  context.Books.Single(c => EF.Functions.FreeText(c.File.Data, "heroic"));

            Assert.Equal(2, result.BookId);

            AssertSql(
                @"SELECT TOP(2) [b].[BookId], [b].[AuthorId], [b].[Created], [b].[FileId], [b].[Title]
FROM [Books] AS [b]
INNER JOIN [Files] AS [f] ON [b].[FileId] = [f].[FileId]
WHERE FREETEXT([f].[Data], N'heroic')");
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
        public void FreeText_multiple_words()
        {
            using var context = CreateContext();
            var result = context.Books.Single(c => EF.Functions.FreeText(c.File.Data, "heroic hearts"));

            Assert.Equal(2, result.BookId);

            AssertSql(
                @"SELECT TOP(2) [b].[BookId], [b].[AuthorId], [b].[Created], [b].[FileId], [b].[Title]
FROM [Books] AS [b]
INNER JOIN [Files] AS [f] ON [b].[FileId] = [f].[FileId]
WHERE FREETEXT([f].[Data], N'heroic hearts')");
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
        public void FreeText_with_language_term()
        {
            using var context = CreateContext();
            var result = context.Books.Single(c => EF.Functions.FreeText(c.File.Data, "heroic", 1033));

            Assert.Equal(2, result.BookId);

            AssertSql(
                @"SELECT TOP(2) [b].[BookId], [b].[AuthorId], [b].[Created], [b].[FileId], [b].[Title]
FROM [Books] AS [b]
INNER JOIN [Files] AS [f] ON [b].[FileId] = [f].[FileId]
WHERE FREETEXT([f].[Data], N'heroic', LANGUAGE 1033)");
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
        public void FreeText_with_multiple_words_and_language_term()
        {
            using var context = CreateContext();
            var result = context.Books
                .Single(c => EF.Functions.FreeText(c.File.Data, "heroic hearts", 1033));

            Assert.Equal(2, result.BookId);

            AssertSql(
                @"SELECT TOP(2) [b].[BookId], [b].[AuthorId], [b].[Created], [b].[FileId], [b].[Title]
FROM [Books] AS [b]
INNER JOIN [Files] AS [f] ON [b].[FileId] = [f].[FileId]
WHERE FREETEXT([f].[Data], N'heroic hearts', LANGUAGE 1033)");
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
        public void FreeText_multiple_predicates()
        {
            using var context = CreateContext();
            var result = context.Books
                .Single(
                    c => EF.Functions.FreeText(c.File.Data, "Nebelstreif")
                        && EF.Functions.FreeText(c.File.Data, "Vater", 1031));

            Assert.Equal(1, result.BookId);

            AssertSql(
                @"SELECT TOP(2) [b].[BookId], [b].[AuthorId], [b].[Created], [b].[FileId], [b].[Title]
FROM [Books] AS [b]
INNER JOIN [Files] AS [f] ON [b].[FileId] = [f].[FileId]
WHERE FREETEXT([f].[Data], N'Nebelstreif') AND FREETEXT([f].[Data], N'Vater', LANGUAGE 1031)");
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
        public async Task Contains_literal()
        {
            using var context = CreateContext();
            var result = await context.Books
                .Where(c => EF.Functions.Contains(c.File.Data, "Achilles"))
                .ToListAsync();

            Assert.Equal(2, result.First().BookId);

            AssertSql(
                @"SELECT [b].[BookId], [b].[AuthorId], [b].[Created], [b].[FileId], [b].[Title]
FROM [Books] AS [b]
INNER JOIN [Files] AS [f] ON [b].[FileId] = [f].[FileId]
WHERE CONTAINS([f].[Data], N'Achilles')");
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
        public void Contains_with_language_term()
        {
            using var context = CreateContext();
            var result = context.Books.Single(c => EF.Functions.Contains(c.File.Data, "Achilles", 1033));

            Assert.Equal(2, result.BookId);

            AssertSql(
                @"SELECT TOP(2) [b].[BookId], [b].[AuthorId], [b].[Created], [b].[FileId], [b].[Title]
FROM [Books] AS [b]
INNER JOIN [Files] AS [f] ON [b].[FileId] = [f].[FileId]
WHERE CONTAINS([f].[Data], N'Achilles', LANGUAGE 1033)");
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
        public async Task Contains_with_logical_operator()
        {
            using var context = CreateContext();
            var result = await context.Books
                .Where(c => EF.Functions.Contains(c.File.Data, "Achilles OR Nebelstreif"))
                .ToListAsync();

            Assert.Equal(2, result.Count);
            Assert.Equal(1, result.First().BookId);

            AssertSql(
                @"SELECT [b].[BookId], [b].[AuthorId], [b].[Created], [b].[FileId], [b].[Title]
FROM [Books] AS [b]
INNER JOIN [Files] AS [f] ON [b].[FileId] = [f].[FileId]
WHERE CONTAINS([f].[Data], N'Achilles OR Nebelstreif')");
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
        public async Task Contains_with_prefix_term_and_language_term()
        {
            using var context = CreateContext();
            var result = await context.Books
                .SingleOrDefaultAsync(c => EF.Functions.Contains(c.File.Data, "\"govern*\"", 1033));

            Assert.Equal(2, result.BookId);

            AssertSql(
                @"SELECT TOP(2) [b].[BookId], [b].[AuthorId], [b].[Created], [b].[FileId], [b].[Title]
FROM [Books] AS [b]
INNER JOIN [Files] AS [f] ON [b].[FileId] = [f].[FileId]
WHERE CONTAINS([f].[Data], N'""govern*""', LANGUAGE 1033)");
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
        public async Task Contains_with_proximity_term_and_language_term()
        {
            using var context = CreateContext();
            var result = await context.Books
                .SingleOrDefaultAsync(c => EF.Functions.Contains(c.File.Data, "NEAR((bunte, Blumen), 1)", 1033));

            Assert.Equal(1, result.BookId);

            AssertSql(
                @"SELECT TOP(2) [b].[BookId], [b].[AuthorId], [b].[Created], [b].[FileId], [b].[Title]
FROM [Books] AS [b]
INNER JOIN [Files] AS [f] ON [b].[FileId] = [f].[FileId]
WHERE CONTAINS([f].[Data], N'NEAR((bunte, Blumen), 1)', LANGUAGE 1033)");
        }

        private void AssertSql(params string[] expected)
            => testSqlLoggerFactory.AssertBaseline(expected);

        private TestSqlLoggerFactory testSqlLoggerFactory => (TestSqlLoggerFactory)Fixture.ListLoggerFactory;
    }
}

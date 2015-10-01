// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class DataAnnotationSqlServerTest : DataAnnotationTestBase<SqlServerTestStore, DataAnnotationSqlServerFixture>
    {
        public DataAnnotationSqlServerTest(DataAnnotationSqlServerFixture fixture)
            : base(fixture)
        {
        }

        public override void ConcurrencyCheckAttribute_throws_if_value_in_database_changed()
        {
            base.ConcurrencyCheckAttribute_throws_if_value_in_database_changed();

            Assert.Equal(@"SELECT TOP(1) [r].[UniqueNo], [r].[MaxLengthProperty], [r].[Name], [r].[RowVersion]
FROM [Sample] AS [r]
WHERE [r].[UniqueNo] = 1

SELECT TOP(1) [r].[UniqueNo], [r].[MaxLengthProperty], [r].[Name], [r].[RowVersion]
FROM [Sample] AS [r]
WHERE [r].[UniqueNo] = 1

@p0: 1
@p1: ModifiedData
@p2: 00000000-0000-0000-0003-000000000001
@p3: 00000001-0000-0000-0000-000000000001

SET NOCOUNT OFF;
UPDATE [Sample] SET [Name] = @p1, [RowVersion] = @p2
WHERE [UniqueNo] = @p0 AND [RowVersion] = @p3;
SELECT @@ROWCOUNT;

@p0: 1
@p1: ChangedData
@p2: 00000000-0000-0000-0002-000000000001
@p3: 00000001-0000-0000-0000-000000000001

SET NOCOUNT OFF;
UPDATE [Sample] SET [Name] = @p1, [RowVersion] = @p2
WHERE [UniqueNo] = @p0 AND [RowVersion] = @p3;
SELECT @@ROWCOUNT;",
                Sql);
        }

        public override void DatabaseGeneratedAttribute_autogenerates_values_when_set_to_identity()
        {
            base.DatabaseGeneratedAttribute_autogenerates_values_when_set_to_identity();

            Assert.Equal(@"@p0: 
@p1: Third
@p2: 00000000-0000-0000-0000-000000000003

SET NOCOUNT OFF;
INSERT INTO [Sample] ([MaxLengthProperty], [Name], [RowVersion])
OUTPUT INSERTED.[UniqueNo]
VALUES (@p0, @p1, @p2);",
                Sql);
        }

        public override void MaxLengthAttribute_throws_while_inserting_value_longer_than_max_length()
        {
            base.MaxLengthAttribute_throws_while_inserting_value_longer_than_max_length();

            Assert.Equal(@"@p0: Short
@p1: ValidString
@p2: 00000000-0000-0000-0000-000000000001

SET NOCOUNT OFF;
INSERT INTO [Sample] ([MaxLengthProperty], [Name], [RowVersion])
OUTPUT INSERTED.[UniqueNo]
VALUES (@p0, @p1, @p2);

@p0: VeryVeryVeryVeryVeryVeryLongString
@p1: ValidString
@p2: 00000000-0000-0000-0000-000000000002

SET NOCOUNT OFF;
INSERT INTO [Sample] ([MaxLengthProperty], [Name], [RowVersion])
OUTPUT INSERTED.[UniqueNo]
VALUES (@p0, @p1, @p2);",
                Sql);
        }

        public override void RequiredAttribute_for_navigation_throws_while_inserting_null_value()
        {
            base.RequiredAttribute_for_navigation_throws_while_inserting_null_value();

            Assert.Equal(@"@p0: Book1

SET NOCOUNT OFF;
INSERT INTO [BookDetail] ([BookId])
OUTPUT INSERTED.[Id]
VALUES (@p0);

@p0: 

SET NOCOUNT OFF;
INSERT INTO [BookDetail] ([BookId])
OUTPUT INSERTED.[Id]
VALUES (@p0);",
                Sql);
        }

        public override void RequiredAttribute_for_property_throws_while_inserting_null_value()
        {
            base.RequiredAttribute_for_property_throws_while_inserting_null_value();

            Assert.Equal(@"@p0: 
@p1: ValidString
@p2: 00000000-0000-0000-0000-000000000001

SET NOCOUNT OFF;
INSERT INTO [Sample] ([MaxLengthProperty], [Name], [RowVersion])
OUTPUT INSERTED.[UniqueNo]
VALUES (@p0, @p1, @p2);

@p0: 
@p1: 
@p2: 00000000-0000-0000-0000-000000000002

SET NOCOUNT OFF;
INSERT INTO [Sample] ([MaxLengthProperty], [Name], [RowVersion])
OUTPUT INSERTED.[UniqueNo]
VALUES (@p0, @p1, @p2);",
                Sql);
        }

        public override void StringLengthAttribute_throws_while_inserting_value_longer_than_max_length()
        {
            base.StringLengthAttribute_throws_while_inserting_value_longer_than_max_length();

            Assert.Equal(@"@p0: ValidString

SET NOCOUNT OFF;
INSERT INTO [Two] ([Data])
OUTPUT INSERTED.[Id], INSERTED.[Timestamp]
VALUES (@p0);

@p0: ValidButLongString

SET NOCOUNT OFF;
INSERT INTO [Two] ([Data])
OUTPUT INSERTED.[Id], INSERTED.[Timestamp]
VALUES (@p0);",
                Sql);
        }

        public override void TimestampAttribute_throws_if_value_in_database_changed()
        {
            base.TimestampAttribute_throws_if_value_in_database_changed();

            Assert.Equal(@"SELECT TOP(1) [r].[Id], [r].[Data], [r].[Timestamp]
FROM [Two] AS [r]
WHERE [r].[Id] = 1

SELECT TOP(1) [r].[Id], [r].[Data], [r].[Timestamp]
FROM [Two] AS [r]
WHERE [r].[Id] = 1

@p0: 1
@p1: ModifiedData
@p2: System.Byte[]

SET NOCOUNT OFF;
UPDATE [Two] SET [Data] = @p1
OUTPUT INSERTED.[Timestamp]
WHERE [Id] = @p0 AND [Timestamp] = @p2;

@p0: 1
@p1: ChangedData
@p2: System.Byte[]

SET NOCOUNT OFF;
UPDATE [Two] SET [Data] = @p1
OUTPUT INSERTED.[Timestamp]
WHERE [Id] = @p0 AND [Timestamp] = @p2;",
                Sql);
        }

        private static string Sql => TestSqlLoggerFactory.Sql;
    }
}

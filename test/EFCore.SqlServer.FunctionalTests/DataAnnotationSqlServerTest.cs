// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public class DataAnnotationSqlServerTest : DataAnnotationTestBase<DataAnnotationSqlServerTest.DataAnnotationSqlServerFixture>
    {
        // ReSharper disable once UnusedParameter.Local
        public DataAnnotationSqlServerTest(DataAnnotationSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
            //fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
            => facade.UseTransaction(transaction.GetDbTransaction());

        [Fact]
        public virtual ModelBuilder Default_for_key_string_column_throws()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Login1>().Property(l => l.UserName).HasDefaultValue("default");
            modelBuilder.Ignore<Profile1>();

            Assert.Equal(
                CoreStrings.WarningAsErrorTemplate(
                    RelationalEventId.ModelValidationKeyDefaultValueWarning,
                    RelationalStrings.LogKeyHasDefaultValue.GenerateMessage(nameof(Login1.UserName), nameof(Login1))),
                Assert.Throws<InvalidOperationException>(() => Validate(modelBuilder)).Message);

            return modelBuilder;
        }

        public override ModelBuilder Non_public_annotations_are_enabled()
        {
            var modelBuilder = base.Non_public_annotations_are_enabled();

            var relational = GetProperty<PrivateMemberAnnotationClass>(modelBuilder, "PersonFirstName").Relational();
            Assert.Equal("dsdsd", relational.ColumnName);
            Assert.Equal("nvarchar(128)", relational.ColumnType);

            return modelBuilder;
        }

        public override ModelBuilder Field_annotations_are_enabled()
        {
            var modelBuilder = base.Field_annotations_are_enabled();

            var relational = GetProperty<FieldAnnotationClass>(modelBuilder, "_personFirstName").Relational();
            Assert.Equal("dsdsd", relational.ColumnName);
            Assert.Equal("nvarchar(128)", relational.ColumnType);

            return modelBuilder;
        }

        public override ModelBuilder Key_and_column_work_together()
        {
            var modelBuilder = base.Key_and_column_work_together();

            var relational = GetProperty<ColumnKeyAnnotationClass1>(modelBuilder, "PersonFirstName").Relational();
            Assert.Equal("dsdsd", relational.ColumnName);
            Assert.Equal("nvarchar(128)", relational.ColumnType);

            return modelBuilder;
        }

        public override ModelBuilder Key_and_MaxLength_64_produce_nvarchar_64()
        {
            var modelBuilder = base.Key_and_MaxLength_64_produce_nvarchar_64();

            var property = GetProperty<ColumnKeyAnnotationClass2>(modelBuilder, "PersonFirstName");
            Assert.Equal("nvarchar(64)", TestServiceFactory.Instance.Create<SqlServerTypeMapper>().FindMapping(property).StoreType);

            return modelBuilder;
        }

        public override ModelBuilder Timestamp_takes_precedence_over_MaxLength()
        {
            var modelBuilder = base.Timestamp_takes_precedence_over_MaxLength();

            var property = GetProperty<TimestampAndMaxlen>(modelBuilder, "MaxTimestamp");
            Assert.Equal("rowversion", TestServiceFactory.Instance.Create<SqlServerTypeMapper>().FindMapping(property).StoreType);

            return modelBuilder;
        }

        public override ModelBuilder Timestamp_takes_precedence_over_MaxLength_with_value()
        {
            var modelBuilder = base.Timestamp_takes_precedence_over_MaxLength_with_value();

            var property = GetProperty<TimestampAndMaxlen>(modelBuilder, "NonMaxTimestamp");
            Assert.Equal("rowversion", TestServiceFactory.Instance.Create<SqlServerTypeMapper>().FindMapping(property).StoreType);

            return modelBuilder;
        }

        public override ModelBuilder TableNameAttribute_affects_table_name_in_TPH()
        {
            var modelBuilder = base.TableNameAttribute_affects_table_name_in_TPH();

            var relational = modelBuilder.Model.FindEntityType(typeof(TNAttrBase)).Relational();
            Assert.Equal("A", relational.TableName);

            return modelBuilder;
        }

        public override ModelBuilder DatabaseGeneratedOption_configures_the_property_correctly()
        {
            var modelBuilder = base.DatabaseGeneratedOption_configures_the_property_correctly();

            var identity = modelBuilder.Model.FindEntityType(typeof(GeneratedEntity)).FindProperty(nameof(GeneratedEntity.Identity));
            Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn, identity.SqlServer().ValueGenerationStrategy);

            return modelBuilder;
        }

        public override ModelBuilder DatabaseGeneratedOption_Identity_does_not_throw_on_noninteger_properties()
        {
            var modelBuilder = base.DatabaseGeneratedOption_Identity_does_not_throw_on_noninteger_properties();

            var entity = modelBuilder.Model.FindEntityType(typeof(GeneratedEntityNonInteger));

            var stringProperty = entity.FindProperty(nameof(GeneratedEntityNonInteger.String));
            Assert.Null(stringProperty.SqlServer().ValueGenerationStrategy);

            var dateTimeProperty = entity.FindProperty(nameof(GeneratedEntityNonInteger.DateTime));
            Assert.Null(dateTimeProperty.SqlServer().ValueGenerationStrategy);

            var guidProperty = entity.FindProperty(nameof(GeneratedEntityNonInteger.Guid));
            Assert.Null(guidProperty.SqlServer().ValueGenerationStrategy);

            return modelBuilder;
        }

        public override void ConcurrencyCheckAttribute_throws_if_value_in_database_changed()
        {
            base.ConcurrencyCheckAttribute_throws_if_value_in_database_changed();

            Assert.Equal(
                @"SELECT TOP(1) [r].[UniqueNo], [r].[MaxLengthProperty], [r].[Name], [r].[RowVersion], [r].[UniqueNo], [r].[Details_Name], [r].[UniqueNo], [r].[AdditionalDetails_Name]
FROM [Sample] AS [r]
WHERE [r].[UniqueNo] = 1

SELECT TOP(1) [r].[UniqueNo], [r].[MaxLengthProperty], [r].[Name], [r].[RowVersion], [r].[UniqueNo], [r].[Details_Name], [r].[UniqueNo], [r].[AdditionalDetails_Name]
FROM [Sample] AS [r]
WHERE [r].[UniqueNo] = 1

@p2='1'
@p0='ModifiedData' (Nullable = false) (Size = 4000)
@p1='00000000-0000-0000-0003-000000000001'
@p3='00000001-0000-0000-0000-000000000001'

SET NOCOUNT ON;
UPDATE [Sample] SET [Name] = @p0, [RowVersion] = @p1
WHERE [UniqueNo] = @p2 AND [RowVersion] = @p3;
SELECT @@ROWCOUNT;

@p2='1'
@p0='ChangedData' (Nullable = false) (Size = 4000)
@p1='00000000-0000-0000-0002-000000000001'
@p3='00000001-0000-0000-0000-000000000001'

SET NOCOUNT ON;
UPDATE [Sample] SET [Name] = @p0, [RowVersion] = @p1
WHERE [UniqueNo] = @p2 AND [RowVersion] = @p3;
SELECT @@ROWCOUNT;",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void DatabaseGeneratedAttribute_autogenerates_values_when_set_to_identity()
        {
            base.DatabaseGeneratedAttribute_autogenerates_values_when_set_to_identity();

            Assert.Equal(
                @"@p0='' (Size = 10) (DbType = String)
@p1='Third' (Nullable = false) (Size = 4000)
@p2='00000000-0000-0000-0000-000000000003'
@p3='Third Additional Name' (Size = 4000)
@p4='Third Name' (Size = 4000)

SET NOCOUNT ON;
INSERT INTO [Sample] ([MaxLengthProperty], [Name], [RowVersion], [AdditionalDetails_Name], [Details_Name])
VALUES (@p0, @p1, @p2, @p3, @p4);
SELECT [UniqueNo]
FROM [Sample]
WHERE @@ROWCOUNT = 1 AND [UniqueNo] = scope_identity();",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void MaxLengthAttribute_throws_while_inserting_value_longer_than_max_length()
        {
            base.MaxLengthAttribute_throws_while_inserting_value_longer_than_max_length();

            Assert.Equal(
                @"@p0='Short' (Size = 10)
@p1='ValidString' (Nullable = false) (Size = 4000)
@p2='00000000-0000-0000-0000-000000000001'
@p3='Third Additional Name' (Size = 4000)
@p4='Third Name' (Size = 4000)

SET NOCOUNT ON;
INSERT INTO [Sample] ([MaxLengthProperty], [Name], [RowVersion], [AdditionalDetails_Name], [Details_Name])
VALUES (@p0, @p1, @p2, @p3, @p4);
SELECT [UniqueNo]
FROM [Sample]
WHERE @@ROWCOUNT = 1 AND [UniqueNo] = scope_identity();

@p0='VeryVeryVeryVeryVeryVeryLongString' (Size = -1)
@p1='ValidString' (Nullable = false) (Size = 4000)
@p2='00000000-0000-0000-0000-000000000002'
@p3='Third Additional Name' (Size = 4000)
@p4='Third Name' (Size = 4000)

SET NOCOUNT ON;
INSERT INTO [Sample] ([MaxLengthProperty], [Name], [RowVersion], [AdditionalDetails_Name], [Details_Name])
VALUES (@p0, @p1, @p2, @p3, @p4);
SELECT [UniqueNo]
FROM [Sample]
WHERE @@ROWCOUNT = 1 AND [UniqueNo] = scope_identity();",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void RequiredAttribute_for_navigation_throws_while_inserting_null_value()
        {
            base.RequiredAttribute_for_navigation_throws_while_inserting_null_value();

            Assert.Contains(
                @"@p1='1'" + _eol,
                Sql);

            Assert.Contains(
                @"@p1='' (Nullable = false) (DbType = Int32)" + _eol,
                Sql);
        }

        public override void RequiredAttribute_for_property_throws_while_inserting_null_value()
        {
            base.RequiredAttribute_for_property_throws_while_inserting_null_value();

            Assert.Equal(
                @"@p0='' (Size = 10) (DbType = String)
@p1='ValidString' (Nullable = false) (Size = 4000)
@p2='00000000-0000-0000-0000-000000000001'
@p3='Two' (Size = 4000)
@p4='One' (Size = 4000)

SET NOCOUNT ON;
INSERT INTO [Sample] ([MaxLengthProperty], [Name], [RowVersion], [AdditionalDetails_Name], [Details_Name])
VALUES (@p0, @p1, @p2, @p3, @p4);
SELECT [UniqueNo]
FROM [Sample]
WHERE @@ROWCOUNT = 1 AND [UniqueNo] = scope_identity();

@p0='' (Size = 10) (DbType = String)
@p1='' (Nullable = false) (Size = 4000) (DbType = String)
@p2='00000000-0000-0000-0000-000000000002'
@p3='Two' (Size = 4000)
@p4='One' (Size = 4000)

SET NOCOUNT ON;
INSERT INTO [Sample] ([MaxLengthProperty], [Name], [RowVersion], [AdditionalDetails_Name], [Details_Name])
VALUES (@p0, @p1, @p2, @p3, @p4);
SELECT [UniqueNo]
FROM [Sample]
WHERE @@ROWCOUNT = 1 AND [UniqueNo] = scope_identity();",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void StringLengthAttribute_throws_while_inserting_value_longer_than_max_length()
        {
            base.StringLengthAttribute_throws_while_inserting_value_longer_than_max_length();

            Assert.Equal(
                @"@p0='ValidString' (Size = 16)

SET NOCOUNT ON;
INSERT INTO [Two] ([Data])
VALUES (@p0);
SELECT [Id], [Timestamp]
FROM [Two]
WHERE @@ROWCOUNT = 1 AND [Id] = scope_identity();

@p0='ValidButLongString' (Size = -1)

SET NOCOUNT ON;
INSERT INTO [Two] ([Data])
VALUES (@p0);
SELECT [Id], [Timestamp]
FROM [Two]
WHERE @@ROWCOUNT = 1 AND [Id] = scope_identity();",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void TimestampAttribute_throws_if_value_in_database_changed()
        {
            base.TimestampAttribute_throws_if_value_in_database_changed();

            // Not validating SQL because not significantly different from other tests and
            // row version value is not stable.
        }

        private static readonly string _eol = Environment.NewLine;

        private string Sql => Fixture.TestSqlLoggerFactory.Sql;

        public class DataAnnotationSqlServerFixture : DataAnnotationFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();
        }
    }
}

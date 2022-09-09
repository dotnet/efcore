// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Update;

#nullable enable

public class StoredProcedureUpdateSqlServerTest
    : StoredProcedureUpdateTestBase<StoredProcedureUpdateSqlServerTest.StoredProcedureUpdateSqlServerFixture>
{
    public StoredProcedureUpdateSqlServerTest(StoredProcedureUpdateSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        // Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Insert_with_output_parameter(bool async)
    {
        await base.Insert_with_output_parameter(async);

        AssertSql(
            @"@p0='New' (Size = 4000)
@p1='1' (Direction = Output)

SET NOCOUNT ON;
EXEC [WithOutputParameter_Insert] @p0, @p1 OUTPUT;");
    }

    public override async Task Insert_twice_with_output_parameter(bool async)
    {
        await base.Insert_twice_with_output_parameter(async);

        AssertSql(
            @"@p0='New1' (Size = 4000)
@p1='1' (Direction = Output)
@p2='New2' (Size = 4000)
@p3='2' (Direction = Output)

SET NOCOUNT ON;
EXEC [WithOutputParameter_Insert] @p0, @p1 OUTPUT;
EXEC [WithOutputParameter_Insert] @p2, @p3 OUTPUT;");
    }

    public override async Task Insert_with_result_column(bool async)
    {
        await base.Insert_with_result_column(async);

        AssertSql(
            @"@p0='Foo' (Size = 4000)

SET NOCOUNT ON;
EXEC [WithResultColumn_Insert] @p0;");
    }

    public override async Task Insert_with_two_result_columns(bool async)
    {
        await base.Insert_with_two_result_columns(async);

        AssertSql(
            @"@p0='Foo' (Size = 4000)

SET NOCOUNT ON;
EXEC [WithTwoResultColumns_Insert] @p0;");
    }

    public override async Task Insert_with_output_parameter_and_result_column(bool async)
    {
        await base.Insert_with_output_parameter_and_result_column(async);

        AssertSql(
            @"@p0=NULL (Nullable = false) (Direction = Output) (DbType = Int32)
@p1='Foo' (Size = 4000)

SET NOCOUNT ON;
EXEC [WithOutputParameterAndResultColumn_Insert] @p0 OUTPUT, @p1;");
    }

    public override async Task Update(bool async)
    {
        await base.Update(async);

        AssertSql(
            @"@p0='1'
@p1='Updated' (Size = 4000)

SET NOCOUNT ON;
EXEC [WithOutputParameter_Update] @p0, @p1;");
    }

    public override async Task Update_partial(bool async)
    {
        await base.Update_partial(async);

        AssertSql(
            @"@p0='1'
@p1='Updated' (Size = 4000)
@p2='8'

SET NOCOUNT ON;
EXEC [WithTwoInputParameters_Update] @p0, @p1, @p2;");
    }

    public override async Task Update_with_output_parameter_and_rows_affected_result_column(bool async)
    {
        await base.Update_with_output_parameter_and_rows_affected_result_column(async);

        AssertSql(
            @"@p0='1'
@p1='Updated' (Size = 4000)
@p2=NULL (Nullable = false) (Direction = Output) (DbType = Int32)

SET NOCOUNT ON;
EXEC [WithOutputParameterAndRowsAffectedResultColumn_Update] @p0, @p1, @p2 OUTPUT;");
    }

    public override async Task Update_with_output_parameter_and_rows_affected_result_column_concurrency_failure(bool async)
    {
        await base.Update_with_output_parameter_and_rows_affected_result_column_concurrency_failure(async);

        AssertSql(
            @"@p0='1'
@p1='Updated' (Size = 4000)
@p2=NULL (Nullable = false) (Direction = Output) (DbType = Int32)

SET NOCOUNT ON;
EXEC [WithOutputParameterAndRowsAffectedResultColumn_Update] @p0, @p1, @p2 OUTPUT;");
    }

    public override async Task Delete(bool async)
    {
        await base.Delete(async);

        AssertSql(
            @"@p0='1'

SET NOCOUNT ON;
EXEC [WithOutputParameter_Delete] @p0;");
    }

    public override async Task Delete_and_insert(bool async)
    {
        await base.Delete_and_insert(async);

        AssertSql(
            @"@p0='1'
@p1='Entity2' (Size = 4000)
@p2='2' (Direction = Output)

SET NOCOUNT ON;
EXEC [WithOutputParameter_Delete] @p0;
EXEC [WithOutputParameter_Insert] @p1, @p2 OUTPUT;");
    }

    public override async Task Rows_affected_parameter(bool async)
    {
        await base.Rows_affected_parameter(async);

        AssertSql(
            @"@p0='1'
@p1='Updated' (Size = 4000)
@p2='1' (Direction = Output)

SET NOCOUNT ON;
EXEC [WithRowsAffectedParameter_Update] @p0, @p1, @p2 OUTPUT;");
    }

    public override async Task Rows_affected_parameter_and_concurrency_failure(bool async)
    {
        await base.Rows_affected_parameter_and_concurrency_failure(async);

        AssertSql(
            @"@p0='1'
@p1='Updated' (Size = 4000)
@p2='0' (Direction = Output)

SET NOCOUNT ON;
EXEC [WithRowsAffectedParameter_Update] @p0, @p1, @p2 OUTPUT;");
    }

    public override async Task Rows_affected_result_column(bool async)
    {
        await base.Rows_affected_result_column(async);

        AssertSql(
            @"@p0='1'
@p1='Updated' (Size = 4000)

SET NOCOUNT ON;
EXEC [WithRowsAffectedResultColumn_Update] @p0, @p1;");
    }

    public override async Task Rows_affected_result_column_and_concurrency_failure(bool async)
    {
        await base.Rows_affected_result_column_and_concurrency_failure(async);

        AssertSql(
            @"@p0='1'
@p1='Updated' (Size = 4000)

SET NOCOUNT ON;
EXEC [WithRowsAffectedResultColumn_Update] @p0, @p1;");
    }

    public override async Task Rows_affected_return_value(bool async)
    {
        await base.Rows_affected_return_value(async);

        AssertSql(
            @"@p0='1' (Direction = Output)
@p1='1'
@p2='Updated' (Size = 4000)

SET NOCOUNT ON;
EXEC @p0 = [WithRowsAffectedReturnValue_Update] @p1, @p2;");
    }

    public override async Task Rows_affected_return_value_and_concurrency_failure(bool async)
    {
        await base.Rows_affected_return_value_and_concurrency_failure(async);

        AssertSql(
            @"@p0='0' (Direction = Output)
@p1='1'
@p2='Updated' (Size = 4000)

SET NOCOUNT ON;
EXEC @p0 = [WithRowsAffectedReturnValue_Update] @p1, @p2;");
    }

    public override async Task Store_generated_concurrency_token_as_in_out_parameter(bool async)
    {
        await base.Store_generated_concurrency_token_as_in_out_parameter(async);

        // Can't assert SQL baseline as usual because the concurrency token changes
        Assert.Contains("(Size = 8) (Direction = InputOutput)", Fixture.TestSqlLoggerFactory.Sql);

        Assert.Equal(
            @"@p2='Updated' (Size = 4000)
@p3='0' (Direction = Output)

SET NOCOUNT ON;
EXEC [WithStoreGeneratedConcurrencyTokenAsInOutParameter_Update] @p0, @p1 OUTPUT, @p2, @p3 OUTPUT;",
            Fixture.TestSqlLoggerFactory.Sql.Substring(Fixture.TestSqlLoggerFactory.Sql.IndexOf("@p2", StringComparison.Ordinal)),
            ignoreLineEndingDifferences: true);

        Assert.Equal(
            @"@p2='Updated' (Size = 4000)
@p3='0' (Direction = Output)

SET NOCOUNT ON;
EXEC [WithStoreGeneratedConcurrencyTokenAsInOutParameter_Update] @p0, @p1 OUTPUT, @p2, @p3 OUTPUT;",
            Fixture.TestSqlLoggerFactory.Sql.Substring(Fixture.TestSqlLoggerFactory.Sql.IndexOf("@p2", StringComparison.Ordinal)),
            ignoreLineEndingDifferences: true);
    }

    public override async Task Store_generated_concurrency_token_as_two_parameters(bool async)
    {
        await base.Store_generated_concurrency_token_as_two_parameters(async);

        // Can't assert SQL baseline as usual because the concurrency token changes
        Assert.Equal(
            @"@p2='Updated' (Size = 4000)
@p3=NULL (Size = 8) (Direction = Output) (DbType = Binary)
@p4='0' (Direction = Output)

SET NOCOUNT ON;
EXEC [WithStoreGeneratedConcurrencyTokenAsTwoParameters_Update] @p0, @p1, @p2, @p3 OUTPUT, @p4 OUTPUT;",
            Fixture.TestSqlLoggerFactory.Sql.Substring(Fixture.TestSqlLoggerFactory.Sql.IndexOf("@p2", StringComparison.Ordinal)),
            ignoreLineEndingDifferences: true);
    }

    public override async Task User_managed_concurrency_token(bool async)
    {
        await base.User_managed_concurrency_token(async);

        AssertSql(
            @"@p0='1'
@p1='8'
@p2='Updated' (Size = 4000)
@p3='9'
@p4='0' (Direction = Output)

SET NOCOUNT ON;
EXEC [WithUserManagedConcurrencyToken_Update] @p0, @p1, @p2, @p3, @p4 OUTPUT;");
    }

    public override async Task Original_and_current_value_on_non_concurrency_token(bool async)
    {
        await base.Original_and_current_value_on_non_concurrency_token(async);

        AssertSql(
            @"@p0='1'
@p1='Updated' (Size = 4000)
@p2='Initial' (Size = 4000)

SET NOCOUNT ON;
EXEC [WithOriginalAndCurrentValueOnNonConcurrencyToken_Update] @p0, @p1, @p2;");
    }

    public override async Task Tph(bool async)
    {
        await base.Tph(async);

        AssertSql(
            @"@p0=NULL (Nullable = false) (Direction = Output) (DbType = Int32)
@p1='TphChild1' (Nullable = false) (Size = 4000)
@p2='Child' (Size = 4000)
@p3='8' (Nullable = true)
@p4=NULL (DbType = Int32)
@p5=NULL (Direction = Output) (DbType = Int32)

SET NOCOUNT ON;
EXEC [Tph_Insert] @p0 OUTPUT, @p1, @p2, @p3, @p4, @p5 OUTPUT;");
    }

    public override async Task Tpt(bool async)
    {
        await base.Tpt(async);

        AssertSql(
            @"@p0='1' (Direction = Output)
@p1='Child' (Size = 4000)

SET NOCOUNT ON;
EXEC [TptParent_Insert] @p0 OUTPUT, @p1;",
            //
            @"@p2='1'
@p3='8'

SET NOCOUNT ON;
EXEC [TptChild_Insert] @p2, @p3;");
    }

    public override async Task Tpt_mixed_sproc_and_non_sproc(bool async)
    {
        await base.Tpt_mixed_sproc_and_non_sproc(async);

        AssertSql(
            @"@p0='1' (Direction = Output)
@p1='Child' (Size = 4000)

SET NOCOUNT ON;
EXEC [TptMixedParent_Insert] @p0 OUTPUT, @p1;",
            //
            @"@p2='1'
@p3='8'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [TptMixedChild] ([Id], [ChildProperty])
VALUES (@p2, @p3);");
    }

    public override async Task Tpc(bool async)
    {
        await base.Tpc(async);

        AssertSql(
            @"@p0='1' (Direction = Output)
@p1='Child' (Size = 4000)
@p2='8'

SET NOCOUNT ON;
EXEC [TpcChild_Insert] @p0 OUTPUT, @p1, @p2;");
    }

    public override async Task Input_or_output_parameter_with_input(bool async)
    {
        await base.Input_or_output_parameter_with_input(async);

        AssertSql(
            @"@p0='1' (Direction = Output)
@p1=NULL (Nullable = false) (Size = 4000) (Direction = InputOutput)

SET NOCOUNT ON;
EXEC [WithInputOrOutputParameter_Insert] @p0 OUTPUT, @p1 OUTPUT;");
    }

    public override async Task Input_or_output_parameter_with_output(bool async)
    {
        await base.Input_or_output_parameter_with_output(async);

        AssertSql(
            @"@p0='1' (Direction = Output)
@p1='Some default value' (Nullable = false) (Size = 4000) (Direction = InputOutput)

SET NOCOUNT ON;
EXEC [WithInputOrOutputParameter_Insert] @p0 OUTPUT, @p1 OUTPUT;");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public class StoredProcedureUpdateSqlServerFixture : StoredProcedureUpdateFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => StoredProcedureTestStoryFactory.Instance;

        protected override void ConfigureStoreGeneratedConcurrencyToken(EntityTypeBuilder entityTypeBuilder, string propertyName)
            => entityTypeBuilder.Property<byte[]>(propertyName).IsRowVersion();

        public override void CleanData()
        {
            using var context = CreateContext();
            context.Database.ExecuteSqlRaw(CleanDataSql);
        }

        private const string CleanDataSql = @"
-- Regular tables without foreign keys
TRUNCATE TABLE [WithInputOrOutputParameter];
TRUNCATE TABLE [WithOriginalAndCurrentValueOnNonConcurrencyToken];
TRUNCATE TABLE [WithOutputParameter];
TRUNCATE TABLE [WithOutputParameterAndResultColumn];
TRUNCATE TABLE [WithOutputParameterAndRowsAffectedResultColumn];
TRUNCATE TABLE [WithResultColumn];
TRUNCATE TABLE [WithTwoResultColumns];
TRUNCATE TABLE [WithRowsAffectedParameter];
TRUNCATE TABLE [WithRowsAffectedResultColumn];
TRUNCATE TABLE [WithRowsAffectedReturnValue];
TRUNCATE TABLE [WithStoreGeneratedConcurrencyTokenAsInOutParameter];
TRUNCATE TABLE [WithStoreGeneratedConcurrencyTokenAsTwoParameters];
TRUNCATE TABLE [WithTwoInputParameters];
TRUNCATE TABLE [WithUserManagedConcurrencyToken];
TRUNCATE TABLE [Tph];
TRUNCATE TABLE [TpcChild];
TRUNCATE TABLE [TpcParent];

ALTER SEQUENCE [TpcParentSequence] RESTART WITH 1;

-- We can't use TRUNCATE on tables with foreign keys, so we DELETE and reset IDENTITY manually.
-- DBCC CHECKIDENT resets IDENTITY, but behaves differently based on whether whether rows were ever inserted (seed+1) or not (seed).
-- So we insert a dummy row before deleting everything to make sure we get the seed value 1.
INSERT INTO [TptMixedParent] DEFAULT VALUES;
DELETE FROM [TptMixedChild];
DELETE FROM [TptMixedParent];
DBCC CHECKIDENT ('[TptMixedParent]', RESEED, 0);

INSERT INTO [TptParent] DEFAULT VALUES;
DELETE FROM [TptChild];
DELETE FROM [TptParent];
DBCC CHECKIDENT ('[TptParent]', RESEED, 0);";

        private class StoredProcedureTestStoryFactory : SqlServerTestStoreFactory
        {
            public static new StoredProcedureTestStoryFactory Instance { get; } = new();

            public override TestStore GetOrCreate(string storeName)
                => SqlServerTestStore.GetOrCreateWithInitScript(storeName, InitScript);

            private const string InitScript = @"
CREATE PROCEDURE WithOutputParameter_Insert(@Name varchar(max), @Id int OUT)
AS BEGIN
    INSERT INTO [WithOutputParameter] ([Name]) VALUES (@Name);
    SET @Id = SCOPE_IDENTITY();
END;

GO

CREATE PROCEDURE WithOutputParameter_Update(@Id int, @Name varchar(max))
AS UPDATE [WithOutputParameter] SET [Name] = @Name WHERE [Id] = @id;

GO

CREATE PROCEDURE WithOutputParameter_Delete(@Id int)
AS DELETE FROM [WithOutputParameter] WHERE [Id] = @Id;

GO

CREATE PROCEDURE WithResultColumn_Insert(@Name varchar(max))
AS INSERT INTO [WithResultColumn] ([Name]) OUTPUT [Inserted].[Id] VALUES (@Name);

GO

CREATE PROCEDURE WithTwoResultColumns_Insert(@Name varchar(max))
AS INSERT INTO [WithTwoResultColumns] ([Name]) OUTPUT [Inserted].[AdditionalProperty], [Inserted].[Id] VALUES (@Name);

GO

CREATE PROCEDURE WithOutputParameterAndResultColumn_Insert(@Id int OUT, @Name varchar(max))
AS BEGIN
    INSERT INTO [WithOutputParameterAndResultColumn] ([Name]) VALUES (@Name);
    SET @Id = SCOPE_IDENTITY();
    SELECT [AdditionalProperty] FROM [WithOutputParameterAndResultColumn] WHERE [Id] = @Id
END;

GO

CREATE PROCEDURE WithOutputParameterAndRowsAffectedResultColumn_Update(@Id int, @Name varchar(max), @AdditionalProperty int OUT)
AS BEGIN
    UPDATE [WithOutputParameterAndRowsAffectedResultColumn] SET [Name] = @Name, @AdditionalProperty = [AdditionalProperty] WHERE [Id] = @Id;
    SELECT @@ROWCOUNT;
END;

GO

CREATE PROCEDURE WithTwoInputParameters_Update(@Id int, @Name varchar(max), @AdditionalProperty int)
AS UPDATE [WithTwoInputParameters] SET [Name] = @Name, [AdditionalProperty] = @AdditionalProperty WHERE [Id] = @id;

GO

CREATE PROCEDURE WithRowsAffectedParameter_Update(@Id int, @Name varchar(max), @RowsAffected int OUT)
AS BEGIN
    UPDATE [WithRowsAffectedParameter] SET [Name] = @Name WHERE [Id] = @Id;
    SET @RowsAffected = @@ROWCOUNT;
END;

GO

CREATE PROCEDURE WithRowsAffectedResultColumn_Update(@Id int, @Name varchar(max))
AS BEGIN
    UPDATE [WithRowsAffectedResultColumn] SET [Name] = @Name WHERE [Id] = @Id;
    SELECT @@ROWCOUNT;
END;

GO

CREATE PROCEDURE WithRowsAffectedReturnValue_Update(@Id int, @Name varchar(max))
AS BEGIN
    UPDATE [WithRowsAffectedReturnValue] SET [Name] = @Name WHERE [Id] = @Id;
    RETURN @@ROWCOUNT;
END;

GO

CREATE PROCEDURE WithStoreGeneratedConcurrencyTokenAsInOutParameter_Update(@Id int, @ConcurrencyToken rowversion OUT, @Name varchar(max), @RowsAffected int OUT)
AS BEGIN
    UPDATE [WithStoreGeneratedConcurrencyTokenAsInOutParameter] SET [Name] = @Name WHERE [Id] = @Id AND [ConcurrencyToken] = @ConcurrencyToken;
    SET @RowsAffected = @@ROWCOUNT;
END;

GO

CREATE PROCEDURE WithStoreGeneratedConcurrencyTokenAsTwoParameters_Update(@Id int, @ConcurrencyTokenIn rowversion, @Name varchar(max), @ConcurrencyTokenOut rowversion OUT, @RowsAffected int OUT)
AS BEGIN
    UPDATE [WithStoreGeneratedConcurrencyTokenAsTwoParameters] SET [Name] = @Name, @ConcurrencyTokenOut = [ConcurrencyToken] WHERE [Id] = @Id AND [ConcurrencyToken] = @ConcurrencyTokenIn;
    SET @RowsAffected = @@ROWCOUNT;
END;

GO

CREATE PROCEDURE WithUserManagedConcurrencyToken_Update(@Id int, @ConcurrencyTokenOriginal int, @Name varchar(max), @ConcurrencyTokenCurrent int, @RowsAffected int OUT)
AS BEGIN
    UPDATE [WithUserManagedConcurrencyToken] SET [Name] = @Name, [AdditionalProperty] = @ConcurrencyTokenCurrent WHERE [Id] = @Id AND [AdditionalProperty] = @ConcurrencyTokenOriginal;
    SET @RowsAffected = @@ROWCOUNT;
END;

GO

CREATE PROCEDURE WithOriginalAndCurrentValueOnNonConcurrencyToken_Update(@Id int, @NameCurrent varchar(max), @NameOriginal varchar(max))
AS BEGIN
    IF @NameCurrent <> @NameOriginal
    BEGIN
        UPDATE [WithOriginalAndCurrentValueOnNonConcurrencyToken] SET [Name] = @NameCurrent WHERE [Id] = @Id;
    END
END;

GO

CREATE PROCEDURE WithInputOrOutputParameter_Insert(@Id int OUT, @Name varchar(max) OUT)
AS BEGIN
    IF @Name IS NULL
    BEGIN
        INSERT INTO [WithInputOrOutputParameter] ([Name]) VALUES ('Some default value');
        SET @Name = 'Some default value';
    END
    ELSE
    BEGIN
        INSERT INTO [WithInputOrOutputParameter] ([Name]) VALUES (@Name);
        SET @Name = NULL;
    END

    SET @Id = SCOPE_IDENTITY();
END;

GO

CREATE PROCEDURE Tph_Insert(@Id int OUT, @Discriminator varchar(max), @Name varchar(max), @Child1Property int, @Child2InputProperty int, @Child2OutputParameterProperty int OUT)
AS BEGIN
    DECLARE @Table table ([Child2OutputParameterProperty] int);
    INSERT INTO [Tph] ([Discriminator], [Name], [Child1Property], [Child2InputProperty]) OUTPUT [Inserted].[Child2OutputParameterProperty] INTO @Table VALUES (@Discriminator, @Name, @Child1Property, @Child2InputProperty);
    SET @Id = SCOPE_IDENTITY();
    SELECT @Child2OutputParameterProperty = [Child2OutputParameterProperty] FROM @Table;
    SELECT [Child2ResultColumnProperty] FROM [Tph] WHERE [Id] = @Id
END;

GO

CREATE PROCEDURE TptParent_Insert(@Id int OUT, @Name varchar(max))
AS BEGIN
    INSERT INTO [TptParent] ([Name]) VALUES (@Name);
    SET @Id = SCOPE_IDENTITY();
END;

GO

CREATE PROCEDURE TptChild_Insert(@Id int, @ChildProperty int)
AS BEGIN
    INSERT INTO [TptChild] ([Id], [ChildProperty]) VALUES (@Id, @ChildProperty);
    SET @Id = SCOPE_IDENTITY();
END;

GO

CREATE PROCEDURE TptMixedParent_Insert(@Id int OUT, @Name varchar(max))
AS BEGIN
    INSERT INTO [TptMixedParent] ([Name]) VALUES (@Name);
    SET @Id = SCOPE_IDENTITY();
END;

GO

CREATE PROCEDURE TpcChild_Insert(@Id int OUT, @Name varchar(max), @ChildProperty int)
AS BEGIN
    DECLARE @Table table ([Id] int);
    INSERT INTO [TpcChild] ([Name], [ChildProperty]) OUTPUT [Inserted].[Id] INTO @Table VALUES (@Name, @ChildProperty);
    SELECT @Id = [Id] FROM @Table;
END;";
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Update;

#nullable disable

public class StoredProcedureUpdateSqlServerTest : StoredProcedureUpdateTestBase
{
    public override async Task Insert_with_output_parameter(bool async)
    {
        await Insert_with_output_parameter(
            async,
            """
CREATE PROCEDURE Entity_Insert(@Name varchar(max), @Id int OUT)
AS BEGIN
    INSERT INTO [Entity] ([Name]) VALUES (@Name);
    SET @Id = SCOPE_IDENTITY();
END
""");

        AssertSql(
            """
@p0='New' (Size = 4000)
@p1='1' (Direction = Output)

SET NOCOUNT ON;
EXEC [Entity_Insert] @p0, @p1 OUTPUT;
""");
    }

    public override async Task Insert_twice_with_output_parameter(bool async)
    {
        await Insert_twice_with_output_parameter(
            async,
            """
CREATE PROCEDURE Entity_Insert(@Name varchar(max), @Id int OUT)
AS BEGIN
    INSERT INTO [Entity] ([Name]) VALUES (@Name);
    SET @Id = SCOPE_IDENTITY();
END
""");

        AssertSql(
            """
@p0='New1' (Size = 4000)
@p1='1' (Direction = Output)
@p2='New2' (Size = 4000)
@p3='2' (Direction = Output)

SET NOCOUNT ON;
EXEC [Entity_Insert] @p0, @p1 OUTPUT;
EXEC [Entity_Insert] @p2, @p3 OUTPUT;
""");
    }

    public override async Task Insert_with_result_column(bool async)
    {
        await Insert_with_result_column(
            async,
            """
CREATE PROCEDURE Entity_Insert(@Name varchar(max))
AS INSERT INTO [Entity] ([Name]) OUTPUT [Inserted].[Id] VALUES (@Name)
""");

        AssertSql(
            """
@p0='Foo' (Size = 4000)

SET NOCOUNT ON;
EXEC [Entity_Insert] @p0;
""");
    }

    public override async Task Insert_with_two_result_columns(bool async)
    {
        await Insert_with_two_result_columns(
            async,
            """
CREATE PROCEDURE EntityWithAdditionalProperty_Insert(@Name varchar(max))
AS INSERT INTO [EntityWithAdditionalProperty] ([Name]) OUTPUT [Inserted].[AdditionalProperty], [Inserted].[Id] VALUES (@Name)
""");

        AssertSql(
            """
@p0='Foo' (Size = 4000)

SET NOCOUNT ON;
EXEC [EntityWithAdditionalProperty_Insert] @p0;
""");
    }

    public override async Task Insert_with_output_parameter_and_result_column(bool async)
    {
        await Insert_with_output_parameter_and_result_column(
            async,
            """
CREATE PROCEDURE EntityWithAdditionalProperty_Insert(@Id int OUT, @Name varchar(max))
AS BEGIN
    INSERT INTO [EntityWithAdditionalProperty] ([Name]) VALUES (@Name);
    SET @Id = SCOPE_IDENTITY();
    SELECT [AdditionalProperty] FROM [EntityWithAdditionalProperty] WHERE [Id] = @Id
END
""");

        AssertSql(
            """
@p0=NULL (Nullable = false) (Direction = Output) (DbType = Int32)
@p1='Foo' (Size = 4000)

SET NOCOUNT ON;
EXEC [EntityWithAdditionalProperty_Insert] @p0 OUTPUT, @p1;
""");
    }

    public override async Task Update(bool async)
    {
        await Update(
            async,
            """
CREATE PROCEDURE Entity_Update(@Id int, @Name varchar(max))
AS UPDATE [Entity] SET [Name] = @Name WHERE [Id] = @id
""");

        AssertSql(
            """
@p0='1'
@p1='Updated' (Size = 4000)

SET NOCOUNT ON;
EXEC [Entity_Update] @p0, @p1;
""");
    }

    public override async Task Update_partial(bool async)
    {
        await Update_partial(
            async,
            """
CREATE PROCEDURE EntityWithAdditionalProperty_Update(@Id int, @Name varchar(max), @AdditionalProperty int)
AS UPDATE [EntityWithAdditionalProperty] SET [Name] = @Name, [AdditionalProperty] = @AdditionalProperty WHERE [Id] = @id
""");

        AssertSql(
            """
@p0='1'
@p1='Updated' (Size = 4000)
@p2='8'

SET NOCOUNT ON;
EXEC [EntityWithAdditionalProperty_Update] @p0, @p1, @p2;
""");
    }

    public override async Task Update_with_output_parameter_and_rows_affected_result_column(bool async)
    {
        await Update_with_output_parameter_and_rows_affected_result_column(
            async,
            """
CREATE PROCEDURE EntityWithAdditionalProperty_Update(@Id int, @Name varchar(max), @AdditionalProperty int OUT)
AS BEGIN
    UPDATE [EntityWithAdditionalProperty] SET [Name] = @Name, @AdditionalProperty = [AdditionalProperty] WHERE [Id] = @Id;
    SELECT @@ROWCOUNT;
END
""");

        AssertSql(
            """
@p0='1'
@p1='Updated' (Size = 4000)
@p2=NULL (Nullable = false) (Direction = Output) (DbType = Int32)

SET NOCOUNT ON;
EXEC [EntityWithAdditionalProperty_Update] @p0, @p1, @p2 OUTPUT;
""");
    }

    public override async Task Update_with_output_parameter_and_rows_affected_result_column_concurrency_failure(bool async)
    {
        await Update_with_output_parameter_and_rows_affected_result_column_concurrency_failure(
            async,
            """
CREATE PROCEDURE EntityWithAdditionalProperty_Update(@Id int, @Name varchar(max), @AdditionalProperty int OUT)
AS BEGIN
    UPDATE [EntityWithAdditionalProperty] SET [Name] = @Name, @AdditionalProperty = [AdditionalProperty] WHERE [Id] = @Id;
    SELECT @@ROWCOUNT;
END
""");

        AssertSql(
            """
@p0='1'
@p1='Updated' (Size = 4000)
@p2=NULL (Nullable = false) (Direction = Output) (DbType = Int32)

SET NOCOUNT ON;
EXEC [EntityWithAdditionalProperty_Update] @p0, @p1, @p2 OUTPUT;
""");
    }

    public override async Task Delete(bool async)
    {
        await Delete(
            async,
            """
CREATE PROCEDURE Entity_Delete(@Id int)
AS DELETE FROM [Entity] WHERE [Id] = @Id
""");

        AssertSql(
            """
@p0='1'

SET NOCOUNT ON;
EXEC [Entity_Delete] @p0;
""");
    }

    public override async Task Delete_and_insert(bool async)
    {
        await Delete_and_insert(
            async,
            """
CREATE PROCEDURE Entity_Insert(@Name varchar(max), @Id int OUT)
AS BEGIN
    INSERT INTO [Entity] ([Name]) VALUES (@Name);
    SET @Id = SCOPE_IDENTITY();
END;

GO;

CREATE PROCEDURE Entity_Delete(@Id int)
AS DELETE FROM [Entity] WHERE [Id] = @Id;
""");

        AssertSql(
            """
@p0='1'
@p1='Entity2' (Size = 4000)
@p2='2' (Direction = Output)

SET NOCOUNT ON;
EXEC [Entity_Delete] @p0;
EXEC [Entity_Insert] @p1, @p2 OUTPUT;
""");
    }

    public override async Task Rows_affected_parameter(bool async)
    {
        await Rows_affected_parameter(
            async,
            """
CREATE PROCEDURE Entity_Update(@Id int, @Name varchar(max), @RowsAffected int OUT)
AS BEGIN
    UPDATE [Entity] SET [Name] = @Name WHERE [Id] = @Id;
    SET @RowsAffected = @@ROWCOUNT;
END
""");

        AssertSql(
            """
@p0='1'
@p1='Updated' (Size = 4000)
@p2='1' (Direction = Output)

SET NOCOUNT ON;
EXEC [Entity_Update] @p0, @p1, @p2 OUTPUT;
""");
    }

    public override async Task Rows_affected_parameter_and_concurrency_failure(bool async)
    {
        await Rows_affected_parameter_and_concurrency_failure(
            async,
            """
CREATE PROCEDURE Entity_Update(@Id int, @Name varchar(max), @RowsAffected int OUT)
AS BEGIN
    UPDATE [Entity] SET [Name] = @Name WHERE [Id] = @Id;
    SET @RowsAffected = @@ROWCOUNT;
END
""");

        AssertSql(
            """
@p0='1'
@p1='Updated' (Size = 4000)
@p2='0' (Direction = Output)

SET NOCOUNT ON;
EXEC [Entity_Update] @p0, @p1, @p2 OUTPUT;
""");
    }

    public override async Task Rows_affected_result_column(bool async)
    {
        await Rows_affected_result_column(
            async,
            """
CREATE PROCEDURE Entity_Update(@Id int, @Name varchar(max))
AS BEGIN
    UPDATE [Entity] SET [Name] = @Name WHERE [Id] = @Id;
    SELECT @@ROWCOUNT;
END
""");

        AssertSql(
            """
@p0='1'
@p1='Updated' (Size = 4000)

SET NOCOUNT ON;
EXEC [Entity_Update] @p0, @p1;
""");
    }

    public override async Task Rows_affected_result_column_and_concurrency_failure(bool async)
    {
        await Rows_affected_result_column_and_concurrency_failure(
            async,
            """
CREATE PROCEDURE Entity_Update(@Id int, @Name varchar(max))
AS BEGIN
    UPDATE [Entity] SET [Name] = @Name WHERE [Id] = @Id;
    SELECT @@ROWCOUNT;
END
""");

        AssertSql(
            """
@p0='1'
@p1='Updated' (Size = 4000)

SET NOCOUNT ON;
EXEC [Entity_Update] @p0, @p1;
""");
    }

    public override async Task Rows_affected_return_value(bool async)
    {
        await Rows_affected_return_value(
            async,
            """
CREATE PROCEDURE Entity_Update(@Id int, @Name varchar(max))
AS BEGIN
    UPDATE [Entity] SET [Name] = @Name WHERE [Id] = @Id;
    RETURN @@ROWCOUNT;
END
""");

        AssertSql(
            """
@p0='1' (Direction = Output)
@p1='1'
@p2='Updated' (Size = 4000)

SET NOCOUNT ON;
EXEC @p0 = [Entity_Update] @p1, @p2;
""");
    }

    public override async Task Rows_affected_return_value_and_concurrency_failure(bool async)
    {
        await Rows_affected_return_value_and_concurrency_failure(
            async,
            """
CREATE PROCEDURE Entity_Update(@Id int, @Name varchar(max))
AS BEGIN
    UPDATE [Entity] SET [Name] = @Name WHERE [Id] = @Id;
    RETURN @@ROWCOUNT;
END
""");

        AssertSql(
            """
@p0='0' (Direction = Output)
@p1='1'
@p2='Updated' (Size = 4000)

SET NOCOUNT ON;
EXEC @p0 = [Entity_Update] @p1, @p2;
""");
    }

    public override async Task Store_generated_concurrency_token_as_in_out_parameter(bool async)
    {
        await Store_generated_concurrency_token_as_in_out_parameter(
            async,
            """
CREATE PROCEDURE Entity_Update(@Id int, @ConcurrencyToken rowversion OUT, @Name varchar(max), @RowsAffected int OUT)
AS BEGIN
    UPDATE [Entity] SET [Name] = @Name WHERE [Id] = @Id AND [ConcurrencyToken] = @ConcurrencyToken;
    SET @RowsAffected = @@ROWCOUNT;
END
""");

        // Can't assert SQL baseline as usual because the concurrency token changes
        Assert.Contains("(Size = 8) (Direction = InputOutput)", TestSqlLoggerFactory.Sql);

        Assert.Equal(
            @"@p2='Updated' (Size = 4000)
@p3='0' (Direction = Output)

SET NOCOUNT ON;
EXEC [Entity_Update] @p0, @p1 OUTPUT, @p2, @p3 OUTPUT;",
            TestSqlLoggerFactory.Sql.Substring(TestSqlLoggerFactory.Sql.IndexOf("@p2", StringComparison.Ordinal)),
            ignoreLineEndingDifferences: true);
    }

    public override async Task Store_generated_concurrency_token_as_two_parameters(bool async)
    {
        await Store_generated_concurrency_token_as_two_parameters(
            async,
            """
CREATE PROCEDURE Entity_Update(@Id int, @ConcurrencyTokenIn rowversion, @Name varchar(max), @ConcurrencyTokenOut rowversion OUT, @RowsAffected int OUT)
AS BEGIN
    UPDATE [Entity] SET [Name] = @Name, @ConcurrencyTokenOut = [ConcurrencyToken] WHERE [Id] = @Id AND [ConcurrencyToken] = @ConcurrencyTokenIn;
    SET @RowsAffected = @@ROWCOUNT;
END
""");

        // Can't assert SQL baseline as usual because the concurrency token changes
        Assert.Equal(
            @"@p2='Updated' (Size = 4000)
@p3=NULL (Size = 8) (Direction = Output) (DbType = Binary)
@p4='0' (Direction = Output)

SET NOCOUNT ON;
EXEC [Entity_Update] @p0, @p1, @p2, @p3 OUTPUT, @p4 OUTPUT;",
            TestSqlLoggerFactory.Sql.Substring(TestSqlLoggerFactory.Sql.IndexOf("@p2", StringComparison.Ordinal)),
            ignoreLineEndingDifferences: true);
    }

    public override async Task User_managed_concurrency_token(bool async)
    {
        await User_managed_concurrency_token(
            async,
            """
CREATE PROCEDURE EntityWithAdditionalProperty_Update(@Id int, @ConcurrencyTokenOriginal int, @Name varchar(max), @ConcurrencyTokenCurrent int, @RowsAffected int OUT)
AS BEGIN
    UPDATE [EntityWithAdditionalProperty] SET [Name] = @Name, [AdditionalProperty] = @ConcurrencyTokenCurrent WHERE [Id] = @Id AND [AdditionalProperty] = @ConcurrencyTokenOriginal;
    SET @RowsAffected = @@ROWCOUNT;
END
""");

        AssertSql(
            """
@p0='1'
@p1='8'
@p2='Updated' (Size = 4000)
@p3='9'
@p4='0' (Direction = Output)

SET NOCOUNT ON;
EXEC [EntityWithAdditionalProperty_Update] @p0, @p1, @p2, @p3, @p4 OUTPUT;
""");
    }

    public override async Task Original_and_current_value_on_non_concurrency_token(bool async)
    {
        await Original_and_current_value_on_non_concurrency_token(
            async,
            """
CREATE PROCEDURE Entity_Update(@Id int, @NameCurrent varchar(max), @NameOriginal varchar(max))
AS BEGIN
    IF @NameCurrent <> @NameOriginal
    BEGIN
        UPDATE [Entity] SET [Name] = @NameCurrent WHERE [Id] = @Id;
    END
END
""");

        AssertSql(
            """
@p0='1'
@p1='Updated' (Size = 4000)
@p2='Initial' (Size = 4000)

SET NOCOUNT ON;
EXEC [Entity_Update] @p0, @p1, @p2;
""");
    }

    public override async Task Input_or_output_parameter_with_input(bool async)
    {
        await Input_or_output_parameter_with_input(
            async,
            """
CREATE PROCEDURE Entity_Insert(@Id int OUT, @Name varchar(max) OUT)
AS BEGIN
    IF @Name IS NULL
    BEGIN
        INSERT INTO [Entity] ([Name]) VALUES ('Some default value');
        SET @Name = 'Some default value';
    END
    ELSE
    BEGIN
        INSERT INTO [Entity] ([Name]) VALUES (@Name);
        SET @Name = NULL;
    END

    SET @Id = SCOPE_IDENTITY();
END
""");

        AssertSql(
            """
@p0='1' (Direction = Output)
@p1=NULL (Nullable = false) (Size = 4000) (Direction = InputOutput)

SET NOCOUNT ON;
EXEC [Entity_Insert] @p0 OUTPUT, @p1 OUTPUT;
""");
    }

    public override async Task Input_or_output_parameter_with_output(bool async)
    {
        await Input_or_output_parameter_with_output(
            async,
            """
CREATE PROCEDURE Entity_Insert(@Id int OUT, @Name varchar(max) OUT)
AS BEGIN
    IF @Name IS NULL
    BEGIN
        INSERT INTO [Entity] ([Name]) VALUES ('Some default value');
        SET @Name = 'Some default value';
    END
    ELSE
    BEGIN
        INSERT INTO [Entity] ([Name]) VALUES (@Name);
        SET @Name = NULL;
    END

    SET @Id = SCOPE_IDENTITY();
END
""");

        AssertSql(
            """
@p0='1' (Direction = Output)
@p1='Some default value' (Nullable = false) (Size = 4000) (Direction = InputOutput)

SET NOCOUNT ON;
EXEC [Entity_Insert] @p0 OUTPUT, @p1 OUTPUT;
""");
    }

    public override async Task Tph(bool async)
    {
        await Tph(
            async,
            """
CREATE PROCEDURE Tph_Insert(@Id int OUT, @Discriminator varchar(max), @Name varchar(max), @Child2InputProperty int, @Child2OutputParameterProperty int OUT, @Child1Property int)
AS BEGIN
    DECLARE @Table table ([Child2OutputParameterProperty] int);
    INSERT INTO [Tph] ([Discriminator], [Name], [Child1Property], [Child2InputProperty]) OUTPUT [Inserted].[Child2OutputParameterProperty] INTO @Table VALUES (@Discriminator, @Name, @Child1Property, @Child2InputProperty);
    SET @Id = SCOPE_IDENTITY();
    SELECT @Child2OutputParameterProperty = [Child2OutputParameterProperty] FROM @Table;
    SELECT [Child2ResultColumnProperty] FROM [Tph] WHERE [Id] = @Id
END
""");

        AssertSql(
            """
@p0=NULL (Nullable = false) (Direction = Output) (DbType = Int32)
@p1='Child1' (Nullable = false) (Size = 8)
@p2='Child' (Size = 4000)
@p3=NULL (DbType = Int32)
@p4=NULL (Direction = Output) (DbType = Int32)
@p5='8' (Nullable = true)

SET NOCOUNT ON;
EXEC [Tph_Insert] @p0 OUTPUT, @p1, @p2, @p3, @p4 OUTPUT, @p5;
""");
    }

    public override async Task Tpt(bool async)
    {
        await Tpt(
            async,
            """
CREATE PROCEDURE Parent_Insert(@Id int OUT, @Name varchar(max))
AS BEGIN
    INSERT INTO [Parent] ([Name]) VALUES (@Name);
    SET @Id = SCOPE_IDENTITY();
END;

GO

CREATE PROCEDURE Child1_Insert(@Id int, @Child1Property int)
AS BEGIN
    INSERT INTO [Child1] ([Id], [Child1Property]) VALUES (@Id, @Child1Property);
    SET @Id = SCOPE_IDENTITY();
END;
""");

        AssertSql(
            """
@p0='1' (Direction = Output)
@p1='Child' (Size = 4000)

SET NOCOUNT ON;
EXEC [Parent_Insert] @p0 OUTPUT, @p1;
""",
            //
            """
@p2='1'
@p3='8'

SET NOCOUNT ON;
EXEC [Child1_Insert] @p2, @p3;
""");
    }

    public override async Task Tpt_mixed_sproc_and_non_sproc(bool async)
    {
        await Tpt_mixed_sproc_and_non_sproc(
            async,
            """
CREATE PROCEDURE Parent_Insert(@Id int OUT, @Name varchar(max))
AS BEGIN
    INSERT INTO [Parent] ([Name]) VALUES (@Name);
    SET @Id = SCOPE_IDENTITY();
END
""");

        AssertSql(
            """
@p0='1' (Direction = Output)
@p1='Child' (Size = 4000)

SET NOCOUNT ON;
EXEC [Parent_Insert] @p0 OUTPUT, @p1;
""",
            //
            """
@p2='1'
@p3='8'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [Child1] ([Id], [Child1Property])
VALUES (@p2, @p3);
""");
    }

    public override async Task Tpc(bool async)
    {
        await Tpc(
            async,
            """
CREATE PROCEDURE Child1_Insert(@Id int OUT, @Name varchar(max), @Child1Property int)
AS BEGIN
    DECLARE @Table table ([Id] int);
    INSERT INTO [Child1] ([Name], [Child1Property]) OUTPUT [Inserted].[Id] INTO @Table VALUES (@Name, @Child1Property);
    SELECT @Id = [Id] FROM @Table;
END
""");

        AssertSql(
            """
@p0='1' (Direction = Output)
@p1='Child' (Size = 4000)
@p2='8'

SET NOCOUNT ON;
EXEC [Child1_Insert] @p0 OUTPUT, @p1, @p2;
""");
    }

    public override async Task Non_sproc_followed_by_sproc_commands_in_the_same_batch(bool async)
    {
        await Non_sproc_followed_by_sproc_commands_in_the_same_batch(
            async,
            """
CREATE PROCEDURE EntityWithAdditionalProperty_Insert(@Name varchar(max), @Id int OUT, @AdditionalProperty int)
AS BEGIN
    INSERT INTO [EntityWithAdditionalProperty] ([Name], [AdditionalProperty]) VALUES (@Name, @AdditionalProperty);
    SET @Id = SCOPE_IDENTITY();
END
""");

        AssertSql(
            """
@p2='1'
@p0='2'
@p3='1'
@p1='Entity1_Modified' (Size = 4000)
@p4='Entity2' (Size = 4000)
@p5=NULL (Nullable = false) (Direction = Output) (DbType = Int32)
@p6='0'

SET NOCOUNT ON;
UPDATE [EntityWithAdditionalProperty] SET [AdditionalProperty] = @p0, [Name] = @p1
OUTPUT 1
WHERE [Id] = @p2 AND [AdditionalProperty] = @p3;
EXEC [EntityWithAdditionalProperty_Insert] @p4, @p5 OUTPUT, @p6;
""");
    }

    protected override void ConfigureStoreGeneratedConcurrencyToken(EntityTypeBuilder entityTypeBuilder, string propertyName)
        => entityTypeBuilder.Property<byte[]>(propertyName).IsRowVersion();

    protected override ITestStoreFactory TestStoreFactory
        => SqlServerTestStoreFactory.Instance;
}

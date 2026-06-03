// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class DataAnnotationSqliteTest : DataAnnotationRelationalTestBase<DataAnnotationSqliteTest.DataAnnotationSqliteFixture>
{
    public DataAnnotationSqliteTest(DataAnnotationSqliteFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
        fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    protected override TestHelpers TestHelpers
        => SqliteTestHelpers.Instance;

    public override IModel Non_public_annotations_are_enabled()
    {
        var model = base.Non_public_annotations_are_enabled();

        var relational = GetProperty<PrivateMemberAnnotationClass>(model, "PersonFirstName");
        Assert.Equal("dsdsd", relational.GetColumnName());
        Assert.Equal("nvarchar(128)", relational.GetColumnType());

        return model;
    }

    public override IModel Field_annotations_are_enabled()
    {
        var model = base.Field_annotations_are_enabled();

        var relational = GetProperty<FieldAnnotationClass>(model, "_personFirstName");
        Assert.Equal("dsdsd", relational.GetColumnName());
        Assert.Equal("nvarchar(128)", relational.GetColumnType());

        return model;
    }

    public override IModel Key_and_column_work_together()
    {
        var model = base.Key_and_column_work_together();

        var relational = GetProperty<ColumnKeyAnnotationClass1>(model, "PersonFirstName");
        Assert.Equal("dsdsd", relational.GetColumnName());
        Assert.Equal("nvarchar(128)", relational.GetColumnType());

        return model;
    }

    public override IModel Key_and_MaxLength_64_produce_nvarchar_64()
    {
        var model = base.Key_and_MaxLength_64_produce_nvarchar_64();

        var property = GetProperty<ColumnKeyAnnotationClass2>(model, "PersonFirstName");

        var storeType = property.GetRelationalTypeMapping().StoreType;

        Assert.Equal("TEXT", storeType);

        return model;
    }

    public override IModel Timestamp_takes_precedence_over_MaxLength()
    {
        var model = base.Timestamp_takes_precedence_over_MaxLength();

        var property = GetProperty<TimestampAndMaxlength>(model, "MaxTimestamp");

        var storeType = property.GetRelationalTypeMapping().StoreType;

        Assert.Equal("BLOB", storeType);

        return model;
    }

    public override IModel TableNameAttribute_affects_table_name_in_TPH()
    {
        var model = base.TableNameAttribute_affects_table_name_in_TPH();

        var relational = model.FindEntityType(typeof(TNAttrBase));
        Assert.Equal("A", relational.GetTableName());

        return model;
    }

    public override async Task ConcurrencyCheckAttribute_throws_if_value_in_database_changed()
    {
        await base.ConcurrencyCheckAttribute_throws_if_value_in_database_changed();

        AssertSql(
            """
SELECT "s"."Unique_No", "s"."MaxLengthProperty", "s"."Name", "s"."RowVersion", "s"."AdditionalDetails_Name", "s"."AdditionalDetails_Value", "s"."Details_Name", "s"."Details_Value"
FROM "Sample" AS "s"
WHERE "s"."Unique_No" = 1
LIMIT 1
""",
            //
            """
SELECT "s"."Unique_No", "s"."MaxLengthProperty", "s"."Name", "s"."RowVersion", "s"."AdditionalDetails_Name", "s"."AdditionalDetails_Value", "s"."Details_Name", "s"."Details_Value"
FROM "Sample" AS "s"
WHERE "s"."Unique_No" = 1
LIMIT 1
""",
            //
            """
@p2='1'
@p0='ModifiedData' (Nullable = false) (Size = 12)
@p1='00000000-0000-0000-0003-000000000001'
@p3='00000001-0000-0000-0000-000000000001'

UPDATE "Sample" SET "Name" = @p0, "RowVersion" = @p1
WHERE "Unique_No" = @p2 AND "RowVersion" = @p3
RETURNING 1;
""",
            //
            """
@p2='1'
@p0='ChangedData' (Nullable = false) (Size = 11)
@p1='00000000-0000-0000-0002-000000000001'
@p3='00000001-0000-0000-0000-000000000001'

UPDATE "Sample" SET "Name" = @p0, "RowVersion" = @p1
WHERE "Unique_No" = @p2 AND "RowVersion" = @p3
RETURNING 1;
""");
    }

    public override async Task DatabaseGeneratedAttribute_autogenerates_values_when_set_to_identity()
    {
        await base.DatabaseGeneratedAttribute_autogenerates_values_when_set_to_identity();

        AssertSql(
            """
@p0=NULL
@p1='Third' (Nullable = false) (Size = 5)
@p2='00000000-0000-0000-0000-000000000003'
@p3='Third Additional Name' (Size = 21)
@p4='0' (Nullable = true)
@p5='Third Name' (Size = 10)
@p6='0' (Nullable = true)

INSERT INTO "Sample" ("MaxLengthProperty", "Name", "RowVersion", "AdditionalDetails_Name", "AdditionalDetails_Value", "Details_Name", "Details_Value")
VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6)
RETURNING "Unique_No";
""");
    }

    // Sqlite does not support length
    public override Task MaxLengthAttribute_throws_while_inserting_value_longer_than_max_length()
    {
        using var context = CreateContext();
        Assert.Equal(10, context.Model.FindEntityType(typeof(One)).FindProperty("MaxLengthProperty").GetMaxLength());
        return Task.CompletedTask;
    }

    // Sqlite does not support length
    public override Task StringLengthAttribute_throws_while_inserting_value_longer_than_max_length()
    {
        using var context = CreateContext();
        Assert.Equal(16, context.Model.FindEntityType(typeof(Two)).FindProperty("Data").GetMaxLength());
        return Task.CompletedTask;
    }

    // Sqlite does not support rowversion. See issue #2195
    public override Task TimestampAttribute_throws_if_value_in_database_changed()
    {
        using var context = CreateContext();
        Assert.True(context.Model.FindEntityType(typeof(Two)).FindProperty("Timestamp").IsConcurrencyToken);
        return Task.CompletedTask;
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class DataAnnotationSqliteFixture : DataAnnotationRelationalFixtureBase, ITestSqlLoggerFactory
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;
    }
}

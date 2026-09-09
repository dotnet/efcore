// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Diagnostics.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class DataAnnotationSqlServerTest : DataAnnotationRelationalTestBase<DataAnnotationSqlServerTest.DataAnnotationSqlServerFixture>
{
    public DataAnnotationSqlServerTest(DataAnnotationSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
        fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    protected override TestHelpers TestHelpers
        => SqlServerTestHelpers.Instance;

    [ConditionalFact]
    public virtual void Default_for_key_string_column_throws()
    {
        var modelBuilder = CreateModelBuilder();

        modelBuilder.Entity<Login1>().Property(l => l.UserName).HasDefaultValue("default");
        modelBuilder.Ignore<Profile1>();

        Assert.Equal(
            CoreStrings.WarningAsErrorTemplate(
                RelationalEventId.ModelValidationKeyDefaultValueWarning,
                RelationalResources.LogKeyHasDefaultValue(new TestLogger<SqlServerLoggingDefinitions>())
                    .GenerateMessage(nameof(Login1.UserName), nameof(Login1)),
                "RelationalEventId.ModelValidationKeyDefaultValueWarning"),
            Assert.Throws<InvalidOperationException>(() => Validate(modelBuilder)).Message);
    }

    [ConditionalFact]
    public virtual void Default_for_key_which_is_also_an_fk_column_does_not_throw()
    {
        var modelBuilder = CreateModelBuilder();

        modelBuilder.Entity<PrincipalA>();
        modelBuilder.Entity<DependantA>(
            b =>
            {
                b.HasKey(e => new { e.Id, e.PrincipalId });
                b.Property(e => e.PrincipalId).HasDefaultValue(77);
            });

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Default_for_part_of_composite_key_does_not_throw()
    {
        var modelBuilder = CreateModelBuilder();

        modelBuilder.Entity<PrincipalB>(
            b =>
            {
                b.HasKey(e => new { e.Id1, e.Id2 });
                b.Property(e => e.Id1).HasDefaultValue(77);
            });

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Default_for_all_parts_of_composite_key_throws()
    {
        var modelBuilder = CreateModelBuilder();

        modelBuilder.Entity<PrincipalB>(
            b =>
            {
                b.HasKey(e => new { e.Id1, e.Id2 });
                b.Property(e => e.Id1).HasDefaultValue(77);
                b.Property(e => e.Id2).HasDefaultValue(78);
            });

        Assert.Equal(
            CoreStrings.WarningAsErrorTemplate(
                RelationalEventId.ModelValidationKeyDefaultValueWarning,
                RelationalResources.LogKeyHasDefaultValue(new TestLogger<SqlServerLoggingDefinitions>())
                    .GenerateMessage(nameof(PrincipalB.Id1), nameof(PrincipalB)),
                "RelationalEventId.ModelValidationKeyDefaultValueWarning"),
            Assert.Throws<InvalidOperationException>(() => Validate(modelBuilder)).Message);
    }

    public override IModel Non_public_annotations_are_enabled()
    {
        var model = base.Non_public_annotations_are_enabled();

        var property = GetProperty<PrivateMemberAnnotationClass>(model, "PersonFirstName");
        Assert.Equal("dsdsd", property.GetColumnName());
        Assert.Equal("nvarchar(128)", property.GetColumnType());

        return model;
    }

    public override IModel Field_annotations_are_enabled()
    {
        var model = base.Field_annotations_are_enabled();

        var property = GetProperty<FieldAnnotationClass>(model, "_personFirstName");
        Assert.Equal("dsdsd", property.GetColumnName());
        Assert.Equal("nvarchar(128)", property.GetColumnType());

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

        Assert.Equal("nvarchar(64)", storeType);

        return model;
    }

    public override IModel Timestamp_takes_precedence_over_MaxLength()
    {
        var model = base.Timestamp_takes_precedence_over_MaxLength();

        var property = GetProperty<TimestampAndMaxlength>(model, "MaxTimestamp");

        var storeType = property.GetRelationalTypeMapping().StoreType;

        Assert.Equal("rowversion", storeType);

        return model;
    }

    public override IModel TableNameAttribute_affects_table_name_in_TPH()
    {
        var model = base.TableNameAttribute_affects_table_name_in_TPH();

        Assert.Equal("A", model.FindEntityType(typeof(TNAttrBase)).GetTableName());

        return model;
    }

    public override IModel DatabaseGeneratedOption_configures_the_property_correctly()
    {
        var model = base.DatabaseGeneratedOption_configures_the_property_correctly();

        var identity = model.FindEntityType(typeof(GeneratedEntity)).FindProperty(nameof(GeneratedEntity.Identity));
        Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn, identity.GetValueGenerationStrategy());

        return model;
    }

    [ConditionalFact]
    public virtual void ColumnAttribute_configures_the_property_correctly()
    {
        var modelBuilder = CreateModelBuilder();
        modelBuilder.Entity<One>().HasKey(o => o.UniqueNo);

        var model = modelBuilder.FinalizeModel();

        Assert.Equal(
            "Unique_No",
            model.FindEntityType(typeof(One)).FindProperty(nameof(One.UniqueNo)).GetColumnName());
    }

    public override IModel DatabaseGeneratedOption_Identity_does_not_throw_on_noninteger_properties()
    {
        var model = base.DatabaseGeneratedOption_Identity_does_not_throw_on_noninteger_properties();

        var entity = model.FindEntityType(typeof(GeneratedEntityNonInteger));

        var stringProperty = entity.FindProperty(nameof(GeneratedEntityNonInteger.String));
        Assert.Equal(SqlServerValueGenerationStrategy.None, stringProperty.GetValueGenerationStrategy());

        var dateTimeProperty = entity.FindProperty(nameof(GeneratedEntityNonInteger.DateTime));
        Assert.Equal(SqlServerValueGenerationStrategy.None, dateTimeProperty.GetValueGenerationStrategy());

        var guidProperty = entity.FindProperty(nameof(GeneratedEntityNonInteger.Guid));
        Assert.Equal(SqlServerValueGenerationStrategy.None, guidProperty.GetValueGenerationStrategy());

        return model;
    }

    public override async Task ConcurrencyCheckAttribute_throws_if_value_in_database_changed()
    {
        await base.ConcurrencyCheckAttribute_throws_if_value_in_database_changed();

        AssertSql(
            """
SELECT TOP(1) [s].[Unique_No], [s].[MaxLengthProperty], [s].[Name], [s].[RowVersion], [s].[AdditionalDetails_Name], [s].[AdditionalDetails_Value], [s].[Details_Name], [s].[Details_Value]
FROM [Sample] AS [s]
WHERE [s].[Unique_No] = 1
""",
            //
            """
SELECT TOP(1) [s].[Unique_No], [s].[MaxLengthProperty], [s].[Name], [s].[RowVersion], [s].[AdditionalDetails_Name], [s].[AdditionalDetails_Value], [s].[Details_Name], [s].[Details_Value]
FROM [Sample] AS [s]
WHERE [s].[Unique_No] = 1
""",
            //
            """
@p2='1'
@p0='ModifiedData' (Nullable = false) (Size = 4000)
@p1='00000000-0000-0000-0003-000000000001'
@p3='00000001-0000-0000-0000-000000000001'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [Sample] SET [Name] = @p0, [RowVersion] = @p1
OUTPUT 1
WHERE [Unique_No] = @p2 AND [RowVersion] = @p3;
""",
            //
            """
@p2='1'
@p0='ChangedData' (Nullable = false) (Size = 4000)
@p1='00000000-0000-0000-0002-000000000001'
@p3='00000001-0000-0000-0000-000000000001'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [Sample] SET [Name] = @p0, [RowVersion] = @p1
OUTPUT 1
WHERE [Unique_No] = @p2 AND [RowVersion] = @p3;
""");
    }

    public override async Task DatabaseGeneratedAttribute_autogenerates_values_when_set_to_identity()
    {
        await base.DatabaseGeneratedAttribute_autogenerates_values_when_set_to_identity();

        AssertSql(
            """
@p0=NULL (Size = 10)
@p1='Third' (Nullable = false) (Size = 4000)
@p2='00000000-0000-0000-0000-000000000003'
@p3='Third Additional Name' (Size = 4000)
@p4='0' (Nullable = true)
@p5='Third Name' (Size = 4000)
@p6='0' (Nullable = true)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [Sample] ([MaxLengthProperty], [Name], [RowVersion], [AdditionalDetails_Name], [AdditionalDetails_Value], [Details_Name], [Details_Value])
OUTPUT INSERTED.[Unique_No]
VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6);
""");
    }

    public override async Task MaxLengthAttribute_throws_while_inserting_value_longer_than_max_length()
    {
        await base.MaxLengthAttribute_throws_while_inserting_value_longer_than_max_length();

        AssertSql(
            """
@p0='Short' (Size = 10)
@p1='ValidString' (Nullable = false) (Size = 4000)
@p2='00000000-0000-0000-0000-000000000001'
@p3='Third Additional Name' (Size = 4000)
@p4='0' (Nullable = true)
@p5='Third Name' (Size = 4000)
@p6='0' (Nullable = true)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [Sample] ([MaxLengthProperty], [Name], [RowVersion], [AdditionalDetails_Name], [AdditionalDetails_Value], [Details_Name], [Details_Value])
OUTPUT INSERTED.[Unique_No]
VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6);
""",
            //
            """
@p0='VeryVeryVeryVeryVeryVeryLongString' (Size = 4000)
@p1='ValidString' (Nullable = false) (Size = 4000)
@p2='00000000-0000-0000-0000-000000000002'
@p3='Third Additional Name' (Size = 4000)
@p4='0' (Nullable = true)
@p5='Third Name' (Size = 4000)
@p6='0' (Nullable = true)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [Sample] ([MaxLengthProperty], [Name], [RowVersion], [AdditionalDetails_Name], [AdditionalDetails_Value], [Details_Name], [Details_Value])
OUTPUT INSERTED.[Unique_No]
VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6);
""");
    }

    public override async Task StringLengthAttribute_throws_while_inserting_value_longer_than_max_length()
    {
        await base.StringLengthAttribute_throws_while_inserting_value_longer_than_max_length();

        AssertSql(
            """
@p0='ValidString' (Size = 16)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [Two] ([Data])
OUTPUT INSERTED.[Id], INSERTED.[Timestamp]
VALUES (@p0);
""",
            //
            """
@p0='ValidButLongString' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [Two] ([Data])
OUTPUT INSERTED.[Id], INSERTED.[Timestamp]
VALUES (@p0);
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class DataAnnotationSqlServerFixture : DataAnnotationRelationalFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;
    }
}

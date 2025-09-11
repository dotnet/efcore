// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Types.Temporal;

public class DateTimeTypeTest(DateTimeTypeTest.DateTimeTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : RelationalTypeTestBase<DateTime, DateTimeTypeTest.DateTimeTypeFixture>(fixture, testOutputHelper)
{
    public class DateTimeTypeFixture : SqlServerTypeFixture<DateTime>
    {
        public override DateTime Value { get; } = new DateTime(2020, 1, 5, 12, 30, 45, DateTimeKind.Unspecified);
        public override DateTime OtherValue { get; } = new DateTime(2022, 5, 3, 0, 0, 0, DateTimeKind.Unspecified);

        protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;
    }

    public override async Task SaveChanges_within_json()
    {
        await base.SaveChanges_within_json();

        AssertSql(
            """
@p0='{"OtherValue":"2022-05-03T00:00:00","Value":"2022-05-03T00:00:00"}' (Nullable = false) (Size = 66)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonTypeEntity] SET [JsonContainer] = @p0
OUTPUT 1
WHERE [Id] = @p1;
""");
    }

    public override async Task ExecuteUpdate_within_json_to_constant()
    {
        await base.ExecuteUpdate_within_json_to_constant();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
UPDATE [j]
SET [JsonContainer].modify('$.Value', N'2022-05-03T00:00:00')
FROM [JsonTypeEntity] AS [j]
""");
        }
        else
        {
            AssertSql(
                """
UPDATE [j]
SET [j].[JsonContainer] = JSON_MODIFY([j].[JsonContainer], '$.Value', N'2022-05-03T00:00:00')
FROM [JsonTypeEntity] AS [j]
""");
        }
    }

    [SqlServerCondition(SqlServerCondition.SupportsFunctions2022)]
    public override async Task ExecuteUpdate_within_json_to_nonjson_column()
    {
        // TODO: Currently failing on Helix only, see #36746
        if (Environment.GetEnvironmentVariable("HELIX_WORKITEM_ROOT") is not null)
        {
            return;
        }

        await base.ExecuteUpdate_within_json_to_nonjson_column();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
UPDATE [j]
SET [JsonContainer].modify('$.Value', JSON_VALUE(JSON_OBJECT('v': [j].[OtherValue]), '$.v'))
FROM [JsonTypeEntity] AS [j]
""");
        }
        else
        {
            AssertSql(
                """
UPDATE [j]
SET [j].[JsonContainer] = JSON_MODIFY([j].[JsonContainer], '$.Value', JSON_VALUE(JSON_OBJECT('v': [j].[OtherValue]), '$.v'))
FROM [JsonTypeEntity] AS [j]
""");
        }
    }
}

public class DateTimeOffsetTypeTest(DateTimeOffsetTypeTest.DateTimeOffsetTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : RelationalTypeTestBase<DateTimeOffset, DateTimeOffsetTypeTest.DateTimeOffsetTypeFixture>(fixture, testOutputHelper)
{
    public override async Task SaveChanges_within_json()
    {
        await base.SaveChanges_within_json();

        AssertSql(
            """
@p0='{"OtherValue":"2020-01-05T12:30:45+03:00","Value":"2020-01-05T12:30:45+03:00"}' (Nullable = false) (Size = 78)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonTypeEntity] SET [JsonContainer] = @p0
OUTPUT 1
WHERE [Id] = @p1;
""");
    }

    public override async Task ExecuteUpdate_within_json_to_constant()
    {
        await base.ExecuteUpdate_within_json_to_constant();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
UPDATE [j]
SET [JsonContainer].modify('$.Value', N'2020-01-05T12:30:45+03:00')
FROM [JsonTypeEntity] AS [j]
""");
        }
        else
        {
            AssertSql(
                """
UPDATE [j]
SET [j].[JsonContainer] = JSON_MODIFY([j].[JsonContainer], '$.Value', N'2020-01-05T12:30:45+03:00')
FROM [JsonTypeEntity] AS [j]
""");
        }
    }

    [SqlServerCondition(SqlServerCondition.SupportsFunctions2022)]
    public override async Task ExecuteUpdate_within_json_to_nonjson_column()
    {
        // TODO: Currently failing on Helix only, see #36746
        if (Environment.GetEnvironmentVariable("HELIX_WORKITEM_ROOT") is not null)
        {
            return;
        }

        await base.ExecuteUpdate_within_json_to_nonjson_column();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
UPDATE [j]
SET [JsonContainer].modify('$.Value', JSON_VALUE(JSON_OBJECT('v': [j].[OtherValue]), '$.v'))
FROM [JsonTypeEntity] AS [j]
""");
        }
        else
        {
            AssertSql(
                """
UPDATE [j]
SET [j].[JsonContainer] = JSON_MODIFY([j].[JsonContainer], '$.Value', JSON_VALUE(JSON_OBJECT('v': [j].[OtherValue]), '$.v'))
FROM [JsonTypeEntity] AS [j]
""");
        }
    }

    public class DateTimeOffsetTypeFixture : SqlServerTypeFixture<DateTimeOffset>
    {
        public override DateTimeOffset Value { get; } = new DateTimeOffset(2020, 1, 5, 12, 30, 45, TimeSpan.FromHours(2));
        public override DateTimeOffset OtherValue { get; } = new DateTimeOffset(2020, 1, 5, 12, 30, 45, TimeSpan.FromHours(3));

        protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;
    }
}

public class DateOnlyTypeTest(DateOnlyTypeTest.DateTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : RelationalTypeTestBase<DateOnly, DateOnlyTypeTest.DateTypeFixture>(fixture, testOutputHelper)
{
    public override async Task SaveChanges_within_json()
    {
        await base.SaveChanges_within_json();

        AssertSql(
            """
@p0='{"OtherValue":"2022-05-03","Value":"2022-05-03"}' (Nullable = false) (Size = 48)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonTypeEntity] SET [JsonContainer] = @p0
OUTPUT 1
WHERE [Id] = @p1;
""");
    }

    public override async Task ExecuteUpdate_within_json_to_constant()
    {
        await base.ExecuteUpdate_within_json_to_constant();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
UPDATE [j]
SET [JsonContainer].modify('$.Value', N'2022-05-03')
FROM [JsonTypeEntity] AS [j]
""");
        }
        else
        {
            AssertSql(
                """
UPDATE [j]
SET [j].[JsonContainer] = JSON_MODIFY([j].[JsonContainer], '$.Value', N'2022-05-03')
FROM [JsonTypeEntity] AS [j]
""");
        }
    }

    [SqlServerCondition(SqlServerCondition.SupportsFunctions2022)]
    public override async Task ExecuteUpdate_within_json_to_nonjson_column()
    {
        // TODO: Currently failing on Helix only, see #36746
        if (Environment.GetEnvironmentVariable("HELIX_WORKITEM_ROOT") is not null)
        {
            return;
        }

        await base.ExecuteUpdate_within_json_to_nonjson_column();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
UPDATE [j]
SET [JsonContainer].modify('$.Value', JSON_VALUE(JSON_OBJECT('v': [j].[OtherValue]), '$.v'))
FROM [JsonTypeEntity] AS [j]
""");
        }
        else
        {
            AssertSql(
                """
UPDATE [j]
SET [j].[JsonContainer] = JSON_MODIFY([j].[JsonContainer], '$.Value', JSON_VALUE(JSON_OBJECT('v': [j].[OtherValue]), '$.v'))
FROM [JsonTypeEntity] AS [j]
""");
        }
    }

    public class DateTypeFixture : SqlServerTypeFixture<DateOnly>
    {
        public override DateOnly Value { get; } = new DateOnly(2020, 1, 5);
        public override DateOnly OtherValue { get; } = new DateOnly(2022, 5, 3);

        protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;
    }
}

public class TimeOnlyTypeTest(TimeOnlyTypeTest.TimeOnlyTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : RelationalTypeTestBase<TimeOnly, TimeOnlyTypeTest.TimeOnlyTypeFixture>(fixture, testOutputHelper)
{
    public override async Task SaveChanges_within_json()
    {
        await base.SaveChanges_within_json();

        AssertSql(
            """
@p0='{"OtherValue":"14:00:00.0000000","Value":"14:00:00.0000000"}' (Nullable = false) (Size = 60)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonTypeEntity] SET [JsonContainer] = @p0
OUTPUT 1
WHERE [Id] = @p1;
""");
    }

    public override async Task ExecuteUpdate_within_json_to_constant()
    {
        await base.ExecuteUpdate_within_json_to_constant();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
UPDATE [j]
SET [JsonContainer].modify('$.Value', N'14:00:00.0000000')
FROM [JsonTypeEntity] AS [j]
""");
        }
        else
        {
            AssertSql(
                """
UPDATE [j]
SET [j].[JsonContainer] = JSON_MODIFY([j].[JsonContainer], '$.Value', N'14:00:00.0000000')
FROM [JsonTypeEntity] AS [j]
""");
        }
    }

    [SqlServerCondition(SqlServerCondition.SupportsFunctions2022)]
    public override async Task ExecuteUpdate_within_json_to_nonjson_column()
    {
        // TODO: Currently failing on Helix only, see #36746
        if (Environment.GetEnvironmentVariable("HELIX_WORKITEM_ROOT") is not null)
        {
            return;
        }

        await base.ExecuteUpdate_within_json_to_nonjson_column();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
UPDATE [j]
SET [JsonContainer].modify('$.Value', JSON_VALUE(JSON_OBJECT('v': [j].[OtherValue]), '$.v'))
FROM [JsonTypeEntity] AS [j]
""");
        }
        else
        {
            AssertSql(
                """
UPDATE [j]
SET [j].[JsonContainer] = JSON_MODIFY([j].[JsonContainer], '$.Value', JSON_VALUE(JSON_OBJECT('v': [j].[OtherValue]), '$.v'))
FROM [JsonTypeEntity] AS [j]
""");
        }
    }

    public class TimeOnlyTypeFixture : SqlServerTypeFixture<TimeOnly>
    {
        public override TimeOnly Value { get; } = new TimeOnly(12, 30, 45);
        public override TimeOnly OtherValue { get; } = new TimeOnly(14, 0, 0);

        protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;
    }
}

public class TimeSpanTypeTest(TimeSpanTypeTest.TimeSpanTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : RelationalTypeTestBase<TimeSpan, TimeSpanTypeTest.TimeSpanTypeFixture>(fixture, testOutputHelper)
{
    public override async Task SaveChanges_within_json()
    {
        await base.SaveChanges_within_json();

        AssertSql(
            """
@p0='{"OtherValue":"14:00:00","Value":"14:00:00"}' (Nullable = false) (Size = 44)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonTypeEntity] SET [JsonContainer] = @p0
OUTPUT 1
WHERE [Id] = @p1;
""");
    }

    public override async Task ExecuteUpdate_within_json_to_constant()
    {
        await base.ExecuteUpdate_within_json_to_constant();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
UPDATE [j]
SET [JsonContainer].modify('$.Value', N'14:00:00')
FROM [JsonTypeEntity] AS [j]
""");
        }
        else
        {
            AssertSql(
                """
UPDATE [j]
SET [j].[JsonContainer] = JSON_MODIFY([j].[JsonContainer], '$.Value', N'14:00:00')
FROM [JsonTypeEntity] AS [j]
""");
        }
    }

    [SqlServerCondition(SqlServerCondition.SupportsFunctions2022)]
    public override async Task ExecuteUpdate_within_json_to_nonjson_column()
    {
        // TODO: Currently failing on Helix only, see #36746
        if (Environment.GetEnvironmentVariable("HELIX_WORKITEM_ROOT") is not null)
        {
            return;
        }

        await base.ExecuteUpdate_within_json_to_nonjson_column();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
UPDATE [j]
SET [JsonContainer].modify('$.Value', JSON_VALUE(JSON_OBJECT('v': [j].[OtherValue]), '$.v'))
FROM [JsonTypeEntity] AS [j]
""");
        }
        else
        {
            AssertSql(
                """
UPDATE [j]
SET [j].[JsonContainer] = JSON_MODIFY([j].[JsonContainer], '$.Value', JSON_VALUE(JSON_OBJECT('v': [j].[OtherValue]), '$.v'))
FROM [JsonTypeEntity] AS [j]
""");
        }
    }

    public class TimeSpanTypeFixture : SqlServerTypeFixture<TimeSpan>
    {
        public override TimeSpan Value { get; } = new TimeSpan(12, 30, 45);
        public override TimeSpan OtherValue { get; } = new TimeSpan(14, 0, 0);

        protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;
    }
}

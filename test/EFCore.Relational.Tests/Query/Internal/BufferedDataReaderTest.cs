// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query.Internal;

public class BufferedDataReaderTest
{
    public static IEnumerable<object[]> IsAsyncData = new object[][] { [false], [true] };

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Metadata_methods_return_expected_results(bool async)
    {
        var reader = new FakeDbDataReader(["columnName"], new[] { [new object()], new[] { new object() } });
        var columns = new ReaderColumn[] { new ReaderColumn<object>(true, null, null, (r, _) => r.GetValue(0)) };
        var bufferedDataReader = new BufferedDataReader(reader, false);
        if (async)
        {
            await bufferedDataReader.InitializeAsync(columns, CancellationToken.None);
        }
        else
        {
            bufferedDataReader.Initialize(columns);
        }

        Assert.Equal(1, bufferedDataReader.FieldCount);
        Assert.Equal(0, bufferedDataReader.GetOrdinal("columnName"));
        Assert.Equal(typeof(object).Name, bufferedDataReader.GetDataTypeName(0));
        Assert.Equal(typeof(object), bufferedDataReader.GetFieldType(0));
        Assert.Equal("columnName", bufferedDataReader.GetName(0));
        Assert.Equal(2, bufferedDataReader.RecordsAffected);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Manipulation_methods_perform_expected_actions(bool async)
    {
        var reader = new FakeDbDataReader(
            ["id", "name"],
            new List<IList<object[]>> { new[] { new object[] { 1, "a" } }, Array.Empty<object[]>() });
        var columns = new ReaderColumn[]
        {
            new ReaderColumn<int>(false, null, null, (r, _) => r.GetInt32(0)),
            new ReaderColumn<object>(true, null, null, (r, _) => r.GetValue(1))
        };

        var bufferedDataReader = new BufferedDataReader(reader, false);

        Assert.False(bufferedDataReader.IsClosed);
        if (async)
        {
            await bufferedDataReader.InitializeAsync(columns, CancellationToken.None);
        }
        else
        {
            bufferedDataReader.Initialize(columns);
        }

        Assert.False(bufferedDataReader.IsClosed);

        Assert.True(bufferedDataReader.HasRows);

        if (async)
        {
            Assert.True(await bufferedDataReader.ReadAsync());
            Assert.False(await bufferedDataReader.ReadAsync());
        }
        else
        {
            Assert.True(bufferedDataReader.Read());
            Assert.False(bufferedDataReader.Read());
        }

        Assert.True(bufferedDataReader.HasRows);

        if (async)
        {
            Assert.True(await bufferedDataReader.NextResultAsync());
        }
        else
        {
            Assert.True(bufferedDataReader.NextResult());
        }

        Assert.False(bufferedDataReader.HasRows);

        if (async)
        {
            Assert.False(await bufferedDataReader.ReadAsync());
            Assert.False(await bufferedDataReader.NextResultAsync());
        }
        else
        {
            Assert.False(bufferedDataReader.Read());
            Assert.False(bufferedDataReader.NextResult());
        }

        Assert.False(bufferedDataReader.IsClosed);
        bufferedDataReader.Close();
        Assert.True(bufferedDataReader.IsClosed);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Initialize_is_idempotent(bool async)
    {
        var reader = new FakeDbDataReader(["name"], new[] { new[] { new object() } });
        var columns = new ReaderColumn[] { new ReaderColumn<object>(true, null, null, (r, _) => r.GetValue(0)) };
        var bufferedReader = new BufferedDataReader(reader, false);

        Assert.False(reader.IsClosed);
        if (async)
        {
            await bufferedReader.InitializeAsync(columns, CancellationToken.None);
        }
        else
        {
            bufferedReader.Initialize(columns);
        }

        Assert.True(reader.IsClosed);

        if (async)
        {
            await bufferedReader.InitializeAsync(columns, CancellationToken.None);
        }
        else
        {
            bufferedReader.Initialize(columns);
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Data_methods_return_expected_results(bool async)
    {
        await Verify_get_method_returns_supplied_value(true, async);
        await Verify_get_method_returns_supplied_value((byte)1, async);
        await Verify_get_method_returns_supplied_value((short)1, async);
        await Verify_get_method_returns_supplied_value(1, async);
        await Verify_get_method_returns_supplied_value(1L, async);
        await Verify_get_method_returns_supplied_value(1F, async);
        await Verify_get_method_returns_supplied_value(1D, async);
        await Verify_get_method_returns_supplied_value(1M, async);
        await Verify_get_method_returns_supplied_value('a', async);
        await Verify_get_method_returns_supplied_value("a", async);
        await Verify_get_method_returns_supplied_value(DateTime.Now, async);
        await Verify_get_method_returns_supplied_value(Guid.NewGuid(), async);
        var obj = new object();
        await Verify_method_result(r => r.GetValue(0), async, obj, [obj]);
        await Verify_method_result(r => r.GetFieldValue<object>(0), async, obj, [obj]);
        await Verify_method_result(r => r.GetFieldValueAsync<object>(0).Result, async, obj, [obj]);
        await Verify_method_result(r => r.IsDBNull(0), async, true, [DBNull.Value]);
        await Verify_method_result(r => r.IsDBNull(0), async, false, [true]);
        await Verify_method_result(r => r.IsDBNullAsync(0).Result, async, true, [DBNull.Value]);
        await Verify_method_result(r => r.IsDBNullAsync(0).Result, async, false, [true]);

        await Assert.ThrowsAsync<NotSupportedException>(
            () => Verify_method_result(r => r.GetBytes(0, 0, [], 0, 0), async, 0, [1L]));
        await Assert.ThrowsAsync<NotSupportedException>(
            () => Verify_method_result(r => r.GetChars(0, 0, [], 0, 0), async, 0, [1L]));
    }

    private async Task Verify_method_result<T>(
        Func<BufferedDataReader, T> method,
        bool async,
        T expectedResult,
        params object[][] dataReaderContents)
    {
        var reader = new FakeDbDataReader(["name"], dataReaderContents);
        var columnType = typeof(T);
        if (!columnType.IsValueType)
        {
            columnType = typeof(object);
        }

        var columns = new[]
        {
            ReaderColumn.Create(columnType, true, null, null, (Func<DbDataReader, int[], T>)((r, _) => r.GetFieldValue<T>(0)))
        };

        var bufferedReader = new BufferedDataReader(reader, false);
        if (async)
        {
            await bufferedReader.InitializeAsync(columns, CancellationToken.None);

            Assert.True(await bufferedReader.ReadAsync());
        }
        else
        {
            bufferedReader.Initialize(columns);

            Assert.True(bufferedReader.Read());
        }

        Assert.Equal(expectedResult, method(bufferedReader));
    }

    private Task Verify_get_method_returns_supplied_value<T>(T value, bool async)
    {
        // use the specific reader.GetXXX method
        var readerMethod = GetReaderMethod(typeof(T));
        return Verify_method_result(
            r => (T)readerMethod.Invoke(r, [0]), async, value, [value]);
    }

    private static MethodInfo GetReaderMethod(Type type)
        => RelationalTypeMapping.GetDataReaderMethod(type);
}

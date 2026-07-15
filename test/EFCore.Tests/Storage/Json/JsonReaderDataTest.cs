// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Storage.Json;

public class JsonReaderDataTest
{
    [Fact]
    public void BytesConsumed_is_updated_when_state_is_captured_for_buffer()
    {
        var json = Encoding.UTF8.GetBytes("""{"Id":77,"Name":"A"}""");
        var data = new JsonReaderData(json);
        var manager = new Utf8JsonReaderManager(data, queryLogger: null);

        manager.MoveNext();
        manager.MoveNext();
        manager.MoveNext();
        manager.CaptureState();

        Assert.Equal("""{"Id":77"""u8.Length, data.BytesConsumed);

        manager = new Utf8JsonReaderManager(data, queryLogger: null);
        while (manager.MoveNext() != JsonTokenType.EndObject)
        {
        }

        manager.CaptureState();

        Assert.Equal(json.Length, data.BytesConsumed);
    }

    [Fact]
    public void BytesConsumed_is_preserved_across_stream_buffer_refills()
    {
        var json = Encoding.UTF8.GetBytes($"[{string.Join(",", Enumerable.Range(0, 300))}]");
        using var stream = new BufferedStream(new MemoryStream(json));
        var data = new JsonReaderData(stream);
        var manager = new Utf8JsonReaderManager(data, queryLogger: null);

        for (var i = 0; i < 150; i++)
        {
            manager.MoveNext();
        }

        manager.CaptureState();
        var bytesRead = data.BytesConsumed;

        manager = new Utf8JsonReaderManager(data, queryLogger: null);
        while (manager.MoveNext() != JsonTokenType.EndArray)
        {
        }

        manager.CaptureState();

        Assert.InRange(bytesRead, 257, json.Length - 1);
        Assert.Equal(json.Length, data.BytesConsumed);
    }

    [Fact]
    public void CaptureState_resets_reader_to_captured_position()
    {
        var data = new JsonReaderData("""{"Collection":[],"Value":1}"""u8.ToArray());
        var manager = new Utf8JsonReaderManager(data, queryLogger: null);

        Assert.Equal(JsonTokenType.StartObject, manager.MoveNext());
        Assert.Equal(JsonTokenType.PropertyName, manager.MoveNext());
        Assert.True(manager.CurrentReader.ValueTextEquals("Collection"u8));
        Assert.Equal(JsonTokenType.StartArray, manager.MoveNext());

        manager.CaptureState();

        Assert.Equal("""{"Collection":["""u8.Length, data.BytesConsumed);
        Assert.Equal(JsonTokenType.EndArray, manager.MoveNext());

        manager.CaptureState();

        Assert.Equal("""{"Collection":[]"""u8.Length, data.BytesConsumed);
        Assert.Equal(JsonTokenType.PropertyName, manager.MoveNext());
        Assert.True(manager.CurrentReader.ValueTextEquals("Value"u8));
    }
}

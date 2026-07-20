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

    [Fact]
    public void BytesConsumed_is_preserved_across_partial_stream_reads()
    {
        var json = Encoding.UTF8.GetBytes($"[{string.Join(",", Enumerable.Range(0, 300))}]");
        using var stream = new PartialReadStream(json, 32);
        var data = new JsonReaderData(stream);
        var manager = new Utf8JsonReaderManager(data, queryLogger: null);

        while (manager.MoveNext() != JsonTokenType.EndArray)
        {
        }

        manager.CaptureState();

        Assert.Equal(json.Length, data.BytesConsumed);
    }

    private sealed class PartialReadStream : Stream
    {
        private readonly ReadOnlyMemory<byte> _buffer;
        private readonly int _maxBytesPerRead;
        private int _position;

        public PartialReadStream(byte[] buffer, int maxBytesPerRead)
        {
            _buffer = buffer;
            _maxBytesPerRead = maxBytesPerRead > 0
                ? maxBytesPerRead
                : throw new ArgumentOutOfRangeException(nameof(maxBytesPerRead));
        }

        public override int Read(Span<byte> destination)
        {
            if (_position >= _buffer.Length)
            {
                return 0;
            }

            var bytesToRead = Math.Min(destination.Length, Math.Min(_maxBytesPerRead, _buffer.Length - _position));
            _buffer.Span.Slice(_position, bytesToRead).CopyTo(destination);
            _position += bytesToRead;
            return bytesToRead;
        }

        public override int Read(byte[] buffer, int offset, int count)
            => Read(buffer.AsSpan(offset, count));

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => _buffer.Length;

        public override long Position
        {
            get => _position;
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin)
            => throw new NotSupportedException();

        public override void SetLength(long value)
            => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count)
            => throw new NotSupportedException();
    }
}

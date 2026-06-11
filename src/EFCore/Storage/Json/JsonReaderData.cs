// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Storage.Json;

/// <summary>
///     Contains state for use with a <see cref="Utf8JsonReaderManager" />, abstracting the reading from a <see cref="Stream" /> or a buffer.
/// </summary>
public class JsonReaderData
{
    private readonly Stream? _stream;
    private ReadOnlyMemory<byte> _buffer;
    private byte[]? _mutableBuffer;
    private int _positionInBuffer;
    private int _bytesAvailable;
    private JsonReaderState _readerState;

    /// <summary>
    ///     Creates a new <see cref="JsonReaderData" /> object to read JSON from the given buffer.
    /// </summary>
    /// <param name="buffer">The buffer containing UTF8 JSON bytes.</param>
    public JsonReaderData(ReadOnlyMemory<byte> buffer)
    {
        _buffer = buffer;
        _bytesAvailable = buffer.Length;
    }

    /// <summary>
    ///     Creates a new <see cref="JsonReaderData" /> object to read JSON from the given stream.
    /// </summary>
    /// <param name="stream">The stream providing UTF8 JSON bytes.</param>
    public JsonReaderData(Stream stream)
    {
        if (stream is MemoryStream memoryStream && memoryStream.TryGetBuffer(out var segment))
        {
            _buffer = segment.AsMemory()[(int)memoryStream.Position..];
            _bytesAvailable = _buffer.Length;
        }
        else
        {
            _stream = stream;
            _mutableBuffer = new byte[256];
            _buffer = _mutableBuffer;
            ReadBytes(0, default);
        }
    }

    /// <summary>
    ///     Called to capture the state of the given <see cref="Utf8JsonReaderManager" /> so that a new <see cref="Utf8JsonReaderManager" />
    ///     can later be created to pick up at the same position in the JSON document.
    /// </summary>
    /// <param name="manager">The manager.</param>
    public virtual void CaptureState(ref Utf8JsonReaderManager manager)
    {
        _positionInBuffer += (int)manager.CurrentReader.BytesConsumed;
        _readerState = manager.CurrentReader.CurrentState;
    }

    /// <summary>
    ///     Called to read bytes from the stream.
    /// </summary>
    /// <param name="bytesConsumed">The bytes consumed so far.</param>
    /// <param name="state">The current <see cref="JsonReaderState" />.</param>
    /// <returns>The new <see cref="Utf8JsonReader" />, having read my bytes from the stream.</returns>
    public virtual Utf8JsonReader ReadBytes(int bytesConsumed, JsonReaderState state)
    {
        if (_stream == null)
        {
            _bytesAvailable = 0;
        }
        else
        {
            var buffer = _mutableBuffer!;
            var totalConsumed = bytesConsumed + _positionInBuffer;
            if (_bytesAvailable != 0 && totalConsumed < buffer.Length)
            {
                var leftover = buffer.AsSpan(totalConsumed);

                if (leftover.Length == buffer.Length)
                {
                    Array.Resize(ref buffer, buffer.Length * 2);
                }

                leftover.CopyTo(buffer);
                _bytesAvailable = _stream.Read(buffer.AsSpan(leftover.Length)) + leftover.Length;
            }
            else
            {
                _bytesAvailable = _stream.Read(buffer);
            }

            _mutableBuffer = buffer;
            _buffer = buffer;
        }

        _positionInBuffer = 0;
        _readerState = state;

        return CreateReader();
    }

    /// <summary>
    ///     Creates a <see cref="Utf8JsonReader" /> for the current captured state.
    /// </summary>
    /// <returns>The new reader.</returns>
    public virtual Utf8JsonReader CreateReader()
        => new(
            _buffer.Span[_positionInBuffer..],
            isFinalBlock: _stream is null || _bytesAvailable != _buffer.Length,
            _readerState);
}

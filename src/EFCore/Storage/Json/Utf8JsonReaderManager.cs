// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Storage.Json;

/// <summary>
///     Manages buffering underneath a <see cref="Utf8JsonReader" />.
/// </summary>
/// <remarks>
///     The consumer should call <see cref="MoveNext" /> to advance to the next token in the JSON document, which may involve reading
///     more data from the stream and creating a new <see cref="Utf8JsonReader " /> instance in <see cref="CurrentReader" />.
/// </remarks>
public ref struct Utf8JsonReaderManager
{
    /// <summary>
    ///     Tracks state and underlying stream or buffer of UTF8 bytes.
    /// </summary>
    public readonly JsonReaderData Data;

    /// <summary>
    ///     The <see cref="Utf8JsonReader" /> set to the next token to be consumed.
    /// </summary>
    public Utf8JsonReader CurrentReader;

    /// <summary>
    ///     Creates a new <see cref="Utf8JsonReaderManager" /> instance that will start reading at the position in the JSON document
    ///     captured in the given <see cref="JsonReaderData" />
    /// </summary>
    /// <param name="data">The data.</param>
    /// <param name="queryLogger">
    ///     Logger for logging events that happen when reading/writing JSON values, or <see langword="null" /> if logging is
    ///     not available.
    /// </param>
    public Utf8JsonReaderManager(JsonReaderData data, IDiagnosticsLogger<DbLoggerCategory.Query>? queryLogger)
    {
        QueryLogger = queryLogger;
        Data = data;
        CurrentReader = data.CreateReader();
    }

    /// <summary>
    ///     Moves to the next token, which may involve reading more data from the stream and creating a new <see cref="Utf8JsonReader " />
    ///     instance in <see cref="CurrentReader" />.
    /// </summary>
    /// <returns>The token type of the current token.</returns>
    public JsonTokenType MoveNext()
    {
        while (!CurrentReader.Read())
        {
            CurrentReader = Data.ReadBytes((int)CurrentReader.BytesConsumed, CurrentReader.CurrentState);
        }

        return CurrentReader.TokenType;
    }

    /// <summary>
    ///     Skips the children of the current JSON token, which may involve reading more data from the stream and creating a new <see cref="Utf8JsonReader " />
    ///     instance in <see cref="CurrentReader" />.
    /// </summary>
    public void Skip()
    {
        while (!CurrentReader.TrySkip())
        {
            CurrentReader = Data.ReadBytes((int)CurrentReader.BytesConsumed, CurrentReader.CurrentState);
        }
    }

    /// <summary>
    ///     Called to capture the state of this <see cref="Utf8JsonReaderManager" /> into the associated <see cref="JsonReaderData" /> so
    ///     that a new <see cref="Utf8JsonReaderManager" /> can later be created to pick up at the same position in the JSON document.
    /// </summary>
    public void CaptureState()
        => Data.CaptureState(ref this);

    /// <summary>
    ///     Logger for logging events that happen when reading/writing JSON values, or <see langword="null" /> if logging is not available.
    /// </summary>
    public IDiagnosticsLogger<DbLoggerCategory.Query>? QueryLogger { get; }
}

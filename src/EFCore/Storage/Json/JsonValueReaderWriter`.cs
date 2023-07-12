// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Storage.Json;

/// <summary>
///     Reads and writes the JSON value for a given model or provider value.
/// </summary>
public abstract class JsonValueReaderWriter<TValue> : JsonValueReaderWriter
{
    /// <inheritdoc />
    public sealed override object FromJson(ref Utf8JsonReaderManager manager, object? existingObject = null)
        => FromJsonTyped(ref manager, existingObject)!;

    /// <inheritdoc />
    public sealed override void ToJson(Utf8JsonWriter writer, object value)
        => ToJsonTyped(writer, (TValue)value!);

    /// <inheritdoc />
    public sealed override Type ValueType
        => typeof(TValue);

    /// <summary>
    ///     Reads the value from JSON.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The <see cref="Utf8JsonReaderManager.CurrentReader" /> is at the node that contains the value to be read. The value should be read
    ///         as appropriate from the JSON, and then further converted as necessary.
    ///     </para>
    ///     <para>
    ///         Nulls are handled externally to this reader. That is, this method will never be called if the JSON value is "null".
    ///     </para>
    ///     <para>
    ///         In most cases, the value is represented in the JSON document as a simple property value--e.g. a number, boolean, or string.
    ///         However, it could be an array or sub-document. In this case, the <see cref="Utf8JsonReaderManager" /> should be used to parse
    ///         the JSON as appropriate.
    ///     </para>
    /// </remarks>
    /// <param name="manager">The <see cref="Utf8JsonReaderManager" /> for the JSON being read.</param>
    /// <param name="existingObject">Can be used to update an existing object, rather than create a new one.</param>
    /// <returns>The read value.</returns>
    public abstract TValue FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null);

    /// <summary>
    ///     Writes the value to JSON.
    /// </summary>
    /// <param name="writer">The <see cref="Utf8JsonWriter" /> into which the value should be written.</param>
    /// <param name="value">The value to write.</param>
    public abstract void ToJsonTyped(Utf8JsonWriter writer, TValue value);
}

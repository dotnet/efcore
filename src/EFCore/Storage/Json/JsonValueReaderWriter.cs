// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Storage.Json;

/// <summary>
///     Reads and writes the JSON value for a given model or provider value.
/// </summary>
/// <remarks>
///     Implementations of this type must inherit from the generic <see cref="JsonValueReaderWriter{TValue}" />
/// </remarks>
public abstract class JsonValueReaderWriter
{
    /// <summary>
    ///     Ensures the external types extend from the generic <see cref="JsonValueReaderWriter{TValue}" />
    /// </summary>
    internal JsonValueReaderWriter()
    {
    }

    /// <summary>
    ///     Reads the value from a UTF8 JSON stream or buffer.
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
    public abstract object FromJson(ref Utf8JsonReaderManager manager, object? existingObject = null);

    /// <summary>
    ///     Writes the value to JSON.
    /// </summary>
    /// <param name="writer">The <see cref="Utf8JsonWriter" /> into which the value should be written.</param>
    /// <param name="value">The value to write.</param>
    public abstract void ToJson(Utf8JsonWriter writer, object value);

    /// <summary>
    ///     The type of the value being read/written.
    /// </summary>
    public abstract Type ValueType { get; }

    /// <summary>
    ///     Reads the value from JSON in a string.
    /// </summary>
    /// <param name="json">The JSON to parse.</param>
    /// <param name="existingObject">Can be used to update an existing object, rather than create a new one.</param>
    /// <returns>The read value.</returns>
    public object FromJsonString(string json, object? existingObject = null)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new InvalidOperationException(CoreStrings.EmptyJsonString);
        }

        var readerManager = new Utf8JsonReaderManager(new JsonReaderData(Encoding.UTF8.GetBytes(json)), null);
        readerManager.MoveNext();
        return FromJson(ref readerManager, existingObject);
    }

    /// <summary>
    ///     Writes the value to a JSON string.
    /// </summary>
    /// <param name="value">The value to write.</param>
    /// <returns>The JSON representation of the given value.</returns>
    public string ToJsonString(object value)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        ToJson(writer, value);

        writer.Flush();
        var buffer = stream.ToArray();

        return Encoding.UTF8.GetString(buffer);
    }

    /// <summary>
    ///     Creates a <see cref="JsonValueReaderWriter{TValue}" /> instance of the given type, using the <c>Instance</c>
    ///     property to get th singleton instance if possible.
    /// </summary>
    /// <param name="readerWriterType">The type, which must inherit from <see cref="JsonValueReaderWriter{TValue}" />.</param>
    /// <returns>The reader/writer instance./</returns>
    /// <exception cref="InvalidOperationException">
    ///     if the type does not represent a
    ///     <see cref="JsonValueReaderWriter{TValue}" /> that can be instantiated.
    /// </exception>
    public static JsonValueReaderWriter? CreateFromType(Type? readerWriterType)
    {
        if (readerWriterType != null)
        {
            var instanceProperty = readerWriterType.GetAnyProperty("Instance");
            try
            {
                return instanceProperty != null
                    && instanceProperty.IsStatic()
                    && instanceProperty.GetMethod?.IsPublic == true
                    && readerWriterType.IsAssignableFrom(instanceProperty.PropertyType)
                        ? (JsonValueReaderWriter?)instanceProperty.GetValue(null)
                        : (JsonValueReaderWriter?)Activator.CreateInstance(readerWriterType);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(
                    CoreStrings.CannotCreateJsonValueReaderWriter(
                        readerWriterType.ShortDisplayName()), e);
            }
        }

        return null;
    }
}

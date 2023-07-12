// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Storage.Json;

/// <summary>
///     A <see cref="JsonValueReaderWriter{TValue}" /> for collections of primitive elements that will never be <see langword="null" />.
/// </summary>
/// <typeparam name="TCollection">The collection type.</typeparam>
/// <typeparam name="TElement">The element type.</typeparam>
public class JsonNoNullsCollectionReaderWriter<TCollection, TElement> : JsonValueReaderWriter<IEnumerable<TElement>>
{
    private readonly JsonValueReaderWriter<TElement> _elementReaderWriter;

    /// <summary>
    ///     Creates a new instance of this collection reader/writer, using the given reader/writer for its elements.
    /// </summary>
    /// <param name="elementReaderWriter">The reader/writer to use for each element.</param>
    public JsonNoNullsCollectionReaderWriter(JsonValueReaderWriter elementReaderWriter)
    {
        _elementReaderWriter = (JsonValueReaderWriter<TElement>)elementReaderWriter;
    }

    /// <inheritdoc />
    public override IEnumerable<TElement> FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
    {
        var value = new List<TElement>();

        while (manager.CurrentReader.TokenType != JsonTokenType.EndArray)
        {
            manager.MoveNext();

            switch (manager.CurrentReader.TokenType)
            {
                case JsonTokenType.String:
                case JsonTokenType.Number:
                case JsonTokenType.True:
                case JsonTokenType.False:
                    value.Add(_elementReaderWriter.FromJsonTyped(ref manager));
                    break;
            }
        }

        return typeof(TCollection).IsArray ? value.ToArray() : value;
    }

    /// <inheritdoc />
    public override void ToJsonTyped(Utf8JsonWriter writer, IEnumerable<TElement> value)
    {
        writer.WriteStartArray();
        foreach (var element in value)
        {
            _elementReaderWriter.ToJsonTyped(writer, element);
        }

        writer.WriteEndArray();
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Storage.Json;

/// <summary>
///     A <see cref="JsonValueReaderWriter{TValue}" /> for collections of primitives nullable value types.
/// </summary>
/// <typeparam name="TCollection">The collection type.</typeparam>
/// <typeparam name="TConcreteCollection">The collection type to create an index of, if needed.</typeparam>
/// <typeparam name="TElement">The element type.</typeparam>
public class JsonNullableStuctsCollectionReaderWriter<TCollection, TConcreteCollection, TElement> : JsonValueReaderWriter<IEnumerable<TElement?>>
    where TElement : struct
    where TCollection : IEnumerable<TElement?>
    where TConcreteCollection : IList<TElement?>
{
    private readonly JsonValueReaderWriter<TElement> _elementReaderWriter;

    /// <summary>
    ///     Creates a new instance of this collection reader/writer, using the given reader/writer for its elements.
    /// </summary>
    /// <param name="elementReaderWriter">The reader/writer to use for each element.</param>
    public JsonNullableStuctsCollectionReaderWriter(JsonValueReaderWriter<TElement> elementReaderWriter)
    {
        _elementReaderWriter = elementReaderWriter;
    }

    /// <inheritdoc />
    public override IEnumerable<TElement?> FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
    {
        IList<TElement?> collection;
        if (typeof(TCollection).IsArray)
        {
            collection = new List<TElement?>();
        }
        else if (existingObject == null)
        {
            collection = Activator.CreateInstance<TConcreteCollection>();
        }
        else
        {
            collection = (IList<TElement?>)existingObject;
            collection.Clear();
        }

        while (manager.CurrentReader.TokenType != JsonTokenType.EndArray)
        {
            manager.MoveNext();

            switch (manager.CurrentReader.TokenType)
            {
                case JsonTokenType.String:
                case JsonTokenType.Number:
                case JsonTokenType.True:
                case JsonTokenType.False:
                    collection.Add(_elementReaderWriter.FromJsonTyped(ref manager));
                    break;
                case JsonTokenType.Null:
                    collection.Add(null);
                    break;
            }
        }

        return typeof(TCollection).IsArray ? collection.ToArray() : collection;
    }

    /// <inheritdoc />
    public override void ToJsonTyped(Utf8JsonWriter writer, IEnumerable<TElement?> value)
    {
        writer.WriteStartArray();
        foreach (var element in value)
        {
            if (element.HasValue)
            {
                _elementReaderWriter.ToJsonTyped(writer, element.Value);
            }
            else
            {
                writer.WriteNullValue();
            }
        }

        writer.WriteEndArray();
    }
}

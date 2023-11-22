// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Storage.Json;

/// <summary>
///     A <see cref="JsonValueReaderWriter{TValue}" /> for collections of primitive elements that are a not <see cref="Nullable" />.
/// </summary>
/// <typeparam name="TCollection">The collection type.</typeparam>
/// <typeparam name="TConcreteCollection">The collection type to create an index of, if needed.</typeparam>
/// <typeparam name="TElement">The element type.</typeparam>
public class JsonCollectionReaderWriter<TCollection, TConcreteCollection, TElement> :
    JsonValueReaderWriter<IEnumerable<TElement?>>,
    ICompositeJsonValueReaderWriter
    where TCollection : IEnumerable<TElement?>
{
    private readonly JsonValueReaderWriter<TElement> _elementReaderWriter;

    /// <summary>
    ///     Creates a new instance of this collection reader/writer, using the given reader/writer for its elements.
    /// </summary>
    /// <param name="elementReaderWriter">The reader/writer to use for each element.</param>
    public JsonCollectionReaderWriter(JsonValueReaderWriter<TElement> elementReaderWriter)
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
            collection = (IList<TElement?>)Activator.CreateInstance<TConcreteCollection>()!;
        }
        else
        {
            collection = (IList<TElement?>)existingObject;
            collection.Clear();
        }

        var tokenType = manager.CurrentReader.TokenType;
        if (tokenType != JsonTokenType.StartArray)
        {
            throw new InvalidOperationException(
                CoreStrings.JsonReaderInvalidTokenType(tokenType.ToString()));
        }

        while (tokenType != JsonTokenType.EndArray)
        {
            manager.MoveNext();
            tokenType = manager.CurrentReader.TokenType;

            switch (tokenType)
            {
                case JsonTokenType.String:
                case JsonTokenType.Number:
                case JsonTokenType.True:
                case JsonTokenType.False:
                    collection.Add(_elementReaderWriter.FromJsonTyped(ref manager));
                    break;
                case JsonTokenType.Null:
                    collection.Add(default);
                    break;
                case JsonTokenType.Comment:
                case JsonTokenType.EndArray:
                    break;
                case JsonTokenType.None: // Explicitly listing all states that we throw for
                case JsonTokenType.StartObject:
                case JsonTokenType.EndObject:
                case JsonTokenType.StartArray:
                case JsonTokenType.PropertyName:
                default:
                    throw new InvalidOperationException(
                        CoreStrings.JsonReaderInvalidTokenType(tokenType.ToString()));
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
            if (element == null)
            {
                writer.WriteNullValue();
            }
            else
            {
                _elementReaderWriter.ToJsonTyped(writer, element);
            }
        }

        writer.WriteEndArray();
    }

    JsonValueReaderWriter ICompositeJsonValueReaderWriter.InnerReaderWriter
        => _elementReaderWriter;
}

﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Storage.Json;

/// <summary>
///     A <see cref="JsonValueReaderWriter{TValue}" /> for collections of primitives nullable value types.
/// </summary>
/// <typeparam name="TConcreteCollection">The collection type to create an index of, if needed.</typeparam>
/// <typeparam name="TElement">The element type.</typeparam>
public class JsonCollectionOfNullableStructsReaderWriter<TConcreteCollection, TElement> :
    JsonValueReaderWriter<IEnumerable<TElement?>>,
    ICompositeJsonValueReaderWriter
    where TElement : struct
{
    private readonly JsonValueReaderWriter<TElement> _elementReaderWriter;

    private static readonly bool IsArray = typeof(TConcreteCollection).IsArray;

    private static readonly bool IsReadOnly = IsArray
        || (typeof(TConcreteCollection).IsGenericType
            && typeof(TConcreteCollection).GetGenericTypeDefinition() == typeof(ReadOnlyCollection<>));

    /// <summary>
    ///     Creates a new instance of this collection reader/writer, using the given reader/writer for its elements.
    /// </summary>
    /// <param name="elementReaderWriter">The reader/writer to use for each element.</param>
    public JsonCollectionOfNullableStructsReaderWriter(JsonValueReaderWriter<TElement> elementReaderWriter)
    {
        _elementReaderWriter = elementReaderWriter;
    }

    /// <inheritdoc />
    public override IEnumerable<TElement?> FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
    {
        IList<TElement?> collection;
        if (IsReadOnly)
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

        if (manager.CurrentReader.TokenType == JsonTokenType.None)
        {
            manager.MoveNext();
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
                    collection.Add(null);
                    break;
                case JsonTokenType.EndArray:
                case JsonTokenType.Comment:
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

        return IsReadOnly
            ? IsArray
                ? collection.ToArray()
                : (IList<TElement?>)Activator.CreateInstance(typeof(TConcreteCollection), [collection])!
            : collection;
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

    JsonValueReaderWriter ICompositeJsonValueReaderWriter.InnerReaderWriter
        => _elementReaderWriter;

    private readonly ConstructorInfo _constructorInfo =
        typeof(JsonCollectionOfNullableStructsReaderWriter<TConcreteCollection, TElement>).GetConstructor([typeof(JsonValueReaderWriter<TElement>)])!;

    /// <inheritdoc />
    public override Expression ConstructorExpression =>
        Expression.New(_constructorInfo, ((ICompositeJsonValueReaderWriter)this).InnerReaderWriter.ConstructorExpression);
}

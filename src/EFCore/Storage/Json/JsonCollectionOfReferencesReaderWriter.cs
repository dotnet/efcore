﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Storage.Json;

/// <summary>
///     A <see cref="JsonValueReaderWriter{TValue}" /> for collections of primitive elements that are reference types. />.
/// </summary>
/// <typeparam name="TConcreteCollection">The collection type to create an index of, if needed.</typeparam>
/// <typeparam name="TElement">The element type.</typeparam>
public class JsonCollectionOfReferencesReaderWriter<TConcreteCollection, TElement> :
    JsonValueReaderWriter<object>,
    ICompositeJsonValueReaderWriter
    where TElement : class?
{
    private readonly JsonValueReaderWriter _elementReaderWriter;

    private static readonly bool IsArray = typeof(TConcreteCollection).IsArray;

    private static readonly bool IsReadOnly = IsArray
        || (typeof(TConcreteCollection).IsGenericType
            && typeof(TConcreteCollection).GetGenericTypeDefinition() == typeof(ReadOnlyCollection<>));

    /// <summary>
    ///     Creates a new instance of this collection reader/writer, using the given reader/writer for its elements.
    /// </summary>
    /// <param name="elementReaderWriter">The reader/writer to use for each element.</param>
    public JsonCollectionOfReferencesReaderWriter(JsonValueReaderWriter elementReaderWriter)
    {
        _elementReaderWriter = elementReaderWriter;
    }

    /// <inheritdoc />
    public override object FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
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
                case JsonTokenType.StartArray:
                    collection.Add((TElement)_elementReaderWriter.FromJson(ref manager));
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
    public override void ToJsonTyped(Utf8JsonWriter writer, object? value)
    {
        writer.WriteStartArray();
        if (value != null)
        {
            foreach (var element in (IEnumerable<object?>)value)
            {
                if (element == null)
                {
                    writer.WriteNullValue();
                }
                else
                {
                    _elementReaderWriter.ToJson(writer, element);
                }
            }
        }

        writer.WriteEndArray();
    }

    JsonValueReaderWriter ICompositeJsonValueReaderWriter.InnerReaderWriter
        => _elementReaderWriter;

    private readonly ConstructorInfo _constructorInfo = typeof(JsonCollectionOfReferencesReaderWriter<TConcreteCollection, TElement>).GetConstructor([typeof(JsonValueReaderWriter)])!;

    /// <inheritdoc />
    public override Expression ConstructorExpression =>
        Expression.New(_constructorInfo, ((ICompositeJsonValueReaderWriter)this).InnerReaderWriter.ConstructorExpression);
}

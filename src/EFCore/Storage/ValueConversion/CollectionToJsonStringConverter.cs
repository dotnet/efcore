// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Storage.Json;
using static System.Linq.Expressions.Expression;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion;

/// <summary>
///     A value converter that converts a .NET primitive collection into a JSON string.
/// </summary>
public class CollectionToJsonStringConverter<TElement> : ValueConverter<IEnumerable<TElement>, string>
{
    private static readonly MethodInfo ToJsonStringMethod
        = typeof(JsonValueReaderWriter).GetMethod(nameof(JsonValueReaderWriter.ToJsonString), [typeof(object)])!;

    private static readonly MethodInfo FromJsonStringMethod
        = typeof(JsonValueReaderWriter).GetMethod(nameof(JsonValueReaderWriter.FromJsonString), [typeof(string), typeof(object)])!;

    /// <summary>
    ///     Creates a new instance of this converter.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information and examples.
    /// </remarks>
    /// <param name="collectionJsonReaderWriter">The reader/writer to use.</param>
    public CollectionToJsonStringConverter(JsonValueReaderWriter collectionJsonReaderWriter)
        : base(
            ToJsonString(collectionJsonReaderWriter),
            FromJsonString(collectionJsonReaderWriter))
    {
        JsonReaderWriter = collectionJsonReaderWriter;
    }

    private static Expression<Func<IEnumerable<TElement>, string>> ToJsonString(JsonValueReaderWriter collectionJsonReaderWriter)
    {
        var prm = Parameter(typeof(IEnumerable<TElement>), "v");

        return  Lambda<Func<IEnumerable<TElement>, string>>(
            Call(
                collectionJsonReaderWriter.ConstructorExpression,
                ToJsonStringMethod,
                prm),
            prm);
    }

    private static Expression<Func<string, IEnumerable<TElement>>> FromJsonString(JsonValueReaderWriter collectionJsonReaderWriter)
    {
        var prm = Parameter(typeof(string), "v");

        return Lambda<Func<string, IEnumerable<TElement>>>(
            Convert(
                Call(
                    collectionJsonReaderWriter.ConstructorExpression,
                    FromJsonStringMethod,
                    prm,
                    Constant(null, typeof(object))),
                typeof(IEnumerable<TElement>)),
            prm);
    }

    /// <summary>
    ///     The reader/writer to use.
    /// </summary>
    public virtual JsonValueReaderWriter JsonReaderWriter { get; }
}

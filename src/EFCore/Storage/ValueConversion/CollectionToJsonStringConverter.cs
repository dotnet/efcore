// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion;

/// <summary>
///     A value converter that converts a .NET primitive collection into a JSON string.
/// </summary>
public class CollectionToJsonStringConverter<TElement> : ValueConverter<IEnumerable<TElement>, string>
{
    /// <summary>
    ///     Creates a new instance of this converter.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information and examples.
    /// </remarks>
    /// <param name="collectionJsonReaderWriter">The reader/writer to use.</param>
    public CollectionToJsonStringConverter(JsonValueReaderWriter collectionJsonReaderWriter)
        : base(
            v => collectionJsonReaderWriter.ToJsonString(v),
            v => (IEnumerable<TElement>)collectionJsonReaderWriter.FromJsonString(v, null))
    {
        JsonReaderWriter = collectionJsonReaderWriter;
    }

    /// <summary>
    ///     The reader/writer to use.
    /// </summary>
    public virtual JsonValueReaderWriter JsonReaderWriter { get; }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Storage.Json;

/// <summary>
///     A <see cref="JsonValueReaderWriter{TValue}" /> that wraps an existing reader/writer and adds casts to the given type.
/// </summary>
public class JsonCastValueReaderWriter<TConverted> :
    JsonValueReaderWriter<TConverted>,
    ICompositeJsonValueReaderWriter
{
    private readonly JsonValueReaderWriter _providerReaderWriter;

    /// <summary>
    ///     Creates a new instance of this reader/writer wrapping the given reader/writer.
    /// </summary>
    /// <param name="providerReaderWriter">The underlying provider type reader/writer.</param>
    public JsonCastValueReaderWriter(JsonValueReaderWriter providerReaderWriter)
    {
        _providerReaderWriter = providerReaderWriter;
    }

    /// <inheritdoc />
    public override TConverted FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
        => (TConverted)_providerReaderWriter.FromJson(ref manager, existingObject);

    /// <inheritdoc />
    public override void ToJsonTyped(Utf8JsonWriter writer, TConverted value)
        => _providerReaderWriter.ToJson(writer, value!);

    JsonValueReaderWriter ICompositeJsonValueReaderWriter.InnerReaderWriter
        => _providerReaderWriter;

    private readonly ConstructorInfo _constructorInfo = typeof(JsonCastValueReaderWriter<TConverted>).GetConstructor([typeof(JsonValueReaderWriter)])!;

    /// <inheritdoc />
    [Experimental(EFDiagnostics.PrecompiledQueryExperimental)]
    public override Expression ConstructorExpression =>
        Expression.New(_constructorInfo, ((ICompositeJsonValueReaderWriter)this).InnerReaderWriter.ConstructorExpression);
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Storage.Json;

/// <summary>
///     A <see cref="JsonValueReaderWriter{TValue}" /> that wraps an existing reader/writer and adds conversions from the model
///     type to and from the provider type.
/// </summary>
/// <typeparam name="TModel">The model type.</typeparam>
/// <typeparam name="TProvider">The provider type.</typeparam>
public class JsonConvertedValueReaderWriter<TModel, TProvider> :
    JsonValueReaderWriter<TModel>,
    IJsonConvertedValueReaderWriter
{
    private static readonly bool UseOldBehavior36856 =
        AppContext.TryGetSwitch("Microsoft.EntityFrameworkCore.Issue36856", out var enabled) && enabled;

    private readonly JsonValueReaderWriter<TProvider> _providerReaderWriter;
    private readonly ValueConverter _converter;

    /// <summary>
    ///     Creates a new instance of this reader/writer wrapping the given reader/writer.
    /// </summary>
    /// <param name="providerReaderWriter">The underlying provider type reader/writer.</param>
    /// <param name="converter">The value converter.</param>
    public JsonConvertedValueReaderWriter(
        JsonValueReaderWriter<TProvider> providerReaderWriter,
        ValueConverter converter)
    {
        _providerReaderWriter = providerReaderWriter;
        _converter = converter;
    }

    /// <inheritdoc />
    public override TModel FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
        => (TModel)_converter.ConvertFromProvider(_providerReaderWriter.FromJsonTyped(ref manager, existingObject))!;

    /// <inheritdoc />
    public override void ToJsonTyped(Utf8JsonWriter writer, TModel value)
        => _providerReaderWriter.ToJson(writer, (TProvider)_converter.ConvertToProvider(value)!);

    JsonValueReaderWriter ICompositeJsonValueReaderWriter.InnerReaderWriter
        => _providerReaderWriter;

    ValueConverter IJsonConvertedValueReaderWriter.Converter
        => _converter;

    private readonly ConstructorInfo _constructorInfo =
        typeof(JsonConvertedValueReaderWriter<TModel, TProvider>).GetConstructor(
            [typeof(JsonValueReaderWriter<TProvider>), typeof(ValueConverter)])!;

    /// <inheritdoc />
    public override Expression ConstructorExpression
        => UseOldBehavior36856
            ? Expression.New(
                _constructorInfo,
                ((ICompositeJsonValueReaderWriter)this).InnerReaderWriter.ConstructorExpression,
                ((IJsonConvertedValueReaderWriter)this).Converter.ConstructorExpression)
            : Expression.New(
                _constructorInfo,
                ((ICompositeJsonValueReaderWriter)this).InnerReaderWriter.ConstructorExpression,
                // We shouldn't quote converters, because it will create a new instance every time and
                // it will have to compile the expression again and
                // it will have a negative performance impact. See #36856 for more info.
                // This means this is currently unsupported scenario for precompilation.
                Expression.Constant(((IJsonConvertedValueReaderWriter)this).Converter));
}

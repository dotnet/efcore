// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Storage.Json;

public class JsonValueReaderWriterTest
{
    [Theory]
    [InlineData(false, "\"caf\\u00E9 \\u003C\\u0026\"")]
    [InlineData(true, "\"caf\u00e9 <&\"")]
    public void Writes_UTF8_using_writer_options(bool relaxedEscaping, string expected)
    {
        var options = new JsonWriterOptions
        {
            Encoder = relaxedEscaping ? JavaScriptEncoder.UnsafeRelaxedJsonEscaping : JavaScriptEncoder.Default
        };
        var json = JsonStringReaderWriter.Instance.ToJsonBytes("caf\u00e9 <&", options);

        Assert.Equal(Encoding.UTF8.GetBytes(expected), json.ToArray());
    }

    [Fact]
    public void Rejects_null_when_reader_writer_does_not_handle_nulls()
    {
        Assert.Equal(
            "value", Assert.Throws<ArgumentNullException>(() => JsonStringReaderWriter.Instance.ToJsonBytes(null)).ParamName);
        Assert.Equal(
            "value", Assert.Throws<ArgumentNullException>(() => JsonStringReaderWriter.Instance.ToJsonString(null)).ParamName);
    }

    [Fact]
    public void Passes_null_to_reader_writer_that_handles_nulls()
    {
        var readerWriter = new NullHandlingReaderWriter();

        Assert.Equal("\"sentinel\"", Encoding.UTF8.GetString(readerWriter.ToJsonBytes(null).Span));
        Assert.Equal("\"sentinel\"", readerWriter.ToJsonString(null));
    }

    private sealed class NullHandlingReaderWriter : JsonValueReaderWriter<string>
    {
        public override bool HandlesNullWrites
            => true;

        public override string FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
            => manager.CurrentReader.GetString()!;

        public override void ToJsonTyped(Utf8JsonWriter writer, string value)
            => writer.WriteStringValue(value ?? "sentinel");

        public override Expression ConstructorExpression
            => Expression.New(typeof(NullHandlingReaderWriter));
    }
}

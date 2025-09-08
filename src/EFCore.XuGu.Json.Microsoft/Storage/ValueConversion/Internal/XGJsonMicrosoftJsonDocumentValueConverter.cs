// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.XuGu.Json.Microsoft.Storage.ValueConversion.Internal
{
    public class XGJsonMicrosoftJsonDocumentValueConverter : ValueConverter<JsonDocument, string>
    {
        public XGJsonMicrosoftJsonDocumentValueConverter()
            : base(
                v => ConvertToProviderCore(v),
                v => ConvertFromProviderCore(v))
        {
        }

        public static string ConvertToProviderCore(JsonDocument v)
        {
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream);
            v.WriteTo(writer);
            writer.Flush();
            return Encoding.UTF8.GetString(stream.ToArray());
        }

        public static JsonDocument ConvertFromProviderCore(string v)
            => JsonDocument.Parse(v);
    }
}

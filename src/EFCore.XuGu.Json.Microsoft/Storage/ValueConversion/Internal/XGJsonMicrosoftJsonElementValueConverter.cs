using System.IO;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.XuGu.Json.Microsoft.Storage.ValueConversion.Internal
{
    public class XGJsonMicrosoftJsonElementValueConverter : ValueConverter<JsonElement, string>
    {
        public XGJsonMicrosoftJsonElementValueConverter()
            : base(
                v => ConvertToProviderCore(v),
                v => ConvertFromProviderCore(v))
        {
        }

        public static string ConvertToProviderCore(JsonElement v)
        {
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream);
            v.WriteTo(writer);
            writer.Flush();
            return Encoding.UTF8.GetString(stream.ToArray());
        }

        public static JsonElement ConvertFromProviderCore(string v)
            => JsonDocument.Parse(v).RootElement;
    }
}

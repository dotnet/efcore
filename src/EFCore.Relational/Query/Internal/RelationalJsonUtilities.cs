// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Globalization;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public static class RelationalJsonUtilities
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
#pragma warning disable EF1001 // Internal EF Core API usage.
    public static object? ReadJsonScalar(JsonElement element, object? value, JsonValueReaderWriter readerWriter)
        => element.ValueKind switch
        {
            JsonValueKind.Null => null,
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => readerWriter is IJsonConvertedValueReaderWriter converted
                ? converted.Converter.ConvertToProvider(value)
                : value is Enum
                    ? Convert.ToDecimal(value, CultureInfo.InvariantCulture)
                    : value,
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => element.GetRawText()
        };
#pragma warning restore EF1001

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly MethodInfo SerializeComplexTypeToJsonMethod =
        typeof(RelationalJsonUtilities).GetTypeInfo().GetDeclaredMethod(nameof(SerializeComplexTypeToJson))!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static string? SerializeComplexTypeToJson(IComplexType complexType, object? value, bool collection)
    {
        // Note that we treat toplevel null differently: we return a relational NULL for that case. For nested nulls,
        // we return JSON null string (so you get { "foo": null })
        if (value is null)
        {
            return null;
        }

        var stream = new MemoryStream();
        var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = false });

        WriteJson(writer, complexType, value, collection);

        writer.Flush();

        return Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int)stream.Length);

        void WriteJson(Utf8JsonWriter writer, IComplexType complexType, object? value, bool collection)
        {
            if (collection)
            {
                if (value is null)
                {
                    writer.WriteNullValue();

                    return;
                }

                writer.WriteStartArray();

                foreach (var element in (IEnumerable)value)
                {
                    WriteJsonObject(writer, complexType, element);
                }

                writer.WriteEndArray();
                return;
            }

            WriteJsonObject(writer, complexType, value);
        }

        void WriteJsonObject(Utf8JsonWriter writer, IComplexType complexType, object? objectValue)
        {
            if (objectValue is null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartObject();

            foreach (var property in complexType.GetProperties())
            {
                var jsonPropertyName = property.GetJsonPropertyName();
                Check.DebugAssert(jsonPropertyName is not null);
                writer.WritePropertyName(jsonPropertyName);

                var jsonValueReaderWriter = property.GetTypeMapping().JsonValueReaderWriter;

                var propertyValue = property.GetGetter().GetClrValue(objectValue);
                if (propertyValue is null && jsonValueReaderWriter?.HandlesNullWrites != true)
                {
                    if (!property.IsNullable)
                    {
                        throw new InvalidOperationException(RelationalStrings.NullValueInRequiredJsonProperty(property.Name));
                    }

                    writer.WriteNullValue();
                }
                else
                {
                    Check.DebugAssert(jsonValueReaderWriter is not null, "Missing JsonValueReaderWriter on JSON property");
                    jsonValueReaderWriter.ToJson(writer, propertyValue);
                }
            }

            foreach (var complexProperty in complexType.GetComplexProperties())
            {
                var jsonPropertyName = complexProperty.GetJsonPropertyName();
                Check.DebugAssert(jsonPropertyName is not null);
                writer.WritePropertyName(jsonPropertyName);

                var propertyValue = complexProperty.GetGetter().GetClrValue(objectValue);

                if (propertyValue is null && !complexProperty.IsNullable)
                {
                    throw new InvalidOperationException(RelationalStrings.NullValueInRequiredJsonProperty(complexProperty.Name));
                }

                WriteJson(writer, complexProperty.ComplexType, propertyValue, complexProperty.IsCollection);
            }

            writer.WriteEndObject();
        }
    }
}

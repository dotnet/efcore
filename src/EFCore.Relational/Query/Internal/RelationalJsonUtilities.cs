// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Text;
using System.Text.Json;

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

        return Encoding.UTF8.GetString(stream.ToArray());

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

                var propertyValue = property.GetGetter().GetClrValue(objectValue);
                if (propertyValue is null)
                {
                    writer.WriteNullValue();
                }
                else
                {
                    var jsonValueReaderWriter = property.GetJsonValueReaderWriter() ?? property.GetTypeMapping().JsonValueReaderWriter;
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

                WriteJson(writer, complexProperty.ComplexType, propertyValue, complexProperty.IsCollection);
            }

            writer.WriteEndObject();
        }
    }
}

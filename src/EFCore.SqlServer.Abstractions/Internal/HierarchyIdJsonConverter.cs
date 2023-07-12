// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.EntityFrameworkCore.Internal;

internal class HierarchyIdJsonConverter : JsonConverter<HierarchyId>
{
    public override HierarchyId? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();

        return value is null
            ? null
            : HierarchyId.Parse(value);
    }

    public override void Write(Utf8JsonWriter writer, HierarchyId? value, JsonSerializerOptions options)
        => writer.WriteStringValue(value?.ToString());
}

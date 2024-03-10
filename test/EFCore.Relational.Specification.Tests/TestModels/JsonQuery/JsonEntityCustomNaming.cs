// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json.Serialization;

namespace Microsoft.EntityFrameworkCore.TestModels.JsonQuery;

#nullable disable

public class JsonEntityCustomNaming
{
    public int Id { get; set; }

    public string Title { get; set; }

    [JsonPropertyName("CustomOwnedReferenceRoot")]
    public JsonOwnedCustomNameRoot OwnedReferenceRoot { get; set; }

    [JsonPropertyName("CustomOwnedCollectionRoot")]
    public List<JsonOwnedCustomNameRoot> OwnedCollectionRoot { get; set; }
}

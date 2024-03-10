// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json.Serialization;

namespace Microsoft.EntityFrameworkCore.TestModels.JsonQuery;

#nullable disable

public class JsonOwnedCustomNameRoot
{
    [JsonPropertyName("CustomName")]
    public string Name { get; set; }

    [JsonPropertyName("CustomNumber")]
    public int Number { get; set; }

    [JsonPropertyName("CustomEnum")]
    public JsonEnum Enum { get; set; }

    [JsonPropertyName("CustomOwnedReferenceBranch")]
    public JsonOwnedCustomNameBranch OwnedReferenceBranch { get; set; }

    [JsonPropertyName("CustomOwnedCollectionBranch")]
    public List<JsonOwnedCustomNameBranch> OwnedCollectionBranch { get; set; }
}

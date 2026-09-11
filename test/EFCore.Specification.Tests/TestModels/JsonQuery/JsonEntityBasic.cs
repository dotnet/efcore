// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.JsonQuery;

public class JsonEntityBasic
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    public JsonOwnedRoot OwnedReferenceRoot { get; set; } = null!;
    public List<JsonOwnedRoot> OwnedCollectionRoot { get; set; } = null!;

    public JsonEntityBasicForReference EntityReference { get; set; } = null!;
    public List<JsonEntityBasicForCollection> EntityCollection { get; set; } = null!;
}

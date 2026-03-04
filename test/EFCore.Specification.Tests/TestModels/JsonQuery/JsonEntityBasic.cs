// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.JsonQuery;

#nullable disable

public class JsonEntityBasic
{
    public int Id { get; set; }
    public string Name { get; set; }

    public JsonOwnedRoot OwnedReferenceRoot { get; set; }
    public List<JsonOwnedRoot> OwnedCollectionRoot { get; set; }

    public JsonEntityBasicForReference EntityReference { get; set; }
    public List<JsonEntityBasicForCollection> EntityCollection { get; set; }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.JsonQuery;

#nullable disable

public class JsonEntityInheritanceBase
{
    public int Id { get; set; }
    public string Name { get; set; }

    public JsonOwnedBranch ReferenceOnBase { get; set; }
    public List<JsonOwnedBranch> CollectionOnBase { get; set; }
}

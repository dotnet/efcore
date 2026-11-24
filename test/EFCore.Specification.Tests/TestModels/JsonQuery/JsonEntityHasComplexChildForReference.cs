// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.JsonQuery;

public class JsonEntityHasComplexChildForReference
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    // A is required for sorting
    public AJsonEntityHasComplexChildForReferenceForReference AEntityReference { get; set; } = null!;

    public int? ParentId { get; set; }
    public JsonEntityHasComplexChild Parent { get; set; } = null!;
}

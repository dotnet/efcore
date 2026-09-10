// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.JsonQuery;

#nullable disable

public class JsonEntityHasComplexChildForReference
{
    public int Id { get; set; }
    public string Name { get; set; }

    // A is required for sorting
    public AJsonEntityHasComplexChildForReferenceForReference AEntityReference { get; set; }

    public int? ParentId { get; set; }
    public JsonEntityHasComplexChild Parent { get; set; }
}

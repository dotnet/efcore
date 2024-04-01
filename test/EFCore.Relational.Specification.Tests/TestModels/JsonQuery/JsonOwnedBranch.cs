// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.JsonQuery;

#nullable disable

public class JsonOwnedBranch
{
    public DateTime Date { get; set; }
    public decimal Fraction { get; set; }

    public JsonEnum Enum { get; set; }
    public JsonEnum? NullableEnum { get; init; }
    public JsonEnum[] Enums { get; init; }
    public JsonEnum?[] NullableEnums { get; init; }

    public JsonOwnedLeaf OwnedReferenceLeaf { get; set; }
    public List<JsonOwnedLeaf> OwnedCollectionLeaf { get; set; }
}

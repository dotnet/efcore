// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.JsonQuery;

#nullable disable

public class JsonEntityInheritanceDerived : JsonEntityInheritanceBase
{
    public double Fraction { get; set; }
    public JsonOwnedBranch ReferenceOnDerived { get; set; }
    public List<JsonOwnedBranch> CollectionOnDerived { get; set; }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.JsonQuery;

#nullable disable

public class JsonOwnedLeaf
{
    public string SomethingSomething { get; set; }

    public JsonOwnedBranch Parent { get; set; }
}

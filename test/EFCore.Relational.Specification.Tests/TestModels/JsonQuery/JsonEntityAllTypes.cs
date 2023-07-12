// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.JsonQuery;

public class JsonEntityAllTypes
{
    public int Id { get; set; }
    public JsonOwnedAllTypes Reference { get; set; }
    public List<JsonOwnedAllTypes> Collection { get; set; }
}

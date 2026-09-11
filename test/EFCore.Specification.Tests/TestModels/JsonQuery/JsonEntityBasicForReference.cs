// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.JsonQuery;

public class JsonEntityBasicForReference
{
    public int Id { get; private set; }

    public string Name { get; private set; } = null!;

    public int? ParentId { get; set; }
    public JsonEntityBasic Parent { get; set; } = null!;

    public void SetIdAndName(int id, string name)
    {
        Id = id;
        Name = name;
    }
}

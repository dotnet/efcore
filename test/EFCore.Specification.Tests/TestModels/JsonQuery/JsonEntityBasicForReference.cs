// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.JsonQuery;

#nullable disable

public class JsonEntityBasicForReference
{
    private int _id;
    private string _name;

    public int Id
        => _id;

    public string Name
        => _name;

    public int? ParentId { get; set; }
    public JsonEntityBasic Parent { get; set; }

    public void SetIdAndName(int id, string name)
    {
        _id = id;
        _name = name;
    }
}

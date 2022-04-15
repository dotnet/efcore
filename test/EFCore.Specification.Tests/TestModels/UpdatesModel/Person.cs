// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.UpdatesModel;

public class Person
{
    protected Person()
    {
    }

    public Person(string name, Person parent)
    {
        Name = name;
        Parent = parent;
    }

    public int PersonId { get; set; }
    public string Name { get; set; }
    public int? ParentId { get; set; }
    public Person Parent { get; set; }
}

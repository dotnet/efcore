// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

# nullable enable

namespace Microsoft.EntityFrameworkCore.TestModels.UpdatesModel;

public class Person
{
    protected Person()
    {
        Name = null!;
    }

    public Person(string name, Person? parent)
    {
        Name = name;
        Parent = parent;
    }

    public int PersonId { get; set; }
    public string Name { get; set; }
    public int? ParentId { get; set; }
    public string? Country { get; set; }
    public string? ZipCode { get; set; }
    public Person? Parent { get; set; }
    public Address? Address { get; set; }
}

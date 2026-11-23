// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.InheritanceModel;

public class Manufacturer
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    // Owned reference with a complex property, declared on the base type
    public Site Headquarters { get; set; } = null!;
}

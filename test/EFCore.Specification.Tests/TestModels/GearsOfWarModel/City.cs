// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;

#nullable disable

public class City
{
    // non-integer key with not conventional name
    public string Name { get; set; }

    public string Location { get; set; }

    public string Nation { get; set; }

    public List<Gear> BornGears { get; set; }
    public List<Gear> StationedGears { get; set; }
}

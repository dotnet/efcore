// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.InheritanceModel;

#nullable disable

public abstract class Plant
{
    public PlantGenus Genus { get; set; }
    public string Species { get; set; }
    public string Name { get; set; }
}

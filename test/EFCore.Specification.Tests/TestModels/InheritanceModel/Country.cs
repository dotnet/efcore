// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.InheritanceModel;

#nullable disable

public class Country
{
    public int Id { get; set; }
    public string Name { get; set; }

    public IList<Animal> Animals { get; set; } = new List<Animal>();
    public IList<Plant> Plants { get; set; }
}

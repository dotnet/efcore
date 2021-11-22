// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.InheritanceModel;

public class Eagle : Bird
{
    public Eagle()
    {
        Prey = new List<Bird>();
    }

    public EagleGroup Group { get; set; }

    public ICollection<Bird> Prey { get; set; }
}

public enum EagleGroup
{
    Fish,
    Booted,
    Snake,
    Harpy
}

public class EagleQuery : BirdQuery
{
    public EagleGroup Group { get; set; }
}

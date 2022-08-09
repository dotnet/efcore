// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.InheritanceModel;

public class Kiwi : Bird
{
    public Island FoundOn { get; set; }
}

public enum Island : byte
{
    North,
    South
}

public class KiwiQuery : BirdQuery
{
    public Island FoundOn { get; set; }
}

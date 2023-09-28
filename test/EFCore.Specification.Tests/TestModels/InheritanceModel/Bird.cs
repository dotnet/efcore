// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.InheritanceModel;

public abstract class Bird : Animal
{
    public bool IsFlightless { get; set; }
    public int? EagleId { get; set; }
}

public abstract class BirdQuery : AnimalQuery
{
    public bool IsFlightless { get; set; }
    public int? EagleId { get; set; }
}

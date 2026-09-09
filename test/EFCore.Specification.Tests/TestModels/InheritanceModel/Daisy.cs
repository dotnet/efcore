// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.InheritanceModel;

public class Daisy : Flower
{
    public required AdditionalDaisyInfo AdditionalInfo { get; set; }
}

public class AdditionalDaisyInfo
{
    public string? Nickname { get; set; }
    public required DaisyLeafStructure LeafStructure { get; set; }
}

public class DaisyLeafStructure
{
    public int NumLeaves { get; set; }
    public bool AreLeavesBig { get; set; }
}

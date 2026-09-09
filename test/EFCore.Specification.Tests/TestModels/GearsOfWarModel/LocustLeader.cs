// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;

#nullable disable

public class LocustLeader
{
    public string Name { get; set; }
    public short ThreatLevel { get; set; }
    public byte ThreatLevelByte { get; set; }
    public byte? ThreatLevelNullableByte { get; set; }
}

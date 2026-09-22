// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;

#nullable disable

public class LocustCommander : LocustLeader
{
    public LocustHorde CommandingFaction { get; set; }

    public string DefeatedByNickname { get; set; }
    public int? DefeatedBySquadId { get; set; }
    public Gear DefeatedBy { get; set; }

    public LocustHighCommand HighCommand { get; set; }
    public int HighCommandId { get; set; }
}

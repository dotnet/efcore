// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;

#nullable disable

public class SquadMission
{
    public Squad Squad { get; set; }
    public int MissionId { get; set; }

    public int SquadId { get; set; }
    public Mission Mission { get; set; }
}

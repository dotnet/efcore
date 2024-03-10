// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;

#nullable disable

public class Mission
{
    public int Id { get; set; }

    public string CodeName { get; set; }
    public double? Rating { get; set; }
    public DateTimeOffset Timeline { get; set; }
    public TimeSpan Duration { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly Time { get; set; }

    public virtual ICollection<SquadMission> ParticipatingSquads { get; set; }
}

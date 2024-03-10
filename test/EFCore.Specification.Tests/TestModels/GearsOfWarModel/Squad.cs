// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;

#nullable disable

public class Squad
{
    public Squad()
    {
        Members = new List<Gear>();
    }

    // non-auto generated key
    public int Id { get; set; }

    public string Name { get; set; }

    // auto-generated non-key (sequence)
    public int InternalNumber { get; set; }

    public virtual byte[] Banner { get; set; }

    public virtual byte[] Banner5 { get; set; }

    public virtual ICollection<Gear> Members { get; set; }
    public virtual ICollection<SquadMission> Missions { get; set; }
}

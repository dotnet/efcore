// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;

#nullable disable

public class Gear
{
    public Gear()
    {
        Weapons = new List<Weapon>();
    }

    // composite key
    public string Nickname { get; set; }

    public int SquadId { get; set; }

    public string FullName { get; set; }

    public string CityOfBirthName { get; set; }
    public virtual City CityOfBirth { get; set; }

    public virtual City AssignedCity { get; set; }

    public MilitaryRank Rank { get; set; }

    public virtual CogTag Tag { get; set; } = new(); // Initialized to test #23851

    public virtual Squad Squad { get; set; }

    // TODO: make this many to many - not supported at the moment
    public virtual ICollection<Weapon> Weapons { get; set; }

    public string LeaderNickname { get; set; }
    public int LeaderSquadId { get; set; }

    public bool HasSoulPatch { get; set; }

    [NotMapped]
    public bool IsMarcus
        => Nickname == "Marcus";
}

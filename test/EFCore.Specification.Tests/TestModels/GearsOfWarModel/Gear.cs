// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;

public class Gear
{
    public Gear()
        => Weapons = [];

    // composite key
    public string Nickname { get; set; } = null!;

    public int SquadId { get; set; }

    public string FullName { get; set; } = null!;

    public string CityOfBirthName { get; set; } = null!;
    public virtual City CityOfBirth { get; set; } = null!;

    public virtual City? AssignedCity { get; set; }

    public MilitaryRank Rank { get; set; }

    public virtual CogTag Tag { get; set; } = new(); // Initialized to test #23851

    public virtual Squad Squad { get; set; } = null!;

    // TODO: make this many to many - not supported at the moment
    public virtual ICollection<Weapon> Weapons { get; set; }

    public string? LeaderNickname { get; set; }
    public int LeaderSquadId { get; set; }

    public bool HasSoulPatch { get; set; }

    [NotMapped]
    public bool IsMarcus
        => Nickname == "Marcus";
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel
{
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

        public virtual CogTag Tag { get; set; }

        public virtual Squad Squad { get; set; }

        // TODO: make this many to many - not supported at the moment
        public virtual ICollection<Weapon> Weapons { get; set; }

        public string LeaderNickname { get; set; }
        public int LeaderSquadId { get; set; }

        public bool HasSoulPatch { get; set; }

        [NotMapped]
        public bool IsMarcus => Nickname == "Marcus";
    }
}

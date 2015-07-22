// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.Data.Entity.FunctionalTests.TestModels.GearsOfWarModel
{
    public class Gear
    {
        public Gear()
        {
            Weapons = new List<Weapon>();
            Reports = new List<Gear>();
        }

        // composite key
        public string Nickname { get; set; }
        public int SquadId { get; set; }

        public string FullName { get; set; }

        public string CityOrBirthName { get; set; }
        public virtual City CityOfBirth { get; set; }

        public virtual City AssignedCity { get; set; }

        public MilitaryRank Rank { get; set; }

        public virtual CogTag Tag { get; set; }
        public virtual Squad Squad { get; set; }

        // TODO: make this many to many - not supported at the moment
        public virtual ICollection<Weapon> Weapons { get; set; }

        public string LeaderNickname { get; set; }
        public int LeaderSquadId { get; set; }

        // 1 - many self reference
        public virtual ICollection<Gear> Reports { get; set; }
    }
}

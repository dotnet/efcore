// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel
{
    public class Driver
    {
        private readonly ILazyLoader _loader;
        private Team _team;

        public Driver()
        {
        }

        protected Driver(
            ILazyLoader loader,
            int id,
            string name,
            int? carNumber,
            int championships,
            int races,
            int wins,
            int podiums,
            int poles,
            int fastestLaps,
            int teamId)
        {
            _loader = loader;

            Id = id;
            Name = name;
            CarNumber = carNumber;
            Championships = championships;
            Races = races;
            Wins = wins;
            Podiums = podiums;
            Poles = poles;
            FastestLaps = fastestLaps;
            TeamId = teamId;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int? CarNumber { get; set; }
        public int Championships { get; set; }
        public int Races { get; set; }
        public int Wins { get; set; }
        public int Podiums { get; set; }
        public int Poles { get; set; }
        public int FastestLaps { get; set; }

        public virtual Team Team
        {
            get => _loader.Load(this, ref _team);
            set => _team = value;
        }

        public int TeamId { get; set; }
    }
}

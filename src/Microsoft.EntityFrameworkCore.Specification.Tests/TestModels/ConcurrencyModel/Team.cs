// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.ConcurrencyModel
{
    public class Team
    {
#if NET451
        private readonly ObservableCollection<Driver> _drivers = new ObservableCollectionListSource<Driver>();
#else
        private readonly ObservableCollection<Driver> _drivers = new ObservableCollection<Driver>();
#endif
        private readonly ObservableCollection<Sponsor> _sponsors = new ObservableCollection<Sponsor>();

        public int Id { get; set; }

        public string Name { get; set; }
        public string Constructor { get; set; }
        public string Tire { get; set; }
        public string Principal { get; set; }
        public int ConstructorsChampionships { get; set; }
        public int DriversChampionships { get; set; }
        public int Races { get; set; }
        public int Victories { get; set; }
        public int Poles { get; set; }
        public int FastestLaps { get; set; }

        public virtual Engine Engine { get; set; } // Independent Association

        public virtual Chassis Chassis { get; set; }

        public virtual ICollection<Driver> Drivers => _drivers;

        [NotMapped]
        public virtual ICollection<Sponsor> Sponsors => _sponsors;

        public int? GearboxId { get; set; }
        public virtual Gearbox Gearbox { get; set; } // Uni-directional

        public const int McLaren = 1;
        public const int Mercedes = 2;
        public const int RedBull = 3;
        public const int Ferrari = 4;
        public const int Williams = 5;
        public const int Renault = 6;
        public const int ForceIndia = 7;
        public const int ToroRosso = 8;
        public const int Lotus = 9;
        public const int Hispania = 10;
        public const int Sauber = 11;
        public const int Virgin = 12;
    }
}

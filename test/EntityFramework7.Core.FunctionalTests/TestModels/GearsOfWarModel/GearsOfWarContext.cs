// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Infrastructure;

namespace Microsoft.Data.Entity.FunctionalTests.TestModels.GearsOfWarModel
{
    public class GearsOfWarContext : DbContext
    {
        public static readonly string StoreName = "GearsOfWar";

        public GearsOfWarContext(IServiceProvider serviceProvider, DbContextOptions options)
            : base(serviceProvider, options)
        {
        }

        public DbSet<Gear> Gears { get; set; }
        public DbSet<Squad> Squads { get; set; }
        public DbSet<CogTag> Tags { get; set; }
        public DbSet<Weapon> Weapons { get; set; }
        public DbSet<City> Cities { get; set; }
    }
}

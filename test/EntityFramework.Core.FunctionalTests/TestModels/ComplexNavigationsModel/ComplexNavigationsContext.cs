// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Infrastructure;

namespace Microsoft.Data.Entity.FunctionalTests.TestModels.ComplexNavigationsModel
{
    public class ComplexNavigationsContext : DbContext
    {
        public static readonly string StoreName = "ComplexNavigations";

        public ComplexNavigationsContext(IServiceProvider serviceProvider, DbContextOptions options)
            : base(serviceProvider, options)
        {
        }

        public DbSet<Level1> LevelOne { get; set; }
        public DbSet<Level2> LevelTwo { get; set; }
        public DbSet<Level3> LevelThree { get; set; }
        public DbSet<Level4> LevelFour { get; set; }
    }
}

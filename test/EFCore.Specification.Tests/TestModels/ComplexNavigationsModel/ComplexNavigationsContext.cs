// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel
{
    public class ComplexNavigationsContext : PoolableDbContext
    {
        public ComplexNavigationsContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Level1> LevelOne { get; set; }
        public DbSet<Level2> LevelTwo { get; set; }
        public DbSet<Level3> LevelThree { get; set; }
        public DbSet<Level4> LevelFour { get; set; }

        public DbSet<ComplexNavigationField> Fields { get; set; }
        public DbSet<ComplexNavigationString> MultilingualStrings { get; set; }
        public DbSet<ComplexNavigationGlobalization> Globalizations { get; set; }
        public DbSet<ComplexNavigationLanguage> Languages { get; set; }

        public DbSet<InheritanceBase1> InheritanceOne { get; set; }
        public DbSet<InheritanceBase2> InheritanceTwo { get; set; }
        public DbSet<InheritanceLeaf1> InheritanceLeafOne { get; set; }
        public DbSet<InheritanceLeaf2> InheritanceLeafTwo { get; set; }
    }
}

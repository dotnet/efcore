// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel
{
    public class ComplexNavigationsContext : DbContext
    {
        public static readonly string StoreName = "ComplexNavigations";

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

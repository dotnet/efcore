// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Data.Entity.FunctionalTests.TestModels.Inheritance
{
    public class AnimalContext : DbContext
    {
        public static readonly string StoreName = "Animals";

        public AnimalContext(IServiceProvider serviceProvider, DbContextOptions options)
            : base(serviceProvider, options)
        {
        }

        public virtual DbSet<Animal> Animals { get; set; }
        public virtual DbSet<Bird> Birds { get; set; }
        public virtual DbSet<Kiwi> Kiwis { get; set; }
    }
}

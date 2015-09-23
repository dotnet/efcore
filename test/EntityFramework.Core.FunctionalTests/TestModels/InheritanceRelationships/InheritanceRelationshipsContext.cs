// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Infrastructure;

namespace Microsoft.Data.Entity.FunctionalTests.TestModels.InheritanceRelationships
{
    public class InheritanceRelationshipsContext : DbContext
    {
        public static readonly string StoreName = "InheritanceRelationships";

        public InheritanceRelationshipsContext(IServiceProvider serviceProvider, DbContextOptions options)
            : base(serviceProvider, options)
        {
        }

        public DbSet<BaseInheritanceRelationshipEntity> BaseEntities { get; set; }
        public DbSet<DerivedInheritanceRelationshipEntity> DerivedEntities { get; set; }

        public DbSet<BaseReferenceOnBase> BaseReferencesOnBase { get; set; }
        public DbSet<BaseReferenceOnDerived> BaseReferencesOnDerived { get; set; }
        public DbSet<ReferenceOnBase> ReferencesOnBase { get; set; }
        public DbSet<ReferenceOnDerived> ReferencesOnDerived { get; set; }
        public DbSet<NestedReferenceBase> NestedReferences { get; set; }

        public DbSet<BaseCollectionOnBase> BaseCollectionsOnBase { get; set; }
        public DbSet<BaseCollectionOnDerived> BaseCollectionsOnDerived { get; set; }
        public DbSet<CollectionOnBase> CollectionsOnBase { get; set; }
        public DbSet<CollectionOnDerived> CollectionsOnDerived { get; set; }
        public DbSet<NestedCollectionBase> NestedCollections { get; set; }
    }
}

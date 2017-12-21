﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable ClassWithVirtualMembersNeverInherited.Local
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    public class ForeignKeyIndexConventionTest
    {
        [Fact]
        public void Does_not_override_foreign_key_index_uniqueness_when_referenced_key_changes()
        {
            var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

            var principalTypeBuilder = modelBuilder.Entity<PrincipalEntity>();
            var dependentTypeBuilder = modelBuilder.Entity<DependentEntity>();

            principalTypeBuilder.HasKey(t => t.InitialPrincipalId);
            dependentTypeBuilder.HasOne(t => t.Principal).WithOne().HasForeignKey<DependentEntity>(t => t.PrincipalId);
            dependentTypeBuilder.HasIndex(t => t.PrincipalId).IsUnique(false);
            principalTypeBuilder.HasKey(t => t.ChangedPrincipalId);

            var entityType = modelBuilder.Model.FindEntityType(typeof(DependentEntity));
            var property = entityType.FindProperty(nameof(DependentEntity.PrincipalId));
            var index = entityType.FindIndex(property);

            Assert.False(index.IsUnique);
        }

        private class PrincipalEntity
        {
            public int InitialPrincipalId { get; set; }

            public int ChangedPrincipalId { get; set; }
        }

        private class DependentEntity
        {
            public int PrincipalId { get; set; }

            public int DependentId { get; set; }

            public virtual PrincipalEntity Principal { get; set; }
        }
    }
}

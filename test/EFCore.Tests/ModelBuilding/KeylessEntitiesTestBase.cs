// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ModelBuilding
{
    public abstract partial class ModelBuilderTest
    {
        public abstract class KeylessEntitiesTestBase : ModelBuilderTestBase
        {
            [ConditionalFact]
            public virtual void Keyless_type_discovered_before_entity_type_does_not_leave_temp_id()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Ignore<Order>();
                modelBuilder.Ignore<CustomerDetails>();

                modelBuilder.Entity<KeylessEntity>().HasNoKey();
                modelBuilder.Entity<Customer>();

                modelBuilder.FinalizeModel();

                Assert.Null(modelBuilder.Model.FindEntityType(typeof(Customer))?.FindProperty("TempId"));
                Assert.Null(modelBuilder.Model.FindEntityType(typeof(KeylessEntity)).FindPrimaryKey());
            }

            [ConditionalFact]
            public virtual void Keyless_type_with_collection_navigations_does_not_throw()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Ignore<CustomerDetails>();

                modelBuilder.Entity<Customer>().HasNoKey();

                modelBuilder.FinalizeModel();

                Assert.Empty(modelBuilder.Model.FindEntityType(typeof(Customer)).GetNavigations());
                Assert.Null(modelBuilder.Model.FindEntityType(typeof(Customer)).FindPrimaryKey());
            }
        }
    }
}

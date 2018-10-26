
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Internal;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ModelBuilding
{
    public abstract partial class ModelBuilderTest
    {
        public abstract class QueryTypesTestBase : ModelBuilderTestBase
        {
            [Fact]
            public virtual void Entity_throws_when_called_for_query()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Query<Customer>();

                Assert.Equal(
                    CoreStrings.CannotAccessQueryAsEntity(nameof(Customer)),
                    Assert.Throws<InvalidOperationException>(
                        () => modelBuilder.Entity<Customer>()).Message);
            }

            [Fact]
            public virtual void Query_type_discovered_before_entity_type_does_not_leave_temp_id()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Ignore<Order>();
                modelBuilder.Ignore<CustomerDetails>();

                modelBuilder.Query<QueryType>();
                modelBuilder.Entity<Customer>();

                modelBuilder.Validate();

                Assert.Null(modelBuilder.Model.FindEntityType(typeof(Customer))?.FindProperty("TempId"));
            }

            [Fact]
            public virtual void Query_type_with_collection_navigations_does_not_throw()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Ignore<CustomerDetails>();

                modelBuilder.Query<Customer>();

                modelBuilder.Validate();

                Assert.Empty(modelBuilder.Model.FindEntityType(typeof(Customer)).GetNavigations());
            }
        }
    }
}

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
            public void Entity_throws_when_called_for_query()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Query<Customer>();

                Assert.Equal(
                    CoreStrings.CannotAccessQueryAsEntity(nameof(Customer)),
                    Assert.Throws<InvalidOperationException>(
                        () => modelBuilder.Entity<Customer>()).Message);
            }
        }
    }
}

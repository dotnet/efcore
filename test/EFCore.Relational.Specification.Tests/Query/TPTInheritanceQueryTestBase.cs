// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


// ReSharper disable InconsistentNaming
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestModels.InheritanceModel;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class TPTInheritanceQueryTestBase<TFixture> : InheritanceQueryTestBase<TFixture>
        where TFixture : TPTInheritanceQueryFixture, new()
    {
        public TPTInheritanceQueryTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        // Keyless entities does not have TPT
        public override Task Can_query_all_animal_views(bool async) => Task.CompletedTask;

        // TPT does not have discriminator
        public override Task Discriminator_used_when_projection_over_derived_type(bool async) => Task.CompletedTask;

        // TPT does not have discriminator
        public override Task Discriminator_used_when_projection_over_derived_type2(bool async) => Task.CompletedTask;

        // TPT does not have discriminator
        public override Task Discriminator_used_when_projection_over_of_type(bool async) => Task.CompletedTask;

        // TPT does not have discriminator
        public override Task Discriminator_with_cast_in_shadow_property(bool async) => Task.CompletedTask;

        [ConditionalFact]
        public virtual void Using_from_sql_throws()
        {
            using var context = CreateContext();

            var message = Assert.Throws<InvalidOperationException>(() => context.Set<Bird>().FromSqlRaw("Select * from Birds")).Message;

            Assert.Equal(RelationalStrings.NonTPHOnFromSqlNotSupported("FromSqlRaw", typeof(Bird).Name), message);

            message = Assert.Throws<InvalidOperationException>(() => context.Set<Bird>().FromSqlInterpolated($"Select * from Birds")).Message;

            Assert.Equal(RelationalStrings.NonTPHOnFromSqlNotSupported("FromSqlInterpolated", typeof(Bird).Name), message);
        }
    }
}

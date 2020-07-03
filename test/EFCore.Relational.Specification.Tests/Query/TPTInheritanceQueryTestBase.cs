// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


// ReSharper disable InconsistentNaming
using System.Threading.Tasks;
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
    }
}

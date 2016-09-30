// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Xunit;

namespace Microsoft.EntityFrameworkCore.InMemory.FunctionalTests
{
    public class InheritanceInMemoryTest : InheritanceTestBase<InheritanceInMemoryFixture>
    {
        public override void Discriminator_used_when_projection_over_derived_type2()
        {
            Assert.Equal(
                CoreStrings.PropertyNotFound("Discriminator", "Bird"),
                Assert.Throws<InvalidOperationException>(() =>
                        base.Discriminator_used_when_projection_over_derived_type2()).Message);
        }

        public override void Discriminator_with_cast_in_shadow_property()
        {
            Assert.Equal(
                CoreStrings.PropertyNotFound("Discriminator", "Animal"),
                Assert.Throws<InvalidOperationException>(() =>
                        base.Discriminator_with_cast_in_shadow_property()).Message);
        }

        public InheritanceInMemoryTest(InheritanceInMemoryFixture fixture)
            : base(fixture)
        {
        }
    }
}

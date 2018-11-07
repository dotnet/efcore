// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Internal;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class InheritanceInMemoryTest : InheritanceTestBase<InheritanceInMemoryFixture>
    {
        public InheritanceInMemoryTest(InheritanceInMemoryFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestLoggerFactory.TestOutputHelper = testOutputHelper;
        }

        public override void Discriminator_used_when_projection_over_derived_type2()
        {
            Assert.Equal(
                CoreStrings.PropertyNotFound(property: "Discriminator", entityType: "Bird"),
                Assert.Throws<InvalidOperationException>(
                    () => base.Discriminator_used_when_projection_over_derived_type2()).Message);
        }

        public override void Discriminator_with_cast_in_shadow_property()
        {
            Assert.Equal(
                CoreStrings.PropertyNotFound(property: "Discriminator", entityType: "Animal"),
                Assert.Throws<InvalidOperationException>(
                    () => base.Discriminator_with_cast_in_shadow_property()).Message);
        }

        [Fact(Skip = "See issue#13857")]
        public override void Can_query_all_animal_views()
        {
            base.Can_query_all_animal_views();
        }
    }
}

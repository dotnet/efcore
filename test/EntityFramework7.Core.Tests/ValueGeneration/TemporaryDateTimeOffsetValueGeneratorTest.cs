// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.ValueGeneration;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ValueGeneration
{
    public class TemporaryDateTimeOffsetValueGeneratorTest
    {
        [Fact]
        public void Can_create_values_for_DateTime_types()
        {
            var generator = new TemporaryDateTimeOffsetValueGenerator();
            Assert.Equal(new DateTimeOffset(1, TimeSpan.Zero), generator.Next());
            Assert.Equal(new DateTimeOffset(2, TimeSpan.Zero), generator.Next());
        }

        [Fact]
        public void Generates_temporary_values()
        {
            Assert.True(new TemporaryDateTimeOffsetValueGenerator().GeneratesTemporaryValues);
        }
    }
}

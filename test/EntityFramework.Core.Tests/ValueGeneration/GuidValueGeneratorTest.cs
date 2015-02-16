// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.ValueGeneration;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ValueGeneration
{
    public class GuidValueGeneratorTest
    {
        [Fact]
        public void Creates_GUIDs()
        {
            var sequentialGuidIdentityGenerator = new GuidValueGenerator();

            var values = new HashSet<Guid>();
            for (var i = 0; i < 100; i++)
            {
                var generatedValue = sequentialGuidIdentityGenerator.Next(new DbContextService<DataStoreServices>(() => null));

                values.Add(generatedValue);
            }

            Assert.Equal(100, values.Count);
        }

        [Fact]
        public void Does_not_generate_temp_values()
        {
            Assert.False(new GuidValueGenerator().GeneratesTemporaryValues);
        }
    }
}

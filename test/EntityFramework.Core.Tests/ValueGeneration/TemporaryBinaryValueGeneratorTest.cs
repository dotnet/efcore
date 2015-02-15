// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.ValueGeneration;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ValueGeneration
{
    public class TemporaryBinaryValueGeneratorTest
    {
        private static readonly Model _model = TestHelpers.Instance.BuildModelFor<WithBinary>();

        [Fact]
        public void Creates_GUID_arrays()
        {
            var generator = new TemporaryBinaryValueGenerator();

            var entry = TestHelpers.Instance.CreateInternalEntry<WithBinary>(_model);
            var property = entry.EntityType.GetProperty("Id");

            var values = new HashSet<Guid>();
            for (var i = 0; i < 100; i++)
            {
                var generatedValue = generator.Next(property, new DbContextService<DataStoreServices>(() => null));

                values.Add(new Guid((byte[])generatedValue));
            }

            Assert.Equal(100, values.Count);
        }

        [Fact]
        public void Generates_temp_values()
        {
            Assert.True(new TemporaryBinaryValueGenerator().GeneratesTemporaryValues);
        }

        private class WithBinary
        {
            public byte[] Id { get; set; }
        }
    }
}

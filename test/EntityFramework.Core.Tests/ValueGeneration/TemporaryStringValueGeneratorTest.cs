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
    public class TemporaryStringValueGeneratorTest
    {
        private static readonly Model _model = TestHelpers.Instance.BuildModelFor<WithString>();

        [Fact]
        public void Creates_GUID_strings()
        {
            var generator = new TemporaryStringValueGenerator();

            var entry = TestHelpers.Instance.CreateInternalEntry<WithString>(_model);
            var property = entry.EntityType.GetProperty("Id");

            var values = new HashSet<Guid>();
            for (var i = 0; i < 100; i++)
            {
                var generatedValue = generator.Next(property, new DbContextService<DataStoreServices>(() => null));

                values.Add(Guid.Parse((string)generatedValue));
            }

            Assert.Equal(100, values.Count);
        }

        [Fact]
        public void Generates_temp_values()
        {
            Assert.True(new TemporaryStringValueGenerator().GeneratesTemporaryValues);
        }

        private class WithString
        {
            public string Id { get; set; }
        }
    }
}

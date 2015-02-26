// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Metadata.ModelConventions;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Metadata.ModelConventions
{
    public class KeyConventionTest
    {
        private class SampleEntity
        {
            public int Id { get; set; }
            public string Title { get; set; }
        }

        [Fact]
        public void ConfigureKeyProperties_set_GenerateValueOnAdd_flag_for_key_properties()
        {
            var conventions = new ConventionSet();
            conventions.EntityTypeAddedConventions.Add(new PropertiesConvention());

            var modelBuilder = new InternalModelBuilder(new Model(), conventions);
            var entityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);

            var properties = new List<string>() { "Id", "Title" };
            var keyBuilder = entityBuilder.Key(properties, ConfigurationSource.Convention);

            Assert.Same(keyBuilder, new KeyConvention().Apply(keyBuilder));

            var keyProperties = keyBuilder.Metadata.Properties;

            Assert.NotNull(keyProperties[0].GenerateValueOnAdd);
            Assert.NotNull(keyProperties[1].GenerateValueOnAdd);

            Assert.True(keyProperties[0].GenerateValueOnAdd.Value);
            Assert.True(keyProperties[1].GenerateValueOnAdd.Value);
        }
    }
}

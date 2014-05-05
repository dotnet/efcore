// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System;
using System.Linq;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class EntityConfigurationTest
    {
        [Fact]
        public void Mutating_methods_throw_when_configuratin_locked()
        {
            IDbContextOptionsConstruction configuration = new ImmutableDbContextOptions();
            configuration.Lock();

            Assert.Equal(
                Strings.FormatEntityConfigurationLocked("Model"),
                Assert.Throws<InvalidOperationException>(() => configuration.Model = Mock.Of<IModel>()).Message);

            Assert.Equal(
                Strings.FormatEntityConfigurationLocked("AddOrUpdateExtension"),
                Assert.Throws<InvalidOperationException>(() => configuration.AddOrUpdateExtension<FakeEntityConfigurationExtension>(e => { })).Message);
        }

        [Fact]
        public void Can_update_an_existing_extension()
        {
            IDbContextOptionsConstruction configuration = new ImmutableDbContextOptions();

            configuration.AddOrUpdateExtension<FakeEntityConfigurationExtension>(e => e.Something += "One");
            configuration.AddOrUpdateExtension<FakeEntityConfigurationExtension>(e => e.Something += "Two");

            Assert.Equal(
                "OneTwo", ((ImmutableDbContextOptions)configuration).Extensions.OfType<FakeEntityConfigurationExtension>().Single().Something);
        }

        private class FakeEntityConfigurationExtension : EntityConfigurationExtension
        {
            public string Something { get; set; }

            protected internal override void ApplyServices(EntityServicesBuilder builder)
            {
            }
        }
    }
}

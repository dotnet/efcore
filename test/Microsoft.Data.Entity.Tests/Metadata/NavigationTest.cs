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
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Metadata
{
    public class NavigationTest
    {
        [Fact]
        public void Members_check_arguments()
        {
            Assert.Equal(
                "foreignKey",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => new Navigation(null, "Handlebars")).ParamName);
            Assert.Equal(
                Strings.FormatArgumentIsEmpty("name"),
                Assert.Throws<ArgumentException>(() => new Navigation(new Mock<ForeignKey>().Object, "")).Message);
        }

        [Fact]
        public void Can_create_navigation()
        {
            var foreignKey = new Mock<ForeignKey>().Object;

            var navigation = new Navigation(foreignKey, "Deception");

            Assert.Same(foreignKey, navigation.ForeignKey);
            Assert.Equal("Deception", navigation.Name);
            Assert.Null(navigation.EntityType);

            Assert.Same(foreignKey, ((INavigation)navigation).ForeignKey);
            Assert.Null(((INavigation)navigation).EntityType);
        }

        [Fact]
        public void Can_set_entity_type()
        {
            var navigation = new Navigation(new Mock<ForeignKey>().Object, "TheBattle");
            var entityType = new Mock<EntityType>().Object;

            navigation.EntityType = entityType;

            Assert.Same(entityType, navigation.EntityType);
            Assert.Same(entityType, ((INavigation)navigation).EntityType);
        }
    }
}

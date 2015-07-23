// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Metadata
{
    public class NavigationTest
    {
        [Fact]
        public void Can_create_navigation()
        {
            var foreignKey = CreateForeignKey();

            var navigation = new Navigation("Deception", foreignKey);

            Assert.Same(foreignKey, navigation.ForeignKey);
            Assert.Equal("Deception", navigation.Name);
            Assert.Same(foreignKey.DeclaringEntityType, navigation.DeclaringEntityType);
        }

        private ForeignKey CreateForeignKey()
        {
            var model = new Model();
            var entityType = model.AddEntityType("E");
            var idProperty = entityType.AddProperty("id", typeof(int), shadowProperty: true);
            var key = entityType.SetPrimaryKey(idProperty);
            var fkProperty = entityType.AddProperty("p", typeof(int), shadowProperty: true);
            return entityType.AddForeignKey(fkProperty, key, entityType);
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.Data.Entity.Metadata.Internal
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
            var idProperty = entityType.AddProperty("id", typeof(int));
            var key = entityType.SetPrimaryKey(idProperty);
            var fkProperty = entityType.AddProperty("p", typeof(int));
            return entityType.AddForeignKey(fkProperty, key, entityType);
        }
    }
}

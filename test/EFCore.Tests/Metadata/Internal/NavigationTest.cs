// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Moq;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Tests.Metadata.Internal
{
    public class NavigationTest
    {
        [Fact]
        public void Use_of_custom_INavigation_throws()
        {
            Assert.Equal(
                CoreStrings.CustomMetadata(nameof(Use_of_custom_INavigation_throws), nameof(INavigation), "INavigationProxy"),
                Assert.Throws<NotSupportedException>(() => Mock.Of<INavigation>().AsNavigation()).Message);
        }

        [Fact]
        public void Can_create_navigation()
        {
            var foreignKey = CreateForeignKey();

            var navigation = foreignKey.HasDependentToPrincipal(E.DeceptionProperty);

            Assert.Same(foreignKey, navigation.ForeignKey);
            Assert.Equal(nameof(E.Deception), navigation.Name);
            Assert.Same(foreignKey.DeclaringEntityType, navigation.DeclaringEntityType);
        }

        private ForeignKey CreateForeignKey()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(E));
            var idProperty = entityType.AddProperty("id", typeof(int));
            var key = entityType.SetPrimaryKey(idProperty);
            var fkProperty = entityType.AddProperty("p", typeof(int));
            return entityType.AddForeignKey(fkProperty, key, entityType);
        }

        private class E
        {
            public static readonly PropertyInfo DeceptionProperty = typeof(E).GetProperty(nameof(Deception));

            public E Deception { get; set; }
        }
    }
}

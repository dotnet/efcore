// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public class NavigationTest
    {
        [Fact]
        public void Use_of_custom_INavigation_throws()
        {
            var navigation = new FakeNavigation();

            Assert.Equal(
                CoreStrings.CustomMetadata(nameof(Use_of_custom_INavigation_throws), nameof(INavigation), nameof(FakeNavigation)),
                Assert.Throws<NotSupportedException>(() => navigation.AsNavigation()).Message);
        }

        private class FakeNavigation : INavigation
        {
            public object this[string name] => throw new NotImplementedException();
            public IAnnotation FindAnnotation(string name) => throw new NotImplementedException();
            public IEnumerable<IAnnotation> GetAnnotations() => throw new NotImplementedException();
            public string Name { get; }
            public ITypeBase DeclaringType { get; }
            public Type ClrType { get; }
            public PropertyInfo PropertyInfo { get; }
            public FieldInfo FieldInfo { get; }
            public bool IsShadowProperty { get; }
            public IEntityType DeclaringEntityType { get; }
            public IForeignKey ForeignKey { get; }
            public bool IsEagerLoaded { get; }
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

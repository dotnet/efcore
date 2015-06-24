// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Xunit;

// ReSharper disable once CheckNamespace
namespace Microsoft.Data.Entity.Tests
{
    public partial class ModelBuilderTest
    {
        public abstract class InheritanceTestBase : ModelBuilderTestBase
        {
            [Fact]
            public virtual void Can_set_switch_and_remove_base_type()
            {
                var modelBuilder = CreateModelBuilder();

                var pickleBuilder = modelBuilder.Entity<Pickle>();
                var pickle = pickleBuilder.Metadata;

                Assert.Null(pickle.BaseType);
                var initialProperties = pickle.Properties.ToList();
                var initialNavigations = pickle.Navigations.ToList();
                var initialIndexes = pickle.Indexes.ToList();
                var initialForeignKey = pickle.GetForeignKeys().ToList();
                var initialKeys = pickle.GetKeys().ToList();

                pickleBuilder.BaseEntity<Ingredient>();

                Assert.Same(pickle.BaseType.ClrType, typeof(Ingredient));
                AssertEqual(initialProperties.Select(p => p.Name), pickle.Properties.Select(p =>p.Name));
                AssertEqual(initialNavigations.Select(p => p.Name), pickle.Navigations.Select(p => p.Name));
                AssertEqual(initialIndexes, pickle.Indexes);
                AssertEqual(initialForeignKey, pickle.GetForeignKeys());
                AssertEqual(initialKeys, pickle.GetKeys());
                var ingredient = pickle.BaseType;
                AssertEqual(initialProperties.Select(p => p.Name), ingredient.Properties.Select(p => p.Name));
                AssertEqual(initialNavigations.Select(p => p.Name), ingredient.Navigations.Select(p => p.Name));
                AssertEqual(initialIndexes, ingredient.Indexes);
                AssertEqual(initialForeignKey, ingredient.GetForeignKeys());
                AssertEqual(initialKeys, ingredient.GetKeys());
                /*
                pickleBuilder.BaseEntity(null);

                Assert.Null(pickle.BaseType);
                AssertEqual(initialProperties.Select(p => p.Name), pickle.Properties.Select(p => p.Name));
                AssertEqual(initialNavigations.Select(p => p.Name), pickle.Navigations.Select(p => p.Name));
                AssertEqual(initialIndexes, pickle.Indexes);
                AssertEqual(initialForeignKey, pickle.GetForeignKeys());
                AssertEqual(initialKeys, pickle.GetKeys());

                AssertEqual(initialProperties, ingredient.Properties);
                AssertEqual(initialNavigations, ingredient.Navigations);
                AssertEqual(initialIndexes, ingredient.Indexes);
                AssertEqual(initialForeignKey, ingredient.GetForeignKeys());
                AssertEqual(initialKeys, ingredient.GetKeys());
                */
            }
        }
    }
}

// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Update;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests.Update
{
    public class ModificationCommandComparerTest
    {
        [Fact]
        public void Compare_returns_0_only_for_commands_that_are_equal()
        {
            var mCC = new ModificationCommandComparer();

            var configuration = new DbContext(new DbContextOptions().UseInMemoryStore(persist: false)).Configuration;

            var entityType1 = new EntityType(typeof(object));
            var key1 = entityType1.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false);
            entityType1.SetKey(key1);
            var stateEntry1 = new MixedStateEntry(configuration, entityType1, new object());
            stateEntry1[key1] = 0;
            stateEntry1.EntityState = EntityState.Added;
            var modificationCommandAdded = new ModificationCommand(new SchemaQualifiedName("A"), new ParameterNameGenerator());
            modificationCommandAdded.AddStateEntry(stateEntry1);

            var entityType2 = new EntityType(typeof(object));
            var key2 = entityType2.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false);
            entityType2.SetKey(key2);
            var stateEntry2 = new MixedStateEntry(configuration, entityType2, new object());
            stateEntry2[key2] = 0;
            stateEntry2.EntityState = EntityState.Modified;
            var modificationCommandModified = new ModificationCommand(new SchemaQualifiedName("A"), new ParameterNameGenerator());
            modificationCommandModified.AddStateEntry(stateEntry2);

            var entityType3 = new EntityType(typeof(object));
            var key3 = entityType3.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false);
            entityType3.SetKey(key3);
            var stateEntry3 = new MixedStateEntry(configuration, entityType3, new object());
            stateEntry3[key3] = 0;
            stateEntry3.EntityState = EntityState.Deleted;
            var modificationCommandDeleted = new ModificationCommand(new SchemaQualifiedName("A"), new ParameterNameGenerator());
            modificationCommandDeleted.AddStateEntry(stateEntry3);

            Assert.True(0 == mCC.Compare(modificationCommandAdded, modificationCommandAdded));
            Assert.True(0 == mCC.Compare(null, null));
            Assert.True(0 == mCC.Compare(
                new ModificationCommand(new SchemaQualifiedName("A", "dbo"), new ParameterNameGenerator()),
                new ModificationCommand(new SchemaQualifiedName("A", "dbo"), new ParameterNameGenerator())));

            Assert.True(0 > mCC.Compare(null, new ModificationCommand(new SchemaQualifiedName("A"), new ParameterNameGenerator())));
            Assert.True(0 < mCC.Compare(new ModificationCommand(new SchemaQualifiedName("A"), new ParameterNameGenerator()), null));

            Assert.True(0 > mCC.Compare(
                new ModificationCommand(new SchemaQualifiedName("A"), new ParameterNameGenerator()),
                new ModificationCommand(new SchemaQualifiedName("A", "dbo"), new ParameterNameGenerator())));
            Assert.True(0 < mCC.Compare(
                new ModificationCommand(new SchemaQualifiedName("A", "dbo"), new ParameterNameGenerator()),
                new ModificationCommand(new SchemaQualifiedName("A"), new ParameterNameGenerator())));

            Assert.True(0 > mCC.Compare(
                new ModificationCommand(new SchemaQualifiedName("A", "dbo"), new ParameterNameGenerator()),
                new ModificationCommand(new SchemaQualifiedName("A", "foo"), new ParameterNameGenerator())));
            Assert.True(0 < mCC.Compare(
                new ModificationCommand(new SchemaQualifiedName("A", "foo"), new ParameterNameGenerator()),
                new ModificationCommand(new SchemaQualifiedName("A", "dbo"), new ParameterNameGenerator())));

            Assert.True(0 > mCC.Compare(
                new ModificationCommand(new SchemaQualifiedName("A"), new ParameterNameGenerator()),
                new ModificationCommand(new SchemaQualifiedName("B"), new ParameterNameGenerator())));
            Assert.True(0 < mCC.Compare(
                new ModificationCommand(new SchemaQualifiedName("B"), new ParameterNameGenerator()),
                new ModificationCommand(new SchemaQualifiedName("A"), new ParameterNameGenerator())));

            Assert.True(0 > mCC.Compare(modificationCommandModified, modificationCommandAdded));
            Assert.True(0 < mCC.Compare(modificationCommandAdded, modificationCommandModified));

            Assert.True(0 > mCC.Compare(modificationCommandDeleted, modificationCommandAdded));
            Assert.True(0 < mCC.Compare(modificationCommandAdded, modificationCommandDeleted));

            Assert.True(0 > mCC.Compare(modificationCommandDeleted, modificationCommandModified));
            Assert.True(0 < mCC.Compare(modificationCommandModified, modificationCommandDeleted));
        }
    }
}

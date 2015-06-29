// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Update;
using Microsoft.Framework.DependencyInjection;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Update
{
    public class ModificationCommandComparerTest
    {
        [Fact]
        public void Compare_returns_0_only_for_commands_that_are_equal()
        {
            var model = new Entity.Metadata.Model();
            var entityType = model.AddEntityType(typeof(object));

            var optionsBuilder = new DbContextOptionsBuilder()
                .UseModel(model);
            optionsBuilder.UseInMemoryDatabase(persist: false);

            var contextServices = new DbContext(optionsBuilder.Options).GetService();
            var stateManager = contextServices.GetRequiredService<IStateManager>();

            var key = entityType.GetOrAddProperty("Id", typeof(int), shadowProperty: true);
            entityType.GetOrSetPrimaryKey(key);

            var entry1 = stateManager.GetOrCreateEntry(new object());
            entry1[key] = 1;
            entry1.SetEntityState(EntityState.Added);
            var modificationCommandAdded = new ModificationCommand("A", null, new ParameterNameGenerator(), p => p.Relational(), new UntypedValueBufferFactoryFactory());
            modificationCommandAdded.AddEntry(entry1);

            var entry2 = stateManager.GetOrCreateEntry(new object());
            entry2[key] = 2;
            entry2.SetEntityState(EntityState.Modified);
            var modificationCommandModified = new ModificationCommand("A", null, new ParameterNameGenerator(), p => p.Relational(), new UntypedValueBufferFactoryFactory());
            modificationCommandModified.AddEntry(entry2);

            var entry3 = stateManager.GetOrCreateEntry(new object());
            entry3[key] = 3;
            entry3.SetEntityState(EntityState.Deleted);
            var modificationCommandDeleted = new ModificationCommand("A", null, new ParameterNameGenerator(), p => p.Relational(), new UntypedValueBufferFactoryFactory());
            modificationCommandDeleted.AddEntry(entry3);

            var mCC = new ModificationCommandComparer();

            Assert.True(0 == mCC.Compare(modificationCommandAdded, modificationCommandAdded));
            Assert.True(0 == mCC.Compare(null, null));
            Assert.True(0 == mCC.Compare(
                new ModificationCommand("A", "dbo", new ParameterNameGenerator(), p => p.Relational(), new UntypedValueBufferFactoryFactory()),
                new ModificationCommand("A", "dbo", new ParameterNameGenerator(), p => p.Relational(), new UntypedValueBufferFactoryFactory())));

            Assert.True(0 > mCC.Compare(null, new ModificationCommand("A", null, new ParameterNameGenerator(), p => p.Relational(), new UntypedValueBufferFactoryFactory())));
            Assert.True(0 < mCC.Compare(new ModificationCommand("A", null, new ParameterNameGenerator(), p => p.Relational(), new UntypedValueBufferFactoryFactory()), null));

            Assert.True(0 > mCC.Compare(
                new ModificationCommand("A", null, new ParameterNameGenerator(), p => p.Relational(), new UntypedValueBufferFactoryFactory()),
                new ModificationCommand("A", "dbo", new ParameterNameGenerator(), p => p.Relational(), new UntypedValueBufferFactoryFactory())));
            Assert.True(0 < mCC.Compare(
                new ModificationCommand("A", "dbo", new ParameterNameGenerator(), p => p.Relational(), new UntypedValueBufferFactoryFactory()),
                new ModificationCommand("A", null, new ParameterNameGenerator(), p => p.Relational(), new UntypedValueBufferFactoryFactory())));

            Assert.True(0 > mCC.Compare(
                new ModificationCommand("A", "dbo", new ParameterNameGenerator(), p => p.Relational(), new UntypedValueBufferFactoryFactory()),
                new ModificationCommand("A", "foo", new ParameterNameGenerator(), p => p.Relational(), new UntypedValueBufferFactoryFactory())));
            Assert.True(0 < mCC.Compare(
                new ModificationCommand("A", "foo", new ParameterNameGenerator(), p => p.Relational(), new UntypedValueBufferFactoryFactory()),
                new ModificationCommand("A", "dbo", new ParameterNameGenerator(), p => p.Relational(), new UntypedValueBufferFactoryFactory())));

            Assert.True(0 > mCC.Compare(
                new ModificationCommand("A", null, new ParameterNameGenerator(), p => p.Relational(), new UntypedValueBufferFactoryFactory()),
                new ModificationCommand("B", null, new ParameterNameGenerator(), p => p.Relational(), new UntypedValueBufferFactoryFactory())));
            Assert.True(0 < mCC.Compare(
                new ModificationCommand("B", null, new ParameterNameGenerator(), p => p.Relational(), new UntypedValueBufferFactoryFactory()),
                new ModificationCommand("A", null, new ParameterNameGenerator(), p => p.Relational(), new UntypedValueBufferFactoryFactory())));

            Assert.True(0 > mCC.Compare(modificationCommandModified, modificationCommandAdded));
            Assert.True(0 < mCC.Compare(modificationCommandAdded, modificationCommandModified));

            Assert.True(0 > mCC.Compare(modificationCommandDeleted, modificationCommandAdded));
            Assert.True(0 < mCC.Compare(modificationCommandAdded, modificationCommandDeleted));

            Assert.True(0 > mCC.Compare(modificationCommandDeleted, modificationCommandModified));
            Assert.True(0 < mCC.Compare(modificationCommandModified, modificationCommandDeleted));
        }
    }
}

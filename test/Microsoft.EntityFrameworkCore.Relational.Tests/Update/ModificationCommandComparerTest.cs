// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Tests.Update
{
    public class ModificationCommandComparerTest
    {
        [Fact]
        public void Compare_returns_0_only_for_commands_that_are_equal()
        {
            var model = new EntityFrameworkCore.Metadata.Internal.Model();
            var entityType = model.AddEntityType(typeof(object));

            var optionsBuilder = new DbContextOptionsBuilder()
                .UseModel(model);
            optionsBuilder.UseInMemoryDatabase();

            var contextServices = new DbContext(optionsBuilder.Options).GetInfrastructure();
            var stateManager = contextServices.GetRequiredService<IStateManager>();

            var key = entityType.AddProperty("Id", typeof(int));
            entityType.GetOrSetPrimaryKey(key);

            var entry1 = stateManager.GetOrCreateEntry(new object());
            entry1[key] = 1;
            entry1.SetEntityState(EntityState.Added);
            var modificationCommandAdded = new ModificationCommand("A", null, new ParameterNameGenerator(), p => p.TestProvider());
            modificationCommandAdded.AddEntry(entry1);

            var entry2 = stateManager.GetOrCreateEntry(new object());
            entry2[key] = 2;
            entry2.SetEntityState(EntityState.Modified);
            var modificationCommandModified = new ModificationCommand("A", null, new ParameterNameGenerator(), p => p.TestProvider());
            modificationCommandModified.AddEntry(entry2);

            var entry3 = stateManager.GetOrCreateEntry(new object());
            entry3[key] = 3;
            entry3.SetEntityState(EntityState.Deleted);
            var modificationCommandDeleted = new ModificationCommand("A", null, new ParameterNameGenerator(), p => p.TestProvider());
            modificationCommandDeleted.AddEntry(entry3);

            var mCC = new ModificationCommandComparer();

            Assert.True(0 == mCC.Compare(modificationCommandAdded, modificationCommandAdded));
            Assert.True(0 == mCC.Compare(null, null));
            Assert.True(0 == mCC.Compare(
                new ModificationCommand("A", "dbo", new ParameterNameGenerator(), p => p.TestProvider()),
                new ModificationCommand("A", "dbo", new ParameterNameGenerator(), p => p.TestProvider())));

            Assert.True(0 > mCC.Compare(null, new ModificationCommand("A", null, new ParameterNameGenerator(), p => p.TestProvider())));
            Assert.True(0 < mCC.Compare(new ModificationCommand("A", null, new ParameterNameGenerator(), p => p.TestProvider()), null));

            Assert.True(0 > mCC.Compare(
                new ModificationCommand("A", null, new ParameterNameGenerator(), p => p.TestProvider()),
                new ModificationCommand("A", "dbo", new ParameterNameGenerator(), p => p.TestProvider())));
            Assert.True(0 < mCC.Compare(
                new ModificationCommand("A", "dbo", new ParameterNameGenerator(), p => p.TestProvider()),
                new ModificationCommand("A", null, new ParameterNameGenerator(), p => p.TestProvider())));

            Assert.True(0 > mCC.Compare(
                new ModificationCommand("A", "dbo", new ParameterNameGenerator(), p => p.TestProvider()),
                new ModificationCommand("A", "foo", new ParameterNameGenerator(), p => p.TestProvider())));
            Assert.True(0 < mCC.Compare(
                new ModificationCommand("A", "foo", new ParameterNameGenerator(), p => p.TestProvider()),
                new ModificationCommand("A", "dbo", new ParameterNameGenerator(), p => p.TestProvider())));

            Assert.True(0 > mCC.Compare(
                new ModificationCommand("A", null, new ParameterNameGenerator(), p => p.TestProvider()),
                new ModificationCommand("B", null, new ParameterNameGenerator(), p => p.TestProvider())));
            Assert.True(0 < mCC.Compare(
                new ModificationCommand("B", null, new ParameterNameGenerator(), p => p.TestProvider()),
                new ModificationCommand("A", null, new ParameterNameGenerator(), p => p.TestProvider())));

            Assert.True(0 > mCC.Compare(modificationCommandModified, modificationCommandAdded));
            Assert.True(0 < mCC.Compare(modificationCommandAdded, modificationCommandModified));

            Assert.True(0 > mCC.Compare(modificationCommandDeleted, modificationCommandAdded));
            Assert.True(0 < mCC.Compare(modificationCommandAdded, modificationCommandDeleted));

            Assert.True(0 > mCC.Compare(modificationCommandDeleted, modificationCommandModified));
            Assert.True(0 < mCC.Compare(modificationCommandModified, modificationCommandDeleted));
        }
    }
}

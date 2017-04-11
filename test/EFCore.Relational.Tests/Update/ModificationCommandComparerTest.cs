// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Relational.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Relational.Tests.Update
{
    public class ModificationCommandComparerTest
    {
        [Fact]
        public void Compare_returns_0_only_for_commands_that_are_equal()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(object));

            var optionsBuilder = new DbContextOptionsBuilder()
                .UseModel(model);
            optionsBuilder.UseTransientInMemoryDatabase();

            var stateManager = new DbContext(optionsBuilder.Options).GetService<IStateManager>();

            var key = entityType.AddProperty("Id", typeof(int));
            entityType.GetOrSetPrimaryKey(key);

            var entry1 = stateManager.GetOrCreateEntry(new object());
            entry1[key] = 1;
            entry1.SetEntityState(EntityState.Added);
            var modificationCommandAdded = new ModificationCommand("A", null, new ParameterNameGenerator().GenerateNext, p => p.TestProvider());
            modificationCommandAdded.AddEntry(entry1);

            var entry2 = stateManager.GetOrCreateEntry(new object());
            entry2[key] = 2;
            entry2.SetEntityState(EntityState.Modified);
            var modificationCommandModified = new ModificationCommand("A", null, new ParameterNameGenerator().GenerateNext, p => p.TestProvider());
            modificationCommandModified.AddEntry(entry2);

            var entry3 = stateManager.GetOrCreateEntry(new object());
            entry3[key] = 3;
            entry3.SetEntityState(EntityState.Deleted);
            var modificationCommandDeleted = new ModificationCommand("A", null, new ParameterNameGenerator().GenerateNext, p => p.TestProvider());
            modificationCommandDeleted.AddEntry(entry3);

            var mCC = new ModificationCommandComparer();

            Assert.True(0 == mCC.Compare(modificationCommandAdded, modificationCommandAdded));
            Assert.True(0 == mCC.Compare(null, null));
            Assert.True(0 == mCC.Compare(
                            new ModificationCommand("A", "dbo", new ParameterNameGenerator().GenerateNext, p => p.TestProvider()),
                            new ModificationCommand("A", "dbo", new ParameterNameGenerator().GenerateNext, p => p.TestProvider())));

            Assert.True(0 > mCC.Compare(null, new ModificationCommand("A", null, new ParameterNameGenerator().GenerateNext, p => p.TestProvider())));
            Assert.True(0 < mCC.Compare(new ModificationCommand("A", null, new ParameterNameGenerator().GenerateNext, p => p.TestProvider()), null));

            Assert.True(0 > mCC.Compare(
                            new ModificationCommand("A", null, new ParameterNameGenerator().GenerateNext, p => p.TestProvider()),
                            new ModificationCommand("A", "dbo", new ParameterNameGenerator().GenerateNext, p => p.TestProvider())));
            Assert.True(0 < mCC.Compare(
                            new ModificationCommand("A", "dbo", new ParameterNameGenerator().GenerateNext, p => p.TestProvider()),
                            new ModificationCommand("A", null, new ParameterNameGenerator().GenerateNext, p => p.TestProvider())));

            Assert.True(0 > mCC.Compare(
                            new ModificationCommand("A", "dbo", new ParameterNameGenerator().GenerateNext, p => p.TestProvider()),
                            new ModificationCommand("A", "foo", new ParameterNameGenerator().GenerateNext, p => p.TestProvider())));
            Assert.True(0 < mCC.Compare(
                            new ModificationCommand("A", "foo", new ParameterNameGenerator().GenerateNext, p => p.TestProvider()),
                            new ModificationCommand("A", "dbo", new ParameterNameGenerator().GenerateNext, p => p.TestProvider())));

            Assert.True(0 > mCC.Compare(
                            new ModificationCommand("A", null, new ParameterNameGenerator().GenerateNext, p => p.TestProvider()),
                            new ModificationCommand("B", null, new ParameterNameGenerator().GenerateNext, p => p.TestProvider())));
            Assert.True(0 < mCC.Compare(
                            new ModificationCommand("B", null, new ParameterNameGenerator().GenerateNext, p => p.TestProvider()),
                            new ModificationCommand("A", null, new ParameterNameGenerator().GenerateNext, p => p.TestProvider())));

            Assert.True(0 > mCC.Compare(modificationCommandModified, modificationCommandAdded));
            Assert.True(0 < mCC.Compare(modificationCommandAdded, modificationCommandModified));

            Assert.True(0 > mCC.Compare(modificationCommandDeleted, modificationCommandAdded));
            Assert.True(0 < mCC.Compare(modificationCommandAdded, modificationCommandDeleted));

            Assert.True(0 > mCC.Compare(modificationCommandDeleted, modificationCommandModified));
            Assert.True(0 < mCC.Compare(modificationCommandModified, modificationCommandDeleted));
        }

        [Fact]
        public void Compare_returns_0_only_for_entries_that_have_same_key_values()
        {
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<short>(-1, 1);
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic(1, 2);
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<long>(1, 2);
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<double>(1, 2);
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<decimal>(1, 2);
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<float>(1, 2);
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<byte>(1, 2);
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<ushort>(1, 2);
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<uint>(1, 2);
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<ulong>(1, 2);
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<sbyte>(1, 2);
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic(false, true);
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic('1', '2');
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic(new DateTime(1, 1, 1), new DateTime(1, 1, 2));
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic(
                new DateTimeOffset(new DateTime(10, 1, 1), TimeSpan.FromMinutes(2)), new DateTimeOffset(new DateTime(10, 1, 1), TimeSpan.FromMinutes(1)));
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(2));
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic(new Guid(), Guid.NewGuid());
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic(FlagsEnum.First, FlagsEnum.First | FlagsEnum.Second);

            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<short?>(-1, 1);
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<int?>(1, 2);
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<long?>(1, 2);
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<double?>(1, 2);
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<decimal?>(1, 2);
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<float?>(1, 2);
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<byte?>(1, 2);
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<ushort?>(1, 2);
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<uint?>(1, 2);
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<ulong?>(1, 2);
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<sbyte?>(1, 2);
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<bool?>(false, true);
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<char?>('1', '2');
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<DateTime?>(new DateTime(1, 1, 1), new DateTime(1, 1, 2));
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<DateTimeOffset?>(
                new DateTimeOffset(new DateTime(10, 1, 1), TimeSpan.FromMinutes(2)), new DateTimeOffset(new DateTime(10, 1, 1), TimeSpan.FromMinutes(1)));
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<TimeSpan?>(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(2));
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<Guid?>(new Guid(), Guid.NewGuid());
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<FlagsEnum?>(FlagsEnum.Default, FlagsEnum.First | FlagsEnum.Second);

            Compare_returns_0_only_for_entries_that_have_same_key_values_generic("1", "2");
        }

        private void Compare_returns_0_only_for_entries_that_have_same_key_values_generic<T>(T value1, T value2)
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(object));

            var optionsBuilder = new DbContextOptionsBuilder().UseModel(model).UseTransientInMemoryDatabase();

            var stateManager = new DbContext(optionsBuilder.Options).GetService<IStateManager>();

            var keyProperty = entityType.AddProperty("Id", typeof(T));
            keyProperty.IsNullable = false;
            entityType.GetOrSetPrimaryKey(keyProperty);

            var entry1 = stateManager.GetOrCreateEntry(new object());
            entry1[keyProperty] = value1;
            entry1.SetEntityState(EntityState.Modified);
            var modificationCommand1 = new ModificationCommand("A", null, new ParameterNameGenerator().GenerateNext, p => p.TestProvider());
            modificationCommand1.AddEntry(entry1);

            var entry2 = stateManager.GetOrCreateEntry(new object());
            entry2[keyProperty] = value2;
            entry2.SetEntityState(EntityState.Modified);
            var modificationCommand2 = new ModificationCommand("A", null, new ParameterNameGenerator().GenerateNext, p => p.TestProvider());
            modificationCommand2.AddEntry(entry2);

            var modificationCommand3 = new ModificationCommand("A", null, new ParameterNameGenerator().GenerateNext, p => p.TestProvider());
            modificationCommand3.AddEntry(entry1);

            var mCC = new ModificationCommandComparer();

            Assert.True(0 > mCC.Compare(modificationCommand1, modificationCommand2));
            Assert.True(0 < mCC.Compare(modificationCommand2, modificationCommand1));
            Assert.True(0 == mCC.Compare(modificationCommand1, modificationCommand3));
        }

        [Flags]
        private enum FlagsEnum
        {
            Default = 0,
            First = 1 << 0,
            Second = 1 << 2
        }
    }
}

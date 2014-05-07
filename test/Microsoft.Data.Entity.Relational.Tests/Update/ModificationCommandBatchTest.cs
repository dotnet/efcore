// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests.Update
{
    public class ModificationCommandBatchTest
    {
        [Fact]
        public void CompileBatch_compiles_inserts()
        {
            var stateEntry = CreateStateEntry(EntityState.Added);

            var command = new ModificationCommand("T1", new ParameterNameGenerator());
            command.AddStateEntry(stateEntry);

            var batch = new ModificationCommandBatch(new[] { command });

            Assert.Equal(
                "BatchHeader$" + Environment.NewLine +
                "INSERT INTO [T1] ([Col1], [Col2]) VALUES (@p0, @p1)$" + Environment.NewLine,
                batch.CompileBatch(new ConcreteSqlGenerator()));
        }

        [Fact]
        public void CompileBatch_compiles_updates()
        {
            var stateEntry = CreateStateEntry(EntityState.Modified, ValueGenerationStrategy.StoreIdentity);

            var command = new ModificationCommand("T1", new ParameterNameGenerator());
            command.AddStateEntry(stateEntry);

            var batch = new ModificationCommandBatch(new[] { command });

            Assert.Equal(
                "BatchHeader$" + Environment.NewLine +
                "UPDATE [T1] SET [Col2] = @p1 WHERE [Col1] = @p0$" + Environment.NewLine,
                batch.CompileBatch(new ConcreteSqlGenerator()));
        }

        [Fact]
        public void CompileBatch_compiles_deletes()
        {
            var stateEntry = CreateStateEntry(EntityState.Deleted);

            var command = new ModificationCommand("T1", new ParameterNameGenerator());
            command.AddStateEntry(stateEntry);

            var batch = new ModificationCommandBatch(new[] { command });

            Assert.Equal(
                "BatchHeader$" + Environment.NewLine +
                "DELETE FROM [T1] WHERE [Col1] = @p0$" + Environment.NewLine,
                batch.CompileBatch(new ConcreteSqlGenerator()));
        }

        [Fact]
        public void Batch_separator_not_appended_if_batch_header_empty()
        {
            var stateEntry = CreateStateEntry(EntityState.Deleted);

            var command = new ModificationCommand("T1", new ParameterNameGenerator());
            command.AddStateEntry(stateEntry);

            var batch = new ModificationCommandBatch(new[] { command });

            Assert.Equal(
                "DELETE FROM [T1] WHERE [Col1] = @p0$" + Environment.NewLine,
                batch.CompileBatch(new ConcreteSqlGenerator(useBatchHeader: false)));
        }

        private class ConcreteSqlGenerator : SqlGenerator
        {
            private readonly string _batchHeader;

            public ConcreteSqlGenerator(bool useBatchHeader = true)
            {
                _batchHeader = useBatchHeader ? "BatchHeader" : null;
            }

            protected override void AppendIdentityWhereCondition(StringBuilder commandStringBuilder, ColumnModification columnModification)
            {
                commandStringBuilder
                    .Append(QuoteIdentifier(columnModification.ColumnName))
                    .Append(" = ")
                    .Append("provider_specific_identity()");
            }

            public override void AppendBatchHeader(StringBuilder commandStringBuilder)
            {
                commandStringBuilder.Append(_batchHeader);
            }

            public override string BatchCommandSeparator
            {
                get { return "$"; }
            }
        }

        private class T1
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private static IModel BuildModel(ValueGenerationStrategy keyStrategy, ValueGenerationStrategy nonKeyStrategy)
        {
            var model = new Metadata.Model();

            var entityType = new EntityType(typeof(T1));

            var key = entityType.AddProperty("Id", typeof(int));
            key.ValueGenerationStrategy = keyStrategy;
            key.StorageName = "Col1";
            entityType.SetKey(key);

            var nonKey = entityType.AddProperty("Name", typeof(string));
            nonKey.StorageName = "Col2";
            nonKey.ValueGenerationStrategy = nonKeyStrategy;

            model.AddEntityType(entityType);

            return model;
        }

        private static DbContextConfiguration CreateConfiguration(IModel model)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddEntityFramework();
            return new DbContext(serviceCollection.BuildServiceProvider(),
                new DbContextOptions()
                    .UseModel(model)
                    .BuildConfiguration())
                .Configuration;
        }

        private static StateEntry CreateStateEntry(
            EntityState entityState,
            ValueGenerationStrategy keyStrategy = ValueGenerationStrategy.None,
            ValueGenerationStrategy nonKeyStrategy = ValueGenerationStrategy.None)
        {
            var model = BuildModel(keyStrategy, nonKeyStrategy);
            var stateEntry = CreateConfiguration(model).Services.StateEntryFactory.Create(
                model.GetEntityType("T1"), new T1 { Id = 1, Name = "Test" });
            stateEntry.EntityState = entityState;
            return stateEntry;
        }
    }
}

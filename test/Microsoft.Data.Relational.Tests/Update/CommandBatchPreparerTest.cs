// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Relational.Model;
using Microsoft.Data.Relational.Update;
using Xunit;
using Database = Microsoft.Data.Relational.Model.Database;

namespace Microsoft.Data.Relational.Tests.Update
{
    public class CommandBatchPreparerTest
    {
        [Fact]
        public async Task BatchCommands_creates_valid_batch_for_added_entities()
        {
            var database = CreateDatabase();
            var model = CreateModel();

            var stateEntry = new MixedStateEntry(
                CreateConfiguration(),
                model.GetEntityType(typeof(FakeEntity)), new FakeEntity { Id = 42, Value = "Test" });

            await stateEntry.SetEntityStateAsync(EntityState.Added);

            var commandBatches = new CommandBatchPreparer().BatchCommands(new[] { stateEntry }, database).ToArray();
            Assert.Equal(1, commandBatches.Count());
            Assert.Equal(1, commandBatches.First().BatchCommands.Count());

            var command = commandBatches.First().BatchCommands.Single();
            Assert.Equal(ModificationOperation.Insert, command.Operation);
            Assert.Equal(
                new[]
                    {
                        new KeyValuePair<string, object>("Id", 42),
                        new KeyValuePair<string, object>("Value", "Test"),
                    },
                command.ColumnValues.Select(v => new KeyValuePair<string, object>(v.Key.Name, v.Value)));
        }

        [Fact]
        public async Task BatchCommands_creates_valid_batch_for_updated_entities()
        {
            var database = CreateDatabase();
            var model = CreateModel();

            var stateEntry = new MixedStateEntry(
                CreateConfiguration(),
                model.GetEntityType(typeof(FakeEntity)), new FakeEntity { Id = 42, Value = "Test" });

            await stateEntry.SetEntityStateAsync(EntityState.Modified);

            var commandBatches = new CommandBatchPreparer().BatchCommands(new[] { stateEntry }, database).ToArray();
            Assert.Equal(1, commandBatches.Count());
            Assert.Equal(1, commandBatches.First().BatchCommands.Count());

            var command = commandBatches.First().BatchCommands.Single();
            Assert.Equal(ModificationOperation.Update, command.Operation);

            Assert.Equal(
                new[]
                    {
                        new KeyValuePair<string, object>("Value", "Test"),
                    },
                command.ColumnValues.Select(v => new KeyValuePair<string, object>(v.Key.Name, v.Value)));

            Assert.Equal(
                new[]
                    {
                        new KeyValuePair<string, object>("Id", 42),
                    },
                command.WhereClauses.Select(v => new KeyValuePair<string, object>(v.Key.Name, v.Value)));
        }

        [Fact]
        public async Task BatchCommands_creates_valid_batch_for_deleted_entities()
        {
            var database = CreateDatabase();
            var model = CreateModel();

            var stateEntry = new MixedStateEntry(
                CreateConfiguration(),
                model.GetEntityType(typeof(FakeEntity)), new FakeEntity { Id = 42, Value = "Test" });

            await stateEntry.SetEntityStateAsync(EntityState.Deleted);

            var commandBatches = new CommandBatchPreparer().BatchCommands(new[] { stateEntry }, database).ToArray();
            Assert.Equal(1, commandBatches.Count());
            Assert.Equal(1, commandBatches.First().BatchCommands.Count());

            var command = commandBatches.First().BatchCommands.Single();
            Assert.Equal(ModificationOperation.Delete, command.Operation);

            Assert.Equal(null, command.ColumnValues);

            Assert.Equal(
                new[]
                    {
                        new KeyValuePair<string, object>("Id", 42),
                    },
                command.WhereClauses.Select(v => new KeyValuePair<string, object>(v.Key.Name, v.Value)));
        }

        private static DbContextConfiguration CreateConfiguration()
        {
            return new DbContext(new DbContextOptions().BuildConfiguration()).Configuration;
        }

        private static Database CreateDatabase()
        {
            var table = new Table("FakeEntity", new[] { new Column("Id", "_"), new Column("Value", "_") });
            table.PrimaryKey = new PrimaryKey("PK", table.Columns.Where(c => c.Name == "Id").ToArray());
            var database = new Database();
            database.AddTable(table);
            return database;
        }

        private static IModel CreateModel()
        {
            var model = new Entity.Metadata.Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder
                .Entity<FakeEntity>()
                .Key(c => c.Id)
                .Properties(ps => ps.Property(c => c.Value));

            return model;
        }

        private class FakeEntity
        {
            public int Id { get; set; }
            public string Value { get; set; }
        }
    }
}

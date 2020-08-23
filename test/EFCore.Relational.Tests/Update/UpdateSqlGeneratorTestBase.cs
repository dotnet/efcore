// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Update
{
    public abstract class UpdateSqlGeneratorTestBase
    {
        [ConditionalFact]
        public virtual void AppendDeleteOperation_creates_full_delete_command_text()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateDeleteCommand(false);

            CreateSqlGenerator().AppendDeleteOperation(stringBuilder, command, 0);

            Assert.Equal(
                "DELETE FROM "
                + SchemaPrefix
                + OpenDelimiter
                + "Ducks"
                + CloseDelimiter
                + ""
                + Environment.NewLine
                + "WHERE "
                + OpenDelimiter
                + "Id"
                + CloseDelimiter
                + " = @p0;"
                + Environment.NewLine
                + "SELECT "
                + RowsAffected
                + ";"
                + Environment.NewLine
                + Environment.NewLine,
                stringBuilder.ToString());
        }

        [ConditionalFact]
        public virtual void AppendDeleteOperation_creates_full_delete_command_text_with_concurrency_check()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateDeleteCommand();

            CreateSqlGenerator().AppendDeleteOperation(stringBuilder, command, 0);

            Assert.Equal(
                "DELETE FROM "
                + SchemaPrefix
                + OpenDelimiter
                + "Ducks"
                + CloseDelimiter
                + ""
                + Environment.NewLine
                + "WHERE "
                + OpenDelimiter
                + "Id"
                + CloseDelimiter
                + " = @p0 AND "
                + OpenDelimiter
                + "ConcurrencyToken"
                + CloseDelimiter
                + " IS NULL;"
                + Environment.NewLine
                + "SELECT "
                + RowsAffected
                + ";"
                + Environment.NewLine
                + Environment.NewLine,
                stringBuilder.ToString());
        }

        [ConditionalFact]
        public virtual void AppendInsertOperation_appends_insert_and_select_and_where_if_store_generated_columns_exist()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateInsertCommand();

            CreateSqlGenerator().AppendInsertOperation(stringBuilder, command, 0);

            AppendInsertOperation_appends_insert_and_select_and_where_if_store_generated_columns_exist_verification(stringBuilder);
        }

        protected virtual void AppendInsertOperation_appends_insert_and_select_and_where_if_store_generated_columns_exist_verification(
            StringBuilder stringBuilder)
        {
            Assert.Equal(
                "INSERT INTO "
                + SchemaPrefix
                + OpenDelimiter
                + "Ducks"
                + CloseDelimiter
                + " ("
                + OpenDelimiter
                + "Name"
                + CloseDelimiter
                + ", "
                + OpenDelimiter
                + "Quacks"
                + CloseDelimiter
                + ", "
                + OpenDelimiter
                + "ConcurrencyToken"
                + CloseDelimiter
                + ")"
                + Environment.NewLine
                + "VALUES (@p0, @p1, @p2);"
                + Environment.NewLine
                + "SELECT "
                + OpenDelimiter
                + "Id"
                + CloseDelimiter
                + ", "
                + OpenDelimiter
                + "Computed"
                + CloseDelimiter
                + ""
                + Environment.NewLine
                + "FROM "
                + SchemaPrefix
                + OpenDelimiter
                + "Ducks"
                + CloseDelimiter
                + ""
                + Environment.NewLine
                + "WHERE "
                + RowsAffected
                + " = 1 AND "
                + GetIdentityWhereCondition("Id")
                + ";"
                + Environment.NewLine
                + Environment.NewLine,
                stringBuilder.ToString());
        }

        [ConditionalFact]
        public virtual void
            AppendInsertOperation_appends_insert_and_select_rowcount_if_no_store_generated_columns_exist_or_conditions_exist()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateInsertCommand(false, false);

            CreateSqlGenerator().AppendInsertOperation(stringBuilder, command, 0);

            Assert.Equal(
                "INSERT INTO "
                + SchemaPrefix
                + OpenDelimiter
                + "Ducks"
                + CloseDelimiter
                + " ("
                + OpenDelimiter
                + "Id"
                + CloseDelimiter
                + ", "
                + OpenDelimiter
                + "Name"
                + CloseDelimiter
                + ", "
                + OpenDelimiter
                + "Quacks"
                + CloseDelimiter
                + ", "
                + OpenDelimiter
                + "ConcurrencyToken"
                + CloseDelimiter
                + ")"
                + Environment.NewLine
                + "VALUES (@p0, @p1, @p2, @p3);"
                + Environment.NewLine,
                stringBuilder.ToString());
        }

        [ConditionalFact]
        public virtual void AppendInsertOperation_appends_insert_and_select_store_generated_columns_but_no_identity()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateInsertCommand(false);

            CreateSqlGenerator().AppendInsertOperation(stringBuilder, command, 0);

            AppendInsertOperation_appends_insert_and_select_store_generated_columns_but_no_identity_verification(stringBuilder);
        }

        protected virtual void AppendInsertOperation_appends_insert_and_select_store_generated_columns_but_no_identity_verification(
            StringBuilder stringBuilder)
        {
            Assert.Equal(
                "INSERT INTO "
                + SchemaPrefix
                + OpenDelimiter
                + "Ducks"
                + CloseDelimiter
                + " ("
                + OpenDelimiter
                + "Id"
                + CloseDelimiter
                + ", "
                + OpenDelimiter
                + "Name"
                + CloseDelimiter
                + ", "
                + OpenDelimiter
                + "Quacks"
                + CloseDelimiter
                + ", "
                + OpenDelimiter
                + "ConcurrencyToken"
                + CloseDelimiter
                + ")"
                + Environment.NewLine
                + "VALUES (@p0, @p1, @p2, @p3);"
                + Environment.NewLine
                + "SELECT "
                + OpenDelimiter
                + "Computed"
                + CloseDelimiter
                + ""
                + Environment.NewLine
                + "FROM "
                + SchemaPrefix
                + OpenDelimiter
                + "Ducks"
                + CloseDelimiter
                + ""
                + Environment.NewLine
                + "WHERE "
                + RowsAffected
                + " = 1 AND "
                + OpenDelimiter
                + "Id"
                + CloseDelimiter
                + " = @p0;"
                + Environment.NewLine
                + Environment.NewLine,
                stringBuilder.ToString());
        }

        [ConditionalFact]
        public virtual void AppendInsertOperation_appends_insert_and_select_for_only_identity()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateInsertCommand(true, false);

            CreateSqlGenerator().AppendInsertOperation(stringBuilder, command, 0);

            AppendInsertOperation_appends_insert_and_select_for_only_identity_verification(stringBuilder);
        }

        protected virtual void AppendInsertOperation_appends_insert_and_select_for_only_identity_verification(StringBuilder stringBuilder)
        {
            Assert.Equal(
                "INSERT INTO "
                + SchemaPrefix
                + OpenDelimiter
                + "Ducks"
                + CloseDelimiter
                + " ("
                + OpenDelimiter
                + "Name"
                + CloseDelimiter
                + ", "
                + OpenDelimiter
                + "Quacks"
                + CloseDelimiter
                + ", "
                + OpenDelimiter
                + "ConcurrencyToken"
                + CloseDelimiter
                + ")"
                + Environment.NewLine
                + "VALUES (@p0, @p1, @p2);"
                + Environment.NewLine
                + "SELECT "
                + OpenDelimiter
                + "Id"
                + CloseDelimiter
                + ""
                + Environment.NewLine
                + "FROM "
                + SchemaPrefix
                + OpenDelimiter
                + "Ducks"
                + CloseDelimiter
                + ""
                + Environment.NewLine
                + "WHERE "
                + RowsAffected
                + " = 1 AND "
                + GetIdentityWhereCondition("Id")
                + ";"
                + Environment.NewLine
                + Environment.NewLine,
                stringBuilder.ToString());
        }

        [ConditionalFact]
        public virtual void AppendInsertOperation_appends_insert_and_select_for_all_store_generated_columns()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateInsertCommand(true, true, true);

            CreateSqlGenerator().AppendInsertOperation(stringBuilder, command, 0);

            AppendInsertOperation_appends_insert_and_select_for_all_store_generated_columns_verification(stringBuilder);
        }

        protected virtual void AppendInsertOperation_appends_insert_and_select_for_all_store_generated_columns_verification(
            StringBuilder stringBuilder)
        {
            Assert.Equal(
                "INSERT INTO "
                + SchemaPrefix
                + OpenDelimiter
                + "Ducks"
                + CloseDelimiter
                + ""
                + Environment.NewLine
                + "DEFAULT VALUES;"
                + Environment.NewLine
                + "SELECT "
                + OpenDelimiter
                + "Id"
                + CloseDelimiter
                + ", "
                + OpenDelimiter
                + "Computed"
                + CloseDelimiter
                + ""
                + Environment.NewLine
                + "FROM "
                + SchemaPrefix
                + OpenDelimiter
                + "Ducks"
                + CloseDelimiter
                + ""
                + Environment.NewLine
                + "WHERE "
                + RowsAffected
                + " = 1 AND "
                + GetIdentityWhereCondition("Id")
                + ";"
                + Environment.NewLine
                + Environment.NewLine,
                stringBuilder.ToString());
        }

        [ConditionalFact]
        public virtual void AppendInsertOperation_appends_insert_and_select_for_only_single_identity_columns()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateInsertCommand(true, false, true);

            CreateSqlGenerator().AppendInsertOperation(stringBuilder, command, 0);

            AppendInsertOperation_appends_insert_and_select_for_only_single_identity_columns_verification(stringBuilder);
        }

        protected virtual void AppendInsertOperation_appends_insert_and_select_for_only_single_identity_columns_verification(
            StringBuilder stringBuilder)
        {
            Assert.Equal(
                "INSERT INTO "
                + SchemaPrefix
                + OpenDelimiter
                + "Ducks"
                + CloseDelimiter
                + ""
                + Environment.NewLine
                + "DEFAULT VALUES;"
                + Environment.NewLine
                + "SELECT "
                + OpenDelimiter
                + "Id"
                + CloseDelimiter
                + ""
                + Environment.NewLine
                + "FROM "
                + SchemaPrefix
                + OpenDelimiter
                + "Ducks"
                + CloseDelimiter
                + ""
                + Environment.NewLine
                + "WHERE "
                + RowsAffected
                + " = 1 AND "
                + GetIdentityWhereCondition("Id")
                + ";"
                + Environment.NewLine
                + Environment.NewLine,
                stringBuilder.ToString());
        }

        [ConditionalFact]
        public virtual void AppendUpdateOperation_appends_update_and_select_if_store_generated_columns_exist()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateUpdateCommand();

            CreateSqlGenerator().AppendUpdateOperation(stringBuilder, command, 0);

            AppendUpdateOperation_appends_update_and_select_if_store_generated_columns_exist_verification(stringBuilder);
        }

        protected virtual void AppendUpdateOperation_appends_update_and_select_if_store_generated_columns_exist_verification(
            StringBuilder stringBuilder)
        {
            Assert.Equal(
                "UPDATE "
                + SchemaPrefix
                + OpenDelimiter
                + "Ducks"
                + CloseDelimiter
                + " SET "
                + OpenDelimiter
                + "Name"
                + CloseDelimiter
                + " = @p0, "
                + OpenDelimiter
                + "Quacks"
                + CloseDelimiter
                + " = @p1, "
                + OpenDelimiter
                + "ConcurrencyToken"
                + CloseDelimiter
                + " = @p2"
                + Environment.NewLine
                + "WHERE "
                + OpenDelimiter
                + "Id"
                + CloseDelimiter
                + " = @p3 AND "
                + OpenDelimiter
                + "ConcurrencyToken"
                + CloseDelimiter
                + " IS NULL;"
                + Environment.NewLine
                + "SELECT "
                + OpenDelimiter
                + "Computed"
                + CloseDelimiter
                + ""
                + Environment.NewLine
                + "FROM "
                + SchemaPrefix
                + OpenDelimiter
                + "Ducks"
                + CloseDelimiter
                + ""
                + Environment.NewLine
                + "WHERE "
                + RowsAffected
                + " = 1 AND "
                + OpenDelimiter
                + "Id"
                + CloseDelimiter
                + " = @p3;"
                + Environment.NewLine
                + Environment.NewLine,
                stringBuilder.ToString());
        }

        [ConditionalFact]
        public virtual void AppendUpdateOperation_appends_update_and_select_rowcount_if_store_generated_columns_dont_exist()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateUpdateCommand(false, false);

            CreateSqlGenerator().AppendUpdateOperation(stringBuilder, command, 0);

            Assert.Equal(
                "UPDATE "
                + SchemaPrefix
                + OpenDelimiter
                + "Ducks"
                + CloseDelimiter
                + " SET "
                + OpenDelimiter
                + "Name"
                + CloseDelimiter
                + " = @p0, "
                + OpenDelimiter
                + "Quacks"
                + CloseDelimiter
                + " = @p1, "
                + OpenDelimiter
                + "ConcurrencyToken"
                + CloseDelimiter
                + " = @p2"
                + Environment.NewLine
                + "WHERE "
                + OpenDelimiter
                + "Id"
                + CloseDelimiter
                + " = @p3;"
                + Environment.NewLine
                + "SELECT "
                + RowsAffected
                + ";"
                + Environment.NewLine
                + Environment.NewLine,
                stringBuilder.ToString());
        }

        [ConditionalFact]
        public virtual void AppendUpdateOperation_appends_where_for_concurrency_token()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateUpdateCommand(false);

            CreateSqlGenerator().AppendUpdateOperation(stringBuilder, command, 0);

            Assert.Equal(
                "UPDATE "
                + SchemaPrefix
                + OpenDelimiter
                + "Ducks"
                + CloseDelimiter
                + " SET "
                + OpenDelimiter
                + "Name"
                + CloseDelimiter
                + " = @p0, "
                + OpenDelimiter
                + "Quacks"
                + CloseDelimiter
                + " = @p1, "
                + OpenDelimiter
                + "ConcurrencyToken"
                + CloseDelimiter
                + " = @p2"
                + Environment.NewLine
                + "WHERE "
                + OpenDelimiter
                + "Id"
                + CloseDelimiter
                + " = @p3 AND "
                + OpenDelimiter
                + "ConcurrencyToken"
                + CloseDelimiter
                + " IS NULL;"
                + Environment.NewLine
                + "SELECT "
                + RowsAffected
                + ";"
                + Environment.NewLine
                + Environment.NewLine,
                stringBuilder.ToString());
        }

        [ConditionalFact]
        public virtual void AppendUpdateOperation_appends_select_for_computed_property()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateUpdateCommand(true, false);

            CreateSqlGenerator().AppendUpdateOperation(stringBuilder, command, 0);

            AppendUpdateOperation_appends_select_for_computed_property_verification(stringBuilder);
        }

        protected virtual void AppendUpdateOperation_appends_select_for_computed_property_verification(StringBuilder stringBuilder)
        {
            Assert.Equal(
                "UPDATE "
                + SchemaPrefix
                + OpenDelimiter
                + "Ducks"
                + CloseDelimiter
                + " SET "
                + OpenDelimiter
                + "Name"
                + CloseDelimiter
                + " = @p0, "
                + OpenDelimiter
                + "Quacks"
                + CloseDelimiter
                + " = @p1, "
                + OpenDelimiter
                + "ConcurrencyToken"
                + CloseDelimiter
                + " = @p2"
                + Environment.NewLine
                + "WHERE "
                + OpenDelimiter
                + "Id"
                + CloseDelimiter
                + " = @p3;"
                + Environment.NewLine
                + "SELECT "
                + OpenDelimiter
                + "Computed"
                + CloseDelimiter
                + ""
                + Environment.NewLine
                + "FROM "
                + SchemaPrefix
                + OpenDelimiter
                + "Ducks"
                + CloseDelimiter
                + ""
                + Environment.NewLine
                + "WHERE "
                + RowsAffected
                + " = 1 AND "
                + OpenDelimiter
                + "Id"
                + CloseDelimiter
                + " = @p3;"
                + Environment.NewLine
                + Environment.NewLine,
                stringBuilder.ToString());
        }

        [ConditionalFact]
        public virtual void GenerateNextSequenceValueOperation_returns_statement_with_sanitized_sequence()
        {
            var statement = CreateSqlGenerator().GenerateNextSequenceValueOperation("sequence" + CloseDelimiter + "; --", null);

            Assert.Equal(
                "SELECT NEXT VALUE FOR " + OpenDelimiter + "sequence" + CloseDelimiter + CloseDelimiter + "; --" + CloseDelimiter,
                statement);
        }

        [ConditionalFact]
        public virtual void GenerateNextSequenceValueOperation_correctly_handles_schemas()
        {
            var statement = CreateSqlGenerator().GenerateNextSequenceValueOperation("mysequence", "dbo");

            Assert.Equal(
                "SELECT NEXT VALUE FOR " + SchemaPrefix + OpenDelimiter + "mysequence" + CloseDelimiter,
                statement);
        }

        protected abstract IUpdateSqlGenerator CreateSqlGenerator();

        protected abstract string RowsAffected { get; }

        protected virtual string Identity
            => throw new NotImplementedException();

        protected virtual string OpenDelimiter
            => "\"";

        protected virtual string CloseDelimiter
            => "\"";

        protected virtual string Schema
            => "dbo";

        protected virtual string SchemaPrefix
            => string.IsNullOrEmpty(Schema) ? string.Empty : OpenDelimiter + Schema + CloseDelimiter + ".";

        protected virtual string GetIdentityWhereCondition(string columnName)
            => OpenDelimiter + columnName + CloseDelimiter + " = " + Identity;

        protected ModificationCommand CreateInsertCommand(bool identityKey = true, bool isComputed = true, bool defaultsOnly = false)
        {
            var duckType = GetDuckType();
            var stateManager = TestHelpers.CreateContextServices(duckType.Model.FinalizeModel()).GetRequiredService<IStateManager>();
            var entry = stateManager.GetOrCreateEntry(new Duck());
            var generator = new ParameterNameGenerator();

            var idProperty = duckType.FindProperty(nameof(Duck.Id));
            var nameProperty = duckType.FindProperty(nameof(Duck.Name));
            var quacksProperty = duckType.FindProperty(nameof(Duck.Quacks));
            var computedProperty = duckType.FindProperty(nameof(Duck.Computed));
            var concurrencyProperty = duckType.FindProperty(nameof(Duck.ConcurrencyToken));
            var columnModifications = new[]
            {
                new ColumnModification(
                    entry, idProperty, idProperty.GetTableColumnMappings().Single().Column, generator.GenerateNext,
                    idProperty.GetTableColumnMappings().Single().TypeMapping, identityKey, !identityKey, true, false, true),
                new ColumnModification(
                    entry, nameProperty, nameProperty.GetTableColumnMappings().Single().Column, generator.GenerateNext,
                    nameProperty.GetTableColumnMappings().Single().TypeMapping, false, true, false, false, true),
                new ColumnModification(
                    entry, quacksProperty, quacksProperty.GetTableColumnMappings().Single().Column, generator.GenerateNext,
                    quacksProperty.GetTableColumnMappings().Single().TypeMapping, false, true, false, false, true),
                new ColumnModification(
                    entry, computedProperty, computedProperty.GetTableColumnMappings().Single().Column, generator.GenerateNext,
                    computedProperty.GetTableColumnMappings().Single().TypeMapping, isComputed, false, false, false, true),
                new ColumnModification(
                    entry, concurrencyProperty, concurrencyProperty.GetTableColumnMappings().Single().Column, generator.GenerateNext,
                    concurrencyProperty.GetTableColumnMappings().Single().TypeMapping, false, true, false, false, true)
            };

            if (defaultsOnly)
            {
                columnModifications = columnModifications.Where(c => !c.IsWrite).ToArray();
            }

            return new FakeModificationCommand(
                "Ducks", Schema, new ParameterNameGenerator().GenerateNext, false, columnModifications);
        }

        protected ModificationCommand CreateUpdateCommand(bool isComputed = true, bool concurrencyToken = true)
        {
            var duckType = GetDuckType();
            var stateManager = TestHelpers.CreateContextServices(duckType.Model.FinalizeModel()).GetRequiredService<IStateManager>();
            var entry = stateManager.GetOrCreateEntry(new Duck());
            var generator = new ParameterNameGenerator();

            var idProperty = duckType.FindProperty(nameof(Duck.Id));
            var nameProperty = duckType.FindProperty(nameof(Duck.Name));
            var quacksProperty = duckType.FindProperty(nameof(Duck.Quacks));
            var computedProperty = duckType.FindProperty(nameof(Duck.Computed));
            var concurrencyProperty = duckType.FindProperty(nameof(Duck.ConcurrencyToken));
            var columnModifications = new[]
            {
                new ColumnModification(
                    entry, idProperty, idProperty.GetTableColumnMappings().Single().Column, generator.GenerateNext,
                    idProperty.GetTableColumnMappings().Single().TypeMapping, false, false, true, true, true),
                new ColumnModification(
                    entry, nameProperty, nameProperty.GetTableColumnMappings().Single().Column, generator.GenerateNext,
                    nameProperty.GetTableColumnMappings().Single().TypeMapping, false, true, false, false, true),
                new ColumnModification(
                    entry, quacksProperty, quacksProperty.GetTableColumnMappings().Single().Column, generator.GenerateNext,
                    quacksProperty.GetTableColumnMappings().Single().TypeMapping, false, true, false, false, true),
                new ColumnModification(
                    entry, computedProperty, computedProperty.GetTableColumnMappings().Single().Column, generator.GenerateNext,
                    computedProperty.GetTableColumnMappings().Single().TypeMapping, isComputed, false, false, false, true),
                new ColumnModification(
                    entry, concurrencyProperty, concurrencyProperty.GetTableColumnMappings().Single().Column, generator.GenerateNext,
                    concurrencyProperty.GetTableColumnMappings().Single().TypeMapping, false, true, false, concurrencyToken, true)
            };

            return new FakeModificationCommand(
                "Ducks", Schema, new ParameterNameGenerator().GenerateNext, false, columnModifications);
        }

        protected ModificationCommand CreateDeleteCommand(bool concurrencyToken = true)
        {
            var duckType = GetDuckType();
            var stateManager = TestHelpers.CreateContextServices(duckType.Model.FinalizeModel()).GetRequiredService<IStateManager>();
            var entry = stateManager.GetOrCreateEntry(new Duck());
            var generator = new ParameterNameGenerator();

            var idProperty = duckType.FindProperty(nameof(Duck.Id));
            var concurrencyProperty = duckType.FindProperty(nameof(Duck.ConcurrencyToken));
            var columnModifications = new[]
            {
                new ColumnModification(
                    entry, idProperty, idProperty.GetTableColumnMappings().Single().Column, generator.GenerateNext,
                    idProperty.GetTableColumnMappings().Single().TypeMapping, false, false, true, true, true),
                new ColumnModification(
                    entry, concurrencyProperty, concurrencyProperty.GetTableColumnMappings().Single().Column, generator.GenerateNext,
                    concurrencyProperty.GetTableColumnMappings().Single().TypeMapping, false, false, false, concurrencyToken, true)
            };

            return new FakeModificationCommand(
                "Ducks", Schema, new ParameterNameGenerator().GenerateNext, false, columnModifications);
        }

        protected abstract TestHelpers TestHelpers { get; }

        private IMutableEntityType GetDuckType()
        {
            var modelBuilder = TestHelpers.CreateConventionBuilder();
            modelBuilder.Entity<Duck>().ToTable("Ducks", Schema).Property(e => e.Id).ValueGeneratedNever();
            return modelBuilder.Model.FindEntityType(typeof(Duck));
        }

        protected class Duck
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int Quacks { get; set; }
            public Guid Computed { get; set; }
            public byte[] ConcurrencyToken { get; set; }
        }
    }
}

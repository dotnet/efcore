// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Update
{
    public abstract class UpdateSqlGeneratorTestBase
    {
        [Fact]
        public virtual void AppendDeleteOperation_creates_full_delete_command_text()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateDeleteCommand(false);

            CreateSqlGenerator().AppendDeleteOperation(stringBuilder, command, 0);

            Assert.Equal(
                "DELETE FROM " + SchemaPrefix + OpenDelimeter + "Ducks" + CloseDelimeter + "" + Environment.NewLine +
                "WHERE " + OpenDelimeter + "Id" + CloseDelimeter + " = @p0;" + Environment.NewLine +
                "SELECT " + RowsAffected + ";" + Environment.NewLine + Environment.NewLine,
                stringBuilder.ToString());
        }

        [Fact]
        public virtual void AppendDeleteOperation_creates_full_delete_command_text_with_concurrency_check()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateDeleteCommand(concurrencyToken: true);

            CreateSqlGenerator().AppendDeleteOperation(stringBuilder, command, 0);

            Assert.Equal(
                "DELETE FROM " + SchemaPrefix + OpenDelimeter + "Ducks" + CloseDelimeter + "" + Environment.NewLine +
                "WHERE " + OpenDelimeter + "Id" + CloseDelimeter + " = @p0 AND " + OpenDelimeter + "ConcurrencyToken" + CloseDelimeter + " IS NULL;" + Environment.NewLine +
                "SELECT " + RowsAffected + ";" + Environment.NewLine + Environment.NewLine,
                stringBuilder.ToString());
        }

        [Fact]
        public virtual void AppendInsertOperation_appends_insert_and_select_and_where_if_store_generated_columns_exist()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateInsertCommand(identityKey: true, isComputed: true);

            CreateSqlGenerator().AppendInsertOperation(stringBuilder, command, 0);

            AppendInsertOperation_appends_insert_and_select_and_where_if_store_generated_columns_exist_verification(stringBuilder);
        }

        protected virtual void AppendInsertOperation_appends_insert_and_select_and_where_if_store_generated_columns_exist_verification(StringBuilder stringBuilder)
        {
            Assert.Equal(
                "INSERT INTO " + SchemaPrefix + OpenDelimeter + "Ducks" + CloseDelimeter + " (" + OpenDelimeter + "Name" + CloseDelimeter + ", " + OpenDelimeter + "Quacks" + CloseDelimeter + ", " + OpenDelimeter + "ConcurrencyToken" + CloseDelimeter + ")" + Environment.NewLine +
                "VALUES (@p0, @p1, @p2);" + Environment.NewLine +
                "SELECT " + OpenDelimeter + "Id" + CloseDelimeter + ", " + OpenDelimeter + "Computed" + CloseDelimeter + "" + Environment.NewLine +
                "FROM " + SchemaPrefix + OpenDelimeter + "Ducks" + CloseDelimeter + "" + Environment.NewLine +
                "WHERE " + RowsAffected + " = 1 AND " + OpenDelimeter + "Id" + CloseDelimeter + " = " + Identity + ";" + Environment.NewLine + Environment.NewLine,
                stringBuilder.ToString());
        }

        [Fact]
        public virtual void AppendInsertOperation_appends_insert_and_select_rowcount_if_no_store_generated_columns_exist_or_conditions_exist()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateInsertCommand(false, false);

            CreateSqlGenerator().AppendInsertOperation(stringBuilder, command, 0);

            Assert.Equal(
                "INSERT INTO " + SchemaPrefix + OpenDelimeter + "Ducks" + CloseDelimeter + " (" +
                OpenDelimeter + "Id" + CloseDelimeter + ", " + OpenDelimeter + "Name" + CloseDelimeter + ", " + OpenDelimeter + "Quacks"
                + CloseDelimeter + ", " + OpenDelimeter + "ConcurrencyToken" + CloseDelimeter + ")" + Environment.NewLine +
                "VALUES (@p0, @p1, @p2, @p3);" + Environment.NewLine,
                stringBuilder.ToString());
        }

        [Fact]
        public virtual void AppendInsertOperation_appends_insert_and_select_store_generated_columns_but_no_identity()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateInsertCommand(false, isComputed: true);

            CreateSqlGenerator().AppendInsertOperation(stringBuilder, command, 0);

            AppendInsertOperation_appends_insert_and_select_store_generated_columns_but_no_identity_verification(stringBuilder);
        }

        protected virtual void AppendInsertOperation_appends_insert_and_select_store_generated_columns_but_no_identity_verification(StringBuilder stringBuilder)
        {
            Assert.Equal(
                "INSERT INTO " + SchemaPrefix + OpenDelimeter + "Ducks" + CloseDelimeter + " (" + OpenDelimeter + "Id" +
                CloseDelimeter + ", " + OpenDelimeter + "Name" + CloseDelimeter + ", " + OpenDelimeter + "Quacks" + CloseDelimeter + ", " + OpenDelimeter +
                "ConcurrencyToken" + CloseDelimeter + ")" + Environment.NewLine +
                "VALUES (@p0, @p1, @p2, @p3);" + Environment.NewLine +
                "SELECT " + OpenDelimeter + "Computed" + CloseDelimeter + "" + Environment.NewLine +
                "FROM " + SchemaPrefix + OpenDelimeter + "Ducks" + CloseDelimeter + "" + Environment.NewLine +
                "WHERE " + RowsAffected + " = 1 AND " + OpenDelimeter + "Id" + CloseDelimeter + " = @p0;" + Environment.NewLine + Environment.NewLine,
                stringBuilder.ToString());
        }

        [Fact]
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
                "INSERT INTO " + SchemaPrefix + OpenDelimeter + "Ducks" + CloseDelimeter + " (" + OpenDelimeter + "Name" +
                CloseDelimeter + ", " + OpenDelimeter + "Quacks" + CloseDelimeter + ", " + OpenDelimeter + "ConcurrencyToken" + CloseDelimeter + ")" + Environment.NewLine +
                "VALUES (@p0, @p1, @p2);" + Environment.NewLine +
                "SELECT " + OpenDelimeter + "Id" + CloseDelimeter + "" + Environment.NewLine +
                "FROM " + SchemaPrefix + OpenDelimeter + "Ducks" + CloseDelimeter + "" + Environment.NewLine +
                "WHERE " + RowsAffected + " = 1 AND " + OpenDelimeter + "Id" + CloseDelimeter + " = " + Identity + ";" + Environment.NewLine + Environment.NewLine,
                stringBuilder.ToString());
        }

        [Fact]
        public virtual void AppendInsertOperation_appends_insert_and_select_for_all_store_generated_columns()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateInsertCommand(true, true, true);

            CreateSqlGenerator().AppendInsertOperation(stringBuilder, command, 0);

            AppendInsertOperation_appends_insert_and_select_for_all_store_generated_columns_verification(stringBuilder);
        }

        protected virtual void AppendInsertOperation_appends_insert_and_select_for_all_store_generated_columns_verification(StringBuilder stringBuilder)
        {
            Assert.Equal(
                "INSERT INTO " + SchemaPrefix + OpenDelimeter + "Ducks" + CloseDelimeter + "" + Environment.NewLine +
                "DEFAULT VALUES;" + Environment.NewLine +
                "SELECT " + OpenDelimeter + "Id" + CloseDelimeter + ", " + OpenDelimeter + "Computed" + CloseDelimeter + "" + Environment.NewLine +
                "FROM " + SchemaPrefix + OpenDelimeter + "Ducks" + CloseDelimeter + "" + Environment.NewLine +
                "WHERE " + RowsAffected + " = 1 AND " + OpenDelimeter + "Id" + CloseDelimeter + " = " + Identity + ";" + Environment.NewLine + Environment.NewLine,
                stringBuilder.ToString());
        }

        [Fact]
        public virtual void AppendInsertOperation_appends_insert_and_select_for_only_single_identity_columns()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateInsertCommand(true, false, true);

            CreateSqlGenerator().AppendInsertOperation(stringBuilder, command, 0);

            AppendInsertOperation_appends_insert_and_select_for_only_single_identity_columns_verification(stringBuilder);
        }

        protected virtual void AppendInsertOperation_appends_insert_and_select_for_only_single_identity_columns_verification(StringBuilder stringBuilder)
        {
            Assert.Equal(
                "INSERT INTO " + SchemaPrefix + OpenDelimeter + "Ducks" + CloseDelimeter + "" + Environment.NewLine +
                "DEFAULT VALUES;" + Environment.NewLine +
                "SELECT " + OpenDelimeter + "Id" + CloseDelimeter + "" + Environment.NewLine +
                "FROM " + SchemaPrefix + OpenDelimeter + "Ducks" + CloseDelimeter + "" + Environment.NewLine +
                "WHERE " + RowsAffected + " = 1 AND " + OpenDelimeter + "Id" + CloseDelimeter + " = " + Identity + ";" + Environment.NewLine + Environment.NewLine,
                stringBuilder.ToString());
        }

        [Fact]
        public virtual void AppendUpdateOperation_appends_update_and_select_if_store_generated_columns_exist()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateUpdateCommand(isComputed: true, concurrencyToken: true);

            CreateSqlGenerator().AppendUpdateOperation(stringBuilder, command, 0);

            AppendUpdateOperation_appends_update_and_select_if_store_generated_columns_exist_verification(stringBuilder);
        }

        protected virtual void AppendUpdateOperation_appends_update_and_select_if_store_generated_columns_exist_verification(StringBuilder stringBuilder)
        {
            Assert.Equal(
                "UPDATE " + SchemaPrefix + OpenDelimeter + "Ducks" + CloseDelimeter + " SET " + OpenDelimeter + "Name" + CloseDelimeter +
                " = @p0, " + OpenDelimeter + "Quacks" + CloseDelimeter + " = @p1, " + OpenDelimeter + "ConcurrencyToken" + CloseDelimeter + " = @p2" + Environment.NewLine +
                "WHERE " + OpenDelimeter + "Id" + CloseDelimeter + " = @p3 AND " + OpenDelimeter + "ConcurrencyToken" + CloseDelimeter + " IS NULL;" + Environment.NewLine +
                "SELECT " + OpenDelimeter + "Computed" + CloseDelimeter + "" + Environment.NewLine +
                "FROM " + SchemaPrefix + OpenDelimeter + "Ducks" + CloseDelimeter + "" + Environment.NewLine +
                "WHERE " + RowsAffected + " = 1 AND " + OpenDelimeter + "Id" + CloseDelimeter + " = @p3;" + Environment.NewLine + Environment.NewLine,
                stringBuilder.ToString());
        }

        [Fact]
        public virtual void AppendUpdateOperation_appends_update_and_select_rowcount_if_store_generated_columns_dont_exist()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateUpdateCommand(false, false);

            CreateSqlGenerator().AppendUpdateOperation(stringBuilder, command, 0);

            Assert.Equal(
                "UPDATE " + SchemaPrefix + OpenDelimeter + "Ducks" + CloseDelimeter + " SET " +
                OpenDelimeter + "Name" + CloseDelimeter + " = @p0, " + OpenDelimeter + "Quacks" + CloseDelimeter + " = @p1, " +
                OpenDelimeter + "ConcurrencyToken" + CloseDelimeter + " = @p2" + Environment.NewLine +
                "WHERE " + OpenDelimeter + "Id" + CloseDelimeter + " = @p3;" + Environment.NewLine +
                "SELECT " + RowsAffected + ";" + Environment.NewLine + Environment.NewLine,
                stringBuilder.ToString());
        }

        [Fact]
        public virtual void AppendUpdateOperation_appends_where_for_concurrency_token()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateUpdateCommand(false, concurrencyToken: true);

            CreateSqlGenerator().AppendUpdateOperation(stringBuilder, command, 0);

            Assert.Equal(
                "UPDATE " + SchemaPrefix + OpenDelimeter + "Ducks" + CloseDelimeter + " SET " +
                OpenDelimeter + "Name" + CloseDelimeter + " = @p0, " + OpenDelimeter + "Quacks" + CloseDelimeter + " = @p1, " +
                OpenDelimeter + "ConcurrencyToken" + CloseDelimeter + " = @p2" + Environment.NewLine +
                "WHERE " + OpenDelimeter + "Id" + CloseDelimeter + " = @p3 AND " + OpenDelimeter + "ConcurrencyToken" + CloseDelimeter + " IS NULL;" + Environment.NewLine +
                "SELECT " + RowsAffected + ";" + Environment.NewLine + Environment.NewLine,
                stringBuilder.ToString());
        }

        [Fact]
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
                "UPDATE " + SchemaPrefix + OpenDelimeter + "Ducks" + CloseDelimeter + " SET " +
                OpenDelimeter + "Name" + CloseDelimeter + " = @p0, " + OpenDelimeter + "Quacks" + CloseDelimeter + " = @p1, " +
                OpenDelimeter + "ConcurrencyToken" + CloseDelimeter + " = @p2" + Environment.NewLine +
                "WHERE " + OpenDelimeter + "Id" + CloseDelimeter + " = @p3;" + Environment.NewLine +
                "SELECT " + OpenDelimeter + "Computed" + CloseDelimeter + "" + Environment.NewLine +
                "FROM " + SchemaPrefix + OpenDelimeter + "Ducks" + CloseDelimeter + "" + Environment.NewLine +
                "WHERE " + RowsAffected + " = 1 AND " + OpenDelimeter + "Id" + CloseDelimeter + " = @p3;" + Environment.NewLine + Environment.NewLine,
                stringBuilder.ToString());
        }

        [Fact]
        public virtual void GenerateNextSequenceValueOperation_returns_statement_with_sanatized_sequence()
        {
            var statement = CreateSqlGenerator().GenerateNextSequenceValueOperation("sequence" + CloseDelimeter + "; --", null);

            Assert.Equal(
                "SELECT NEXT VALUE FOR " + OpenDelimeter + "sequence" + CloseDelimeter + CloseDelimeter + "; --" + CloseDelimeter,
                statement);
        }

        [Fact]
        public virtual void GenerateNextSequenceValueOperation_correctly_handles_schemas()
        {
            var statement = CreateSqlGenerator().GenerateNextSequenceValueOperation("mysequence", "dbo");

            Assert.Equal(
                "SELECT NEXT VALUE FOR " + SchemaPrefix + OpenDelimeter + "mysequence" + CloseDelimeter,
                statement);
        }

        protected abstract IUpdateSqlGenerator CreateSqlGenerator();

        protected abstract string RowsAffected { get; }

        protected abstract string Identity { get; }

        protected virtual string OpenDelimeter => "\"";

        protected virtual string CloseDelimeter => "\"";

        protected virtual string Schema => "dbo";

        protected virtual string SchemaPrefix =>
            string.IsNullOrEmpty(Schema) ? string.Empty : OpenDelimeter + Schema + CloseDelimeter + ".";

        protected ModificationCommand CreateInsertCommand(bool identityKey = true, bool isComputed = true, bool defaultsOnly = false)
        {
            var duckType = GetDuckType();
            var stateManager = TestHelpers.CreateContextServices(duckType.Model).GetRequiredService<IStateManager>();
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
                    entry, idProperty, idProperty.TestProvider(), generator.GenerateNext, identityKey, !identityKey, true, false, false),
                new ColumnModification(
                    entry, nameProperty, nameProperty.TestProvider(), generator.GenerateNext, false, true, false, false, false),
                new ColumnModification(
                    entry, quacksProperty, quacksProperty.TestProvider(), generator.GenerateNext, false, true, false, false, false),
                new ColumnModification(
                    entry, computedProperty, computedProperty.TestProvider(), generator.GenerateNext, isComputed, false, false, false, true),
                new ColumnModification(
                    entry, concurrencyProperty, concurrencyProperty.TestProvider(), generator.GenerateNext, false, true, false, false, false)
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
            var stateManager = TestHelpers.CreateContextServices(duckType.Model).GetRequiredService<IStateManager>();
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
                    entry, idProperty, idProperty.TestProvider(), generator.GenerateNext, false, false, true, true, false),
                new ColumnModification(
                    entry, nameProperty, nameProperty.TestProvider(), generator.GenerateNext, false, true, false, false, false),
                new ColumnModification(
                    entry, quacksProperty, quacksProperty.TestProvider(), generator.GenerateNext, false, true, false, false, false),
                new ColumnModification(
                    entry, computedProperty, computedProperty.TestProvider(), generator.GenerateNext, isComputed, false, false, false, false),
                new ColumnModification(
                    entry, concurrencyProperty, concurrencyProperty.TestProvider(), generator.GenerateNext, false, true, false, concurrencyToken, concurrencyToken)
            };

            return new FakeModificationCommand(
                "Ducks", Schema, new ParameterNameGenerator().GenerateNext, false, columnModifications);
        }

        protected ModificationCommand CreateDeleteCommand(bool concurrencyToken = true)
        {
            var duckType = GetDuckType();
            var stateManager = TestHelpers.CreateContextServices(duckType.Model).GetRequiredService<IStateManager>();
            var entry = stateManager.GetOrCreateEntry(new Duck());
            var generator = new ParameterNameGenerator();

            var idProperty = duckType.FindProperty(nameof(Duck.Id));
            var concurrencyProperty = duckType.FindProperty(nameof(Duck.ConcurrencyToken));
            var columnModifications = new[]
            {
                new ColumnModification(
                    entry, idProperty, idProperty.TestProvider(), generator.GenerateNext, false, false, true, true, concurrencyToken),
                new ColumnModification(
                    entry, concurrencyProperty, concurrencyProperty.TestProvider(), generator.GenerateNext, false, false, false, concurrencyToken, concurrencyToken)
            };

            return new FakeModificationCommand(
                "Ducks", Schema, new ParameterNameGenerator().GenerateNext, false, columnModifications);
        }

        protected abstract TestHelpers TestHelpers { get; }

        private EntityType GetDuckType()
        {
            var entityType = new Model().AddEntityType(typeof(Duck));
            var id = entityType.AddProperty(typeof(Duck).GetTypeInfo().GetDeclaredProperty(nameof(Duck.Id)));
            entityType.AddProperty(typeof(Duck).GetTypeInfo().GetDeclaredProperty(nameof(Duck.Name)));
            entityType.AddProperty(typeof(Duck).GetTypeInfo().GetDeclaredProperty(nameof(Duck.Quacks)));
            entityType.AddProperty(typeof(Duck).GetTypeInfo().GetDeclaredProperty(nameof(Duck.Computed)));
            entityType.AddProperty(typeof(Duck).GetTypeInfo().GetDeclaredProperty(nameof(Duck.ConcurrencyToken)));
            entityType.SetPrimaryKey(id);
            return entityType;
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

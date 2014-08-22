// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Text;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Update;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests
{
    public abstract class SqlGeneratorTestBase
    {
        [Fact]
        public void AppendDeleteOperation_creates_full_delete_command_text()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateDeleteCommand(false);

            CreateSqlGenerator().AppendDeleteOperation(stringBuilder, command);

            Assert.Equal(
                "DELETE FROM " + OpenDelimeter + "dbo" + CloseDelimeter + "." + OpenDelimeter + "Ducks" + CloseDelimeter + "" + Environment.NewLine +
                "WHERE " + OpenDelimeter + "Id" + CloseDelimeter + " = @p0;" + Environment.NewLine +
                "SELECT " + RowsAffected + ";" + Environment.NewLine,
                stringBuilder.ToString());
        }

        [Fact]
        public void AppendDeleteOperation_creates_full_delete_command_text_with_concurrency_check()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateDeleteCommand(concurrencyToken: true);

            CreateSqlGenerator().AppendDeleteOperation(stringBuilder, command);

            Assert.Equal(
                "DELETE FROM " + OpenDelimeter + "dbo" + CloseDelimeter + "." + OpenDelimeter + "Ducks" + CloseDelimeter + "" + Environment.NewLine +
                "WHERE " + OpenDelimeter + "Id" + CloseDelimeter + " = @p0 AND " + OpenDelimeter + "ConcurrencyToken" + CloseDelimeter + " = @p1;" + Environment.NewLine +
                "SELECT " + RowsAffected + ";" + Environment.NewLine,
                stringBuilder.ToString());
        }

        [Fact]
        public void AppendInsertOperation_appends_insert_and_select_and_where_if_store_generated_columns_exist()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateInsertCommand(identityKey: true, computedProperty: true);

            CreateSqlGenerator().AppendInsertOperation(stringBuilder, command);

            AppendInsertOperation_appends_insert_and_select_and_where_if_store_generated_columns_exist_verification(stringBuilder);
        }

        protected virtual void AppendInsertOperation_appends_insert_and_select_and_where_if_store_generated_columns_exist_verification(StringBuilder stringBuilder)
        {
            Assert.Equal(
                "INSERT INTO " + OpenDelimeter + "dbo" + CloseDelimeter + "." + OpenDelimeter + "Ducks" + CloseDelimeter + " (" + OpenDelimeter + "Name" + CloseDelimeter + ", " + OpenDelimeter + "Quacks" + CloseDelimeter + ", " + OpenDelimeter + "ConcurrencyToken" + CloseDelimeter + ")" + Environment.NewLine +
                "VALUES (@p0, @p1, @p2);" + Environment.NewLine +
                "SELECT " + OpenDelimeter + "Id" + CloseDelimeter + ", " + OpenDelimeter + "Computed" + CloseDelimeter + "" + Environment.NewLine +
                "FROM " + OpenDelimeter + "dbo" + CloseDelimeter + "." + OpenDelimeter + "Ducks" + CloseDelimeter + "" + Environment.NewLine +
                "WHERE " + RowsAffected + " = 1 AND " + OpenDelimeter + "Id" + CloseDelimeter + " = " + Identity + ";" + Environment.NewLine,
                stringBuilder.ToString());
        }

        [Fact]
        public void AppendInsertOperation_appends_insert_and_select_rowcount_if_no_store_generated_columns_exist_or_conditions_exist()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateInsertCommand(false, false);

            CreateSqlGenerator().AppendInsertOperation(stringBuilder, command);

            Assert.Equal(
                "INSERT INTO " + OpenDelimeter + "dbo" + CloseDelimeter + "." + OpenDelimeter + "Ducks" + CloseDelimeter + " (" +
                OpenDelimeter + "Id" + CloseDelimeter + ", " + OpenDelimeter + "Name" + CloseDelimeter + ", " + OpenDelimeter + "Quacks"
                + CloseDelimeter + ", " + OpenDelimeter + "Concurrency" + "Token" + CloseDelimeter + ")" + Environment.NewLine +
                "VALUES (@p0, @p1, @p2, @p3);" + Environment.NewLine +
                "SELECT " + RowsAffected + ";" + Environment.NewLine,
                stringBuilder.ToString());
        }

        [Fact]
        public void AppendInsertOperation_appends_insert_and_select_store_generated_columns_but_no_identity()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateInsertCommand(false, computedProperty: true);

            CreateSqlGenerator().AppendInsertOperation(stringBuilder, command);

            AppendInsertOperation_appends_insert_and_select_store_generated_columns_but_no_identity_verification(stringBuilder);
        }

        protected virtual void AppendInsertOperation_appends_insert_and_select_store_generated_columns_but_no_identity_verification(StringBuilder stringBuilder)
        {
            Assert.Equal(
                "INSERT INTO " + OpenDelimeter + "dbo" + CloseDelimeter + "." + OpenDelimeter + "Ducks" + CloseDelimeter + " (" + OpenDelimeter + "Id" +
                CloseDelimeter + ", " + OpenDelimeter + "Name" + CloseDelimeter + ", " + OpenDelimeter + "Quacks" + CloseDelimeter + ", " + OpenDelimeter +
                "ConcurrencyToken" + CloseDelimeter + ")" + Environment.NewLine +
                "VALUES (@p0, @p1, @p2, @p3);" + Environment.NewLine +
                "SELECT " + OpenDelimeter + "Computed" + CloseDelimeter + "" + Environment.NewLine +
                "FROM " + OpenDelimeter + "dbo" + CloseDelimeter + "." + OpenDelimeter + "Ducks" + CloseDelimeter + "" + Environment.NewLine +
                "WHERE " + RowsAffected + " = 1 AND " + OpenDelimeter + "Id" + CloseDelimeter + " = @p0;" + Environment.NewLine,
                stringBuilder.ToString());
        }

        [Fact]
        public void AppendInsertOperation_appends_insert_and_select_for_only_identity()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateInsertCommand(true, false);

            CreateSqlGenerator().AppendInsertOperation(stringBuilder, command);

            AppendInsertOperation_appends_insert_and_select_for_only_identity_verification(stringBuilder);
        }

        protected virtual void AppendInsertOperation_appends_insert_and_select_for_only_identity_verification(StringBuilder stringBuilder)
        {
            Assert.Equal(
                "INSERT INTO " + OpenDelimeter + "dbo" + CloseDelimeter + "." + OpenDelimeter + "Ducks" + CloseDelimeter + " (" + OpenDelimeter + "Name" +
                CloseDelimeter + ", " + OpenDelimeter + "Quacks" + CloseDelimeter + ", " + OpenDelimeter + "ConcurrencyToken" + CloseDelimeter + ")" + Environment.NewLine +
                "VALUES (@p0, @p1, @p2);" + Environment.NewLine +
                "SELECT " + OpenDelimeter + "Id" + CloseDelimeter + "" + Environment.NewLine +
                "FROM " + OpenDelimeter + "dbo" + CloseDelimeter + "." + OpenDelimeter + "Ducks" + CloseDelimeter + "" + Environment.NewLine +
                "WHERE " + RowsAffected + " = 1 AND " + OpenDelimeter + "Id" + CloseDelimeter + " = " + Identity + ";" + Environment.NewLine,
                stringBuilder.ToString());
        }

        [Fact]
        public void AppendInsertOperation_appends_insert_and_select_for_all_store_generated_columns()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateInsertCommand(true, true, true);

            CreateSqlGenerator().AppendInsertOperation(stringBuilder, command);

            AppendInsertOperation_appends_insert_and_select_for_all_store_generated_columns_verification(stringBuilder);
        }

        protected virtual void AppendInsertOperation_appends_insert_and_select_for_all_store_generated_columns_verification(StringBuilder stringBuilder)
        {
            Assert.Equal(
                "INSERT INTO " + OpenDelimeter + "dbo" + CloseDelimeter + "." + OpenDelimeter + "Ducks" + CloseDelimeter + "" + Environment.NewLine +
                "DEFAULT VALUES;" + Environment.NewLine +
                "SELECT " + OpenDelimeter + "Id" + CloseDelimeter + ", " + OpenDelimeter + "Computed" + CloseDelimeter + "" + Environment.NewLine +
                "FROM " + OpenDelimeter + "dbo" + CloseDelimeter + "." + OpenDelimeter + "Ducks" + CloseDelimeter + "" + Environment.NewLine +
                "WHERE " + RowsAffected + " = 1 AND " + OpenDelimeter + "Id" + CloseDelimeter + " = " + Identity + ";" + Environment.NewLine,
                stringBuilder.ToString());
        }

        [Fact]
        public void AppendInsertOperation_appends_insert_and_select_for_only_single_identity_columns()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateInsertCommand(true, false, true);

            CreateSqlGenerator().AppendInsertOperation(stringBuilder, command);

            AppendInsertOperation_appends_insert_and_select_for_only_single_identity_columns_verification(stringBuilder);
        }

        protected virtual void AppendInsertOperation_appends_insert_and_select_for_only_single_identity_columns_verification(StringBuilder stringBuilder)
        {
            Assert.Equal(
                "INSERT INTO " + OpenDelimeter + "dbo" + CloseDelimeter + "." + OpenDelimeter + "Ducks" + CloseDelimeter + "" + Environment.NewLine +
                "DEFAULT VALUES;" + Environment.NewLine +
                "SELECT " + OpenDelimeter + "Id" + CloseDelimeter + "" + Environment.NewLine +
                "FROM " + OpenDelimeter + "dbo" + CloseDelimeter + "." + OpenDelimeter + "Ducks" + CloseDelimeter + "" + Environment.NewLine +
                "WHERE " + RowsAffected + " = 1 AND " + OpenDelimeter + "Id" + CloseDelimeter + " = " + Identity + ";" + Environment.NewLine,
                stringBuilder.ToString());
        }

        [Fact]
        public void AppendUpdateOperation_appends_update_and_select_if_store_generated_columns_exist()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateUpdateCommand(computedProperty: true, concurrencyToken: true);

            CreateSqlGenerator().AppendUpdateOperation(stringBuilder, command);

            AppendUpdateOperation_appends_update_and_select_if_store_generated_columns_exist_verification(stringBuilder);
        }

        protected virtual void AppendUpdateOperation_appends_update_and_select_if_store_generated_columns_exist_verification(StringBuilder stringBuilder)
        {
            Assert.Equal(
                "UPDATE " + OpenDelimeter + "dbo" + CloseDelimeter + "." + OpenDelimeter + "Ducks" + CloseDelimeter + " SET " + OpenDelimeter + "Name" + CloseDelimeter +
                " = @p0, " + OpenDelimeter + "Quacks" + CloseDelimeter + " = @p1, " + OpenDelimeter + "ConcurrencyToken" + CloseDelimeter + " = @p2" + Environment.NewLine + 
                "WHERE " + OpenDelimeter + "Id" + CloseDelimeter + " = @p3 AND " + OpenDelimeter + "ConcurrencyToken" + CloseDelimeter + " = @p4;" + Environment.NewLine +
                "SELECT " + OpenDelimeter + "Computed" + CloseDelimeter + "" + Environment.NewLine +
                "FROM " + OpenDelimeter + "dbo" + CloseDelimeter + "." + OpenDelimeter + "Ducks" + CloseDelimeter + "" + Environment.NewLine +
                "WHERE " + RowsAffected + " = 1 AND " + OpenDelimeter + "Id" + CloseDelimeter + " = @p3;" + Environment.NewLine,
                stringBuilder.ToString());
        }

        [Fact]
        public void AppendUpdateOperation_appends_update_and_select_rowcount_if_store_generated_columns_dont_exist()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateUpdateCommand(false, false);

            CreateSqlGenerator().AppendUpdateOperation(stringBuilder, command);

            Assert.Equal(
                "UPDATE " + OpenDelimeter + "dbo" + CloseDelimeter + "." + OpenDelimeter + "Ducks" + CloseDelimeter + " SET " +
                OpenDelimeter + "Name" + CloseDelimeter + " = @p0, " + OpenDelimeter + "Quacks" + CloseDelimeter + " = @p1, " +
                OpenDelimeter + "ConcurrencyToken" + CloseDelimeter + " = @p2" + Environment.NewLine +
                "WHERE " + OpenDelimeter + "Id" + CloseDelimeter + " = @p3;" + Environment.NewLine +
                "SELECT " + RowsAffected + ";" + Environment.NewLine,
                stringBuilder.ToString());
        }

        [Fact]
        public void AppendUpdateOperation_appends_where_for_concurrency_token()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateUpdateCommand(false, concurrencyToken: true);

            CreateSqlGenerator().AppendUpdateOperation(stringBuilder, command);

            Assert.Equal(
                "UPDATE " + OpenDelimeter + "dbo" + CloseDelimeter + "." + OpenDelimeter + "Ducks" + CloseDelimeter + " SET " +
                OpenDelimeter + "Name" + CloseDelimeter + " = @p0, " + OpenDelimeter + "Quacks" + CloseDelimeter + " = @p1, " +
                OpenDelimeter + "ConcurrencyToken" + CloseDelimeter + " = @p2" + Environment.NewLine +
                "WHERE " + OpenDelimeter + "Id" + CloseDelimeter + " = @p3 AND " + OpenDelimeter + "ConcurrencyToken" + CloseDelimeter + " = @p4;" + Environment.NewLine +
                "SELECT " + RowsAffected + ";" + Environment.NewLine,
                stringBuilder.ToString());
        }

        [Fact]
        public void AppendUpdateOperation_appends_select_for_computed_property()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateUpdateCommand(true, false);

            CreateSqlGenerator().AppendUpdateOperation(stringBuilder, command);

            AppendUpdateOperation_appends_select_for_computed_property_verification(stringBuilder);
        }

        protected virtual void AppendUpdateOperation_appends_select_for_computed_property_verification(StringBuilder stringBuilder)
        {
            Assert.Equal(
                "UPDATE " + OpenDelimeter + "dbo" + CloseDelimeter + "." + OpenDelimeter + "Ducks" + CloseDelimeter + " SET " +
                OpenDelimeter + "Name" + CloseDelimeter + " = @p0, " + OpenDelimeter + "Quacks" + CloseDelimeter + " = @p1, " +
                OpenDelimeter + "ConcurrencyToken" + CloseDelimeter + " = @p2" + Environment.NewLine +
                "WHERE " + OpenDelimeter + "Id" + CloseDelimeter + " = @p3;" + Environment.NewLine +
                "SELECT " + OpenDelimeter + "Computed" + CloseDelimeter + "" + Environment.NewLine +
                "FROM " + OpenDelimeter + "dbo" + CloseDelimeter + "." + OpenDelimeter + "Ducks" + CloseDelimeter + "" + Environment.NewLine +
                "WHERE " + RowsAffected + " = 1 AND " + OpenDelimeter + "Id" + CloseDelimeter + " = @p3;" + Environment.NewLine,
                stringBuilder.ToString());
        }

        [Fact]
        public void Default_BatchCommandSeparator_is_semicolon()
        {
            Assert.Equal(";", CreateSqlGenerator().BatchCommandSeparator);
        }

        protected abstract SqlGenerator CreateSqlGenerator();

        protected abstract string RowsAffected { get; }

        protected abstract string Identity { get; }

        protected IProperty CreateMockProperty(string name)
        {
            var propertyMock = new Mock<IProperty>();
            propertyMock.Setup(m => m.Name).Returns(name);
            return propertyMock.Object;
        }

        protected virtual string OpenDelimeter
        {
            get { return "\""; }
        }

        protected virtual string CloseDelimeter
        {
            get { return "\""; }
        }

        protected ModificationCommand CreateInsertCommand(bool identityKey = true, bool computedProperty = true, bool defaultsOnly = false)
        {
            var entry = new Mock<StateEntry>().Object;
            var generator = new ParameterNameGenerator();

            var columnModifications = new[]
                {
                    new ColumnModification(
                        entry, CreateMockProperty("Id"), generator, identityKey, !identityKey, true, false),
                    new ColumnModification(
                        entry, CreateMockProperty("Name"), generator, false, true, false, false),
                    new ColumnModification(
                        entry, CreateMockProperty("Quacks"), generator, false, true, false, false),
                    new ColumnModification(
                        entry, CreateMockProperty("Computed"), generator, computedProperty, false, false, false),
                    new ColumnModification(
                        entry, CreateMockProperty("ConcurrencyToken"), generator, false, true, false, false)
                };

            if (defaultsOnly)
            {
                columnModifications = columnModifications.Where(c => !c.IsWrite).ToArray();
            }

            var commandMock = new Mock<ModificationCommand>(new SchemaQualifiedName("Ducks", "dbo"), new ParameterNameGenerator()) { CallBase = true };
            commandMock.Setup(m => m.ColumnModifications).Returns(columnModifications);

            return commandMock.Object;
        }

        protected ModificationCommand CreateUpdateCommand(bool computedProperty = true, bool concurrencyToken = true)
        {
            var entry = new Mock<StateEntry>().Object;
            var generator = new ParameterNameGenerator();

            var columnModifications = new[]
                {
                    new ColumnModification(
                        entry, CreateMockProperty("Id"), generator, false, false, true, true),
                    new ColumnModification(
                        entry, CreateMockProperty("Name"), generator, false, true, false, false),
                    new ColumnModification(
                        entry, CreateMockProperty("Quacks"), generator, false, true, false, false),
                    new ColumnModification(
                        entry, CreateMockProperty("Computed"), generator, computedProperty, false, false, false),
                    new ColumnModification(
                        entry, CreateMockProperty("ConcurrencyToken"), generator, false, true, false, concurrencyToken)
                };

            var commandMock = new Mock<ModificationCommand>(new SchemaQualifiedName("Ducks", "dbo"), new ParameterNameGenerator()) { CallBase = true };
            commandMock.Setup(m => m.ColumnModifications).Returns(columnModifications);

            return commandMock.Object;
        }

        protected ModificationCommand CreateDeleteCommand(bool concurrencyToken = true)
        {
            var entry = new Mock<StateEntry>().Object;
            var generator = new ParameterNameGenerator();

            var columnModifications = new[]
                {
                    new ColumnModification(
                        entry, CreateMockProperty("Id"), generator, false, false, true, true),
                    new ColumnModification(
                        entry, CreateMockProperty("ConcurrencyToken"), generator, false, false, false, concurrencyToken)
                };

            var commandMock = new Mock<ModificationCommand>(new SchemaQualifiedName("Ducks", "dbo"), new ParameterNameGenerator()) { CallBase = true };
            commandMock.Setup(m => m.ColumnModifications).Returns(columnModifications);

            return commandMock.Object;
        }
    }
}

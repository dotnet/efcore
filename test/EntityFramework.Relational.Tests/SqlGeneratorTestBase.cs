// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Text;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Metadata;
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
                "DELETE FROM " + OpenDelimeter + SchemaName + CloseDelimeter + "." + OpenDelimeter + "Ducks" + CloseDelimeter + "" + Environment.NewLine +
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
                "DELETE FROM " + OpenDelimeter + SchemaName + CloseDelimeter + "." + OpenDelimeter + "Ducks" + CloseDelimeter + "" + Environment.NewLine +
                "WHERE " + OpenDelimeter + "Id" + CloseDelimeter + " = @p0 AND " + OpenDelimeter + "ConcurrencyToken" + CloseDelimeter + " = @p1;" + Environment.NewLine +
                "SELECT " + RowsAffected + ";" + Environment.NewLine,
                stringBuilder.ToString());
        }

        [Fact]
        public void AppendInsertOperation_appends_insert_and_select_and_where_if_store_generated_columns_exist()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateInsertCommand(identityKey: true, isComputed: true);

            CreateSqlGenerator().AppendInsertOperation(stringBuilder, command);

            AppendInsertOperation_appends_insert_and_select_and_where_if_store_generated_columns_exist_verification(stringBuilder);
        }

        protected virtual void AppendInsertOperation_appends_insert_and_select_and_where_if_store_generated_columns_exist_verification(StringBuilder stringBuilder)
        {
            Assert.Equal(
                "INSERT INTO " + OpenDelimeter + SchemaName + CloseDelimeter + "." + OpenDelimeter + "Ducks" + CloseDelimeter + " (" + OpenDelimeter + "Name" + CloseDelimeter + ", " + OpenDelimeter + "Quacks" + CloseDelimeter + ", " + OpenDelimeter + "ConcurrencyToken" + CloseDelimeter + ")" + Environment.NewLine +
                "VALUES (@p0, @p1, @p2);" + Environment.NewLine +
                "SELECT " + OpenDelimeter + "Id" + CloseDelimeter + ", " + OpenDelimeter + "Computed" + CloseDelimeter + "" + Environment.NewLine +
                "FROM " + OpenDelimeter + SchemaName + CloseDelimeter + "." + OpenDelimeter + "Ducks" + CloseDelimeter + "" + Environment.NewLine +
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
                "INSERT INTO " + OpenDelimeter + SchemaName + CloseDelimeter + "." + OpenDelimeter + "Ducks" + CloseDelimeter + " (" +
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
            var command = CreateInsertCommand(false, isComputed: true);

            CreateSqlGenerator().AppendInsertOperation(stringBuilder, command);

            AppendInsertOperation_appends_insert_and_select_store_generated_columns_but_no_identity_verification(stringBuilder);
        }

        protected virtual void AppendInsertOperation_appends_insert_and_select_store_generated_columns_but_no_identity_verification(StringBuilder stringBuilder)
        {
            Assert.Equal(
                "INSERT INTO " + OpenDelimeter + SchemaName + CloseDelimeter + "." + OpenDelimeter + "Ducks" + CloseDelimeter + " (" + OpenDelimeter + "Id" +
                CloseDelimeter + ", " + OpenDelimeter + "Name" + CloseDelimeter + ", " + OpenDelimeter + "Quacks" + CloseDelimeter + ", " + OpenDelimeter +
                "ConcurrencyToken" + CloseDelimeter + ")" + Environment.NewLine +
                "VALUES (@p0, @p1, @p2, @p3);" + Environment.NewLine +
                "SELECT " + OpenDelimeter + "Computed" + CloseDelimeter + "" + Environment.NewLine +
                "FROM " + OpenDelimeter + SchemaName + CloseDelimeter + "." + OpenDelimeter + "Ducks" + CloseDelimeter + "" + Environment.NewLine +
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
                "INSERT INTO " + OpenDelimeter + SchemaName + CloseDelimeter + "." + OpenDelimeter + "Ducks" + CloseDelimeter + " (" + OpenDelimeter + "Name" +
                CloseDelimeter + ", " + OpenDelimeter + "Quacks" + CloseDelimeter + ", " + OpenDelimeter + "ConcurrencyToken" + CloseDelimeter + ")" + Environment.NewLine +
                "VALUES (@p0, @p1, @p2);" + Environment.NewLine +
                "SELECT " + OpenDelimeter + "Id" + CloseDelimeter + "" + Environment.NewLine +
                "FROM " + OpenDelimeter + SchemaName + CloseDelimeter + "." + OpenDelimeter + "Ducks" + CloseDelimeter + "" + Environment.NewLine +
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
                "INSERT INTO " + OpenDelimeter + SchemaName + CloseDelimeter + "." + OpenDelimeter + "Ducks" + CloseDelimeter + "" + Environment.NewLine +
                "DEFAULT VALUES;" + Environment.NewLine +
                "SELECT " + OpenDelimeter + "Id" + CloseDelimeter + ", " + OpenDelimeter + "Computed" + CloseDelimeter + "" + Environment.NewLine +
                "FROM " + OpenDelimeter + SchemaName + CloseDelimeter + "." + OpenDelimeter + "Ducks" + CloseDelimeter + "" + Environment.NewLine +
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
                "INSERT INTO " + OpenDelimeter + SchemaName + CloseDelimeter + "." + OpenDelimeter + "Ducks" + CloseDelimeter + "" + Environment.NewLine +
                "DEFAULT VALUES;" + Environment.NewLine +
                "SELECT " + OpenDelimeter + "Id" + CloseDelimeter + "" + Environment.NewLine +
                "FROM " + OpenDelimeter + SchemaName + CloseDelimeter + "." + OpenDelimeter + "Ducks" + CloseDelimeter + "" + Environment.NewLine +
                "WHERE " + RowsAffected + " = 1 AND " + OpenDelimeter + "Id" + CloseDelimeter + " = " + Identity + ";" + Environment.NewLine,
                stringBuilder.ToString());
        }

        [Fact]
        public void AppendUpdateOperation_appends_update_and_select_if_store_generated_columns_exist()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateUpdateCommand(isComputed: true, concurrencyToken: true);

            CreateSqlGenerator().AppendUpdateOperation(stringBuilder, command);

            AppendUpdateOperation_appends_update_and_select_if_store_generated_columns_exist_verification(stringBuilder);
        }

        protected virtual void AppendUpdateOperation_appends_update_and_select_if_store_generated_columns_exist_verification(StringBuilder stringBuilder)
        {
            Assert.Equal(
                "UPDATE " + OpenDelimeter + SchemaName + CloseDelimeter + "." + OpenDelimeter + "Ducks" + CloseDelimeter + " SET " + OpenDelimeter + "Name" + CloseDelimeter +
                " = @p0, " + OpenDelimeter + "Quacks" + CloseDelimeter + " = @p1, " + OpenDelimeter + "ConcurrencyToken" + CloseDelimeter + " = @p2" + Environment.NewLine +
                "WHERE " + OpenDelimeter + "Id" + CloseDelimeter + " = @p3 AND " + OpenDelimeter + "ConcurrencyToken" + CloseDelimeter + " = @p4;" + Environment.NewLine +
                "SELECT " + OpenDelimeter + "Computed" + CloseDelimeter + "" + Environment.NewLine +
                "FROM " + OpenDelimeter + SchemaName + CloseDelimeter + "." + OpenDelimeter + "Ducks" + CloseDelimeter + "" + Environment.NewLine +
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
                "UPDATE " + OpenDelimeter + SchemaName + CloseDelimeter + "." + OpenDelimeter + "Ducks" + CloseDelimeter + " SET " +
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
                "UPDATE " + OpenDelimeter + SchemaName + CloseDelimeter + "." + OpenDelimeter + "Ducks" + CloseDelimeter + " SET " +
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
                "UPDATE " + OpenDelimeter + SchemaName + CloseDelimeter + "." + OpenDelimeter + "Ducks" + CloseDelimeter + " SET " +
                OpenDelimeter + "Name" + CloseDelimeter + " = @p0, " + OpenDelimeter + "Quacks" + CloseDelimeter + " = @p1, " +
                OpenDelimeter + "ConcurrencyToken" + CloseDelimeter + " = @p2" + Environment.NewLine +
                "WHERE " + OpenDelimeter + "Id" + CloseDelimeter + " = @p3;" + Environment.NewLine +
                "SELECT " + OpenDelimeter + "Computed" + CloseDelimeter + "" + Environment.NewLine +
                "FROM " + OpenDelimeter + SchemaName + CloseDelimeter + "." + OpenDelimeter + "Ducks" + CloseDelimeter + "" + Environment.NewLine +
                "WHERE " + RowsAffected + " = 1 AND " + OpenDelimeter + "Id" + CloseDelimeter + " = @p3;" + Environment.NewLine,
                stringBuilder.ToString());
        }

        [Fact]
        public void Default_BatchCommandSeparator_is_semicolon()
        {
            Assert.Equal(";", CreateSqlGenerator().BatchCommandSeparator);
        }

        [Fact]
        public virtual void BatchSeparator_returns_seperator()
        {
            Assert.Equal(string.Empty, CreateSqlGenerator().BatchSeparator);
        }

        [Fact]
        public virtual void GenerateLiteral_returns_ByteArray_literal()
        {
            var literal = CreateSqlGenerator().GenerateLiteral(new byte[] { 0xDA, 0x7A });
            Assert.Equal("X'DA7A'", literal);
        }

        [Fact]
        public virtual void GenerateLiteral_returns_bool_literal_when_true()
        {
            var literal = CreateSqlGenerator().GenerateLiteral(true);
            Assert.Equal("TRUE", literal);
        }

        [Fact]
        public virtual void GenerateLiteral_returns_bool_literal_when_false()
        {
            var literal = CreateSqlGenerator().GenerateLiteral(false);
            Assert.Equal("FALSE", literal);
        }

        [Fact]
        public virtual void GenerateLiteral_returns_char_literal()
        {
            var literal = CreateSqlGenerator().GenerateLiteral('A');
            Assert.Equal("'A'", literal);
        }

        [Fact]
        public virtual void GenerateLiteral_returns_DateTime_literal()
        {
            var value = new DateTime(2015, 3, 12, 13, 36, 37, 371);
            var literal = CreateSqlGenerator().GenerateLiteral(value);
            Assert.Equal("TIMESTAMP '2015-03-12 13:36:37.3710000'", literal);
        }

        [Fact]
        public virtual void GenerateLiteral_returns_DateTimeOffset_literal()
        {
            var value = new DateTimeOffset(2015, 3, 12, 13, 36, 37, 371, new TimeSpan(-7, 0, 0));
            var literal = CreateSqlGenerator().GenerateLiteral(value);
            Assert.Equal("TIMESTAMP '2015-03-12 13:36:37.3710000-07:00'", literal);
        }

        [Fact]
        public virtual void GenerateLiteral_returns_NullableInt_literal_when_null()
        {
            var literal = CreateSqlGenerator().GenerateLiteral(default(int?));
            Assert.Equal("NULL", literal);
        }

        [Fact]
        public virtual void GenerateLiteral_returns_NullableInt_literal_when_not_null()
        {
            var literal = CreateSqlGenerator().GenerateLiteral((char?)'A');
            Assert.Equal("'A'", literal);
        }

        [Fact]
        public virtual void GenerateLiteral_returns_object_literal_when_null()
        {
            var literal = CreateSqlGenerator().GenerateLiteral(default(object));
            Assert.Equal("NULL", literal);
        }

        [Fact]
        public virtual void GenerateLiteral_returns_object_literal_when_not_null()
        {
            var literal = CreateSqlGenerator().GenerateLiteral((object)42);
            Assert.Equal("42", literal);
        }

        [Fact]
        public virtual void GenerateNextSequenceValueOperation_returns_statement_with_sanatized_sequence()
        {
            var statement = CreateSqlGenerator().GenerateNextSequenceValueOperation("sequence" + CloseDelimeter + "; --");

            Assert.Equal(
                "SELECT NEXT VALUE FOR " + OpenDelimeter + "sequence" + CloseDelimeter + CloseDelimeter + "; --" + CloseDelimeter,
                statement);
        }

        protected abstract ISqlGenerator CreateSqlGenerator();

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

        protected virtual string SchemaName
        {
            get { return "dbo"; }
        }

        protected ModificationCommand CreateInsertCommand(bool identityKey = true, bool isComputed = true, bool defaultsOnly = false)
        {
            var entry = CreateInternalEntryMock().Object;
            var generator = new ParameterNameGenerator();

            var idProperty = CreateMockProperty("Id");
            var nameProperty = CreateMockProperty("Name");
            var quacksProperty = CreateMockProperty("Quacks");
            var computedProperty = CreateMockProperty("Computed");
            var concurrencyProperty = CreateMockProperty("ConcurrencyToken");
            var columnModifications = new[]
                {
                    new ColumnModification(
                        entry, idProperty, idProperty.Relational(), generator, new GenericBoxedValueReader<int>() , identityKey, !identityKey, true, false),
                    new ColumnModification(
                        entry, nameProperty, nameProperty.Relational(), generator, null, false, true, false, false),
                    new ColumnModification(
                        entry, quacksProperty, quacksProperty.Relational(), generator, null, false, true, false, false),
                    new ColumnModification(
                        entry, computedProperty, computedProperty.Relational(), generator, new GenericBoxedValueReader<int>(), isComputed, false, false, false),
                    new ColumnModification(
                        entry, concurrencyProperty, concurrencyProperty.Relational(), generator, null, false, true, false, false)
                };

            if (defaultsOnly)
            {
                columnModifications = columnModifications.Where(c => !c.IsWrite).ToArray();
            }

            Func<IProperty, IRelationalPropertyExtensions> func = p => p.Relational();
            var commandMock = new Mock<ModificationCommand>("Ducks", SchemaName, new ParameterNameGenerator(), func, new BoxedValueReaderSource(), Mock.Of<IRelationalValueReaderFactoryFactory>()) { CallBase = true };
            commandMock.Setup(m => m.ColumnModifications).Returns(columnModifications);

            return commandMock.Object;
        }

        protected ModificationCommand CreateUpdateCommand(bool isComputed = true, bool concurrencyToken = true)
        {
            var entry = CreateInternalEntryMock().Object;
            var generator = new ParameterNameGenerator();

            var idProperty = CreateMockProperty("Id");
            var nameProperty = CreateMockProperty("Name");
            var quacksProperty = CreateMockProperty("Quacks");
            var computedProperty = CreateMockProperty("Computed");
            var concurrencyProperty = CreateMockProperty("ConcurrencyToken");
            var columnModifications = new[]
                {
                    new ColumnModification(
                        entry, idProperty, idProperty.Relational(), generator, null, false, false, true, true),
                    new ColumnModification(
                        entry, nameProperty, nameProperty.Relational(), generator, null, false, true, false, false),
                    new ColumnModification(
                        entry, quacksProperty, quacksProperty.Relational(), generator, null, false, true, false, false),
                    new ColumnModification(
                        entry, computedProperty, computedProperty.Relational(), generator, new GenericBoxedValueReader<int>(), isComputed, false, false, false),
                    new ColumnModification(
                        entry, concurrencyProperty, concurrencyProperty.Relational(), generator, null, false, true, false, concurrencyToken)
                };

            Func<IProperty, IRelationalPropertyExtensions> func = p => p.Relational();
            var commandMock = new Mock<ModificationCommand>("Ducks", SchemaName, new ParameterNameGenerator(), func, new BoxedValueReaderSource(), Mock.Of<IRelationalValueReaderFactoryFactory>()) { CallBase = true };
            commandMock.Setup(m => m.ColumnModifications).Returns(columnModifications);

            return commandMock.Object;
        }

        protected ModificationCommand CreateDeleteCommand(bool concurrencyToken = true)
        {
            var entry = CreateInternalEntryMock().Object;
            var generator = new ParameterNameGenerator();

            var idProperty = CreateMockProperty("Id");
            var concurrencyProperty = CreateMockProperty("ConcurrencyToken");
            var columnModifications = new[]
                {
                    new ColumnModification(
                        entry, idProperty, idProperty.Relational(), generator, null, false, false, true, true),
                    new ColumnModification(
                        entry, concurrencyProperty, concurrencyProperty.Relational(), generator, null, false, false, false, concurrencyToken)
                };

            Func<IProperty, IRelationalPropertyExtensions> func = p => p.Relational();
            var commandMock = new Mock<ModificationCommand>("Ducks", SchemaName, new ParameterNameGenerator(), func, new BoxedValueReaderSource(), Mock.Of<IRelationalValueReaderFactoryFactory>()) { CallBase = true };
            commandMock.Setup(m => m.ColumnModifications).Returns(columnModifications);

            return commandMock.Object;
        }

        private static Mock<InternalEntityEntry> CreateInternalEntryMock()
        {
            var entityTypeMock = new Mock<IEntityType>();
            entityTypeMock.Setup(e => e.GetProperties()).Returns(new IProperty[0]);

            var internalEntryMock = new Mock<InternalEntityEntry>(
                Mock.Of<IStateManager>(), entityTypeMock.Object, Mock.Of<IEntityEntryMetadataServices>());
            return internalEntryMock;
        }
    }
}

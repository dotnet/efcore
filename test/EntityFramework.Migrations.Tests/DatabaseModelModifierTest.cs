// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Relational.Model;
using Xunit;
using ForeignKey = Microsoft.Data.Entity.Relational.Model.ForeignKey;
using Index = Microsoft.Data.Entity.Relational.Model.Index;

namespace Microsoft.Data.Entity.Migrations.Tests
{
    public class DatabaseModelModifierTest
    {
        [Fact]
        public void Visit_with_create_sequence_operation()
        {
            var model = new DatabaseModel();
            var sequence = new Sequence("dbo.MySequence", "bigint", 2, 3);
            var operation = new CreateSequenceOperation(sequence);

            Assert.Equal(0, model.Sequences.Count);

            operation.Accept(new DatabaseModelModifier(), model);

            Assert.Equal(1, model.Sequences.Count);

            Assert.NotSame(sequence, model.Sequences[0]);
            Assert.Equal("dbo.MySequence", model.Sequences[0].Name);
            Assert.Equal("bigint", model.Sequences[0].DataType);
            Assert.Equal(2, model.Sequences[0].StartWith);
            Assert.Equal(3, model.Sequences[0].IncrementBy);
        }

        [Fact]
        public void Visit_with_drop_sequence_operation()
        {
            var model = new DatabaseModel();
            var operation = new DropSequenceOperation("dbo.MySequence");

            model.AddSequence(new Sequence("dbo.MySequence", "bigint", 0, 1));

            Assert.Equal(1, model.Sequences.Count);

            operation.Accept(new DatabaseModelModifier(), model);

            Assert.Equal(0, model.Sequences.Count);
        }

        [Fact]
        public void Visit_with_move_sequence_operation()
        {
            var model = new DatabaseModel();
            var operation = new MoveSequenceOperation("dbo.MySequence", "RenamedSchema");

            model.AddSequence(new Sequence("dbo.MySequence", "bigint", 0, 1));
            operation.Accept(new DatabaseModelModifier(), model);

            Assert.Equal(1, model.Sequences.Count);
            Assert.Equal("RenamedSchema.MySequence", model.Sequences[0].Name);
        }

        [Fact]
        public void Visit_with_rename_sequence_operation()
        {
            var model = new DatabaseModel();
            var operation = new RenameSequenceOperation("dbo.MySequence", "RenamedSequence");

            model.AddSequence(new Sequence("dbo.MySequence", "bigint", 0, 1));
            operation.Accept(new DatabaseModelModifier(), model);

            Assert.Equal(1, model.Sequences.Count);
            Assert.Equal("dbo.RenamedSequence", model.Sequences[0].Name);
        }

        [Fact]
        public void Visit_with_alter_sequence_operation()
        {
            var model = new DatabaseModel();
            var operation = new AlterSequenceOperation("dbo.MySequence", 7);

            model.AddSequence(new Sequence("dbo.MySequence", "bigint", 0, 6));
            operation.Accept(new DatabaseModelModifier(), model);

            Assert.Equal(1, model.Sequences.Count);
            Assert.Equal("dbo.MySequence", model.Sequences[0].Name);
            Assert.Equal("bigint", model.Sequences[0].DataType);
            Assert.Equal(0, model.Sequences[0].StartWith);
            Assert.Equal(7, model.Sequences[0].IncrementBy);
        }

        [Fact]
        public void Visit_with_create_table_operation()
        {
            var model = new DatabaseModel();
            var column0 = new Column("Id", typeof(string));
            var column1 = new Column("Id", typeof(string));
            var column2 = new Column("C", typeof(int));
            var dependent = new Table("T1", new[] { column1, column2 });

            dependent.PrimaryKey = new PrimaryKey("PK", new[] { column1 }, isClustered: false);
            dependent.AddForeignKey(new ForeignKey("FK", new[] { column1 }, new[] { column0 }));
            dependent.AddIndex(new Index("IX", new[] { column2 }));

            var operation = new CreateTableOperation(dependent);

            Assert.Equal(0, model.Tables.Count);

            operation.Accept(new DatabaseModelModifier(), model);

            Assert.Equal(1, model.Tables.Count);
            Assert.NotSame(dependent, model.Tables[0]);
            Assert.Equal("T1", model.Tables[0].Name);
            Assert.Equal(new[] { "Id", "C" }, model.Tables[0].Columns.Select(c => c.Name));
            Assert.Equal(typeof(string), model.Tables[0].Columns[0].ClrType);
            Assert.Equal(typeof(int), model.Tables[0].Columns[1].ClrType);

            Assert.NotSame(dependent.PrimaryKey, model.Tables[0].PrimaryKey);
            Assert.Equal("PK", model.Tables[0].PrimaryKey.Name);
            Assert.Equal(new[] { "Id" }, model.Tables[0].PrimaryKey.Columns.Select(c => c.Name));
            Assert.False(model.Tables[0].PrimaryKey.IsClustered);

            Assert.Equal(0, model.Tables[0].ForeignKeys.Count);
            Assert.Equal(0, model.Tables[0].Indexes.Count);
        }

        [Fact]
        public void Visit_with_drop_table_operation()
        {
            var model = new DatabaseModel();
            var operation = new DropTableOperation("dbo.MyTable");

            model.AddTable(new Table("dbo.MyTable"));

            Assert.Equal(1, model.Tables.Count);

            operation.Accept(new DatabaseModelModifier(), model);

            Assert.Equal(0, model.Tables.Count);
        }

        [Fact]
        public void Visit_with_rename_table_operation()
        {
            var model = new DatabaseModel();
            var operation = new RenameTableOperation("dbo.MyTable", "RenamedTable");

            model.AddTable(new Table("dbo.MyTable"));

            operation.Accept(new DatabaseModelModifier(), model);

            Assert.Equal(1, model.Tables.Count);
            Assert.Equal("dbo.RenamedTable", model.Tables[0].Name);
        }

        [Fact]
        public void Visit_with_move_table_operation()
        {
            var model = new DatabaseModel();
            var operation = new MoveTableOperation("dbo.MyTable", "RenamedSchema");

            model.AddTable(new Table("dbo.MyTable"));

            operation.Accept(new DatabaseModelModifier(), model);

            Assert.Equal(1, model.Tables.Count);
            Assert.Equal("RenamedSchema.MyTable", model.Tables[0].Name);
        }

        [Fact]
        public void Visit_with_add_column_operation()
        {
            var model = new DatabaseModel();
            var table = new Table("dbo.MyTable");
            var column = new Column("Foo", typeof(int));
            var operation = new AddColumnOperation("dbo.MyTable", column);

            model.AddTable(table);

            Assert.Equal(0, table.Columns.Count);

            operation.Accept(new DatabaseModelModifier(), model);

            Assert.Equal(1, table.Columns.Count);
            Assert.NotSame(column, table.Columns[0]);
            Assert.Equal("Foo", table.Columns[0].Name);
            Assert.Equal(typeof(int), table.Columns[0].ClrType);
        }

        [Fact]
        public void Visit_with_drop_column_operation()
        {
            var model = new DatabaseModel();
            var table = new Table("dbo.MyTable", new[] { new Column("Foo", typeof(int)) });
            var operation = new DropColumnOperation("dbo.MyTable", "Foo");

            model.AddTable(table);

            Assert.Equal(1, table.Columns.Count);

            operation.Accept(new DatabaseModelModifier(), model);

            Assert.Equal(0, table.Columns.Count);
        }

        [Fact]
        public void Visit_with_alter_column_operation()
        {
            var model = new DatabaseModel();
            var column = new Column("Foo", typeof(string));
            var table = new Table("dbo.MyTable", new[] { column });
            var operation
                = new AlterColumnOperation(
                    "dbo.MyTable",
                    new Column("Foo", typeof(int))
                        {
                            DataType = "T",
                            IsNullable = false,
                            DefaultValue = "V",
                            DefaultSql = "Sql",
                            ValueGenerationStrategy = ValueGeneration.OnAddAndUpdate,
                            IsTimestamp = true,
                            MaxLength = 4,
                            Precision = 3,
                            Scale = 2,
                            IsFixedLength = true,
                            IsUnicode = true
                        },
                    isDestructiveChange: true);

            model.AddTable(table);

            operation.Accept(new DatabaseModelModifier(), model);

            Assert.Equal("Foo", column.Name);
            Assert.Same(typeof(int), column.ClrType);
            Assert.Equal("T", column.DataType);
            Assert.Equal("V", column.DefaultValue);
            Assert.Equal("Sql", column.DefaultSql);
            Assert.Equal(ValueGeneration.OnAddAndUpdate, column.ValueGenerationStrategy);
            Assert.True(column.IsTimestamp);
            Assert.Equal(4, column.MaxLength.Value);
            Assert.Equal(3, column.Precision.Value);
            Assert.Equal(2, column.Scale.Value);
            Assert.True(column.IsFixedLength.Value);
            Assert.True(column.IsUnicode.Value);
        }

        [Fact]
        public void Visit_with_rename_column_operation()
        {
            var model = new DatabaseModel();
            var column = new Column("Foo", typeof(int));
            var table = new Table("dbo.MyTable", new[] { column });
            var operation = new RenameColumnOperation("dbo.MyTable", "Foo", "Bar");

            model.AddTable(table);

            operation.Accept(new DatabaseModelModifier(), model);

            Assert.Equal("Bar", column.Name);
        }

        [Fact]
        public void Visit_with_add_default_constraint_operation()
        {
            var model = new DatabaseModel();
            var column = new Column("Foo", typeof(int));
            var table = new Table("dbo.MyTable", new[] { column });
            var operation = new AddDefaultConstraintOperation("dbo.MyTable", "Foo", 5, "Sql");

            model.AddTable(table);

            operation.Accept(new DatabaseModelModifier(), model);

            Assert.Equal(5, column.DefaultValue);
            Assert.Equal("Sql", column.DefaultSql);
        }

        [Fact]
        public void Visit_with_drop_default_constraint_operation()
        {
            var model = new DatabaseModel();
            var column = new Column("Foo", typeof(int)) { DefaultValue = 5, DefaultSql = "Sql" };
            var table = new Table("dbo.MyTable", new[] { column });
            var operation = new DropDefaultConstraintOperation("dbo.MyTable", "Foo");

            model.AddTable(table);

            Assert.True(column.HasDefault);

            operation.Accept(new DatabaseModelModifier(), model);

            Assert.False(column.HasDefault);
        }

        [Fact]
        public void Visit_with_add_primary_key_operation()
        {
            var model = new DatabaseModel();
            var column = new Column("Foo", typeof(int));
            var table = new Table("dbo.MyTable", new[] { column });
            var operation = new AddPrimaryKeyOperation("dbo.MyTable", "PK", new[] { "Foo" }, isClustered: false);

            model.AddTable(table);

            Assert.Null(table.PrimaryKey);

            operation.Accept(new DatabaseModelModifier(), model);

            Assert.NotNull(table.PrimaryKey);
            Assert.Equal("PK", table.PrimaryKey.Name);
            Assert.Equal(1, table.PrimaryKey.Columns.Count);
            Assert.Same(column, table.PrimaryKey.Columns[0]);
            Assert.False(table.PrimaryKey.IsClustered);
        }

        [Fact]
        public void Visit_with_drop_primary_key_operation()
        {
            var model = new DatabaseModel();
            var column = new Column("Foo", typeof(int));
            var table
                = new Table("dbo.MyTable", new[] { column })
                    {
                        PrimaryKey = new PrimaryKey("PK", new[] { column }, isClustered: false)
                    };
            var operation = new DropPrimaryKeyOperation("dbo.MyTable", "PK");

            model.AddTable(table);

            Assert.NotNull(table.PrimaryKey);

            operation.Accept(new DatabaseModelModifier(), model);

            Assert.Null(table.PrimaryKey);
        }

        [Fact]
        public void Visit_with_add_foreign_key_operation()
        {
            var model = new DatabaseModel();
            var column0 = new Column("Foo", typeof(int));
            var column1 = new Column("Bar", typeof(int));
            var table0 = new Table("dbo.T0", new[] { column0 });
            var table1 = new Table("dbo.T1", new[] { column1 });
            var operation = new AddForeignKeyOperation(
                "dbo.T0", "FK", new[] { "Foo" }, "dbo.T1", new[] { "Bar" }, cascadeDelete: true);

            model.AddTable(table0);
            model.AddTable(table1);

            Assert.Equal(0, table0.ForeignKeys.Count);

            operation.Accept(new DatabaseModelModifier(), model);

            Assert.Equal(1, table0.ForeignKeys.Count);

            var foreignKey = table0.ForeignKeys[0];

            Assert.Equal("FK", foreignKey.Name);
            Assert.Same(table0, foreignKey.Table);
            Assert.Same(table1, foreignKey.ReferencedTable);
            Assert.Equal(1, foreignKey.Columns.Count);
            Assert.Equal(1, foreignKey.ReferencedColumns.Count);
            Assert.Same(column0, foreignKey.Columns[0]);
            Assert.Same(column1, foreignKey.ReferencedColumns[0]);
            Assert.True(foreignKey.CascadeDelete);
        }

        [Fact]
        public void Visit_with_drop_foreign_key_operation()
        {
            var model = new DatabaseModel();
            var column0 = new Column("Foo", typeof(int));
            var column1 = new Column("Bar", typeof(int));
            var table0 = new Table("dbo.T0", new[] { column0 });
            var table1 = new Table("dbo.T1", new[] { column1 });
            var foreignKey = new ForeignKey("FK", new[] { column0 }, new[] { column1 }, cascadeDelete: true);
            var operation = new DropForeignKeyOperation("dbo.T0", "FK");

            model.AddTable(table0);
            model.AddTable(table1);
            table0.AddForeignKey(foreignKey);

            Assert.Equal(1, table0.ForeignKeys.Count);

            operation.Accept(new DatabaseModelModifier(), model);

            Assert.Equal(0, table0.ForeignKeys.Count);
        }

        [Fact]
        public void Visit_with_create_index_operation()
        {
            var model = new DatabaseModel();
            var column = new Column("Foo", typeof(int));
            var table = new Table("dbo.MyTable", new[] { column });
            var operation = new CreateIndexOperation(
                "dbo.MyTable", "IX", new[] { "Foo" }, isUnique: true, isClustered: true);

            model.AddTable(table);

            Assert.Equal(0, table.Indexes.Count);

            operation.Accept(new DatabaseModelModifier(), model);

            Assert.Equal(1, table.Indexes.Count);

            var index = table.Indexes[0];

            Assert.Equal("IX", index.Name);
            Assert.Same(table, index.Table);
            Assert.Equal(1, index.Columns.Count);
            Assert.Same(column, index.Columns[0]);
            Assert.True(index.IsUnique);
            Assert.True(index.IsClustered);
        }

        [Fact]
        public void Visit_with_drop_index_operation()
        {
            var model = new DatabaseModel();
            var column = new Column("Foo", typeof(int));
            var table = new Table("dbo.MyTable", new[] { column });
            var index = new Index("IX", new[] { column }, isUnique: true, isClustered: true);
            var operation = new DropIndexOperation("dbo.MyTable", "IX");

            model.AddTable(table);
            table.AddIndex(index);

            Assert.Equal(1, table.Indexes.Count);

            operation.Accept(new DatabaseModelModifier(), model);

            Assert.Equal(0, table.Indexes.Count);
        }

        [Fact]
        public void Visit_with_rename_index_operation()
        {
            var model = new DatabaseModel();
            var column = new Column("Foo", typeof(int));
            var table = new Table("dbo.MyTable", new[] { column });
            var index = new Index("IX", new[] { column }, isUnique: true, isClustered: true);
            var operation = new RenameIndexOperation("dbo.MyTable", "IX", "RenamedIndex");

            model.AddTable(table);
            table.AddIndex(index);

            operation.Accept(new DatabaseModelModifier(), model);

            Assert.Equal(1, table.Indexes.Count);
            Assert.Equal("RenamedIndex", table.Indexes[0].Name);
        }
    }
}

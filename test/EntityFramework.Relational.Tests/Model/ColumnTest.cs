// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.Relational.Utilities;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests.Model
{
    public class ColumnTest
    {
        [Fact]
        public void Create_and_initialize_column()
        {
            var column = new Column("Foo", "int")
                { IsNullable = true, DefaultValue = 5 };

            Assert.Equal("Foo", column.Name);
            Assert.Null(column.ClrType);
            Assert.Equal("int", column.DataType);
            Assert.True(column.IsNullable);
            Assert.Equal(5, column.DefaultValue);
            Assert.Null(column.DefaultSql);

            column = new Column("Bar", typeof(int), null)
                { IsNullable = false, DefaultSql = "GETDATE()" };

            Assert.Equal("Bar", column.Name);
            Assert.Same(typeof(int), column.ClrType);
            Assert.Null(column.DataType);
            Assert.False(column.IsNullable);
            Assert.Null(column.DefaultValue);
            Assert.Equal("GETDATE()", column.DefaultSql);
        }

        [Fact]
        public void Can_set_name()
        {
            var column = new Column("Foo", typeof(int));

            Assert.Equal("Foo", column.Name);

            column.Name = "Bar";

            Assert.Equal("Bar", column.Name);
        }

        [Fact]
        public void Copy_replicates_source()
        {
            var column1 
                = new Column("Foo", typeof(string))
                    {
                        DataType = "T",
                        IsNullable = false,
                        DefaultValue = "V",
                        DefaultSql = "Sql",
                        ValueGenerationStrategy = ValueGenerationOnSave.WhenInsertingAndUpdating,
                        IsTimestamp = true,
                        MaxLength = 4,
                        Precision = 3,
                        Scale = 2,
                        IsFixedLength = true,
                        IsUnicode = true
                    };
            var column2 = new Column("Bar", typeof(int));

            column2.Copy(column1);

            Assert.Equal("Foo", column2.Name);
            Assert.Same(typeof(string), column2.ClrType);
            Assert.Equal("T", column2.DataType);
            Assert.Equal("V", column2.DefaultValue);
            Assert.Equal("Sql", column2.DefaultSql);
            Assert.Equal(ValueGenerationOnSave.WhenInsertingAndUpdating, column2.ValueGenerationStrategy);
            Assert.True(column2.IsTimestamp);
            Assert.Equal(4, column2.MaxLength.Value);
            Assert.Equal(3, column2.Precision.Value);
            Assert.Equal(2, column2.Scale.Value);
            Assert.True(column2.IsFixedLength.Value);
            Assert.True(column2.IsUnicode.Value);
        }

        [Fact]
        public void Clone_replicates_instance()
        {
            var cloneContext = new CloneContext();
            var column
                = new Column("Foo", typeof(string))
                {
                    DataType = "T",
                    IsNullable = false,
                    DefaultValue = "V",
                    DefaultSql = "Sql",
                    ValueGenerationStrategy = ValueGenerationOnSave.WhenInsertingAndUpdating,
                    IsTimestamp = true,
                    MaxLength = 4,
                    Precision = 3,
                    Scale = 2,
                    IsFixedLength = true,
                    IsUnicode = true
                };
            var clone = column.Clone(cloneContext);

            Assert.NotSame(column, clone);
            Assert.Equal("Foo", clone.Name);
            Assert.Same(typeof(string), clone.ClrType);
            Assert.Equal("T", clone.DataType);
            Assert.Equal("V", clone.DefaultValue);
            Assert.Equal("Sql", clone.DefaultSql);
            Assert.Equal(ValueGenerationOnSave.WhenInsertingAndUpdating, clone.ValueGenerationStrategy);
            Assert.True(clone.IsTimestamp);
            Assert.Equal(4, clone.MaxLength.Value);
            Assert.Equal(3, clone.Precision.Value);
            Assert.Equal(2, clone.Scale.Value);
            Assert.True(clone.IsFixedLength.Value);
            Assert.True(clone.IsUnicode.Value);
        }

        [Fact]
        public void Clone_gets_existing_clone_from_cache()
        {
            var cloneContext = new CloneContext();
            var column = new Column("Foo", typeof(string));
            var clone = new Column("Foo", typeof(string));

            Assert.Same(clone, cloneContext.GetOrAdd(column, () => clone));
            Assert.Same(clone, column.Clone(cloneContext));
        }

        public void Clone_adds_new_clone_to_cache()
        {
            var cloneContext = new CloneContext();
            var column = new Column("Foo", typeof(string));
            var clone = column.Clone(cloneContext);

            Assert.NotSame(column, clone);
            Assert.Same(clone, cloneContext.GetOrAdd(column, () => null));
        }
    }
}

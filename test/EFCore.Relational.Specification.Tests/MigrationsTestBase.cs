// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

#nullable enable

namespace Microsoft.EntityFrameworkCore
{
    public class MigrationsTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : MigrationsTestBase<TFixture>.MigrationsFixtureBase, new()
    {
        private readonly ISqlGenerationHelper _sqlGenerationHelper;
        private readonly IRelationalTypeMappingSource _typeMappingSource;

        protected TFixture Fixture { get; }

        protected MigrationsTestBase(TFixture fixture)
        {
            Fixture = fixture;
            _sqlGenerationHelper = Fixture.ServiceProvider.GetService<ISqlGenerationHelper>();
            _typeMappingSource = Fixture.ServiceProvider.GetService<IRelationalTypeMappingSource>();
        }

        [ConditionalFact]
        public virtual Task Create_table()
            => Test(
                builder => { },
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.Property<string>("Name");
                        e.HasKey("Id");
                    }),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    Assert.Collection(
                        table.Columns,
                        c => Assert.Equal("Id", c.Name),
                        c => Assert.Equal("Name", c.Name));
                    Assert.Same(
                        table.Columns.Single(c => c.Name == "Id"),
                        Assert.Single(table.PrimaryKey!.Columns));
                });

        [ConditionalFact]
        public virtual async Task Create_table_all_settings()
        {
            var intStoreType = TypeMappingSource.FindMapping(typeof(int)).StoreType;
            var char11StoreType = TypeMappingSource.FindMapping(typeof(string), storeTypeName: null, size: 11).StoreType;

            await Test(
                builder => builder.Entity(
                    "Employers", e =>
                    {
                        e.Property<int>("Id");
                        e.HasKey("Id");
                    }),
                builder => { },
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.ToTable("People", "dbo2");

                        e.Property<int>("CustomId");
                        e.Property<int>("EmployerId")
                            .HasComment("Employer ID comment");
                        e.Property<string>("SSN")
                            .HasColumnType(char11StoreType)
                            .IsRequired(false);

                        e.HasKey("CustomId");
                        e.HasAlternateKey("SSN");
                        e.HasCheckConstraint("CK_EmployerId", $"{DelimitIdentifier("EmployerId")} > 0");
                        e.HasOne("Employers").WithMany("People").HasForeignKey("EmployerId");

                        e.HasComment("Table comment");
                    }),
                model =>
                {
                    var employersTable = Assert.Single(model.Tables, t => t.Name == "Employers");
                    var peopleTable = Assert.Single(model.Tables, t => t.Name == "People");

                    Assert.Equal("People", peopleTable.Name);
                    Assert.Equal("dbo2", peopleTable.Schema);

                    Assert.Collection(
                        peopleTable.Columns.OrderBy(c => c.Name),
                        c =>
                        {
                            Assert.Equal("CustomId", c.Name);
                            Assert.False(c.IsNullable);
                            Assert.Equal(intStoreType, c.StoreType);
                            Assert.Null(c.Comment);
                        },
                        c =>
                        {
                            Assert.Equal("EmployerId", c.Name);
                            Assert.False(c.IsNullable);
                            Assert.Equal(intStoreType, c.StoreType);
                            Assert.Equal("Employer ID comment", c.Comment);
                        },
                        c =>
                        {
                            Assert.Equal("SSN", c.Name);
                            Assert.False(c.IsNullable);
                            Assert.Equal(char11StoreType, c.StoreType);
                            Assert.Null(c.Comment);
                        });

                    Assert.Same(
                        peopleTable.Columns.Single(c => c.Name == "CustomId"),
                        Assert.Single(peopleTable.PrimaryKey!.Columns));
                    Assert.Same(
                        peopleTable.Columns.Single(c => c.Name == "SSN"),
                        Assert.Single(Assert.Single(peopleTable.UniqueConstraints).Columns));
                    // TODO: Need to scaffold check constraints, https://github.com/aspnet/EntityFrameworkCore/issues/15408

                    var foreignKey = Assert.Single(peopleTable.ForeignKeys);
                    Assert.Same(peopleTable, foreignKey.Table);
                    Assert.Same(peopleTable.Columns.Single(c => c.Name == "EmployerId"), Assert.Single(foreignKey.Columns));
                    Assert.Same(employersTable, foreignKey.PrincipalTable);
                    Assert.Same(employersTable.Columns.Single(), Assert.Single(foreignKey.PrincipalColumns));

                    Assert.Equal("Table comment", peopleTable.Comment);
                });
        }

        [ConditionalFact]
        public virtual Task Create_table_no_key()
            => Test(
                builder => { },
                builder => builder.Entity("Anonymous").Property<int>("SomeColumn"),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    Assert.Null(table.PrimaryKey);
                });

        [ConditionalFact]
        public virtual Task Create_table_with_comments()
            => Test(
                builder => { },
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<string>("Name").HasComment("Column comment");
                        e.HasComment("Table comment");
                    }),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    Assert.Equal("Table comment", table.Comment);
                    var column = Assert.Single(table.Columns, c => c.Name == "Name");
                    Assert.Equal("Column comment", column.Comment);
                });

        [ConditionalFact]
        public virtual Task Create_table_with_multiline_comments()
        {
            var tableComment = @"This is a multi-line
table comment.
More information can
be found in the docs.";
            var columnComment = @"This is a multi-line
column comment.
More information can
be found in the docs.";

            return Test(
                builder => { },
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<string>("Name").HasComment(columnComment);
                        e.HasComment(tableComment);
                    }),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    Assert.Equal(tableComment, table.Comment);
                    var column = Assert.Single(table.Columns, c => c.Name == "Name");
                    Assert.Equal(columnComment, column.Comment);
                });
        }

        [ConditionalFact]
        public virtual Task Alter_table_add_comment()
            => Test(
                builder => builder.Entity("People").Property<int>("Id"),
                builder => { },
                builder => builder.Entity("People").HasComment("Table comment"),
                model => Assert.Equal("Table comment", Assert.Single(model.Tables).Comment));

        [ConditionalFact]
        public virtual Task Alter_table_add_comment_non_default_schema()
            => Test(
                builder => builder.Entity("People")
                    .ToTable("People", "SomeOtherSchema")
                    .Property<int>("Id"),
                builder => { },
                builder => builder.Entity("People")
                    .ToTable("People", "SomeOtherSchema")
                    .HasComment("Table comment"),
                model => Assert.Equal("Table comment", Assert.Single(model.Tables).Comment));

        [ConditionalFact]
        public virtual Task Alter_table_change_comment()
            => Test(
                builder => builder.Entity("People").Property<int>("Id"),
                builder => builder.Entity("People").HasComment("Table comment1"),
                builder => builder.Entity("People").HasComment("Table comment2"),
                model => Assert.Equal("Table comment2", Assert.Single(model.Tables).Comment));

        [ConditionalFact]
        public virtual Task Alter_table_remove_comment()
            => Test(
                builder => builder.Entity("People").Property<int>("Id"),
                builder => builder.Entity("People").HasComment("Table comment1"),
                builder => { },
                model => Assert.Null(Assert.Single(model.Tables).Comment));

        [ConditionalFact]
        public virtual Task Drop_table()
            => Test(
                builder => builder.Entity("People", e => e.Property<int>("Id")),
                builder => { },
                model => Assert.Empty(model.Tables));

        [ConditionalFact]
        public virtual Task Rename_table()
            => Test(
                builder => builder.Entity("People").Property<int>("Id"),
                builder => builder.Entity("people").Property<int>("Id"),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    Assert.Equal("people", table.Name);
                });

        [ConditionalFact]
        public virtual Task Rename_table_with_primary_key()
            => Test(
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.HasKey("Id");
                    }),
                builder => builder.Entity(
                    "people", e =>
                    {
                        e.Property<int>("Id");
                        e.HasKey("Id");
                    }),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    Assert.Equal("people", table.Name);
                });

        [ConditionalFact]
        public virtual Task Move_table()
            => Test(
                builder => builder.Entity("TestTable").Property<int>("Id"),
                builder => { },
                builder => builder.Entity("TestTable").ToTable("TestTable", "TestTableSchema"),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    Assert.Equal("TestTableSchema", table.Schema);
                    Assert.Equal("TestTable", table.Name);
                });

        [ConditionalFact]
        public virtual Task Create_schema()
            => Test(
                builder => { },
                builder => builder.Entity("People")
                    .ToTable("People", "SomeOtherSchema")
                    .Property<int>("Id"),
                model => Assert.Equal("SomeOtherSchema", Assert.Single(model.Tables).Schema));

        [ConditionalFact]
        public virtual Task Add_column_with_defaultValue_string()
            => Test(
                builder => builder.Entity("People").Property<int>("Id"),
                builder => { },
                builder => builder.Entity("People").Property<string>("Name")
                    .IsRequired()
                    .HasDefaultValue("John Doe"),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    Assert.Equal(2, table.Columns.Count);
                    var nameColumn = Assert.Single(table.Columns, c => c.Name == "Name");
                    Assert.False(nameColumn.IsNullable);
                    Assert.Contains("John Doe", nameColumn.DefaultValueSql);
                });

        [ConditionalFact]
        public virtual Task Add_column_with_defaultValue_datetime()
            => Test(
                builder => builder.Entity("People").Property<int>("Id"),
                builder => { },
                builder => builder.Entity("People").Property<DateTime>("Birthday")
                    .HasDefaultValue(new DateTime(2015, 4, 12, 17, 5, 0)),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    Assert.Equal(2, table.Columns.Count);
                    var birthdayColumn = Assert.Single(table.Columns, c => c.Name == "Birthday");
                    Assert.False(birthdayColumn.IsNullable);
                });

        [ConditionalFact]
        public virtual Task Add_column_with_defaultValueSql()
            => Test(
                builder => builder.Entity("People").Property<int>("Id"),
                builder => { },
                builder => builder.Entity("People").Property<int>("Sum")
                    .HasDefaultValueSql("1 + 2"),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    Assert.Equal(2, table.Columns.Count);
                    var sumColumn = Assert.Single(table.Columns, c => c.Name == "Sum");
                    Assert.Contains("1", sumColumn.DefaultValueSql);
                    Assert.Contains("+", sumColumn.DefaultValueSql);
                    Assert.Contains("2", sumColumn.DefaultValueSql);
                });

        [ConditionalFact]
        public virtual Task Add_column_with_computedSql()
            => Test(
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<int>("X");
                        e.Property<int>("Y");
                    }),
                builder => { },
                builder => builder.Entity("People").Property<string>("Sum").HasComputedColumnSql($"{DelimitIdentifier("X")} + {DelimitIdentifier("Y")}"),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var sumColumn = Assert.Single(table.Columns, c => c.Name == "Sum");
                    Assert.Contains("X", sumColumn.ComputedColumnSql);
                    Assert.Contains("Y", sumColumn.ComputedColumnSql);
                });

        // TODO: Check this out
        [ConditionalFact]
        public virtual Task Add_column_with_required()
            => Test(
                builder => builder.Entity("People").Property<int>("Id"),
                builder => { },
                builder => builder.Entity("People").Property<string>("Name").IsRequired(),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var column = Assert.Single(table.Columns, c => c.Name == "Name");
                    Assert.Equal(TypeMappingSource.FindMapping(typeof(string)).StoreType, column.StoreType);
                    Assert.False(column.IsNullable);
                });

        [ConditionalFact]
        public virtual Task Add_column_with_ansi()
            => Test(
                builder => builder.Entity("People").Property<int>("Id"),
                builder => { },
                builder => builder.Entity("People").Property<string>("Name").IsUnicode(false),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var column = Assert.Single(table.Columns, c => c.Name == "Name");
                    Assert.Equal(
                        TypeMappingSource
                            .FindMapping(typeof(string), storeTypeName: null, unicode: false)
                            .StoreType, column.StoreType);
                    Assert.True(column.IsNullable);
                });

        [ConditionalFact]
        public virtual Task Add_column_with_max_length()
            => Test(
                builder => builder.Entity("People").Property<int>("Id"),
                builder => { },
                builder => builder.Entity("People").Property<string>("Name").HasMaxLength(30),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var column = Assert.Single(table.Columns, c => c.Name == "Name");
                    Assert.Equal(
                        TypeMappingSource
                            .FindMapping(typeof(string), storeTypeName: null, size: 30)
                            .StoreType,
                        column.StoreType);
                });

        [ConditionalFact]
        public virtual Task Add_column_with_max_length_on_derived()
            => Test(
                builder =>
                {
                    builder.Entity("Person");
                    builder.Entity(
                        "SpecialPerson", e =>
                        {
                            e.HasBaseType("Person");
                            e.Property<string>("Name").HasMaxLength(30);
                        });

                    builder.Entity("MoreSpecialPerson").HasBaseType("SpecialPerson");
                },
                builder => { },
                builder => builder.Entity("Person").Property<string>("Name").HasMaxLength(30),
                model =>
                {
                    var table = Assert.Single(model.Tables, t => t.Name == "Person");
                    var column = Assert.Single(table.Columns, c => c.Name == "Name");
                    Assert.Equal(
                        TypeMappingSource
                            .FindMapping(typeof(string), storeTypeName: null, size: 30)
                            .StoreType,
                        column.StoreType);
                });

        [ConditionalFact]
        public virtual Task Add_column_with_fixed_length()
            => Test(
                builder => builder.Entity("People").Property<int>("Id"),
                builder => { },
                builder => builder.Entity("People").Property<string>("Name")
                    .IsFixedLength()
                    .HasMaxLength(100),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var column = Assert.Single(table.Columns, c => c.Name == "Name");
                    Assert.Equal(
                        TypeMappingSource
                            .FindMapping(typeof(string), storeTypeName: null, fixedLength: true, size: 100)
                            .StoreType,
                        column.StoreType);
                });

        [ConditionalFact]
        public virtual Task Add_column_with_comment()
            => Test(
                builder => builder.Entity("People").Property<int>("Id"),
                builder => { },
                builder => builder.Entity("People").Property<string>("FullName").HasComment("My comment"),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var column = Assert.Single(table.Columns, c => c.Name == "FullName");
                    Assert.Equal("My comment", column.Comment);
                });

        [ConditionalFact]
        public virtual Task Add_column_shared()
            => Test(
                builder =>
                {
                    builder.Entity("Base").Property<int>("Id");
                    builder.Entity("Derived1").Property<string>("Foo");
                    builder.Entity("Derived2").Property<string>("Foo");
                },
                builder => { },
                builder => builder.Entity("Base").Property<string>("Foo"),
                model =>
                {
                    // var table = Assert.Single(model.Tables);
                    // var column = Assert.Single(table.Columns, c => c.Name == "Name");
                    // Assert.Equal("nvarchar(30)", column.StoreType);
                });

        [ConditionalFact]
        public virtual Task Alter_column_change_type()
            => Test(
                builder => builder.Entity("People").Property<int>("Id"),
                builder => builder.Entity("People").Property<int>("SomeColumn"),
                builder => builder.Entity("People").Property<long>("SomeColumn"),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var column = Assert.Single(table.Columns, c => c.Name == "SomeColumn");
                    Assert.Equal(_typeMappingSource.FindMapping(typeof(long)).StoreType, column.StoreType);
                });

        [ConditionalFact]
        public virtual Task Alter_column_make_required()
            => Test(
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<string>("SomeColumn");
                    }),
                builder => { },
                builder => builder.Entity("People").Property<string>("SomeColumn").IsRequired(),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var column = Assert.Single(table.Columns, c => c.Name != "Id");
                    Assert.False(column.IsNullable);
                });

        [ConditionalFact]
        public virtual Task Alter_column_make_required_with_index()
            => Test(
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<string>("SomeColumn");
                        e.HasIndex("SomeColumn");
                    }),
                builder => { },
                builder => builder.Entity("People").Property<string>("SomeColumn").IsRequired(),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var column = Assert.Single(table.Columns, c => c.Name != "Id");
                    Assert.False(column.IsNullable);
                    var index = Assert.Single(table.Indexes);
                    Assert.Same(column, Assert.Single(index.Columns));
                });

        [ConditionalFact]
        public virtual Task Alter_column_make_required_with_composite_index()
            => Test(
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<string>("FirstName");
                        e.Property<string>("LastName");
                        e.HasIndex("FirstName", "LastName");
                    }),
                builder => { },
                builder => builder.Entity("People").Property<string>("FirstName").IsRequired(),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var firstNameColumn = Assert.Single(table.Columns, c => c.Name == "FirstName");
                    Assert.False(firstNameColumn.IsNullable);
                    var index = Assert.Single(table.Indexes);
                    Assert.Equal(2, index.Columns.Count);
                    Assert.Contains(table.Columns.Single(c => c.Name == "FirstName"), index.Columns);
                    Assert.Contains(table.Columns.Single(c => c.Name == "LastName"), index.Columns);
                });

        [ConditionalFact]
        public virtual Task Alter_column_make_computed()
            => Test(
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<int>("X");
                        e.Property<int>("Y");
                    }),
                builder => builder.Entity("People").Property<int>("Sum"),
                builder => builder.Entity("People").Property<int>("Sum").HasComputedColumnSql($"{DelimitIdentifier("X")} + {DelimitIdentifier("Y")}"),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var sumColumn = Assert.Single(table.Columns, c => c.Name == "Sum");
                    Assert.Contains("X", sumColumn.ComputedColumnSql);
                    Assert.Contains("Y", sumColumn.ComputedColumnSql);
                    Assert.Contains("+", sumColumn.ComputedColumnSql);
                });

        [ConditionalFact]
        public virtual Task Alter_column_change_computed()
            => Test(
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<int>("X");
                        e.Property<int>("Y");
                        e.Property<int>("Sum");
                    }),
                builder => builder.Entity("People").Property<int>("Sum").HasComputedColumnSql($"{DelimitIdentifier("X")} + {DelimitIdentifier("Y")}"),
                builder => builder.Entity("People").Property<int>("Sum").HasComputedColumnSql($"{DelimitIdentifier("X")} - {DelimitIdentifier("Y")}"),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var sumColumn = Assert.Single(table.Columns, c => c.Name == "Sum");
                    Assert.Contains("X", sumColumn.ComputedColumnSql);
                    Assert.Contains("Y", sumColumn.ComputedColumnSql);
                    Assert.Contains("-", sumColumn.ComputedColumnSql);
                });

        [ConditionalFact]
        public virtual Task Alter_column_add_comment()
            => Test(
                builder => builder.Entity("People").Property<int>("Id"),
                builder => builder.Entity("People").Property<int>("Id").HasComment("Some comment"),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var column = Assert.Single(table.Columns);
                    Assert.Equal("Some comment", column.Comment);
                });

        [ConditionalFact]
        public virtual Task Alter_column_change_comment()
            => Test(
                builder => builder.Entity("People").Property<int>("Id").HasComment("Some comment1"),
                builder => builder.Entity("People").Property<int>("Id").HasComment("Some comment2"),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var column = Assert.Single(table.Columns);
                    Assert.Equal("Some comment2", column.Comment);
                });

        [ConditionalFact]
        public virtual Task Alter_column_remove_comment()
            => Test(
                builder => builder.Entity("People").Property<int>("Id").HasComment("Some comment"),
                builder => builder.Entity("People").Property<int>("Id"),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var column = Assert.Single(table.Columns);
                    Assert.Null(column.Comment);
                });

        [ConditionalFact]
        public virtual Task Drop_column()
            => Test(
                builder => builder.Entity("People").Property<int>("Id"),
                builder => builder.Entity("People").Property<int>("SomeColumn"),
                builder => { },
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    Assert.Equal("Id", Assert.Single(table.Columns).Name);
                });

        [ConditionalFact]
        public virtual Task Drop_column_primary_key()
            => Test(
                builder => builder.Entity("People").Property<int>("SomeColumn"),
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.HasKey("Id");
                    }),
                builder => { },
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    Assert.Equal("SomeColumn", Assert.Single(table.Columns).Name);
                });

        [ConditionalFact]
        public virtual Task Rename_column()
            => Test(
                builder => builder.Entity("People").Property<int>("Id"),
                builder => builder.Entity("People").Property<string>("SomeColumn"),
                builder => builder.Entity("People").Property<string>("somecolumn"),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    Assert.Equal(2, table.Columns.Count);
                    Assert.Single(table.Columns, c => c.Name == "somecolumn");
                });

        [ConditionalFact]
        public virtual Task Create_index()
            => Test(
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<string>("FirstName");
                    }),
                builder => { },
                builder => builder.Entity("People").HasIndex("FirstName"),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var index = Assert.Single(table.Indexes);
                    Assert.Same(table, index.Table);
                    Assert.Same(table.Columns.Single(c => c.Name == "FirstName"), Assert.Single(index.Columns));
                    Assert.Equal("IX_People_FirstName", index.Name);
                    Assert.False(index.IsUnique);
                    Assert.Null(index.Filter);
                });

        [ConditionalFact]
        public virtual Task Create_index_unique()
            => Test(
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<string>("FirstName");
                        e.Property<string>("LastName");
                    }),
                builder => { },
                builder => builder.Entity("People").HasIndex("FirstName", "LastName").IsUnique(),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var index = Assert.Single(table.Indexes);
                    Assert.True(index.IsUnique);
                });

        [ConditionalFact]
        public virtual Task Create_index_with_filter()
            => Test(
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<string>("Name");
                    }),
                builder => { },
                builder => builder.Entity("People").HasIndex("Name")
                    .HasFilter($"{DelimitIdentifier("Name")} IS NOT NULL"),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var index = Assert.Single(table.Indexes);
                    Assert.Same(table.Columns.Single(c => c.Name == "Name"), Assert.Single(index.Columns));
                    Assert.Contains("Name", index.Filter);
                });

        [ConditionalFact]
        public virtual Task Create_unique_index_with_filter()
            => Test(
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<string>("Name");
                    }),
                builder => { },
                builder => builder.Entity("People").HasIndex("Name").IsUnique()
                    .HasFilter($"{DelimitIdentifier("Name")} IS NOT NULL AND {DelimitIdentifier("Name")} <> ''"),
                model => Assert.Contains("Name", model.Tables.Single().Indexes.Single().Filter));

        [ConditionalFact]
        public virtual Task Drop_index()
            => Test(
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<int>("SomeField");
                    }),
                builder => builder.Entity("People").HasIndex("SomeField"),
                builder => { },
                model => Assert.Empty(Assert.Single(model.Tables).Indexes));

        [ConditionalFact]
        public virtual Task Rename_index()
            => Test(
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<string>("FirstName");
                    }),
                builder => builder.Entity("People").HasIndex("FirstName").HasName("Foo"),
                builder => builder.Entity("People").HasIndex("FirstName").HasName("foo"),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var index = Assert.Single(table.Indexes);
                    Assert.Equal("foo", index.Name);
                });

        [ConditionalFact]
        public virtual Task Add_primary_key()
            => Test(
                builder => builder.Entity("People").Property<string>("SomeField"),
                builder => { },
                builder => builder.Entity("People").HasKey("SomeField"),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var primaryKey = table.PrimaryKey;
                    Assert.NotNull(primaryKey);
                    Assert.Same(table, primaryKey!.Table);
                    Assert.Same(table.Columns.Single(), Assert.Single(primaryKey.Columns));
                    Assert.Equal("PK_People", primaryKey.Name);
                });

        [ConditionalFact]
        public virtual Task Add_primary_key_with_name()
            => Test(
                builder => builder.Entity("People").Property<string>("SomeField"),
                builder => { },
                builder => builder.Entity("People").HasKey("SomeField").HasName("PK_Foo"),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var primaryKey = table.PrimaryKey;
                    Assert.NotNull(primaryKey);
                    Assert.Same(table, primaryKey!.Table);
                    Assert.Same(table.Columns.Single(), Assert.Single(primaryKey.Columns));
                    Assert.Equal("PK_Foo", primaryKey.Name);
                });

        [ConditionalFact]
        public virtual Task Add_primary_key_composite_with_name()
            => Test(
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<string>("SomeField1");
                        e.Property<string>("SomeField2");
                    }),
                builder => { },
                builder => builder.Entity("People").HasKey("SomeField1", "SomeField2").HasName("PK_Foo"),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var primaryKey = table.PrimaryKey;
                    Assert.NotNull(primaryKey);
                    Assert.Same(table, primaryKey!.Table!);
                    Assert.Collection(
                        primaryKey.Columns,
                        c => Assert.Same(table.Columns[0], c),
                        c => Assert.Same(table.Columns[1], c));
                    Assert.Equal("PK_Foo", primaryKey.Name);
                });

        [ConditionalFact]
        public virtual Task Drop_primary_key()
            => Test(
                builder => builder.Entity("People").Property<int>("SomeField"),
                builder => builder.Entity("People").HasKey("SomeField"),
                builder => { },
                model => Assert.Null(Assert.Single(model.Tables).PrimaryKey));

        [ConditionalFact]
        public virtual Task Add_foreign_key()
            => Test(
                builder =>
                {
                    builder.Entity(
                        "Customers", e =>
                        {
                            e.Property<int>("Id");
                            e.HasKey("Id");
                        });
                    builder.Entity(
                        "Orders", e =>
                        {
                            e.Property<int>("Id");
                            e.Property<int>("CustomerId");
                        });
                },
                builder => { },
                builder => builder.Entity("Orders").HasOne("Customers").WithMany()
                    .HasForeignKey("CustomerId"),
                model =>
                {
                    var customersTable = Assert.Single(model.Tables, t => t.Name == "Customers");
                    var ordersTable = Assert.Single(model.Tables, t => t.Name == "Orders");
                    var foreignKey = ordersTable.ForeignKeys.Single();
                    Assert.Equal("FK_Orders_Customers_CustomerId", foreignKey.Name);
                    Assert.Equal(ReferentialAction.NoAction, foreignKey.OnDelete);
                    Assert.Same(customersTable, foreignKey.PrincipalTable);
                    Assert.Same(customersTable.Columns.Single(), Assert.Single(foreignKey.PrincipalColumns));
                    Assert.Equal("CustomerId", Assert.Single(foreignKey.Columns).Name);
                });

        [ConditionalFact]
        public virtual Task Add_foreign_key_with_name()
            => Test(
                builder =>
                {
                    builder.Entity(
                        "Customers", e =>
                        {
                            e.Property<int>("Id");
                            e.HasKey("Id");
                        });
                    builder.Entity(
                        "Orders", e =>
                        {
                            e.Property<int>("Id");
                            e.Property<int>("CustomerId");
                        });
                },
                builder => { },
                builder => builder.Entity("Orders").HasOne("Customers").WithMany()
                    .HasForeignKey("CustomerId").HasConstraintName("FK_Foo"),
                model =>
                {
                    var table = Assert.Single(model.Tables, t => t.Name == "Orders");
                    var foreignKey = table.ForeignKeys.Single();
                    Assert.Equal("FK_Foo", foreignKey.Name);
                });

        [ConditionalFact]
        public virtual Task Drop_foreign_key()
            => Test(
                builder =>
                {
                    builder.Entity(
                        "Customers", e =>
                        {
                            e.Property<int>("Id");
                            e.HasKey("Id");
                        });
                    builder.Entity(
                        "Orders", e =>
                        {
                            e.Property<int>("Id");
                            e.Property<int>("CustomerId");
                        });
                },
                builder => builder.Entity("Orders").HasOne("Customers").WithMany().HasForeignKey("CustomerId"),
                builder => { },
                model =>
                {
                    var customersTable = Assert.Single(model.Tables, t => t.Name == "Customers");
                    Assert.Empty(customersTable.ForeignKeys);
                });

        [ConditionalFact]
        public virtual Task Add_unique_constraint()
            => Test(
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<int>("AlternateKeyColumn");
                    }),
                builder => { },
                builder => builder.Entity("People").HasAlternateKey("AlternateKeyColumn"),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var uniqueConstraint = table.UniqueConstraints.Single();
                    Assert.Same(table, uniqueConstraint.Table);
                    Assert.Same(table.Columns.Single(c => c.Name == "AlternateKeyColumn"), Assert.Single(uniqueConstraint.Columns));
                    Assert.Equal("AK_People_AlternateKeyColumn", uniqueConstraint.Name);
                });

        [ConditionalFact]
        public virtual Task Add_unique_constraint_composite_with_name()
            => Test(
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<int>("AlternateKeyColumn1");
                        e.Property<int>("AlternateKeyColumn2");
                    }),
                builder => { },
                builder => builder.Entity("People").HasAlternateKey("AlternateKeyColumn1", "AlternateKeyColumn2").HasName("AK_Foo"),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var uniqueConstraint = table.UniqueConstraints.Single();
                    Assert.Same(table, uniqueConstraint.Table);
                    Assert.Collection(
                        uniqueConstraint.Columns,
                        c => Assert.Same(table.Columns.Single(c => c.Name == "AlternateKeyColumn1"), c),
                        c => Assert.Same(table.Columns.Single(c => c.Name == "AlternateKeyColumn2"), c));
                    Assert.Equal("AK_Foo", uniqueConstraint.Name);
                });

        [ConditionalFact]
        public virtual Task Drop_unique_constraint()
            => Test(
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<int>("AlternateKeyColumn");
                    }),
                builder => builder.Entity("People").HasAlternateKey("AlternateKeyColumn"),
                builder => { },
                model =>
                {
                    Assert.Empty(Assert.Single(model.Tables).UniqueConstraints);
                });

        [ConditionalFact]
        public virtual Task Add_check_constraint_with_name()
            => Test(
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<int>("DriverLicense");
                    }),
                builder => { },
                builder => builder.Entity("People").HasCheckConstraint("CK_Foo", $"{DelimitIdentifier("DriverLicense")} > 0"),
                model =>
                {
                    // TODO: no scaffolding support for check constraints, https://github.com/aspnet/EntityFrameworkCore/issues/15408
                });

        [ConditionalFact]
        public virtual Task Drop_check_constraint()
            => Test(
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<int>("DriverLicense");
                    }),
                builder => builder.Entity("People").HasCheckConstraint("CK_Foo", $"{DelimitIdentifier("DriverLicense")} > 0"),
                builder => { },
                model =>
                {
                    // TODO: no scaffolding support for check constraints, https://github.com/aspnet/EntityFrameworkCore/issues/15408
                });

        [ConditionalFact]
        public virtual Task Create_sequence()
            => Test(
                builder => { },
                builder => builder.HasSequence<int>("TestSequence"),
                model =>
                {
                    var sequence = Assert.Single(model.Sequences);
                    Assert.Equal("TestSequence", sequence.Name);
                });

        [ConditionalFact]
        public virtual Task Create_sequence_all_settings()
            => Test(
                builder => { },
                builder => builder.HasSequence<long>("TestSequence", "dbo2")
                    .StartsAt(3)
                    .IncrementsBy(2)
                    .HasMin(2)
                    .HasMax(916)
                    .IsCyclic(),
                model =>
                {
                    var sequence = Assert.Single(model.Sequences);
                    Assert.Equal("TestSequence", sequence.Name);
                    Assert.Equal("dbo2", sequence.Schema);
                    Assert.Equal(3, sequence.StartValue);
                    Assert.Equal(2, sequence.IncrementBy);
                    Assert.Equal(2, sequence.MinValue);
                    Assert.Equal(916, sequence.MaxValue);
                    Assert.True(sequence.IsCyclic);
                });

        [ConditionalFact]
        public virtual Task Alter_sequence_all_settings()
            => Test(
                builder => builder.HasSequence<int>("foo"),
                builder => { },
                builder => builder.HasSequence<int>("foo")
                    .StartsAt(-3)
                    .IncrementsBy(2)
                    .HasMin(-5)
                    .HasMax(10)
                    .IsCyclic(),
                model =>
                {
                    var sequence = Assert.Single(model.Sequences);
                    Assert.Equal(-3, sequence.StartValue);
                    Assert.Equal(2, sequence.IncrementBy);
                    Assert.Equal(-5, sequence.MinValue);
                    Assert.Equal(10, sequence.MaxValue);
                    Assert.True(sequence.IsCyclic);
                });

        [ConditionalFact]
        public virtual Task Alter_sequence_increment_by()
            => Test(
                builder => builder.HasSequence<int>("foo"),
                builder => { },
                builder => builder.HasSequence<int>("foo").IncrementsBy(2),
                model =>
                {
                    var sequence = Assert.Single(model.Sequences);
                    Assert.Equal(2, sequence.IncrementBy);
                });

        [ConditionalFact]
        public virtual Task Drop_sequence()
            => Test(
                builder => builder.HasSequence("TestSequence"),
                builder => { },
                model => Assert.Empty(model.Sequences));

        [ConditionalFact]
        public virtual Task Rename_sequence()
            => Test(
                builder => builder.HasSequence<int>("TestSequence"),
                builder => builder.HasSequence<int>("testsequence"),
                model =>
                {
                    var sequence = Assert.Single(model.Sequences);
                    Assert.Equal("testsequence", sequence.Name);
                });

        [ConditionalFact]
        public virtual Task Move_sequence()
            => Test(
                builder => builder.HasSequence<int>("TestSequence"),
                builder => builder.HasSequence<int>("TestSequence", "TestSequenceSchema"),
                model =>
                {
                    var sequence = Assert.Single(model.Sequences);
                    Assert.Equal("TestSequenceSchema", sequence.Schema);
                    Assert.Equal("TestSequence", sequence.Name);
                });

        [ConditionalFact]
        public virtual Task InsertDataOperation()
            => Test(
                builder => builder.Entity(
                    "Person", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<string>("Name");
                        e.HasKey("Id");
                    }),
                builder => { },
                builder => builder.Entity("Person")
                    .HasData(
                        new Person { Id = 1, Name = "Daenerys Targaryen" },
                        new Person { Id = 2, Name = "John Snow" },
                        new Person { Id = 3, Name = "Arya Stark" },
                        new Person { Id = 4, Name = "Harry Strickland" },
                        new Person { Id = 5, Name = null }),
                model => { });

        [ConditionalFact]
        public virtual Task DeleteDataOperation_simple_key()
            => Test(
                builder => builder.Entity(
                    "Person", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<string>("Name");
                        e.HasKey("Id");
                        e.HasData(new Person { Id = 1, Name = "Daenerys Targaryen" });
                    }),
                builder => builder.Entity("Person").HasData(new Person { Id = 2, Name = "John Snow" }),
                builder => { },
                model => { });

        [ConditionalFact]
        public virtual Task DeleteDataOperation_composite_key()
            => Test(
                builder => builder.Entity(
                    "Person", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<int>("AnotherId");
                        e.HasKey("Id", "AnotherId");
                        e.Property<string>("Name");
                        e.HasData(
                            new Person
                            {
                                Id = 1,
                                AnotherId = 11,
                                Name = "Daenerys Targaryen"
                            });
                    }),
                builder => builder.Entity("Person").HasData(
                    new Person
                    {
                        Id = 2,
                        AnotherId = 12,
                        Name = "John Snow"
                    }),
                builder => { },
                model => { });

        [ConditionalFact]
        public virtual Task UpdateDataOperation_simple_key()
            => Test(
                builder => builder.Entity(
                    "Person", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<string>("Name");
                        e.HasKey("Id");
                        e.HasData(new Person { Id = 1, Name = "Daenerys Targaryen" });
                    }),
                builder => builder.Entity("Person").HasData(new Person { Id = 2, Name = "John Snow" }),
                builder => builder.Entity("Person").HasData(new Person { Id = 2, Name = "Another John Snow" }),
                model => { });

        [ConditionalFact]
        public virtual Task UpdateDataOperation_composite_key()
            => Test(
                builder => builder.Entity(
                    "Person", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<int>("AnotherId");
                        e.HasKey("Id", "AnotherId");
                        e.Property<string>("Name");
                        e.HasData(
                            new Person
                            {
                                Id = 1,
                                AnotherId = 11,
                                Name = "Daenerys Targaryen"
                            });
                    }),
                builder => builder.Entity("Person").HasData(
                    new Person
                    {
                        Id = 2,
                        AnotherId = 11,
                        Name = "John Snow"
                    }),
                builder => builder.Entity("Person").HasData(
                    new Person
                    {
                        Id = 2,
                        AnotherId = 11,
                        Name = "Another John Snow"
                    }),
                model => { });

        [ConditionalFact]
        public virtual Task UpdateDataOperation_multiple_columns()
            => Test(
                builder => builder.Entity(
                    "Person", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<string>("Name");
                        e.Property<int>("Age");
                        e.HasKey("Id");
                        e.HasData(
                            new Person
                            {
                                Id = 1,
                                Name = "Daenerys Targaryen",
                                Age = 18
                            });
                    }),
                builder => builder.Entity("Person").HasData(
                    new Person
                    {
                        Id = 2,
                        Name = "John Snow",
                        Age = 20
                    }),
                builder => builder.Entity("Person").HasData(
                    new Person
                    {
                        Id = 2,
                        Name = "Another John Snow",
                        Age = 21
                    }),
                model => { });

        [ConditionalFact]
        public virtual async Task SqlOperation()
        {
            await Test(
                builder => { },
                new SqlOperation { Sql = "-- I <3 DDL" },
                model =>
                {
                    Assert.Empty(model.Tables);
                    Assert.Empty(model.Sequences);
                });

            AssertSql(
                @"-- I <3 DDL");
        }

        private class Person
        {
            public int Id { get; set; }
            public int AnotherId { get; set; }
            public string? Name { get; set; }
            public int Age { get; set; }
        }

        protected virtual DbContext CreateContext() => Fixture.CreateContext();

        protected virtual string DelimitIdentifier(string unquotedIdentifier)
            => _sqlGenerationHelper?.DelimitIdentifier(unquotedIdentifier)
                ?? throw new InvalidOperationException(
                    $"No ISqlGenerationHelper singleton was found, consider overriding {nameof(DelimitIdentifier)}");

        protected virtual IRelationalTypeMappingSource TypeMappingSource
            => _typeMappingSource
                ?? throw new InvalidOperationException(
                    $"No IRelationalTypeMappingSource singleton was found, consider overriding {nameof(TypeMappingSource)}");

        protected virtual Task Test(
            Action<ModelBuilder> buildSourceAction,
            Action<ModelBuilder> buildTargetAction,
            Action<DatabaseModel>? asserter)
            => Test(b => { }, buildSourceAction, buildTargetAction, asserter);

        protected virtual Task Test(
            Action<ModelBuilder> buildCommonAction,
            Action<ModelBuilder> buildSourceAction,
            Action<ModelBuilder> buildTargetAction,
            Action<DatabaseModel>? asserter)
        {
            // Build the source and target models. Add current/latest product version if one wasn't set.
            var sourceModelBuilder = CreateConventionlessModelBuilder();
            buildCommonAction(sourceModelBuilder);
            buildSourceAction(sourceModelBuilder);
            var sourceModel = sourceModelBuilder.FinalizeModel();

            var targetModelBuilder = CreateConventionlessModelBuilder();
            buildCommonAction(targetModelBuilder);
            buildTargetAction(targetModelBuilder);
            var targetModel = targetModelBuilder.FinalizeModel();

            var context = CreateContext();
            var serviceProvider = ((IInfrastructure<IServiceProvider>)context).Instance;
            var modelDiffer = serviceProvider.GetRequiredService<IMigrationsModelDiffer>();

            var operations = modelDiffer.GetDifferences(sourceModel, targetModel);

            return Test(sourceModel, targetModel, operations, asserter);
        }

        protected virtual Task Test(
            Action<ModelBuilder> buildSourceAction,
            MigrationOperation operation,
            Action<DatabaseModel>? asserter)
            => Test(buildSourceAction, new[] { operation }, asserter);

        protected virtual Task Test(
            Action<ModelBuilder> buildSourceAction,
            IReadOnlyList<MigrationOperation> operations,
            Action<DatabaseModel>? asserter)
        {
            var sourceModelBuilder = CreateConventionlessModelBuilder();
            buildSourceAction(sourceModelBuilder);
            if (sourceModelBuilder.Model.GetProductVersion() is null)
            {
                sourceModelBuilder.Model.SetProductVersion(ProductInfo.GetVersion());
            }

            var sourceModel = sourceModelBuilder.FinalizeModel();

            return Test(sourceModel, targetModel: null, operations, asserter);
        }

        protected virtual async Task Test(
            IModel sourceModel,
            IModel? targetModel,
            IReadOnlyList<MigrationOperation> operations,
            Action<DatabaseModel>? asserter)
        {
            var context = CreateContext();
            var serviceProvider = ((IInfrastructure<IServiceProvider>)context).Instance;
            var migrationsSqlGenerator = serviceProvider.GetRequiredService<IMigrationsSqlGenerator>();
            var modelDiffer = serviceProvider.GetRequiredService<IMigrationsModelDiffer>();
            var migrationsCommandExecutor = serviceProvider.GetRequiredService<IMigrationCommandExecutor>();
            var connection = serviceProvider.GetRequiredService<IRelationalConnection>();
            var databaseModelFactory = serviceProvider.GetRequiredService<IDatabaseModelFactory>();

            try
            {
                // Apply migrations to get to the source state, and do a scaffolding snapshot for later comparison.
                // Suspending event recording, we're not interested in the SQL of this part
                using (Fixture.TestSqlLoggerFactory.SuspendRecordingEvents())
                {
                    await migrationsCommandExecutor.ExecuteNonQueryAsync(
                        migrationsSqlGenerator.Generate(modelDiffer.GetDifferences(null, sourceModel), sourceModel),
                        connection);
                }

                // Apply migrations to get from source to target, then reverse-engineer and execute the
                // test-provided assertions on the resulting database model
                await migrationsCommandExecutor.ExecuteNonQueryAsync(
                    migrationsSqlGenerator.Generate(operations, targetModel), connection);

                var scaffoldedModel = databaseModelFactory.Create(
                    context.Database.GetDbConnection(),
                    new DatabaseModelFactoryOptions());

                asserter?.Invoke(scaffoldedModel);
            }
            finally
            {
                using var _ = Fixture.TestSqlLoggerFactory.SuspendRecordingEvents();
                Fixture.TestStore.Clean(context);
            }
        }

        protected virtual Task<T> TestThrows<T>(
            Action<ModelBuilder> buildSourceAction,
            Action<ModelBuilder> buildTargetAction)
            where T : Exception
            => TestThrows<T>(b => { }, buildSourceAction, buildTargetAction);

        protected virtual Task<T> TestThrows<T>(
            Action<ModelBuilder> buildCommonAction,
            Action<ModelBuilder> buildSourceAction,
            Action<ModelBuilder> buildTargetAction)
            where T : Exception
            => Assert.ThrowsAsync<T>(() => Test(buildCommonAction, buildSourceAction, buildTargetAction, asserter: null));

        protected virtual void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        public abstract class MigrationsFixtureBase : SharedStoreFixtureBase<PoolableDbContext>
        {
            public abstract TestHelpers TestHelpers { get; }
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;
        }

        protected virtual ModelBuilder CreateConventionlessModelBuilder(bool sensitiveDataLoggingEnabled = false)
        {
            var conventionSet = new ConventionSet();

            var dependencies = Fixture.TestHelpers.CreateContextServices().GetRequiredService<ProviderConventionSetBuilderDependencies>();
            conventionSet.ModelFinalizingConventions.Add(new TypeMappingConvention(dependencies));

            return new ModelBuilder(conventionSet);
        }
    }
}

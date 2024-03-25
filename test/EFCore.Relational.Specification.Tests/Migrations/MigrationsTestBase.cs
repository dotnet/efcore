// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

namespace Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

public abstract class MigrationsTestBase<TFixture> : IClassFixture<TFixture>
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
                    e.ToTable(
                        "People", "dbo2", tb =>
                        {
                            tb.HasCheckConstraint("CK_People_EmployerId", $"{DelimitIdentifier("EmployerId")} > 0");
                            tb.HasComment("Table comment");
                        });

                    e.Property<int>("CustomId");
                    e.Property<int>("EmployerId")
                        .HasComment("Employer ID comment");
                    e.Property<string>("SSN")
                        .HasColumnType(char11StoreType)
                        .UseCollation(NonDefaultCollation)
                        .IsRequired(false);

                    e.HasKey("CustomId");
                    e.HasAlternateKey("SSN");
                    e.HasOne("Employers").WithMany("People").HasForeignKey("EmployerId");
                }),
            model =>
            {
                var employersTable = Assert.Single(model.Tables, t => t.Name == "Employers");
                var peopleTable = Assert.Single(model.Tables, t => t.Name == "People");

                Assert.Equal("People", peopleTable.Name);
                if (AssertSchemaNames)
                {
                    Assert.Equal("dbo2", peopleTable.Schema);
                }

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
                        if (AssertComments)
                        {
                            Assert.Equal("Employer ID comment", c.Comment);
                        }
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

                if (AssertComments)
                {
                    Assert.Equal("Table comment", peopleTable.Comment);
                }
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
                    e.ToTable(tb => tb.HasComment("Table comment"));
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var column = Assert.Single(table.Columns, c => c.Name == "Name");
                if (AssertComments)
                {
                    Assert.Equal("Table comment", table.Comment);
                    Assert.Equal("Column comment", column.Comment);
                }
            });

    [ConditionalFact]
    public virtual Task Create_table_with_multiline_comments()
    {
        var tableComment = "This is a multi-line\r\ntable comment.\r\nMore information can\r\nbe found in the docs.";
        var columnComment = "This is a multi-line\ncolumn comment.\nMore information can\nbe found in the docs.";

        return Test(
            builder => { },
            builder => builder.Entity(
                "People", e =>
                {
                    e.Property<int>("Id");
                    e.Property<string>("Name").HasComment(columnComment);
                    e.ToTable(tb => tb.HasComment(tableComment));
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var column = Assert.Single(table.Columns, c => c.Name == "Name");
                if (AssertComments)
                {
                    Assert.Equal(tableComment, table.Comment);
                    Assert.Equal(columnComment, column.Comment);
                }
            });
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    [InlineData(null)]
    public virtual Task Create_table_with_computed_column(bool? stored)
        => Test(
            builder => { },
            builder => builder.Entity(
                "People", e =>
                {
                    e.Property<int>("Id");
                    e.Property<int>("X");
                    e.Property<int>("Y");
                    e.Property<string>("Sum").HasComputedColumnSql(
                        $"{DelimitIdentifier("X")} + {DelimitIdentifier("Y")}",
                        stored);
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var sumColumn = Assert.Single(table.Columns, c => c.Name == "Sum");
                if (AssertComputedColumns)
                {
                    Assert.Contains("X", sumColumn.ComputedColumnSql);
                    Assert.Contains("Y", sumColumn.ComputedColumnSql);
                    if (stored != null)
                    {
                        Assert.Equal(stored, sumColumn.IsStored);
                    }
                }
            });

    [ConditionalFact]
    public virtual async Task Create_table_with_json_column()
        => await Test(
            builder => { },
            builder =>
            {
                builder.Entity(
                    "Entity", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.HasKey("Id");
                        e.Property<string>("Name");
                        e.OwnsOne(
                            "Owned", "OwnedReference", o =>
                            {
                                o.OwnsOne(
                                    "Nested", "NestedReference", n =>
                                    {
                                        n.Property<int>("Number");
                                    });
                                o.OwnsMany(
                                    "Nested2", "NestedCollection", n =>
                                    {
                                        n.Property<int>("Number2");
                                    });
                                o.Property<DateTime>("Date");
                                o.ToJson();
                            });

                        e.OwnsMany(
                            "Owned2", "OwnedCollection", o =>
                            {
                                o.OwnsOne(
                                    "Nested3", "NestedReference2", n =>
                                    {
                                        n.Property<int>("Number3");
                                    });
                                o.OwnsMany(
                                    "Nested4", "NestedCollection2", n =>
                                    {
                                        n.Property<int>("Number4");
                                    });
                                o.Property<DateTime>("Date2");
                                o.ToJson();
                            });

                        e.OwnsOne(
                            "Owned", "OwnedRequiredReference", o =>
                            {
                                o.Property<DateTime>("Date");
                                o.ToJson();
                            });

                        e.Navigation("OwnedRequiredReference").IsRequired();
                    });
            },
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Entity", table.Name);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name),
                    c => Assert.Equal("OwnedCollection", c.Name),
                    c =>
                    {
                        Assert.Equal("OwnedReference", c.Name);
                        Assert.True(c.IsNullable);
                    },
                    c =>
                    {
                        Assert.Equal("OwnedRequiredReference", c.Name);
                        Assert.False(c.IsNullable);
                    });
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

    [ConditionalFact]
    public virtual async Task Create_table_with_json_column_explicit_json_column_names()
        => await Test(
            builder => { },
            builder =>
            {
                builder.Entity(
                    "Entity", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.HasKey("Id");
                        e.Property<string>("Name");
                        e.OwnsOne(
                            "Owned", "json_reference", o =>
                            {
                                o.OwnsOne("Nested", "json_reference", n => n.Property<int>("Number"));
                                o.OwnsMany("Nested2", "NestedCollection", n => n.Property<int>("Number2"));
                                o.Property<DateTime>("Date");
                                o.ToJson();
                            });

                        e.OwnsMany(
                            "Owned2", "json_collection", o =>
                            {
                                o.OwnsOne("Nested3", "NestedReference2", n => n.Property<int>("Number3"));
                                o.OwnsMany("Nested4", "NestedCollection2", n => n.Property<int>("Number4"));
                                o.Property<DateTime>("Date2");
                                o.ToJson();
                            });
                    });
            },
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Entity", table.Name);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name),
                    c => Assert.Equal("json_collection", c.Name),
                    c => Assert.Equal("json_reference", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

    [ConditionalFact]
    public virtual Task Alter_table_add_comment()
        => Test(
            builder => builder.Entity("People").Property<int>("Id"),
            builder => { },
            builder => builder.Entity("People").ToTable(tb => tb.HasComment("Table comment")),
            model =>
            {
                var table = Assert.Single(model.Tables);
                if (AssertComments)
                {
                    Assert.Equal("Table comment", table.Comment);
                }
            });

    [ConditionalFact]
    public virtual Task Alter_table_add_comment_non_default_schema()
        => Test(
            builder => builder.Entity("People")
                .ToTable("People", "SomeOtherSchema")
                .Property<int>("Id"),
            builder => { },
            builder => builder.Entity("People")
                .ToTable("People", "SomeOtherSchema", tb => tb.HasComment("Table comment")),
            model =>
            {
                var table = Assert.Single(model.Tables);
                if (AssertComments)
                {
                    Assert.Equal("Table comment", table.Comment);
                }
            });

    [ConditionalFact]
    public virtual Task Alter_table_change_comment()
        => Test(
            builder => builder.Entity("People").Property<int>("Id"),
            builder => builder.Entity("People").ToTable(tb => tb.HasComment("Table comment1")),
            builder => builder.Entity("People").ToTable(tb => tb.HasComment("Table comment2")),
            model =>
            {
                var table = Assert.Single(model.Tables);
                if (AssertComments)
                {
                    Assert.Equal("Table comment2", table.Comment);
                }
            });

    [ConditionalFact]
    public virtual Task Alter_table_remove_comment()
        => Test(
            builder => builder.Entity("People").Property<int>("Id"),
            builder => builder.Entity("People").ToTable(tb => tb.HasComment("Table comment1")),
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
            builder => builder.Entity("People").ToTable("Persons").Property<int>("Id"),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Persons", table.Name);
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
                "People", e =>
                {
                    e.ToTable("Persons");
                    e.Property<int>("Id");
                    e.HasKey("Id");
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Persons", table.Name);
            });

    [ConditionalFact]
    public virtual async Task Rename_table_with_json_column()
        => await Test(
            builder =>
            {
                builder.Entity(
                    "Entity", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.HasKey("Id");
                        e.Property<string>("Name");
                        e.ToTable("Entities");

                        e.OwnsOne(
                            "Owned", "OwnedReference", o =>
                            {
                                o.OwnsOne(
                                    "Nested", "NestedReference", n =>
                                    {
                                        n.Property<int>("Number");
                                    });
                                o.OwnsMany(
                                    "Nested2", "NestedCollection", n =>
                                    {
                                        n.Property<int>("Number2");
                                    });
                                o.Property<DateTime>("Date");
                                o.ToJson();
                            });

                        e.OwnsMany(
                            "Owned2", "OwnedCollection", o =>
                            {
                                o.OwnsOne(
                                    "Nested3", "NestedReference2", n =>
                                    {
                                        n.Property<int>("Number3");
                                    });
                                o.OwnsMany(
                                    "Nested4", "NestedCollection2", n =>
                                    {
                                        n.Property<int>("Number4");
                                    });
                                o.Property<DateTime>("Date2");
                                o.ToJson();
                            });
                    });
            },
            builder =>
            {
                builder.Entity(
                    "Entity", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.HasKey("Id");
                        e.Property<string>("Name");
                        e.ToTable("NewEntities");

                        e.OwnsOne(
                            "Owned", "OwnedReference", o =>
                            {
                                o.OwnsOne("Nested", "NestedReference", n => n.Property<int>("Number"));
                                o.OwnsMany("Nested2", "NestedCollection", n => n.Property<int>("Number2"));
                                o.Property<DateTime>("Date");
                                o.ToJson();
                            });

                        e.OwnsMany(
                            "Owned2", "OwnedCollection", o =>
                            {
                                o.OwnsOne("Nested3", "NestedReference2", n => n.Property<int>("Number3"));
                                o.OwnsMany("Nested4", "NestedCollection2", n => n.Property<int>("Number4"));
                                o.Property<DateTime>("Date2");
                                o.ToJson();
                            });
                    });
            },
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("NewEntities", table.Name);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name),
                    c => Assert.Equal("OwnedCollection", c.Name),
                    c => Assert.Equal("OwnedReference", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
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
                if (AssertSchemaNames)
                {
                    Assert.Equal("TestTableSchema", table.Schema);
                }

                Assert.Equal("TestTable", table.Name);
            });

    [ConditionalFact]
    public virtual Task Create_schema()
        => Test(
            builder => { },
            builder => builder.Entity("People")
                .ToTable("People", "SomeOtherSchema")
                .Property<int>("Id"),
            model =>
            {
                var table = Assert.Single(model.Tables);
                if (AssertSchemaNames)
                {
                    Assert.Equal("SomeOtherSchema", table.Schema);
                }
            });

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
    public virtual async Task Add_column_with_defaultValueSql_unspecified()
    {
        var ex = await TestThrows<InvalidOperationException>(
            builder => builder.Entity("People").Property<int>("Id"),
            builder => { },
            builder => builder.Entity("People").Property<int?>("Sum")
                .HasDefaultValueSql());
        Assert.Equal(RelationalStrings.DefaultValueSqlUnspecified("People", "Sum"), ex.Message);
    }

    [ConditionalFact]
    public virtual async Task Add_column_with_defaultValue_unspecified()
    {
        var ex = await TestThrows<InvalidOperationException>(
            builder => builder.Entity("People").Property<int>("Id"),
            builder => { },
            builder => builder.Entity("People").Property<int?>("Sum")
                .HasDefaultValue());
        Assert.Equal(RelationalStrings.DefaultValueUnspecified("People", "Sum"), ex.Message);
    }

    protected class Owner
    {
        public int Id { get; set; }
        public Owned Owned { get; set; }
    }

    protected class Owned
    {
        public int Foo { get; set; }
    }

    [ConditionalFact]
    public virtual async Task Add_json_columns_to_existing_table()
        => await Test(
            builder => builder.Entity(
                "Entity", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                }),
            builder =>
            {
                builder.Entity(
                    "Entity", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.HasKey("Id");
                        e.Property<string>("Name");

                        e.OwnsOne(
                            "Owned", "OwnedReference", o =>
                            {
                                o.OwnsOne(
                                    "Nested", "NestedReference", n =>
                                    {
                                        n.Property<int>("Number");
                                    });
                                o.OwnsMany(
                                    "Nested2", "NestedCollection", n =>
                                    {
                                        n.Property<int>("Number2");
                                    });
                                o.Property<DateTime>("Date");
                                o.ToJson();
                            });

                        e.OwnsOne(
                            "Owned", "OwnedRequiredReference", o =>
                            {
                                o.Property<DateTime>("Date");
                                o.ToJson();
                            });

                        e.Navigation("OwnedRequiredReference").IsRequired();

                        e.OwnsMany(
                            "Owned2", "OwnedCollection", o =>
                            {
                                o.OwnsOne(
                                    "Nested3", "NestedReference2", n =>
                                    {
                                        n.Property<int>("Number3");
                                    });
                                o.OwnsMany(
                                    "Nested4", "NestedCollection2", n =>
                                    {
                                        n.Property<int>("Number4");
                                    });
                                o.Property<DateTime>("Date2");
                                o.ToJson();
                            });
                    });
            },
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Entity", table.Name);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name),
                    c => Assert.Equal("OwnedCollection", c.Name),
                    c =>
                    {
                        Assert.Equal("OwnedReference", c.Name);
                        Assert.True(c.IsNullable);
                    },
                    c =>
                    {
                        Assert.Equal("OwnedRequiredReference", c.Name);
                        Assert.False(c.IsNullable);
                    });
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    [InlineData(null)]
    public virtual Task Add_column_with_computedSql(bool? stored)
        => Test(
            builder => builder.Entity(
                "People", e =>
                {
                    e.Property<int>("Id");
                    e.Property<int>("X");
                    e.Property<int>("Y");
                }),
            builder => { },
            builder => builder.Entity("People").Property<string>("Sum")
                .HasComputedColumnSql($"{DelimitIdentifier("X")} + {DelimitIdentifier("Y")}", stored),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var sumColumn = Assert.Single(table.Columns, c => c.Name == "Sum");
                if (AssertComputedColumns)
                {
                    Assert.Contains("X", sumColumn.ComputedColumnSql);
                    Assert.Contains("Y", sumColumn.ComputedColumnSql);
                    if (stored != null)
                    {
                        Assert.Equal(stored, sumColumn.IsStored);
                    }
                }
            });

    [ConditionalFact]
    public virtual async Task Add_column_with_computedSql_unspecified()
    {
        var ex = await TestThrows<InvalidOperationException>(
            builder => builder.Entity("People").Property<int>("Id"),
            builder => { },
            builder => builder.Entity("People").Property<int?>("Sum")
                .HasComputedColumnSql());
        Assert.Equal(RelationalStrings.ComputedColumnSqlUnspecified("Sum", "People"), ex.Message);
    }

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
    public virtual Task Add_column_with_unbounded_max_length()
        => Test(
            builder => builder.Entity("People").Property<int>("Id"),
            builder => { },
            builder => builder.Entity("People").Property<string>("Name").HasMaxLength(-1),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var column = Assert.Single(table.Columns, c => c.Name == "Name");
                Assert.Equal(
                    TypeMappingSource
                        .FindMapping(typeof(string), storeTypeName: null, size: -1)
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
                if (AssertComments)
                {
                    Assert.Equal("My comment", column.Comment);
                }
            });

    [ConditionalFact]
    public virtual Task Add_column_with_collation()
        => Test(
            builder => builder.Entity("People").Property<int>("Id"),
            builder => { },
            builder => builder.Entity("People").Property<string>("Name")
                .UseCollation(NonDefaultCollation),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal(2, table.Columns.Count);
                var nameColumn = Assert.Single(table.Columns, c => c.Name == "Name");
                if (AssertCollations)
                {
                    Assert.Equal(NonDefaultCollation, nameColumn.Collation);
                }
            });

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public virtual Task Add_column_computed_with_collation(bool stored)
        => Test(
            builder => builder.Entity("People").Property<int>("Id"),
            builder => { },
            builder => builder.Entity("People").Property<string>("Name")
                .HasComputedColumnSql("'hello'", stored)
                .UseCollation(NonDefaultCollation),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal(2, table.Columns.Count);
                var nameColumn = Assert.Single(table.Columns, c => c.Name == "Name");
                if (AssertComputedColumns)
                {
                    Assert.Contains("hello", nameColumn.ComputedColumnSql);
                    Assert.Equal(stored, nameColumn.IsStored);
                }

                if (AssertCollations)
                {
                    Assert.Equal(NonDefaultCollation, nameColumn.Collation);
                }
            });

    [ConditionalFact]
    public virtual Task Add_column_shared()
        => Test(
            builder =>
            {
                builder.Entity("Base").Property<int>("Id");
                builder.Entity("Derived1").HasBaseType("Base").Property<string>("Foo");
                builder.Entity("Derived2").HasBaseType("Base").Property<string>("Foo");
            },
            builder => { },
            builder => builder.Entity("Base").Property<string>("Foo"),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var column = Assert.Single(table.Columns, c => c.Name == "Foo");
            });

    [ConditionalFact]
    public virtual Task Add_column_with_check_constraint()
        => Test(
            builder => builder.Entity("People").Property<int>("Id"),
            builder => { },
            builder => builder.Entity(
                "People", e =>
                {
                    e.Property<int>("DriverLicense");
                    e.ToTable(tb => tb.HasCheckConstraint("CK_People_Foo", $"{DelimitIdentifier("DriverLicense")} > 0"));
                }),
            model =>
            {
                // TODO: no scaffolding support for check constraints, https://github.com/aspnet/EntityFrameworkCore/issues/15408
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
    public virtual Task Alter_column_make_required_with_null_data()
        => Test(
            builder => builder.Entity(
                "People", e =>
                {
                    e.Property<int>("Id");
                    e.Property<string>("SomeColumn");
                    e.HasData(new Dictionary<string, object> { { "Id", 1 }, { "SomeColumn", null } });
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

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    [InlineData(null)]
    public virtual Task Alter_column_make_computed(bool? stored)
        => Test(
            builder => builder.Entity(
                "People", e =>
                {
                    e.Property<int>("Id");
                    e.Property<int>("X");
                    e.Property<int>("Y");
                }),
            builder => builder.Entity("People").Property<int>("Sum"),
            builder => builder.Entity("People").Property<int>("Sum")
                .HasComputedColumnSql($"{DelimitIdentifier("X")} + {DelimitIdentifier("Y")}", stored),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var sumColumn = Assert.Single(table.Columns, c => c.Name == "Sum");
                if (AssertComputedColumns)
                {
                    Assert.Contains("X", sumColumn.ComputedColumnSql);
                    Assert.Contains("Y", sumColumn.ComputedColumnSql);
                    Assert.Contains("+", sumColumn.ComputedColumnSql);
                    if (stored != null)
                    {
                        Assert.Equal(stored, sumColumn.IsStored);
                    }
                }
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
            builder => builder.Entity("People").Property<int>("Sum")
                .HasComputedColumnSql($"{DelimitIdentifier("X")} + {DelimitIdentifier("Y")}"),
            builder => builder.Entity("People").Property<int>("Sum")
                .HasComputedColumnSql($"{DelimitIdentifier("X")} - {DelimitIdentifier("Y")}"),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var sumColumn = Assert.Single(table.Columns, c => c.Name == "Sum");
                if (AssertComputedColumns)
                {
                    Assert.Contains("X", sumColumn.ComputedColumnSql);
                    Assert.Contains("Y", sumColumn.ComputedColumnSql);
                    Assert.Contains("-", sumColumn.ComputedColumnSql);
                }
            });

    [ConditionalFact]
    public virtual Task Alter_column_change_computed_recreates_indexes()
        => Test(
            builder => builder.Entity(
                "People", e =>
                {
                    e.Property<int>("Id");
                    e.Property<int>("X");
                    e.Property<int>("Y");
                    e.Property<int>("Sum");

                    e.HasIndex("Sum");
                }),
            builder => builder.Entity("People").Property<int>("Sum")
                .HasComputedColumnSql($"{DelimitIdentifier("X")} + {DelimitIdentifier("Y")}"),
            builder => builder.Entity("People").Property<int>("Sum")
                .HasComputedColumnSql($"{DelimitIdentifier("X")} - {DelimitIdentifier("Y")}"),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var sumColumn = Assert.Single(table.Columns, c => c.Name == "Sum");
                if (AssertComputedColumns)
                {
                    Assert.Contains("X", sumColumn.ComputedColumnSql);
                    Assert.Contains("Y", sumColumn.ComputedColumnSql);
                    Assert.Contains("-", sumColumn.ComputedColumnSql);
                }

                var sumIndex = Assert.Single(table.Indexes);
                Assert.Collection(sumIndex.Columns, c => Assert.Equal("Sum", c.Name));
            });

    [ConditionalFact]
    public virtual Task Alter_column_change_computed_type()
        => Test(
            builder => builder.Entity(
                "People", e =>
                {
                    e.Property<int>("Id");
                    e.Property<int>("X");
                    e.Property<int>("Y");
                    e.Property<int>("Sum");
                }),
            builder => builder.Entity("People").Property<int>("Sum")
                .HasComputedColumnSql($"{DelimitIdentifier("X")} + {DelimitIdentifier("Y")}", stored: false),
            builder => builder.Entity("People").Property<int>("Sum")
                .HasComputedColumnSql($"{DelimitIdentifier("X")} + {DelimitIdentifier("Y")}", stored: true),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var sumColumn = Assert.Single(table.Columns, c => c.Name == "Sum");
                if (AssertComputedColumns)
                {
                    Assert.True(sumColumn.IsStored);
                }
            });

    [ConditionalFact]
    public virtual Task Alter_column_make_non_computed()
        => Test(
            builder => builder.Entity(
                "People", e =>
                {
                    e.Property<int>("Id");
                    e.Property<int>("X");
                    e.Property<int>("Y");
                }),
            builder => builder.Entity("People").Property<int>("Sum")
                .HasComputedColumnSql($"{DelimitIdentifier("X")} + {DelimitIdentifier("Y")}"),
            builder => builder.Entity("People").Property<int>("Sum"),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var sumColumn = Assert.Single(table.Columns, c => c.Name == "Sum");
                Assert.Null(sumColumn.ComputedColumnSql);
                Assert.NotEqual(true, sumColumn.IsStored);
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
                if (AssertComments)
                {
                    Assert.Equal("Some comment", column.Comment);
                }
            });

    [ConditionalFact]
    public virtual Task Alter_computed_column_add_comment()
        => Test(
            builder => builder.Entity(
                "People", x =>
                {
                    x.Property<int>("Id");
                    x.Property<int>("SomeColumn").HasComputedColumnSql("42");
                }),
            builder => { },
            builder => builder.Entity("People").Property<int>("SomeColumn").HasComment("Some comment"),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var column = Assert.Single(table.Columns.Where(c => c.Name == "SomeColumn"));
                if (AssertComments)
                {
                    Assert.Equal("Some comment", column.Comment);
                }
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
                if (AssertComments)
                {
                    Assert.Equal("Some comment2", column.Comment);
                }
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
    public virtual Task Alter_column_set_collation()
        => Test(
            builder => builder.Entity("People").Property<string>("Name"),
            builder => { },
            builder => builder.Entity("People").Property<string>("Name")
                .UseCollation(NonDefaultCollation),
            model =>
            {
                var nameColumn = Assert.Single(Assert.Single(model.Tables).Columns);
                if (AssertCollations)
                {
                    Assert.Equal(NonDefaultCollation, nameColumn.Collation);
                }
            });

    [ConditionalFact]
    public virtual Task Alter_column_reset_collation()
        => Test(
            builder => builder.Entity("People").Property<string>("Name"),
            builder => builder.Entity("People").Property<string>("Name")
                .UseCollation(NonDefaultCollation),
            builder => { },
            model =>
            {
                var nameColumn = Assert.Single(Assert.Single(model.Tables).Columns);
                Assert.Null(nameColumn.Collation);
            });

    [ConditionalFact]
    public virtual async Task Convert_json_entities_to_regular_owned()
        => await Test(
            builder =>
            {
                builder.Entity(
                    "Entity", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.HasKey("Id");
                        e.Property<string>("Name");

                        e.OwnsOne(
                            "Owned", "OwnedReference", o =>
                            {
                                o.OwnsOne(
                                    "Nested", "NestedReference", n =>
                                    {
                                        n.Property<int>("Number");
                                    });
                                o.OwnsMany(
                                    "Nested2", "NestedCollection", n =>
                                    {
                                        n.Property<int>("Number2");
                                    });
                                o.Property<DateTime>("Date");
                                o.ToJson();
                            });

                        e.OwnsMany(
                            "Owned2", "OwnedCollection", o =>
                            {
                                o.OwnsOne(
                                    "Nested3", "NestedReference2", n =>
                                    {
                                        n.Property<int>("Number3");
                                    });
                                o.OwnsMany(
                                    "Nested4", "NestedCollection2", n =>
                                    {
                                        n.Property<int>("Number4");
                                    });
                                o.Property<DateTime>("Date2");
                                o.ToJson();
                            });
                    });
            },
            builder =>
            {
                builder.Entity(
                    "Entity", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.HasKey("Id");
                        e.Property<string>("Name");

                        e.OwnsOne(
                            "Owned", "OwnedReference", o =>
                            {
                                o.OwnsOne(
                                    "Nested", "NestedReference", n =>
                                    {
                                        n.Property<int>("Number");
                                    });
                                o.OwnsMany(
                                    "Nested2", "NestedCollection", n =>
                                    {
                                        n.Property<int>("Number2");
                                    });
                                o.Property<DateTime>("Date");
                            });

                        e.OwnsMany(
                            "Owned2", "OwnedCollection", o =>
                            {
                                o.OwnsOne(
                                    "Nested3", "NestedReference2", n =>
                                    {
                                        n.Property<int>("Number3");
                                    });
                                o.OwnsMany(
                                    "Nested4", "NestedCollection2", n =>
                                    {
                                        n.Property<int>("Number4");
                                    });
                                o.Property<DateTime>("Date2");
                            });
                    });
            },
            model =>
            {
                Assert.Equal(4, model.Tables.Count());
            });

    [ConditionalFact]
    public virtual async Task Convert_regular_owned_entities_to_json()
        => await Test(
            builder =>
            {
                builder.Entity(
                    "Entity", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.HasKey("Id");
                        e.Property<string>("Name");

                        e.OwnsOne(
                            "Owned", "OwnedReference", o =>
                            {
                                o.OwnsOne(
                                    "Nested", "NestedReference", n =>
                                    {
                                        n.Property<int>("Number");
                                    });
                                o.OwnsMany(
                                    "Nested2", "NestedCollection", n =>
                                    {
                                        n.Property<int>("Number2");
                                    });
                                o.Property<DateTime>("Date");
                            });

                        e.OwnsMany(
                            "Owned2", "OwnedCollection", o =>
                            {
                                o.OwnsOne(
                                    "Nested3", "NestedReference2", n =>
                                    {
                                        n.Property<int>("Number3");
                                    });
                                o.OwnsMany(
                                    "Nested4", "NestedCollection2", n =>
                                    {
                                        n.Property<int>("Number4");
                                    });
                                o.Property<DateTime>("Date2");
                            });
                    });
            },
            builder =>
            {
                builder.Entity(
                    "Entity", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.HasKey("Id");
                        e.Property<string>("Name");

                        e.OwnsOne(
                            "Owned", "OwnedReference", o =>
                            {
                                o.OwnsOne("Nested", "NestedReference", n => n.Property<int>("Number"));
                                o.OwnsMany("Nested2", "NestedCollection", n => n.Property<int>("Number2"));
                                o.Property<DateTime>("Date");
                                o.ToJson();
                            });

                        e.OwnsMany(
                            "Owned2", "OwnedCollection", o =>
                            {
                                o.OwnsOne("Nested3", "NestedReference2", n => n.Property<int>("Number3"));
                                o.OwnsMany("Nested4", "NestedCollection2", n => n.Property<int>("Number4"));
                                o.Property<DateTime>("Date2");
                                o.ToJson();
                            });
                    });
            },
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Entity", table.Name);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name),
                    c => Assert.Equal("OwnedCollection", c.Name),
                    c => Assert.Equal("OwnedReference", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

    [ConditionalFact]
    public virtual async Task Convert_string_column_to_a_json_column_containing_reference()
    {
        await Test(
            builder =>
            {
                builder.Entity(
                    "Entity", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.HasKey("Id");
                        e.Property<string>("Name");
                    });
            },
            builder =>
            {
                builder.Entity(
                    "Entity", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.HasKey("Id");

                        e.OwnsOne(
                            "Owned", "OwnedReference", o =>
                            {
                                o.ToJson("Name");
                                o.OwnsOne("Nested", "NestedReference", n => n.Property<int>("Number"));
                                o.OwnsMany("Nested2", "NestedCollection", n => n.Property<int>("Number2"));
                                o.Property<DateTime>("Date");
                            });
                    });
            },
            model =>
            {
                var table = model.Tables.Single();
                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
            });

        AssertSql();
    }

    [ConditionalFact]
    public virtual async Task Convert_string_column_to_a_json_column_containing_required_reference()
        => await Test(
            builder =>
            {
                builder.Entity(
                    "Entity", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.HasKey("Id");
                        e.Property<string>("Name");
                    });
            },
            builder =>
            {
                builder.Entity(
                    "Entity", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.HasKey("Id");

                        e.OwnsOne(
                            "Owned", "OwnedReference", o =>
                            {
                                o.ToJson("Name");
                                o.OwnsOne("Nested", "NestedReference", n => n.Property<int>("Number"));
                                o.OwnsMany("Nested2", "NestedCollection", n => n.Property<int>("Number2"));
                                o.Property<DateTime>("Date");
                            });

                        e.Navigation("OwnedReference").IsRequired();
                    });
            },
            model =>
            {
                var table = model.Tables.Single();
                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
            });

    [ConditionalFact]
    public virtual async Task Convert_string_column_to_a_json_column_containing_collection()
    {
        await Test(
            builder =>
            {
                builder.Entity(
                    "Entity", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.HasKey("Id");
                        e.Property<string>("Name");
                    });
            },
            builder =>
            {
                builder.Entity(
                    "Entity", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.HasKey("Id");

                        e.OwnsMany(
                            "Owned2", "OwnedCollection", o =>
                            {
                                o.OwnsOne("Nested3", "NestedReference2", n => n.Property<int>("Number3"));
                                o.OwnsMany("Nested4", "NestedCollection2", n => n.Property<int>("Number4"));
                                o.Property<DateTime>("Date2");
                                o.ToJson("Name");
                            });
                    });
            },
            model =>
            {
                var table = model.Tables.Single();
                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
            });

        AssertSql();
    }

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
    public virtual Task Drop_column_computed_and_non_computed_with_dependency()
        => Test(
            builder => builder.Entity("People").Property<int>("Id"),
            builder => builder.Entity(
                "People", e =>
                {
                    e.Property<int>("X");
                    e.Property<int>("Y").HasComputedColumnSql($"{DelimitIdentifier("X")} + 1");
                }),
            builder => { },
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Id", Assert.Single(table.Columns).Name);
            });

    [ConditionalFact]
    public virtual async Task Drop_json_columns_from_existing_table()
        => await Test(
            builder =>
            {
                builder.Entity(
                    "Entity", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.HasKey("Id");
                        e.Property<string>("Name");
                        e.OwnsOne(
                            "Owned", "OwnedReference", o =>
                            {
                                o.OwnsOne(
                                    "Nested", "NestedReference", n =>
                                    {
                                        n.Property<int>("Number");
                                    });
                                o.OwnsMany(
                                    "Nested2", "NestedCollection", n =>
                                    {
                                        n.Property<int>("Number2");
                                    });
                                o.Property<DateTime>("Date");
                                o.ToJson();
                            });

                        e.OwnsMany(
                            "Owned2", "OwnedCollection", o =>
                            {
                                o.OwnsOne(
                                    "Nested3", "NestedReference2", n =>
                                    {
                                        n.Property<int>("Number3");
                                    });
                                o.OwnsMany(
                                    "Nested4", "NestedCollection2", n =>
                                    {
                                        n.Property<int>("Number4");
                                    });
                                o.Property<DateTime>("Date2");
                                o.ToJson();
                            });
                    });
            },
            builder => builder.Entity(
                "Entity", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Entity", table.Name);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
            });

    [ConditionalFact]
    public virtual Task Rename_column()
        => Test(
            builder => builder.Entity("People").Property<int>("Id"),
            builder => builder.Entity("People").Property<string>("SomeColumn"),
            builder => builder.Entity("People").Property<string>("SomeColumn").HasColumnName("SomeOtherColumn"),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal(2, table.Columns.Count);
                Assert.Single(table.Columns, c => c.Name == "SomeOtherColumn");
            });

    [ConditionalFact]
    public virtual async Task Rename_json_column()
        => await Test(
            builder =>
            {
                builder.Entity(
                    "Entity", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.HasKey("Id");
                        e.Property<string>("Name");

                        e.OwnsOne(
                            "Owned", "OwnedReference", o =>
                            {
                                o.OwnsOne(
                                    "Nested", "NestedReference", n =>
                                    {
                                        n.Property<int>("Number");
                                    });
                                o.OwnsMany(
                                    "Nested2", "NestedCollection", n =>
                                    {
                                        n.Property<int>("Number2");
                                    });
                                o.Property<DateTime>("Date");
                                o.ToJson("json_reference");
                            });

                        e.OwnsMany(
                            "Owned2", "OwnedCollection", o =>
                            {
                                o.OwnsOne(
                                    "Nested3", "NestedReference2", n =>
                                    {
                                        n.Property<int>("Number3");
                                    });
                                o.OwnsMany(
                                    "Nested4", "NestedCollection2", n =>
                                    {
                                        n.Property<int>("Number4");
                                    });
                                o.Property<DateTime>("Date2");
                                o.ToJson("json_collection");
                            });
                    });
            },
            builder =>
            {
                builder.Entity(
                    "Entity", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.HasKey("Id");
                        e.Property<string>("Name");

                        e.OwnsOne(
                            "Owned", "OwnedReference", o =>
                            {
                                o.OwnsOne("Nested", "NestedReference", n => n.Property<int>("Number"));
                                o.OwnsMany("Nested2", "NestedCollection", n => n.Property<int>("Number2"));
                                o.Property<DateTime>("Date");
                                o.ToJson("new_json_reference");
                            });

                        e.OwnsMany(
                            "Owned2", "OwnedCollection", o =>
                            {
                                o.OwnsOne("Nested3", "NestedReference2", n => n.Property<int>("Number3"));
                                o.OwnsMany("Nested4", "NestedCollection2", n => n.Property<int>("Number4"));
                                o.Property<DateTime>("Date2");
                                o.ToJson("new_json_collection");
                            });
                    });
            },
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Entity", table.Name);

                Assert.Collection(
                    table.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name),
                    c => Assert.Equal("new_json_collection", c.Name),
                    c => Assert.Equal("new_json_reference", c.Name));
                Assert.Same(
                    table.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(table.PrimaryKey!.Columns));
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

                if (index.IsDescending.Count > 0)
                {
                    Assert.Collection(index.IsDescending, descending => Assert.False(descending));
                }

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
    public virtual Task Create_index_descending()
        => Test(
            builder => builder.Entity(
                "People", e =>
                {
                    e.Property<int>("Id");
                    e.Property<int>("X");
                }),
            builder => { },
            builder => builder.Entity("People").HasIndex("X").IsDescending(),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var index = Assert.Single(table.Indexes);
                Assert.Collection(index.IsDescending, Assert.True);
            });

    [ConditionalFact]
    public virtual Task Create_index_descending_mixed()
        => Test(
            builder => builder.Entity(
                "People", e =>
                {
                    e.Property<int>("Id");
                    e.Property<int>("X");
                    e.Property<int>("Y");
                    e.Property<int>("Z");
                }),
            builder => { },
            builder => builder.Entity("People")
                .HasIndex("X", "Y", "Z")
                .IsDescending(false, true, false),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var index = Assert.Single(table.Indexes);
                Assert.Collection(index.IsDescending, Assert.False, Assert.True, Assert.False);
            });

    [ConditionalFact]
    public virtual Task Alter_index_make_unique()
        => Test(
            builder => builder.Entity(
                "People", e =>
                {
                    e.Property<int>("Id");
                    e.Property<int>("X");
                }),
            builder => builder.Entity("People").HasIndex("X"),
            builder => builder.Entity("People").HasIndex("X").IsUnique(),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var index = Assert.Single(table.Indexes);
                Assert.True(index.IsUnique);
            });

    [ConditionalFact]
    public virtual Task Alter_index_change_sort_order()
        => Test(
            builder => builder.Entity(
                "People", e =>
                {
                    e.Property<int>("Id");
                    e.Property<int>("X");
                    e.Property<int>("Y");
                    e.Property<int>("Z");
                }),
            builder => builder.Entity("People")
                .HasIndex("X", "Y", "Z")
                .IsDescending(true, false, true),
            builder => builder.Entity("People")
                .HasIndex("X", "Y", "Z")
                .IsDescending(false, true, false),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var index = Assert.Single(table.Indexes);
                Assert.Collection(index.IsDescending, Assert.False, Assert.True, Assert.False);
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
                if (AssertIndexFilters)
                {
                    Assert.Contains("Name", index.Filter);
                }
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
            model =>
            {
                var index = model.Tables.Single().Indexes.Single();
                Assert.True(index.IsUnique);
                if (AssertIndexFilters)
                {
                    Assert.Contains("Name", index.Filter);
                }
            });

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
            builder => builder.Entity("People").HasIndex(["FirstName"], "Foo"),
            builder => builder.Entity("People").HasIndex(["FirstName"], "foo"),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var index = Assert.Single(table.Indexes);
                Assert.Equal("foo", index.Name);
            });

    [ConditionalFact]
    public virtual Task Add_primary_key_int()
        => Test(
            builder => builder.Entity("People").Property<int>("SomeField"),
            builder => { },
            builder => builder.Entity("People").HasKey("SomeField"),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var primaryKey = table.PrimaryKey;
                Assert.NotNull(primaryKey);
                Assert.Same(table, primaryKey!.Table);
                Assert.Same(table.Columns.Single(), Assert.Single(primaryKey.Columns));
                if (AssertConstraintNames)
                {
                    Assert.Equal("PK_People", primaryKey.Name);
                }
            });

    [ConditionalFact]
    public virtual Task Add_primary_key_string()
        => Test(
            builder => builder.Entity("People").Property<string>("SomeField").IsRequired(),
            builder => { },
            builder => builder.Entity("People").HasKey("SomeField"),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var primaryKey = table.PrimaryKey;
                Assert.NotNull(primaryKey);
                Assert.Same(table, primaryKey!.Table);
                Assert.Same(table.Columns.Single(), Assert.Single(primaryKey.Columns));
                if (AssertConstraintNames)
                {
                    Assert.Equal("PK_People", primaryKey.Name);
                }
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
                if (AssertConstraintNames)
                {
                    Assert.Equal("PK_Foo", primaryKey.Name);
                }
            });

    [ConditionalFact]
    public virtual Task Add_primary_key_composite_with_name()
        => Test(
            builder => builder.Entity(
                "People", e =>
                {
                    e.Property<int>("SomeField1");
                    e.Property<int>("SomeField2");
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
                if (AssertConstraintNames)
                {
                    Assert.Equal("PK_Foo", primaryKey.Name);
                }
            });

    [ConditionalFact]
    public virtual Task Drop_primary_key_int()
        => Test(
            builder => builder.Entity("People").Property<int>("SomeField"),
            builder => builder.Entity("People").HasKey("SomeField"),
            builder => { },
            model => Assert.Null(Assert.Single(model.Tables).PrimaryKey));

    [ConditionalFact]
    public virtual Task Drop_primary_key_string()
        => Test(
            builder => builder.Entity("People").Property<string>("SomeField").IsRequired(),
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
                if (AssertConstraintNames)
                {
                    Assert.Equal("FK_Orders_Customers_CustomerId", foreignKey.Name);
                }

                Assert.Equal(ReferentialAction.Cascade, foreignKey.OnDelete);
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
                if (AssertConstraintNames)
                {
                    Assert.Equal("FK_Foo", foreignKey.Name);
                }
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
                if (AssertConstraintNames)
                {
                    Assert.Equal("AK_People_AlternateKeyColumn", uniqueConstraint.Name);
                }
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
                if (AssertConstraintNames)
                {
                    Assert.Equal("AK_Foo", uniqueConstraint.Name);
                }
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
            builder => builder.Entity("People")
                .ToTable(tb => tb.HasCheckConstraint("CK_People_Foo", $"{DelimitIdentifier("DriverLicense")} > 0")),
            model =>
            {
                // TODO: no scaffolding support for check constraints, https://github.com/aspnet/EntityFrameworkCore/issues/15408
            });

    [ConditionalFact]
    public virtual Task Alter_check_constraint()
        => Test(
            builder => builder.Entity(
                "People", e =>
                {
                    e.Property<int>("Id");
                    e.Property<int>("DriverLicense");
                }),
            builder => builder.Entity("People")
                .ToTable(tb => tb.HasCheckConstraint("CK_People_Foo", $"{DelimitIdentifier("DriverLicense")} > 0")),
            builder => builder.Entity("People")
                .ToTable(tb => tb.HasCheckConstraint("CK_People_Foo", $"{DelimitIdentifier("DriverLicense")} > 1")),
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
            builder => builder.Entity("People")
                .ToTable(tb => tb.HasCheckConstraint("CK_People_Foo", $"{DelimitIdentifier("DriverLicense")} > 0")),
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
    public virtual Task Create_sequence_long()
        => Test(
            builder => { },
            builder => builder.HasSequence<long>("TestSequence"),
            model =>
            {
                var sequence = Assert.Single(model.Sequences);
                Assert.Equal("TestSequence", sequence.Name);
            });

    [ConditionalFact]
    public virtual Task Create_sequence_short()
        => Test(
            builder => { },
            builder => builder.HasSequence<short>("TestSequence"),
            model =>
            {
                var sequence = Assert.Single(model.Sequences);
                Assert.Equal("TestSequence", sequence.Name);
            });


    [ConditionalFact]
    public virtual Task Create_sequence_nocache()
        => Test(
            builder => { },
            builder => builder.HasSequence("Alpha").UseNoCache(),
            model =>
            {
                var sequence = Assert.Single(model.Sequences);
                Assert.Equal("Alpha", sequence.Name);
                Assert.False(sequence.IsCached);
            });


    [ConditionalFact]
    public virtual Task Create_sequence_cache()
        => Test(
            builder => { },
            builder => builder.HasSequence("Beta").UseCache(20),
            model =>
            {
                var sequence = Assert.Single(model.Sequences);
                Assert.Equal("Beta", sequence.Name);
                Assert.True(sequence.IsCached);
                Assert.Equal(20, sequence.CacheSize);
            });

    [ConditionalFact]
    public virtual Task Create_sequence_default_cache()
        => Test(
            builder => { },
            builder => builder.HasSequence("Gamma").UseCache(),
            model =>
            {
                var sequence = Assert.Single(model.Sequences);
                Assert.Equal("Gamma", sequence.Name);
                Assert.True(sequence.IsCached);
                Assert.Null(sequence.CacheSize);
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
                .IsCyclic()
                .UseCache(20),
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
                Assert.True(sequence.IsCached);
                Assert.Equal(20, sequence.CacheSize);
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
                .IsCyclic()
                .UseCache(20),
            model =>
            {
                var sequence = Assert.Single(model.Sequences);
                Assert.Equal(-3, sequence.StartValue);
                Assert.Equal(2, sequence.IncrementBy);
                Assert.Equal(-5, sequence.MinValue);
                Assert.Equal(10, sequence.MaxValue);
                Assert.True(sequence.IsCyclic);
                Assert.True(sequence.IsCached);
                Assert.Equal(20, sequence.CacheSize);
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
    public virtual Task Alter_sequence_default_cache_to_cache()
        => Test(
            builder => builder.HasSequence<int>("Delta").UseCache(),
            builder => { },
            builder => builder.HasSequence<int>("Delta").UseCache(20),
            model =>
            {
                var sequence = Assert.Single(model.Sequences);
                Assert.True(sequence.IsCached);
                Assert.Equal(20, sequence.CacheSize);
            });


    [ConditionalFact]
    public virtual Task Alter_sequence_default_cache_to_nocache()
        => Test(
            builder => builder.HasSequence<int>("Epsilon").UseCache(),
            builder => { },
            builder => builder.HasSequence<int>("Epsilon").UseNoCache(),
            model =>
            {
                var sequence = Assert.Single(model.Sequences);
                Assert.False(sequence.IsCached);
                Assert.Null(sequence.CacheSize);
            });

    [ConditionalFact]
    public virtual Task Alter_sequence_cache_to_nocache()
        => Test(
            builder => builder.HasSequence<int>("Zeta").UseCache(20),
            builder => { },
            builder => builder.HasSequence<int>("Zeta").UseNoCache(),
            model =>
            {
                var sequence = Assert.Single(model.Sequences);
                Assert.False(sequence.IsCached);
                Assert.Null(sequence.CacheSize);
            });

    [ConditionalFact]
    public virtual Task Alter_sequence_cache_to_default_cache()
        => Test(
            builder => builder.HasSequence<int>("Eta").UseCache(20),
            builder => { },
            builder => builder.HasSequence<int>("Eta").UseCache(),
            model =>
            {
                var sequence = Assert.Single(model.Sequences);
                Assert.True(sequence.IsCached);
                Assert.Null(sequence.CacheSize);
            });

    [ConditionalFact]
    public virtual Task Alter_sequence_nocache_to_cache()
        => Test(
            builder => builder.HasSequence<int>("Theta").UseNoCache(),
            builder => { },
            builder => builder.HasSequence<int>("Theta").UseCache(20),
            model =>
            {
                var sequence = Assert.Single(model.Sequences);
                Assert.True(sequence.IsCached);
                Assert.Equal(20, sequence.CacheSize);
            });

    [ConditionalFact]
    public virtual Task Alter_sequence_nocache_to_default_cache()
        => Test(
            builder => builder.HasSequence<int>("Iota").UseNoCache(),
            builder => { },
            builder => builder.HasSequence<int>("Iota").UseCache(),
            model =>
            {
                var sequence = Assert.Single(model.Sequences);
                Assert.True(sequence.IsCached);
                Assert.Null(sequence.CacheSize);
            });

    [ConditionalFact]
    public virtual Task Alter_sequence_restart_with()
        => Test(
            builder => builder.HasSequence<int>("foo"),
            builder => { },
            builder => builder.HasSequence<int>("foo").StartsAt(3),
            model =>
            {
                var sequence = Assert.Single(model.Sequences);
                Assert.Equal(3, sequence.StartValue);
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
            """
-- I <3 DDL
""");
    }

    [ConditionalFact]
    public virtual Task Create_table_with_complex_type_with_required_properties_on_derived_entity_in_TPH()
        => Test(
            builder => { },
            builder =>
            {
                builder.Entity(
                    "Contact", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.HasKey("Id");
                        e.Property<string>("Name");
                        e.ToTable("Contacts");
                    });

                builder.Entity(
                    "Supplier", e =>
                    {
                        e.HasBaseType("Contact");
                        e.Property<int>("Number");
                        e.ComplexProperty<MyComplex>(
                            "MyComplex", ct =>
                            {
                                ct.ComplexProperty<MyNestedComplex>("MyNestedComplex").IsRequired();
                            });
                    });
            },
            model =>
            {
                var contactsTable = Assert.Single(model.Tables.Where(t => t.Name == "Contacts"));
                Assert.Collection(
                    contactsTable.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Discriminator", c.Name),
                    c => Assert.Equal("Name", c.Name),
                    c => Assert.Equal("Number", c.Name),
                    c =>
                    {
                        Assert.Equal("MyComplex_Prop", c.Name);
                        Assert.Equal(true, c.IsNullable);
                    },
                    c =>
                    {
                        Assert.Equal("MyComplex_MyNestedComplex_Bar", c.Name);
                        Assert.Equal(true, c.IsNullable);
                    },
                    c =>
                    {
                        Assert.Equal("MyComplex_MyNestedComplex_Foo", c.Name);
                        Assert.Equal(true, c.IsNullable);
                    });
            });

    protected class MyComplex
    {
        [Required]
        public string Prop { get; set; }

        [Required]
        public MyNestedComplex Nested { get; set; }
    }

    public class MyNestedComplex
    {
        public int Foo { get; set; }
        public DateTime Bar { get; set; }
    }

    [ConditionalFact]
    public virtual Task Add_required_primitive_collection_to_existing_table()
        => Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.ToTable("Customers");
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.Property<List<int>>("Numbers").IsRequired();
                    e.ToTable("Customers");
                }),
            model =>
            {
                var customersTable = Assert.Single(model.Tables.Where(t => t.Name == "Customers"));
                Assert.Collection(
                    customersTable.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name),
                    c => Assert.Equal("Numbers", c.Name));
                Assert.Same(
                    customersTable.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(customersTable.PrimaryKey!.Columns));
            });

    [ConditionalFact]
    public virtual Task Add_required_primitive_collection_with_custom_default_value_to_existing_table()
        => Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.ToTable("Customers");
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.Property<List<int>>("Numbers").IsRequired().HasDefaultValue(
                        new List<int>
                        {
                            1,
                            2,
                            3
                        });
                    e.ToTable("Customers");
                }),
            model =>
            {
                var customersTable = Assert.Single(model.Tables.Where(t => t.Name == "Customers"));
                Assert.Collection(
                    customersTable.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name),
                    c => Assert.Equal("Numbers", c.Name));
                Assert.Same(
                    customersTable.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(customersTable.PrimaryKey!.Columns));
            });

    [ConditionalFact]
    public abstract Task Add_required_primitive_collection_with_custom_default_value_sql_to_existing_table();

    protected virtual Task Add_required_primitive_collection_with_custom_default_value_sql_to_existing_table_core(string defaultValueSql)
        => Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.ToTable("Customers");
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.Property<List<int>>("Numbers").IsRequired().HasDefaultValueSql(defaultValueSql);
                    e.ToTable("Customers");
                }),
            model =>
            {
                var customersTable = Assert.Single(model.Tables.Where(t => t.Name == "Customers"));
                Assert.Collection(
                    customersTable.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name),
                    c => Assert.Equal("Numbers", c.Name));
                Assert.Same(
                    customersTable.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(customersTable.PrimaryKey!.Columns));
            });

    [ConditionalFact(Skip = "issue #33038")]
    public virtual Task Add_required_primitive_collection_with_custom_converter_to_existing_table()
        => Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.ToTable("Customers");
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.Property<List<int>>("Numbers").HasConversion(
                            new ValueConverter<List<int>, string>(
                                convertToProviderExpression: x => x != null && x.Count > 0 ? "some numbers" : "nothing",
                                convertFromProviderExpression: x => x == "nothing"
                                    ? new List<int> { }
                                    : new List<int>
                                    {
                                        7,
                                        8,
                                        9
                                    }))
                        .IsRequired();
                    e.ToTable("Customers");
                }),
            model =>
            {
                var customersTable = Assert.Single(model.Tables.Where(t => t.Name == "Customers"));
                Assert.Collection(
                    customersTable.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name),
                    c => Assert.Equal("Numbers", c.Name));
                Assert.Same(
                    customersTable.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(customersTable.PrimaryKey!.Columns));
            });

    [ConditionalFact]
    public virtual Task Add_required_primitive_collection_with_custom_converter_and_custom_default_value_to_existing_table()
        => Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.ToTable("Customers");
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.Property<List<int>>("Numbers").HasConversion(
                            new ValueConverter<List<int>, string>(
                                convertToProviderExpression: x => x != null && x.Count > 0 ? "some numbers" : "nothing",
                                convertFromProviderExpression: x => x == "nothing"
                                    ? new List<int> { }
                                    : new List<int>
                                    {
                                        7,
                                        8,
                                        9
                                    }))
                        .HasDefaultValue(new List<int> { 42 })
                        .IsRequired();
                    e.ToTable("Customers");
                }),
            model =>
            {
                var customersTable = Assert.Single(model.Tables.Where(t => t.Name == "Customers"));
                Assert.Collection(
                    customersTable.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name),
                    c => Assert.Equal("Numbers", c.Name));
                Assert.Same(
                    customersTable.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(customersTable.PrimaryKey!.Columns));
            });

    [ConditionalFact]
    public virtual Task Add_optional_primitive_collection_to_existing_table()
        => Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.ToTable("Customers");
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.Property<List<int>>("Numbers");
                    e.ToTable("Customers");
                }),
            model =>
            {
                var customersTable = Assert.Single(model.Tables.Where(t => t.Name == "Customers"));
                Assert.Collection(
                    customersTable.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name),
                    c => Assert.Equal("Numbers", c.Name));
                Assert.Same(
                    customersTable.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(customersTable.PrimaryKey!.Columns));
            });

    [ConditionalFact]
    public virtual Task Create_table_with_required_primitive_collection()
        => Test(
            builder => { },
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.Property<List<int>>("Numbers").IsRequired();
                    e.ToTable("Customers");
                }),
            model =>
            {
                var customersTable = Assert.Single(model.Tables.Where(t => t.Name == "Customers"));
                Assert.Collection(
                    customersTable.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name),
                    c => Assert.Equal("Numbers", c.Name));
                Assert.Same(
                    customersTable.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(customersTable.PrimaryKey!.Columns));
            });

    [ConditionalFact]
    public virtual Task Create_table_with_optional_primitive_collection()
        => Test(
            builder => { },
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.Property<List<int>>("Numbers");
                    e.ToTable("Customers");
                }),
            model =>
            {
                var customersTable = Assert.Single(model.Tables.Where(t => t.Name == "Customers"));
                Assert.Collection(
                    customersTable.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name),
                    c => Assert.Equal("Numbers", c.Name));
                Assert.Same(
                    customersTable.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(customersTable.PrimaryKey!.Columns));
            });

    [ConditionalFact]
    public virtual Task Add_required_primitve_collection_to_existing_table()
        => Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.ToTable("Customers");
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.Property<List<int>>("Numbers").IsRequired();
                    e.ToTable("Customers");
                }),
            model =>
            {
                var customersTable = Assert.Single(model.Tables.Where(t => t.Name == "Customers"));
                Assert.Collection(
                    customersTable.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name),
                    c => Assert.Equal("Numbers", c.Name));
                Assert.Same(
                    customersTable.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(customersTable.PrimaryKey!.Columns));
            });

    [ConditionalFact]
    public virtual Task Add_required_primitve_collection_with_custom_default_value_to_existing_table()
        => Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.ToTable("Customers");
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.Property<List<int>>("Numbers").IsRequired().HasDefaultValue(
                        new List<int>
                        {
                            1,
                            2,
                            3
                        });
                    e.ToTable("Customers");
                }),
            model =>
            {
                var customersTable = Assert.Single(model.Tables.Where(t => t.Name == "Customers"));
                Assert.Collection(
                    customersTable.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name),
                    c => Assert.Equal("Numbers", c.Name));
                Assert.Same(
                    customersTable.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(customersTable.PrimaryKey!.Columns));
            });

    [ConditionalFact]
    public abstract Task Add_required_primitve_collection_with_custom_default_value_sql_to_existing_table();

    protected virtual Task Add_required_primitve_collection_with_custom_default_value_sql_to_existing_table_core(string defaultValueSql)
        => Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.ToTable("Customers");
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.Property<List<int>>("Numbers").IsRequired().HasDefaultValueSql(defaultValueSql);
                    e.ToTable("Customers");
                }),
            model =>
            {
                var customersTable = Assert.Single(model.Tables.Where(t => t.Name == "Customers"));
                Assert.Collection(
                    customersTable.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name),
                    c => Assert.Equal("Numbers", c.Name));
                Assert.Same(
                    customersTable.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(customersTable.PrimaryKey!.Columns));
            });

    [ConditionalFact(Skip = "issue #33038")]
    public virtual Task Add_required_primitve_collection_with_custom_converter_to_existing_table()
        => Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.ToTable("Customers");
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.Property<List<int>>("Numbers").HasConversion(
                            new ValueConverter<List<int>, string>(
                                convertToProviderExpression: x => x != null && x.Count > 0 ? "some numbers" : "nothing",
                                convertFromProviderExpression: x => x == "nothing"
                                    ? new List<int> { }
                                    : new List<int>
                                    {
                                        7,
                                        8,
                                        9
                                    }))
                        .IsRequired();
                    e.ToTable("Customers");
                }),
            model =>
            {
                var customersTable = Assert.Single(model.Tables.Where(t => t.Name == "Customers"));
                Assert.Collection(
                    customersTable.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name),
                    c => Assert.Equal("Numbers", c.Name));
                Assert.Same(
                    customersTable.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(customersTable.PrimaryKey!.Columns));
            });

    [ConditionalFact]
    public virtual Task Add_required_primitve_collection_with_custom_converter_and_custom_default_value_to_existing_table()
        => Test(
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.ToTable("Customers");
                }),
            builder => builder.Entity(
                "Customer", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.Property<List<int>>("Numbers").HasConversion(
                            new ValueConverter<List<int>, string>(
                                convertToProviderExpression: x => x != null && x.Count > 0 ? "some numbers" : "nothing",
                                convertFromProviderExpression: x => x == "nothing"
                                    ? new List<int> { }
                                    : new List<int>
                                    {
                                        7,
                                        8,
                                        9
                                    }))
                        .HasDefaultValue(new List<int> { 42 })
                        .IsRequired();
                    e.ToTable("Customers");
                }),
            model =>
            {
                var customersTable = Assert.Single(model.Tables.Where(t => t.Name == "Customers"));
                Assert.Collection(
                    customersTable.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Name", c.Name),
                    c => Assert.Equal("Numbers", c.Name));
                Assert.Same(
                    customersTable.Columns.Single(c => c.Name == "Id"),
                    Assert.Single(customersTable.PrimaryKey!.Columns));
            });

    protected class Person
    {
        public int Id { get; set; }
        public int AnotherId { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
    }

    protected virtual bool AssertSchemaNames
        => true;

    protected virtual bool AssertComments
        => true;

    protected virtual bool AssertComputedColumns
        => true;

    protected virtual bool AssertCollations
        => true;

    protected virtual bool AssertIndexFilters
        => true;

    protected virtual bool AssertConstraintNames
        => true;

    protected abstract string NonDefaultCollation { get; }

    protected virtual DbContext CreateContext()
        => Fixture.CreateContext();

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
        Action<DatabaseModel> asserter,
        bool withConventions = true,
        MigrationsSqlGenerationOptions migrationsSqlGenerationOptions = MigrationsSqlGenerationOptions.Default)
        => Test(_ => { }, buildSourceAction, buildTargetAction, asserter, withConventions, migrationsSqlGenerationOptions);

    protected virtual Task Test(
        Action<ModelBuilder> buildCommonAction,
        Action<ModelBuilder> buildSourceAction,
        Action<ModelBuilder> buildTargetAction,
        Action<DatabaseModel> asserter,
        bool withConventions = true,
        MigrationsSqlGenerationOptions migrationsSqlGenerationOptions = MigrationsSqlGenerationOptions.Default)
    {
        var context = CreateContext();
        var modelDiffer = context.GetService<IMigrationsModelDiffer>();
        var modelRuntimeInitializer = context.GetService<IModelRuntimeInitializer>();

        // Build the source model, possibly with conventions
        var sourceModelBuilder = CreateModelBuilder(withConventions);
        buildCommonAction(sourceModelBuilder);
        buildSourceAction(sourceModelBuilder);
        var preSnapshotSourceModel = modelRuntimeInitializer.Initialize(
            (IModel)sourceModelBuilder.Model, designTime: true, validationLogger: null);

        // Round-trip the source model through a snapshot, compiling it and then extracting it back again.
        // This simulates the real-world migration flow and can expose errors in snapshot generation
        var migrationsCodeGenerator = Fixture.TestHelpers.CreateDesignServiceProvider().GetRequiredService<IMigrationsCodeGenerator>();
        var sourceModelSnapshot = migrationsCodeGenerator.GenerateSnapshot(
            modelSnapshotNamespace: null, typeof(DbContext), "MigrationsTestSnapshot", preSnapshotSourceModel);
        var sourceModel = BuildModelFromSnapshotSource(sourceModelSnapshot);

        // Build the target model, possibly with conventions
        var targetModelBuilder = CreateModelBuilder(withConventions);
        buildCommonAction(targetModelBuilder);
        buildTargetAction(targetModelBuilder);
        var targetModel = modelRuntimeInitializer.Initialize(
            (IModel)targetModelBuilder.Model, designTime: true, validationLogger: null);

        // Get the migration operations between the two models and test
        var operations = modelDiffer.GetDifferences(sourceModel.GetRelationalModel(), targetModel.GetRelationalModel());

        return Test(sourceModel, targetModel, operations, asserter, migrationsSqlGenerationOptions);
    }

    protected virtual Task Test(
        Action<ModelBuilder> buildSourceAction,
        MigrationOperation operation,
        Action<DatabaseModel> asserter,
        bool withConventions = true,
        MigrationsSqlGenerationOptions migrationsSqlGenerationOptions = MigrationsSqlGenerationOptions.Default)
        => Test(buildSourceAction, new[] { operation }, asserter, withConventions, migrationsSqlGenerationOptions);

    protected virtual Task Test(
        Action<ModelBuilder> buildSourceAction,
        IReadOnlyList<MigrationOperation> operations,
        Action<DatabaseModel> asserter,
        bool withConventions = true,
        MigrationsSqlGenerationOptions migrationsSqlGenerationOptions = MigrationsSqlGenerationOptions.Default)
    {
        var sourceModelBuilder = CreateModelBuilder(withConventions);
        buildSourceAction(sourceModelBuilder);
        if (sourceModelBuilder.Model.GetProductVersion() is null)
        {
            sourceModelBuilder.Model.SetProductVersion(ProductInfo.GetVersion());
        }

        var context = CreateContext();
        var modelRuntimeInitializer = context.GetService<IModelRuntimeInitializer>();
        var preSnapshotSourceModel = modelRuntimeInitializer.Initialize(
            (IModel)sourceModelBuilder.Model, designTime: true, validationLogger: null);

        // Round-trip the source model through a snapshot, compiling it and then extracting it back again.
        // This simulates the real-world migration flow and can expose errors in snapshot generation
        var migrationsCodeGenerator = Fixture.TestHelpers.CreateDesignServiceProvider().GetRequiredService<IMigrationsCodeGenerator>();
        var sourceModelSnapshot = migrationsCodeGenerator.GenerateSnapshot(
            modelSnapshotNamespace: null, typeof(DbContext), "MigrationsTestSnapshot", preSnapshotSourceModel);
        var sourceModel = BuildModelFromSnapshotSource(sourceModelSnapshot);

        return Test(sourceModel, targetModel: null, operations, asserter, migrationsSqlGenerationOptions);
    }

    protected virtual async Task Test(
        IModel sourceModel,
        IModel targetModel,
        IReadOnlyList<MigrationOperation> operations,
        Action<DatabaseModel> asserter,
        MigrationsSqlGenerationOptions migrationsSqlGenerationOptions = MigrationsSqlGenerationOptions.Default)
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
                    migrationsSqlGenerator.Generate(
                        modelDiffer.GetDifferences(null, sourceModel.GetRelationalModel()),
                        sourceModel,
                        migrationsSqlGenerationOptions),
                    connection);
            }

            // Apply migrations to get from source to target, then reverse-engineer and execute the
            // test-provided assertions on the resulting database model
            await migrationsCommandExecutor.ExecuteNonQueryAsync(
                migrationsSqlGenerator.Generate(operations, targetModel, migrationsSqlGenerationOptions), connection);

            var scaffoldedModel = databaseModelFactory.Create(
                context.Database.GetDbConnection(),
                new DatabaseModelFactoryOptions());

            asserter?.Invoke(scaffoldedModel);
        }
        finally
        {
            using var _ = Fixture.TestSqlLoggerFactory.SuspendRecordingEvents();
            await Fixture.TestStore.CleanAsync(context);
        }
    }

    protected virtual Task<T> TestThrows<T>(
        Action<ModelBuilder> buildSourceAction,
        Action<ModelBuilder> buildTargetAction,
        bool withConventions = true)
        where T : Exception
        => TestThrows<T>(b => { }, buildSourceAction, buildTargetAction, withConventions);

    protected virtual Task<T> TestThrows<T>(
        Action<ModelBuilder> buildCommonAction,
        Action<ModelBuilder> buildSourceAction,
        Action<ModelBuilder> buildTargetAction,
        bool withConventions = true)
        where T : Exception
        => Assert.ThrowsAsync<T>(() => Test(buildCommonAction, buildSourceAction, buildTargetAction, asserter: null, withConventions));

    protected virtual void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public abstract class MigrationsFixtureBase : SharedStoreFixtureBase<PoolableDbContext>
    {
        public abstract RelationalTestHelpers TestHelpers { get; }

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;
    }

    private ModelBuilder CreateModelBuilder(bool withConventions)
        => withConventions ? Fixture.TestHelpers.CreateConventionBuilder() : new ModelBuilder(new ConventionSet());

    protected IModel BuildModelFromSnapshotSource(string code)
    {
        var build = new BuildSource { Sources = { { "Snapshot.cs", code } } };

        // Add standard EF references, a reference to the provider's assembly, and any extra references added by the provider's test suite
        build.References.Add(BuildReference.ByName("Microsoft.EntityFrameworkCore"));
        build.References.Add(BuildReference.ByName("Microsoft.EntityFrameworkCore.Abstractions"));
        build.References.Add(BuildReference.ByName("Microsoft.EntityFrameworkCore.Relational"));

        var databaseProvider = Fixture.TestHelpers.CreateContextServices().GetRequiredService<IDatabaseProvider>();
        build.References.Add(BuildReference.ByName(databaseProvider.Name));

        foreach (var buildReference in GetAdditionalReferences())
        {
            build.References.Add(buildReference);
        }

        var assembly = build.BuildInMemory();
        var factoryType = assembly.GetType("MigrationsTestSnapshot");

        var buildModelMethod = factoryType.GetMethod(
            "BuildModel",
            BindingFlags.Instance | BindingFlags.NonPublic,
            null,
            [typeof(ModelBuilder)],
            null);

        var builder = new ModelBuilder();
        builder.Model.RemoveAnnotation(CoreAnnotationNames.ProductVersion);

        buildModelMethod.Invoke(
            Activator.CreateInstance(factoryType),
            [builder]);

        var services = Fixture.TestHelpers.CreateContextServices();
        var processor = new SnapshotModelProcessor(new TestOperationReporter(), services.GetService<IModelRuntimeInitializer>());
        return processor.Process(builder.Model);
    }

    protected virtual ICollection<BuildReference> GetAdditionalReferences()
        => [];
}

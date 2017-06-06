// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Migrations.Internal
{
    public class MigrationsModelDifferTest : MigrationsModelDifferTestBase
    {
        [Fact]
        public void Model_differ_breaks_foreign_key_cycles_in_create_table_operations()
        {
            Execute(
                _ => { },
                modelBuilder =>
                    {
                        modelBuilder.Entity(
                            "First",
                            x =>
                                {
                                    x.Property<int>("ID");
                                    x.HasKey("ID");
                                    x.Property<int>("FK");
                                });

                        modelBuilder.Entity(
                            "Second",
                            x =>
                                {
                                    x.Property<int>("ID");
                                    x.HasKey("ID");
                                    x.Property<int>("FK");
                                });

                        modelBuilder.Entity("First").HasOne("Second").WithMany().HasForeignKey("FK").HasPrincipalKey("ID");
                        modelBuilder.Entity("Second").HasOne("First").WithMany().HasForeignKey("FK").HasPrincipalKey("ID");
                    },
                result =>
                    {
                        Assert.Equal(5, result.Count);

                        var createFirstTableOperation = Assert.IsType<CreateTableOperation>(result[0]);
                        var createSecondTableOperation = Assert.IsType<CreateTableOperation>(result[1]);
                        Assert.IsType<CreateIndexOperation>(result[2]);
                        Assert.IsType<CreateIndexOperation>(result[3]);
                        var addFkOperation = Assert.IsType<AddForeignKeyOperation>(result[4]);

                        Assert.Equal(0, createFirstTableOperation.ForeignKeys.Count);
                        Assert.Equal(1, createSecondTableOperation.ForeignKeys.Count);
                        Assert.Equal(createFirstTableOperation.Name, addFkOperation.Table);
                    });
        }

        [Fact]
        public void Model_differ_breaks_foreign_key_cycles_in_drop_table_operations()
        {
            Execute(
                modelBuilder =>
                    {
                        modelBuilder.Entity(
                            "Third",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                    x.Property<int>("FourthId");
                                });
                        modelBuilder.Entity(
                            "Fourth",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                    x.Property<int>("ThirdId");
                                });

                        modelBuilder.Entity("Third").HasOne("Fourth").WithMany().HasForeignKey("FourthId");
                        modelBuilder.Entity("Fourth").HasOne("Third").WithMany().HasForeignKey("ThirdId");
                    },
                _ => { },
                operations =>
                    {
                        Assert.Collection(
                            operations,
                            o => Assert.IsType<DropForeignKeyOperation>(o),
                            o => Assert.IsType<DropTableOperation>(o),
                            o => Assert.IsType<DropTableOperation>(o));
                    });
        }

        [Fact]
        public void Create_table()
        {
            Execute(
                _ => { },
                modelBuilder => modelBuilder.Entity(
                    "Node",
                    x =>
                        {
                            x.ToTable("Node", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("AltId");
                            x.HasAlternateKey("AltId");
                            x.Property<int?>("ParentAltId");
                            x.HasOne("Node").WithMany().HasForeignKey("ParentAltId");
                            x.HasIndex("ParentAltId");
                        }),
                operations =>
                    {
                        Assert.Equal(3, operations.Count);

                        var ensureSchemaOperation = Assert.IsType<EnsureSchemaOperation>(operations[0]);
                        Assert.Equal("dbo", ensureSchemaOperation.Name);

                        var createTableOperation = Assert.IsType<CreateTableOperation>(operations[1]);
                        Assert.Equal("Node", createTableOperation.Name);
                        Assert.Equal("dbo", createTableOperation.Schema);
                        Assert.Equal(3, createTableOperation.Columns.Count);
                        Assert.Null(createTableOperation.Columns.First(o => o.Name == "AltId").DefaultValue);
                        Assert.NotNull(createTableOperation.PrimaryKey);
                        Assert.Equal(1, createTableOperation.UniqueConstraints.Count);
                        Assert.Equal(1, createTableOperation.ForeignKeys.Count);

                        Assert.IsType<CreateIndexOperation>(operations[2]);
                    });
        }

        [Fact]
        public void Drop_table()
        {
            Execute(
                modelBuilder => modelBuilder.Entity("Fox").ToTable("Fox", "dbo"),
                _ => { },
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<DropTableOperation>(operations[0]);
                        Assert.Equal("Fox", operation.Name);
                        Assert.Equal("dbo", operation.Schema);
                    });
        }

        [Fact]
        public void Rename_table()
        {
            Execute(
                source => source.Entity(
                    "Cat",
                    x =>
                        {
                            x.ToTable("Cat", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                        }),
                target => target.Entity(
                    "Cat",
                    x =>
                        {
                            x.ToTable("Cats", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id").HasName("PK_Cat");
                        }),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<RenameTableOperation>(operations[0]);
                        Assert.Equal("Cat", operation.Name);
                        Assert.Equal("dbo", operation.Schema);
                        Assert.Equal("Cats", operation.NewName);
                        Assert.Null(operation.NewSchema);
                    });
        }

        [Fact]
        public void Move_table()
        {
            Execute(
                source => source.Entity(
                    "Person",
                    x =>
                        {
                            x.ToTable("People", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                        }),
                target => target.Entity("Person",
                    x =>
                        {
                            x.ToTable("People", "public");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                        }),
                operations =>
                    {
                        Assert.Equal(2, operations.Count);

                        var ensureSchemaOperation = Assert.IsType<EnsureSchemaOperation>(operations[0]);
                        Assert.Equal("public", ensureSchemaOperation.Name);

                        var renameTableOperation = Assert.IsType<RenameTableOperation>(operations[1]);
                        Assert.Equal("People", renameTableOperation.Name);
                        Assert.Equal("dbo", renameTableOperation.Schema);
                        Assert.Null(renameTableOperation.NewName);
                        Assert.Equal("public", renameTableOperation.NewSchema);
                    });
        }

        [Fact]
        public void Rename_entity_type()
        {
            Execute(
                source => source.Entity(
                    "Dog",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id").HasName("PK_Dog");
                        }),
                target => target.Entity(
                    "Doge",
                    x =>
                        {
                            x.ToTable("Dog");
                            x.Property<int>("Id");
                            x.HasKey("Id").HasName("PK_Dog");
                        }),
                Assert.Empty);
        }

        [Fact]
        public void Create_shared_table_with_two_types()
        {
            Execute(
                _ => { },
                modelBuilder =>
                    {
                        modelBuilder.Entity(
                            "Cat",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.Property<string>("MouseId");
                                    x.HasKey("Id");
                                    x.ToTable("Animal");
                                });
                        modelBuilder.Entity(
                            "Dog",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.Property<string>("BoneId");
                                    x.HasKey("Id");
                                    x.HasOne("Cat").WithOne().HasForeignKey("Dog", "Id");
                                    x.ToTable("Animal");
                                });
                    },
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var createTableOperation = Assert.IsType<CreateTableOperation>(operations[0]);
                        Assert.Equal("Animal", createTableOperation.Name);
                        Assert.Equal("Id", createTableOperation.PrimaryKey.Columns.Single());
                        Assert.Equal(new[] { "Id", "MouseId", "BoneId" }, createTableOperation.Columns.Select(c => c.Name));
                        Assert.Equal(0, createTableOperation.ForeignKeys.Count);
                        Assert.Equal(0, createTableOperation.UniqueConstraints.Count);
                    });
        }

        [Fact]
        public void Drop_shared_table_with_two_types()
        {
            Execute(
                modelBuilder =>
                    {
                        modelBuilder.Entity(
                            "Cat",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.Property<string>("MouseId");
                                    x.HasKey("Id");
                                    x.ToTable("Animal");
                                });
                        modelBuilder.Entity(
                            "Dog",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.Property<string>("BoneId");
                                    x.HasKey("Id");
                                    x.HasOne("Cat").WithOne().HasForeignKey("Dog", "Id");
                                    x.ToTable("Animal");
                                });
                    },
                _ => { },
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var dropTableOperation = Assert.IsType<DropTableOperation>(operations[0]);
                        Assert.Equal("Animal", dropTableOperation.Name);
                    });
        }

        [Fact]
        public void Add_type_to_shared_table()
        {
            Execute(
                modelBuilder => {
                         modelBuilder.Entity(
                             "Cat",
                             x =>
                                 {
                                     x.Property<int>("Id");
                                     x.Property<string>("MouseId");
                                     x.HasKey("Id");
                                     x.ToTable("Animal");
                                 });
                },
                _ => { },
                modelBuilder =>
                    {
                        modelBuilder.Entity(
                            "Dog",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.Property<string>("BoneId");
                                    x.HasKey("Id");
                                    x.HasOne("Cat").WithOne().HasForeignKey("Dog", "Id");
                                    x.ToTable("Animal");
                                });
                    },
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var alterTableOperation = Assert.IsType<AddColumnOperation>(operations[0]);
                        Assert.Equal("BoneId", alterTableOperation.Name);
                    });
        }

        [Fact]
        public void Remove_type_from_shared_table()
        {
            Execute(
                modelBuilder => {
                                    modelBuilder.Entity(
                                        "Cat",
                                        x =>
                                            {
                                                x.Property<int>("Id");
                                                x.Property<string>("MouseId");
                                                x.HasKey("Id");
                                                x.ToTable("Animal");
                                            });
                },
                modelBuilder =>
                    {
                        modelBuilder.Entity(
                            "Dog",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.Property<string>("BoneId");
                                    x.HasKey("Id");
                                    x.HasOne("Cat").WithOne().HasForeignKey("Dog", "Id");
                                    x.ToTable("Animal");
                                });
                    },
                _ => { },
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var alterTableOperation = Assert.IsType<DropColumnOperation>(operations[0]);
                        Assert.Equal("BoneId", alterTableOperation.Name);
                    });
        }

        [Fact]
        public void Move_type_from_one_shared_table_to_another()
        {
            Execute(
                modelBuilder =>
                    {
                        modelBuilder.Entity(
                            "Cat",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.Property<string>("MouseId");
                                    x.HasKey("Id");
                                });
                        modelBuilder.Entity(
                            "Dog",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.Property<string>("BoneId");
                                    x.HasKey("Id");
                                });
                        modelBuilder.Entity(
                            "Animal",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.Property<string>("HandlerId");
                                    x.HasKey("Id");
                                });
                    },
                modelBuilder =>
                    {
                        modelBuilder.Entity(
                            "Animal",
                            x =>
                                {
                                    x.HasOne("Dog").WithOne().HasForeignKey("Dog", "Id");
                                    x.ToTable("Dog");
                                });
                    },
                modelBuilder =>
                    {
                        modelBuilder.Entity(
                            "Animal",
                            x =>
                                {
                                    x.HasOne("Cat").WithOne().HasForeignKey("Cat", "Id");
                                    x.ToTable("Cat");
                                });
                    },
                operations =>
                    {
                        Assert.Equal(2, operations.Count);

                        var dropColumnOperation = Assert.IsType<DropColumnOperation>(operations[0]);
                        Assert.Equal("HandlerId", dropColumnOperation.Name);
                        Assert.Equal("Dog", dropColumnOperation.Table);


                        var addColumnOperation = Assert.IsType<AddColumnOperation>(operations[1]);
                        Assert.Equal("HandlerId", addColumnOperation.Name);
                        Assert.Equal("Cat", addColumnOperation.Table);
                    });
        }

        [Fact]
        public void Add_column()
        {
            Execute(
                source => source.Entity(
                    "Dragon",
                    x =>
                        {
                            x.ToTable("Dragon", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                        }),
                target => target.Entity(
                    "Dragon",
                    x =>
                        {
                            x.ToTable("Dragon", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<string>("Name")
                                .HasColumnType("nvarchar(30)")
                                .IsRequired()
                                .HasDefaultValue("Draco")
                                .HasDefaultValueSql("CreateDragonName()");
                        }),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<AddColumnOperation>(operations[0]);
                        Assert.Equal("dbo", operation.Schema);
                        Assert.Equal("Dragon", operation.Table);
                        Assert.Equal("Name", operation.Name);
                        Assert.Equal(typeof(string), operation.ClrType);
                        Assert.Equal("nvarchar(30)", operation.ColumnType);
                        Assert.False(operation.IsNullable);
                        Assert.Equal("", operation.DefaultValue);
                        Assert.Equal("CreateDragonName()", operation.DefaultValueSql);
                    });
        }

        [Fact]
        public void Add_column_with_computed_value()
        {
            Execute(
                source => source.Entity(
                    "Dragon",
                    x =>
                        {
                            x.ToTable("Dragon", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                        }),
                target => target.Entity(
                    "Dragon",
                    x =>
                        {
                            x.ToTable("Dragon", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<string>("Name")
                                .HasColumnType("nvarchar(30)")
                                .IsRequired()
                                .HasDefaultValue("Draco")
                                .HasComputedColumnSql("CreateDragonName()");
                        }),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<AddColumnOperation>(operations[0]);
                        Assert.Equal("dbo", operation.Schema);
                        Assert.Equal("Dragon", operation.Table);
                        Assert.Equal("Name", operation.Name);
                        Assert.Equal(typeof(string), operation.ClrType);
                        Assert.Equal("nvarchar(30)", operation.ColumnType);
                        Assert.False(operation.IsNullable);
                        Assert.Equal("", operation.DefaultValue);
                        Assert.Equal("CreateDragonName()", operation.ComputedColumnSql);
                    });
        }

        [Fact] // Issue #4501
        public void Add_column_ValueGeneratedOnAddOrUpdate_with_default_value_sql()
        {
            Execute(
                source => source.Entity(
                    "Dragon",
                    x =>
                        {
                            x.ToTable("Dragon", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                        }),
                target => target.Entity(
                    "Dragon",
                    x =>
                        {
                            x.ToTable("Dragon", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<DateTime>("LastModified")
                                .HasDefaultValueSql("GETDATE()")
                                .ValueGeneratedOnAddOrUpdate();
                        }),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<AddColumnOperation>(operations[0]);
                        Assert.Equal("dbo", operation.Schema);
                        Assert.Equal("Dragon", operation.Table);
                        Assert.Equal("LastModified", operation.Name);
                        Assert.Equal(typeof(DateTime), operation.ClrType);
                        Assert.Null(operation.ComputedColumnSql);
                        Assert.Equal("GETDATE()", operation.DefaultValueSql);
                    });
        }

        [Fact]
        public void Add_column_ValueGeneratedOnUpdate_with_default_value_sql()
        {
            Execute(
                source => source.Entity(
                    "Dragon",
                    x =>
                        {
                            x.ToTable("Dragon", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                        }),
                target => target.Entity(
                    "Dragon",
                    x =>
                        {
                            x.ToTable("Dragon", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<DateTime>("LastModified")
                                .HasDefaultValueSql("GETDATE()")
                                .ValueGeneratedOnUpdate();
                        }),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<AddColumnOperation>(operations[0]);
                        Assert.Equal("dbo", operation.Schema);
                        Assert.Equal("Dragon", operation.Table);
                        Assert.Equal("LastModified", operation.Name);
                        Assert.Equal(typeof(DateTime), operation.ClrType);
                        Assert.Null(operation.ComputedColumnSql);
                        Assert.Equal("GETDATE()", operation.DefaultValueSql);
                    });
        }

        [Theory]
        [InlineData(typeof(int), 0)]
        [InlineData(typeof(int?), 0)]
        [InlineData(typeof(string), "")]
        [InlineData(typeof(byte[]), new byte[0])]
        [InlineData(typeof(SomeEnum), 0)]
        [InlineData(typeof(SomeEnum?), 0)]
        public void Add_column_not_null(Type type, object expectedDefault)
        {
            Execute(
                source => source.Entity(
                    "Robin",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                        }),
                target => target.Entity(
                    "Robin",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property(type, "Value").IsRequired();
                        }),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<AddColumnOperation>(operations[0]);
                        Assert.Equal("Robin", operation.Table);
                        Assert.Equal("Value", operation.Name);
                        Assert.Equal(expectedDefault, operation.DefaultValue);
                    });
        }

        private enum SomeEnum
        {
            Default
        }

        [Fact]
        public void Drop_column()
        {
            Execute(
                source => source.Entity(
                    "Firefly",
                    x =>
                        {
                            x.ToTable("Firefly", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<string>("Name").HasColumnType("nvarchar(30)");
                        }),
                target => target.Entity(
                    "Firefly",
                    x =>
                        {
                            x.ToTable("Firefly", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                        }),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<DropColumnOperation>(operations[0]);
                        Assert.Equal("dbo", operation.Schema);
                        Assert.Equal("Firefly", operation.Table);
                        Assert.Equal("Name", operation.Name);
                    });
        }

        [Fact]
        public void Rename_column()
        {
            Execute(
                source => source.Entity(
                    "Zebra",
                    x =>
                        {
                            x.ToTable("Zebra", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<string>("Name").HasColumnType("nvarchar(30)");
                        }),
                target => target.Entity(
                    "Zebra",
                    x =>
                        {
                            x.ToTable("Zebra", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<string>("Name").HasColumnName("ZebraName").HasColumnType("nvarchar(30)");
                        }),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<RenameColumnOperation>(operations[0]);
                        Assert.Equal("dbo", operation.Schema);
                        Assert.Equal("Zebra", operation.Table);
                        Assert.Equal("Name", operation.Name);
                        Assert.Equal("ZebraName", operation.NewName);
                    });
        }

        [Fact]
        public void Rename_property()
        {
            Execute(
                source => source.Entity(
                    "Buffalo",
                    x =>
                        {
                            x.ToTable("Buffalo", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<string>("BuffaloName").HasColumnType("nvarchar(30)");
                        }),
                target => target.Entity(
                    "Buffalo",
                    x =>
                        {
                            x.ToTable("Buffalo", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<string>("Name").HasColumnName("BuffaloName").HasColumnType("nvarchar(30)");
                        }),
                Assert.Empty);
        }

        [Fact]
        public void Rename_property_and_column()
        {
            Execute(
                source => source.Entity("Buffalo").Property<int>("BuffaloId"),
                target => target.Entity("Buffalo").Property<int>("Id"),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<RenameColumnOperation>(operations[0]);
                        Assert.Equal("Buffalo", operation.Table);
                        Assert.Equal("BuffaloId", operation.Name);
                        Assert.Equal("Id", operation.NewName);
                    });
        }

        [Fact]
        public void Add_custom_value_generator()
        {
            Execute(
                source => source.Entity(
                    "Toad",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.Property<string>("Name");
                        }),
                target => target.Entity(
                    "Toad",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.Property<string>("Name")
                                .HasValueGenerator<CustomValueGenerator>();
                        }),
                operations => { Assert.Equal(0, operations.Count); });
        }

        [Fact]
        public void Remove_custom_value_generator()
        {
            Execute(
                source => source.Entity(
                    "Toad",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.Property<string>("Name")
                                .HasValueGenerator<CustomValueGenerator>();
                        }),
                target => target.Entity(
                    "Toad",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.Property<string>("Name");
                        }),
                operations => { Assert.Equal(0, operations.Count); });
        }

        private class CustomValueGenerator : ValueGenerator<string>
        {
            public override string Next(EntityEntry entry)
            {
                throw new NotImplementedException();
            }

            public override bool GeneratesTemporaryValues => false;
        }

        [Fact]
        public void Alter_column_nullability()
        {
            Execute(
                source => source.Entity(
                    "Bison",
                    x =>
                        {
                            x.ToTable("Bison", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<string>("Name")
                                .HasColumnType("nvarchar(30)")
                                .IsRequired(true)
                                .HasDefaultValue("Buffy")
                                .HasDefaultValueSql("CreateBisonName()");
                        }),
                target => target.Entity(
                    "Bison",
                    x =>
                        {
                            x.ToTable("Bison", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<string>("Name")
                                .HasColumnType("nvarchar(30)")
                                .IsRequired(false)
                                .HasDefaultValue("Buffy")
                                .HasDefaultValueSql("CreateBisonName()");
                        }),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<AlterColumnOperation>(operations[0]);
                        Assert.Equal("dbo", operation.Schema);
                        Assert.Equal("Bison", operation.Table);
                        Assert.Equal("Name", operation.Name);
                        Assert.Equal(typeof(string), operation.ClrType);
                        Assert.Equal("nvarchar(30)", operation.ColumnType);
                        Assert.True(operation.IsNullable);
                        Assert.Null(operation.DefaultValue);
                        Assert.Equal("CreateBisonName()", operation.DefaultValueSql);
                    });
        }

        [Fact]
        public void Alter_column_type()
        {
            Execute(
                source => source.Entity(
                    "Puma",
                    x =>
                        {
                            x.ToTable("Puma", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<string>("Name")
                                .HasColumnType("varchar(30)")
                                .IsRequired()
                                .HasDefaultValue("Puff")
                                .HasDefaultValueSql("CreatePumaName()");
                        }),
                target => target.Entity(
                    "Puma",
                    x =>
                        {
                            x.ToTable("Puma", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<string>("Name")
                                .HasColumnType("varchar(450)")
                                .IsRequired()
                                .HasDefaultValue("Puff")
                                .HasDefaultValueSql("CreatePumaName()");
                        }),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<AlterColumnOperation>(operations[0]);
                        Assert.Equal("dbo", operation.Schema);
                        Assert.Equal("Puma", operation.Table);
                        Assert.Equal("Name", operation.Name);
                        Assert.Equal(typeof(string), operation.ClrType);
                        Assert.Equal("varchar(450)", operation.ColumnType);
                        Assert.False(operation.IsNullable);
                        Assert.Null(operation.DefaultValue);
                        Assert.Equal("CreatePumaName()", operation.DefaultValueSql);
                    });
        }

        [Fact]
        public void Alter_column_max_length()
        {
            Execute(
                source => source.Entity(
                    "Toad",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.Property<string>("Name");
                        }),
                target => target.Entity(
                    "Toad",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.Property<string>("Name")
                                .HasMaxLength(30);
                        }),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<AlterColumnOperation>(operations[0]);
                        Assert.Equal("Toad", operation.Table);
                        Assert.Equal("Name", operation.Name);
                        Assert.Equal(30, operation.MaxLength);
                        Assert.True(operation.IsDestructiveChange);
                    });
        }

        [Fact]
        public void Alter_column_unicode()
        {
            Execute(
                source => source.Entity(
                    "Toad",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.Property<string>("Name");
                        }),
                target => target.Entity(
                    "Toad",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.Property<string>("Name")
                                .IsUnicode(false);
                        }),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<AlterColumnOperation>(operations[0]);
                        Assert.Equal("Toad", operation.Table);
                        Assert.Equal("Name", operation.Name);
                        Assert.False(operation.IsUnicode);
                        Assert.True(operation.IsDestructiveChange);
                    });
        }

        [Fact]
        public void Alter_column_default()
        {
            Execute(
                source => source.Entity(
                    "Cougar",
                    x =>
                        {
                            x.ToTable("Cougar", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<string>("Name")
                                .HasColumnType("nvarchar(30)")
                                .IsRequired()
                                .HasDefaultValueSql("CreateCougarName()")
                                .HasDefaultValue("Butch");
                        }),
                target => target.Entity(
                    "Cougar",
                    x =>
                        {
                            x.ToTable("Cougar", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<string>("Name")
                                .HasColumnType("nvarchar(30)")
                                .IsRequired()
                                .HasDefaultValueSql("CreateCougarName()")
                                .HasDefaultValue("Cosmo");
                        }),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<AlterColumnOperation>(operations[0]);
                        Assert.Equal("dbo", operation.Schema);
                        Assert.Equal("Cougar", operation.Table);
                        Assert.Equal("Name", operation.Name);
                        Assert.Equal(typeof(string), operation.ClrType);
                        Assert.Equal("nvarchar(30)", operation.ColumnType);
                        Assert.False(operation.IsNullable);
                        Assert.Equal("Cosmo", operation.DefaultValue);
                        Assert.Null(operation.DefaultValueSql);
                    });
        }

        [Fact]
        public void Alter_column_default_expression()
        {
            Execute(
                source => source.Entity(
                    "MountainLion",
                    x =>
                        {
                            x.ToTable("MountainLion", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<string>("Name")
                                .HasColumnType("nvarchar(30)")
                                .IsRequired()
                                .HasDefaultValue("Liam")
                                .HasDefaultValueSql("CreateMountainLionName()");
                        }),
                target => target.Entity(
                    "MountainLion",
                    x =>
                        {
                            x.ToTable("MountainLion", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<string>("Name")
                                .HasColumnType("nvarchar(30)")
                                .IsRequired()
                                .HasDefaultValue("Liam")
                                .HasDefaultValueSql("CreateCatamountName()");
                        }),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<AlterColumnOperation>(operations[0]);
                        Assert.Equal("dbo", operation.Schema);
                        Assert.Equal("MountainLion", operation.Table);
                        Assert.Equal("Name", operation.Name);
                        Assert.Equal(typeof(string), operation.ClrType);
                        Assert.Equal("nvarchar(30)", operation.ColumnType);
                        Assert.False(operation.IsNullable);
                        Assert.Null(operation.DefaultValue);
                        Assert.Equal("CreateCatamountName()", operation.DefaultValueSql);
                    });
        }

        [Fact]
        public void Alter_column_computed_expression()
        {
            Execute(
                source => source.Entity(
                    "MountainLion",
                    x =>
                        {
                            x.ToTable("MountainLion", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<string>("Name")
                                .HasColumnType("nvarchar(30)")
                                .IsRequired()
                                .HasDefaultValue("Liam")
                                .HasComputedColumnSql("CreateMountainLionName()");
                        }),
                target => target.Entity(
                    "MountainLion",
                    x =>
                        {
                            x.ToTable("MountainLion", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<string>("Name")
                                .HasColumnType("nvarchar(30)")
                                .IsRequired()
                                .HasDefaultValue("Liam")
                                .HasComputedColumnSql("CreateCatamountName()");
                        }),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<AlterColumnOperation>(operations[0]);
                        Assert.Equal("dbo", operation.Schema);
                        Assert.Equal("MountainLion", operation.Table);
                        Assert.Equal("Name", operation.Name);
                        Assert.Equal(typeof(string), operation.ClrType);
                        Assert.Equal("nvarchar(30)", operation.ColumnType);
                        Assert.False(operation.IsNullable);
                        Assert.Null(operation.DefaultValue);
                        Assert.Equal("CreateCatamountName()", operation.ComputedColumnSql);
                    });
        }

        [Fact]
        public void Add_unique_constraint()
        {
            Execute(
                source => source.Entity(
                    "Flamingo",
                    x =>
                        {
                            x.ToTable("Flamingo", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("AlternateId");
                        }),
                target => target.Entity(
                    "Flamingo",
                    x =>
                        {
                            x.ToTable("Flamingo", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("AlternateId");
                            x.HasAlternateKey("AlternateId");
                        }),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<AddUniqueConstraintOperation>(operations[0]);
                        Assert.Equal("dbo", operation.Schema);
                        Assert.Equal("Flamingo", operation.Table);
                        Assert.Equal("AK_Flamingo_AlternateId", operation.Name);
                        Assert.Equal(new[] { "AlternateId" }, operation.Columns);
                    });
        }

        [Fact]
        public void Drop_unique_constraint()
        {
            Execute(
                source => source.Entity(
                    "Penguin",
                    x =>
                        {
                            x.ToTable("Penguin", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("AlternateId");
                            x.HasAlternateKey("AlternateId");
                        }),
                target => target.Entity(
                    "Penguin",
                    x =>
                        {
                            x.ToTable("Penguin", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("AlternateId");
                        }),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<DropUniqueConstraintOperation>(operations[0]);
                        Assert.Equal("dbo", operation.Schema);
                        Assert.Equal("Penguin", operation.Table);
                        Assert.Equal("AK_Penguin_AlternateId", operation.Name);
                    });
        }

        [Fact]
        public void Rename_unique_constraint()
        {
            Execute(
                source => source.Entity(
                    "Pelican",
                    x =>
                        {
                            x.ToTable("Pelican", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("AlternateId");
                            x.HasAlternateKey("AlternateId");
                        }),
                target => target.Entity(
                    "Pelican",
                    x =>
                        {
                            x.ToTable("Pelican", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("AlternateId");
                            x.HasAlternateKey("AlternateId").HasName("AK_dbo.Pelican_AlternateId");
                        }),
                operations =>
                    {
                        Assert.Equal(2, operations.Count);

                        var dropOperation = Assert.IsType<DropUniqueConstraintOperation>(operations[0]);
                        Assert.Equal("dbo", dropOperation.Schema);
                        Assert.Equal("Pelican", dropOperation.Table);
                        Assert.Equal("AK_Pelican_AlternateId", dropOperation.Name);

                        var addOperation = Assert.IsType<AddUniqueConstraintOperation>(operations[1]);
                        Assert.Equal("dbo", addOperation.Schema);
                        Assert.Equal("Pelican", addOperation.Table);
                        Assert.Equal("AK_dbo.Pelican_AlternateId", addOperation.Name);
                        Assert.Equal(new[] { "AlternateId" }, addOperation.Columns);
                    });
        }

        [Fact]
        public void Alter_unique_constraint_columns()
        {
            Execute(
                source => source.Entity(
                    "Rook",
                    x =>
                        {
                            x.ToTable("Rook", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("AlternateId");
                            x.HasAlternateKey("AlternateId");
                            x.Property<int>("AlternateRookId");
                        }),
                target => target.Entity(
                    "Rook",
                    x =>
                        {
                            x.ToTable("Rook", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("AlternateId");
                            x.Property<int>("AlternateRookId");
                            x.HasAlternateKey("AlternateRookId").HasName("AK_Rook_AlternateId");
                        }),
                operations =>
                    {
                        Assert.Equal(2, operations.Count);

                        var dropOperation = Assert.IsType<DropUniqueConstraintOperation>(operations[0]);
                        Assert.Equal("dbo", dropOperation.Schema);
                        Assert.Equal("Rook", dropOperation.Table);
                        Assert.Equal("AK_Rook_AlternateId", dropOperation.Name);

                        var addOperation = Assert.IsType<AddUniqueConstraintOperation>(operations[1]);
                        Assert.Equal("dbo", addOperation.Schema);
                        Assert.Equal("Rook", addOperation.Table);
                        Assert.Equal("AK_Rook_AlternateId", addOperation.Name);
                        Assert.Equal(new[] { "AlternateRookId" }, addOperation.Columns);
                    });
        }

        [Fact]
        public void Rename_primary_key()
        {
            Execute(
                source => source.Entity(
                    "Puffin",
                    x =>
                        {
                            x.ToTable("Puffin", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                        }),
                target => target.Entity(
                    "Puffin",
                    x =>
                        {
                            x.ToTable("Puffin", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id").HasName("PK_dbo.Puffin");
                        }),
                operations =>
                    {
                        Assert.Equal(2, operations.Count);

                        var dropOperation = Assert.IsType<DropPrimaryKeyOperation>(operations[0]);
                        Assert.Equal("dbo", dropOperation.Schema);
                        Assert.Equal("Puffin", dropOperation.Table);
                        Assert.Equal("PK_Puffin", dropOperation.Name);

                        var addOperation = Assert.IsType<AddPrimaryKeyOperation>(operations[1]);
                        Assert.Equal("dbo", addOperation.Schema);
                        Assert.Equal("Puffin", addOperation.Table);
                        Assert.Equal("PK_dbo.Puffin", addOperation.Name);
                        Assert.Equal(new[] { "Id" }, addOperation.Columns);
                    });
        }

        [Fact]
        public void Alter_primary_key_columns()
        {
            Execute(
                source => source.Entity(
                    "Raven",
                    x =>
                        {
                            x.ToTable("Raven", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("RavenId");
                        }),
                target => target.Entity(
                    "Raven",
                    x =>
                        {
                            x.ToTable("Raven", "dbo");
                            x.Property<int>("Id");
                            x.Property<int>("RavenId");
                            x.HasKey("RavenId");
                        }),
                operations =>
                    {
                        Assert.Equal(2, operations.Count);

                        var dropOperation = Assert.IsType<DropPrimaryKeyOperation>(operations[0]);
                        Assert.Equal("dbo", dropOperation.Schema);
                        Assert.Equal("Raven", dropOperation.Table);
                        Assert.Equal("PK_Raven", dropOperation.Name);

                        var addOperation = Assert.IsType<AddPrimaryKeyOperation>(operations[1]);
                        Assert.Equal("dbo", addOperation.Schema);
                        Assert.Equal("Raven", addOperation.Table);
                        Assert.Equal("PK_Raven", addOperation.Name);
                        Assert.Equal(new[] { "RavenId" }, addOperation.Columns);
                    });
        }

        [Fact]
        public void Add_foreign_key()
        {
            Execute(
                source => source.Entity(
                    "Amoeba",
                    x =>
                        {
                            x.ToTable("Amoeba", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("ParentId");
                        }),
                target => target.Entity(
                    "Amoeba",
                    x =>
                        {
                            x.ToTable("Amoeba", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("ParentId");
                            x.HasOne("Amoeba").WithMany().HasForeignKey("ParentId");
                        }),
                operations =>
                    {
                        Assert.Equal(2, operations.Count);

                        var createIndexOperation = Assert.IsType<CreateIndexOperation>(operations[0]);
                        Assert.Equal("dbo", createIndexOperation.Schema);
                        Assert.Equal("Amoeba", createIndexOperation.Table);
                        Assert.Equal("IX_Amoeba_ParentId", createIndexOperation.Name);
                        Assert.Equal(new[] { "ParentId" }, createIndexOperation.Columns);

                        var addFkOperation = Assert.IsType<AddForeignKeyOperation>(operations[1]);
                        Assert.Equal("dbo", addFkOperation.Schema);
                        Assert.Equal("Amoeba", addFkOperation.Table);
                        Assert.Equal("FK_Amoeba_Amoeba_ParentId", addFkOperation.Name);
                        Assert.Equal(new[] { "ParentId" }, addFkOperation.Columns);
                        Assert.Equal("dbo", addFkOperation.PrincipalSchema);
                        Assert.Equal("Amoeba", addFkOperation.PrincipalTable);
                        Assert.Equal(new[] { "Id" }, addFkOperation.PrincipalColumns);
                        Assert.Equal(ReferentialAction.Cascade, addFkOperation.OnDelete);
                        Assert.Equal(ReferentialAction.NoAction, addFkOperation.OnUpdate);
                    });
        }

        [Fact]
        public void Add_optional_foreign_key()
        {
            Execute(
                source => source.Entity(
                    "Amoeba",
                    x =>
                        {
                            x.ToTable("Amoeba", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int?>("ParentId");
                        }),
                target => target.Entity(
                    "Amoeba",
                    x =>
                        {
                            x.ToTable("Amoeba", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int?>("ParentId");
                            x.HasOne("Amoeba").WithMany().HasForeignKey("ParentId");
                        }),
                operations =>
                    {
                        Assert.Equal(2, operations.Count);

                        var createIndexOperation = Assert.IsType<CreateIndexOperation>(operations[0]);
                        Assert.Equal("dbo", createIndexOperation.Schema);
                        Assert.Equal("Amoeba", createIndexOperation.Table);
                        Assert.Equal("IX_Amoeba_ParentId", createIndexOperation.Name);
                        Assert.Equal(new[] { "ParentId" }, createIndexOperation.Columns);

                        var addFkOperation = Assert.IsType<AddForeignKeyOperation>(operations[1]);
                        Assert.Equal("dbo", addFkOperation.Schema);
                        Assert.Equal("Amoeba", addFkOperation.Table);
                        Assert.Equal("FK_Amoeba_Amoeba_ParentId", addFkOperation.Name);
                        Assert.Equal(new[] { "ParentId" }, addFkOperation.Columns);
                        Assert.Equal("dbo", addFkOperation.PrincipalSchema);
                        Assert.Equal("Amoeba", addFkOperation.PrincipalTable);
                        Assert.Equal(new[] { "Id" }, addFkOperation.PrincipalColumns);
                        Assert.Equal(ReferentialAction.Restrict, addFkOperation.OnDelete);
                        Assert.Equal(ReferentialAction.NoAction, addFkOperation.OnUpdate);
                    });
        }

        [Fact]
        public void Add_optional_foreign_key_with_cascade_delete()
        {
            Execute(
                source => source.Entity(
                    "Amoeba",
                    x =>
                        {
                            x.ToTable("Amoeba", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int?>("ParentId");
                        }),
                target => target.Entity(
                    "Amoeba",
                    x =>
                        {
                            x.ToTable("Amoeba", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int?>("ParentId");
                            x.HasOne("Amoeba").WithMany().HasForeignKey("ParentId").OnDelete(DeleteBehavior.Cascade);
                        }),
                operations =>
                    {
                        Assert.Equal(2, operations.Count);

                        var createIndexOperation = Assert.IsType<CreateIndexOperation>(operations[0]);
                        Assert.Equal("dbo", createIndexOperation.Schema);
                        Assert.Equal("Amoeba", createIndexOperation.Table);
                        Assert.Equal("IX_Amoeba_ParentId", createIndexOperation.Name);
                        Assert.Equal(new[] { "ParentId" }, createIndexOperation.Columns);

                        var addFkOperation = Assert.IsType<AddForeignKeyOperation>(operations[1]);
                        Assert.Equal("dbo", addFkOperation.Schema);
                        Assert.Equal("Amoeba", addFkOperation.Table);
                        Assert.Equal("FK_Amoeba_Amoeba_ParentId", addFkOperation.Name);
                        Assert.Equal(new[] { "ParentId" }, addFkOperation.Columns);
                        Assert.Equal("dbo", addFkOperation.PrincipalSchema);
                        Assert.Equal("Amoeba", addFkOperation.PrincipalTable);
                        Assert.Equal(new[] { "Id" }, addFkOperation.PrincipalColumns);
                        Assert.Equal(ReferentialAction.Cascade, addFkOperation.OnDelete);
                        Assert.Equal(ReferentialAction.NoAction, addFkOperation.OnUpdate);
                    });
        }

        [Fact]
        public void Add_required_foreign_key_with_restrict()
        {
            Execute(
                source => source.Entity(
                    "Amoeba",
                    x =>
                        {
                            x.ToTable("Amoeba", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("ParentId");
                        }),
                target => target.Entity(
                    "Amoeba",
                    x =>
                        {
                            x.ToTable("Amoeba", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("ParentId");
                            x.HasOne("Amoeba").WithMany().HasForeignKey("ParentId").OnDelete(DeleteBehavior.Restrict);
                        }),
                operations =>
                    {
                        Assert.Equal(2, operations.Count);

                        var createIndexOperation = Assert.IsType<CreateIndexOperation>(operations[0]);
                        Assert.Equal("dbo", createIndexOperation.Schema);
                        Assert.Equal("Amoeba", createIndexOperation.Table);
                        Assert.Equal("IX_Amoeba_ParentId", createIndexOperation.Name);
                        Assert.Equal(new[] { "ParentId" }, createIndexOperation.Columns);

                        var addFkOperation = Assert.IsType<AddForeignKeyOperation>(operations[1]);
                        Assert.Equal("dbo", addFkOperation.Schema);
                        Assert.Equal("Amoeba", addFkOperation.Table);
                        Assert.Equal("FK_Amoeba_Amoeba_ParentId", addFkOperation.Name);
                        Assert.Equal(new[] { "ParentId" }, addFkOperation.Columns);
                        Assert.Equal("dbo", addFkOperation.PrincipalSchema);
                        Assert.Equal("Amoeba", addFkOperation.PrincipalTable);
                        Assert.Equal(new[] { "Id" }, addFkOperation.PrincipalColumns);
                        Assert.Equal(ReferentialAction.Restrict, addFkOperation.OnDelete);
                        Assert.Equal(ReferentialAction.NoAction, addFkOperation.OnUpdate);
                    });
        }

        [Fact]
        public void Add_required_foreign_key_with_default()
        {
            Execute(
                source => source.Entity(
                    "Amoeba",
                    x =>
                        {
                            x.ToTable("Amoeba", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("ParentId");
                        }),
                target => target.Entity(
                    "Amoeba",
                    x =>
                        {
                            x.ToTable("Amoeba", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("ParentId");
                            x.HasOne("Amoeba").WithMany().HasForeignKey("ParentId").OnDelete(DeleteBehavior.ClientSetNull);
                        }),
                operations =>
                    {
                        Assert.Equal(2, operations.Count);

                        var createIndexOperation = Assert.IsType<CreateIndexOperation>(operations[0]);
                        Assert.Equal("dbo", createIndexOperation.Schema);
                        Assert.Equal("Amoeba", createIndexOperation.Table);
                        Assert.Equal("IX_Amoeba_ParentId", createIndexOperation.Name);
                        Assert.Equal(new[] { "ParentId" }, createIndexOperation.Columns);

                        var addFkOperation = Assert.IsType<AddForeignKeyOperation>(operations[1]);
                        Assert.Equal("dbo", addFkOperation.Schema);
                        Assert.Equal("Amoeba", addFkOperation.Table);
                        Assert.Equal("FK_Amoeba_Amoeba_ParentId", addFkOperation.Name);
                        Assert.Equal(new[] { "ParentId" }, addFkOperation.Columns);
                        Assert.Equal("dbo", addFkOperation.PrincipalSchema);
                        Assert.Equal("Amoeba", addFkOperation.PrincipalTable);
                        Assert.Equal(new[] { "Id" }, addFkOperation.PrincipalColumns);
                        Assert.Equal(ReferentialAction.Restrict, addFkOperation.OnDelete);
                        Assert.Equal(ReferentialAction.NoAction, addFkOperation.OnUpdate);
                    });
        }

        [Fact]
        public void Add_optional_foreign_key_with_set_null()
        {
            Execute(
                source => source.Entity(
                    "Amoeba",
                    x =>
                        {
                            x.ToTable("Amoeba", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int?>("ParentId");
                        }),
                target => target.Entity(
                    "Amoeba",
                    x =>
                        {
                            x.ToTable("Amoeba", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int?>("ParentId");
                            x.HasOne("Amoeba").WithMany().HasForeignKey("ParentId").OnDelete(DeleteBehavior.SetNull);
                        }),
                operations =>
                    {
                        Assert.Equal(2, operations.Count);

                        var createIndexOperation = Assert.IsType<CreateIndexOperation>(operations[0]);
                        Assert.Equal("dbo", createIndexOperation.Schema);
                        Assert.Equal("Amoeba", createIndexOperation.Table);
                        Assert.Equal("IX_Amoeba_ParentId", createIndexOperation.Name);
                        Assert.Equal(new[] { "ParentId" }, createIndexOperation.Columns);

                        var addFkOperation = Assert.IsType<AddForeignKeyOperation>(operations[1]);
                        Assert.Equal("dbo", addFkOperation.Schema);
                        Assert.Equal("Amoeba", addFkOperation.Table);
                        Assert.Equal("FK_Amoeba_Amoeba_ParentId", addFkOperation.Name);
                        Assert.Equal(new[] { "ParentId" }, addFkOperation.Columns);
                        Assert.Equal("dbo", addFkOperation.PrincipalSchema);
                        Assert.Equal("Amoeba", addFkOperation.PrincipalTable);
                        Assert.Equal(new[] { "Id" }, addFkOperation.PrincipalColumns);
                        Assert.Equal(ReferentialAction.SetNull, addFkOperation.OnDelete);
                        Assert.Equal(ReferentialAction.NoAction, addFkOperation.OnUpdate);
                    });
        }

        [Fact]
        public void Add_optional_foreign_key_with_restrict()
        {
            Execute(
                source => source.Entity(
                    "Amoeba",
                    x =>
                        {
                            x.ToTable("Amoeba", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int?>("ParentId");
                        }),
                target => target.Entity(
                    "Amoeba",
                    x =>
                        {
                            x.ToTable("Amoeba", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int?>("ParentId");
                            x.HasOne("Amoeba").WithMany().HasForeignKey("ParentId").OnDelete(DeleteBehavior.Restrict);
                        }),
                operations =>
                    {
                        Assert.Equal(2, operations.Count);

                        var createIndexOperation = Assert.IsType<CreateIndexOperation>(operations[0]);
                        Assert.Equal("dbo", createIndexOperation.Schema);
                        Assert.Equal("Amoeba", createIndexOperation.Table);
                        Assert.Equal("IX_Amoeba_ParentId", createIndexOperation.Name);
                        Assert.Equal(new[] { "ParentId" }, createIndexOperation.Columns);

                        var addFkOperation = Assert.IsType<AddForeignKeyOperation>(operations[1]);
                        Assert.Equal("dbo", addFkOperation.Schema);
                        Assert.Equal("Amoeba", addFkOperation.Table);
                        Assert.Equal("FK_Amoeba_Amoeba_ParentId", addFkOperation.Name);
                        Assert.Equal(new[] { "ParentId" }, addFkOperation.Columns);
                        Assert.Equal("dbo", addFkOperation.PrincipalSchema);
                        Assert.Equal("Amoeba", addFkOperation.PrincipalTable);
                        Assert.Equal(new[] { "Id" }, addFkOperation.PrincipalColumns);
                        Assert.Equal(ReferentialAction.Restrict, addFkOperation.OnDelete);
                        Assert.Equal(ReferentialAction.NoAction, addFkOperation.OnUpdate);
                    });
        }

        [Fact]
        public void Remove_foreign_key()
        {
            Execute(
                source => source.Entity(
                    "Anemone",
                    x =>
                        {
                            x.ToTable("Anemone", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("ParentId");
                            x.HasOne("Anemone").WithMany().HasForeignKey("ParentId");
                        }),
                target => target.Entity(
                    "Anemone",
                    x =>
                        {
                            x.ToTable("Anemone", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("ParentId");
                        }),
                operations =>
                    {
                        Assert.Equal(2, operations.Count);

                        var dropFkOperation = Assert.IsType<DropForeignKeyOperation>(operations[0]);
                        Assert.Equal("dbo", dropFkOperation.Schema);
                        Assert.Equal("Anemone", dropFkOperation.Table);
                        Assert.Equal("FK_Anemone_Anemone_ParentId", dropFkOperation.Name);

                        var dropIndexOperation = Assert.IsType<DropIndexOperation>(operations[1]);
                        Assert.Equal("dbo", dropIndexOperation.Schema);
                        Assert.Equal("Anemone", dropIndexOperation.Table);
                        Assert.Equal("IX_Anemone_ParentId", dropIndexOperation.Name);
                    });
        }

        [Fact]
        public void Rename_foreign_key()
        {
            Execute(
                source => source.Entity(
                    "Nematode",
                    x =>
                        {
                            x.ToTable("Nematode", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("ParentId");
                            x.HasOne("Nematode").WithMany().HasForeignKey("ParentId");
                        }),
                target => target.Entity(
                    "Nematode",
                    x =>
                        {
                            x.ToTable("Nematode", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("ParentId");
                            x.HasOne("Nematode").WithMany().HasForeignKey("ParentId").HasConstraintName("FK_Nematode_NematodeParent");
                        }),
                operations =>
                    {
                        Assert.Equal(2, operations.Count);

                        var dropOperation = Assert.IsType<DropForeignKeyOperation>(operations[0]);
                        Assert.Equal("dbo", dropOperation.Schema);
                        Assert.Equal("Nematode", dropOperation.Table);
                        Assert.Equal("FK_Nematode_Nematode_ParentId", dropOperation.Name);

                        var addOperation = Assert.IsType<AddForeignKeyOperation>(operations[1]);
                        Assert.Equal("dbo", addOperation.Schema);
                        Assert.Equal("Nematode", addOperation.Table);
                        Assert.Equal("FK_Nematode_NematodeParent", addOperation.Name);
                        Assert.Equal(new[] { "ParentId" }, addOperation.Columns);
                        Assert.Equal("dbo", addOperation.PrincipalSchema);
                        Assert.Equal("Nematode", addOperation.PrincipalTable);
                        Assert.Equal(new[] { "Id" }, addOperation.PrincipalColumns);
                    });
        }

        [Fact]
        public void Alter_foreign_key_columns()
        {
            Execute(
                source => source.Entity(
                    "Mushroom",
                    x =>
                        {
                            x.ToTable("Mushroom", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("ParentId1");
                            x.HasOne("Mushroom").WithMany().HasForeignKey("ParentId1");
                            x.Property<int>("ParentId2");
                        }),
                target => target.Entity(
                    "Mushroom",
                    x =>
                        {
                            x.ToTable("Mushroom", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("ParentId1");
                            x.Property<int>("ParentId2");
                            x.HasOne("Mushroom").WithMany().HasForeignKey("ParentId2").HasConstraintName("FK_Mushroom_Mushroom_ParentId1");
                        }),
                operations =>
                    {
                        Assert.Equal(4, operations.Count);

                        var dropFkOperation = Assert.IsType<DropForeignKeyOperation>(operations[0]);
                        Assert.Equal("dbo", dropFkOperation.Schema);
                        Assert.Equal("Mushroom", dropFkOperation.Table);
                        Assert.Equal("FK_Mushroom_Mushroom_ParentId1", dropFkOperation.Name);

                        var dropIndexOperation = Assert.IsType<DropIndexOperation>(operations[1]);
                        Assert.Equal("dbo", dropIndexOperation.Schema);
                        Assert.Equal("Mushroom", dropIndexOperation.Table);
                        Assert.Equal("IX_Mushroom_ParentId1", dropIndexOperation.Name);

                        var addIndexOperation = Assert.IsType<CreateIndexOperation>(operations[2]);
                        Assert.Equal("dbo", addIndexOperation.Schema);
                        Assert.Equal("Mushroom", addIndexOperation.Table);
                        Assert.Equal("IX_Mushroom_ParentId2", addIndexOperation.Name);
                        Assert.Equal(new[] { "ParentId2" }, addIndexOperation.Columns);

                        var addFkOperation = Assert.IsType<AddForeignKeyOperation>(operations[3]);
                        Assert.Equal("dbo", addFkOperation.Schema);
                        Assert.Equal("Mushroom", addFkOperation.Table);
                        Assert.Equal("FK_Mushroom_Mushroom_ParentId1", addFkOperation.Name);
                        Assert.Equal(new[] { "ParentId2" }, addFkOperation.Columns);
                        Assert.Equal("dbo", addFkOperation.PrincipalSchema);
                        Assert.Equal("Mushroom", addFkOperation.PrincipalTable);
                        Assert.Equal(new[] { "Id" }, addFkOperation.PrincipalColumns);
                    });
        }

        [Fact]
        public void Alter_foreign_key_cascade_delete()
        {
            Execute(
                source => source.Entity(
                    "Mushroom",
                    x =>
                        {
                            x.ToTable("Mushroom", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("ParentId1");
                            x.HasOne("Mushroom").WithMany().HasForeignKey("ParentId1").OnDelete(DeleteBehavior.Restrict);
                            x.Property<int>("ParentId2");
                        }),
                target => target.Entity(
                    "Mushroom",
                    x =>
                        {
                            x.ToTable("Mushroom", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("ParentId1");
                            x.HasOne("Mushroom").WithMany().HasForeignKey("ParentId1").OnDelete(DeleteBehavior.Cascade);
                            x.Property<int>("ParentId2");
                        }),
                operations =>
                    {
                        Assert.Equal(2, operations.Count);

                        var dropOperation = Assert.IsType<DropForeignKeyOperation>(operations[0]);
                        Assert.Equal("dbo", dropOperation.Schema);
                        Assert.Equal("Mushroom", dropOperation.Table);
                        Assert.Equal("FK_Mushroom_Mushroom_ParentId1", dropOperation.Name);

                        var addOperation = Assert.IsType<AddForeignKeyOperation>(operations[1]);
                        Assert.Equal("dbo", addOperation.Schema);
                        Assert.Equal("Mushroom", addOperation.Table);
                        Assert.Equal("FK_Mushroom_Mushroom_ParentId1", addOperation.Name);
                        Assert.Equal(new[] { "ParentId1" }, addOperation.Columns);
                        Assert.Equal("dbo", addOperation.PrincipalSchema);
                        Assert.Equal("Mushroom", addOperation.PrincipalTable);
                        Assert.Equal(new[] { "Id" }, addOperation.PrincipalColumns);
                        Assert.Equal(ReferentialAction.Cascade, addOperation.OnDelete);
                        Assert.Equal(ReferentialAction.NoAction, addOperation.OnUpdate);
                    });
        }

        [Fact]
        public void Alter_foreign_key_target()
        {
            Execute(
                source =>
                    {
                        source.Entity(
                            "Lion",
                            x =>
                                {
                                    x.ToTable("Lion", "odb");
                                    x.Property<int>("LionId");
                                    x.HasKey("LionId");
                                });
                        source.Entity(
                            "Tiger",
                            x =>
                                {
                                    x.ToTable("Tiger", "bod");
                                    x.Property<int>("TigerId");
                                    x.HasKey("TigerId");
                                });
                        source.Entity(
                            "Liger",
                            x =>
                                {
                                    x.ToTable("Liger", "dbo");
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                    x.Property<int>("ParentId");
                                    x.HasOne("Lion").WithMany().HasForeignKey("ParentId");
                                });
                    },
                target =>
                    {
                        target.Entity(
                            "Lion",
                            x =>
                                {
                                    x.ToTable("Lion", "odb");
                                    x.Property<int>("LionId");
                                    x.HasKey("LionId");
                                });
                        target.Entity(
                            "Tiger",
                            x =>
                                {
                                    x.ToTable("Tiger", "bod");
                                    x.Property<int>("TigerId");
                                    x.HasKey("TigerId");
                                });
                        target.Entity(
                            "Liger",
                            x =>
                                {
                                    x.ToTable("Liger", "dbo");
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                    x.Property<int>("ParentId");
                                    x.HasOne("Tiger").WithMany().HasForeignKey("ParentId").HasConstraintName("FK_Liger_Lion_ParentId");
                                });
                    },
                operations =>
                    {
                        Assert.Equal(2, operations.Count);

                        var dropOperation = Assert.IsType<DropForeignKeyOperation>(operations[0]);
                        Assert.Equal("dbo", dropOperation.Schema);
                        Assert.Equal("Liger", dropOperation.Table);
                        Assert.Equal("FK_Liger_Lion_ParentId", dropOperation.Name);

                        var addOperation = Assert.IsType<AddForeignKeyOperation>(operations[1]);
                        Assert.Equal("dbo", addOperation.Schema);
                        Assert.Equal("Liger", addOperation.Table);
                        Assert.Equal("FK_Liger_Lion_ParentId", addOperation.Name);
                        Assert.Equal(new[] { "ParentId" }, addOperation.Columns);
                        Assert.Equal("bod", addOperation.PrincipalSchema);
                        Assert.Equal("Tiger", addOperation.PrincipalTable);
                        Assert.Equal(new[] { "TigerId" }, addOperation.PrincipalColumns);
                    });
        }

        [Fact]
        public void Create_index()
        {
            Execute(
                source => source.Entity(
                    "Hippo",
                    x =>
                        {
                            x.ToTable("Hippo", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("Value");
                        }),
                target => target.Entity(
                    "Hippo",
                    x =>
                        {
                            x.ToTable("Hippo", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("Value");
                            x.HasIndex("Value").IsUnique();
                        }),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<CreateIndexOperation>(operations[0]);
                        Assert.Equal("dbo", operation.Schema);
                        Assert.Equal("Hippo", operation.Table);
                        Assert.Equal("IX_Hippo_Value", operation.Name);
                        Assert.Equal(new[] { "Value" }, operation.Columns);
                        Assert.True(operation.IsUnique);
                    });
        }

        [Fact]
        public void Drop_index()
        {
            Execute(
                source => source.Entity(
                    "Horse",
                    x =>
                        {
                            x.ToTable("Horse", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("Value");
                            x.HasIndex("Value");
                        }),
                target => target.Entity(
                    "Horse",
                    x =>
                        {
                            x.ToTable("Horse", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("Value");
                        }),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<DropIndexOperation>(operations[0]);
                        Assert.Equal("dbo", operation.Schema);
                        Assert.Equal("Horse", operation.Table);
                        Assert.Equal("IX_Horse_Value", operation.Name);
                    });
        }

        [Fact]
        public void Rename_index()
        {
            Execute(
                source => source.Entity(
                    "Donkey",
                    x =>
                        {
                            x.ToTable("Donkey", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("Value");
                            x.HasIndex("Value");
                        }),
                target => target.Entity(
                    "Donkey",
                    x =>
                        {
                            x.ToTable("Donkey", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("Value");
                            x.HasIndex("Value").HasName("IX_dbo.Donkey_Value");
                        }),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<RenameIndexOperation>(operations[0]);
                        Assert.Equal("dbo", operation.Schema);
                        Assert.Equal("Donkey", operation.Table);
                        Assert.Equal("IX_Donkey_Value", operation.Name);
                        Assert.Equal("IX_dbo.Donkey_Value", operation.NewName);
                    });
        }

        [Fact]
        public void Alter_index_columns()
        {
            Execute(
                source => source.Entity(
                    "Muel",
                    x =>
                        {
                            x.ToTable("Muel", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("Value");
                            x.HasIndex("Value");
                            x.Property<int>("MuleValue");
                        }),
                target => target.Entity(
                    "Muel",
                    x =>
                        {
                            x.ToTable("Muel", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("Value");
                            x.Property<int>("MuleValue");
                            x.HasIndex("MuleValue").HasName("IX_Muel_Value");
                        }),
                operations =>
                    {
                        Assert.Equal(2, operations.Count);

                        var dropOperation = Assert.IsType<DropIndexOperation>(operations[0]);
                        Assert.Equal("dbo", dropOperation.Schema);
                        Assert.Equal("Muel", dropOperation.Table);
                        Assert.Equal("IX_Muel_Value", dropOperation.Name);

                        var createOperation = Assert.IsType<CreateIndexOperation>(operations[1]);
                        Assert.Equal("dbo", createOperation.Schema);
                        Assert.Equal("Muel", createOperation.Table);
                        Assert.Equal("IX_Muel_Value", createOperation.Name);
                        Assert.Equal(new[] { "MuleValue" }, createOperation.Columns);
                    });
        }

        [Fact]
        public void Alter_index_uniqueness()
        {
            Execute(
                source => source.Entity(
                    "Pony",
                    x =>
                        {
                            x.ToTable("Pony", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("Value");
                            x.HasIndex("Value").IsUnique(false);
                        }),
                target => target.Entity(
                    "Pony",
                    x =>
                        {
                            x.ToTable("Pony", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("Value");
                            x.HasIndex("Value").IsUnique(true);
                        }),
                operations =>
                    {
                        Assert.Equal(2, operations.Count);

                        var dropOperation = Assert.IsType<DropIndexOperation>(operations[0]);
                        Assert.Equal("dbo", dropOperation.Schema);
                        Assert.Equal("Pony", dropOperation.Table);
                        Assert.Equal("IX_Pony_Value", dropOperation.Name);

                        var createOperation = Assert.IsType<CreateIndexOperation>(operations[1]);
                        Assert.Equal("dbo", createOperation.Schema);
                        Assert.Equal("Pony", createOperation.Table);
                        Assert.Equal("IX_Pony_Value", createOperation.Name);
                        Assert.True(createOperation.IsUnique);
                    });
        }

        [Fact]
        public void Create_sequence()
        {
            Execute(
                _ => { },
                modelBuilder => modelBuilder.HasSequence<int>("Tango", "dbo")
                    .StartsAt(2)
                    .IncrementsBy(3)
                    .HasMin(1)
                    .HasMax(4)
                    .IsCyclic(),
                operations =>
                    {
                        Assert.Equal(2, operations.Count);

                        Assert.IsType<EnsureSchemaOperation>(operations[0]);

                        var operation = Assert.IsType<CreateSequenceOperation>(operations[1]);
                        Assert.Equal("Tango", operation.Name);
                        Assert.Equal("dbo", operation.Schema);
                        Assert.Equal(typeof(int), operation.ClrType);
                        Assert.Equal(2, operation.StartValue);
                        Assert.Equal(3, operation.IncrementBy);
                        Assert.Equal(1, operation.MinValue);
                        Assert.Equal(4, operation.MaxValue);
                        Assert.True(operation.IsCyclic);
                    });
        }

        [Fact]
        public void Drop_sequence()
        {
            Execute(
                modelBuilder => modelBuilder.HasSequence("Bravo", "dbo"),
                _ => { },
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<DropSequenceOperation>(operations[0]);
                        Assert.Equal("Bravo", operation.Name);
                        Assert.Equal("dbo", operation.Schema);
                    });
        }

        [Fact]
        public void Rename_sequence()
        {
            Execute(
                source => source.HasSequence("Bravo", "dbo"),
                target => target.HasSequence("bravo", "dbo"),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<RenameSequenceOperation>(operations[0]);
                        Assert.Equal("Bravo", operation.Name);
                        Assert.Equal("dbo", operation.Schema);
                        Assert.Equal("bravo", operation.NewName);
                        Assert.Null(operation.NewSchema);
                    });
        }

        [Fact]
        public void Move_sequence()
        {
            Execute(
                source => source.HasSequence("Charlie", "dbo"),
                target => target.HasSequence("Charlie", "odb"),
                operations =>
                    {
                        Assert.Equal(2, operations.Count);

                        Assert.IsType<EnsureSchemaOperation>(operations[0]);

                        var operation = Assert.IsType<RenameSequenceOperation>(operations[1]);
                        Assert.Equal("Charlie", operation.Name);
                        Assert.Equal("dbo", operation.Schema);
                        Assert.Null(operation.NewName);
                        Assert.Equal("odb", operation.NewSchema);
                    });
        }

        [Fact]
        public void Alter_sequence_increment_by()
        {
            Execute(
                source => source.HasSequence<int>("Alpha", "dbo")
                    .StartsAt(2)
                    .IncrementsBy(3)
                    .HasMin(1)
                    .HasMax(4)
                    .IsCyclic(),
                source => source.HasSequence<int>("Alpha", "dbo")
                    .StartsAt(2)
                    .IncrementsBy(5)
                    .HasMin(1)
                    .HasMax(4)
                    .IsCyclic(),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<AlterSequenceOperation>(operations[0]);
                        Assert.Equal("Alpha", operation.Name);
                        Assert.Equal("dbo", operation.Schema);
                        Assert.Equal(5, operation.IncrementBy);
                        Assert.Equal(1, operation.MinValue);
                        Assert.Equal(4, operation.MaxValue);
                        Assert.True(operation.IsCyclic);
                    });
        }

        [Fact]
        public void Alter_sequence_max_value()
        {
            Execute(
                source => source.HasSequence<int>("Echo", "dbo")
                    .StartsAt(2)
                    .IncrementsBy(3)
                    .HasMin(1)
                    .HasMax(4)
                    .IsCyclic(),
                source => source.HasSequence<int>("Echo", "dbo")
                    .StartsAt(2)
                    .IncrementsBy(3)
                    .HasMin(1)
                    .HasMax(5)
                    .IsCyclic(),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<AlterSequenceOperation>(operations[0]);
                        Assert.Equal("Echo", operation.Name);
                        Assert.Equal("dbo", operation.Schema);
                        Assert.Equal(3, operation.IncrementBy);
                        Assert.Equal(1, operation.MinValue);
                        Assert.Equal(5, operation.MaxValue);
                        Assert.True(operation.IsCyclic);
                    });
        }

        [Fact]
        public void Alter_sequence_min_value()
        {
            Execute(
                source => source.HasSequence<int>("Delta", "dbo")
                    .StartsAt(2)
                    .IncrementsBy(3)
                    .HasMin(1)
                    .HasMax(4)
                    .IsCyclic(),
                source => source.HasSequence<int>("Delta", "dbo")
                    .StartsAt(2)
                    .IncrementsBy(3)
                    .HasMin(5)
                    .HasMax(4)
                    .IsCyclic(),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<AlterSequenceOperation>(operations[0]);
                        Assert.Equal("Delta", operation.Name);
                        Assert.Equal("dbo", operation.Schema);
                        Assert.Equal(3, operation.IncrementBy);
                        Assert.Equal(5, operation.MinValue);
                        Assert.Equal(4, operation.MaxValue);
                        Assert.True(operation.IsCyclic);
                    });
        }

        [Fact]
        public void Alter_sequence_cycle()
        {
            Execute(
                source => source.HasSequence<int>("Foxtrot", "dbo")
                    .StartsAt(2)
                    .IncrementsBy(3)
                    .HasMin(1)
                    .HasMax(4)
                    .IsCyclic(true),
                source => source.HasSequence<int>("Foxtrot", "dbo")
                    .StartsAt(2)
                    .IncrementsBy(3)
                    .HasMin(1)
                    .HasMax(4)
                    .IsCyclic(false),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<AlterSequenceOperation>(operations[0]);
                        Assert.Equal("Foxtrot", operation.Name);
                        Assert.Equal("dbo", operation.Schema);
                        Assert.Equal(3, operation.IncrementBy);
                        Assert.Equal(1, operation.MinValue);
                        Assert.Equal(4, operation.MaxValue);
                        Assert.False(operation.IsCyclic);
                    });
        }

        [Fact]
        public void Alter_sequence_type()
        {
            Execute(
                source => source.HasSequence<int>("Hotel", "dbo")
                    .StartsAt(2)
                    .IncrementsBy(3)
                    .HasMin(1)
                    .HasMax(4)
                    .IsCyclic(),
                source => source.HasSequence<long>("Hotel", "dbo")
                    .StartsAt(2)
                    .IncrementsBy(3)
                    .HasMin(1)
                    .HasMax(4)
                    .IsCyclic(),
                operations =>
                    {
                        Assert.Equal(2, operations.Count);

                        var dropOperation = Assert.IsType<DropSequenceOperation>(operations[0]);
                        Assert.Equal("Hotel", dropOperation.Name);
                        Assert.Equal("dbo", dropOperation.Schema);

                        var createOperation = Assert.IsType<CreateSequenceOperation>(operations[1]);
                        Assert.Equal("Hotel", createOperation.Name);
                        Assert.Equal("dbo", createOperation.Schema);
                        Assert.Equal(typeof(long), createOperation.ClrType);
                        Assert.Equal(2, createOperation.StartValue);
                        Assert.Equal(3, createOperation.IncrementBy);
                        Assert.Equal(1, createOperation.MinValue);
                        Assert.Equal(4, createOperation.MaxValue);
                        Assert.True(createOperation.IsCyclic);
                    });
        }

        [Fact]
        public void Alter_sequence_start()
        {
            Execute(
                source => source.HasSequence<int>("Golf", "dbo")
                    .StartsAt(2)
                    .IncrementsBy(3)
                    .HasMin(1)
                    .HasMax(4)
                    .IsCyclic(),
                source => source.HasSequence<int>("Golf", "dbo")
                    .StartsAt(5)
                    .IncrementsBy(3)
                    .HasMin(1)
                    .HasMax(4)
                    .IsCyclic(),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<RestartSequenceOperation>(operations[0]);

                        Assert.Equal("dbo", operation.Schema);
                        Assert.Equal("Golf", operation.Name);
                        Assert.Equal(5, operation.StartValue);
                    });
        }

        [Fact]
        public void Restart_altered_sequence()
        {
            Execute(
                source => source.HasSequence<int>("Golf", "dbo")
                    .StartsAt(2)
                    .IncrementsBy(3)
                    .HasMin(1)
                    .HasMax(4)
                    .IsCyclic(),
                source => source.HasSequence<int>("Golf", "dbo")
                    .StartsAt(5)
                    .IncrementsBy(6)
                    .HasMin(1)
                    .HasMax(4)
                    .IsCyclic(),
                operations => Assert.Collection(
                    operations,
                    o => Assert.IsType<AlterSequenceOperation>(o),
                    o => Assert.IsType<RestartSequenceOperation>(o)));
        }

        [Fact]
        public void Diff_IProperty_destructive_when_null_to_not_null()
        {
            Execute(
                source => source.Entity(
                    "Lizard",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int?>("Value");
                        }),
                target => target.Entity(
                    "Lizard",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("Value");
                        }),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<AlterColumnOperation>(operations[0]);
                        Assert.True(operation.IsDestructiveChange);
                    });
        }

        [Fact]
        public void Diff_IProperty_not_destructive_when_not_null_to_null()
        {
            Execute(
                source => source.Entity(
                    "Frog",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("Value");
                        }),
                target => target.Entity(
                    "Frog",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int?>("Value");
                        }),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<AlterColumnOperation>(operations[0]);
                        Assert.False(operation.IsDestructiveChange);
                    });
        }

        [Fact]
        public void Diff_IProperty_destructive_when_type_changed()
        {
            Execute(
                source => source.Entity(
                    "Frog",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("Value");
                        }),
                target => target.Entity(
                    "Frog",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("Value").HasColumnType("bigint");
                        }),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<AlterColumnOperation>(operations[0]);
                        Assert.True(operation.IsDestructiveChange);
                    });
        }

        [Fact]
        public void Sort_works_with_primary_keys_and_columns()
        {
            Execute(
                source => source.Entity(
                    "Jaguar",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                        }),
                target => target.Entity(
                    "Jaguar",
                    x =>
                        {
                            x.Property<string>("Name");
                            x.HasKey("Name");
                        }),
                operations => Assert.Collection(
                    operations,
                    o => Assert.IsType<DropPrimaryKeyOperation>(o),
                    o => Assert.IsType<DropColumnOperation>(o),
                    o => Assert.IsType<AddColumnOperation>(o),
                    o => Assert.IsType<AddPrimaryKeyOperation>(o)));
        }

        [Fact]
        public void Sort_adds_unique_constraint_after_column()
        {
            Execute(
                source => source.Entity(
                    "Panther",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                        }),
                target => target.Entity(
                    "Panther",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("AlternateId");
                            x.HasAlternateKey("AlternateId");
                        }),
                operations => Assert.Collection(
                    operations,
                    o => Assert.IsType<AddColumnOperation>(o),
                    o => Assert.IsType<AddUniqueConstraintOperation>(o)));
        }

        [Fact]
        public void Sort_drops_unique_constraint_before_column()
        {
            Execute(
                source => source.Entity(
                    "Bobcat",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("AlternateId");
                            x.HasAlternateKey("AlternateId");
                        }),
                target => target.Entity(
                    "Bobcat",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                        }),
                operations => Assert.Collection(
                    operations,
                    o => Assert.IsType<DropUniqueConstraintOperation>(o),
                    o => Assert.IsType<DropColumnOperation>(o)));
        }

        [Fact]
        public void Sort_creates_index_after_column()
        {
            Execute(
                source => source.Entity(
                    "Coyote",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                        }),
                target => target.Entity(
                    "Coyote",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("Value");
                            x.HasIndex("Value");
                        }),
                operations => Assert.Collection(
                    operations,
                    o => Assert.IsType<AddColumnOperation>(o),
                    o => Assert.IsType<CreateIndexOperation>(o)));
        }

        [Fact]
        public void Sort_drops_index_before_column()
        {
            Execute(
                source => source.Entity(
                    "Wolf",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("Value");
                            x.HasIndex("Value");
                        }),
                target => target.Entity(
                    "Wolf",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                        }),
                operations => Assert.Collection(
                    operations,
                    o => Assert.IsType<DropIndexOperation>(o),
                    o => Assert.IsType<DropColumnOperation>(o)));
        }

        [Fact]
        public void Sort_adds_foreign_key_after_column()
        {
            Execute(
                source => source.Entity(
                    "Algae",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                        }),
                target => target.Entity(
                    "Algae",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("ParentId");
                            x.HasOne("Algae").WithMany().HasForeignKey("ParentId");
                        }),
                operations => Assert.Collection(
                    operations,
                    o => Assert.IsType<AddColumnOperation>(o),
                    o => Assert.IsType<CreateIndexOperation>(o),
                    o => Assert.IsType<AddForeignKeyOperation>(o)));
        }

        [Fact]
        public void Sort_drops_foreign_key_before_column()
        {
            Execute(
                source => source.Entity(
                    "Bacteria",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("ParentId");
                            x.HasOne("Bacteria").WithMany().HasForeignKey("ParentId");
                        }),
                target => target.Entity(
                    "Bacteria",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                        }),
                operations => Assert.Collection(
                    operations,
                    o => Assert.IsType<DropForeignKeyOperation>(o),
                    o => Assert.IsType<DropIndexOperation>(o),
                    o => Assert.IsType<DropColumnOperation>(o)));
        }

        [Fact]
        public void Sort_adds_foreign_key_after_target_table()
        {
            Execute(
                source => source.Entity(
                    "Car",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("MakerId");
                        }),
                target =>
                    {
                        target.Entity(
                            "Maker",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                });
                        target.Entity(
                            "Car",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                    x.Property<int>("MakerId");
                                    x.HasOne("Maker").WithMany().HasForeignKey("MakerId");
                                });
                    },
                operations => Assert.Collection(
                    operations,
                    o => Assert.IsType<CreateTableOperation>(o),
                    o => Assert.IsType<CreateIndexOperation>(o),
                    o => Assert.IsType<AddForeignKeyOperation>(o)));
        }

        [Fact]
        public void Sort_drops_foreign_key_before_target_table()
        {
            Execute(
                source =>
                    {
                        source.Entity(
                            "Maker",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                });
                        source.Entity(
                            "Boat",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                    x.Property<int>("MakerId");
                                    x.HasOne("Maker").WithMany().HasForeignKey("MakerId");
                                });
                    },
                target => target.Entity(
                    "Boat",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("MakerId");
                        }),
                operations => Assert.Collection(
                    operations,
                    o => Assert.IsType<DropForeignKeyOperation>(o),
                    o => Assert.IsType<DropTableOperation>(o),
                    o => Assert.IsType<DropIndexOperation>(o)));
        }

        [Fact]
        public void Sort_adds_foreign_key_after_target_column_and_unique_constraint()
        {
            Execute(
                source =>
                    {
                        source.Entity(
                            "Maker",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                });
                        source.Entity(
                            "Airplane",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                    x.Property<int>("MakerId");
                                });
                    },
                target =>
                    {
                        target.Entity(
                            "Maker",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                    x.Property<int>("AlternateId");
                                });
                        target.Entity(
                            "Airplane",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                    x.Property<int>("MakerId");
                                    x.HasOne("Maker").WithMany().HasForeignKey("MakerId").HasPrincipalKey("AlternateId");
                                });
                    },
                operations => Assert.Collection(
                    operations,
                    o => Assert.IsType<AddColumnOperation>(o),
                    o => Assert.IsType<AddUniqueConstraintOperation>(o),
                    o => Assert.IsType<CreateIndexOperation>(o),
                    o => Assert.IsType<AddForeignKeyOperation>(o)));
        }

        [Fact]
        public void Sort_drops_foreign_key_before_target_column_and_unique_constraint()
        {
            Execute(
                source =>
                    {
                        source.Entity(
                            "Maker",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                    x.Property<int>("AlternateId");
                                });
                        source.Entity(
                            "Submarine",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                    x.Property<int>("MakerId");
                                    x.HasOne("Maker").WithMany().HasForeignKey("MakerId").HasPrincipalKey("AlternateId");
                                });
                    },
                target =>
                    {
                        target.Entity(
                            "Maker",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                });
                        target.Entity(
                            "Submarine",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                    x.Property<int>("MakerId");
                                });
                    },
                operations => Assert.Collection(
                    operations,
                    o => Assert.IsType<DropForeignKeyOperation>(o),
                    o => Assert.IsType<DropIndexOperation>(o),
                    o => Assert.IsType<DropUniqueConstraintOperation>(o),
                    o => Assert.IsType<DropColumnOperation>(o)));
        }

        [Fact]
        public void Sort_creates_tables_in_topologic_order()
        {
            Execute(
                _ => { },
                modelBuilder =>
                    {
                        modelBuilder.Entity(
                            "Maker",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                });
                        modelBuilder.Entity(
                            "Helicopter",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                    x.Property<int>("MakerId");
                                    x.HasOne("Maker").WithMany().HasForeignKey("MakerId");
                                });
                    },
                operations =>
                    {
                        Assert.Equal(3, operations.Count);

                        var operation1 = Assert.IsType<CreateTableOperation>(operations[0]);
                        Assert.Equal("Maker", operation1.Name);

                        var operation2 = Assert.IsType<CreateTableOperation>(operations[1]);
                        Assert.Equal("Helicopter", operation2.Name);

                        var operation3 = Assert.IsType<CreateIndexOperation>(operations[2]);
                    });
        }

        [Fact]
        public void Sort_drops_tables_in_topologic_order()
        {
            Execute(
                modelBuilder =>
                    {
                        modelBuilder.Entity(
                            "Maker",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                });
                        modelBuilder.Entity(
                            "Glider",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                    x.Property<int>("MakerId");
                                    x.HasOne("Maker").WithMany().HasForeignKey("MakerId");
                                });
                    },
                _ => { },
                operations =>
                    {
                        Assert.Equal(2, operations.Count);

                        var operation1 = Assert.IsType<DropTableOperation>(operations[0]);
                        Assert.Equal("Glider", operation1.Name);

                        var operation2 = Assert.IsType<DropTableOperation>(operations[1]);
                        Assert.Equal("Maker", operation2.Name);
                    });
        }

        [Fact]
        public void Rename_column_with_primary_key()
        {
            Execute(
                source => source.Entity(
                    "Hornet",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                        }),
                target => target.Entity(
                    "Hornet",
                    x =>
                        {
                            x.Property<int>("Id").HasColumnName("HornetId");
                            x.HasKey("Id");
                        }),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        Assert.IsType<RenameColumnOperation>(operations[0]);
                    });
        }

        [Fact]
        public void Rename_column_with_unique_constraint()
        {
            Execute(
                source => source.Entity(
                    "Wasp",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<string>("Name");
                            x.HasAlternateKey("Name");
                        }),
                target => target.Entity(
                    "Wasp",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<string>("Name").HasColumnName("WaspName");
                            x.HasAlternateKey("Name");
                        }),
                operations =>
                    {
                        Assert.Equal(3, operations.Count);

                        Assert.IsType<DropUniqueConstraintOperation>(operations[0]);
                        Assert.IsType<RenameColumnOperation>(operations[1]);
                        Assert.IsType<AddUniqueConstraintOperation>(operations[2]);
                    });
        }

        [Fact]
        public void Rename_column_with_index()
        {
            Execute(
                source => source.Entity(
                    "Bee",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<string>("Name");
                            x.HasIndex("Name");
                        }),
                target => target.Entity(
                    "Bee",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<string>("Name").HasColumnName("BeeName");
                            x.HasIndex("Name");
                        }),
                operations =>
                    {
                        Assert.Equal(2, operations.Count);

                        Assert.IsType<RenameColumnOperation>(operations[0]);
                        Assert.IsType<RenameIndexOperation>(operations[1]);
                    });
        }

        [Fact]
        public void Rename_table_with_unique_constraint()
        {
            Execute(
                source => source.Entity(
                    "Fly",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<string>("Name");
                            x.HasAlternateKey("Name");
                        }),
                target => target.Entity(
                    "Fly",
                    x =>
                        {
                            x.ToTable("Flies");
                            x.Property<int>("Id");
                            x.HasKey("Id").HasName("PK_Fly");
                            x.Property<string>("Name");
                            x.HasAlternateKey("Name").HasName("AK_Fly_Name");
                        }),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        Assert.IsType<RenameTableOperation>(operations[0]);
                    });
        }

        [Fact]
        public void Rename_table_with_index()
        {
            Execute(
                source => source.Entity(
                    "Gnat",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<string>("Name");
                            x.HasIndex("Name");
                        }),
                target => target.Entity(
                    "Gnat",
                    x =>
                        {
                            x.ToTable("Gnats");
                            x.Property<int>("Id");
                            x.HasKey("Id").HasName("PK_Gnat");
                            x.Property<string>("Name");
                            x.HasIndex("Name").HasName("IX_Gnat_Name");
                        }),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        Assert.IsType<RenameTableOperation>(operations[0]);
                    });
        }

        [Fact]
        public void Rename_entity_type_with_primary_key_and_unique_constraint()
        {
            Execute(
                source => source.Entity(
                    "Grasshopper",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<string>("Name");
                            x.HasAlternateKey("Name");
                        }),
                target => target.Entity(
                    "grasshopper",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id").HasName("PK_Grasshopper");
                            x.Property<string>("Name");
                            x.HasAlternateKey("Name").HasName("AK_Grasshopper_Name");
                        }),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        Assert.IsType<RenameTableOperation>(operations[0]);
                    });
        }

        [Fact]
        public void Rename_entity_type_with_index()
        {
            Execute(
                source => source.Entity(
                    "Cricket",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<string>("Name");
                            x.HasIndex("Name");
                        }),
                target => target.Entity(
                    "cricket",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id").HasName("PK_Cricket");
                            x.Property<string>("Name");
                            x.HasIndex("Name").HasName("IX_Cricket_Name");
                        }),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        Assert.IsType<RenameTableOperation>(operations[0]);
                    });
        }

        [Fact]
        public void Rename_column_with_foreign_key()
        {
            Execute(
                source => source.Entity(
                    "Yeast",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("ParentId");
                            x.HasOne("Yeast").WithMany().HasForeignKey("ParentId");
                        }),
                target => target.Entity(
                    "Yeast",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("ParentId").HasColumnName("ParentYeastId");
                            x.HasOne("Yeast").WithMany().HasForeignKey("ParentId");
                        }),
                operations =>
                    {
                        Assert.Equal(4, operations.Count);

                        Assert.IsType<DropForeignKeyOperation>(operations[0]);
                        Assert.IsType<RenameColumnOperation>(operations[1]);
                        Assert.IsType<RenameIndexOperation>(operations[2]);
                        Assert.IsType<AddForeignKeyOperation>(operations[3]);
                    });
        }

        [Fact]
        public void Rename_column_with_referencing_foreign_key()
        {
            Execute(
                source => source.Entity(
                    "Mucor",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("ParentId");
                            x.HasOne("Mucor").WithMany().HasForeignKey("ParentId");
                        }),
                target => target.Entity(
                    "Mucor",
                    x =>
                        {
                            x.Property<int>("Id").HasColumnName("MucorId");
                            x.HasKey("Id");
                            x.Property<int>("ParentId");
                            x.HasOne("Mucor").WithMany().HasForeignKey("ParentId");
                        }),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);
                        Assert.IsType<RenameColumnOperation>(operations[0]);
                    });
        }

        [Fact]
        public void Rename_table_with_foreign_key()
        {
            Execute(
                source =>
                    {
                        source.Entity(
                            "Zebra",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                });
                        source.Entity(
                            "Zonkey",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                    x.Property<int>("ParentId");
                                    x.HasOne("Zebra").WithMany().HasForeignKey("ParentId");
                                });
                    },
                target =>
                    {
                        target.Entity(
                            "Zebra",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                });
                        target.Entity(
                            "Zonkey",
                            x =>
                                {
                                    x.ToTable("Zonkeys");
                                    x.Property<int>("Id");
                                    x.HasKey("Id").HasName("PK_Zonkey");
                                    x.Property<int>("ParentId");
                                    x.HasOne("Zebra").WithMany().HasForeignKey("ParentId").HasConstraintName("FK_Zonkey_Zebra_ParentId");
                                    x.HasIndex("ParentId").HasName("IX_Zonkey_ParentId");
                                });
                    },
                operations =>
                    {
                        Assert.Equal(1, operations.Count);
                        Assert.IsType<RenameTableOperation>(operations[0]);
                    });
        }

        [Fact]
        public void Rename_table_with_referencing_foreign_key()
        {
            Execute(
                source =>
                    {
                        source.Entity(
                            "Jaguar",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                });
                        source.Entity(
                            "Jaglion",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                    x.Property<int>("ParentId");
                                    x.HasOne("Jaguar").WithMany().HasForeignKey("ParentId");
                                });
                    },
                target =>
                    {
                        target.Entity(
                            "Jaguar",
                            x =>
                                {
                                    x.ToTable("Jaguars");
                                    x.Property<int>("Id");
                                    x.HasKey("Id").HasName("PK_Jaguar");
                                });
                        target.Entity(
                            "Jaglion",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                    x.Property<int>("ParentId");
                                    x.HasOne("Jaguar").WithMany().HasForeignKey("ParentId")
                                        .HasConstraintName("FK_Jaglion_Jaguar_ParentId");
                                });
                    },
                operations =>
                    {
                        Assert.Equal(1, operations.Count);
                        Assert.IsType<RenameTableOperation>(operations[0]);
                    });
        }

        [Fact]
        public void Create_table_with_property_on_subtype()
        {
            Execute(
                _ => { },
                modelBuilder =>
                    {
                        IMutableEntityType animal = null;
                        modelBuilder.Entity(
                            "Animal",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                    var discriminatorProperty = x.Property<string>("Discriminator").IsRequired().Metadata;

                                    animal = x.Metadata;
                                    animal.Relational().DiscriminatorProperty = discriminatorProperty;
                                    animal.Relational().DiscriminatorValue = "Animal";
                                });
                        modelBuilder.Entity(
                            "Fish",
                            x =>
                                {
                                    x.Metadata.BaseType = animal;
                                    x.Property<string>("Name");
                                    x.Metadata.Relational().DiscriminatorValue = "Fish";
                                });
                    },
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<CreateTableOperation>(operations[0]);
                        Assert.Equal("Animal", operation.Name);
                        Assert.Equal(3, operation.Columns.Count);

                        Assert.Contains(operation.Columns, c => c.Name == "Name");
                    });
        }

        [Fact]
        public void Create_table_with_required_property_on_subtype()
        {
            Execute(
                _ => { },
                modelBuilder =>
                    {
                        IMutableEntityType animal = null;
                        modelBuilder.Entity(
                            "Animal",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                    var discriminatorProperty = x.Property<string>("Discriminator").IsRequired().Metadata;

                                    animal = x.Metadata;
                                    animal.Relational().DiscriminatorProperty = discriminatorProperty;
                                    animal.Relational().DiscriminatorValue = "Animal";
                                });
                        modelBuilder.Entity(
                            "Whale",
                            x =>
                                {
                                    x.Metadata.BaseType = animal;
                                    x.Property<int>("Value");
                                    x.Metadata.Relational().DiscriminatorValue = "Whale";
                                });
                    },
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<CreateTableOperation>(operations[0]);
                        Assert.Equal("Animal", operation.Name);
                        Assert.Equal(3, operation.Columns.Count);

                        Assert.True(operation.Columns.First(c => c.Name == "Value").IsNullable);
                    });
        }

        [Fact]
        public void Add_property_on_subtype()
        {
            Execute(
                source =>
                    {
                        IMutableEntityType animal = null;
                        source.Entity(
                            "Animal",
                            x =>
                                {
                                    x.ToTable("Animal", "dbo");
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                    var discriminatorProperty = x.Property<string>("Discriminator").IsRequired().Metadata;

                                    animal = x.Metadata;
                                    animal.Relational().DiscriminatorProperty = discriminatorProperty;
                                    animal.Relational().DiscriminatorValue = "Animal";
                                });
                        source.Entity(
                            "Shark",
                            x =>
                                {
                                    x.Metadata.BaseType = animal;
                                    x.Metadata.Relational().DiscriminatorValue = "Shark";
                                });
                    },
                target =>
                    {
                        IMutableEntityType animal = null;
                        target.Entity(
                            "Animal",
                            x =>
                                {
                                    x.ToTable("Animal", "dbo");
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                    var discriminatorProperty = x.Property<string>("Discriminator").IsRequired().Metadata;

                                    animal = x.Metadata;
                                    animal.Relational().DiscriminatorProperty = discriminatorProperty;
                                    animal.Relational().DiscriminatorValue = "Animal";
                                });
                        target.Entity(
                            "Shark",
                            x =>
                                {
                                    x.Metadata.BaseType = animal;
                                    x.Property<string>("Name");
                                    x.Metadata.Relational().DiscriminatorValue = "Shark";
                                });
                    },
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<AddColumnOperation>(operations[0]);
                        Assert.Equal("dbo", operation.Schema);
                        Assert.Equal("Animal", operation.Table);
                        Assert.Equal("Name", operation.Name);
                    });
        }

        [Fact]
        public void Add_required_property_on_subtype()
        {
            Execute(
                source =>
                    {
                        IMutableEntityType animal = null;
                        source.Entity(
                            "Animal",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                    var discriminatorProperty = x.Property<string>("Discriminator").IsRequired().Metadata;

                                    animal = x.Metadata;
                                    animal.Relational().DiscriminatorProperty = discriminatorProperty;
                                    animal.Relational().DiscriminatorValue = "Animal";
                                });
                        source.Entity(
                            "Marlin",
                            x =>
                                {
                                    x.Metadata.BaseType = animal;
                                    x.Metadata.Relational().DiscriminatorValue = "Marlin";
                                });
                    },
                target =>
                    {
                        IMutableEntityType animal = null;
                        target.Entity(
                            "Animal",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                    var discriminatorProperty = x.Property<string>("Discriminator").IsRequired().Metadata;

                                    animal = x.Metadata;
                                    animal.Relational().DiscriminatorProperty = discriminatorProperty;
                                    animal.Relational().DiscriminatorValue = "Animal";
                                });
                        target.Entity(
                            "Marlin",
                            x =>
                                {
                                    x.Metadata.BaseType = animal;
                                    x.Property<int>("Value");
                                    x.Metadata.Relational().DiscriminatorValue = "Marlin";
                                });
                    },
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<AddColumnOperation>(operations[0]);
                        Assert.Equal("Value", operation.Name);
                        Assert.Equal("Value", operation.Name);
                        Assert.True(operation.IsNullable);
                    });
        }

        [Fact]
        public void Remove_property_on_subtype()
        {
            Execute(
                source =>
                    {
                        IMutableEntityType animal = null;
                        source.Entity(
                            "Animal",
                            x =>
                                {
                                    x.ToTable("Animal", "dbo");
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                    var discriminatorProperty = x.Property<string>("Discriminator").IsRequired().Metadata;

                                    animal = x.Metadata;
                                    animal.Relational().DiscriminatorProperty = discriminatorProperty;
                                    animal.Relational().DiscriminatorValue = "Animal";
                                });
                        source.Entity(
                            "Blowfish",
                            x =>
                                {
                                    x.Metadata.BaseType = animal;
                                    x.Property<string>("Name");
                                    x.Metadata.Relational().DiscriminatorValue = "Blowfish";
                                });
                    },
                target =>
                    {
                        IMutableEntityType animal = null;
                        target.Entity(
                            "Animal",
                            x =>
                                {
                                    x.ToTable("Animal", "dbo");
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                    var discriminatorProperty = x.Property<string>("Discriminator").IsRequired().Metadata;

                                    animal = x.Metadata;
                                    animal.Relational().DiscriminatorProperty = discriminatorProperty;
                                    animal.Relational().DiscriminatorValue = "Animal";
                                });
                        target.Entity(
                            "Blowfish",
                            x =>
                                {
                                    x.Metadata.BaseType = animal;
                                    x.Metadata.Relational().DiscriminatorValue = "Blowfish";
                                });
                    },
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<DropColumnOperation>(operations[0]);
                        Assert.Equal("dbo", operation.Schema);
                        Assert.Equal("Animal", operation.Table);
                        Assert.Equal("Name", operation.Name);
                    });
        }

        [Fact]
        public void Alter_property_on_subtype()
        {
            Execute(
                source =>
                    {
                        IMutableEntityType animal = null;
                        source.Entity(
                            "Animal",
                            x =>
                                {
                                    x.ToTable("Animal", "dbo");
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                    var discriminatorProperty = x.Property<string>("Discriminator").IsRequired().Metadata;

                                    animal = x.Metadata;
                                    animal.Relational().DiscriminatorProperty = discriminatorProperty;
                                    animal.Relational().DiscriminatorValue = "Animal";
                                });
                        source.Entity(
                            "Barracuda",
                            x =>
                                {
                                    x.Metadata.BaseType = animal;
                                    x.Property<string>("Name");
                                    x.Metadata.Relational().DiscriminatorValue = "Barracuda";
                                });
                    },
                target =>
                    {
                        IMutableEntityType animal = null;
                        target.Entity(
                            "Animal",
                            x =>
                                {
                                    x.ToTable("Animal", "dbo");
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                    var discriminatorProperty = x.Property<string>("Discriminator").IsRequired().Metadata;

                                    animal = x.Metadata;
                                    animal.Relational().DiscriminatorProperty = discriminatorProperty;
                                    animal.Relational().DiscriminatorValue = "Animal";
                                });
                        target.Entity(
                            "Barracuda",
                            x =>
                                {
                                    x.Metadata.BaseType = animal;
                                    x.Property<string>("Name").HasColumnType("varchar(30)");
                                    x.Metadata.Relational().DiscriminatorValue = "Barracuda";
                                });
                    },
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<AlterColumnOperation>(operations[0]);
                        Assert.Equal("dbo", operation.Schema);
                        Assert.Equal("Animal", operation.Table);
                        Assert.Equal("Name", operation.Name);
                        Assert.Equal("varchar(30)", operation.ColumnType);
                    });
        }

        [Fact]
        public void Create_index_on_subtype()
        {
            Execute(
                source =>
                    {
                        IMutableEntityType animal = null;
                        source.Entity(
                            "Animal",
                            x =>
                                {
                                    x.ToTable("Animal", "dbo");
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                    var discriminatorProperty = x.Property<string>("Discriminator").IsRequired().Metadata;

                                    animal = x.Metadata;
                                    animal.Relational().DiscriminatorProperty = discriminatorProperty;
                                    animal.Relational().DiscriminatorValue = "Animal";
                                });
                        source.Entity(
                            "Minnow",
                            x =>
                                {
                                    x.Metadata.BaseType = animal;
                                    x.Property<string>("Name");
                                    x.Metadata.Relational().DiscriminatorValue = "Minnow";
                                });
                    },
                target =>
                    {
                        IMutableEntityType animal = null;
                        target.Entity(
                            "Animal",
                            x =>
                                {
                                    x.ToTable("Animal", "dbo");
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                    var discriminatorProperty = x.Property<string>("Discriminator").IsRequired().Metadata;

                                    animal = x.Metadata;
                                    animal.Relational().DiscriminatorProperty = discriminatorProperty;
                                    animal.Relational().DiscriminatorValue = "Animal";
                                });
                        target.Entity(
                            "Minnow",
                            x =>
                                {
                                    x.Metadata.BaseType = animal;
                                    x.Property<string>("Name");
                                    x.HasIndex("Name");
                                    x.Metadata.Relational().DiscriminatorValue = "Minnow";
                                });
                    },
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<CreateIndexOperation>(operations[0]);
                        Assert.Equal("dbo", operation.Schema);
                        Assert.Equal("Animal", operation.Table);
                        Assert.Equal("IX_Animal_Name", operation.Name);
                        Assert.Equal(new[] { "Name" }, operation.Columns);
                    });
        }

        [Fact]
        public void Alter_index_on_subtype()
        {
            Execute(
                source =>
                    {
                        IMutableEntityType animal = null;
                        source.Entity(
                            "Animal",
                            x =>
                                {
                                    x.ToTable("Animal", "dbo");
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                    var discriminatorProperty = x.Property<string>("Discriminator").IsRequired().Metadata;

                                    animal = x.Metadata;
                                    animal.Relational().DiscriminatorProperty = discriminatorProperty;
                                    animal.Relational().DiscriminatorValue = "Animal";
                                });
                        source.Entity(
                            "Pike",
                            x =>
                                {
                                    x.Metadata.BaseType = animal;
                                    x.Property<string>("Name");
                                    x.HasIndex("Name");
                                    x.Metadata.Relational().DiscriminatorValue = "Pike";
                                });
                    },
                target =>
                    {
                        IMutableEntityType animal = null;
                        target.Entity(
                            "Animal",
                            x =>
                                {
                                    x.ToTable("Animal", "dbo");
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                    var discriminatorProperty = x.Property<string>("Discriminator").IsRequired().Metadata;

                                    animal = x.Metadata;
                                    animal.Relational().DiscriminatorProperty = discriminatorProperty;
                                    animal.Relational().DiscriminatorValue = "Animal";
                                });
                        target.Entity(
                            "Pike",
                            x =>
                                {
                                    x.Metadata.BaseType = animal;
                                    x.Property<string>("Name");
                                    x.HasIndex("Name").HasName("IX_Animal_Pike_Name");
                                    x.Metadata.Relational().DiscriminatorValue = "Pike";
                                });
                    },
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<RenameIndexOperation>(operations[0]);
                        Assert.Equal("dbo", operation.Schema);
                        Assert.Equal("Animal", operation.Table);
                        Assert.Equal("IX_Animal_Name", operation.Name);
                        Assert.Equal("IX_Animal_Pike_Name", operation.NewName);
                    });
        }

        [Fact]
        public void Drop_index_on_subtype()
        {
            Execute(
                source =>
                    {
                        IMutableEntityType animal = null;
                        source.Entity(
                            "Animal",
                            x =>
                                {
                                    x.ToTable("Animal", "dbo");
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                    var discriminatorProperty = x.Property<string>("Discriminator").IsRequired().Metadata;

                                    animal = x.Metadata;
                                    animal.Relational().DiscriminatorProperty = discriminatorProperty;
                                    animal.Relational().DiscriminatorValue = "Animal";
                                });
                        source.Entity(
                            "Catfish",
                            x =>
                                {
                                    x.Metadata.BaseType = animal;
                                    x.Property<string>("Name");
                                    x.HasIndex("Name");
                                    x.Metadata.Relational().DiscriminatorValue = "Catfish";
                                });
                    },
                target =>
                    {
                        IMutableEntityType animal = null;
                        target.Entity(
                            "Animal",
                            x =>
                                {
                                    x.ToTable("Animal", "dbo");
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                    var discriminatorProperty = x.Property<string>("Discriminator").IsRequired().Metadata;

                                    animal = x.Metadata;
                                    animal.Relational().DiscriminatorProperty = discriminatorProperty;
                                    animal.Relational().DiscriminatorValue = "Animal";
                                });
                        target.Entity(
                            "Catfish",
                            x =>
                                {
                                    x.Metadata.BaseType = animal;
                                    x.Property<string>("Name");
                                    x.Metadata.Relational().DiscriminatorValue = "Catfish";
                                });
                    },
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<DropIndexOperation>(operations[0]);
                        Assert.Equal("dbo", operation.Schema);
                        Assert.Equal("Animal", operation.Table);
                        Assert.Equal("IX_Animal_Name", operation.Name);
                    });
        }

        [Fact]
        public void Create_table_with_foreign_key_on_base_type()
        {
            Execute(
                _ => { },
                modelBuilder =>
                    {
                        modelBuilder.Entity(
                            "Person",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                });
                        modelBuilder.Entity(
                            "Animal",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                    x.Property<int>("HandlerId");
                                    x.HasOne("Person").WithMany().HasForeignKey("HandlerId");
                                });
                        modelBuilder.Entity("Wyvern").HasBaseType("Animal");
                    },
                operations =>
                    {
                        Assert.Equal(3, operations.Count);
                        Assert.IsType<CreateTableOperation>(operations[0]);

                        var createTableOperation = Assert.IsType<CreateTableOperation>(operations[1]);
                        Assert.Equal("Animal", createTableOperation.Name);
                        Assert.Equal(1, createTableOperation.ForeignKeys.Count);

                        var addForeignKeyOperation = createTableOperation.ForeignKeys[0];
                        Assert.Equal("FK_Animal_Person_HandlerId", addForeignKeyOperation.Name);
                        Assert.Equal(new[] { "HandlerId" }, addForeignKeyOperation.Columns);
                        Assert.Equal("Person", addForeignKeyOperation.PrincipalTable);
                        Assert.Equal(new[] { "Id" }, addForeignKeyOperation.PrincipalColumns);

                        var createIndexOperation = Assert.IsType<CreateIndexOperation>(operations[2]);
                        Assert.Equal("Animal", createIndexOperation.Table);
                        Assert.Equal("IX_Animal_HandlerId", createIndexOperation.Name);
                        Assert.Equal(new[] { "HandlerId" }, createIndexOperation.Columns);
                    });
        }

        [Fact]
        public void Create_table_with_foreign_key_on_subtype()
        {
            Execute(
                _ => { },
                modelBuilder =>
                    {
                        modelBuilder.Entity(
                            "Person",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                });
                        IMutableEntityType animal = null;
                        modelBuilder.Entity(
                            "Animal",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                    var discriminatorProperty = x.Property<string>("Discriminator").IsRequired().Metadata;

                                    animal = x.Metadata;
                                    animal.Relational().DiscriminatorProperty = discriminatorProperty;
                                    animal.Relational().DiscriminatorValue = "Animal";
                                });
                        modelBuilder.Entity(
                            "Stag",
                            x =>
                                {
                                    x.Metadata.BaseType = animal;
                                    x.Property<int>("HandlerId");
                                    x.HasOne("Person").WithMany().HasForeignKey("HandlerId");
                                    x.Metadata.Relational().DiscriminatorValue = "Stag";
                                });
                    },
                operations =>
                    {
                        Assert.Equal(3, operations.Count);

                        Assert.IsType<CreateTableOperation>(operations[0]);

                        var createTableOperation = Assert.IsType<CreateTableOperation>(operations[1]);
                        Assert.Equal("Animal", createTableOperation.Name);
                        Assert.Equal(1, createTableOperation.ForeignKeys.Count);

                        var addForeignKeyOperation = createTableOperation.ForeignKeys[0];
                        Assert.Equal("FK_Animal_Person_HandlerId", addForeignKeyOperation.Name);
                        Assert.Equal(new[] { "HandlerId" }, addForeignKeyOperation.Columns);
                        Assert.Equal("Person", addForeignKeyOperation.PrincipalTable);
                        Assert.Equal(new[] { "Id" }, addForeignKeyOperation.PrincipalColumns);

                        var createIndexOperation = Assert.IsType<CreateIndexOperation>(operations[2]);
                        Assert.Equal("Animal", createIndexOperation.Table);
                        Assert.Equal("IX_Animal_HandlerId", createIndexOperation.Name);
                        Assert.Equal(new[] { "HandlerId" }, createIndexOperation.Columns);
                    });
        }

        [Fact]
        public void Create_table_with_foreign_key_to_subtype()
        {
            Execute(
                _ => { },
                modelBuilder =>
                    {
                        IMutableEntityType animal = null;
                        modelBuilder.Entity(
                            "Animal",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                    var discriminatorProperty = x.Property<string>("Discriminator").IsRequired().Metadata;

                                    animal = x.Metadata;
                                    animal.Relational().DiscriminatorProperty = discriminatorProperty;
                                    animal.Relational().DiscriminatorValue = "Animal";
                                });
                        modelBuilder.Entity(
                            "DomesticAnimal",
                            x =>
                                {
                                    x.Metadata.BaseType = animal;
                                    x.Metadata.Relational().DiscriminatorValue = "DomesticAnimal";
                                });
                        modelBuilder.Entity(
                            "Person",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                    x.Property<int>("PetId");
                                    x.HasOne("DomesticAnimal").WithMany().HasForeignKey("PetId");
                                });
                    },
                operations =>
                    {
                        Assert.Equal(3, operations.Count);

                        Assert.IsType<CreateTableOperation>(operations[0]);

                        var createTableOperation = Assert.IsType<CreateTableOperation>(operations[1]);
                        Assert.Equal("Person", createTableOperation.Name);
                        Assert.Equal(1, createTableOperation.ForeignKeys.Count);

                        var addForeignKeyOperation = createTableOperation.ForeignKeys[0];
                        Assert.Equal("FK_Person_Animal_PetId", addForeignKeyOperation.Name);
                        Assert.Equal(new[] { "PetId" }, addForeignKeyOperation.Columns);
                        Assert.Equal("Animal", addForeignKeyOperation.PrincipalTable);
                        Assert.Equal(new[] { "Id" }, addForeignKeyOperation.PrincipalColumns);

                        var createIndexOperation = Assert.IsType<CreateIndexOperation>(operations[2]);
                        Assert.Equal("Person", createIndexOperation.Table);
                        Assert.Equal("IX_Person_PetId", createIndexOperation.Name);
                        Assert.Equal(new[] { "PetId" }, createIndexOperation.Columns);
                    });
        }

        [Fact]
        public void Create_table_with_selfReferencing_foreign_key_in_hierarchy()
        {
            Execute(
                _ => { },
                modelBuilder =>
                    {
                        IMutableEntityType animal = null;
                        modelBuilder.Entity(
                            "Animal",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                    var discriminatorProperty = x.Property<string>("Discriminator").IsRequired().Metadata;

                                    animal = x.Metadata;
                                    animal.Relational().DiscriminatorProperty = discriminatorProperty;
                                    animal.Relational().DiscriminatorValue = "Animal";
                                });
                        modelBuilder.Entity(
                            "Predator",
                            x =>
                                {
                                    x.Metadata.BaseType = animal;
                                    x.Property<int>("PreyId");
                                    x.HasOne("Animal").WithMany().HasForeignKey("PreyId");
                                    x.Metadata.Relational().DiscriminatorValue = "Predator";
                                });
                    },
                operations =>
                    {
                        Assert.Equal(2, operations.Count);

                        var createTableOperation = Assert.IsType<CreateTableOperation>(operations[0]);
                        Assert.Equal(1, createTableOperation.ForeignKeys.Count);

                        var addForeignKeyOperation = createTableOperation.ForeignKeys[0];
                        Assert.Equal("FK_Animal_Animal_PreyId", addForeignKeyOperation.Name);
                        Assert.Equal(new[] { "PreyId" }, addForeignKeyOperation.Columns);
                        Assert.Equal("Animal", addForeignKeyOperation.PrincipalTable);
                        Assert.Equal(new[] { "Id" }, addForeignKeyOperation.PrincipalColumns);

                        var createIndexOperation = Assert.IsType<CreateIndexOperation>(operations[1]);
                        Assert.Equal("Animal", createIndexOperation.Table);
                        Assert.Equal("IX_Animal_PreyId", createIndexOperation.Name);
                        Assert.Equal(new[] { "PreyId" }, createIndexOperation.Columns);
                    });
        }

        [Fact]
        public void Create_table_with_overlapping_columns_in_hierarchy()
        {
            Execute(
                _ => { },
                modelBuilder =>
                    {
                        modelBuilder.Entity("Animal").Property<int>("Id");
                        modelBuilder.Entity("Cat").HasBaseType("Animal").Property<int>("BreederId");
                        modelBuilder.Entity("Dog").HasBaseType("Animal").Property<int>("BreederId");
                    },
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var createTableOperation = Assert.IsType<CreateTableOperation>(operations[0]);
                        Assert.Equal(3, createTableOperation.Columns.Count);
                    });
        }

        [Fact]
        public void Add_foreign_key_on_base_type()
        {
            Execute(
                modelBuilder =>
                    {
                        modelBuilder.Entity(
                            "Person",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                });
                        modelBuilder.Entity(
                            "Animal",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                    x.Property<int>("HandlerId");
                                });
                        modelBuilder.Entity("Drakee").HasBaseType("Animal");
                    },
                modelBuilder =>
                    {
                        modelBuilder.Entity(
                            "Person",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                });
                        modelBuilder.Entity(
                            "Animal",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                    x.Property<int>("HandlerId");
                                    x.HasOne("Person").WithMany().HasForeignKey("HandlerId");
                                });
                        modelBuilder.Entity("Drakee").HasBaseType("Animal");
                    },
                operations =>
                    {
                        Assert.Equal(2, operations.Count);

                        var createIndexOperation = Assert.IsType<CreateIndexOperation>(operations[0]);
                        Assert.Equal("Animal", createIndexOperation.Table);
                        Assert.Equal("IX_Animal_HandlerId", createIndexOperation.Name);
                        Assert.Equal(new[] { "HandlerId" }, createIndexOperation.Columns);

                        var addFkOperation = Assert.IsType<AddForeignKeyOperation>(operations[1]);
                        Assert.Equal("Animal", addFkOperation.Table);
                        Assert.Equal("FK_Animal_Person_HandlerId", addFkOperation.Name);
                        Assert.Equal(new[] { "HandlerId" }, addFkOperation.Columns);
                        Assert.Equal("Person", addFkOperation.PrincipalTable);
                        Assert.Equal(new[] { "Id" }, addFkOperation.PrincipalColumns);
                    });
        }

        [Fact]
        public void Add_shared_foreign_key_on_subtypes()
        {
            Execute(
                common =>
                    {
                        common.Entity(
                            "Person",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                });
                        IMutableEntityType animal = null;
                        common.Entity(
                            "Animal",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                    var discriminatorProperty = x.Property<string>("Discriminator").IsRequired().Metadata;

                                    animal = x.Metadata;
                                    animal.Relational().DiscriminatorProperty = discriminatorProperty;
                                    animal.Relational().DiscriminatorValue = "Animal";
                                });
                        common.Entity(
                            "GameAnimal",
                            x =>
                                {
                                    x.Metadata.BaseType = animal;
                                    x.Property<int>("HunterId");
                                    x.Metadata.Relational().DiscriminatorValue = "GameAnimal";
                                });
                        common.Entity(
                            "EndangeredAnimal",
                            x =>
                                {
                                    x.Metadata.BaseType = animal;
                                    x.Property<int>("HunterId");
                                    x.Metadata.Relational().DiscriminatorValue = "EndangeredAnimal";
                                });
                    },
                source => { },
                target =>
                    {
                        target.Entity(
                            "GameAnimal",
                            x => { x.HasOne("Person").WithMany().HasForeignKey("HunterId"); });
                        target.Entity(
                            "EndangeredAnimal",
                            x => { x.HasOne("Person").WithMany().HasForeignKey("HunterId"); });
                    },
                operations =>
                    {
                        Assert.Equal(2, operations.Count);

                        var createIndexOperation = Assert.IsType<CreateIndexOperation>(operations[0]);
                        Assert.Equal("Animal", createIndexOperation.Table);
                        Assert.Equal("IX_Animal_HunterId", createIndexOperation.Name);
                        Assert.Equal(new[] { "HunterId" }, createIndexOperation.Columns);

                        var addFkOperation = Assert.IsType<AddForeignKeyOperation>(operations[1]);
                        Assert.Equal("Animal", addFkOperation.Table);
                        Assert.Equal("FK_Animal_Person_HunterId", addFkOperation.Name);
                        Assert.Equal(new[] { "HunterId" }, addFkOperation.Columns);
                        Assert.Equal("Person", addFkOperation.PrincipalTable);
                        Assert.Equal(new[] { "Id" }, addFkOperation.PrincipalColumns);
                    });
        }

        [Fact]
        public void Add_foreign_key_to_subtype()
        {
            Execute(
                source =>
                    {
                        IMutableEntityType animal = null;
                        source.Entity(
                            "Animal",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                    var discriminatorProperty = x.Property<string>("Discriminator").IsRequired().Metadata;

                                    animal = x.Metadata;
                                    animal.Relational().DiscriminatorProperty = discriminatorProperty;
                                    animal.Relational().DiscriminatorValue = "Animal";
                                });
                        source.Entity(
                            "TrophyAnimal",
                            x =>
                                {
                                    x.Metadata.BaseType = animal;
                                    x.Metadata.Relational().DiscriminatorValue = "TrophyAnimal";
                                });
                        source.Entity(
                            "Person",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                    x.Property<int>("TrophyId");
                                });
                    },
                target =>
                    {
                        IMutableEntityType animal = null;
                        target.Entity(
                            "Animal",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                    var discriminatorProperty = x.Property<string>("Discriminator").IsRequired().Metadata;

                                    animal = x.Metadata;
                                    animal.Relational().DiscriminatorProperty = discriminatorProperty;
                                    animal.Relational().DiscriminatorValue = "Animal";
                                });
                        target.Entity(
                            "TrophyAnimal",
                            x =>
                                {
                                    x.Metadata.BaseType = animal;
                                    x.Metadata.Relational().DiscriminatorValue = "TrophyAnimal";
                                });
                        target.Entity(
                            "Person",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                    x.Property<int>("TrophyId");
                                    x.HasOne("TrophyAnimal").WithMany().HasForeignKey("TrophyId");
                                });
                    },
                operations =>
                    {
                        Assert.Equal(2, operations.Count);

                        var createIndexOperation = Assert.IsType<CreateIndexOperation>(operations[0]);
                        Assert.Equal("Person", createIndexOperation.Table);
                        Assert.Equal("IX_Person_TrophyId", createIndexOperation.Name);
                        Assert.Equal(new[] { "TrophyId" }, createIndexOperation.Columns);

                        var addFkOperation = Assert.IsType<AddForeignKeyOperation>(operations[1]);
                        Assert.Equal("Person", addFkOperation.Table);
                        Assert.Equal("FK_Person_Animal_TrophyId", addFkOperation.Name);
                        Assert.Equal(new[] { "TrophyId" }, addFkOperation.Columns);
                        Assert.Equal("Animal", addFkOperation.PrincipalTable);
                        Assert.Equal(new[] { "Id" }, addFkOperation.PrincipalColumns);
                    });
        }

        [Fact]
        public void Drop_foreign_key_on_subtype()
        {
            Execute(
                source =>
                    {
                        source.Entity(
                            "Person",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                });
                        IMutableEntityType animal = null;
                        source.Entity(
                            "Animal",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                    var discriminatorProperty = x.Property<string>("Discriminator").IsRequired().Metadata;

                                    animal = x.Metadata;
                                    animal.Relational().DiscriminatorProperty = discriminatorProperty;
                                    animal.Relational().DiscriminatorValue = "Animal";
                                });
                        source.Entity(
                            "MountAnimal",
                            x =>
                                {
                                    x.Metadata.BaseType = animal;
                                    x.Property<int>("RiderId");
                                    x.HasOne("Person").WithMany().HasForeignKey("RiderId");
                                    x.Metadata.Relational().DiscriminatorValue = "MountAnimal";
                                });
                    },
                target =>
                    {
                        target.Entity(
                            "Person",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                });
                        IMutableEntityType animal = null;
                        target.Entity(
                            "Animal",
                            x =>
                                {
                                    x.Property<int>("Id");
                                    x.HasKey("Id");
                                    var discriminatorProperty = x.Property<string>("Discriminator").IsRequired().Metadata;

                                    animal = x.Metadata;
                                    animal.Relational().DiscriminatorProperty = discriminatorProperty;
                                    animal.Relational().DiscriminatorValue = "Animal";
                                });
                        target.Entity(
                            "MountAnimal",
                            x =>
                                {
                                    x.Metadata.BaseType = animal;
                                    x.Property<int>("RiderId");
                                    x.Metadata.Relational().DiscriminatorValue = "MountAnimal";
                                });
                    },
                operations =>
                    {
                        Assert.Equal(2, operations.Count);

                        var dropFkOperation = Assert.IsType<DropForeignKeyOperation>(operations[0]);
                        Assert.Equal("Animal", dropFkOperation.Table);
                        Assert.Equal("FK_Animal_Person_RiderId", dropFkOperation.Name);

                        var dropIndexOperation = Assert.IsType<DropIndexOperation>(operations[1]);
                        Assert.Equal("Animal", dropIndexOperation.Table);
                        Assert.Equal("IX_Animal_RiderId", dropIndexOperation.Name);
                    });
        }

        [Fact] // See #2802
        public void Diff_IProperty_compares_values_not_references()
        {
            Execute(
                source => source.Entity(
                    "Stork",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<bool>("Value").HasDefaultValue(true);
                        }),
                target => target.Entity(
                    "Stork",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<bool>("Value").HasDefaultValue(true);
                        }),
                Assert.Empty);
        }

        [Fact]
        public void Add_column_to_renamed_table()
        {
            Execute(
                source => source
                    .Entity(
                        "Table",
                        x =>
                            {
                                x.ToTable("Table", "old");
                                x.Property<int>("Id");
                            }),
                target => target
                    .Entity(
                        "Table",
                        x =>
                            {
                                x.ToTable("RenamedTable", "new");
                                x.Property<int>("Id");
                                x.HasKey("Id").HasName("PK_Table");
                                x.Property<string>("Value");
                            }),
                operations =>
                    {
                        Assert.Equal(3, operations.Count);

                        Assert.IsType<EnsureSchemaOperation>(operations[0]);

                        Assert.IsType<RenameTableOperation>(operations[1]);

                        var addColumnOperation = Assert.IsType<AddColumnOperation>(operations[2]);
                        Assert.Equal("new", addColumnOperation.Schema);
                        Assert.Equal("RenamedTable", addColumnOperation.Table);
                        Assert.Equal("Value", addColumnOperation.Name);
                    });
        }

        [Fact]
        public void Add_foreign_key_to_renamed_table()
        {
            Execute(
                source => source
                    .Entity("ReferencedTable", x => x.Property<int>("Id"))
                    .Entity(
                        "Table",
                        x =>
                            {
                                x.ToTable("Table", "old");
                                x.Property<int>("Id");
                                x.Property<int>("ForeignId");
                                x.HasIndex("ForeignId");
                            }),
                target => target
                    .Entity("ReferencedTable", x => x.Property<int>("Id"))
                    .Entity(
                        "Table",
                        x =>
                            {
                                x.ToTable("RenamedTable", "new");
                                x.Property<int>("Id");
                                x.HasKey("Id").HasName("PK_Table");
                                x.Property<int>("ForeignId");
                                x.HasIndex("ForeignId").HasName("IX_Table_ForeignId");
                                x.HasOne("ReferencedTable").WithMany().HasForeignKey("ForeignId");
                            }),
                operations =>
                    {
                        Assert.Equal(3, operations.Count);

                        Assert.IsType<EnsureSchemaOperation>(operations[0]);

                        Assert.IsType<RenameTableOperation>(operations[1]);

                        var addForeignKeyOperation = Assert.IsType<AddForeignKeyOperation>(operations[2]);
                        Assert.Equal("new", addForeignKeyOperation.Schema);
                        Assert.Equal("RenamedTable", addForeignKeyOperation.Table);
                        Assert.Equal("FK_RenamedTable_ReferencedTable_ForeignId", addForeignKeyOperation.Name);
                    });
        }

        [Fact]
        public void Add_foreign_key_to_renamed_column()
        {
            Execute(
                source => source
                    .Entity("ReferencedTable", x => x.Property<int>("Id"))
                    .Entity(
                        "Table",
                        x =>
                            {
                                x.Property<int>("Id");
                                x.Property<int>("ForeignId");
                                x.HasIndex("ForeignId");
                            }),
                target => target
                    .Entity("ReferencedTable", x => x.Property<int>("Id"))
                    .Entity(
                        "Table",
                        x =>
                            {
                                x.Property<int>("Id");
                                x.HasKey("Id").HasName("PK_Table");
                                x.Property<int>("ForeignId").HasColumnName("RenamedForeignId");
                                x.HasIndex("ForeignId").HasName("IX_Table_ForeignId");
                                x.HasOne("ReferencedTable").WithMany().HasForeignKey("ForeignId");
                            }),
                operations =>
                    {
                        Assert.Equal(2, operations.Count);

                        Assert.IsType<RenameColumnOperation>(operations[0]);

                        var addForeignKeyOperation = Assert.IsType<AddForeignKeyOperation>(operations[1]);
                        Assert.Equal("FK_Table_ReferencedTable_RenamedForeignId", addForeignKeyOperation.Name);
                        Assert.Equal(new[] { "RenamedForeignId" }, addForeignKeyOperation.Columns);
                    });
        }

        [Fact]
        public void Add_foreign_key_referencing_renamed_table()
        {
            Execute(
                source => source
                    .Entity(
                        "ReferencedTable",
                        x =>
                            {
                                x.ToTable("ReferencedTable", "old");
                                x.Property<int>("Id");
                            })
                    .Entity(
                        "Table",
                        x =>
                            {
                                x.Property<int>("Id");
                                x.Property<int>("ForeignId");
                                x.HasIndex("ForeignId");
                            }),
                target => target
                    .Entity(
                        "ReferencedTable",
                        x =>
                            {
                                x.ToTable("RenamedReferencedTable", "new");
                                x.Property<int>("Id");
                                x.HasKey("Id").HasName("PK_ReferencedTable");
                            })
                    .Entity(
                        "Table",
                        x =>
                            {
                                x.Property<int>("Id");
                                x.Property<int>("ForeignId");
                                x.HasOne("ReferencedTable").WithMany().HasForeignKey("ForeignId");
                            }),
                operations =>
                    {
                        Assert.Equal(3, operations.Count);

                        Assert.IsType<EnsureSchemaOperation>(operations[0]);

                        Assert.IsType<RenameTableOperation>(operations[1]);

                        var addForeignKeyOperation = Assert.IsType<AddForeignKeyOperation>(operations[2]);
                        Assert.Equal("new", addForeignKeyOperation.PrincipalSchema);
                        Assert.Equal("RenamedReferencedTable", addForeignKeyOperation.PrincipalTable);
                        Assert.Equal("FK_Table_RenamedReferencedTable_ForeignId", addForeignKeyOperation.Name);
                    });
        }

        [Fact]
        public void Add_foreign_key_referencing_renamed_column()
        {
            Execute(
                source => source
                    .Entity("ReferencedTable", x => x.Property<int>("Id"))
                    .Entity(
                        "Table",
                        x =>
                            {
                                x.Property<int>("Id");
                                x.Property<int>("ForeignId");
                                x.HasIndex("ForeignId");
                            }),
                target => target
                    .Entity("ReferencedTable", x => x.Property<int>("Id").HasColumnName("ReferencedTableId"))
                    .Entity(
                        "Table",
                        x =>
                            {
                                x.Property<int>("Id");
                                x.Property<int>("ForeignId");
                                x.HasOne("ReferencedTable").WithMany().HasForeignKey("ForeignId");
                            }),
                operations =>
                    {
                        Assert.Equal(2, operations.Count);

                        Assert.IsType<RenameColumnOperation>(operations[0]);

                        var addForeignKeyOperation = Assert.IsType<AddForeignKeyOperation>(operations[1]);
                        Assert.Equal(new[] { "ReferencedTableId" }, addForeignKeyOperation.PrincipalColumns);
                        Assert.Equal("FK_Table_ReferencedTable_ForeignId", addForeignKeyOperation.Name);
                    });
        }

        [Fact]
        public void Create_table_with_foreign_key_referencing_renamed_table()
        {
            Execute(
                source => source.Entity(
                    "ReferencedTable",
                    x =>
                        {
                            x.ToTable("ReferencedTable", "old");
                            x.Property<int>("Id");
                        }),
                target => target
                    .Entity(
                        "ReferencedTable",
                        x =>
                            {
                                x.ToTable("RenamedReferencedTable", "new");
                                x.Property<int>("Id");
                                x.HasKey("Id").HasName("PK_ReferencedTable");
                            })
                    .Entity(
                        "Table",
                        x =>
                            {
                                x.Property<int>("Id");
                                x.Property<int>("ForeignId");
                                x.HasOne("ReferencedTable").WithMany().HasForeignKey("ForeignId");
                            }),
                operations =>
                    {
                        Assert.Equal(4, operations.Count);

                        Assert.IsType<EnsureSchemaOperation>(operations[0]);

                        Assert.IsType<RenameTableOperation>(operations[1]);

                        var createTableOperation = Assert.IsType<CreateTableOperation>(operations[2]);
                        Assert.Equal(1, createTableOperation.ForeignKeys.Count);

                        var addForeignKeyOperation = createTableOperation.ForeignKeys[0];
                        Assert.Equal("new", addForeignKeyOperation.PrincipalSchema);
                        Assert.Equal("RenamedReferencedTable", addForeignKeyOperation.PrincipalTable);
                        Assert.Equal("FK_Table_RenamedReferencedTable_ForeignId", addForeignKeyOperation.Name);

                        Assert.IsType<CreateIndexOperation>(operations[3]);
                    });
        }

        [Fact]
        public void Create_table_with_foreign_key_referencing_renamed_column()
        {
            Execute(
                source => source
                    .Entity("ReferencedTable", x => x.Property<int>("Id")),
                target => target
                    .Entity("ReferencedTable", x => x.Property<int>("Id").HasColumnName("ReferencedTableId"))
                    .Entity(
                        "Table",
                        x =>
                            {
                                x.Property<int>("Id");
                                x.Property<int>("ForeignId");
                                x.HasOne("ReferencedTable").WithMany().HasForeignKey("ForeignId");
                            }),
                operations =>
                    {
                        Assert.Equal(3, operations.Count);

                        Assert.IsType<RenameColumnOperation>(operations[0]);

                        var createTableOperation = Assert.IsType<CreateTableOperation>(operations[1]);
                        Assert.Equal(1, createTableOperation.ForeignKeys.Count);

                        var addForeignKeyOperation = createTableOperation.ForeignKeys[0];
                        Assert.Equal(new[] { "ReferencedTableId" }, addForeignKeyOperation.PrincipalColumns);
                        Assert.Equal("FK_Table_ReferencedTable_ForeignId", addForeignKeyOperation.Name);

                        Assert.IsType<CreateIndexOperation>(operations[2]);
                    });
        }

        [Fact]
        public void Rename_primary_key_on_renamed_table()
        {
            Execute(
                source => source.Entity("Table").ToTable("Table", "old").Property<int>("Id"),
                target => target.Entity("Table").ToTable("RenamedTable", "new").Property<int>("Id"),
                operations =>
                    {
                        Assert.Equal(4, operations.Count);

                        var dropPrimaryKeyOperation = Assert.IsType<DropPrimaryKeyOperation>(operations[0]);
                        Assert.Equal("old", dropPrimaryKeyOperation.Schema);
                        Assert.Equal("Table", dropPrimaryKeyOperation.Table);
                        Assert.Equal("PK_Table", dropPrimaryKeyOperation.Name);

                        Assert.IsType<EnsureSchemaOperation>(operations[1]);

                        Assert.IsType<RenameTableOperation>(operations[2]);

                        var addPrimaryKeyOperation = Assert.IsType<AddPrimaryKeyOperation>(operations[3]);
                        Assert.Equal("new", addPrimaryKeyOperation.Schema);
                        Assert.Equal("RenamedTable", addPrimaryKeyOperation.Table);
                        Assert.Equal("PK_RenamedTable", addPrimaryKeyOperation.Name);
                    });
        }

        [Fact]
        public void Rename_primary_key_on_renamed_column()
        {
            Execute(
                source => source.Entity("Table").Property<int>("Id"),
                target => target.Entity(
                    "Table",
                    x =>
                        {
                            x.Property<int>("Id").HasColumnName("RenamedId");
                            x.HasKey("Id").HasName("PK_Table_Renamed");
                        }),
                operations =>
                    {
                        Assert.Equal(3, operations.Count);

                        Assert.IsType<DropPrimaryKeyOperation>(operations[0]);

                        Assert.IsType<RenameColumnOperation>(operations[1]);

                        var addPrimaryKeyOperation = Assert.IsType<AddPrimaryKeyOperation>(operations[2]);
                        Assert.Equal(new[] { "RenamedId" }, addPrimaryKeyOperation.Columns);
                        Assert.Equal("PK_Table_Renamed", addPrimaryKeyOperation.Name);
                    });
        }

        [Fact]
        public void Add_alternate_key_to_renamed_table()
        {
            Execute(
                source => source.Entity(
                    "Table",
                    x =>
                        {
                            x.ToTable("Table", "old");
                            x.Property<int>("Id");
                            x.Property<int>("AlternateId");
                        }),
                target => target.Entity(
                    "Table",
                    x =>
                        {
                            x.ToTable("RenamedTable", "new");
                            x.Property<int>("Id");
                            x.HasKey("Id").HasName("PK_Table");
                            x.Property<int>("AlternateId");
                            x.HasAlternateKey("AlternateId");
                        }),
                operations =>
                    {
                        Assert.Equal(3, operations.Count);

                        Assert.IsType<EnsureSchemaOperation>(operations[0]);

                        Assert.IsType<RenameTableOperation>(operations[1]);

                        var addUniqueConstraintOperation = Assert.IsType<AddUniqueConstraintOperation>(operations[2]);
                        Assert.Equal("new", addUniqueConstraintOperation.Schema);
                        Assert.Equal("RenamedTable", addUniqueConstraintOperation.Table);
                        Assert.Equal("AK_RenamedTable_AlternateId", addUniqueConstraintOperation.Name);
                    });
        }

        [Fact]
        public void Add_alternate_key_to_renamed_column()
        {
            Execute(
                source => source.Entity(
                    "Table",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.Property<int>("AlternateId");
                        }),
                target => target.Entity(
                    "Table",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.Property<int>("AlternateId").HasColumnName("RenamedAlternateId");
                            x.HasAlternateKey("AlternateId");
                        }),
                operations =>
                    {
                        Assert.Equal(2, operations.Count);

                        Assert.IsType<RenameColumnOperation>(operations[0]);

                        var addUniqueConstraintOperation = Assert.IsType<AddUniqueConstraintOperation>(operations[1]);
                        Assert.Equal(new[] { "RenamedAlternateId" }, addUniqueConstraintOperation.Columns);
                        Assert.Equal("AK_Table_RenamedAlternateId", addUniqueConstraintOperation.Name);
                    });
        }

        [Fact]
        public void Alter_column_on_renamed_table()
        {
            Execute(
                source => source.Entity(
                    "Table",
                    x =>
                        {
                            x.ToTable("Table", "old");
                            x.Property<int>("Id");
                            x.Property<string>("Value");
                        }),
                target => target.Entity(
                    "Table",
                    x =>
                        {
                            x.ToTable("RenamedTable", "new");
                            x.Property<int>("Id");
                            x.HasKey("Id").HasName("PK_Table");
                            x.Property<string>("Value").IsRequired();
                        }),
                operations =>
                    {
                        Assert.Equal(3, operations.Count);

                        Assert.IsType<EnsureSchemaOperation>(operations[0]);

                        Assert.IsType<RenameTableOperation>(operations[1]);

                        var alterColumnOperation = Assert.IsType<AlterColumnOperation>(operations[2]);
                        Assert.Equal("new", alterColumnOperation.Schema);
                        Assert.Equal("RenamedTable", alterColumnOperation.Table);
                        Assert.Equal("Value", alterColumnOperation.Name);
                    });
        }

        [Fact]
        public void Alter_renamed_column()
        {
            Execute(
                source => source.Entity(
                    "Table",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.Property<string>("Value");
                        }),
                target => target.Entity(
                    "Table",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.Property<string>("Value").HasColumnName("RenamedValue").IsRequired();
                        }),
                operations =>
                    {
                        Assert.Equal(2, operations.Count);

                        Assert.IsType<RenameColumnOperation>(operations[0]);

                        var alterColumnOperation = Assert.IsType<AlterColumnOperation>(operations[1]);
                        Assert.Equal("Table", alterColumnOperation.Table);
                        Assert.Equal("RenamedValue", alterColumnOperation.Name);
                    });
        }

        [Fact]
        public void Alter_renamed_sequence()
        {
            Execute(
                source => source.HasSequence("Sequence", "old"),
                target => target.HasSequence("Sequence", "new").IncrementsBy(2),
                operations =>
                    {
                        Assert.Equal(3, operations.Count);

                        Assert.IsType<EnsureSchemaOperation>(operations[0]);

                        Assert.IsType<RenameSequenceOperation>(operations[1]);

                        var alterSequenceOperation = Assert.IsType<AlterSequenceOperation>(operations[2]);
                        Assert.Equal("new", alterSequenceOperation.Schema);
                        Assert.Equal("Sequence", alterSequenceOperation.Name);
                    });
        }

        [Fact]
        public void Create_index_on_renamed_table()
        {
            Execute(
                source => source.Entity(
                    "Table",
                    x =>
                        {
                            x.ToTable("Table", "old");
                            x.Property<int>("Id");
                            x.Property<int>("Value");
                        }),
                target => target.Entity(
                    "Table",
                    x =>
                        {
                            x.ToTable("RenamedTable", "new");
                            x.Property<int>("Id");
                            x.HasKey("Id").HasName("PK_Table");
                            x.Property<int>("Value");
                            x.HasIndex("Value");
                        }),
                operations =>
                    {
                        Assert.Equal(3, operations.Count);

                        Assert.IsType<EnsureSchemaOperation>(operations[0]);

                        Assert.IsType<RenameTableOperation>(operations[1]);

                        var createIndexOperation = Assert.IsType<CreateIndexOperation>(operations[2]);
                        Assert.Equal("new", createIndexOperation.Schema);
                        Assert.Equal("RenamedTable", createIndexOperation.Table);
                        Assert.Equal("IX_RenamedTable_Value", createIndexOperation.Name);
                    });
        }

        [Fact]
        public void Create_index_on_renamed_column()
        {
            Execute(
                source => source.Entity(
                    "Table",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.Property<int>("Value");
                        }),
                target => target.Entity(
                    "Table",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.Property<int>("Value").HasColumnName("RenamedValue");
                            x.HasIndex("Value");
                        }),
                operations =>
                    {
                        Assert.Equal(2, operations.Count);

                        Assert.IsType<RenameColumnOperation>(operations[0]);

                        var createIndexOperation = Assert.IsType<CreateIndexOperation>(operations[1]);
                        Assert.Equal(new[] { "RenamedValue" }, createIndexOperation.Columns);
                        Assert.Equal("IX_Table_RenamedValue", createIndexOperation.Name);
                    });
        }

        [Fact]
        public void Drop_column_on_renamed_table()
        {
            Execute(
                source => source
                    .Entity(
                        "Table",
                        x =>
                            {
                                x.ToTable("Table", "old");
                                x.Property<int>("Id");
                                x.Property<string>("Value");
                            }),
                target => target
                    .Entity(
                        "Table",
                        x =>
                            {
                                x.ToTable("RenamedTable", "new");
                                x.Property<int>("Id");
                                x.HasKey("Id").HasName("PK_Table");
                            }),
                operations =>
                    {
                        Assert.Equal(3, operations.Count);

                        var dropColumnOperation = Assert.IsType<DropColumnOperation>(operations[0]);
                        Assert.Equal("old", dropColumnOperation.Schema);
                        Assert.Equal("Table", dropColumnOperation.Table);
                        Assert.Equal("Value", dropColumnOperation.Name);

                        Assert.IsType<EnsureSchemaOperation>(operations[1]);

                        Assert.IsType<RenameTableOperation>(operations[2]);
                    });
        }

        [Fact]
        public void Drop_foreign_key_on_renamed_table()
        {
            Execute(
                source => source
                    .Entity("ReferencedTable", x => x.Property<int>("Id"))
                    .Entity(
                        "Table",
                        x =>
                            {
                                x.ToTable("Table", "old");
                                x.Property<int>("Id");
                                x.Property<int>("ForeignId");
                                x.HasIndex("ForeignId");
                                x.HasOne("ReferencedTable").WithMany().HasForeignKey("ForeignId");
                            }),
                target => target
                    .Entity("ReferencedTable", x => x.Property<int>("Id"))
                    .Entity(
                        "Table",
                        x =>
                            {
                                x.ToTable("RenamedTable", "new");
                                x.Property<int>("Id");
                                x.HasKey("Id").HasName("PK_Table");
                                x.Property<int>("ForeignId");
                                x.HasIndex("ForeignId").HasName("IX_Table_ForeignId");
                            }),
                operations =>
                    {
                        Assert.Equal(3, operations.Count);

                        var dropForeignKeyOperation = Assert.IsType<DropForeignKeyOperation>(operations[0]);
                        Assert.Equal("old", dropForeignKeyOperation.Schema);
                        Assert.Equal("Table", dropForeignKeyOperation.Table);
                        Assert.Equal("FK_Table_ReferencedTable_ForeignId", dropForeignKeyOperation.Name);

                        Assert.IsType<EnsureSchemaOperation>(operations[1]);

                        Assert.IsType<RenameTableOperation>(operations[2]);
                    });
        }

        [Fact]
        public void Drop_alternate_key_on_renamed_table()
        {
            Execute(
                source => source.Entity(
                    "Table",
                    x =>
                        {
                            x.ToTable("Table", "old");
                            x.Property<int>("Id");
                            x.Property<int>("AlternateId");
                            x.HasAlternateKey("AlternateId");
                        }),
                target => target.Entity(
                    "Table",
                    x =>
                        {
                            x.ToTable("RenamedTable", "new");
                            x.Property<int>("Id");
                            x.HasKey("Id").HasName("PK_Table");
                            x.Property<int>("AlternateId");
                        }),
                operations =>
                    {
                        Assert.Equal(3, operations.Count);

                        var dropUniqueConstraintOperation = Assert.IsType<DropUniqueConstraintOperation>(operations[0]);
                        Assert.Equal("old", dropUniqueConstraintOperation.Schema);
                        Assert.Equal("Table", dropUniqueConstraintOperation.Table);
                        Assert.Equal("AK_Table_AlternateId", dropUniqueConstraintOperation.Name);

                        Assert.IsType<EnsureSchemaOperation>(operations[1]);

                        Assert.IsType<RenameTableOperation>(operations[2]);
                    });
        }

        [Fact]
        public void Drop_index_on_renamed_table()
        {
            Execute(
                source => source.Entity(
                    "Table",
                    x =>
                        {
                            x.ToTable("Table", "old");
                            x.Property<int>("Id");
                            x.Property<int>("Value");
                            x.HasIndex("Value");
                        }),
                target => target.Entity(
                    "Table",
                    x =>
                        {
                            x.ToTable("RenamedTable", "new");
                            x.Property<int>("Id");
                            x.HasKey("Id").HasName("PK_Table");
                            x.Property<int>("Value");
                        }),
                operations =>
                    {
                        Assert.Equal(3, operations.Count);

                        var dropIndexOperation = Assert.IsType<DropIndexOperation>(operations[0]);
                        Assert.Equal("old", dropIndexOperation.Schema);
                        Assert.Equal("Table", dropIndexOperation.Table);
                        Assert.Equal("IX_Table_Value", dropIndexOperation.Name);

                        Assert.IsType<EnsureSchemaOperation>(operations[1]);

                        Assert.IsType<RenameTableOperation>(operations[2]);
                    });
        }

        [Fact]
        public void Restart_renamed_sequence()
        {
            Execute(
                source => source.HasSequence("Sequence", "old"),
                target => target.HasSequence("Sequence", "new").StartsAt(2),
                operations =>
                    {
                        Assert.Equal(3, operations.Count);

                        Assert.IsType<EnsureSchemaOperation>(operations[0]);

                        Assert.IsType<RenameSequenceOperation>(operations[1]);

                        var alterSequenceOperation = Assert.IsType<RestartSequenceOperation>(operations[2]);
                        Assert.Equal("new", alterSequenceOperation.Schema);
                        Assert.Equal("Sequence", alterSequenceOperation.Name);
                    });
        }

        [Fact]
        public void Rename_column_on_renamed_table()
        {
            Execute(
                source => source
                    .Entity(
                        "Table",
                        x =>
                            {
                                x.ToTable("Table", "old");
                                x.Property<int>("Id");
                                x.Property<string>("Value");
                            }),
                target => target
                    .Entity(
                        "Table",
                        x =>
                            {
                                x.ToTable("RenamedTable", "new");
                                x.Property<int>("Id");
                                x.HasKey("Id").HasName("PK_Table");
                                x.Property<string>("Value").HasColumnName("RenamedValue");
                            }),
                operations =>
                    {
                        Assert.Equal(3, operations.Count);

                        Assert.IsType<EnsureSchemaOperation>(operations[0]);

                        Assert.IsType<RenameTableOperation>(operations[1]);

                        var renameColumnOperation = Assert.IsType<RenameColumnOperation>(operations[2]);
                        Assert.Equal("new", renameColumnOperation.Schema);
                        Assert.Equal("RenamedTable", renameColumnOperation.Table);
                        Assert.Equal("RenamedValue", renameColumnOperation.NewName);
                    });
        }

        [Fact]
        public void Rename_index_on_renamed_table()
        {
            Execute(
                source => source.Entity(
                    "Table",
                    x =>
                        {
                            x.ToTable("Table", "old");
                            x.Property<int>("Id");
                            x.Property<int>("Value");
                            x.HasIndex("Value");
                        }),
                target => target.Entity(
                    "Table",
                    x =>
                        {
                            x.ToTable("RenamedTable", "new");
                            x.Property<int>("Id");
                            x.HasKey("Id").HasName("PK_Table");
                            x.Property<int>("Value");
                            x.HasIndex("Value");
                        }),
                operations =>
                    {
                        Assert.Equal(3, operations.Count);

                        Assert.IsType<EnsureSchemaOperation>(operations[0]);

                        Assert.IsType<RenameTableOperation>(operations[1]);

                        var renameIndexOperation = Assert.IsType<RenameIndexOperation>(operations[2]);
                        Assert.Equal("new", renameIndexOperation.Schema);
                        Assert.Equal("RenamedTable", renameIndexOperation.Table);
                        Assert.Equal("IX_RenamedTable_Value", renameIndexOperation.NewName);
                    });
        }

        [Fact]
        public void Add_alternate_key_on_added_column()
        {
            Execute(
                source => source
                    .Entity(
                        "Table",
                        x => { x.Property<int>("Id"); }),
                target => target
                    .Entity(
                        "Table",
                        x =>
                            {
                                x.Property<int>("Id");
                                x.Property<int>("AlternateId");
                                x.HasAlternateKey("AlternateId");
                            }),
                operations =>
                    {
                        Assert.Equal(2, operations.Count);

                        Assert.IsType<AddColumnOperation>(operations[0]);
                        Assert.IsType<AddUniqueConstraintOperation>(operations[1]);
                    });
        }

        [Fact]
        public void Add_foreign_key_referencing_added_alternate_key()
        {
            Execute(
                source => source
                    .Entity(
                        "Table",
                        x =>
                            {
                                x.Property<int>("Id");
                                x.Property<int>("AlternateId");
                            })
                    .Entity(
                        "ReferencingTable",
                        x =>
                            {
                                x.Property<int>("Id");
                                x.Property<int>("ReferencedAlternateId");
                                x.HasIndex("ReferencedAlternateId");
                            }),
                target => target
                    .Entity(
                        "Table",
                        x =>
                            {
                                x.Property<int>("Id");
                                x.Property<int>("AlternateId");
                                x.HasAlternateKey("AlternateId");
                            })
                    .Entity(
                        "ReferencingTable",
                        x =>
                            {
                                x.Property<int>("Id");
                                x.Property<int>("ReferencedAlternateId");
                                x.HasOne("Table").WithMany()
                                    .HasForeignKey("ReferencedAlternateId")
                                    .HasPrincipalKey("AlternateId");
                            }),
                operations =>
                    {
                        Assert.Equal(2, operations.Count);

                        Assert.IsType<AddUniqueConstraintOperation>(operations[0]);
                        Assert.IsType<AddForeignKeyOperation>(operations[1]);
                    });
        }

        protected override ModelBuilder CreateModelBuilder() => RelationalTestHelpers.Instance.CreateConventionBuilder();
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.Migrations.Operations;
using Xunit;

namespace Microsoft.Data.Entity.Migrations.Infrastructure
{
    // TODO: Test matching
    public class ModelDifferTest : ModelDifferTestBase
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
                            x.Key("ID");
                            x.Property<int>("FK");
                        });

                    modelBuilder.Entity(
                        "Second",
                        x =>
                        {
                            x.Property<int>("ID");
                            x.Key("ID");
                            x.Property<int>("FK");
                        });

                    modelBuilder.Entity("First").Reference("Second").InverseCollection().ForeignKey("FK").PrincipalKey("ID");
                    modelBuilder.Entity("Second").Reference("First").InverseCollection().ForeignKey("FK").PrincipalKey("ID");
                },
                result =>
                {
                    Assert.Equal(3, result.Count);

                    var firstOperation = result[0] as CreateTableOperation;
                    var secondOperation = result[1] as CreateTableOperation;
                    var thirdOperation = result[2] as AddForeignKeyOperation;

                    Assert.NotNull(firstOperation);
                    Assert.NotNull(secondOperation);
                    Assert.NotNull(thirdOperation);

                    Assert.Equal(0, firstOperation.ForeignKeys.Count);
                    Assert.Equal(1, secondOperation.ForeignKeys.Count);
                    Assert.Equal(firstOperation.Name, thirdOperation.Table);
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
                            x.Key("Id");
                            x.Property<int>("FourthId");
                        });
                    modelBuilder.Entity(
                        "Fourth",
                        x =>
                        {
                            x.Property<int>("Id");
                            x.Key("Id");
                            x.Property<int>("ThirdId");
                        });

                    modelBuilder.Entity("Third").Reference("Fourth").InverseCollection().ForeignKey("FourthId");
                    modelBuilder.Entity("Fourth").Reference("Third").InverseCollection().ForeignKey("ThirdId");
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
                        x.Key("Id");
                        x.Property<int>("AltId");
                        x.AlternateKey("AltId");
                        x.Property<int?>("ParentAltId");
                        x.Reference("Node").InverseCollection().ForeignKey("ParentAltId");
                        x.Index("ParentAltId");
                    }),
                operations =>
                {
                    Assert.Equal(3, operations.Count);

                    var createSchemaOperation = Assert.IsType<CreateSchemaOperation>(operations[0]);
                    Assert.Equal("dbo", createSchemaOperation.Name);

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
                        x.Key("Id");
                    }),
                target => target.Entity(
                    "Cat",
                    x =>
                    {
                        x.ToTable("Cats", "dbo");
                        x.Property<int>("Id");
                        x.Key("Id");
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
                        x.Key("Id");
                    }),
                target => target.Entity("Person",
                    x =>
                    {
                        x.ToTable("People", "public");
                        x.Property<int>("Id");
                        x.Key("Id");
                    }),
                operations =>
                {
                    Assert.Equal(2, operations.Count);

                    var createSchemaOperation = Assert.IsType<CreateSchemaOperation>(operations[0]);
                    Assert.Equal("public", createSchemaOperation.Name);

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
                        x.Key("Id").KeyName("PK_Dog");
                    }),
                target => target.Entity(
                    "Doge",
                    x =>
                    {
                        x.ToTable("Dog");
                        x.Property<int>("Id");
                        x.Key("Id").KeyName("PK_Dog");
                    }),
                operations => Assert.Empty(operations));
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
                        x.Key("Id");
                    }),
                target => target.Entity(
                    "Dragon",
                    x =>
                    {
                        x.ToTable("Dragon", "dbo");
                        x.Property<int>("Id");
                        x.Key("Id");
                        x.Property<string>("Name")
                            .HasColumnType("nvarchar(30)")
                            .Required()
                            .DefaultValue("Draco")
                            .DefaultValueSql("CreateDragonName()");
                    }),
                operations =>
                {
                    Assert.Equal(1, operations.Count);

                    var operation = Assert.IsType<AddColumnOperation>(operations[0]);
                    Assert.Equal("dbo", operation.Schema);
                    Assert.Equal("Dragon", operation.Table);
                    Assert.Equal("Name", operation.Name);
                    Assert.Equal("nvarchar(30)", operation.Type);
                    Assert.False(operation.IsNullable);
                    Assert.Equal("Draco", operation.DefaultValue);
                    Assert.Equal("CreateDragonName()", operation.DefaultValueSql);
                });
        }

        [Theory]
        [InlineData(typeof(int), 0)]
        [InlineData(typeof(int?), 0)]
        [InlineData(typeof(string), "")]
        [InlineData(typeof(byte[]), new byte[0])]
        public void Add_column_not_null(Type type, object expectedDefault)
        {
            Execute(
                source => source.Entity(
                    "Robin",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Key("Id");
                    }),
                target => target.Entity(
                    "Robin",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Key("Id");
                        x.Property(type, "Value").Required();
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
                        x.Key("Id");
                        x.Property<string>("Name").HasColumnType("nvarchar(30)");
                    }),
                target => target.Entity(
                    "Firefly",
                    x =>
                    {
                        x.ToTable("Firefly", "dbo");
                        x.Property<int>("Id");
                        x.Key("Id");
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
                        x.Key("Id");
                        x.Property<string>("Name").HasColumnType("nvarchar(30)");
                    }),
                target => target.Entity(
                    "Zebra",
                    x =>
                    {
                        x.ToTable("Zebra", "dbo");
                        x.Property<int>("Id");
                        x.Key("Id");
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
                        x.Key("Id");
                        x.Property<string>("BuffaloName").HasColumnType("nvarchar(30)");
                    }),
                target => target.Entity(
                    "Buffalo",
                    x =>
                    {
                        x.ToTable("Buffalo", "dbo");
                        x.Property<int>("Id");
                        x.Key("Id");
                        x.Property<string>("Name").HasColumnName("BuffaloName").HasColumnType("nvarchar(30)");
                    }),
                operations => Assert.Empty(operations));
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
                        x.Key("Id");
                        x.Property<string>("Name")
                            .HasColumnType("nvarchar(30)")
                            .Required(true)
                            .DefaultValue("Buffy")
                            .DefaultValueSql("CreateBisonName()");
                    }),
                target => target.Entity(
                    "Bison",
                    x =>
                    {
                        x.ToTable("Bison", "dbo");
                        x.Property<int>("Id");
                        x.Key("Id");
                        x.Property<string>("Name")
                            .HasColumnType("nvarchar(30)")
                            .Required(false)
                            .DefaultValue("Buffy")
                            .DefaultValueSql("CreateBisonName()");
                    }),
                operations =>
                {
                    Assert.Equal(1, operations.Count);

                    var operation = Assert.IsType<AlterColumnOperation>(operations[0]);
                    Assert.Equal("dbo", operation.Schema);
                    Assert.Equal("Bison", operation.Table);
                    Assert.Equal("Name", operation.Name);
                    Assert.Equal("nvarchar(30)", operation.Type);
                    Assert.True(operation.IsNullable);
                    Assert.Equal("Buffy", operation.DefaultValue);
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
                        x.Key("Id");
                        x.Property<string>("Name")
                            .HasColumnType("nvarchar(30)")
                            .Required()
                            .DefaultValue("Puff")
                            .DefaultValueSql("CreatePumaName()");
                    }),
                target => target.Entity(
                    "Puma",
                    x =>
                    {
                        x.ToTable("Puma", "dbo");
                        x.Property<int>("Id");
                        x.Key("Id");
                        x.Property<string>("Name")
                            .HasColumnType("nvarchar(450)")
                            .Required()
                            .DefaultValue("Puff")
                            .DefaultValueSql("CreatePumaName()");
                    }),
                operations =>
                {
                    Assert.Equal(1, operations.Count);

                    var operation = Assert.IsType<AlterColumnOperation>(operations[0]);
                    Assert.Equal("dbo", operation.Schema);
                    Assert.Equal("Puma", operation.Table);
                    Assert.Equal("Name", operation.Name);
                    Assert.Equal("nvarchar(450)", operation.Type);
                    Assert.False(operation.IsNullable);
                    Assert.Equal("Puff", operation.DefaultValue);
                    Assert.Equal("CreatePumaName()", operation.DefaultValueSql);
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
                        x.Key("Id");
                        x.Property<string>("Name")
                            .HasColumnType("nvarchar(30)")
                            .Required()
                            .DefaultValue("Butch")
                            .DefaultValueSql("CreateCougarName()");
                    }),
                target => target.Entity(
                    "Cougar",
                    x =>
                    {
                        x.ToTable("Cougar", "dbo");
                        x.Property<int>("Id");
                        x.Key("Id");
                        x.Property<string>("Name")
                            .HasColumnType("nvarchar(30)")
                            .Required()
                            .DefaultValue("Cosmo")
                            .DefaultValueSql("CreateCougarName()");
                    }),
                operations =>
                {
                    Assert.Equal(1, operations.Count);

                    var operation = Assert.IsType<AlterColumnOperation>(operations[0]);
                    Assert.Equal("dbo", operation.Schema);
                    Assert.Equal("Cougar", operation.Table);
                    Assert.Equal("Name", operation.Name);
                    Assert.Equal("nvarchar(30)", operation.Type);
                    Assert.False(operation.IsNullable);
                    Assert.Equal("Cosmo", operation.DefaultValue);
                    Assert.Equal("CreateCougarName()", operation.DefaultValueSql);
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
                        x.Key("Id");
                        x.Property<string>("Name")
                            .HasColumnType("nvarchar(30)")
                            .Required()
                            .DefaultValue("Liam")
                            .DefaultValueSql("CreateMountainLionName()");
                    }),
                target => target.Entity(
                    "MountainLion",
                    x =>
                    {
                        x.ToTable("MountainLion", "dbo");
                        x.Property<int>("Id");
                        x.Key("Id");
                        x.Property<string>("Name")
                            .HasColumnType("nvarchar(30)")
                            .Required()
                            .DefaultValue("Liam")
                            .DefaultValueSql("CreateCatamountName()");
                    }),
                operations =>
                {
                    Assert.Equal(1, operations.Count);

                    var operation = Assert.IsType<AlterColumnOperation>(operations[0]);
                    Assert.Equal("dbo", operation.Schema);
                    Assert.Equal("MountainLion", operation.Table);
                    Assert.Equal("Name", operation.Name);
                    Assert.Equal("nvarchar(30)", operation.Type);
                    Assert.False(operation.IsNullable);
                    Assert.Equal("Liam", operation.DefaultValue);
                    Assert.Equal("CreateCatamountName()", operation.DefaultValueSql);
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
                        x.Key("Id");
                        x.Property<int>("AlternateId");
                    }),
                target => target.Entity(
                    "Flamingo",
                    x =>
                    {
                        x.ToTable("Flamingo", "dbo");
                        x.Property<int>("Id");
                        x.Key("Id");
                        x.Property<int>("AlternateId");
                        x.AlternateKey("AlternateId");
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
                        x.Key("Id");
                        x.Property<int>("AlternateId");
                        x.AlternateKey("AlternateId");
                    }),
                target => target.Entity(
                    "Penguin",
                    x =>
                    {
                        x.ToTable("Penguin", "dbo");
                        x.Property<int>("Id");
                        x.Key("Id");
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
                        x.Key("Id");
                        x.Property<int>("AlternateId");
                        x.AlternateKey("AlternateId");
                    }),
                target => target.Entity(
                    "Pelican",
                    x =>
                    {
                        x.ToTable("Pelican", "dbo");
                        x.Property<int>("Id");
                        x.Key("Id");
                        x.Property<int>("AlternateId");
                        x.AlternateKey("AlternateId").KeyName("AK_dbo.Pelican_AlternateId");
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
                        x.Key("Id");
                        x.Property<int>("AlternateId");
                        x.AlternateKey("AlternateId");
                        x.Property<int>("AlternateRookId");
                    }),
                target => target.Entity(
                    "Rook",
                    x =>
                    {
                        x.ToTable("Rook", "dbo");
                        x.Property<int>("Id");
                        x.Key("Id");
                        x.Property<int>("AlternateId");
                        x.Property<int>("AlternateRookId");
                        x.AlternateKey("AlternateRookId").KeyName("AK_Rook_AlternateId");
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
                        x.Key("Id");
                    }),
                target => target.Entity(
                    "Puffin",
                    x =>
                    {
                        x.ToTable("Puffin", "dbo");
                        x.Property<int>("Id");
                        x.Key("Id").KeyName("PK_dbo.Puffin");
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
                        x.Key("Id");
                        x.Property<int>("RavenId");
                    }),
                target => target.Entity(
                    "Raven",
                    x =>
                    {
                        x.ToTable("Raven", "dbo");
                        x.Property<int>("Id");
                        x.Property<int>("RavenId");
                        x.Key("RavenId");
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
                        x.Key("Id");
                        x.Property<int>("ParentId");
                    }),
                target => target.Entity(
                    "Amoeba",
                    x =>
                    {
                        x.ToTable("Amoeba", "dbo");
                        x.Property<int>("Id");
                        x.Key("Id");
                        x.Property<int>("ParentId");
                        x.Reference("Amoeba").InverseCollection().ForeignKey("ParentId");
                    }),
                operations =>
                {
                    Assert.Equal(1, operations.Count);

                    var operation = Assert.IsType<AddForeignKeyOperation>(operations[0]);
                    Assert.Equal("dbo", operation.Schema);
                    Assert.Equal("Amoeba", operation.Table);
                    Assert.Equal("FK_Amoeba_Amoeba_ParentId", operation.Name);
                    Assert.Equal(new[] { "ParentId" }, operation.Columns);
                    Assert.Equal("dbo", operation.ReferencedSchema);
                    Assert.Equal("Amoeba", operation.ReferencedTable);
                    Assert.Equal(new[] { "Id" }, operation.ReferencedColumns);
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
                        x.Key("Id");
                        x.Property<int>("ParentId");
                        x.Reference("Anemone").InverseCollection().ForeignKey("ParentId");
                    }),
                target => target.Entity(
                    "Anemone",
                    x =>
                    {
                        x.ToTable("Anemone", "dbo");
                        x.Property<int>("Id");
                        x.Key("Id");
                        x.Property<int>("ParentId");
                    }),
                operations =>
                {
                    Assert.Equal(1, operations.Count);

                    var operation = Assert.IsType<DropForeignKeyOperation>(operations[0]);
                    Assert.Equal("dbo", operation.Schema);
                    Assert.Equal("Anemone", operation.Table);
                    Assert.Equal("FK_Anemone_Anemone_ParentId", operation.Name);
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
                        x.Key("Id");
                        x.Property<int>("ParentId");
                        x.Reference("Nematode").InverseCollection().ForeignKey("ParentId");
                    }),
                target => target.Entity(
                    "Nematode",
                    x =>
                    {
                        x.ToTable("Nematode", "dbo");
                        x.Property<int>("Id");
                        x.Key("Id");
                        x.Property<int>("ParentId");
                        x.Reference("Nematode").InverseCollection().ForeignKey("ParentId").ConstraintName("FK_Nematode_NematodeParent");
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
                    Assert.Equal("dbo", addOperation.ReferencedSchema);
                    Assert.Equal("Nematode", addOperation.ReferencedTable);
                    Assert.Equal(new[] { "Id" }, addOperation.ReferencedColumns);
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
                        x.Key("Id");
                        x.Property<int>("ParentId1");
                        x.Reference("Mushroom").InverseCollection().ForeignKey("ParentId1");
                        x.Property<int>("ParentId2");
                    }),
                target => target.Entity(
                    "Mushroom",
                    x =>
                    {
                        x.ToTable("Mushroom", "dbo");
                        x.Property<int>("Id");
                        x.Key("Id");
                        x.Property<int>("ParentId1");
                        x.Property<int>("ParentId2");
                        x.Reference("Mushroom").InverseCollection().ForeignKey("ParentId2").ConstraintName("FK_Mushroom_Mushroom_ParentId1");
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
                    Assert.Equal(new[] { "ParentId2" }, addOperation.Columns);
                    Assert.Equal("dbo", addOperation.ReferencedSchema);
                    Assert.Equal("Mushroom", addOperation.ReferencedTable);
                    Assert.Equal(new[] { "Id" }, addOperation.ReferencedColumns);
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
                            x.Key("LionId");
                        });
                    source.Entity(
                        "Tiger",
                        x =>
                        {
                            x.ToTable("Tiger", "bod");
                            x.Property<int>("TigerId");
                            x.Key("TigerId");
                        });
                    source.Entity(
                        "Liger",
                        x =>
                        {
                            x.ToTable("Liger", "dbo");
                            x.Property<int>("Id");
                            x.Key("Id");
                            x.Property<int>("ParentId");
                            x.Reference("Lion").InverseCollection().ForeignKey("ParentId");
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
                            x.Key("LionId");
                        });
                    target.Entity(
                        "Tiger",
                        x =>
                        {
                            x.ToTable("Tiger", "bod");
                            x.Property<int>("TigerId");
                            x.Key("TigerId");
                        });
                    target.Entity(
                        "Liger",
                        x =>
                        {
                            x.ToTable("Liger", "dbo");
                            x.Property<int>("Id");
                            x.Key("Id");
                            x.Property<int>("ParentId");
                            x.Reference("Tiger").InverseCollection().ForeignKey("ParentId").ConstraintName("FK_Liger_Lion_ParentId");
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
                    Assert.Equal("bod", addOperation.ReferencedSchema);
                    Assert.Equal("Tiger", addOperation.ReferencedTable);
                    Assert.Equal(new[] { "TigerId" }, addOperation.ReferencedColumns);
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
                        x.Key("Id");
                        x.Property<int>("Value");
                    }),
                target => target.Entity(
                    "Hippo",
                    x =>
                    {
                        x.ToTable("Hippo", "dbo");
                        x.Property<int>("Id");
                        x.Key("Id");
                        x.Property<int>("Value");
                        x.Index("Value").Unique();
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
                        x.Key("Id");
                        x.Property<int>("Value");
                        x.Index("Value");
                    }),
                target => target.Entity(
                    "Horse",
                    x =>
                    {
                        x.ToTable("Horse", "dbo");
                        x.Property<int>("Id");
                        x.Key("Id");
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
                        x.Key("Id");
                        x.Property<int>("Value");
                        x.Index("Value");
                    }),
                target => target.Entity(
                    "Donkey",
                    x =>
                    {
                        x.ToTable("Donkey", "dbo");
                        x.Property<int>("Id");
                        x.Key("Id");
                        x.Property<int>("Value");
                        x.Index("Value").IndexName("IX_dbo.Donkey_Value");
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
                        x.Key("Id");
                        x.Property<int>("Value");
                        x.Index("Value");
                        x.Property<int>("MuleValue");
                    }),
                target => target.Entity(
                    "Muel",
                    x =>
                    {
                        x.ToTable("Muel", "dbo");
                        x.Property<int>("Id");
                        x.Key("Id");
                        x.Property<int>("Value");
                        x.Property<int>("MuleValue");
                        x.Index("MuleValue").IndexName("IX_Muel_Value");
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
                        x.Key("Id");
                        x.Property<int>("Value");
                        x.Index("Value").Unique(false);
                    }),
                target => target.Entity(
                    "Pony",
                    x =>
                    {
                        x.ToTable("Pony", "dbo");
                        x.Property<int>("Id");
                        x.Key("Id");
                        x.Property<int>("Value");
                        x.Index("Value").Unique(true);
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
                modelBuilder => modelBuilder.Sequence("Tango", "dbo")
                    .Type<int>()
                    .Start(2)
                    .IncrementBy(3)
                    .Min(1)
                    .Max(4)
                    .Cycle(),
                operations =>
                {
                    Assert.Equal(1, operations.Count);

                    var operation = Assert.IsType<CreateSequenceOperation>(operations[0]);
                    Assert.Equal("Tango", operation.Name);
                    Assert.Equal("dbo", operation.Schema);
                    Assert.Equal("int", operation.Type);
                    Assert.Equal(2, operation.StartWith);
                    Assert.Equal(3, operation.IncrementBy);
                    Assert.Equal(1, operation.MinValue);
                    Assert.Equal(4, operation.MaxValue);
                    Assert.True(operation.Cycle);
                });
        }

        [Fact]
        public void Drop_sequence()
        {
            Execute(
                modelBuilder => modelBuilder.Sequence("Bravo", "dbo"),
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
                source => source.Sequence("Bravo", "dbo"),
                target => target.Sequence("bravo", "dbo"),
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
                source => source.Sequence("Charlie", "dbo"),
                target => target.Sequence("Charlie", "odb"),
                operations =>
                {
                    Assert.Equal(1, operations.Count);

                    var operation = Assert.IsType<RenameSequenceOperation>(operations[0]);
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
                source => source.Sequence("Alpha", "dbo")
                    .Type<int>()
                    .Start(2)
                    .IncrementBy(3)
                    .Min(1)
                    .Max(4)
                    .Cycle(),
                source => source.Sequence("Alpha", "dbo")
                    .Type<int>()
                    .Start(2)
                    .IncrementBy(5)
                    .Min(1)
                    .Max(4)
                    .Cycle(),
                operations =>
                {
                    Assert.Equal(1, operations.Count);

                    var operation = Assert.IsType<AlterSequenceOperation>(operations[0]);
                    Assert.Equal("Alpha", operation.Name);
                    Assert.Equal("dbo", operation.Schema);
                    Assert.Equal(5, operation.IncrementBy);
                    Assert.Equal(1, operation.MinValue);
                    Assert.Equal(4, operation.MaxValue);
                    Assert.True(operation.Cycle);
                });
        }

        [Fact]
        public void Alter_sequence_max_value()
        {
            Execute(
                source => source.Sequence("Echo", "dbo")
                    .Type<int>()
                    .Start(2)
                    .IncrementBy(3)
                    .Min(1)
                    .Max(4)
                    .Cycle(),
                source => source.Sequence("Echo", "dbo")
                    .Type<int>()
                    .Start(2)
                    .IncrementBy(3)
                    .Min(1)
                    .Max(5)
                    .Cycle(),
                operations =>
                {
                    Assert.Equal(1, operations.Count);

                    var operation = Assert.IsType<AlterSequenceOperation>(operations[0]);
                    Assert.Equal("Echo", operation.Name);
                    Assert.Equal("dbo", operation.Schema);
                    Assert.Equal(3, operation.IncrementBy);
                    Assert.Equal(1, operation.MinValue);
                    Assert.Equal(5, operation.MaxValue);
                    Assert.True(operation.Cycle);
                });
        }

        [Fact]
        public void Alter_sequence_min_value()
        {
            Execute(
                source => source.Sequence("Delta", "dbo")
                    .Type<int>()
                    .Start(2)
                    .IncrementBy(3)
                    .Min(1)
                    .Max(4)
                    .Cycle(),
                source => source.Sequence("Delta", "dbo")
                    .Type<int>()
                    .Start(2)
                    .IncrementBy(3)
                    .Min(5)
                    .Max(4)
                    .Cycle(),
                operations =>
                {
                    Assert.Equal(1, operations.Count);

                    var operation = Assert.IsType<AlterSequenceOperation>(operations[0]);
                    Assert.Equal("Delta", operation.Name);
                    Assert.Equal("dbo", operation.Schema);
                    Assert.Equal(3, operation.IncrementBy);
                    Assert.Equal(5, operation.MinValue);
                    Assert.Equal(4, operation.MaxValue);
                    Assert.True(operation.Cycle);
                });
        }

        [Fact]
        public void Alter_sequence_cycle()
        {
            Execute(
                source => source.Sequence("Foxtrot", "dbo")
                    .Type<int>()
                    .Start(2)
                    .IncrementBy(3)
                    .Min(1)
                    .Max(4)
                    .Cycle(true),
                source => source.Sequence("Foxtrot", "dbo")
                    .Type<int>()
                    .Start(2)
                    .IncrementBy(3)
                    .Min(1)
                    .Max(4)
                    .Cycle(false),
                operations =>
                {
                    Assert.Equal(1, operations.Count);

                    var operation = Assert.IsType<AlterSequenceOperation>(operations[0]);
                    Assert.Equal("Foxtrot", operation.Name);
                    Assert.Equal("dbo", operation.Schema);
                    Assert.Equal(3, operation.IncrementBy);
                    Assert.Equal(1, operation.MinValue);
                    Assert.Equal(4, operation.MaxValue);
                    Assert.False(operation.Cycle);
                });
        }

        [Fact]
        public void Alter_sequence_type()
        {
            Execute(
                source => source.Sequence("Hotel", "dbo")
                    .Type<int>()
                    .Start(2)
                    .IncrementBy(3)
                    .Min(1)
                    .Max(4)
                    .Cycle(),
                source => source.Sequence("Hotel", "dbo")
                    .Type<long>()
                    .Start(2)
                    .IncrementBy(3)
                    .Min(1)
                    .Max(4)
                    .Cycle(),
                operations =>
                {
                    Assert.Equal(2, operations.Count);

                    var dropOperation = Assert.IsType<DropSequenceOperation>(operations[0]);
                    Assert.Equal("Hotel", dropOperation.Name);
                    Assert.Equal("dbo", dropOperation.Schema);

                    var createOperation = Assert.IsType<CreateSequenceOperation>(operations[1]);
                    Assert.Equal("Hotel", createOperation.Name);
                    Assert.Equal("dbo", createOperation.Schema);
                    Assert.Equal("bigint", createOperation.Type);
                    Assert.Equal(2, createOperation.StartWith);
                    Assert.Equal(3, createOperation.IncrementBy);
                    Assert.Equal(1, createOperation.MinValue);
                    Assert.Equal(4, createOperation.MaxValue);
                    Assert.True(createOperation.Cycle);
                });
        }

        [Fact]
        public void Alter_sequence_start()
        {
            Execute(
                source => source.Sequence("Golf", "dbo")
                    .Type<int>()
                    .Start(2)
                    .IncrementBy(3)
                    .Min(1)
                    .Max(4)
                    .Cycle(),
                source => source.Sequence("Golf", "dbo")
                    .Type<int>()
                    .Start(5)
                    .IncrementBy(3)
                    .Min(1)
                    .Max(4)
                    .Cycle(),
                operations => Assert.Empty(operations));
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
                        x.Key("Id");
                        x.Property<int?>("Value");
                    }),
                target => target.Entity(
                    "Lizard",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Key("Id");
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
                        x.Key("Id");
                        x.Property<int>("Value");
                    }),
                target => target.Entity(
                    "Frog",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Key("Id");
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
                        x.Key("Id");
                        x.Property<int>("Value");
                    }),
                target => target.Entity(
                    "Frog",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Key("Id");
                        x.Property<int>("Value").HasColumnType("integer");
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
                        x.Key("Id");
                    }),
                target => target.Entity(
                    "Jaguar",
                    x =>
                    {
                        x.Property<string>("Name");
                        x.Key("Name");
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
                        x.Key("Id");
                    }),
                target => target.Entity(
                    "Panther",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Key("Id");
                        x.Property<int>("AlternateId");
                        x.AlternateKey("AlternateId");
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
                        x.Key("Id");
                        x.Property<int>("AlternateId");
                        x.AlternateKey("AlternateId");
                    }),
                target => target.Entity(
                    "Bobcat",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Key("Id");
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
                        x.Key("Id");
                    }),
                target => target.Entity(
                    "Coyote",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Key("Id");
                        x.Property<int>("Value");
                        x.Index("Value");
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
                        x.Key("Id");
                        x.Property<int>("Value");
                        x.Index("Value");
                    }),
                target => target.Entity(
                    "Wolf",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Key("Id");
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
                        x.Key("Id");
                    }),
                target => target.Entity(
                    "Algae",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Key("Id");
                        x.Property<int>("ParentId");
                        x.Reference("Algae").InverseCollection().ForeignKey("ParentId");
                    }),
                operations => Assert.Collection(
                    operations,
                    o => Assert.IsType<AddColumnOperation>(o),
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
                        x.Key("Id");
                        x.Property<int>("ParentId");
                        x.Reference("Bacteria").InverseCollection().ForeignKey("ParentId");
                    }),
                target => target.Entity(
                    "Bacteria",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Key("Id");
                    }),
                operations => Assert.Collection(
                    operations,
                    o => Assert.IsType<DropForeignKeyOperation>(o),
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
                        x.Key("Id");
                        x.Property<int>("MakerId");
                    }),
                target =>
                {
                    target.Entity(
                        "Maker",
                        x =>
                        {
                            x.Property<int>("Id");
                            x.Key("Id");
                        });
                    target.Entity(
                        "Car",
                        x =>
                        {
                            x.Property<int>("Id");
                            x.Key("Id");
                            x.Property<int>("MakerId");
                            x.Reference("Maker").InverseCollection().ForeignKey("MakerId");
                        });
                },
                operations => Assert.Collection(
                    operations,
                    o => Assert.IsType<CreateTableOperation>(o),
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
                            x.Key("Id");
                        });
                    source.Entity(
                        "Boat",
                        x =>
                        {
                            x.Property<int>("Id");
                            x.Key("Id");
                            x.Property<int>("MakerId");
                            x.Reference("Maker").InverseCollection().ForeignKey("MakerId");
                        });
                },
                target => target.Entity(
                    "Boat",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Key("Id");
                        x.Property<int>("MakerId");
                    }),
                operations => Assert.Collection(
                    operations,
                    o => Assert.IsType<DropForeignKeyOperation>(o),
                    o => Assert.IsType<DropTableOperation>(o)));
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
                            x.Key("Id");
                        });
                    source.Entity(
                        "Airplane",
                        x =>
                        {
                            x.Property<int>("Id");
                            x.Key("Id");
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
                            x.Key("Id");
                            x.Property<int>("AlternateId");
                        });
                    target.Entity(
                        "Airplane",
                        x =>
                        {
                            x.Property<int>("Id");
                            x.Key("Id");
                            x.Property<int>("MakerId");
                            x.Reference("Maker").InverseCollection().ForeignKey("MakerId").PrincipalKey("AlternateId");
                        });
                },
                operations => Assert.Collection(
                    operations,
                    o => Assert.IsType<AddColumnOperation>(o),
                    o => Assert.IsType<AddUniqueConstraintOperation>(o),
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
                            x.Key("Id");
                            x.Property<int>("AlternateId");
                        });
                    source.Entity(
                        "Submarine",
                        x =>
                        {
                            x.Property<int>("Id");
                            x.Key("Id");
                            x.Property<int>("MakerId");
                            x.Reference("Maker").InverseCollection().ForeignKey("MakerId").PrincipalKey("AlternateId");
                        });
                },
                target =>
                {
                    target.Entity(
                        "Maker",
                        x =>
                        {
                            x.Property<int>("Id");
                            x.Key("Id");
                        });
                    target.Entity(
                        "Submarine",
                        x =>
                        {
                            x.Property<int>("Id");
                            x.Key("Id");
                            x.Property<int>("MakerId");
                        });
                },
                operations => Assert.Collection(
                    operations,
                    o => Assert.IsType<DropForeignKeyOperation>(o),
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
                            x.Key("Id");
                        });
                    modelBuilder.Entity(
                        "Helicopter",
                        x =>
                        {
                            x.Property<int>("Id");
                            x.Key("Id");
                            x.Property<int>("MakerId");
                            x.Reference("Maker").InverseCollection().ForeignKey("MakerId");
                        });
                },
                operations =>
                {
                    Assert.Equal(2, operations.Count);

                    var operation1 = Assert.IsType<CreateTableOperation>(operations[0]);
                    Assert.Equal("Maker", operation1.Name);

                    var operation2 = Assert.IsType<CreateTableOperation>(operations[1]);
                    Assert.Equal("Helicopter", operation2.Name);
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
                            x.Key("Id");
                        });
                    modelBuilder.Entity(
                        "Glider",
                        x =>
                        {
                            x.Property<int>("Id");
                            x.Key("Id");
                            x.Property<int>("MakerId");
                            x.Reference("Maker").InverseCollection().ForeignKey("MakerId");
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
                        x.Key("Id");
                    }),
                target => target.Entity(
                    "Hornet",
                    x =>
                    {
                        x.Property<int>("Id").HasColumnName("HornetId");
                        x.Key("Id");
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
                        x.Key("Id");
                        x.Property<string>("Name");
                        x.AlternateKey("Name");
                    }),
                target => target.Entity(
                    "Wasp",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Key("Id");
                        x.Property<string>("Name").HasColumnName("WaspName");
                        x.AlternateKey("Name");
                    }),
                operations =>
                {
                    Assert.Equal(1, operations.Count);

                    Assert.IsType<RenameColumnOperation>(operations[0]);
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
                        x.Key("Id");
                        x.Property<string>("Name");
                        x.Index("Name");
                    }),
                target => target.Entity(
                    "Bee",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Key("Id");
                        x.Property<string>("Name").HasColumnName("BeeName");
                        x.Index("Name");
                    }),
                operations =>
                {
                    Assert.Equal(1, operations.Count);

                    Assert.IsType<RenameColumnOperation>(operations[0]);
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
                        x.Key("Id");
                        x.Property<string>("Name");
                        x.AlternateKey("Name");
                    }),
                target => target.Entity(
                    "Fly",
                    x =>
                    {
                        x.ToTable("Flies");
                        x.Property<int>("Id");
                        x.Key("Id");
                        x.Property<string>("Name");
                        x.AlternateKey("Name");
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
                        x.Key("Id");
                        x.Property<string>("Name");
                        x.Index("Name");
                    }),
                target => target.Entity(
                    "Gnat",
                    x =>
                    {
                        x.ToTable("Gnats");
                        x.Property<int>("Id");
                        x.Key("Id");
                        x.Property<string>("Name");
                        x.Index("Name");
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
                        x.Key("Id");
                        x.Property<string>("Name");
                        x.AlternateKey("Name");
                    }),
                target => target.Entity(
                    "grasshopper",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Key("Id").KeyName("PK_Grasshopper");
                        x.Property<string>("Name");
                        x.AlternateKey("Name").KeyName("AK_Grasshopper_Name");
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
                        x.Key("Id");
                        x.Property<string>("Name");
                        x.Index("Name");
                    }),
                target => target.Entity(
                    "cricket",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Key("Id").KeyName("PK_Cricket");
                        x.Property<string>("Name");
                        x.Index("Name").IndexName("IX_Cricket_Name");
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
                        x.Key("Id");
                        x.Property<int>("ParentId");
                        x.Reference("Yeast").InverseCollection().ForeignKey("ParentId");
                    }),
                target => target.Entity(
                    "Yeast",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Key("Id");
                        x.Property<int>("ParentId").HasColumnName("ParentYeastId");
                        x.Reference("Yeast").InverseCollection().ForeignKey("ParentId");
                    }),
                operations =>
                {
                    Assert.Equal(1, operations.Count);
                    Assert.IsType<RenameColumnOperation>(operations[0]);
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
                        x.Key("Id");
                        x.Property<int>("ParentId");
                        x.Reference("Mucor").InverseCollection().ForeignKey("ParentId");
                    }),
                target => target.Entity(
                    "Mucor",
                    x =>
                    {
                        x.Property<int>("Id").HasColumnName("MucorId");
                        x.Key("Id");
                        x.Property<int>("ParentId");
                        x.Reference("Mucor").InverseCollection().ForeignKey("ParentId");
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
                            x.Key("Id");
                        });
                    source.Entity(
                        "Zonkey",
                        x =>
                        {
                            x.Property<int>("Id");
                            x.Key("Id");
                            x.Property<int>("ParentId");
                            x.Reference("Zebra").InverseCollection().ForeignKey("ParentId");
                        });
                },
                target =>
                {
                    target.Entity(
                        "Zebra",
                        x =>
                        {
                            x.Property<int>("Id");
                            x.Key("Id");
                        });
                    target.Entity(
                        "Zonkey",
                        x =>
                        {
                            x.ToTable("Zonkeys");
                            x.Property<int>("Id");
                            x.Key("Id");
                            x.Property<int>("ParentId");
                            x.Reference("Zebra").InverseCollection().ForeignKey("ParentId");
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
                            x.Key("Id");
                        });
                    source.Entity(
                        "Jaglion",
                        x =>
                        {
                            x.Property<int>("Id");
                            x.Key("Id");
                            x.Property<int>("ParentId");
                            x.Reference("Jaguar").InverseCollection().ForeignKey("ParentId");
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
                            x.Key("Id");
                        });
                    target.Entity(
                        "Jaglion",
                        x =>
                        {
                            x.Property<int>("Id");
                            x.Key("Id");
                            x.Property<int>("ParentId");
                            x.Reference("Jaguar").InverseCollection().ForeignKey("ParentId");
                        });
                },
                operations =>
                {
                    Assert.Equal(1, operations.Count);
                    Assert.IsType<RenameTableOperation>(operations[0]);
                });
        }
    }
}

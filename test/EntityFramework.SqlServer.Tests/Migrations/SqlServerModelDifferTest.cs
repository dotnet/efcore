// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Operations;
using Microsoft.Data.Entity.Tests;
using Xunit;

namespace Microsoft.Data.Entity.Migrations.Internal
{
    public class SqlServerModelDifferTest : MigrationsModelDifferTestBase
    {
        [Fact]
        public void Create_table_overridden()
        {
            Execute(
                _ => { },
                modelBuilder => modelBuilder.Entity(
                    "Person",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id").SqlServerKeyName("PK_People");
                            x.ToSqlServerTable("People", "dbo");
                        }),
                operations =>
                    {
                        Assert.Equal(2, operations.Count);

                        var createSchemaOperation = Assert.IsType<EnsureSchemaOperation>(operations[0]);
                        Assert.Equal("dbo", createSchemaOperation.Name);

                        var addTableOperation = Assert.IsType<CreateTableOperation>(operations[1]);
                        Assert.Equal("dbo", addTableOperation.Schema);
                        Assert.Equal("People", addTableOperation.Name);

                        Assert.Equal("PK_People", addTableOperation.PrimaryKey.Name);
                    });
        }

        [Fact]
        public void Rename_table_overridden()
        {
            Execute(
                source => source.Entity(
                    "Person",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                        }),
                target => target.Entity(
                    "Person",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.ToSqlServerTable("People", "dbo");
                        }),
                operations =>
                    {
                        Assert.Equal(2, operations.Count);

                        var createSchemaOperation = Assert.IsType<EnsureSchemaOperation>(operations[0]);
                        Assert.Equal("dbo", createSchemaOperation.Name);

                        var addTableOperation = Assert.IsType<RenameTableOperation>(operations[1]);
                        Assert.Equal("Person", addTableOperation.Name);
                        Assert.Equal("dbo", addTableOperation.NewSchema);
                        Assert.Equal("People", addTableOperation.NewName);
                    });
        }

        [Fact]
        public void Drop_table_overridden()
        {
            Execute(
                modelBuilder => modelBuilder.Entity(
                    "Person",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.ToSqlServerTable("People", "dbo");
                        }),
                _ => { },
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var addTableOperation = Assert.IsType<DropTableOperation>(operations[0]);
                        Assert.Equal("dbo", addTableOperation.Schema);
                        Assert.Equal("People", addTableOperation.Name);
                    });
        }

        [Fact]
        public void Rename_column_overridden()
        {
            Execute(
                source => source.Entity(
                    "Person",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("Value");
                        }),
                target => target.Entity(
                    "Person",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("Value").HasSqlServerColumnName("PersonValue");
                        }),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var addTableOperation = Assert.IsType<RenameColumnOperation>(operations[0]);
                        Assert.Equal("Person", addTableOperation.Table);
                        Assert.Equal("Value", addTableOperation.Name);
                        Assert.Equal("PersonValue", addTableOperation.NewName);
                    });
        }

        [Fact]
        public void Alter_column_overridden()
        {
            Execute(
                source => source.Entity(
                    "Person",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("Value");
                        }),
                target => target.Entity(
                    "Person",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("Value")
                                .HasSqlServerColumnType("varchar(8000)")
                                .HasDefaultValueSql("1 + 1");
                        }),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var addTableOperation = Assert.IsType<AlterColumnOperation>(operations[0]);
                        Assert.Equal("Person", addTableOperation.Table);
                        Assert.Equal("Value", addTableOperation.Name);
                        Assert.Equal("varchar(8000)", addTableOperation.ColumnType);
                        Assert.Equal("1 + 1", addTableOperation.DefaultValueSql);
                    });
        }

        [Fact]
        public void Alter_column_identity()
        {
            Execute(
                source => source.Entity(
                    "Lamb",
                    x =>
                        {
                            x.ToTable("Lamb", "bah");
                            x.Property<int>("Id").ValueGeneratedNever();
                            x.HasKey("Id");
                        }),
                target => target.Entity(
                    "Lamb",
                    x =>
                        {
                            x.ToTable("Lamb", "bah");
                            x.Property<int>("Id").ValueGeneratedOnAdd();
                            x.HasKey("Id");
                        }),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<AlterColumnOperation>(operations[0]);
                        Assert.Equal("bah", operation.Schema);
                        Assert.Equal("Lamb", operation.Table);
                        Assert.Equal("Id", operation.Name);
                        Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn, operation["SqlServer:ValueGenerationStrategy"]);
                    });
        }

        [Fact]
        public void Alter_column_computation()
        {
            Execute(
                source => source.Entity(
                    "Sheep",
                    x =>
                        {
                            x.ToTable("Sheep", "bah");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("Now");
                        }),
                target => target.Entity(
                    "Sheep",
                    x =>
                        {
                            x.ToTable("Sheep", "bah");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("Now").HasSqlServerComputedColumnSql("CAST(CURRENT_TIMESTAMP AS int)");
                        }),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<AlterColumnOperation>(operations[0]);
                        Assert.Equal("bah", operation.Schema);
                        Assert.Equal("Sheep", operation.Table);
                        Assert.Equal("Now", operation.Name);
                        Assert.Equal("CAST(CURRENT_TIMESTAMP AS int)", operation.ComputedColumnSql);
                    });
        }

        [Fact]
        public void Add_column_overridden()
        {
            Execute(
                source => source.Entity(
                    "Person",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                        }),
                target => target.Entity(
                    "Person",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("Value").HasSqlServerColumnName("PersonValue");
                        }),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var addTableOperation = Assert.IsType<AddColumnOperation>(operations[0]);
                        Assert.Equal("Person", addTableOperation.Table);
                        Assert.Equal("PersonValue", addTableOperation.Name);
                    });
        }

        [Fact]
        public void Drop_column_overridden()
        {
            Execute(
                source => source.Entity(
                    "Person",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("Value").HasSqlServerColumnName("PersonValue");
                        }),
                target => target.Entity(
                    "Person",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                        }),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var addTableOperation = Assert.IsType<DropColumnOperation>(operations[0]);
                        Assert.Equal("Person", addTableOperation.Table);
                        Assert.Equal("PersonValue", addTableOperation.Name);
                    });
        }

        [Fact]
        public void Alter_primary_key_clustering()
        {
            Execute(
                source => source.Entity(
                    "Ram",
                    x =>
                        {
                            x.ToTable("Ram", "bah");
                            x.Property<int>("Id");
                            x.HasKey("Id").SqlServerClustered(false);
                        }),
                target => target.Entity(
                    "Ram",
                    x =>
                        {
                            x.ToTable("Ram", "bah");
                            x.Property<int>("Id");
                            x.HasKey("Id").SqlServerClustered(true);
                        }),
                operations =>
                    {
                        Assert.Equal(2, operations.Count);

                        var dropOperation = Assert.IsType<DropPrimaryKeyOperation>(operations[0]);
                        Assert.Equal("bah", dropOperation.Schema);
                        Assert.Equal("Ram", dropOperation.Table);
                        Assert.Equal("PK_Ram", dropOperation.Name);

                        var addOperation = Assert.IsType<AddPrimaryKeyOperation>(operations[1]);
                        Assert.Equal("bah", addOperation.Schema);
                        Assert.Equal("Ram", addOperation.Table);
                        Assert.Equal("PK_Ram", addOperation.Name);
                        Assert.True((bool)addOperation["SqlServer:Clustered"]);
                    });
        }

        [Fact]
        public void Alter_unique_constraint_clustering()
        {
            Execute(
                source => source.Entity(
                    "Ewe",
                    x =>
                        {
                            x.ToTable("Ewe", "bah");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("AlternateId");
                            x.HasAlternateKey("AlternateId").SqlServerClustered(false);
                        }),
                target => target.Entity(
                    "Ewe",
                    x =>
                        {
                            x.ToTable("Ewe", "bah");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("AlternateId");
                            x.HasAlternateKey("AlternateId").SqlServerClustered(true);
                        }),
                operations =>
                    {
                        Assert.Equal(2, operations.Count);

                        var dropOperation = Assert.IsType<DropUniqueConstraintOperation>(operations[0]);
                        Assert.Equal("bah", dropOperation.Schema);
                        Assert.Equal("Ewe", dropOperation.Table);
                        Assert.Equal("AK_Ewe_AlternateId", dropOperation.Name);

                        var addOperation = Assert.IsType<AddUniqueConstraintOperation>(operations[1]);
                        Assert.Equal("bah", addOperation.Schema);
                        Assert.Equal("Ewe", addOperation.Table);
                        Assert.Equal("AK_Ewe_AlternateId", addOperation.Name);
                        Assert.True((bool)addOperation["SqlServer:Clustered"]);
                    });
        }

        [Fact]
        public void Add_unique_constraint_overridden()
        {
            Execute(
                source => source.Entity(
                    "Ewe",
                    x =>
                        {
                            x.ToTable("Ewe", "bah");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("AlternateId");
                        }),
                target => target.Entity(
                    "Ewe",
                    x =>
                        {
                            x.ToTable("Ewe", "bah");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("AlternateId");
                            x.HasAlternateKey("AlternateId").SqlServerKeyName("AK_Ewe");
                        }),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<AddUniqueConstraintOperation>(operations[0]);
                        Assert.Equal("bah", operation.Schema);
                        Assert.Equal("Ewe", operation.Table);
                        Assert.Equal("AK_Ewe", operation.Name);
                    });
        }

        [Fact]
        public void Drop_unique_constraint_overridden()
        {
            Execute(
                source => source.Entity(
                    "Ewe",
                    x =>
                        {
                            x.ToTable("Ewe", "bah");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("AlternateId");
                            x.HasAlternateKey("AlternateId").SqlServerKeyName("AK_Ewe");
                        }),
                target => target.Entity(
                    "Ewe",
                    x =>
                        {
                            x.ToTable("Ewe", "bah");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("AlternateId");
                        }),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<DropUniqueConstraintOperation>(operations[0]);
                        Assert.Equal("bah", operation.Schema);
                        Assert.Equal("Ewe", operation.Table);
                        Assert.Equal("AK_Ewe", operation.Name);
                    });
        }

        [Fact]
        public void Add_foreign_key_overridden()
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
                            x.HasOne("Amoeba").WithMany().ForeignKey("ParentId").SqlServerConstraintName("FK_Amoeba_Parent");
                        }),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<AddForeignKeyOperation>(operations[0]);
                        Assert.Equal("dbo", operation.Schema);
                        Assert.Equal("Amoeba", operation.Table);
                        Assert.Equal("FK_Amoeba_Parent", operation.Name);
                    });
        }

        [Fact]
        public void Drop_foreign_key_overridden()
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
                            x.HasOne("Anemone").WithMany().ForeignKey("ParentId").SqlServerConstraintName("FK_Anemone_Parent");
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
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<DropForeignKeyOperation>(operations[0]);
                        Assert.Equal("dbo", operation.Schema);
                        Assert.Equal("Anemone", operation.Table);
                        Assert.Equal("FK_Anemone_Parent", operation.Name);
                    });
        }

        [Fact]
        public void Alter_index_clustering()
        {
            Execute(
                source => source.Entity(
                    "Mutton",
                    x =>
                        {
                            x.ToTable("Mutton", "bah");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("Value");
                            x.Index("Value").SqlServerClustered(false);
                        }),
                target => target.Entity(
                    "Mutton",
                    x =>
                        {
                            x.ToTable("Mutton", "bah");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("Value");
                            x.Index("Value").SqlServerClustered(true);
                        }),
                operations =>
                    {
                        Assert.Equal(2, operations.Count);

                        var dropOperation = Assert.IsType<DropIndexOperation>(operations[0]);
                        Assert.Equal("bah", dropOperation.Schema);
                        Assert.Equal("Mutton", dropOperation.Table);
                        Assert.Equal("IX_Mutton_Value", dropOperation.Name);

                        var createOperation = Assert.IsType<CreateIndexOperation>(operations[1]);
                        Assert.Equal("bah", createOperation.Schema);
                        Assert.Equal("Mutton", createOperation.Table);
                        Assert.Equal("IX_Mutton_Value", createOperation.Name);
                        Assert.True((bool)createOperation["SqlServer:Clustered"]);
                    });
        }

        [Fact]
        public void Rename_index_overridden()
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
                            x.Index("Value");
                        }),
                target => target.Entity(
                    "Donkey",
                    x =>
                        {
                            x.ToTable("Donkey", "dbo");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("Value");
                            x.Index("Value").SqlServerIndexName("IX_dbo.Donkey_Value");
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
        public void Create_index_overridden()
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
                            x.Index("Value").SqlServerIndexName("IX_HipVal");
                        }),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<CreateIndexOperation>(operations[0]);
                        Assert.Equal("dbo", operation.Schema);
                        Assert.Equal("Hippo", operation.Table);
                        Assert.Equal("IX_HipVal", operation.Name);
                    });
        }

        [Fact]
        public void Drop_index_overridden()
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
                            x.Index("Value").SqlServerIndexName("IX_HorseVal");
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
                        Assert.Equal("IX_HorseVal", operation.Name);
                    });
        }

        [Fact]
        public void Create_sequence_overridden()
        {
            Execute(
                _ => { },
                modelBuilder => modelBuilder.SqlServerSequence("Tango", "dbo"),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<CreateSequenceOperation>(operations[0]);
                        Assert.Equal("Tango", operation.Name);
                        Assert.Equal("dbo", operation.Schema);
                    });
        }

        [Fact]
        public void Drop_sequence_overridden()
        {
            Execute(
                modelBuilder => modelBuilder.SqlServerSequence("Bravo", "dbo"),
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
        public void Alter_sequence_overridden()
        {
            Execute(
                source => source.Sequence("Bravo"),
                target => target.SqlServerSequence("Bravo").IncrementsBy(2),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<AlterSequenceOperation>(operations[0]);
                        Assert.Equal("Bravo", operation.Name);
                        Assert.Equal(2, operation.IncrementBy);
                    });
        }

        protected override ModelBuilder CreateModelBuilder() => SqlServerTestHelpers.Instance.CreateConventionBuilder();

        protected override MigrationsModelDiffer CreateModelDiffer()
            => new MigrationsModelDiffer(
                new SqlServerAnnotationProvider(),
                new SqlServerMigrationsAnnotationProvider());
    }
}

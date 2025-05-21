// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Migrations;

public partial class MigrationsSqlServerTest : MigrationsTestBase<MigrationsSqlServerTest.MigrationsSqlServerFixture>
{
    #region basic operations with explicit name

    [ConditionalFact]
    public virtual async Task Named_default_constraints_add_column_with_explicit_name()
    {
        await Test(
            builder => builder.Entity("Entity").Property<string>("Id"),
            builder => { },
            builder =>
            {
                builder.Entity("Entity").Property<int>("Number").HasDefaultValue(7, defaultConstraintName: "MyConstraint");
                builder.Entity("Entity").Property<Guid>("Guid").HasDefaultValueSql("NEWID()", defaultConstraintName: "MyConstraintSql");
            },
            model =>
            {
                var table = Assert.Single(model.Tables);
                var number = Assert.Single(table.Columns, c => c.Name == "Number");
                Assert.Equal(7, number.DefaultValue);
                Assert.Equal("MyConstraint", number[RelationalAnnotationNames.DefaultConstraintName]);

                var guid = Assert.Single(table.Columns, c => c.Name == "Guid");
                Assert.Equal("(newid())", guid.DefaultValueSql);
                Assert.Equal("MyConstraintSql", guid[RelationalAnnotationNames.DefaultConstraintName]);
            });

        AssertSql(
"""
ALTER TABLE [Entity] ADD [Guid] uniqueidentifier NOT NULL CONSTRAINT [MyConstraintSql] DEFAULT (NEWID());
""",
                //
                """
ALTER TABLE [Entity] ADD [Number] int NOT NULL CONSTRAINT [MyConstraint] DEFAULT 7;
""");
    }

    [ConditionalFact]
    public virtual async Task Named_default_constraints_drop_column_with_explicit_name()
    {
        await Test(
            builder => builder.Entity("Entity").Property<string>("Id"),
            builder =>
            {
                builder.Entity("Entity").Property<int>("Number").HasDefaultValue(7, defaultConstraintName: "MyConstraint");
                builder.Entity("Entity").Property<Guid>("Guid").HasDefaultValueSql("NEWID()", defaultConstraintName: "MyConstraintSql");
            },
            builder => { },
            model =>
            {
                var table = Assert.Single(model.Tables);
                var column = Assert.Single(table.Columns);
            });

        AssertSql(
"""
ALTER TABLE [Entity] DROP CONSTRAINT [MyConstraintSql];
ALTER TABLE [Entity] DROP COLUMN [Guid];
""",
                //
                """
ALTER TABLE [Entity] DROP CONSTRAINT [MyConstraint];
ALTER TABLE [Entity] DROP COLUMN [Number];
""");
    }

    [ConditionalFact]
    public virtual async Task Named_default_constraints_create_table_with_column_with_explicit_name()
    {
        await Test(
            builder => { },
            builder =>
            {
                builder.Entity("Entity").Property<string>("Id");
                builder.Entity("Entity").Property<int>("Number").HasDefaultValue(7, defaultConstraintName: "MyConstraint");
                builder.Entity("Entity").Property<Guid>("Guid").HasDefaultValueSql("NEWID()", defaultConstraintName: "MyConstraintSql");
            },
            model =>
            {
                var table = Assert.Single(model.Tables);
                var number = Assert.Single(table.Columns, c => c.Name == "Number");
                Assert.Equal(7, number.DefaultValue);
                Assert.Equal("MyConstraint", number[RelationalAnnotationNames.DefaultConstraintName]);
                var guid = Assert.Single(table.Columns, c => c.Name == "Guid");
                Assert.Equal("(newid())", guid.DefaultValueSql);
                Assert.Equal("MyConstraintSql", guid[RelationalAnnotationNames.DefaultConstraintName]);
            });

        AssertSql(
"""
CREATE TABLE [Entity] (
    [Id] nvarchar(450) NOT NULL,
    [Guid] uniqueidentifier NOT NULL CONSTRAINT [MyConstraintSql] DEFAULT (NEWID()),
    [Number] int NOT NULL CONSTRAINT [MyConstraint] DEFAULT 7,
    CONSTRAINT [PK_Entity] PRIMARY KEY ([Id])
);
""");
    }

    [ConditionalFact]
    public virtual async Task Named_default_constraints_drop_table_with_column_with_explicit_name()
    {
        await Test(
            builder =>
            {
                builder.Entity("Entity").Property<string>("Id");
                builder.Entity("Entity").Property<int>("Number").HasDefaultValue(7, defaultConstraintName: "MyConstraint");
                builder.Entity("Entity").Property<Guid>("Guid").HasDefaultValueSql("NEWID()", defaultConstraintName: "MyConstraintSql");
            },
            builder => { },
            model =>
            {
                Assert.Empty(model.Tables);
            });

        AssertSql(
"""
DROP TABLE [Entity];
""");
    }

    [ConditionalFact]
    public virtual async Task Named_default_constraints_rename_constraint()
    {
        await Test(
            builder => builder.Entity("Entity").Property<string>("Id"),
            builder =>
            {
                builder.Entity("Entity").Property<int>("Number").HasDefaultValue(7, defaultConstraintName: "MyConstraint");
                builder.Entity("Entity").Property<Guid>("Guid").HasDefaultValueSql("NEWID()", defaultConstraintName: "MyConstraintSql");
            },
            builder =>
            {
                builder.Entity("Entity").Property<int>("Number").HasDefaultValue(7, defaultConstraintName: "RenamedConstraint");
                builder.Entity("Entity").Property<Guid>("Guid").HasDefaultValueSql("NEWID()", defaultConstraintName: "RenamedConstraintSql");
            },
            model =>
            {
                var table = Assert.Single(model.Tables);
                var number = Assert.Single(table.Columns, c => c.Name == "Number");
                Assert.Equal(7, number.DefaultValue);
                Assert.Equal("RenamedConstraint", number[RelationalAnnotationNames.DefaultConstraintName]);
                var guid = Assert.Single(table.Columns, c => c.Name == "Guid");
                Assert.Equal("(newid())", guid.DefaultValueSql);
                Assert.Equal("RenamedConstraintSql", guid[RelationalAnnotationNames.DefaultConstraintName]);
            });

        AssertSql(
"""
ALTER TABLE [Entity] DROP CONSTRAINT [MyConstraint];
ALTER TABLE [Entity] ALTER COLUMN [Number] int NOT NULL;
ALTER TABLE [Entity] ADD CONSTRAINT [RenamedConstraint] DEFAULT 7 FOR [Number];
""",
                //
                """
ALTER TABLE [Entity] DROP CONSTRAINT [MyConstraintSql];
ALTER TABLE [Entity] ALTER COLUMN [Guid] uniqueidentifier NOT NULL;
ALTER TABLE [Entity] ADD CONSTRAINT [RenamedConstraintSql] DEFAULT (NEWID()) FOR [Guid];
""");
    }

    [ConditionalFact]
    public virtual async Task Named_default_constraints_add_explicit_constraint_name()
    {
        await Test(
            builder => builder.Entity("Entity").Property<string>("Id"),
            builder =>
            {
                builder.Entity("Entity").Property<int>("Number").HasDefaultValue(7);
                builder.Entity("Entity").Property<Guid>("Guid").HasDefaultValueSql("NEWID()");
            },
            builder =>
            {
                builder.Entity("Entity").Property<int>("Number").HasDefaultValue(7, defaultConstraintName: "MyConstraint");
                builder.Entity("Entity").Property<Guid>("Guid").HasDefaultValueSql("NEWID()", defaultConstraintName: "MyConstraintSql");
            },
            model =>
            {
                var table = Assert.Single(model.Tables);
                var number = Assert.Single(table.Columns, c => c.Name == "Number");
                Assert.Equal(7, number.DefaultValue);
                Assert.Equal("MyConstraint", number[RelationalAnnotationNames.DefaultConstraintName]);
                var guid = Assert.Single(table.Columns, c => c.Name == "Guid");
                Assert.Equal("(newid())", guid.DefaultValueSql);
                Assert.Equal("MyConstraintSql", guid[RelationalAnnotationNames.DefaultConstraintName]);
            });

        AssertSql(
"""
DECLARE @var sysname;
SELECT @var = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Entity]') AND [c].[name] = N'Number');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [Entity] DROP CONSTRAINT [' + @var + '];');
ALTER TABLE [Entity] ALTER COLUMN [Number] int NOT NULL;
ALTER TABLE [Entity] ADD CONSTRAINT [MyConstraint] DEFAULT 7 FOR [Number];
""",
                //
                """
DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Entity]') AND [c].[name] = N'Guid');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Entity] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [Entity] ALTER COLUMN [Guid] uniqueidentifier NOT NULL;
ALTER TABLE [Entity] ADD CONSTRAINT [MyConstraintSql] DEFAULT (NEWID()) FOR [Guid];
""");
    }

    [ConditionalFact]
    public virtual async Task Named_default_constraints_remove_explicit_constraint_name()
    {
        await Test(
            builder => builder.Entity("Entity").Property<string>("Id"),
            builder =>
            {
                builder.Entity("Entity").Property<int>("Number").HasDefaultValue(7, defaultConstraintName: "MyConstraint");
                builder.Entity("Entity").Property<Guid>("Guid").HasDefaultValueSql("NEWID()", defaultConstraintName: "MyConstraintSql");
            },
            builder =>
            {
                builder.Entity("Entity").Property<int>("Number").HasDefaultValue(7);
                builder.Entity("Entity").Property<Guid>("Guid").HasDefaultValueSql("NEWID()");
            },
            model =>
            {
                var table = Assert.Single(model.Tables);
                var number = Assert.Single(table.Columns, c => c.Name == "Number");
                Assert.Equal(7, number.DefaultValue);
                Assert.Null(number[RelationalAnnotationNames.DefaultConstraintName]);
                var guid = Assert.Single(table.Columns, c => c.Name == "Guid");
                Assert.Equal("(newid())", guid.DefaultValueSql);
                Assert.Null(guid[RelationalAnnotationNames.DefaultConstraintName]);
            });

        AssertSql(
"""
ALTER TABLE [Entity] DROP CONSTRAINT [MyConstraint];
ALTER TABLE [Entity] ALTER COLUMN [Number] int NOT NULL;
ALTER TABLE [Entity] ADD DEFAULT 7 FOR [Number];
""",
                //
                """
ALTER TABLE [Entity] DROP CONSTRAINT [MyConstraintSql];
ALTER TABLE [Entity] ALTER COLUMN [Guid] uniqueidentifier NOT NULL;
ALTER TABLE [Entity] ADD DEFAULT (NEWID()) FOR [Guid];
""");
    }

    [ConditionalFact]
    public virtual async Task Named_default_constraints_add_column_with_implicit_name_on_nested_owned()
    {
        await Test(
            builder =>
            {
                builder.UseNamedDefaultConstraints();
                builder.Entity("Entity").Property<string>("Id");
                builder.Entity("Entity").OwnsOne("OwnedType", "MyOwned", b =>
                {
                    b.OwnsOne("NestedType", "MyNested", bb =>
                    {
                        bb.Property<int>("Foo");
                    });
                });
            },
            builder => { },
            builder =>
            {
                builder.Entity("Entity").OwnsOne("OwnedType", "MyOwned", b =>
                {
                    b.OwnsOne("NestedType", "MyNested", bb =>
                    {
                        bb.Property<int>("Number").HasDefaultValue(7);
                        bb.Property<Guid>("Guid").HasDefaultValueSql("NEWID()");
                    });
                });
            },
            model =>
            {
                var table = Assert.Single(model.Tables);
                var number = Assert.Single(table.Columns, c => c.Name == "MyOwned_MyNested_Number");
                Assert.Equal(7, number.DefaultValue);
                Assert.Equal("DF_Entity_MyOwned_MyNested_Number", number[RelationalAnnotationNames.DefaultConstraintName]);
                var guid = Assert.Single(table.Columns, c => c.Name == "MyOwned_MyNested_Guid");
                Assert.Equal("(newid())", guid.DefaultValueSql);
                Assert.Equal("DF_Entity_MyOwned_MyNested_Guid", guid[RelationalAnnotationNames.DefaultConstraintName]);
            });

        AssertSql(
"""
ALTER TABLE [Entity] ADD [MyOwned_MyNested_Guid] uniqueidentifier NULL CONSTRAINT [DF_Entity_MyOwned_MyNested_Guid] DEFAULT (NEWID());
""",
                //
                """
ALTER TABLE [Entity] ADD [MyOwned_MyNested_Number] int NULL CONSTRAINT [DF_Entity_MyOwned_MyNested_Number] DEFAULT 7;
""");
    }

    [ConditionalFact]
    public virtual async Task Named_default_constraints_add_column_with_explicit_name_and_null_value()
    {
        await Test(
            builder => builder.Entity("Entity").Property<string>("Id"),
            builder => { },
            builder =>
            {
                builder.Entity("Entity").Property<int?>("Number").HasDefaultValue(null);
                builder.Entity("Entity").Property<Guid>("Guid").HasDefaultValueSql(null);
                builder.Entity("Entity").Property<int?>("NumberNamed").HasDefaultValue(null, defaultConstraintName: "MyConstraint");
                builder.Entity("Entity").Property<Guid>("GuidNamed").HasDefaultValueSql(null, defaultConstraintName: "MyConstraintSql");
            },
            model =>
            {
                var table = Assert.Single(model.Tables);
                var number = Assert.Single(table.Columns, c => c.Name == "NumberNamed");
                Assert.Null(number.DefaultValue);
                Assert.Null(number[RelationalAnnotationNames.DefaultConstraintName]);

                var guid = Assert.Single(table.Columns, c => c.Name == "GuidNamed");
                Assert.Equal("('00000000-0000-0000-0000-000000000000')", guid.DefaultValueSql);
                Assert.Equal("MyConstraintSql", guid[RelationalAnnotationNames.DefaultConstraintName]);
            });

        AssertSql(
"""
ALTER TABLE [Entity] ADD [Guid] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
""",
                //
                """
ALTER TABLE [Entity] ADD [GuidNamed] uniqueidentifier NOT NULL CONSTRAINT [MyConstraintSql] DEFAULT '00000000-0000-0000-0000-000000000000';
""",
                //
                """
ALTER TABLE [Entity] ADD [Number] int NULL;
""",
                //
                """
ALTER TABLE [Entity] ADD [NumberNamed] int NULL;
""");
    }

    #endregion

    #region basic operations with implicit name

    [ConditionalFact]
    public virtual async Task Named_default_constraints_with_opt_in_add_column_with_implicit_constraint_name()
    {
        await Test(
            builder =>
            {
                builder.UseNamedDefaultConstraints();
                builder.Entity("Entity").Property<string>("Id");
            },
            builder => { },
            builder =>
            {
                builder.Entity("Entity").Property<int>("Number").HasDefaultValue(7);
                builder.Entity("Entity").Property<Guid>("Guid").HasDefaultValueSql("NEWID()");
            },
            model =>
            {
                var table = Assert.Single(model.Tables);
                var number = Assert.Single(table.Columns, c => c.Name == "Number");
                Assert.Equal(7, number.DefaultValue);
                Assert.Equal("DF_Entity_Number", number[RelationalAnnotationNames.DefaultConstraintName]);

                var guid = Assert.Single(table.Columns, c => c.Name == "Guid");
                Assert.Equal("(newid())", guid.DefaultValueSql);
                Assert.Equal("DF_Entity_Guid", guid[RelationalAnnotationNames.DefaultConstraintName]);
            });

        AssertSql(
"""
ALTER TABLE [Entity] ADD [Guid] uniqueidentifier NOT NULL CONSTRAINT [DF_Entity_Guid] DEFAULT (NEWID());
""",
                //
                """
ALTER TABLE [Entity] ADD [Number] int NOT NULL CONSTRAINT [DF_Entity_Number] DEFAULT 7;
""");
    }

    [ConditionalFact]
    public virtual async Task Named_default_constraints_with_opt_in_drop_column_with_implicit_constraint_name()
    {
        await Test(
            builder =>
            {
                builder.UseNamedDefaultConstraints();
                builder.Entity("Entity").Property<string>("Id");
            },
            builder =>
            {
                builder.Entity("Entity").Property<int>("Number").HasDefaultValue(7);
                builder.Entity("Entity").Property<Guid>("Guid").HasDefaultValueSql("NEWID()");
            },
            builder => { },
            model =>
            {
                var table = Assert.Single(model.Tables);
                var column = Assert.Single(table.Columns);
            });

        AssertSql(
"""
ALTER TABLE [Entity] DROP CONSTRAINT [DF_Entity_Guid];
ALTER TABLE [Entity] DROP COLUMN [Guid];
""",
                //
                """
ALTER TABLE [Entity] DROP CONSTRAINT [DF_Entity_Number];
ALTER TABLE [Entity] DROP COLUMN [Number];
""");
    }

    [ConditionalFact]
    public virtual async Task Named_default_constraints_with_opt_in_create_table_with_column_with_implicit_constraint_name()
    {
        await Test(
            builder => builder.UseNamedDefaultConstraints(),
            builder => { },
            builder =>
            {
                builder.Entity("Entity").Property<string>("Id");
                builder.Entity("Entity").Property<int>("Number").HasDefaultValue(7);
                builder.Entity("Entity").Property<Guid>("Guid").HasDefaultValueSql("NEWID()");
            },
            model =>
            {
                var table = Assert.Single(model.Tables);
                var number = Assert.Single(table.Columns, c => c.Name == "Number");
                Assert.Equal(7, number.DefaultValue);
                Assert.Equal("DF_Entity_Number", number[RelationalAnnotationNames.DefaultConstraintName]);

                var guid = Assert.Single(table.Columns, c => c.Name == "Guid");
                Assert.Equal("(newid())", guid.DefaultValueSql);
                Assert.Equal("DF_Entity_Guid", guid[RelationalAnnotationNames.DefaultConstraintName]);
            });

        AssertSql(
"""
CREATE TABLE [Entity] (
    [Id] nvarchar(450) NOT NULL,
    [Guid] uniqueidentifier NOT NULL CONSTRAINT [DF_Entity_Guid] DEFAULT (NEWID()),
    [Number] int NOT NULL CONSTRAINT [DF_Entity_Number] DEFAULT 7,
    CONSTRAINT [PK_Entity] PRIMARY KEY ([Id])
);
""");
    }

    [ConditionalFact]
    public virtual async Task Named_default_constraints_with_opt_in_drop_table_with_column_with_implicit_constraint_name()
    {
        await Test(
            builder => builder.UseNamedDefaultConstraints(),
            builder =>
            {
                builder.Entity("Entity").Property<string>("Id");
                builder.Entity("Entity").Property<int>("Number").HasDefaultValue(7);
                builder.Entity("Entity").Property<Guid>("Guid").HasDefaultValueSql("NEWID()");
            },
            builder => { },
            model =>
            {
                Assert.Empty(model.Tables);
            });

        AssertSql(
"""
DROP TABLE [Entity];
""");
    }

    [ConditionalFact]
    public virtual async Task Named_default_constraints_with_opt_in_rename_column_with_implicit_constraint_name()
    {
        await Test(
            builder =>
            {
                builder.UseNamedDefaultConstraints();
                builder.Entity("Entity").Property<string>("Id");
            },
            builder =>
            {
                builder.Entity("Entity").Property<int>("Number").HasColumnName("Number").HasDefaultValue(7);
                builder.Entity("Entity").Property<Guid>("Guid").HasColumnName("Guid").HasDefaultValueSql("NEWID()");
            },
            builder =>
            {
                builder.Entity("Entity").Property<int>("Number").HasColumnName("ModifiedNumber").HasDefaultValue(7);
                builder.Entity("Entity").Property<Guid>("Guid").HasColumnName("ModifiedGuid").HasDefaultValueSql("NEWID()");
            },
            model =>
            {
                var table = Assert.Single(model.Tables);
                var number = Assert.Single(table.Columns, c => c.Name == "ModifiedNumber");
                Assert.Equal(7, number.DefaultValue);
                Assert.Equal("DF_Entity_ModifiedNumber", number[RelationalAnnotationNames.DefaultConstraintName]);

                var guid = Assert.Single(table.Columns, c => c.Name == "ModifiedGuid");
                Assert.Equal("(newid())", guid.DefaultValueSql);
                Assert.Equal("DF_Entity_ModifiedGuid", guid[RelationalAnnotationNames.DefaultConstraintName]);
            });

        AssertSql(
"""
EXEC sp_rename N'[Entity].[Number]', N'ModifiedNumber', 'COLUMN';
""",
                //
                """
EXEC sp_rename N'[Entity].[Guid]', N'ModifiedGuid', 'COLUMN';
""",
                //
                """
ALTER TABLE [Entity] DROP CONSTRAINT [DF_Entity_Number];
ALTER TABLE [Entity] ALTER COLUMN [ModifiedNumber] int NOT NULL;
ALTER TABLE [Entity] ADD CONSTRAINT [DF_Entity_ModifiedNumber] DEFAULT 7 FOR [ModifiedNumber];
""",
                //
                """
ALTER TABLE [Entity] DROP CONSTRAINT [DF_Entity_Guid];
ALTER TABLE [Entity] ALTER COLUMN [ModifiedGuid] uniqueidentifier NOT NULL;
ALTER TABLE [Entity] ADD CONSTRAINT [DF_Entity_ModifiedGuid] DEFAULT (NEWID()) FOR [ModifiedGuid];
""");
    }

    [ConditionalFact]
    public virtual async Task Named_default_constraints_with_opt_in_rename_table_with_column_with_implicit_constraint_name()
    {
        await Test(
            builder =>
            {
                builder.UseNamedDefaultConstraints();
                builder.Entity("Entity").Property<string>("Id");
            },
            builder =>
            {
                builder.Entity("Entity").ToTable("Entities").Property<int>("Number").HasDefaultValue(7);
                builder.Entity("Entity").ToTable("Entities").Property<Guid>("Guid").HasDefaultValueSql("NEWID()");
            },
            builder =>
            {
                builder.Entity("Entity").ToTable("RenamedEntities").Property<int>("Number").HasDefaultValue(7);
                builder.Entity("Entity").ToTable("RenamedEntities").Property<Guid>("Guid").HasDefaultValueSql("NEWID()");
            },
            model =>
            {
                var table = Assert.Single(model.Tables);
                var number = Assert.Single(table.Columns, c => c.Name == "Number");
                Assert.Equal(7, number.DefaultValue);
                Assert.Equal("DF_RenamedEntities_Number", number[RelationalAnnotationNames.DefaultConstraintName]);

                var guid = Assert.Single(table.Columns, c => c.Name == "Guid");
                Assert.Equal("(newid())", guid.DefaultValueSql);
                Assert.Equal("DF_RenamedEntities_Guid", guid[RelationalAnnotationNames.DefaultConstraintName]);
            });

        AssertSql(
"""
ALTER TABLE [Entities] DROP CONSTRAINT [PK_Entities];
""",
                //
                """
EXEC sp_rename N'[Entities]', N'RenamedEntities', 'OBJECT';
""",
                //
                """
ALTER TABLE [RenamedEntities] DROP CONSTRAINT [DF_Entities_Number];
ALTER TABLE [RenamedEntities] ALTER COLUMN [Number] int NOT NULL;
ALTER TABLE [RenamedEntities] ADD CONSTRAINT [DF_RenamedEntities_Number] DEFAULT 7 FOR [Number];
""",
                //
                """
ALTER TABLE [RenamedEntities] DROP CONSTRAINT [DF_Entities_Guid];
ALTER TABLE [RenamedEntities] ALTER COLUMN [Guid] uniqueidentifier NOT NULL;
ALTER TABLE [RenamedEntities] ADD CONSTRAINT [DF_RenamedEntities_Guid] DEFAULT (NEWID()) FOR [Guid];
""",
                //
                """
ALTER TABLE [RenamedEntities] ADD CONSTRAINT [PK_RenamedEntities] PRIMARY KEY ([Id]);
""");
    }

    [ConditionalFact]
    public virtual async Task Named_default_constraints_add_opt_in_with_column_with_implicit_constraint_name()
    {
        await Test(
            builder =>
            {
                builder.Entity("Entity").Property<string>("Id");
                builder.Entity("Entity").Property<int>("Number").HasDefaultValue(7);
                builder.Entity("Entity").Property<Guid>("Guid").HasDefaultValueSql("NEWID()");
            },
            builder => { },
            builder => builder.UseNamedDefaultConstraints(),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var number = Assert.Single(table.Columns, c => c.Name == "Number");
                Assert.Equal(7, number.DefaultValue);
                Assert.Equal("DF_Entity_Number", number[RelationalAnnotationNames.DefaultConstraintName]);

                var guid = Assert.Single(table.Columns, c => c.Name == "Guid");
                Assert.Equal("(newid())", guid.DefaultValueSql);
                Assert.Equal("DF_Entity_Guid", guid[RelationalAnnotationNames.DefaultConstraintName]);
            });

        AssertSql(
"""
DECLARE @var sysname;
SELECT @var = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Entity]') AND [c].[name] = N'Number');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [Entity] DROP CONSTRAINT [' + @var + '];');
ALTER TABLE [Entity] ALTER COLUMN [Number] int NOT NULL;
ALTER TABLE [Entity] ADD CONSTRAINT [DF_Entity_Number] DEFAULT 7 FOR [Number];
""",
                //
                """
DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Entity]') AND [c].[name] = N'Guid');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Entity] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [Entity] ALTER COLUMN [Guid] uniqueidentifier NOT NULL;
ALTER TABLE [Entity] ADD CONSTRAINT [DF_Entity_Guid] DEFAULT (NEWID()) FOR [Guid];
""");
    }

    [ConditionalFact]
    public virtual async Task Named_default_constraints_remove_opt_in_with_column_with_implicit_constraint_name()
    {
        await Test(
            builder =>
            {
                builder.Entity("Entity").Property<string>("Id");
                builder.Entity("Entity").Property<int>("Number").HasDefaultValue(7);
                builder.Entity("Entity").Property<Guid>("Guid").HasDefaultValueSql("NEWID()");
            },
            builder => builder.UseNamedDefaultConstraints(),
            builder => { },
            model =>
            {
                var table = Assert.Single(model.Tables);
                var number = Assert.Single(table.Columns, c => c.Name == "Number");
                Assert.Equal(7, number.DefaultValue);
                Assert.Null(number[RelationalAnnotationNames.DefaultConstraintName]);

                var guid = Assert.Single(table.Columns, c => c.Name == "Guid");
                Assert.Equal("(newid())", guid.DefaultValueSql);
                Assert.Null(guid[RelationalAnnotationNames.DefaultConstraintName]);
            });

        AssertSql(
"""
ALTER TABLE [Entity] DROP CONSTRAINT [DF_Entity_Number];
ALTER TABLE [Entity] ALTER COLUMN [Number] int NOT NULL;
ALTER TABLE [Entity] ADD DEFAULT 7 FOR [Number];
""",
                //
                """
ALTER TABLE [Entity] DROP CONSTRAINT [DF_Entity_Guid];
ALTER TABLE [Entity] ALTER COLUMN [Guid] uniqueidentifier NOT NULL;
ALTER TABLE [Entity] ADD DEFAULT (NEWID()) FOR [Guid];
""");
    }

    [ConditionalFact]
    public virtual async Task Named_default_constraints_add_opt_in_with_column_with_explicit_constraint_name()
    {
        await Test(
            builder =>
            {
                builder.Entity("Entity").Property<string>("Id");
                builder.Entity("Entity").Property<int>("Number").HasDefaultValue(7, defaultConstraintName: "MyConstraint");
                builder.Entity("Entity").Property<Guid>("Guid").HasDefaultValueSql("NEWID()", defaultConstraintName: "MyConstraintSql");
            },
            builder => { },
            builder => builder.UseNamedDefaultConstraints(),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var number = Assert.Single(table.Columns, c => c.Name == "Number");
                Assert.Equal(7, number.DefaultValue);
                Assert.Equal("MyConstraint", number[RelationalAnnotationNames.DefaultConstraintName]);

                var guid = Assert.Single(table.Columns, c => c.Name == "Guid");
                Assert.Equal("(newid())", guid.DefaultValueSql);
                Assert.Equal("MyConstraintSql", guid[RelationalAnnotationNames.DefaultConstraintName]);
            });

        // opt-in doesn't make a difference when constraint name is explicitly defined
        AssertSql();
    }

    [ConditionalFact]
    public virtual async Task Named_default_constraints_remove_opt_in_with_column_with_explicit_constraint_name()
    {
        await Test(
            builder =>
            {
                builder.Entity("Entity").Property<string>("Id");
                builder.Entity("Entity").Property<int>("Number").HasDefaultValue(7, defaultConstraintName: "MyConstraint");
                builder.Entity("Entity").Property<Guid>("Guid").HasDefaultValueSql("NEWID()", defaultConstraintName: "MyConstraintSql");
            },
            builder => builder.UseNamedDefaultConstraints(),
            builder => { },
            model =>
            {
                var table = Assert.Single(model.Tables);
                var number = Assert.Single(table.Columns, c => c.Name == "Number");
                Assert.Equal(7, number.DefaultValue);
                Assert.Equal("MyConstraint", number[RelationalAnnotationNames.DefaultConstraintName]);

                var guid = Assert.Single(table.Columns, c => c.Name == "Guid");
                Assert.Equal("(newid())", guid.DefaultValueSql);
                Assert.Equal("MyConstraintSql", guid[RelationalAnnotationNames.DefaultConstraintName]);
            });

        // opt-in doesn't make a difference when constraint name is explicitly defined
        AssertSql();
    }

    #endregion

    #region edge/advanced cases (e.g. table sharing, name clashes)

    [ConditionalFact]
    public virtual async Task Named_default_constraints_TPT_inheritance_explicit_default_constraint_name()
    {
        await Test(
            builder =>
            {
                builder.UseNamedDefaultConstraints();
                builder.Entity("RootEntity").UseTptMappingStrategy();
                builder.Entity("RootEntity").ToTable("Roots");
                builder.Entity("RootEntity").Property<string>("Id");
                builder.Entity("BranchEntity").HasBaseType("RootEntity");
                builder.Entity("BranchEntity").ToTable("Branches");
                builder.Entity("LeafEntity").HasBaseType("BranchEntity");
                builder.Entity("LeafEntity").ToTable("Leaves");
            },
            builder => { },
            builder =>
            {
                builder.Entity("BranchEntity").Property<int>("Number").HasDefaultValue(7, defaultConstraintName: "MyConstraint");
                builder.Entity("BranchEntity").Property<Guid>("Guid").HasDefaultValueSql("NEWID()", defaultConstraintName: "MyConstraintSql");
            },
            model =>
            {
                var roots = Assert.Single(model.Tables, x => x.Name == "Roots");
                var branches = Assert.Single(model.Tables, x => x.Name == "Branches");
                var leaves = Assert.Single(model.Tables, x => x.Name == "Leaves");

                var branchGuid = Assert.Single(branches.Columns, x => x.Name == "Guid");
                Assert.Equal("MyConstraintSql", branchGuid[RelationalAnnotationNames.DefaultConstraintName]);
                var branchNumber = Assert.Single(branches.Columns, x => x.Name == "Number");
                Assert.Equal("MyConstraint", branchNumber[RelationalAnnotationNames.DefaultConstraintName]);
            });

        AssertSql(
"""
ALTER TABLE [Branches] ADD [Guid] uniqueidentifier NOT NULL CONSTRAINT [MyConstraintSql] DEFAULT (NEWID());
""",
                //
                """
ALTER TABLE [Branches] ADD [Number] int NOT NULL CONSTRAINT [MyConstraint] DEFAULT 7;
""");
    }

    [ConditionalFact]
    public virtual async Task Named_default_constraints_with_opt_in_TPT_inheritance_implicit_default_constraint_name()
    {
        await Test(
            builder =>
            {
                builder.UseNamedDefaultConstraints();
                builder.Entity("RootEntity").UseTptMappingStrategy();
                builder.Entity("RootEntity").ToTable("Roots");
                builder.Entity("RootEntity").Property<string>("Id");
                builder.Entity("BranchEntity").HasBaseType("RootEntity");
                builder.Entity("BranchEntity").ToTable("Branches");
                builder.Entity("LeafEntity").HasBaseType("BranchEntity");
                builder.Entity("LeafEntity").ToTable("Leaves");
            },
            builder => { },
            builder =>
            {
                builder.Entity("BranchEntity").Property<int>("Number").HasDefaultValue(7);
                builder.Entity("BranchEntity").Property<Guid>("Guid").HasDefaultValueSql("NEWID()");
            },
            model =>
            {
                var roots = Assert.Single(model.Tables, x => x.Name == "Roots");
                var branches = Assert.Single(model.Tables, x => x.Name == "Branches");
                var leaves = Assert.Single(model.Tables, x => x.Name == "Leaves");

                var branchGuid = Assert.Single(branches.Columns, x => x.Name == "Guid");
                Assert.Equal("DF_Branches_Guid", branchGuid[RelationalAnnotationNames.DefaultConstraintName]);
                var branchNumber = Assert.Single(branches.Columns, x => x.Name == "Number");
                Assert.Equal("DF_Branches_Number", branchNumber[RelationalAnnotationNames.DefaultConstraintName]);
            });

        AssertSql(
"""
ALTER TABLE [Branches] ADD [Guid] uniqueidentifier NOT NULL CONSTRAINT [DF_Branches_Guid] DEFAULT (NEWID());
""",
                //
                """
ALTER TABLE [Branches] ADD [Number] int NOT NULL CONSTRAINT [DF_Branches_Number] DEFAULT 7;
""");
    }

    [ConditionalFact]
    public virtual async Task Named_default_constraints_with_opt_in_TPC_inheritance_implicit_default_constraint_name()
    {
        await Test(
            builder =>
            {
                builder.UseNamedDefaultConstraints();
                builder.Entity("RootEntity").UseTpcMappingStrategy();
                builder.Entity("RootEntity").ToTable("Roots");
                builder.Entity("RootEntity").Property<string>("Id");
                builder.Entity("BranchEntity").HasBaseType("RootEntity");
                builder.Entity("BranchEntity").ToTable("Branches");
                builder.Entity("LeafEntity").HasBaseType("BranchEntity");
                builder.Entity("LeafEntity").ToTable("Leaves");
            },
            builder => { },
            builder =>
            {
                builder.Entity("BranchEntity").Property<int>("Number").HasDefaultValue(7);
                builder.Entity("BranchEntity").Property<Guid>("Guid").HasDefaultValueSql("NEWID()");
            },
            model =>
            {
                var roots = Assert.Single(model.Tables, x => x.Name == "Roots");
                var branches = Assert.Single(model.Tables, x => x.Name == "Branches");
                var leaves = Assert.Single(model.Tables, x => x.Name == "Leaves");

                var branchGuid = Assert.Single(branches.Columns, x => x.Name == "Guid");
                Assert.Equal("DF_Branches_Guid", branchGuid[RelationalAnnotationNames.DefaultConstraintName]);
                var branchNumber = Assert.Single(branches.Columns, x => x.Name == "Number");
                Assert.Equal("DF_Branches_Number", branchNumber[RelationalAnnotationNames.DefaultConstraintName]);

                var leafGuid = Assert.Single(leaves.Columns, x => x.Name == "Guid");
                Assert.Equal("DF_Leaves_Guid", leafGuid[RelationalAnnotationNames.DefaultConstraintName]);
                var leafNumber = Assert.Single(leaves.Columns, x => x.Name == "Number");
                Assert.Equal("DF_Leaves_Number", leafNumber[RelationalAnnotationNames.DefaultConstraintName]);
            });

        AssertSql(
"""
ALTER TABLE [Leaves] ADD [Guid] uniqueidentifier NOT NULL CONSTRAINT [DF_Leaves_Guid] DEFAULT (NEWID());
""",
                //
                """
ALTER TABLE [Leaves] ADD [Number] int NOT NULL CONSTRAINT [DF_Leaves_Number] DEFAULT 7;
""",
                //
                """
ALTER TABLE [Branches] ADD [Guid] uniqueidentifier NOT NULL CONSTRAINT [DF_Branches_Guid] DEFAULT (NEWID());
""",
                //
                """
ALTER TABLE [Branches] ADD [Number] int NOT NULL CONSTRAINT [DF_Branches_Number] DEFAULT 7;
""");
    }

    [ConditionalFact]
    public virtual async Task Named_default_constraints_name_clash_between_explicit_and_implicit_default_constraint_gets_deduplicated()
    {
        await Test(
            builder =>
            {
                builder.UseNamedDefaultConstraints();
                builder.Entity("Entity").Property<int>("Id");
                builder.Entity("Entity").Property<int>("Number").HasDefaultValue(7, defaultConstraintName: "DF_Entity_Another");
                builder.Entity("Entity").Property<Guid>("Guid").HasDefaultValueSql("NEWID()", defaultConstraintName: "DF_Entity_YetAnother");
            },
            builder => { },
            builder =>
            {
                builder.Entity("Entity").Property<int>("Another").HasDefaultValue(7);
                builder.Entity("Entity").Property<Guid>("YetAnother").HasDefaultValueSql("NEWID()");
            },
            model =>
            {
                var table = Assert.Single(model.Tables);
                var number = Assert.Single(table.Columns, c => c.Name == "Number");
                Assert.Equal("DF_Entity_Another", number[RelationalAnnotationNames.DefaultConstraintName]);
                Assert.Equal(7, number.DefaultValue);

                var guid = Assert.Single(table.Columns, c => c.Name == "Guid");
                Assert.Equal("DF_Entity_YetAnother", guid[RelationalAnnotationNames.DefaultConstraintName]);
                Assert.Equal("(newid())", guid.DefaultValueSql);

                var another = Assert.Single(table.Columns, c => c.Name == "Another");
                Assert.Equal("DF_Entity_Another1", another[RelationalAnnotationNames.DefaultConstraintName]);
                Assert.Equal(7, another.DefaultValue);

                var yetAnother = Assert.Single(table.Columns, c => c.Name == "YetAnother");
                Assert.Equal("DF_Entity_YetAnother1", yetAnother[RelationalAnnotationNames.DefaultConstraintName]);
                Assert.Equal("(newid())", yetAnother.DefaultValueSql);
            });

        AssertSql(
"""
ALTER TABLE [Entity] ADD [Another] int NOT NULL CONSTRAINT [DF_Entity_Another1] DEFAULT 7;
""",
                //
                """
ALTER TABLE [Entity] ADD [YetAnother] uniqueidentifier NOT NULL CONSTRAINT [DF_Entity_YetAnother1] DEFAULT (NEWID());
""");
    }

    [ConditionalFact]
    public virtual async Task Named_default_constraints_very_long_implicit_constraint_name_gets_trimmed_and_deduplicated()
    {
        await Test(
            builder =>
            {
                builder.UseNamedDefaultConstraints();
                builder.Entity("VeryVeryVeryVeryVeryVeryVeryVeryLoooooooooooooooooooooooooooooooonEntity", b =>
                {
                    b.Property<int>("Id");
                    b.OwnsOne("Owned", "YetAnotherVeryVeryVeryVeryVeryLoooooooooooooonnnnnnnnnnnnnnnnnnnnggggggggggggggggggggOwnedNavigation", bb =>
                    {
                        bb.Property<string>("Name");
                    });
                });
            },
            builder =>
            {
                builder.UseNamedDefaultConstraints();
                builder.Entity("VeryVeryVeryVeryVeryVeryVeryVeryLoooooooooooooooooooooooooooooooonEntity", b =>
                {
                    b.Property<int>("Id");
                    b.OwnsOne("Owned", "YetAnotherVeryVeryVeryVeryVeryLoooooooooooooonnnnnnnnnnnnnnnnnnnnggggggggggggggggggggOwnedNavigation", bb =>
                    {
                        bb.Property<string>("Name");
                        bb.Property<int>("Prop").HasDefaultValue(7);
                        bb.Property<Guid>("AnotherProp").HasDefaultValueSql("NEWID()");
                        bb.Property<int>("YetAnotherProp").HasDefaultValue(27);
                    });
                });
            },
            model =>
            {
                var table = Assert.Single(model.Tables);
                var columns = table.Columns.Where(x => x.Name.EndsWith("Prop"));
                Assert.Equal(3, columns.Count());
                Assert.True(columns.All(x => x[RelationalAnnotationNames.DefaultConstraintName] != null));
            });

        AssertSql(
"""
ALTER TABLE [VeryVeryVeryVeryVeryVeryVeryVeryLoooooooooooooooooooooooooooooooonEntity] ADD [YetAnotherVeryVeryVeryVeryVeryLoooooooooooooonnnnnnnnnnnnnnnnnnnnggggggggggggggggggggOwnedNavigation_AnotherProp] uniqueidentifier NULL CONSTRAINT [DF_VeryVeryVeryVeryVeryVeryVeryVeryLoooooooooooooooooooooooooooooooonEntity_YetAnotherVeryVeryVeryVeryVeryLoooooooooooooonnnnnn~] DEFAULT (NEWID());
""",
                //
                """
ALTER TABLE [VeryVeryVeryVeryVeryVeryVeryVeryLoooooooooooooooooooooooooooooooonEntity] ADD [YetAnotherVeryVeryVeryVeryVeryLoooooooooooooonnnnnnnnnnnnnnnnnnnnggggggggggggggggggggOwnedNavigation_Prop] int NULL CONSTRAINT [DF_VeryVeryVeryVeryVeryVeryVeryVeryLoooooooooooooooooooooooooooooooonEntity_YetAnotherVeryVeryVeryVeryVeryLoooooooooooooonnnnn~1] DEFAULT 7;
""",
                //
                """
ALTER TABLE [VeryVeryVeryVeryVeryVeryVeryVeryLoooooooooooooooooooooooooooooooonEntity] ADD [YetAnotherVeryVeryVeryVeryVeryLoooooooooooooonnnnnnnnnnnnnnnnnnnnggggggggggggggggggggOwnedNavigation_YetAnotherProp] int NULL CONSTRAINT [DF_VeryVeryVeryVeryVeryVeryVeryVeryLoooooooooooooooooooooooooooooooonEntity_YetAnotherVeryVeryVeryVeryVeryLoooooooooooooonnnnn~2] DEFAULT 27;
""");
    }

    [ConditionalFact]
    public virtual async Task Named_default_constraints_funky_table_name_with_implicit_constraint()
    {
        await Test(
            builder => builder.Entity("My Entity").Property<string>("Id"),
            builder => { },
            builder =>
            {
                builder.UseNamedDefaultConstraints();
                builder.Entity("My Entity").Property<int>("Number").HasDefaultValue(7);
                builder.Entity("My Entity").Property<Guid>("Guid").HasDefaultValueSql("NEWID()");
            },
            model =>
            {
                var table = Assert.Single(model.Tables);
                var number = Assert.Single(table.Columns, c => c.Name == "Number");
                Assert.Equal(7, number.DefaultValue);
                Assert.Equal("DF_My Entity_Number", number[RelationalAnnotationNames.DefaultConstraintName]);

                var guid = Assert.Single(table.Columns, c => c.Name == "Guid");
                Assert.Equal("(newid())", guid.DefaultValueSql);
                Assert.Equal("DF_My Entity_Guid", guid[RelationalAnnotationNames.DefaultConstraintName]);
            });

        AssertSql(
"""
ALTER TABLE [My Entity] ADD [Guid] uniqueidentifier NOT NULL CONSTRAINT [DF_My Entity_Guid] DEFAULT (NEWID());
""",
                //
                """
ALTER TABLE [My Entity] ADD [Number] int NOT NULL CONSTRAINT [DF_My Entity_Number] DEFAULT 7;
""");
    }

    [ConditionalFact]
    public virtual async Task Named_default_constraints_funky_column_name_with_implicit_constraint()
    {
        await Test(
            builder => builder.Entity("Entity").Property<string>("Id"),
            builder => { },
            builder =>
            {
                builder.UseNamedDefaultConstraints();
                builder.Entity("Entity").Property<int>("Num$be<>r").HasDefaultValue(7);
                builder.Entity("Entity").Property<Guid>("Gu!d").HasDefaultValueSql("NEWID()");
            },
            model =>
            {
                var table = Assert.Single(model.Tables);
                var number = Assert.Single(table.Columns, c => c.Name == "Num$be<>r");
                Assert.Equal(7, number.DefaultValue);
                Assert.Equal("DF_Entity_Num$be<>r", number[RelationalAnnotationNames.DefaultConstraintName]);

                var guid = Assert.Single(table.Columns, c => c.Name == "Gu!d");
                Assert.Equal("(newid())", guid.DefaultValueSql);
                Assert.Equal("DF_Entity_Gu!d", guid[RelationalAnnotationNames.DefaultConstraintName]);
            });

        AssertSql(
"""
ALTER TABLE [Entity] ADD [Gu!d] uniqueidentifier NOT NULL CONSTRAINT [DF_Entity_Gu!d] DEFAULT (NEWID());
""",
                //
                """
ALTER TABLE [Entity] ADD [Num$be<>r] int NOT NULL CONSTRAINT [DF_Entity_Num$be<>r] DEFAULT 7;
""");
    }

    #endregion
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Relational.Design.Specification.TestUtilities;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Relational.Design
{
    public class RelationalDatabaseModelFactoryTest
    {
        private readonly FakeScaffoldingModelFactory _factory;
        private readonly TestDesignLoggerFactory.DesignLogger _logger;
        private static ColumnModel IdColumn => new ColumnModel { Name = "Id", DataType = "long", PrimaryKeyOrdinal = 0 };

        public RelationalDatabaseModelFactoryTest()
        {
            ILoggerFactory loggerFactory = new TestDesignLoggerFactory();
            _logger = (TestDesignLoggerFactory.DesignLogger)loggerFactory.CreateLogger(LoggerCategory.Scaffolding.Name);

            _factory = new FakeScaffoldingModelFactory(
                new DiagnosticsLogger<LoggerCategory.Scaffolding>(
                    loggerFactory,
                    new LoggingOptions(), 
                    new DiagnosticListener("Fake")));
        }

        [Fact]
        public void Creates_entity_types()
        {
            var info = new DatabaseModel
            {
                Tables =
                {
                    new TableModel
                    {
                        Name = "tableWithSchema",
                        SchemaName = "public",
                        Columns = { IdColumn }
                    },
                    new TableModel
                    {
                        Name = "noSchema",
                        Columns = { IdColumn }
                    },
                    new TableModel
                    {
                        Name = "notScaffoldable"
                    }
                }
            };
            var model = _factory.Create(info);
            Assert.Collection(
                model.GetEntityTypes().OrderBy(t => t.Name).Cast<EntityType>(),
                table =>
                    {
                        Assert.Equal("noSchema", table.Relational().TableName);
                        Assert.Null(table.Relational().Schema);
                    },
                pgtable =>
                    {
                        Assert.Equal("tableWithSchema", pgtable.Relational().TableName);
                        Assert.Equal("public", pgtable.Relational().Schema);
                    }
            );
            Assert.NotEmpty(model.Scaffolding().EntityTypeErrors.Values);
        }

        [Fact]
        public void Loads_column_types()
        {
            var info = new DatabaseModel
            {
                Tables =
                {
                    new TableModel
                    {
                        Name = "Jobs",
                        Columns =
                        {
                            IdColumn,
                            new ColumnModel
                            {
                                Name = "occupation",
                                DataType = "string",
                                DefaultValue = "\"dev\""
                            },
                            new ColumnModel
                            {
                                Name = "salary",
                                DataType = "long",
                                IsNullable = true,
                                MaxLength = 100
                            },
                            new ColumnModel
                            {
                                Name = "modified",
                                DataType = "string",
                                IsNullable = false,
                                ValueGenerated = ValueGenerated.OnAddOrUpdate
                            },
                            new ColumnModel
                            {
                                Name = "created",
                                DataType = "string",
                                ValueGenerated = ValueGenerated.OnAdd
                            },
                            new ColumnModel
                            {
                                Name = "current",
                                DataType = "string",
                                ComputedValue = "compute_this()"
                            }
                        }
                    }
                }
            };

            var entityType = (EntityType)_factory.Create(info).FindEntityType("Jobs");

            Assert.Collection(
                entityType.GetProperties(),
                pk =>
                    {
                        Assert.Equal("Id", pk.Name);
                        Assert.Equal(typeof(long), pk.ClrType);
                    },
                col1 =>
                    {
                        Assert.Equal("created", col1.Relational().ColumnName);
                        Assert.Equal(ValueGenerated.OnAdd, col1.ValueGenerated);
                    },
                col2 =>
                    {
                        Assert.Equal("Current", col2.Name);
                        Assert.Equal(typeof(string), col2.ClrType);
                        Assert.Equal("compute_this()", col2.Relational().ComputedColumnSql);
                    },
                col3 =>
                    {
                        Assert.Equal("modified", col3.Relational().ColumnName);
                        Assert.Equal(ValueGenerated.OnAddOrUpdate, col3.ValueGenerated);
                    },
                col4 =>
                    {
                        Assert.Equal("occupation", col4.Relational().ColumnName);
                        Assert.Equal(typeof(string), col4.ClrType);
                        Assert.False(col4.IsColumnNullable());
                        Assert.Null(col4.GetMaxLength());
                        Assert.Equal("\"dev\"", col4.Relational().DefaultValueSql);
                    },
                col5 =>
                    {
                        Assert.Equal("Salary", col5.Name);
                        Assert.Equal(typeof(long?), col5.ClrType);
                        Assert.True(col5.IsColumnNullable());
                        Assert.Equal(100, col5.GetMaxLength());
                        Assert.Null(col5.Relational().DefaultValue);
                    });
        }

        [Theory]
        [InlineData("string", null)]
        [InlineData("alias for string", "alias for string")]
        public void Column_type_annotation(string dataType, string expectedColumnType)
        {
            var info = new DatabaseModel
            {
                Tables =
                {
                    new TableModel
                    {
                        Name = "A",
                        Columns =
                        {
                            new ColumnModel
                            {
                                Name = "Col",
                                DataType = dataType,
                                PrimaryKeyOrdinal = 1
                            }
                        }
                    }
                }
            };

            var property = (Property)_factory.Create(info).FindEntityType("A").FindProperty("Col");

            Assert.Equal(expectedColumnType, property.Relational().ColumnType);
        }

        [Fact]
        public void Column_ordinal_annotation()
        {
            var info = new DatabaseModel
            {
                Tables =
                {
                    new TableModel
                    {
                        Name = "A",
                        Columns =
                        {
                            new ColumnModel
                            {
                                Name = "Col1",
                                DataType = "string",
                                PrimaryKeyOrdinal = 1,
                                Ordinal = 1
                            },
                            new ColumnModel
                            {
                                Name = "Col2",
                                DataType = "string",
                                Ordinal = 2
                            },
                            new ColumnModel
                            {
                                Name = "Col3",
                                DataType = "string",
                                Ordinal = 3
                            }
                        }
                    }
                }
            };

            var entityTypeA = _factory.Create(info).FindEntityType("A");
            var property1 = (Property)entityTypeA.FindProperty("Col1");
            var property2 = (Property)entityTypeA.FindProperty("Col2");
            var property3 = (Property)entityTypeA.FindProperty("Col3");

            Assert.Equal(1, property1.Scaffolding().ColumnOrdinal);
            Assert.Equal(2, property2.Scaffolding().ColumnOrdinal);
            Assert.Equal(3, property3.Scaffolding().ColumnOrdinal);
        }

        [Theory]
        [InlineData("cheese")]
        [InlineData(null)]
        public void Unmappable_column_type(string dataType)
        {
            var info = new DatabaseModel
            {
                Tables =
                {
                    new TableModel
                    {
                        Name = "E",
                        Columns = { IdColumn }
                    }
                }
            };

            info.Tables.First()
                .Columns.Add(
                    new ColumnModel
                    {
                        Table = info.Tables.First(),
                        Name = "Coli",
                        DataType = dataType
                    });

            Assert.Single(_factory.Create(info).FindEntityType("E").GetProperties());
            Assert.Single(_logger.Statements, t => t.Contains(RelationalDesignStrings.LogCannotFindTypeMappingForColumn.GenerateMessage("E.Coli", dataType)));
        }

        [Theory]
        [InlineData(new[] { "Id" }, 1)]
        [InlineData(new[] { "Id", "AltId" }, 2)]
        public void Primary_key(string[] keyProps, int length)

        {
            var ordinal = 3;
            var info = new DatabaseModel
            {
                Tables =
                {
                    new TableModel
                    {
                        Name = "PkTable",
                        Columns = keyProps.Select(k => new ColumnModel { PrimaryKeyOrdinal = ordinal++, Name = k, DataType = "long" }).ToList()
                    }
                }
            };
            var model = (EntityType)_factory.Create(info).GetEntityTypes().Single();

            Assert.Equal(keyProps, model.FindPrimaryKey().Properties.Select(p => p.Relational().ColumnName).ToArray());
        }

        [Fact]
        public void Indexes_and_alternate_keys()
        {
            var table = new TableModel
            {
                Name = "T",
                Columns =
                {
                    new ColumnModel { Name = "C1", DataType = "long", PrimaryKeyOrdinal = 1 },
                    new ColumnModel { Name = "C2", DataType = "long" },
                    new ColumnModel { Name = "C3", DataType = "long" }
                }
            };
            table.Indexes.Add(
                new IndexModel
                {
                    Name = "IDX_C1",
                    IndexColumns = { new IndexColumnModel { Column = table.Columns.ElementAt(0) } },
                    IsUnique = false
                });
            table.Indexes.Add(
                new IndexModel
                {
                    Name = "UNQ_C2",
                    IndexColumns = { new IndexColumnModel { Column = table.Columns.ElementAt(1) } },
                    IsUnique = true
                });
            table.Indexes.Add(
                new IndexModel
                {
                    Name = "IDX_C2_C1",
                    IndexColumns =
                    {
                        new IndexColumnModel { Column = table.Columns.ElementAt(1) },
                        new IndexColumnModel { Column = table.Columns.ElementAt(0) }
                    },
                    IsUnique = false
                });
            table.Indexes.Add(
                new IndexModel
                {
                    /*Name ="UNQ_C3_C1",*/
                    IndexColumns =
                    {
                        new IndexColumnModel { Column = table.Columns.ElementAt(2) },
                        new IndexColumnModel { Column = table.Columns.ElementAt(0) }
                    },
                    IsUnique = true
                });

            var info = new DatabaseModel { Tables = { table } };

            var entityType = (EntityType)_factory.Create(info).GetEntityTypes().Single();

            Assert.Collection(
                entityType.GetIndexes(),
                indexColumn1 =>
                    {
                        Assert.False(indexColumn1.IsUnique);
                        Assert.Equal("IDX_C1", indexColumn1.Relational().Name);
                        Assert.Same(entityType.FindProperty("C1"), indexColumn1.Properties.Single());
                    },
                uniqueColumn2 =>
                    {
                        Assert.True(uniqueColumn2.IsUnique);
                        Assert.Same(entityType.FindProperty("C2"), uniqueColumn2.Properties.Single());
                    },
                indexColumn2Column1 =>
                    {
                        Assert.False(indexColumn2Column1.IsUnique);
                        Assert.Equal(new[] { "C2", "C1" }, indexColumn2Column1.Properties.Select(c => c.Name).ToArray());
                    },
                uniqueColumn3Column1 =>
                    {
                        Assert.True(uniqueColumn3Column1.IsUnique);
                        Assert.Equal(new[] { "C3", "C1" }, uniqueColumn3Column1.Properties.Select(c => c.Name).ToArray());
                    }
            );

            // unique indexes should not cause alternate keys if not used by foreign keys
            Assert.Equal(0, entityType.GetKeys().Count(k => !k.IsPrimaryKey()));
        }

        [Fact]
        public void Foreign_key()

        {
            var parentTable = new TableModel { Name = "Parent", Columns = { IdColumn } };
            var childrenTable = new TableModel
            {
                Name = "Children",
                Columns =
                {
                    IdColumn,
                    new ColumnModel { Name = "ParentId", DataType = "long", IsNullable = true }
                }
            };
            childrenTable.ForeignKeys.Add(
                new ForeignKeyModel
                {
                    Table = childrenTable,
                    PrincipalTable = parentTable,
                    OnDelete = ReferentialAction.Cascade,
                    Columns =
                    {
                        new ForeignKeyColumnModel
                        {
                            Ordinal = 1,
                            Column = childrenTable.Columns.ElementAt(1),
                            PrincipalColumn = parentTable.Columns.ElementAt(0)
                        }
                    }
                });

            var model = _factory.Create(new DatabaseModel { Tables = { parentTable, childrenTable } });

            var parent = (EntityType)model.FindEntityType("Parent");

            var children = (EntityType)model.FindEntityType("Children");

            Assert.NotEmpty(parent.GetReferencingForeignKeys());
            var fk = Assert.Single(children.GetForeignKeys());
            Assert.False(fk.IsUnique);
            Assert.Equal(DeleteBehavior.Cascade, fk.DeleteBehavior);

            var principalKey = fk.PrincipalKey;

            Assert.Same(parent, principalKey.DeclaringEntityType);
            Assert.Same(parent.GetProperties().First(), principalKey.Properties[0]);
        }

        [Fact]
        public void Unique_foreign_key()

        {
            var parentTable = new TableModel { Name = "Parent", Columns = { IdColumn } };
            var childrenTable = new TableModel { Name = "Children", Columns = { IdColumn } };
            childrenTable.ForeignKeys.Add(
                new ForeignKeyModel
                {
                    Table = childrenTable,
                    PrincipalTable = parentTable,
                    OnDelete = ReferentialAction.NoAction,
                    Columns =
                    {
                        new ForeignKeyColumnModel
                        {
                            Ordinal = 1,
                            Column = childrenTable.Columns.ElementAt(0),
                            PrincipalColumn = parentTable.Columns.ElementAt(0)
                        }
                    }
                });

            var model = _factory.Create(new DatabaseModel { Tables = { parentTable, childrenTable } });

            var children = (EntityType)model.FindEntityType("Children");

            var fk = Assert.Single(children.GetForeignKeys());
            Assert.True(fk.IsUnique);
            Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior);
        }

        [Fact]
        public void Composite_foreign_key()

        {
            var parentTable = new TableModel
            {
                Name = "Parent",
                Columns =
                {
                    new ColumnModel { Name = "Id_A", DataType = "long", PrimaryKeyOrdinal = 1 },
                    new ColumnModel { Name = "Id_B", DataType = "long", PrimaryKeyOrdinal = 2 }
                }
            };
            var childrenTable = new TableModel
            {
                Name = "Children",
                Columns =
                {
                    IdColumn,
                    new ColumnModel { Name = "ParentId_A", DataType = "long" },
                    new ColumnModel { Name = "ParentId_B", DataType = "long" }
                }
            };
            childrenTable.ForeignKeys.Add(
                new ForeignKeyModel
                {
                    Table = childrenTable,
                    PrincipalTable = parentTable,
                    OnDelete = ReferentialAction.SetNull,
                    Columns =
                    {
                        new ForeignKeyColumnModel
                        {
                            Ordinal = 1,
                            Column = childrenTable.Columns.ElementAt(1),
                            PrincipalColumn = parentTable.Columns.ElementAt(0)
                        },
                        new ForeignKeyColumnModel
                        {
                            Ordinal = 1,
                            Column = childrenTable.Columns.ElementAt(2),
                            PrincipalColumn = parentTable.Columns.ElementAt(1)
                        }
                    }
                });

            var model = _factory.Create(new DatabaseModel { Tables = { parentTable, childrenTable } });

            var parent = (EntityType)model.FindEntityType("Parent");

            var children = (EntityType)model.FindEntityType("Children");

            Assert.NotEmpty(parent.GetReferencingForeignKeys());

            var fk = Assert.Single(children.GetForeignKeys());
            Assert.False(fk.IsUnique);
            Assert.Equal(DeleteBehavior.SetNull, fk.DeleteBehavior);

            var principalKey = fk.PrincipalKey;

            Assert.Equal("Parent", principalKey.DeclaringEntityType.Name);
            Assert.Equal("IdA", principalKey.Properties[0].Name);
            Assert.Equal("IdB", principalKey.Properties[1].Name);
        }

        [Fact]
        public void It_loads_self_referencing_foreign_key()

        {
            var table = new TableModel
            {
                Name = "ItemsList",
                Columns =
                {
                    IdColumn,
                    new ColumnModel { Name = "ParentId", DataType = "long", IsNullable = false }
                }
            };
            table.ForeignKeys.Add(
                new ForeignKeyModel
                {
                    Table = table,
                    PrincipalTable = table,
                    Columns =
                    {
                        new ForeignKeyColumnModel
                        {
                            Ordinal = 1,
                            Column = table.Columns.ElementAt(1),
                            PrincipalColumn = table.Columns.ElementAt(0)
                        }
                    }
                });

            var model = _factory.Create(new DatabaseModel { Tables = { table } });
            var list = model.FindEntityType("ItemsList");

            Assert.NotEmpty(list.GetReferencingForeignKeys());
            Assert.NotEmpty(list.GetForeignKeys());

            var principalKey = list.FindForeignKeys(list.FindProperty("ParentId")).SingleOrDefault().PrincipalKey;
            Assert.Equal("ItemsList", principalKey.DeclaringEntityType.Name);
            Assert.Equal("Id", principalKey.Properties[0].Name);
        }

        [Fact]
        public void It_logs_warning_for_bad_foreign_key()
        {
            var parentTable = new TableModel
            {
                Name = "Parent",
                Columns =
                {
                    IdColumn,
                    new ColumnModel { Name = "NotPkId", DataType = "long", PrimaryKeyOrdinal = null }
                }
            };
            var childrenTable = new TableModel
            {
                Name = "Children",
                Columns =
                {
                    IdColumn,
                    new ColumnModel { Name = "ParentId", DataType = "long" }
                }
            };
            childrenTable.ForeignKeys.Add(
                new ForeignKeyModel
                {
                    Table = childrenTable,
                    PrincipalTable = parentTable,
                    Columns =
                    {
                        new ForeignKeyColumnModel
                        {
                            Ordinal = 1,
                            Column = childrenTable.Columns.ElementAt(1),
                            PrincipalColumn = parentTable.Columns.ElementAt(1)
                        }
                    }
                });

            _factory.Create(new DatabaseModel { Tables = { parentTable, childrenTable } });

            Assert.Single(
                _logger.Statements, t => t.Contains(
                    "Warning: " +
                    RelationalDesignStrings.LogForeignKeyScaffoldErrorPrincipalKeyNotFound.GenerateMessage(
                        childrenTable.ForeignKeys.ElementAt(0).DisplayName, "NotPkId", "Parent")));
        }

        [Fact]
        public void Unique_nullable_index_unused_by_foreign_key()
        {
            var table = new TableModel
            {
                Name = "Friends",
                Columns =
                {
                    IdColumn,
                    new ColumnModel { Name = "BuddyId", DataType = "long", IsNullable = true }
                }
            };
            table.Indexes.Add(
                new IndexModel
                {
                    IndexColumns = { new IndexColumnModel { Column = table.Columns.ElementAt(1) } },
                    IsUnique = true
                });
            table.ForeignKeys.Add(
                new ForeignKeyModel
                {
                    Table = table,
                    PrincipalTable = table,
                    Columns =
                    {
                        new ForeignKeyColumnModel
                        {
                            Ordinal = 1,
                            Column = table.Columns.ElementAt(1),
                            PrincipalColumn = table.Columns.ElementAt(0)
                        }
                    }
                });

            var model = _factory.Create(new DatabaseModel { Tables = { table } }).FindEntityType("Friends");

            var buddyIdProperty = model.FindProperty("BuddyId");
            Assert.NotNull(buddyIdProperty);
            Assert.True(buddyIdProperty.IsNullable);

            var fk = Assert.Single(model.GetForeignKeys());

            Assert.True(fk.IsUnique);
            Assert.Empty(model.GetKeys().Where(k => !k.IsPrimaryKey()));
            Assert.Equal(model.FindPrimaryKey(), fk.PrincipalKey);
        }

        [Fact]
        public void Unique_nullable_index_used_by_foreign_key()
        {
            var table = new TableModel
            {
                Name = "Friends",
                Columns =
                {
                    IdColumn,
                    new ColumnModel { Name = "BuddyId", DataType = "long", IsNullable = true }
                }
            };
            table.Indexes.Add(
                new IndexModel
                {
                    Name = "FriendsNameUniqueIndex",
                    IndexColumns = { new IndexColumnModel { Column = table.Columns.ElementAt(1) } },
                    IsUnique = true
                });
            table.ForeignKeys.Add(
                new ForeignKeyModel
                {
                    Table = table,
                    PrincipalTable = table,
                    Columns =
                    {
                        new ForeignKeyColumnModel
                        {
                            Ordinal = 1,
                            Column = table.Columns.ElementAt(1),
                            PrincipalColumn = table.Columns.ElementAt(1)
                        }
                    }
                });

            var model = _factory.Create(new DatabaseModel { Tables = { table } }).FindEntityType("Friends");

            var buddyIdProperty = model.FindProperty("BuddyId");
            Assert.NotNull(buddyIdProperty);
            Assert.False(buddyIdProperty.IsNullable);

            var fk = Assert.Single(model.GetForeignKeys());

            Assert.True(fk.IsUnique);
            var alternateKey = model.GetKeys().Single(k => !k.IsPrimaryKey());
            Assert.Equal(alternateKey, fk.PrincipalKey);

            Assert.Single(
                _logger.Statements, t => t.Contains(
                    "Warning: " +
                    RelationalDesignStrings.LogForeignKeyPrincipalEndContainsNullableColumns.GenerateMessage(
                        table.ForeignKeys.ElementAt(0).DisplayName, "FriendsNameUniqueIndex", "BuddyId")));
        }

        [Fact]
        public void Unique_index_composite_foreign_key()
        {
            var parentTable = new TableModel
            {
                Name = "Parent",
                Columns =
                {
                    new ColumnModel { Name = "Id_A", DataType = "long", PrimaryKeyOrdinal = 1 },
                    new ColumnModel { Name = "Id_B", DataType = "long", PrimaryKeyOrdinal = 2 }
                }
            };
            var childrenTable = new TableModel
            {
                Name = "Children",
                Columns =
                {
                    IdColumn,
                    new ColumnModel { Name = "ParentId_A", DataType = "long" },
                    new ColumnModel { Name = "ParentId_B", DataType = "long" }
                }
            };
            childrenTable.Indexes.Add(
                new IndexModel
                {
                    IsUnique = true,
                    IndexColumns =
                    {
                        new IndexColumnModel { Column = childrenTable.Columns.ElementAt(1) },
                        new IndexColumnModel { Column = childrenTable.Columns.ElementAt(2) }
                    }
                });
            childrenTable.ForeignKeys.Add(
                new ForeignKeyModel
                {
                    Table = childrenTable,
                    PrincipalTable = parentTable,
                    Columns =
                    {
                        new ForeignKeyColumnModel
                        {
                            Ordinal = 1,
                            Column = childrenTable.Columns.ElementAt(1),
                            PrincipalColumn = parentTable.Columns.ElementAt(0)
                        },
                        new ForeignKeyColumnModel
                        {
                            Ordinal = 2,
                            Column = childrenTable.Columns.ElementAt(2),
                            PrincipalColumn = parentTable.Columns.ElementAt(1)
                        }
                    }
                });

            var model = _factory.Create(new DatabaseModel { Tables = { parentTable, childrenTable } });
            var parent = model.FindEntityType("Parent");
            var children = model.FindEntityType("Children");

            var fk = Assert.Single(children.GetForeignKeys());

            Assert.True(fk.IsUnique);
            Assert.Equal(parent.FindPrimaryKey(), fk.PrincipalKey);
        }

        [Fact]
        public void Unique_names()
        {
            var info = new DatabaseModel
            {
                Tables =
                {
                    new TableModel
                    {
                        Name = "E F",
                        Columns =
                        {
                            new ColumnModel { Name = "San itized", DataType = "long" },
                            new ColumnModel { Name = "San+itized", DataType = "long" }
                        }
                    },
                    new TableModel { Name = "E+F" }
                }
            };

            info.Tables.ElementAt(0).Columns.Add(new ColumnModel { Name = "Id", DataType = "long", PrimaryKeyOrdinal = 0, Table = info.Tables.ElementAt(0) });
            info.Tables.ElementAt(1).Columns.Add(new ColumnModel { Name = "Id", DataType = "long", PrimaryKeyOrdinal = 0, Table = info.Tables.ElementAt(1) });

            var model = _factory.Create(info);

            Assert.Collection(
                model.GetEntityTypes().Cast<EntityType>(),
                ef1 =>
                    {
                        Assert.Equal("E F", ef1.Relational().TableName);
                        Assert.Equal("EF", ef1.Name);
                        Assert.Collection(
                            ef1.GetProperties(),
                            id => { Assert.Equal("Id", id.Name); },
                            s1 =>
                                {
                                    Assert.Equal("SanItized", s1.Name);
                                    Assert.Equal("San itized", s1.Relational().ColumnName);
                                },
                            s2 =>
                                {
                                    Assert.Equal("SanItized1", s2.Name);
                                    Assert.Equal("San+itized", s2.Relational().ColumnName);
                                });
                    },
                ef2 =>
                    {
                        Assert.Equal("E+F", ef2.Relational().TableName);
                        Assert.Equal("EF1", ef2.Name);
                        var id = Assert.Single(ef2.GetProperties());
                        Assert.Equal("Id", id.Name);
                        Assert.Equal("Id", id.Relational().ColumnName);
                    });
        }

        [Fact]
        public void Sequences()
        {
            var info = new DatabaseModel
            {
                Sequences =
                {
                    new SequenceModel { Name = "CountByThree", IncrementBy = 3 }
                }
            };

            var model = (Model)_factory.Create(info);

            Assert.Collection(
                model.Relational().Sequences, first =>
                    {
                        Assert.NotNull(first);
                        Assert.Equal("CountByThree", first.Name);
                        Assert.Equal(3, first.IncrementBy);
                        Assert.Null(first.Schema);
                        Assert.Null(first.MaxValue);
                        Assert.Null(first.MinValue);
                        Assert.False(first.IsCyclic);
                    });
        }

        [Fact]
        public void DbSet_annotation_is_set()
        {
            var info = new DatabaseModel
            {
                Tables =
                {
                    new TableModel
                    {
                        Name = "Blog",
                        Columns = { IdColumn }
                    }
                }
            };

            var model = _factory.Create(info);
            Assert.Equal("Blog", model.GetEntityTypes().Single().Scaffolding().DbSetName);
        }

        [Fact]
        public void Pluralization_of_entity_and_DbSet()
        {
            var info = new DatabaseModel
            {
                Tables =
                {
                    new TableModel
                    {
                        Name = "Blog",
                        Columns = { IdColumn }
                    },
                    new TableModel
                    {
                        Name = "Posts",
                        Columns = { IdColumn }
                    }
                }
            };

            var factory = new FakeScaffoldingModelFactory(
                new DiagnosticsLogger<LoggerCategory.Scaffolding>(
                    new TestDesignLoggerFactory(),
                    new LoggingOptions(), 
                    new DiagnosticListener("Fake")),
                new FakePluralizer());

            var model = factory.Create(info);

            Assert.Collection(
                model.GetEntityTypes().OrderBy(t => t.Name).Cast<EntityType>(),
                entity =>
                    {
                        Assert.Equal("Blog", entity.Relational().TableName);
                        Assert.Equal("Blog", entity.Name);
                        Assert.Equal("Blogs", entity.Scaffolding().DbSetName);
                    },
                entity =>
                    {
                        Assert.Equal("Posts", entity.Relational().TableName);
                        Assert.Equal("Post", entity.Name);
                        Assert.Equal("Posts", entity.Scaffolding().DbSetName);
                    }
            );
        }

        [Fact]
        public void Pluralization_of_collection_navigations()
        {
            var blogTable = new TableModel { Name = "Blog", Columns = { IdColumn } };
            var postTable = new TableModel
            {
                Name = "Post",
                Columns =
                {
                    IdColumn,
                    new ColumnModel { Name = "BlogId", DataType = "long", IsNullable = true }
                }
            };

            postTable.ForeignKeys.Add(
                new ForeignKeyModel
                {
                    Table = postTable,
                    PrincipalTable = blogTable,
                    OnDelete = ReferentialAction.Cascade,
                    Columns =
                    {
                        new ForeignKeyColumnModel
                        {
                            Ordinal = 1,
                            Column = postTable.Columns.ElementAt(1),
                            PrincipalColumn = blogTable.Columns.ElementAt(0)
                        }
                    }
                });

            var info = new DatabaseModel
            {
                Tables = { blogTable, postTable }
            };

            var factory = new FakeScaffoldingModelFactory(
                new DiagnosticsLogger<LoggerCategory.Scaffolding>(
                    new TestDesignLoggerFactory(),
                    new LoggingOptions(),
                    new DiagnosticListener("Fake")),
                new FakePluralizer());

            var model = factory.Create(info);

            Assert.Collection(
                model.GetEntityTypes().OrderBy(t => t.Name).Cast<EntityType>(),
                entity =>
                    {
                        Assert.Equal("Blog", entity.Name);
                        Assert.Equal("Posts", entity.GetNavigations().Single().Name);
                    },
                entity =>
                    {
                        Assert.Equal("Post", entity.Name);
                        Assert.Equal("Blog", entity.GetNavigations().Single().Name);
                    }
            );
        }
    }

    public class FakeScaffoldingModelFactory : RelationalScaffoldingModelFactory
    {
        public IModel Create(DatabaseModel databaseModel) => CreateFromDatabaseModel(databaseModel);

        public FakeScaffoldingModelFactory(
            [NotNull] IDiagnosticsLogger<LoggerCategory.Scaffolding> logger)
            : this(logger, new NullPluralizer())
        {
        }

        public FakeScaffoldingModelFactory(
            [NotNull] IDiagnosticsLogger<LoggerCategory.Scaffolding> logger,
            [NotNull] IPluralizer pluralizer)
            : base(
                logger,
                new TestTypeMapper(new RelationalTypeMapperDependencies()),
                new FakeDatabaseModelFactory(),
                new CandidateNamingService(),
                pluralizer)
        {
        }
    }

    public class FakeDatabaseModelFactory : IDatabaseModelFactory
    {
        public virtual DatabaseModel Create(string connectionString, TableSelectionSet tableSelectionSet)
        {
            throw new NotImplementedException();
        }
    }

    public class TestTypeMapper : RelationalTypeMapper
    {
        private static readonly RelationalTypeMapping _string = new RelationalTypeMapping("string", typeof(string));
        private static readonly RelationalTypeMapping _long = new RelationalTypeMapping("long", typeof(long));

        public TestTypeMapper(RelationalTypeMapperDependencies dependencies)
            : base(dependencies)
        {
        }

        private readonly IReadOnlyDictionary<Type, RelationalTypeMapping> _simpleMappings
            = new Dictionary<Type, RelationalTypeMapping>
            {
                { typeof(string), _string },
                { typeof(long), _long }
            };

        private readonly IReadOnlyDictionary<string, RelationalTypeMapping> _simpleNameMappings
            = new Dictionary<string, RelationalTypeMapping>(StringComparer.OrdinalIgnoreCase)
            {
                { "string", _string },
                { "alias for string", _string },
                { "long", _long }
            };

        protected override IReadOnlyDictionary<Type, RelationalTypeMapping> GetClrTypeMappings()
            => _simpleMappings;

        protected override IReadOnlyDictionary<string, RelationalTypeMapping> GetStoreTypeMappings()
            => _simpleNameMappings;

        protected override string GetColumnType(IProperty property) => ((Property)property).Relational().ColumnType;
    }

    public class FakePluralizer : IPluralizer
    {
        public string Pluralize(string name)
        {
            return name.EndsWith("s")
                ? name
                : name + "s";
        }

        public string Singularize(string name)
        {
            return name.EndsWith("s")
                ? name.Substring(0, name.Length - 1)
                : name;
        }
    }
}

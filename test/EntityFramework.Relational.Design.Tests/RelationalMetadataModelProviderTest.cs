// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Design.Model;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Internal;
using Microsoft.Data.Entity.Storage;
using Microsoft.Extensions.Logging;
using Xunit;
using ForeignKey = Microsoft.Data.Entity.Relational.Design.Model.ForeignKey;
using Index = Microsoft.Data.Entity.Relational.Design.Model.Index;

namespace Microsoft.Data.Entity.Relational.Design
{
    public class RelationalMetadataModelProviderTest
    {
        private readonly FakeMetadataModelProvider _provider;
        private readonly TestLogger _logger;

        public RelationalMetadataModelProviderTest()
        {
            var factory = new TestLoggerFactory();
            _logger = factory.Logger;

            _provider = new FakeMetadataModelProvider(factory);
        }

        [Fact]
        public void Creates_entity_types()
        {
            var info = new SchemaInfo
            {
                Tables =
                {
                    new Table { Name = "tableWithSchema", SchemaName = "public" },
                    new Table { Name = "noSchema" }
                }
            };
            var model = _provider.GetModel(info);
            Assert.Collection(model.EntityTypes.OrderBy(t => t.Name).Cast<EntityType>(),
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
        }

        [Fact]
        public void Loads_column_types()
        {
            var info = new SchemaInfo
            {
                Tables =
                {
                    new Table
                    {
                        Name = "Jobs",
                        Columns =
                        {
                            new Column
                            {
                                Name = "occupation",
                                DataType = "string",
                                DefaultValue = "\"dev\""
                            },
                            new Column
                            {
                                Name = "salary",
                                DataType = "long",
                                IsNullable = true,
                                MaxLength = 100
                            },
                            new Column
                            {
                                Name = "modified",
                                DataType = "string",
                                IsNullable = false,
                                IsComputed = true
                            },
                            new Column
                            {
                                Name = "created",
                                DataType = "string",
                                IsStoreGenerated = true
                            }
                        }
                    }
                }
            };

            var entityType = (EntityType)_provider.GetModel(info).GetEntityType("Jobs");

            Assert.Collection(entityType.Properties,
                col4 =>
                    {
                        Assert.Equal("created", col4.Relational().ColumnName);
                        Assert.Equal(ValueGenerated.OnAdd, col4.ValueGenerated);
                    },
                col3 =>
                    {
                        Assert.Equal("modified", col3.Relational().ColumnName);
                        Assert.Equal(ValueGenerated.OnAddOrUpdate, col3.ValueGenerated);
                    },
                col1 =>
                    {
                        Assert.Equal("occupation", col1.Relational().ColumnName);
                        Assert.Equal(typeof(string), col1.ClrType);
                        Assert.False(col1.IsColumnNullable());
                        Assert.Null(col1.GetMaxLength());
                        Assert.Equal("\"dev\"", col1.Relational().GeneratedValueSql);
                    },
                col2 =>
                    {
                        Assert.Equal(typeof(long?), col2.ClrType);
                        Assert.True(col2.IsColumnNullable());
                        Assert.Equal(100, col2.GetMaxLength());
                        Assert.Null(col2.Relational().DefaultValue);
                    });

            Assert.Contains(RelationalDesignStrings.MissingPrimaryKey("Jobs"), _logger.FullLog);
        }

        [Theory]
        [InlineData("string", null)]
        [InlineData("alias for string", "alias for string")]
        public void Column_type_annotation(string dataType, string expectedColumnType)
        {
            var info = new SchemaInfo
            {
                Tables =
                {
                    new Table
                    {
                        Name = "A",
                        Columns =
                        {
                            new Column
                            {
                                Name = "Col",
                                DataType = dataType
                            }
                        }
                    }
                }
            };
            var property = (Property)_provider.GetModel(info).GetEntityType("A").GetProperty("Col");
            Assert.Equal(expectedColumnType, property.Relational().ColumnType);
        }

        [Theory]
        [InlineData("cheese")]
        [InlineData(null)]
        public void Unmappable_column_type(string dataType)
        {
            var info = new SchemaInfo { Tables = { new Table { Name = "E" } } };
            info.Tables[0].Columns.Add(new Column
            {
                Table = info.Tables[0],
                Name = "Coli",
                DataType = dataType
            });
            Assert.Empty(_provider.GetModel(info).GetEntityType("E").GetProperties());
            Assert.Contains(RelationalDesignStrings.CannotFindTypeMappingForColumn("E.Coli", dataType), _logger.FullLog);
        }

        [Theory]
        [InlineData(new[] { "Id" }, 1)]
        [InlineData(new[] { "Id", "AltId" }, 2)]
        public void Primary_key(string[] keyProps, int length)

        {
            var ordinal = 3;
            var info = new SchemaInfo
            {
                Tables =
                {
                    new Table
                    {
                        Name = "PkTable",
                        Columns = keyProps.Select(k => new Column { PrimaryKeyOrdinal = ordinal++, Name = k, DataType = "long" }).ToList()
                    }
                }
            };
            var model = (EntityType)_provider.GetModel(info).EntityTypes.Single();

            Assert.Equal(keyProps, model.GetPrimaryKey().Properties.Select(p => p.Relational().ColumnName).ToArray());
        }

        [Fact]
        public void Indexes_and_alternate_keys()
        {
            var table = new Table
            {
                Name = "T",
                Columns =
                {
                    new Column { Name = "C1", DataType = "long" },
                    new Column { Name = "C2", DataType = "long" },
                    new Column { Name = "C3", DataType = "long" }
                }
            };
            table.Indexes.Add(new Index { Name = "IDX_C1", Columns = { table.Columns[0] }, IsUnique = false });
            table.Indexes.Add(new Index { Name = "UNQ_C2", Columns = { table.Columns[1] }, IsUnique = true });
            table.Indexes.Add(new Index { Name = "IDX_C2_C1", Columns = { table.Columns[1], table.Columns[0] }, IsUnique = false });
            table.Indexes.Add(new Index { /*Name ="UNQ_C3_C1",*/ Columns = { table.Columns[2], table.Columns[0] }, IsUnique = true });
            var info = new SchemaInfo { Tables = { table } };

            var entityType = (EntityType)_provider.GetModel(info).EntityTypes.Single();

            Assert.Collection(entityType.Indexes,
                indexColumn1 =>
                    {
                        Assert.False(indexColumn1.IsUnique);
                        Assert.Equal("IDX_C1", indexColumn1.Relational().Name);
                        Assert.Same(entityType.GetProperty("C1"), indexColumn1.Properties.Single());
                    },
                uniqueColumn2 =>
                    {
                        Assert.True(uniqueColumn2.IsUnique);
                        Assert.Same(entityType.GetProperty("C2"), uniqueColumn2.Properties.Single());
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

            Assert.Collection(entityType.GetKeys(),
                single =>
                    {
                        Assert.Equal("UNQ_C2", single.Relational().Name);
                        Assert.Same(entityType.GetProperty("C2"), single.Properties.Single());
                    },
                composite => { Assert.Equal(new[] { "C3", "C1" }, composite.Properties.Select(c => c.Name).ToArray()); });
        }

        [Fact]
        public void Foreign_key()

        {
            var parentTable = new Table { Name = "Parent", Columns = { new Column { Name = "Id", DataType = "long", PrimaryKeyOrdinal = 1 } } };
            var childrenTable = new Table
            {
                Name = "Children",
                Columns =
                {
                    new Column { Name = "Id", DataType = "long", PrimaryKeyOrdinal = 1 },
                    new Column { Name = "ParentId", DataType = "long", IsNullable = true }
                }
            };
            childrenTable.ForeignKeys.Add(new ForeignKey
            {
                Table = childrenTable,
                From = { childrenTable.Columns[1] },
                PrincipalTable = parentTable,
                To = { parentTable.Columns[0] }
            });

            var model = _provider.GetModel(new SchemaInfo { Tables = { parentTable, childrenTable } });

            var parent = (EntityType)model.GetEntityType("Parent");

            var children = (EntityType)model.GetEntityType("Children");

            Assert.NotEmpty(parent.FindReferencingForeignKeys());
            var fk = Assert.Single(children.GetForeignKeys());
            Assert.False(fk.IsUnique);

            var principalKey = fk.PrincipalKey;

            Assert.Same(parent, principalKey.DeclaringEntityType);
            Assert.Same(parent.Properties.First(), principalKey.Properties[0]);
        }

        [Fact]
        public void Unique_foreign_key()

        {
            var parentTable = new Table { Name = "Parent", Columns = { new Column { Name = "Id", DataType = "long", PrimaryKeyOrdinal = 1 } } };
            var childrenTable = new Table
            {
                Name = "Children",
                Columns =
                {
                    new Column { Name = "Id", DataType = "long", PrimaryKeyOrdinal = 1 }
                }
            };
            childrenTable.ForeignKeys.Add(new ForeignKey
            {
                Table = childrenTable,
                From = { childrenTable.Columns[0] },
                PrincipalTable = parentTable,
                To = { parentTable.Columns[0] }
            });

            var model = _provider.GetModel(new SchemaInfo { Tables = { parentTable, childrenTable } });

            var children = (EntityType)model.GetEntityType("Children");

            var fk = Assert.Single(children.GetForeignKeys());
            Assert.True(fk.IsUnique);
        }

        [Fact]
        public void Composite_foreign_key()

        {
            var parentTable = new Table
            {
                Name = "Parent",
                Columns =
                {
                    new Column { Name = "Id_A", DataType = "long", PrimaryKeyOrdinal = 1 },
                    new Column { Name = "Id_B", DataType = "long", PrimaryKeyOrdinal = 2 }
                }
            };
            var childrenTable = new Table
            {
                Name = "Children",
                Columns =
                {
                    new Column { Name = "Id", DataType = "long", PrimaryKeyOrdinal = 1 },
                    new Column { Name = "ParentId_A", DataType = "long" },
                    new Column { Name = "ParentId_B", DataType = "long" }
                }
            };
            childrenTable.ForeignKeys.Add(new ForeignKey
            {
                Table = childrenTable,
                From = { childrenTable.Columns[1], childrenTable.Columns[2] },
                PrincipalTable = parentTable,
                To = { parentTable.Columns[0], parentTable.Columns[1] }
            });

            var model = _provider.GetModel(new SchemaInfo { Tables = { parentTable, childrenTable } });

            var parent = (EntityType)model.GetEntityType("Parent");

            var children = (EntityType)model.GetEntityType("Children");

            Assert.NotEmpty(parent.FindReferencingForeignKeys());

            var fk = Assert.Single(children.GetForeignKeys());
            Assert.False(fk.IsUnique);

            var principalKey = fk.PrincipalKey;

            Assert.Equal("Parent", principalKey.DeclaringEntityType.Name);
            Assert.Equal("Id_A", principalKey.Properties[0].Name);
            Assert.Equal("Id_B", principalKey.Properties[1].Name);
        }

        [Fact]
        public void It_loads_self_referencing_foreign_key()

        {
            var table = new Table
            {
                Name = "ItemsList",
                Columns =
                {
                    new Column { Name = "Id", DataType = "long", PrimaryKeyOrdinal = 1 },
                    new Column { Name = "ParentId", DataType = "long", IsNullable = false }
                }
            };
            table.ForeignKeys.Add(new ForeignKey
            {
                Table = table,
                From = { table.Columns[1] },
                PrincipalTable = table,
                To = { table.Columns[0] }
            });

            var model = _provider.GetModel(new SchemaInfo { Tables = { table } });
            var list = model.GetEntityType("ItemsList");

            Assert.NotEmpty(list.FindReferencingForeignKeys());
            Assert.NotEmpty(list.GetForeignKeys());

            var principalKey = list.GetForeignKey(list.FindProperty("ParentId")).PrincipalKey;
            Assert.Equal("ItemsList", principalKey.EntityType.Name);
            Assert.Equal("Id", principalKey.Properties[0].Name);
        }

        [Fact]
        public void It_logs_warning_for_bad_foreign_key()
        {
            var parentTable = new Table
            {
                Name = "Parent",
                Columns =
                {
                    new Column { Name = "NotPkId", DataType = "long", PrimaryKeyOrdinal = null }
                }
            };
            var childrenTable = new Table
            {
                Name = "Children",
                Columns =
                {
                    new Column { Name = "Id", DataType = "long", PrimaryKeyOrdinal = 1 },
                    new Column { Name = "ParentId", DataType = "long" }
                }
            };
            childrenTable.ForeignKeys.Add(new ForeignKey
            {
                Table = childrenTable,
                From = { childrenTable.Columns[1] },
                PrincipalTable = parentTable,
                To = { parentTable.Columns[0] }
            });

            _provider.GetModel(new SchemaInfo { Tables = { parentTable, childrenTable } });

            Assert.Contains("Warning: " + RelationalDesignStrings.ForeignKeyScaffoldError(childrenTable.ForeignKeys[0].DisplayName()), _logger.FullLog);
        }

        [Fact]
        public void Unique_index_foreign_key()
        {
            var table = new Table
            {
                Name = "Friends",
                Columns =
                {
                    new Column { Name = "Id", DataType = "long", PrimaryKeyOrdinal = 1 },
                    new Column { Name = "BuddyId", DataType = "long", IsNullable = false }
                }
            };
            table.Indexes.Add(new Index { Columns = { table.Columns[1] }, IsUnique = true });
            table.ForeignKeys.Add(new ForeignKey
            {
                Table = table,
                From = { table.Columns[1] },
                PrincipalTable = table,
                To = { table.Columns[0] }
            });

            var model = _provider.GetModel(new SchemaInfo { Tables = { table } }).GetEntityType("Friends");

            var fk = Assert.Single(model.GetForeignKeys());

            Assert.True(fk.IsUnique);
            Assert.Equal(model.GetPrimaryKey(), fk.PrincipalKey);
        }

        [Fact]
        public void Unique_index_composite_foreign_key()
        {
            var parentTable = new Table
            {
                Name = "Parent",
                Columns =
                {
                    new Column { Name = "Id_A", DataType = "long", PrimaryKeyOrdinal = 1 },
                    new Column { Name = "Id_B", DataType = "long", PrimaryKeyOrdinal = 2 }
                }
            };
            var childrenTable = new Table
            {
                Name = "Children",
                Columns =
                {
                    new Column { Name = "Id", DataType = "long", PrimaryKeyOrdinal = 1 },
                    new Column { Name = "ParentId_A", DataType = "long" },
                    new Column { Name = "ParentId_B", DataType = "long" }
                }
            };
            childrenTable.Indexes.Add(new Index { IsUnique = true, Columns = { childrenTable.Columns[1], childrenTable.Columns[2] } });
            childrenTable.ForeignKeys.Add(new ForeignKey
            {
                Table = childrenTable,
                From = { childrenTable.Columns[1], childrenTable.Columns[2] },
                PrincipalTable = parentTable,
                To = { parentTable.Columns[0], parentTable.Columns[1] }
            });

            var model = _provider.GetModel(new SchemaInfo { Tables = { parentTable, childrenTable } });
            var parent = model.GetEntityType("Parent");
            var children = model.GetEntityType("Children");

            var fk = Assert.Single(children.GetForeignKeys());

            Assert.True(fk.IsUnique);
            Assert.Equal(parent.GetPrimaryKey(), fk.PrincipalKey);
        }

        [Fact]
        public void Unique_names()
        {
            var info = new SchemaInfo
            {
                Tables =
                {
                    new Table
                    {
                        Name = "E F", Columns =
                        {
                            new Column { Name = "San itized", DataType = "long" },
                            new Column { Name = "San_itized", DataType = "long" }
                        }
                    },
                    new Table { Name = "E_F" }
                }
            };

            info.Tables[0].Columns.Add(new Column { Name = "Id", DataType = "long", Table = info.Tables[0] });
            info.Tables[1].Columns.Add(new Column { Name = "Id", DataType = "long", Table = info.Tables[1] });

            var model = _provider.GetModel(info);

            Assert.Collection(model.EntityTypes.Cast<EntityType>(),
                ef1 =>
                    {
                        Assert.Equal("E F", ef1.Relational().TableName);
                        Assert.Equal("E_F", ef1.Name);
                        Assert.Collection(ef1.Properties.OfType<Property>(),
                            id => { Assert.Equal("Id", id.Name); },
                            s1 =>
                                {
                                    Assert.Equal("San_itized", s1.Name);
                                    Assert.Equal("San itized", s1.Relational().ColumnName);
                                },
                            s2 =>
                                {
                                    Assert.Equal("San_itized1", s2.Name);
                                    Assert.Equal("San_itized", s2.Relational().ColumnName);
                                });
                    },
                ef2 =>
                    {
                        Assert.Equal("E_F", ef2.Relational().TableName);
                        Assert.Equal("E_F1", ef2.Name);
                        var id = Assert.Single(ef2.Properties.OfType<Property>());
                        Assert.Equal("Id", id.Name);
                        Assert.Equal("Id", id.Relational().ColumnName);
                    });
        }
    }

    public class FakeMetadataModelProvider : RelationalMetadataModelProvider
    {
        public new IModel GetModel(SchemaInfo schemaInfo) => base.GetModel(schemaInfo);

        public override IModel GetModel([NotNull] string connectionString, [CanBeNull] TableSelectionSet tableSelectionSet)
        {
            throw new NotImplementedException();
        }

        public FakeMetadataModelProvider(
            [NotNull] ILoggerFactory loggerFactory)
            : base(loggerFactory,
                new TestTypeMapper())
        {
        }
    }

    public class TestTypeMapper : RelationalTypeMapper
    {
        private static readonly RelationalTypeMapping _string = new RelationalTypeMapping("string", typeof(string));
        private static readonly RelationalTypeMapping _long = new RelationalTypeMapping("long", typeof(long));

        protected override IReadOnlyDictionary<Type, RelationalTypeMapping> SimpleMappings { get; }
            = new Dictionary<Type, RelationalTypeMapping>
            {
                { typeof(string), _string },
                { typeof(long), _long }
            };

        protected override IReadOnlyDictionary<string, RelationalTypeMapping> SimpleNameMappings { get; }
            = new Dictionary<string, RelationalTypeMapping>(StringComparer.OrdinalIgnoreCase)
            {
                { "string", _string },
                { "alias for string", _string },
                { "long", _long }
            };

        protected override string GetColumnType(IProperty property) => ((Property)property).Relational().ColumnType;
    }
}

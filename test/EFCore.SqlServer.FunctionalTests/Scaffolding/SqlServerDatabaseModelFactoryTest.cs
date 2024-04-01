// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.SqlServer.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.Scaffolding;

public class SqlServerDatabaseModelFactoryTest : IClassFixture<SqlServerDatabaseModelFactoryTest.SqlServerDatabaseModelFixture>
{
    protected SqlServerDatabaseModelFixture Fixture { get; }

    public SqlServerDatabaseModelFactoryTest(SqlServerDatabaseModelFixture fixture)
    {
        Fixture = fixture;
        Fixture.OperationReporter.Clear();
    }

    #region Sequences

    [ConditionalFact]
    public void Create_sequences_with_facets()
        => Test(
            @"
CREATE SEQUENCE DefaultFacetsSequence;

CREATE SEQUENCE db2.CustomFacetsSequence
    AS int
    START WITH 3
    INCREMENT BY 2
    MAXVALUE 8
    MINVALUE -3
    CYCLE
    CACHE 20;",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var defaultSequence = dbModel.Sequences.First(ds => ds.Name == "DefaultFacetsSequence");
                Assert.Equal("dbo", defaultSequence.Schema);
                Assert.Equal("DefaultFacetsSequence", defaultSequence.Name);
                Assert.Equal("bigint", defaultSequence.StoreType);
                Assert.False(defaultSequence.IsCyclic);
                Assert.Equal(1, defaultSequence.IncrementBy);
                Assert.Null(defaultSequence.StartValue);
                Assert.Null(defaultSequence.MinValue);
                Assert.Null(defaultSequence.MaxValue);
                Assert.True(defaultSequence.IsCached);
                Assert.Null(defaultSequence.CacheSize);

                var customSequence = dbModel.Sequences.First(ds => ds.Name == "CustomFacetsSequence");
                Assert.Equal("db2", customSequence.Schema);
                Assert.Equal("CustomFacetsSequence", customSequence.Name);
                Assert.Equal("int", customSequence.StoreType);
                Assert.True(customSequence.IsCyclic);
                Assert.Equal(2, customSequence.IncrementBy);
                Assert.Equal(3, customSequence.StartValue);
                Assert.Equal(-3, customSequence.MinValue);
                Assert.Equal(8, customSequence.MaxValue);

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Empty(model.GetEntityTypes());
                Assert.Collection(model.GetSequences(),
                    s =>
                    {
                        Assert.Equal("db2", s.Schema);
                        Assert.Equal("CustomFacetsSequence", s.Name);
                        Assert.Same(typeof(int), s.Type);
                        Assert.True(s.IsCyclic);
                        Assert.Equal(2, s.IncrementBy);
                        Assert.Equal(3, s.StartValue);
                        Assert.Equal(-3, s.MinValue);
                        Assert.Equal(8, s.MaxValue);
                    },
                    s =>
                    {
                        Assert.Equal("dbo", s.Schema);
                        Assert.Equal("DefaultFacetsSequence", s.Name);
                        Assert.Same(typeof(long), s.Type);
                        Assert.False(s.IsCyclic);
                        Assert.Equal(1, s.IncrementBy);
                        Assert.Equal(1, s.StartValue);
                        Assert.Null(s.MinValue);
                        Assert.Null(s.MaxValue);
                    });
                Assert.True(customSequence.IsCached);
                Assert.Equal(20, customSequence.CacheSize);
            },
            @"
DROP SEQUENCE DefaultFacetsSequence;

DROP SEQUENCE db2.CustomFacetsSequence");

    [ConditionalFact]
    public void Create_sequences_caches()
    => Test(
        @"
CREATE SEQUENCE db2.DefaultCacheSequence
    CACHE;

CREATE SEQUENCE db2.NoCacheSequence
    NO CACHE;

CREATE SEQUENCE db2.CacheSequence
    CACHE 20;",
        Enumerable.Empty<string>(),
        Enumerable.Empty<string>(),
        (dbModel, scaffoldingFactory) =>
        {
            var defaultCacheSequence = dbModel.Sequences.First(ds => ds.Name == "DefaultCacheSequence");
            Assert.Equal("db2", defaultCacheSequence.Schema);
            Assert.Equal("DefaultCacheSequence", defaultCacheSequence.Name);
            Assert.Equal("bigint", defaultCacheSequence.StoreType);
            Assert.False(defaultCacheSequence.IsCyclic);
            Assert.Equal(1, defaultCacheSequence.IncrementBy);
            Assert.Null(defaultCacheSequence.StartValue);
            Assert.Null(defaultCacheSequence.MinValue);
            Assert.Null(defaultCacheSequence.MaxValue);
            Assert.True(defaultCacheSequence.IsCached);
            Assert.Null(defaultCacheSequence.CacheSize);

            var noCacheSequence = dbModel.Sequences.First(ds => ds.Name == "NoCacheSequence");
            Assert.Equal("db2", noCacheSequence.Schema);
            Assert.Equal("NoCacheSequence", noCacheSequence.Name);
            Assert.Equal("bigint", noCacheSequence.StoreType);
            Assert.False(noCacheSequence.IsCyclic);
            Assert.Equal(1, noCacheSequence.IncrementBy);
            Assert.Null(noCacheSequence.StartValue);
            Assert.Null(noCacheSequence.MinValue);
            Assert.Null(noCacheSequence.MaxValue);
            Assert.False(noCacheSequence.IsCached);
            Assert.Null(noCacheSequence.CacheSize);

            var cacheSequence = dbModel.Sequences.First(ds => ds.Name == "CacheSequence");
            Assert.Equal("db2", cacheSequence.Schema);
            Assert.Equal("CacheSequence", cacheSequence.Name);
            Assert.Equal("bigint", cacheSequence.StoreType);
            Assert.False(cacheSequence.IsCyclic);
            Assert.Equal(1, cacheSequence.IncrementBy);
            Assert.Null(cacheSequence.StartValue);
            Assert.Null(cacheSequence.MinValue);
            Assert.Null(cacheSequence.MaxValue);
            Assert.True(cacheSequence.IsCached);
            Assert.Equal(20, cacheSequence.CacheSize);
        },
        @"
DROP SEQUENCE db2.DefaultCacheSequence;

DROP SEQUENCE db2.NoCacheSequence;

DROP SEQUENCE db2.CacheSequence;");


    [ConditionalFact]
    public void Sequence_min_max_start_values_are_null_if_default()
        => Test(
            @"
CREATE SEQUENCE [TinyIntSequence] AS tinyint;

CREATE SEQUENCE [SmallIntSequence] AS smallint;

CREATE SEQUENCE [IntSequence] AS int;

CREATE SEQUENCE [BigIntSequence] AS bigint;",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                Assert.All(
                    dbModel.Sequences,
                    s =>
                    {
                        Assert.Null(s.StartValue);
                        Assert.Null(s.MinValue);
                        Assert.Null(s.MaxValue);
                        Assert.False(s.IsCyclic);
                        Assert.True(s.IsCached);
                        Assert.Null(s.CacheSize);
                    });

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Empty(model.GetEntityTypes());
                Assert.Collection(model.GetSequences(),
                    s =>
                    {
                        Assert.Equal("dbo", s.Schema);
                        Assert.Equal("BigIntSequence", s.Name);
                        Assert.Same(typeof(long), s.Type);
                        Assert.False(s.IsCyclic);
                        Assert.Equal(1, s.IncrementBy);
                        Assert.Equal(1, s.StartValue);
                        Assert.Null(s.MinValue);
                        Assert.Null(s.MaxValue);
                    },
                    s =>
                    {
                        Assert.Equal("dbo", s.Schema);
                        Assert.Equal("IntSequence", s.Name);
                        Assert.Same(typeof(int), s.Type);
                        Assert.False(s.IsCyclic);
                        Assert.Equal(1, s.IncrementBy);
                        Assert.Equal(1, s.StartValue);
                        Assert.Null(s.MinValue);
                        Assert.Null(s.MaxValue);
                    },
                    s =>
                    {
                        Assert.Equal("dbo", s.Schema);
                        Assert.Equal("SmallIntSequence", s.Name);
                        Assert.Same(typeof(short), s.Type);
                        Assert.False(s.IsCyclic);
                        Assert.Equal(1, s.IncrementBy);
                        Assert.Equal(1, s.StartValue);
                        Assert.Null(s.MinValue);
                        Assert.Null(s.MaxValue);
                    },
                    s =>
                    {
                        Assert.Equal("dbo", s.Schema);
                        Assert.Equal("TinyIntSequence", s.Name);
                        Assert.Same(typeof(byte), s.Type);
                        Assert.False(s.IsCyclic);
                        Assert.Equal(1, s.IncrementBy);
                        Assert.Equal(1, s.StartValue);
                        Assert.Null(s.MinValue);
                        Assert.Null(s.MaxValue);
                    });
            },
            @"
DROP SEQUENCE [TinyIntSequence];

DROP SEQUENCE [SmallIntSequence];

DROP SEQUENCE [IntSequence];

DROP SEQUENCE [BigIntSequence];");

    [ConditionalFact]
    public void Sequence_min_max_start_values_are_not_null_if_decimal()
        => Test(
            @"
CREATE SEQUENCE [DecimalSequence] AS decimal;

CREATE SEQUENCE [NumericSequence] AS numeric;",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                Assert.All(
                    dbModel.Sequences,
                    s =>
                    {
                        Assert.NotNull(s.StartValue);
                        Assert.NotNull(s.MinValue);
                        Assert.NotNull(s.MaxValue);
                        Assert.False(s.IsCyclic);
                        Assert.True(s.IsCached);
                        Assert.Null(s.CacheSize);
                    });

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Empty(model.GetEntityTypes());
                Assert.Collection(model.GetSequences(),
                    s =>
                    {
                        Assert.Equal("dbo", s.Schema);
                        Assert.Equal("DecimalSequence", s.Name);
                        Assert.Same(typeof(decimal), s.Type);
                        Assert.False(s.IsCyclic);
                        Assert.Equal(1, s.IncrementBy);
                        Assert.Equal(-999999999999999999, s.StartValue);
                        Assert.Equal(-999999999999999999, s.MinValue);
                        Assert.Equal(999999999999999999, s.MaxValue);
                    },
                    s =>
                    {
                        Assert.Equal("dbo", s.Schema);
                        Assert.Equal("NumericSequence", s.Name);
                        Assert.Same(typeof(decimal), s.Type);
                        Assert.False(s.IsCyclic);
                        Assert.Equal(1, s.IncrementBy);
                        Assert.Equal(-999999999999999999, s.StartValue);
                        Assert.Equal(-999999999999999999, s.MinValue);
                        Assert.Equal(999999999999999999, s.MaxValue);
                    });
            },
            @"
DROP SEQUENCE [DecimalSequence];

DROP SEQUENCE [NumericSequence];");

    [ConditionalFact]
    public void Sequence_high_min_max_start_values_are_not_null_if_decimal()
        => Test(
            @"
CREATE SEQUENCE [dbo].[HighDecimalSequence]
 AS [numeric](38, 0)
 START WITH -99999999999999999999999999999999999999
 INCREMENT BY 1
 MINVALUE -99999999999999999999999999999999999999
 MAXVALUE 99999999999999999999999999999999999999
 CACHE;",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                Assert.All(
                    dbModel.Sequences,
                    s =>
                    {
                        Assert.NotNull(s.StartValue);
                        Assert.Equal(long.MinValue, s.StartValue);
                        Assert.NotNull(s.MinValue);
                        Assert.Equal(long.MinValue, s.MinValue);
                        Assert.NotNull(s.MaxValue);
                        Assert.Equal(long.MaxValue, s.MaxValue);
                        Assert.True(s.IsCached);
                        Assert.Null(s.CacheSize);
                    });

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Empty(model.GetEntityTypes());
                Assert.Collection(model.GetSequences(),
                    s =>
                    {
                        Assert.Equal("dbo", s.Schema);
                        Assert.Equal("HighDecimalSequence", s.Name);
                        Assert.Same(typeof(decimal), s.Type);
                        Assert.False(s.IsCyclic);
                        Assert.Equal(1, s.IncrementBy);
                        Assert.Equal(long.MinValue, s.StartValue);
                        Assert.Equal(long.MinValue, s.MinValue);
                        Assert.Equal(long.MaxValue, s.MaxValue);
                    });
            },
            @"
DROP SEQUENCE [HighDecimalSequence];");

    [ConditionalFact]
    public void Sequence_using_type_alias()
    {
        Fixture.TestStore.ExecuteNonQuery(
            @"
CREATE TYPE [dbo].[TestTypeAlias] FROM int;");

        Test(
            @"
CREATE SEQUENCE [TypeAliasSequence] AS [dbo].[TestTypeAlias];",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var sequence = Assert.Single(dbModel.Sequences);
                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("dbo", sequence.Schema);
                Assert.Equal("TypeAliasSequence", sequence.Name);
                Assert.Equal("int", sequence.StoreType);
                Assert.False(sequence.IsCyclic);
                Assert.Equal(1, sequence.IncrementBy);
                Assert.Null(sequence.StartValue);
                Assert.Null(sequence.MinValue);
                Assert.Null(sequence.MaxValue);
                Assert.True(sequence.IsCached);
                Assert.Null(sequence.CacheSize);

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Empty(model.GetEntityTypes());
                Assert.Collection(model.GetSequences(),
                    s =>
                    {
                        Assert.Equal("dbo", s.Schema);
                        Assert.Equal("TypeAliasSequence", s.Name);
                        Assert.Same(typeof(int), s.Type);
                        Assert.False(s.IsCyclic);
                        Assert.Equal(1, s.IncrementBy);
                        Assert.Equal(1, s.StartValue);
                        Assert.Null(s.MinValue);
                        Assert.Null(s.MaxValue);
                    });
            },
            @"
DROP SEQUENCE [TypeAliasSequence];
DROP TYPE [dbo].[TestTypeAlias];");
    }

    [ConditionalFact]
    public void Sequence_using_type_with_facets()
        => Test(
            @"
CREATE SEQUENCE [TypeFacetSequence] AS decimal(10, 0);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var sequence = Assert.Single(dbModel.Sequences);
                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("dbo", sequence.Schema);
                Assert.Equal("TypeFacetSequence", sequence.Name);
                Assert.Equal("decimal(10, 0)", sequence.StoreType);
                Assert.False(sequence.IsCyclic);
                Assert.Equal(1, sequence.IncrementBy);
                Assert.True(sequence.IsCached);
                Assert.Null(sequence.CacheSize);

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Empty(model.GetEntityTypes());
                Assert.Collection(model.GetSequences(),
                    s =>
                    {
                        Assert.Equal("dbo", s.Schema);
                        Assert.Equal("TypeFacetSequence", s.Name);
                        Assert.Same(typeof(decimal), s.Type);
                        Assert.False(s.IsCyclic);
                        Assert.Equal(1, s.IncrementBy);
                        Assert.Equal(-9999999999, s.StartValue);
                        Assert.Equal(-9999999999, s.MinValue);
                        Assert.Equal(9999999999, s.MaxValue);
                    });
            },
            @"
DROP SEQUENCE [TypeFacetSequence];");

    [ConditionalFact]
    public void Filter_sequences_based_on_schema()
        => Test(
            @"
CREATE SEQUENCE [dbo].[Sequence];

CREATE SEQUENCE [db2].[Sequence]",
            Enumerable.Empty<string>(),
            new[] { "db2" },
            (dbModel, scaffoldingFactory) =>
            {
                var sequence = Assert.Single(dbModel.Sequences);
                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("db2", sequence.Schema);
                Assert.Equal("Sequence", sequence.Name);
                Assert.Equal("bigint", sequence.StoreType);
                Assert.False(sequence.IsCyclic);
                Assert.Equal(1, sequence.IncrementBy);
                Assert.True(sequence.IsCached);
                Assert.Null(sequence.CacheSize);

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Empty(model.GetEntityTypes());
                Assert.Collection(model.GetSequences(),
                    s =>
                    {
                        Assert.Equal("db2", s.Schema);
                        Assert.Equal("Sequence", s.Name);
                        Assert.Same(typeof(long), s.Type);
                        Assert.False(s.IsCyclic);
                        Assert.Equal(1, s.IncrementBy);
                        Assert.Equal(1, s.StartValue);
                        Assert.Null(s.MinValue);
                        Assert.Null(s.MaxValue);
                    });
            },
            @"
DROP SEQUENCE [dbo].[Sequence];

DROP SEQUENCE [db2].[Sequence];");

    #endregion

    #region Model

    [ConditionalFact]
    public void Set_default_schema()
        => Test(
            "SELECT 1",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var defaultSchema = Fixture.TestStore.ExecuteScalar<string>("SELECT SCHEMA_NAME()");
                Assert.Equal(defaultSchema, dbModel.DefaultSchema);

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Equal("dbo", model.GetDefaultSchema());
            },
            null);

    [ConditionalFact]
    public void Create_tables()
        => Test(
            @"
CREATE TABLE [dbo].[Everest] ( id int );

CREATE TABLE [dbo].[Denali] ( id int );",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                Assert.Collection(
                    dbModel.Tables.OrderBy(t => t.Name),
                    d =>
                    {
                        Assert.Equal("dbo", d.Schema);
                        Assert.Equal("Denali", d.Name);
                    },
                    e =>
                    {
                        Assert.Equal("dbo", e.Schema);
                        Assert.Equal("Everest", e.Name);
                    });

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Collection(
                    model.GetEntityTypes(),
                    e =>
                    {
                        Assert.Equal("dbo", e.GetSchema());
                        Assert.Equal("Denali", e.Name);
                        Assert.Null(e.FindPrimaryKey());
                        Assert.Collection(e.GetProperties(), p => Assert.Equal("Id", p.Name));
                        Assert.Empty(e.GetForeignKeys());
                        Assert.Empty(e.GetNavigations());
                        Assert.Empty(e.GetSkipNavigations());
                    }, e =>
                    {
                        Assert.Equal("dbo", e.GetSchema());
                        Assert.Equal("Everest", e.Name);
                        Assert.Null(e.FindPrimaryKey());
                        Assert.Collection(e.GetProperties(), p => Assert.Equal("Id", p.Name));
                        Assert.Empty(e.GetForeignKeys());
                        Assert.Empty(e.GetNavigations());
                        Assert.Empty(e.GetSkipNavigations());
                    });
            },
            @"
DROP TABLE [dbo].[Everest];

DROP TABLE [dbo].[Denali];");

    [ConditionalFact]
    public void Scaffold_relationships_in_order()
        => Test(
            @"
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[TableC](
	[IdC] [BIGINT] IDENTITY(1,1) NOT NULL,
 CONSTRAINT [PK_IdC] PRIMARY KEY CLUSTERED
(
	[IdC] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];

CREATE TABLE [dbo].[TableB](
	[IdB] [BIGINT] IDENTITY(1,1) NOT NULL,
 CONSTRAINT [PK_IdB] PRIMARY KEY CLUSTERED
(
	[IdB] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

CREATE TABLE [dbo].[TableAB](
	[IdA] [BIGINT] IDENTITY(1,1) NOT NULL,
	[IdB] [BIGINT] NOT NULL,
 CONSTRAINT [PK_IdA] PRIMARY KEY CLUSTERED
(
	[IdA] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [UC_IdA_IdB] UNIQUE NONCLUSTERED
(
	[IdA] ASC,
	[IdB] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[TableAB]  WITH CHECK ADD  CONSTRAINT [FK_Listings_Category] FOREIGN KEY([IdB])
REFERENCES [dbo].[TableB] ([IdB])

CREATE TABLE [dbo].[AttributesByCategory](
	[IdB] [BIGINT] NOT NULL,
	[IdC] [BIGINT] NOT NULL,
 CONSTRAINT [PK_IdB_IdC] PRIMARY KEY CLUSTERED
(
	[IdB] ASC,
	[IdC] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[AttributesByCategory]  WITH CHECK ADD  CONSTRAINT [FK_AttributesByCategory_Attributes] FOREIGN KEY([IdC])
REFERENCES [dbo].[TableC] ([IdC])

ALTER TABLE [dbo].[AttributesByCategory] CHECK CONSTRAINT [FK_AttributesByCategory_Attributes]

ALTER TABLE [dbo].[AttributesByCategory]  WITH CHECK ADD  CONSTRAINT [FK_AttributesByCategory_Category] FOREIGN KEY([IdB])
REFERENCES [dbo].[TableB] ([IdB])

ALTER TABLE [dbo].[AttributesByCategory] CHECK CONSTRAINT [FK_AttributesByCategory_Category];

CREATE TABLE [dbo].[Properties](
	[IdA] [BIGINT] NOT NULL,
	[IdB] [BIGINT] NOT NULL,
	[IdC] [BIGINT] NOT NULL,
 CONSTRAINT [PK_IdA_IdB_IdC] PRIMARY KEY CLUSTERED
(
	[IdA] ASC,
	[IdB] ASC,
	[IdC] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [UC_IdA_IdC] UNIQUE NONCLUSTERED
(
	[IdA] ASC,
	[IdC] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[Properties]  WITH CHECK ADD  CONSTRAINT [FK_Properties_AttributesByCategory] FOREIGN KEY([IdB], [IdC])
REFERENCES [dbo].[AttributesByCategory] ([IdB], [IdC])

ALTER TABLE [dbo].[Properties] CHECK CONSTRAINT [FK_Properties_AttributesByCategory]

ALTER TABLE [dbo].[Properties]  WITH CHECK ADD  CONSTRAINT [FK_Properties_Listings] FOREIGN KEY([IdA], [IdB])
REFERENCES [dbo].[TableAB] ([IdA], [IdB])

ALTER TABLE [dbo].[Properties] CHECK CONSTRAINT [FK_Properties_Listings]
",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                Assert.Collection(
                    dbModel.Tables.OrderBy(t => t.Name),
                    t =>
                    {
                        Assert.Equal("dbo", t.Schema);
                        Assert.Equal("AttributesByCategory", t.Name);
                        Assert.Collection(t.Columns,
                            c => Assert.Equal("IdB", c.Name),
                            c => Assert.Equal("IdC", c.Name));
                        Assert.Collection(t.PrimaryKey!.Columns,
                            c => Assert.Equal("IdB", c.Name),
                            c => Assert.Equal("IdC", c.Name));
                        Assert.Collection(
                            t.ForeignKeys,
                            k =>
                            {
                                Assert.Equal("TableC", k.PrincipalTable.Name);
                                Assert.Collection(k.Columns, c => Assert.Equal("IdC", c.Name));
                                Assert.Collection(k.PrincipalColumns, c => Assert.Equal("IdC", c.Name));
                            },
                            k =>
                            {
                                Assert.Equal("TableB", k.PrincipalTable.Name);
                                Assert.Collection(k.Columns, c => Assert.Equal("IdB", c.Name));
                                Assert.Collection(k.PrincipalColumns, c => Assert.Equal("IdB", c.Name));
                            });
                        Assert.Empty(t.UniqueConstraints);
                        Assert.Empty(t.Indexes);
                    },
                    t =>
                    {
                        Assert.Equal("dbo", t.Schema);
                        Assert.Equal("Properties", t.Name);
                        Assert.Collection(t.Columns,
                            c => Assert.Equal("IdA", c.Name),
                            c => Assert.Equal("IdB", c.Name),
                            c => Assert.Equal("IdC", c.Name));
                        Assert.Collection(t.PrimaryKey!.Columns,
                            c => Assert.Equal("IdA", c.Name),
                            c => Assert.Equal("IdB", c.Name),
                            c => Assert.Equal("IdC", c.Name));
                        Assert.Collection(t.UniqueConstraints, u => Assert.Collection(u.Columns,
                            c => Assert.Equal("IdA", c.Name),
                            c => Assert.Equal("IdC", c.Name)));
                        Assert.Collection(
                            t.ForeignKeys,
                            k =>
                            {
                                Assert.Equal("AttributesByCategory", k.PrincipalTable.Name);
                                Assert.Collection(k.Columns, c => Assert.Equal("IdB", c.Name), c => Assert.Equal("IdC", c.Name));
                                Assert.Collection(k.PrincipalColumns, c => Assert.Equal("IdB", c.Name), c => Assert.Equal("IdC", c.Name));
                            },
                            k =>
                            {
                                Assert.Equal("TableAB", k.PrincipalTable.Name);
                                Assert.Collection(k.Columns, c => Assert.Equal("IdA", c.Name), c => Assert.Equal("IdB", c.Name));
                                Assert.Collection(k.PrincipalColumns, c => Assert.Equal("IdA", c.Name), c => Assert.Equal("IdB", c.Name));
                            });
                        Assert.Empty(t.Indexes);
                    },
                    t =>
                    {
                        Assert.Equal("dbo", t.Schema);
                        Assert.Equal("TableAB", t.Name);
                        Assert.Collection(t.Columns,
                            c => Assert.Equal("IdA", c.Name),
                            c => Assert.Equal("IdB", c.Name));
                        Assert.Collection(t.PrimaryKey!.Columns, c => Assert.Equal("IdA", c.Name));
                        Assert.Collection(t.UniqueConstraints, u => Assert.Collection(u.Columns,
                            c => Assert.Equal("IdA", c.Name),
                            c => Assert.Equal("IdB", c.Name)));
                        Assert.Collection(t.ForeignKeys, k =>
                        {
                            Assert.Equal("TableB", k.PrincipalTable.Name);
                            Assert.Collection(k.Columns, c => Assert.Equal("IdB", c.Name));
                            Assert.Collection(k.PrincipalColumns, c => Assert.Equal("IdB", c.Name));
                        });
                        Assert.Empty(t.Indexes);
                    },
                    t =>
                    {
                        Assert.Equal("dbo", t.Schema);
                        Assert.Equal("TableB", t.Name);
                        Assert.Collection(t.Columns, c => Assert.Equal("IdB", c.Name));
                        Assert.Collection(t.PrimaryKey!.Columns, c => Assert.Equal("IdB", c.Name));
                        Assert.Empty(t.ForeignKeys);
                        Assert.Empty(t.UniqueConstraints);
                        Assert.Empty(t.Indexes);
                    },
                    t =>
                    {
                        Assert.Equal("dbo", t.Schema);
                        Assert.Equal("TableC", t.Name);
                        Assert.Collection(t.Columns, c => Assert.Equal("IdC", c.Name));
                        Assert.Collection(t.PrimaryKey!.Columns, c => Assert.Equal("IdC", c.Name));
                        Assert.Empty(t.ForeignKeys);
                        Assert.Empty(t.UniqueConstraints);
                        Assert.Empty(t.Indexes);
                    });

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Collection(
                    model.GetEntityTypes(),
                    e =>
                    {
                        Assert.Equal("AttributesByCategory", e.Name);
                        Assert.Collection(e.GetProperties(),
                            p => Assert.Equal("IdB", p.Name),
                            p => Assert.Equal("IdC", p.Name));
                        Assert.Collection(e.GetKeys(),
                            c => Assert.Collection(c.Properties,
                                p => Assert.Equal("IdB", p.Name),
                                p => Assert.Equal("IdC", p.Name)));
                        Assert.Collection(
                            e.GetForeignKeys(),
                            k =>
                            {
                                Assert.Equal("TableB", k.PrincipalEntityType.Name);
                                Assert.Collection(k.Properties, p => Assert.Equal("IdB", p.Name));
                                Assert.Collection(k.PrincipalKey.Properties, p => Assert.Equal("IdB", p.Name));
                                Assert.False(k.IsUnique);
                            },
                            k =>
                            {
                                Assert.Equal("TableC", k.PrincipalEntityType.Name);
                                Assert.Collection(k.Properties, p => Assert.Equal("IdC", p.Name));
                                Assert.Collection(k.PrincipalKey.Properties, p => Assert.Equal("IdC", p.Name));
                                Assert.False(k.IsUnique);
                            });
                        Assert.Empty(e.GetIndexes());
                        Assert.Empty(e.GetSkipNavigations());
                        Assert.Collection(e.GetNavigations(),
                            n =>
                            {
                                Assert.Equal("TableB", n.TargetEntityType.Name);
                                Assert.Equal("IdBNavigation", n.Name);
                                Assert.False(n.IsCollection);
                            },
                            n =>
                            {
                                Assert.Equal("TableC", n.TargetEntityType.Name);
                                Assert.Equal("IdCNavigation", n.Name);
                                Assert.False(n.IsCollection);
                            },
                            n =>
                            {
                                Assert.Equal("Property", n.TargetEntityType.Name);
                                Assert.Equal("Properties", n.Name);
                                Assert.True(n.IsCollection);
                            });
                        Assert.Empty(e.GetIndexes());
                    },
                    e =>
                    {
                        Assert.Equal("Property", e.Name);
                        Assert.Collection(e.GetProperties(),
                            p => Assert.Equal("IdA", p.Name),
                            p => Assert.Equal("IdB", p.Name),
                            p => Assert.Equal("IdC", p.Name));
                        Assert.Collection(e.GetKeys(),
                            k =>
                            {
                                Assert.Collection(
                                    k.Properties,
                                    p => Assert.Equal("IdA", p.Name),
                                    p => Assert.Equal("IdB", p.Name),
                                    p => Assert.Equal("IdC", p.Name));
                                Assert.True(k.IsPrimaryKey());
                            });
                        Assert.Collection(
                            e.GetForeignKeys(),
                            k =>
                            {
                                Assert.Equal("TableAb", k.PrincipalEntityType.Name);
                                Assert.Collection(k.Properties,
                                    p => Assert.Equal("IdA", p.Name),
                                    p => Assert.Equal("IdB", p.Name));
                                Assert.Collection(k.PrincipalKey.Properties,
                                    p => Assert.Equal("IdA", p.Name),
                                    p => Assert.Equal("IdB", p.Name));
                                Assert.False(k.IsUnique);
                            },
                            k =>
                            {
                                Assert.Equal("AttributesByCategory", k.PrincipalEntityType.Name);
                                Assert.Collection(k.Properties,
                                    p => Assert.Equal("IdB", p.Name),
                                    p => Assert.Equal("IdC", p.Name));
                                Assert.Collection(k.PrincipalKey.Properties,
                                    p => Assert.Equal("IdB", p.Name),
                                    p => Assert.Equal("IdC", p.Name));
                                Assert.False(k.IsUnique);
                            });
                        Assert.Collection(e.GetIndexes(),
                            i =>
                            {
                                Assert.Collection(
                                    i.Properties,
                                    p => Assert.Equal("IdA", p.Name),
                                    p => Assert.Equal("IdC", p.Name));
                                Assert.True(i.IsUnique);
                            });
                        Assert.Collection(e.GetNavigations(),
                            n =>
                            {
                                Assert.Equal("AttributesByCategory", n.TargetEntityType.Name);
                                Assert.Equal("AttributesByCategory", n.Name);
                                Assert.False(n.IsCollection);
                            },
                            n =>
                            {
                                Assert.Equal("TableAb", n.TargetEntityType.Name);
                                Assert.Equal("TableAb", n.Name);
                                Assert.False(n.IsCollection);
                            });
                        Assert.Empty(e.GetSkipNavigations());
                    },
                    e =>
                    {
                        Assert.Equal("TableAb", e.Name);
                        Assert.Collection(e.GetProperties(),
                            p => Assert.Equal("IdA", p.Name),
                            p => Assert.Equal("IdB", p.Name));
                        Assert.Collection(e.GetKeys(),
                            k =>
                            {
                                Assert.Collection(k.Properties, p => Assert.Equal("IdA", p.Name));
                                Assert.True(k.IsPrimaryKey());
                            },
                            k =>
                            {
                                Assert.Collection(k.Properties, p => Assert.Equal("IdA", p.Name), p => Assert.Equal("IdB", p.Name));
                                Assert.False(k.IsPrimaryKey());
                            });
                        Assert.Collection(e.GetForeignKeys(), k =>
                        {
                            Assert.Equal("TableB", k.PrincipalEntityType.Name);
                            Assert.Collection(k.Properties, p => Assert.Equal("IdB", p.Name));
                            Assert.Collection(k.PrincipalKey.Properties, p => Assert.Equal("IdB", p.Name));
                            Assert.False(k.IsUnique);
                        });
                        Assert.Collection(e.GetIndexes(),
                            i =>
                            {
                                Assert.Collection(i.Properties, p => Assert.Equal("IdA", p.Name), p => Assert.Equal("IdB", p.Name));
                                Assert.True(i.IsUnique);
                            });
                        Assert.Collection(e.GetNavigations(),
                            n =>
                            {
                                Assert.Equal("TableB", n.TargetEntityType.Name);
                                Assert.Equal("IdBNavigation", n.Name);
                                Assert.False(n.IsCollection);
                            },
                            n =>
                            {
                                Assert.Equal("Property", n.TargetEntityType.Name);
                                Assert.Equal("Properties", n.Name);
                                Assert.True(n.IsCollection);
                            });
                        Assert.Empty(e.GetSkipNavigations());
                    },
                    e =>
                    {
                        Assert.Equal("TableB", e.Name);
                        Assert.Collection(e.GetProperties(), p => Assert.Equal("IdB", p.Name));
                        Assert.Collection(e.GetKeys(), k => Assert.Collection(k.Properties, p => Assert.Equal("IdB", p.Name)));
                        Assert.Empty(e.GetForeignKeys());
                        Assert.Empty(e.GetIndexes());
                        Assert.Collection(e.GetNavigations(),
                            n =>
                            {
                                Assert.Equal("AttributesByCategory", n.TargetEntityType.Name);
                                Assert.Equal("AttributesByCategories", n.Name);
                                Assert.True(n.IsCollection);
                            },
                            n =>
                            {
                                Assert.Equal("TableAb", n.TargetEntityType.Name);
                                Assert.Equal("TableAbs", n.Name);
                                Assert.True(n.IsCollection);
                            });
                        Assert.Empty(e.GetSkipNavigations());
                    },
                    e =>
                    {
                        Assert.Equal("TableC", e.Name);
                        Assert.Collection(e.GetProperties(), p => Assert.Equal("IdC", p.Name));
                        Assert.Collection(e.GetKeys(), k => Assert.Collection(k.Properties, p => Assert.Equal("IdC", p.Name)));
                        Assert.Empty(e.GetForeignKeys());
                        Assert.Empty(e.GetIndexes());
                        Assert.Collection(e.GetNavigations(),
                            n =>
                            {
                                Assert.Equal("AttributesByCategory", n.TargetEntityType.Name);
                                Assert.Equal("AttributesByCategories", n.Name);
                                Assert.True(n.IsCollection);
                            });
                        Assert.Empty(e.GetSkipNavigations());
                    });
            },
            @"
DROP TABLE [dbo].[Properties];
DROP TABLE [dbo].[AttributesByCategory];
DROP TABLE [dbo].[TableAB];
DROP TABLE [dbo].[TableB];
DROP TABLE [dbo].[TableC];");

    [ConditionalFact]
    public void Expose_join_table_when_interloper_reference()
        => Test(
            @"
CREATE TABLE BBlogs (Id int IDENTITY CONSTRAINT [PK_BBlogs] PRIMARY KEY,);
CREATE TABLE PPosts (Id int IDENTITY CONSTRAINT [PK_PPosts] PRIMARY KEY,);

CREATE TABLE BBlogPPosts (
    BBlogId int NOT NULL CONSTRAINT [FK_BBlogPPosts_BBlogs] REFERENCES BBlogs ON DELETE CASCADE,
    PPostId int NOT NULL CONSTRAINT [FK_BBlogPPosts_PPosts] REFERENCES PPosts ON DELETE CASCADE,
    CONSTRAINT [PK_BBlogPPosts ] PRIMARY KEY (BBlogId, PPostId));

CREATE TABLE LinkToBBlogPPosts (
    LinkId1 int NOT NULL,
    LinkId2 int NOT NULL,
    CONSTRAINT [PK_LinkToBBlogPPosts] PRIMARY KEY (LinkId1, LinkId2),
    CONSTRAINT [FK_LinkToBBlogPPosts_BlogPosts] FOREIGN KEY (LinkId1, LinkId2) REFERENCES BBlogPPosts);
",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                Assert.Collection(
                    dbModel.Tables.OrderBy(t => t.Name),
                    t =>
                    {
                        Assert.Equal("dbo", t.Schema);
                        Assert.Equal("BBlogPPosts", t.Name);
                        Assert.Collection(t.Columns,
                            c => Assert.Equal("BBlogId", c.Name),
                            c => Assert.Equal("PPostId", c.Name));
                        Assert.Collection(t.ForeignKeys,
                            c =>
                            {
                                Assert.Equal("BBlogs", c.PrincipalTable.Name);
                                Assert.Equal("BBlogPPosts", c.Table.Name);
                                Assert.Collection(c.Columns, c => Assert.Equal("BBlogId", c.Name));
                            },
                            c =>
                            {
                                Assert.Equal("PPosts", c.PrincipalTable.Name);
                                Assert.Equal("BBlogPPosts", c.Table.Name);
                                Assert.Collection(c.Columns, c => Assert.Equal("PPostId", c.Name));
                            });
                    },
                    t =>
                    {
                        Assert.Equal("dbo", t.Schema);
                        Assert.Equal("BBlogs", t.Name);
                        Assert.Collection(t.Columns, c => Assert.Equal("Id", c.Name));
                    },
                    t =>
                    {
                        Assert.Equal("dbo", t.Schema);
                        Assert.Equal("LinkToBBlogPPosts", t.Name);
                        Assert.Collection(t.Columns,
                            c => Assert.Equal("LinkId1", c.Name),
                            c => Assert.Equal("LinkId2", c.Name));
                        Assert.Collection(t.ForeignKeys,
                            c =>
                            {
                                Assert.Equal("BBlogPPosts", c.PrincipalTable.Name);
                                Assert.Equal("LinkToBBlogPPosts", c.Table.Name);
                                Assert.Collection(
                                    c.Columns,
                                    c => Assert.Equal("LinkId1", c.Name),
                                    c => Assert.Equal("LinkId2", c.Name));
                            });
                    },
                    t =>
                    {
                        Assert.Equal("dbo", t.Schema);
                        Assert.Equal("PPosts", t.Name);
                        Assert.Collection(t.Columns, c => Assert.Equal("Id", c.Name));
                    });

                var model = scaffoldingFactory.Create(dbModel, new ModelReverseEngineerOptions());

                Assert.Collection(
                    model.GetEntityTypes(),
                    e =>
                    {
                        Assert.Equal("Bblog", e.Name);
                        Assert.Collection(e.GetProperties(), p => Assert.Equal("Id", p.Name));
                        Assert.Empty(e.GetForeignKeys());
                        Assert.Empty(e.GetSkipNavigations());
                        Assert.Collection(e.GetNavigations(), p => Assert.Equal("BblogPposts", p.Name));
                    },
                    e =>
                    {
                        Assert.Equal("BblogPpost", e.Name);
                        Assert.Collection(e.GetProperties(),
                            p => Assert.Equal("BblogId", p.Name),
                            p => Assert.Equal("PpostId", p.Name));
                        Assert.Collection(e.GetForeignKeys(),
                            k =>
                            {
                                Assert.Equal("Bblog", k.PrincipalEntityType.Name);
                                Assert.Equal("BblogPpost", k.DeclaringEntityType.Name);
                                Assert.Collection(k.Properties, p => Assert.Equal("BblogId", p.Name));
                                Assert.False(k.IsUnique);
                            },
                            k =>
                            {
                                Assert.Equal("Ppost", k.PrincipalEntityType.Name);
                                Assert.Equal("BblogPpost", k.DeclaringEntityType.Name);
                                Assert.Collection(k.Properties, p => Assert.Equal("PpostId", p.Name));
                                Assert.False(k.IsUnique);
                            });
                        Assert.Empty(e.GetSkipNavigations());
                        Assert.Collection(e.GetNavigations(),
                            p => Assert.Equal("Bblog", p.Name),
                            p => Assert.Equal("LinkToBblogPpost", p.Name),
                            p => Assert.Equal("Ppost", p.Name));
                    },
                    e =>
                    {
                        Assert.Equal("LinkToBblogPpost", e.Name);
                        Assert.Collection(e.GetProperties(),
                            p => Assert.Equal("LinkId1", p.Name),
                            p => Assert.Equal("LinkId2", p.Name));
                        Assert.Collection(e.GetForeignKeys(),
                            k =>
                            {
                                Assert.Equal("BblogPpost", k.PrincipalEntityType.Name);
                                Assert.Equal("LinkToBblogPpost", k.DeclaringEntityType.Name);
                                Assert.Collection(k.Properties,
                                    p => Assert.Equal("LinkId1", p.Name),
                                    p => Assert.Equal("LinkId2", p.Name));
                                Assert.Collection(k.PrincipalKey.Properties,
                                    p => Assert.Equal("BblogId", p.Name),
                                    p => Assert.Equal("PpostId", p.Name));
                                Assert.True(k.IsUnique);
                            });
                        Assert.Empty(e.GetSkipNavigations());
                        Assert.Collection(e.GetNavigations(), p => Assert.Equal("BblogPpost", p.Name));
                    },
                    e =>
                    {
                        Assert.Equal("Ppost", e.Name);
                        Assert.Collection(e.GetProperties(), p => Assert.Equal("Id", p.Name));
                        Assert.Empty(e.GetForeignKeys());
                        Assert.Empty(e.GetSkipNavigations());
                        Assert.Collection(e.GetNavigations(), p => Assert.Equal("BblogPposts", p.Name));
                    });
            },
            @"
DROP TABLE [dbo].[LinkToBBlogPPosts];
DROP TABLE [dbo].[BBlogPPosts];
DROP TABLE [dbo].[PPosts];
DROP TABLE [dbo].[BBlogs];");

    [ConditionalFact]
    public void Default_database_collation_is_not_scaffolded()
        => Test(
            @"",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, _) => Assert.Null(dbModel.Collation),
            @"");

    #endregion

    #region FilteringSchemaTable

    [ConditionalFact]
    public void Filter_schemas()
        => Test(
            @"
CREATE TABLE [db2].[K2] ( Id int, A varchar, UNIQUE (A ) );

CREATE TABLE [dbo].[Kilimanjaro] ( Id int, B varchar, UNIQUE (B));",
            Enumerable.Empty<string>(),
            new[] { "db2" },
            (dbModel, scaffoldingFactory) =>
            {
                var table = Assert.Single(dbModel.Tables);
                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("K2", table.Name);
                Assert.Equal(2, table.Columns.Count);
                Assert.Equal(1, table.UniqueConstraints.Count);
                Assert.Empty(table.ForeignKeys);

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Equal(1, model.GetEntityTypes().Count());
            },
            @"
DROP TABLE [dbo].[Kilimanjaro];

DROP TABLE [db2].[K2];");

    [ConditionalFact]
    public void Filter_tables()
        => Test(
            @"
CREATE TABLE [dbo].[K2] ( Id int, A varchar, UNIQUE (A ) );

CREATE TABLE [dbo].[Kilimanjaro] ( Id int, B varchar, UNIQUE (B), FOREIGN KEY (B) REFERENCES K2 (A) );",
            new[] { "K2" },
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var table = Assert.Single(dbModel.Tables);
                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("K2", table.Name);
                Assert.Equal(2, table.Columns.Count);
                Assert.Equal(1, table.UniqueConstraints.Count);
                Assert.Empty(table.ForeignKeys);

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Equal(1, model.GetEntityTypes().Count());
            },
            @"
DROP TABLE [dbo].[Kilimanjaro];

DROP TABLE [dbo].[K2];");

    [ConditionalFact]
    public void Filter_tables_with_quote_in_name()
        => Test(
            @"
CREATE TABLE [dbo].[K2'] ( Id int, A varchar, UNIQUE (A ) );

CREATE TABLE [dbo].[Kilimanjaro] ( Id int, B varchar, UNIQUE (B), FOREIGN KEY (B) REFERENCES [K2'] (A) );",
            new[] { "K2'" },
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var table = Assert.Single(dbModel.Tables);
                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("K2'", table.Name);
                Assert.Equal(2, table.Columns.Count);
                Assert.Equal(1, table.UniqueConstraints.Count);
                Assert.Empty(table.ForeignKeys);

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Equal(1, model.GetEntityTypes().Count());
            },
            @"
DROP TABLE [dbo].[Kilimanjaro];

DROP TABLE [dbo].[K2'];");

    [ConditionalFact]
    public void Filter_tables_with_qualified_name()
        => Test(
            @"
CREATE TABLE [dbo].[K.2] ( Id int, A varchar, UNIQUE (A ) );

CREATE TABLE [dbo].[Kilimanjaro] ( Id int, B varchar, UNIQUE (B) );",
            new[] { "[K.2]" },
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var table = Assert.Single(dbModel.Tables);
                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("K.2", table.Name);
                Assert.Equal(2, table.Columns.Count);
                Assert.Equal(1, table.UniqueConstraints.Count);
                Assert.Empty(table.ForeignKeys);

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Equal(1, model.GetEntityTypes().Count());
            },
            @"
DROP TABLE [dbo].[Kilimanjaro];

DROP TABLE [dbo].[K.2];");

    [ConditionalFact]
    public void Filter_tables_with_schema_qualified_name1()
        => Test(
            @"
CREATE TABLE [dbo].[K2] ( Id int, A varchar, UNIQUE (A ) );

CREATE TABLE [db2].[K2] ( Id int, A varchar, UNIQUE (A ) );

CREATE TABLE [dbo].[Kilimanjaro] ( Id int, B varchar, UNIQUE (B) );",
            new[] { "dbo.K2" },
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var table = Assert.Single(dbModel.Tables);
                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("K2", table.Name);
                Assert.Equal(2, table.Columns.Count);
                Assert.Equal(1, table.UniqueConstraints.Count);
                Assert.Empty(table.ForeignKeys);

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Equal(1, model.GetEntityTypes().Count());
            },
            @"
DROP TABLE [dbo].[Kilimanjaro];

DROP TABLE [dbo].[K2];

DROP TABLE [db2].[K2];");

    [ConditionalFact]
    public void Filter_tables_with_schema_qualified_name2()
        => Test(
            @"
CREATE TABLE [dbo].[K.2] ( Id int, A varchar, UNIQUE (A ) );

CREATE TABLE [db.2].[K.2] ( Id int, A varchar, UNIQUE (A ) );

CREATE TABLE [db.2].[Kilimanjaro] ( Id int, B varchar, UNIQUE (B) );",
            new[] { "[db.2].[K.2]" },
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var table = Assert.Single(dbModel.Tables);
                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("K.2", table.Name);
                Assert.Equal(2, table.Columns.Count);
                Assert.Equal(1, table.UniqueConstraints.Count);
                Assert.Empty(table.ForeignKeys);

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Equal(1, model.GetEntityTypes().Count());
            },
            @"
DROP TABLE [db.2].[Kilimanjaro];

DROP TABLE [dbo].[K.2];

DROP TABLE [db.2].[K.2];");

    [ConditionalFact]
    public void Filter_tables_with_schema_qualified_name3()
        => Test(
            @"
CREATE TABLE [dbo].[K.2] ( Id int, A varchar, UNIQUE (A ) );

CREATE TABLE [db2].[K.2] ( Id int, A varchar, UNIQUE (A ) );

CREATE TABLE [dbo].[Kilimanjaro] ( Id int, B varchar, UNIQUE (B) );",
            new[] { "dbo.[K.2]" },
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var table = Assert.Single(dbModel.Tables);
                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("K.2", table.Name);
                Assert.Equal(2, table.Columns.Count);
                Assert.Equal(1, table.UniqueConstraints.Count);
                Assert.Empty(table.ForeignKeys);

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Equal(1, model.GetEntityTypes().Count());
            },
            @"
DROP TABLE [dbo].[Kilimanjaro];

DROP TABLE [dbo].[K.2];

DROP TABLE [db2].[K.2];");

    [ConditionalFact]
    public void Filter_tables_with_schema_qualified_name4()
        => Test(
            @"
CREATE TABLE [dbo].[K2] ( Id int, A varchar, UNIQUE (A ) );

CREATE TABLE [db.2].[K2] ( Id int, A varchar, UNIQUE (A ) );

CREATE TABLE [db.2].[Kilimanjaro] ( Id int, B varchar, UNIQUE (B) );",
            new[] { "[db.2].K2" },
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var table = Assert.Single(dbModel.Tables);
                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("K2", table.Name);
                Assert.Equal(2, table.Columns.Count);
                Assert.Equal(1, table.UniqueConstraints.Count);
                Assert.Empty(table.ForeignKeys);

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Equal(1, model.GetEntityTypes().Count());
            },
            @"
DROP TABLE [db.2].[Kilimanjaro];

DROP TABLE [dbo].[K2];

DROP TABLE [db.2].[K2];");

    [ConditionalFact]
    public void Complex_filtering_validation()
    {
        Test(
            @"
CREATE SEQUENCE [dbo].[Sequence];
CREATE SEQUENCE [db2].[Sequence];

CREATE TABLE [db.2].[QuotedTableName] ( Id int PRIMARY KEY );
CREATE TABLE [db.2].[Table.With.Dot] ( Id int PRIMARY KEY );
CREATE TABLE [db.2].[SimpleTableName] ( Id int PRIMARY KEY );
CREATE TABLE [db.2].[JustTableName] ( Id int PRIMARY KEY );

CREATE TABLE [dbo].[QuotedTableName] ( Id int PRIMARY KEY );
CREATE TABLE [dbo].[Table.With.Dot] ( Id int PRIMARY KEY );
CREATE TABLE [dbo].[SimpleTableName] ( Id int PRIMARY KEY );
CREATE TABLE [dbo].[JustTableName] ( Id int PRIMARY KEY );

CREATE TABLE [db2].[QuotedTableName] ( Id int PRIMARY KEY );
CREATE TABLE [db2].[Table.With.Dot] ( Id int PRIMARY KEY );
CREATE TABLE [db2].[SimpleTableName] ( Id int PRIMARY KEY );
CREATE TABLE [db2].[JustTableName] ( Id int PRIMARY KEY );

CREATE TABLE [db2].[PrincipalTable] (
    Id int PRIMARY KEY,
    UC1 nvarchar(450),
    UC2 int,
    Index1 bit,
    Index2 bigint
    CONSTRAINT UX UNIQUE (UC1, UC2),
)

CREATE INDEX IX_COMPOSITE ON [db2].[PrincipalTable] ( Index2, Index1 );

CREATE TABLE [db2].[DependentTable] (
    Id int PRIMARY KEY,
    ForeignKeyId1 nvarchar(450),
    ForeignKeyId2 int,
    FOREIGN KEY (ForeignKeyId1, ForeignKeyId2) REFERENCES [db2].[PrincipalTable](UC1, UC2) ON DELETE CASCADE,
);",
            new[] { "[db.2].[QuotedTableName]", "[db.2].SimpleTableName", "dbo.[Table.With.Dot]", "dbo.SimpleTableName", "JustTableName" },
            new[] { "db2" },
            (dbModel, scaffoldingFactory) =>
            {
                var sequence = Assert.Single(dbModel.Sequences);
                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("db2", sequence.Schema);

                Assert.Single(dbModel.Tables.Where(t => t is { Schema: "db.2", Name: "QuotedTableName" }));
                Assert.Empty(dbModel.Tables.Where(t => t is { Schema: "db.2", Name: "Table.With.Dot" }));
                Assert.Single(dbModel.Tables.Where(t => t is { Schema: "db.2", Name: "SimpleTableName" }));
                Assert.Single(dbModel.Tables.Where(t => t is { Schema: "db.2", Name: "JustTableName" }));

                Assert.Empty(dbModel.Tables.Where(t => t is { Schema: "dbo", Name: "QuotedTableName" }));
                Assert.Single(dbModel.Tables.Where(t => t is { Schema: "dbo", Name: "Table.With.Dot" }));
                Assert.Single(dbModel.Tables.Where(t => t is { Schema: "dbo", Name: "SimpleTableName" }));
                Assert.Single(dbModel.Tables.Where(t => t is { Schema: "dbo", Name: "JustTableName" }));

                Assert.Single(dbModel.Tables.Where(t => t is { Schema: "db2", Name: "QuotedTableName" }));
                Assert.Single(dbModel.Tables.Where(t => t is { Schema: "db2", Name: "Table.With.Dot" }));
                Assert.Single(dbModel.Tables.Where(t => t is { Schema: "db2", Name: "SimpleTableName" }));
                Assert.Single(dbModel.Tables.Where(t => t is { Schema: "db2", Name: "JustTableName" }));

                var principalTable = Assert.Single(dbModel.Tables.Where(t => t is { Schema: "db2", Name: "PrincipalTable" }));
                // ReSharper disable once PossibleNullReferenceException
                Assert.NotNull(principalTable.PrimaryKey);
                Assert.Single(principalTable.UniqueConstraints);
                Assert.Single(principalTable.Indexes);

                var dependentTable = Assert.Single(dbModel.Tables.Where(t => t is { Schema: "db2", Name: "DependentTable" }));
                // ReSharper disable once PossibleNullReferenceException
                Assert.Single(dependentTable.ForeignKeys);

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Collection(
                    model.GetSequences(),
                    s =>
                    {
                        Assert.Equal("db2", s.Schema);
                        Assert.Equal("Sequence", s.Name);
                    });
                Assert.Collection(
                    model.GetEntityTypes(),
                    e =>
                    {
                        Assert.Equal("db2", e.GetSchema());
                        Assert.Equal("DependentTable", e.Name);
                        Assert.Equal("Id", e.FindPrimaryKey()!.Properties.Single().Name);
                        Assert.Collection(
                            e.GetProperties(),
                            p =>
                            {
                                Assert.Equal("Id", p.Name);
                                Assert.Equal(ValueGenerated.Never, p.ValueGenerated);
                            },
                            p => Assert.Equal("ForeignKeyId1", p.Name),
                            p => Assert.Equal("ForeignKeyId2", p.Name));
                        Assert.Collection(
                            e.GetNavigations(),
                            n =>
                            {
                                Assert.Equal("PrincipalTable", n.Name);
                                Assert.False(n.IsCollection);
                            });
                        Assert.Collection(e.GetForeignKeys(),
                            k =>
                            {
                                Assert.Equal("PrincipalTable", k.PrincipalEntityType.Name);
                                Assert.Equal("DependentTable", k.DeclaringEntityType.Name);
                                Assert.Collection(k.Properties,
                                    p => Assert.Equal("ForeignKeyId1", p.Name),
                                    p => Assert.Equal("ForeignKeyId2", p.Name));
                                Assert.Collection(k.PrincipalKey.Properties,
                                    p => Assert.Equal("Uc1", p.Name),
                                    p => Assert.Equal("Uc2", p.Name));
                                Assert.False(k.IsUnique);
                            });
                        Assert.Empty(e.GetIndexes());
                        Assert.Empty(e.GetSkipNavigations());
                    },
                    e =>
                    {
                        Assert.Equal("db.2", e.GetSchema());
                        Assert.Equal("JustTableName", e.Name);
                        AssertIdOnly(e);
                    },
                    e =>
                    {
                        Assert.Equal("db2", e.GetSchema());
                        Assert.Equal("JustTableName1", e.Name);
                        AssertIdOnly(e);
                    },
                    e =>
                    {
                        Assert.Equal("dbo", e.GetSchema());
                        Assert.Equal("JustTableName2", e.Name);
                        AssertIdOnly(e);
                    },
                    e =>
                    {
                        Assert.Equal("db2", e.GetSchema());
                        Assert.Equal("PrincipalTable", e.Name);
                        Assert.Equal("Id", e.FindPrimaryKey()!.Properties.Single().Name);
                        Assert.Collection(
                            e.GetProperties(),
                            p =>
                            {
                                Assert.Equal("Id", p.Name);
                                Assert.Equal(ValueGenerated.Never, p.ValueGenerated);
                            },
                            p => Assert.Equal("Index1", p.Name),
                            p => Assert.Equal("Index2", p.Name),
                            p => Assert.Equal("Uc1", p.Name),
                            p => Assert.Equal("Uc2", p.Name));
                        Assert.Collection(
                            e.GetIndexes(),
                            i =>
                            {
                                Assert.Collection(i.Properties,
                                    p => Assert.Equal("Index2", p.Name),
                                    p => Assert.Equal("Index1", p.Name));
                                Assert.False(i.IsUnique);
                            },
                            i =>
                            {
                                Assert.Collection(i.Properties,
                                    p => Assert.Equal("Uc1", p.Name),
                                    p => Assert.Equal("Uc2", p.Name));
                                Assert.True(i.IsUnique);
                            });
                        Assert.Collection(
                            e.GetNavigations(),
                            n =>
                            {
                                Assert.Equal("DependentTables", n.Name);
                                Assert.True(n.IsCollection);
                            });
                        Assert.Empty(e.GetForeignKeys());
                        Assert.Empty(e.GetSkipNavigations());
                    },
                    e =>
                    {
                        Assert.Equal("db.2", e.GetSchema());
                        Assert.Equal("QuotedTableName", e.Name);
                        AssertIdOnly(e);
                    },
                    e =>
                    {
                        Assert.Equal("db2", e.GetSchema());
                        Assert.Equal("QuotedTableName1", e.Name);
                        AssertIdOnly(e);
                    },
                    e =>
                    {
                        Assert.Equal("db.2", e.GetSchema());
                        Assert.Equal("SimpleTableName", e.Name);
                        AssertIdOnly(e);
                    },
                    e =>
                    {
                        Assert.Equal("db2", e.GetSchema());
                        Assert.Equal("SimpleTableName1", e.Name);
                        AssertIdOnly(e);
                    },
                    e =>
                    {
                        Assert.Equal("dbo", e.GetSchema());
                        Assert.Equal("SimpleTableName2", e.Name);
                        AssertIdOnly(e);
                    },
                    e =>
                    {
                        Assert.Equal("db2", e.GetSchema());
                        Assert.Equal("TableWithDot", e.Name);
                        AssertIdOnly(e);
                    },
                    e =>
                    {
                        Assert.Equal("dbo", e.GetSchema());
                        Assert.Equal("TableWithDot1", e.Name);
                        AssertIdOnly(e);
                    });
            },
            @"
DROP SEQUENCE [dbo].[Sequence];
DROP SEQUENCE [db2].[Sequence];

DROP TABLE [db.2].[QuotedTableName];
DROP TABLE [db.2].[Table.With.Dot];
DROP TABLE [db.2].[SimpleTableName];
DROP TABLE [db.2].[JustTableName];

DROP TABLE [dbo].[QuotedTableName];
DROP TABLE [dbo].[Table.With.Dot];
DROP TABLE [dbo].[SimpleTableName];
DROP TABLE [dbo].[JustTableName];

DROP TABLE [db2].[QuotedTableName];
DROP TABLE [db2].[Table.With.Dot];
DROP TABLE [db2].[SimpleTableName];
DROP TABLE [db2].[JustTableName];
DROP TABLE [db2].[DependentTable];
DROP TABLE [db2].[PrincipalTable];");

        void AssertIdOnly(IEntityType entityType)
        {
            Assert.Equal("Id", entityType.FindPrimaryKey()!.Properties.Single().Name);
            Assert.Collection(entityType.GetProperties(), p => Assert.Equal(ValueGenerated.Never, p.ValueGenerated));
            Assert.Empty(entityType.GetIndexes());
            Assert.Empty(entityType.GetForeignKeys());
            Assert.Empty(entityType.GetNavigations());
            Assert.Empty(entityType.GetSkipNavigations());
        }
    }

    #endregion

    #region Table

    [ConditionalFact]
    [SqlServerCondition(SqlServerCondition.SupportsMemoryOptimized)]
    public void Set_memory_optimized_table_annotation()
        => Test(
            @"
IF SERVERPROPERTY('IsXTPSupported') = 1 AND SERVERPROPERTY('EngineEdition') <> 5
BEGIN
IF NOT EXISTS (
    SELECT 1 FROM [sys].[filegroups] [FG] JOIN [sys].[database_files] [F] ON [FG].[data_space_id] = [F].[data_space_id] WHERE [FG].[type] = N'FX' AND [F].[type] = 2)
    BEGIN
    DECLARE @db_name nvarchar(max) = DB_NAME();
    DECLARE @fg_name nvarchar(max);
    SELECT TOP(1) @fg_name = [name] FROM [sys].[filegroups] WHERE [type] = N'FX';

    IF @fg_name IS NULL
        BEGIN
        SET @fg_name = @db_name + N'_MODFG';
        EXEC(N'ALTER DATABASE CURRENT ADD FILEGROUP [' + @fg_name + '] CONTAINS MEMORY_OPTIMIZED_DATA;');
        END

    DECLARE @path nvarchar(max);
    SELECT TOP(1) @path = [physical_name] FROM [sys].[database_files] WHERE charindex('\', [physical_name]) > 0 ORDER BY [file_id];
    IF (@path IS NULL)
        SET @path = '\' + @db_name;

    DECLARE @filename nvarchar(max) = right(@path, charindex('\', reverse(@path)) - 1);
    SET @filename = REPLACE(left(@filename, len(@filename) - charindex('.', reverse(@filename))), '''', '''''') + N'_MOD';
    DECLARE @new_path nvarchar(max) = REPLACE(CAST(SERVERPROPERTY('InstanceDefaultDataPath') AS nvarchar(max)), '''', '''''') + @filename;

    EXEC(N'
        ALTER DATABASE CURRENT
        ADD FILE (NAME=''' + @filename + ''', filename=''' + @new_path + ''')
        TO FILEGROUP [' + @fg_name + '];')
    END
END

IF SERVERPROPERTY('IsXTPSupported') = 1
EXEC(N'ALTER DATABASE CURRENT SET MEMORY_OPTIMIZED_ELEVATE_TO_SNAPSHOT ON;');

CREATE TABLE [Blogs] (
    [Id] int NOT NULL IDENTITY,
    CONSTRAINT [PK_Blogs] PRIMARY KEY NONCLUSTERED ([Id])
) WITH (MEMORY_OPTIMIZED = ON);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var table = Assert.Single(dbModel.Tables.Where(t => t.Name == "Blogs"));

                // ReSharper disable once PossibleNullReferenceException
                Assert.True((bool)table[SqlServerAnnotationNames.MemoryOptimized]!);

                var model = scaffoldingFactory.Create(dbModel, new());
                Assert.Equal(1, model.GetEntityTypes().Count());
            },
            "DROP TABLE [Blogs]");

    [ConditionalFact]
    public void Class_members_can_have_same_name_as_classes_when_casing_differs() // Issue #30237
        => Test(
            @"
CREATE TABLE [dbo].[UIText]
(
	[UiKey] VARCHAR(100) NOT NULL PRIMARY KEY,
	[UiText] NVARCHAR(1000) NOT NULL
)",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                Assert.Collection(dbModel.Tables,
                    t =>
                    {
                        Assert.Equal("UIText", t.Name);
                        Assert.Collection(
                            t.Columns,
                            c => Assert.Equal("UiKey", c.Name),
                            c => Assert.Equal("UiText", c.Name));
                    });

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Collection(model.GetEntityTypes(),
                    e =>
                    {
                        Assert.Equal("Uitext", e.Name);
                        Assert.Collection(
                            e.GetProperties(),
                            p => Assert.Equal("UiKey", p.Name),
                            p => Assert.Equal("UiText", p.Name));
                    });
            },
            "DROP TABLE [UIText]");

    [ConditionalFact]
    public void Create_columns()
        => Test(
            @"
CREATE TABLE [dbo].[Blogs] (
    Id int,
    Name nvarchar(100) NOT NULL,
);
EXECUTE sys.sp_addextendedproperty @name = N'MS_Description', @value = N'Blog table comment.
On multiple lines.',
    @level0type = N'SCHEMA', @level0name = 'dbo',
    @level1type = N'TABLE', @level1name = 'Blogs';
EXECUTE sys.sp_addextendedproperty @name = N'MS_Description', @value = N'Blog.Id column comment.',
    @level0type = N'SCHEMA', @level0name = 'dbo',
    @level1type = N'TABLE', @level1name = 'Blogs',
    @level2type = N'COLUMN', @level2name = 'Id';
",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var table = dbModel.Tables.Single();

                Assert.Equal(2, table.Columns.Count);
                Assert.All(
                    table.Columns, c =>
                    {
                        Assert.Equal("dbo", c.Table.Schema);
                        Assert.Equal("Blogs", c.Table.Name);
                        Assert.Equal(
                            @"Blog table comment.
On multiple lines.", c.Table.Comment);
                    });

                Assert.Single(table.Columns.Where(c => c.Name == "Id"));
                Assert.Single(table.Columns.Where(c => c.Name == "Name"));
                Assert.Single(table.Columns.Where(c => c.Comment == "Blog.Id column comment."));
                Assert.Single(table.Columns.Where(c => c.Comment != null));

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Collection(
                    model.GetEntityTypes(),
                    e =>
                    {
                        Assert.Equal("dbo", e.GetSchema());
                        Assert.Equal("Blog", e.Name);
                        Assert.Equal("Blogs", e.GetTableName());
                        Assert.Collection(e.GetProperties(),
                            p => Assert.Equal("Id", p.Name),
                            p => Assert.Equal("Name", p.Name));
                        Assert.Empty(e.GetIndexes());
                        Assert.Empty(e.GetForeignKeys());
                        Assert.Empty(e.GetNavigations());
                        Assert.Empty(e.GetSkipNavigations());
                    });
            },
            "DROP TABLE [dbo].[Blogs]");

    [ConditionalFact]
    public void Create_view_columns()
        => Test(
            @"
CREATE VIEW [dbo].[BlogsView]
 AS
SELECT
 CAST(100 AS int) AS Id,
 CAST(N'' AS nvarchar(100)) AS Name;",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var table = Assert.IsType<DatabaseView>(dbModel.Tables.Single());

                Assert.Equal(2, table.Columns.Count);
                Assert.Null(table.PrimaryKey);
                Assert.All(
                    table.Columns, c =>
                    {
                        Assert.Equal("dbo", c.Table.Schema);
                        Assert.Equal("BlogsView", c.Table.Name);
                    });

                Assert.Single(table.Columns.Where(c => c.Name == "Id"));
                Assert.Single(table.Columns.Where(c => c.Name == "Name"));

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Collection(
                    model.GetEntityTypes(),
                    e =>
                    {
                        Assert.Equal("dbo", e.GetViewSchema());
                        Assert.Equal("BlogsView", e.Name);
                        Assert.Equal("BlogsView", e.GetViewName());
                        Assert.Collection(e.GetProperties(),
                            p => Assert.Equal("Id", p.Name),
                            p => Assert.Equal("Name", p.Name));
                        Assert.Empty(e.GetIndexes());
                        Assert.Empty(e.GetForeignKeys());
                        Assert.Empty(e.GetNavigations());
                        Assert.Empty(e.GetSkipNavigations());
                    });
            },
            "DROP VIEW [dbo].[BlogsView];");

    [ConditionalFact]
    public void Create_primary_key()
        => Test(
            @"
CREATE TABLE PrimaryKeyTable (
    Id int PRIMARY KEY
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var pk = dbModel.Tables.Single().PrimaryKey;

                Assert.Equal("dbo", pk!.Table!.Schema);
                Assert.Equal("PrimaryKeyTable", pk.Table.Name);
                Assert.StartsWith("PK__PrimaryK", pk.Name);
                Assert.Null(pk[SqlServerAnnotationNames.Clustered]);
                Assert.Equal(["Id"], pk.Columns.Select(ic => ic.Name).ToList());

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Collection(
                    model.GetEntityTypes(),
                    e =>
                    {
                        Assert.Equal("PrimaryKeyTable", e.Name);
                        Assert.Collection(e.GetProperties(), p =>
                        {
                            Assert.Equal("Id", p.Name);
                            Assert.Equal(ValueGenerated.Never, p.ValueGenerated);
                        });
                        Assert.Collection(e.GetKeys(), k => Assert.Equal("Id", k.Properties.Single().Name));
                        Assert.Empty(e.GetIndexes());
                        Assert.Empty(e.GetForeignKeys());
                        Assert.Empty(e.GetNavigations());
                        Assert.Empty(e.GetSkipNavigations());
                    });
            },
            "DROP TABLE PrimaryKeyTable;");

    [ConditionalFact]
    public void Create_unique_constraints()
        => Test(
            @"
CREATE TABLE UniqueConstraint (
    Id int,
    Name int Unique,
    IndexProperty int,
);

CREATE INDEX IX_INDEX on UniqueConstraint ( IndexProperty );",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var uniqueConstraint = Assert.Single(dbModel.Tables.Single().UniqueConstraints);

                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("dbo", uniqueConstraint.Table.Schema);
                Assert.Equal("UniqueConstraint", uniqueConstraint.Table.Name);
                Assert.StartsWith("UQ__UniqueCo", uniqueConstraint.Name);
                Assert.Null(uniqueConstraint[SqlServerAnnotationNames.Clustered]);
                Assert.Equal(["Name"], uniqueConstraint.Columns.Select(ic => ic.Name).ToList());

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Collection(
                    model.GetEntityTypes(),
                    e =>
                    {
                        Assert.Equal("UniqueConstraint", e.Name);
                        Assert.Collection(e.GetProperties(),
                            p => Assert.Equal("Id", p.Name),
                            p => Assert.Equal("IndexProperty", p.Name),
                            p => Assert.Equal("Name", p.Name));
                        Assert.Empty(e.GetKeys());
                        Assert.Collection(
                            e.GetIndexes(),
                            i =>
                            {
                                Assert.Collection(i.Properties, p => Assert.Equal("IndexProperty", p.Name));
                                Assert.False(i.IsUnique);
                            },
                            i =>
                            {
                                Assert.Collection(i.Properties, p => Assert.Equal("Name", p.Name));
                                Assert.True(i.IsUnique);
                            });
                        Assert.Empty(e.GetForeignKeys());
                        Assert.Empty(e.GetNavigations());
                        Assert.Empty(e.GetSkipNavigations());
                    });
            },
            "DROP TABLE UniqueConstraint;");

    [ConditionalFact]
    public void Create_indexes()
        => Test(
            @"
CREATE TABLE IndexTable (
    Id int,
    Name int,
    IndexProperty int,
);

CREATE INDEX IX_NAME on IndexTable ( Name );
CREATE INDEX IX_INDEX on IndexTable ( IndexProperty );",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var table = dbModel.Tables.Single();

                Assert.Equal(2, table.Indexes.Count);
                Assert.All(
                    table.Indexes, c =>
                    {
                        Assert.Equal("dbo", c.Table!.Schema);
                        Assert.Equal("IndexTable", c.Table.Name);
                    });

                Assert.Single(table.Indexes.Where(c => c.Name == "IX_NAME"));
                Assert.Single(table.Indexes.Where(c => c.Name == "IX_INDEX"));

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Collection(
                    model.GetEntityTypes(),
                    e =>
                    {
                        Assert.Equal("IndexTable", e.Name);
                        Assert.Collection(e.GetProperties(),
                            p => Assert.Equal("Id", p.Name),
                            p => Assert.Equal("IndexProperty", p.Name),
                            p => Assert.Equal("Name", p.Name));
                        Assert.Empty(e.GetKeys());
                        Assert.Collection(
                            e.GetIndexes(),
                            i =>
                            {
                                Assert.Collection(i.Properties, p => Assert.Equal("IndexProperty", p.Name));
                                Assert.False(i.IsUnique);
                            },
                            i =>
                            {
                                Assert.Collection(i.Properties, p => Assert.Equal("Name", p.Name));
                                Assert.False(i.IsUnique);
                            });
                        Assert.Empty(e.GetForeignKeys());
                        Assert.Empty(e.GetNavigations());
                        Assert.Empty(e.GetSkipNavigations());
                    });
            },
            "DROP TABLE IndexTable;");

    [ConditionalFact]
    public void Create_multiple_indexes_on_same_column()
        => Test(
            @"
CREATE TABLE IndexTable (
    Id int,
    IndexProperty int
);

CREATE INDEX IX_One on IndexTable ( IndexProperty ) WITH (FILLFACTOR = 100);
CREATE INDEX IX_Two on IndexTable ( IndexProperty ) WITH (FILLFACTOR = 50);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var table = dbModel.Tables.Single();

                Assert.Equal(2, table.Indexes.Count);
                Assert.All(
                    table.Indexes, c =>
                    {
                        Assert.Equal("dbo", c.Table!.Schema);
                        Assert.Equal("IndexTable", c.Table.Name);
                    });

                Assert.Collection(
                    table.Indexes.OrderBy(i => i.Name),
                    index =>
                    {
                        Assert.Equal("IX_One", index.Name);
                        Assert.Equal(100, index[SqlServerAnnotationNames.FillFactor]);
                    },
                    index =>
                    {
                        Assert.Equal("IX_Two", index.Name);
                        Assert.Equal(50, index[SqlServerAnnotationNames.FillFactor]);
                    });

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Collection(
                    model.GetEntityTypes(),
                    e =>
                    {
                        Assert.Equal("IndexTable", e.Name);
                        Assert.Collection(e.GetProperties(),
                            p => Assert.Equal("Id", p.Name),
                            p => Assert.Equal("IndexProperty", p.Name));
                        Assert.Empty(e.GetKeys());
                        Assert.Collection(
                            e.GetIndexes(),
                            i =>
                            {
                                Assert.Collection(i.Properties, p => Assert.Equal("IndexProperty", p.Name));
                                Assert.False(i.IsUnique);
                                Assert.Equal("IX_One", i.Name);
                                Assert.Equal(100, i.GetFillFactor());
                            },
                            i =>
                            {
                                Assert.Collection(i.Properties, p => Assert.Equal("IndexProperty", p.Name));
                                Assert.False(i.IsUnique);
                                Assert.Equal("IX_Two", i.Name);
                                Assert.Equal(50, i.GetFillFactor());
                            });
                        Assert.Empty(e.GetForeignKeys());
                        Assert.Empty(e.GetNavigations());
                        Assert.Empty(e.GetSkipNavigations());
                    });
            },
            "DROP TABLE IndexTable;");

    [ConditionalFact]
    public void Create_foreign_keys()
        => Test(
            @"
CREATE TABLE PrincipalTable (
    Id int PRIMARY KEY,
);

CREATE TABLE FirstDependent (
    Id int PRIMARY KEY,
    ForeignKeyId int,
    FOREIGN KEY (ForeignKeyId) REFERENCES PrincipalTable(Id) ON DELETE CASCADE,
);

CREATE TABLE SecondDependent (
    Id int PRIMARY KEY,
    FOREIGN KEY (Id) REFERENCES PrincipalTable(Id) ON DELETE NO ACTION,
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var firstFk = Assert.Single(dbModel.Tables.Single(t => t.Name == "FirstDependent").ForeignKeys);

                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("dbo", firstFk.Table.Schema);
                Assert.Equal("FirstDependent", firstFk.Table.Name);
                Assert.Equal("dbo", firstFk.PrincipalTable.Schema);
                Assert.Equal("PrincipalTable", firstFk.PrincipalTable.Name);
                Assert.Equal(["ForeignKeyId"], firstFk.Columns.Select(ic => ic.Name).ToList());
                Assert.Equal(["Id"], firstFk.PrincipalColumns.Select(ic => ic.Name).ToList());
                Assert.Equal(ReferentialAction.Cascade, firstFk.OnDelete);

                var secondFk = Assert.Single(dbModel.Tables.Single(t => t.Name == "SecondDependent").ForeignKeys);

                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("dbo", secondFk.Table.Schema);
                Assert.Equal("SecondDependent", secondFk.Table.Name);
                Assert.Equal("dbo", secondFk.PrincipalTable.Schema);
                Assert.Equal("PrincipalTable", secondFk.PrincipalTable.Name);
                Assert.Equal(["Id"], secondFk.Columns.Select(ic => ic.Name).ToList());
                Assert.Equal(["Id"], secondFk.PrincipalColumns.Select(ic => ic.Name).ToList());
                Assert.Equal(ReferentialAction.NoAction, secondFk.OnDelete);

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Collection(
                    model.GetEntityTypes(),
                    e =>
                    {
                        Assert.Equal("FirstDependent", e.Name);
                        Assert.Collection(e.GetProperties(),
                            p => Assert.Equal("Id", p.Name),
                            p => Assert.Equal("ForeignKeyId", p.Name));
                        Assert.Collection(e.GetForeignKeys(),
                            k =>
                            {
                                Assert.Equal("PrincipalTable", k.PrincipalEntityType.Name);
                                Assert.Equal("FirstDependent", k.DeclaringEntityType.Name);
                                Assert.Collection(k.Properties, p => Assert.Equal("ForeignKeyId", p.Name));
                                Assert.Collection(k.PrincipalKey.Properties, p => Assert.Equal("Id", p.Name));
                                Assert.False(k.IsUnique);
                            });
                        Assert.Empty(e.GetSkipNavigations());
                        Assert.Collection(e.GetNavigations(), n => Assert.Equal("ForeignKey", n.Name));
                    },
                    e =>
                    {
                        Assert.Equal("PrincipalTable", e.Name);
                        Assert.Collection(e.GetProperties(), p => Assert.Equal("Id", p.Name));
                        Assert.Empty(e.GetForeignKeys());
                        Assert.Empty(e.GetSkipNavigations());
                        Assert.Collection(e.GetNavigations(),
                            n =>
                            {
                                Assert.Equal("FirstDependents", n.Name);
                                Assert.True(n.IsCollection);
                            },
                            n =>
                            {
                                Assert.Equal("SecondDependent", n.Name);
                                Assert.False(n.IsCollection);
                            });
                    },
                    e =>
                    {
                        Assert.Equal("SecondDependent", e.Name);
                        Assert.Collection(e.GetProperties(), p => Assert.Equal("Id", p.Name));
                        Assert.Collection(e.GetForeignKeys(),
                            k =>
                            {
                                Assert.Equal("PrincipalTable", k.PrincipalEntityType.Name);
                                Assert.Equal("SecondDependent", k.DeclaringEntityType.Name);
                                Assert.Collection(k.Properties, p => Assert.Equal("Id", p.Name));
                                Assert.Collection(k.PrincipalKey.Properties, p => Assert.Equal("Id", p.Name));
                                Assert.True(k.IsUnique);
                            });
                        Assert.Empty(e.GetSkipNavigations());
                        Assert.Collection(e.GetNavigations(), n => Assert.Equal("IdNavigation", n.Name));
                    });
            },
            @"
DROP TABLE SecondDependent;
DROP TABLE FirstDependent;
DROP TABLE PrincipalTable;");

    [ConditionalFact]
    public void Triggers()
        => Test(
            [
                @"
CREATE TABLE SomeTable (
    Id int IDENTITY PRIMARY KEY,
    Foo int,
    Bar int,
    Baz int
);",
                @"
CREATE TRIGGER Trigger1
    ON SomeTable
    AFTER INSERT AS
BEGIN
    UPDATE SomeTable SET Bar=Foo WHERE Id IN (SELECT INSERTED.Id FROM INSERTED);
END;",
                @"
CREATE TRIGGER Trigger2
    ON SomeTable
    AFTER INSERT AS
BEGIN
    UPDATE SomeTable SET Baz=Foo WHERE Id IN (SELECT INSERTED.Id FROM INSERTED);
END;"
            ],
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var table = dbModel.Tables.Single();
                var triggers = table.Triggers;

                Assert.Collection(
                    triggers.OrderBy(t => t.Name),
                    t => Assert.Equal("Trigger1", t.Name),
                    t => Assert.Equal("Trigger2", t.Name));

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Collection(
                    model.GetEntityTypes(),
                    e =>
                    {
                        Assert.Equal("SomeTable", e.Name);
                        Assert.Collection(e.GetProperties(),
                            p => Assert.Equal("Id", p.Name),
                            p => Assert.Equal("Bar", p.Name),
                            p => Assert.Equal("Baz", p.Name),
                            p => Assert.Equal("Foo", p.Name));
                        Assert.Empty(e.GetForeignKeys());
                        Assert.Empty(e.GetSkipNavigations());
                        Assert.Empty(e.GetNavigations());
                        Assert.Collection(e.GetDeclaredTriggers(),
                            t => Assert.Equal("Trigger1", t.ModelName),
                            t => Assert.Equal("Trigger2", t.ModelName));
                    });
            },
            "DROP TABLE SomeTable;");

    [ConditionalTheory] // Issue #31121
    [InlineData("events", false, false, "Events", "Id", "Class", "Strings", "_", "_1")]
    [InlineData("events", false, true, "Event", "Id", "Class", "Strings", "_", "_1")]
    [InlineData("events", true, false, "events", "Id", "_class", "strings", "_", "_1")]
    [InlineData("events", true, true, "_event", "Id", "_class", "strings", "_", "_1")]
    [InlineData("event", false, false, "Event", "Id", "Class", "Strings", "_", "_1")]
    [InlineData("event", false, true, "Event", "Id", "Class", "Strings", "_", "_1")]
    [InlineData("event", true, false, "_event", "Id", "_class", "strings", "_", "_1")]
    [InlineData("event", true, true, "_event", "Id", "_class", "strings", "_", "_1")]
    public void Table_name_with_pluralized_keywords(
        string tableName,
        bool useDatabaseNames, bool singularize,
        string entityTypeName, string idName, string className, string stringsName, string oneName, string plusName)
        => Test(
            @$"
CREATE TABLE [{tableName}] (
    Id int IDENTITY PRIMARY KEY,
    [class] int,
    [strings] int,
    [1] int,
    [+] int
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                Assert.Collection(dbModel.Tables,
                    t =>
                    {
                        Assert.Equal(tableName, t.Name);
                        Assert.Collection(t.Columns,
                            c => Assert.Equal("Id", c.Name),
                            c => Assert.Equal("class", c.Name),
                            c => Assert.Equal("strings", c.Name),
                            c => Assert.Equal("1", c.Name),
                            c => Assert.Equal("+", c.Name));
                    });

                var model = scaffoldingFactory.Create(dbModel, new() { UseDatabaseNames = useDatabaseNames, NoPluralize = !singularize });

                Assert.Collection(model.GetEntityTypes(),
                    e =>
                    {
                        Assert.Equal(entityTypeName, e.Name);
                        var properties = e.GetProperties().Select(p => p.Name).ToList();
                        Assert.Equal(5, properties.Count());
                        Assert.Contains(idName, properties);
                        Assert.Contains(className, properties);
                        Assert.Contains(stringsName, properties);
                        Assert.Contains(oneName, properties);
                        Assert.Contains(plusName, properties);
                    });
            },
            $"DROP TABLE [{tableName}];");

    #endregion

    #region ColumnFacets

    [ConditionalFact]
    public void Column_with_type_alias_assigns_underlying_store_type()
    {
        Fixture.TestStore.ExecuteNonQuery(
            @"
CREATE TYPE dbo.TestTypeAlias FROM nvarchar(max);
CREATE TYPE db2.TestTypeAlias FROM int;");

        Test(
            @"
CREATE TABLE TypeAlias (
    Id int,
    typeAliasColumn dbo.TestTypeAlias NULL
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var column = Assert.Single(dbModel.Tables.Single().Columns.Where(c => c.Name == "typeAliasColumn"));

                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("nvarchar(max)", column.StoreType);

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Collection(
                    model.GetEntityTypes(),
                    e =>
                    {
                        Assert.Equal("TypeAlias", e.Name);
                        Assert.Collection(e.GetProperties(),
                            p => Assert.Equal("Id", p.Name),
                            p =>
                            {
                                Assert.Equal("TypeAliasColumn", p.Name);
                                Assert.Same(typeof(string), p.ClrType);
                            });
                        Assert.Empty(e.GetForeignKeys());
                        Assert.Empty(e.GetSkipNavigations());
                        Assert.Empty(e.GetNavigations());
                    });
            },
            @"
DROP TABLE TypeAlias;
DROP TYPE dbo.TestTypeAlias;
DROP TYPE db2.TestTypeAlias;");
    }

    [ConditionalFact]
    public void Column_with_sysname_assigns_underlying_store_type_and_nullability()
        => Test(
            @"
CREATE TABLE TypeAlias (
    Id int,
    typeAliasColumn sysname
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var column = Assert.Single(dbModel.Tables.Single().Columns.Where(c => c.Name == "typeAliasColumn"));

                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("nvarchar(128)", column.StoreType);
                Assert.False(column.IsNullable);

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Collection(
                    model.GetEntityTypes(),
                    e =>
                    {
                        Assert.Equal("TypeAlias", e.Name);
                        Assert.Collection(e.GetProperties(),
                            p => Assert.Equal("Id", p.Name),
                            p =>
                            {
                                Assert.Equal("TypeAliasColumn", p.Name);
                                Assert.Same(typeof(string), p.ClrType);
                            });
                        Assert.Empty(e.GetForeignKeys());
                        Assert.Empty(e.GetSkipNavigations());
                        Assert.Empty(e.GetNavigations());
                    });
            },
            @"
DROP TABLE TypeAlias;");

    [ConditionalFact]
    public void Decimal_numeric_types_have_precision_scale()
        => Test(
            @"
CREATE TABLE NumericColumns (
    Id int,
    decimalColumn decimal NOT NULL,
    decimal105Column decimal(10, 5) NOT NULL,
    decimalDefaultColumn decimal(18, 2) NOT NULL,
    numericColumn numeric NOT NULL,
    numeric152Column numeric(15, 2) NOT NULL,
    numericDefaultColumn numeric(18, 2) NOT NULL,
    numericDefaultPrecisionColumn numeric(38, 5) NOT NULL,
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                Assert.Equal("decimal(18, 0)", columns.Single(c => c.Name == "decimalColumn").StoreType);
                Assert.Equal("decimal(10, 5)", columns.Single(c => c.Name == "decimal105Column").StoreType);
                Assert.Equal("decimal(18, 2)", columns.Single(c => c.Name == "decimalDefaultColumn").StoreType);
                Assert.Equal("numeric(18, 0)", columns.Single(c => c.Name == "numericColumn").StoreType);
                Assert.Equal("numeric(15, 2)", columns.Single(c => c.Name == "numeric152Column").StoreType);
                Assert.Equal("numeric(18, 2)", columns.Single(c => c.Name == "numericDefaultColumn").StoreType);
                Assert.Equal("numeric(38, 5)", columns.Single(c => c.Name == "numericDefaultPrecisionColumn").StoreType);

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Collection(
                    model.GetEntityTypes(),
                    e =>
                    {
                        Assert.Equal("NumericColumn", e.Name);
                        Assert.Collection(
                            e.GetProperties(),
                            p =>
                            {
                                Assert.Equal("Decimal105Column", p.Name);
                                Assert.Same(typeof(decimal), p.ClrType);
                                // Assert.Equal(10, p.GetPrecision());
                                // Assert.Equal(5, p.GetScale());
                            },
                            p =>
                            {
                                Assert.Equal("DecimalColumn", p.Name);
                                Assert.Same(typeof(decimal), p.ClrType);
                                // Assert.Equal(18, p.GetPrecision());
                                // Assert.Equal(0, p.GetScale());
                            },
                            p =>
                            {
                                Assert.Equal("DecimalDefaultColumn", p.Name);
                                Assert.Same(typeof(decimal), p.ClrType);
                                // Assert.Equal(18, p.GetPrecision());
                                // Assert.Equal(2, p.GetScale());
                            },
                            p => Assert.Equal("Id", p.Name),
                            p =>
                            {
                                Assert.Equal("Numeric152Column", p.Name);
                                Assert.Same(typeof(decimal), p.ClrType);
                                // Assert.Equal(15, p.GetPrecision());
                                // Assert.Equal(2, p.GetScale());
                            },
                            p =>
                            {
                                Assert.Equal("NumericColumn1", p.Name); // Because property name clashes with class name
                                Assert.Same(typeof(decimal), p.ClrType);
                                // Assert.Equal(18, p.GetPrecision());
                                // Assert.Equal(0, p.GetScale());
                            },
                            p =>
                            {
                                Assert.Equal("NumericDefaultColumn", p.Name);
                                Assert.Same(typeof(decimal), p.ClrType);
                                // Assert.Equal(18, p.GetPrecision());
                                // Assert.Equal(2, p.GetScale());
                            },
                            p =>
                            {
                                Assert.Equal("NumericDefaultPrecisionColumn", p.Name);
                                Assert.Same(typeof(decimal), p.ClrType);
                                // Assert.Equal(38, p.GetPrecision());
                                // Assert.Equal(5, p.GetScale());
                            });
                        Assert.Empty(e.GetForeignKeys());
                        Assert.Empty(e.GetSkipNavigations());
                        Assert.Empty(e.GetNavigations());
                    });
            },
            "DROP TABLE NumericColumns;");

    [ConditionalFact]
    public void Max_length_of_negative_one_translate_to_max_in_store_type()
        => Test(
            @"
CREATE TABLE MaxColumns (
    Id int,
    varcharMaxColumn varchar(max) NULL,
    nvarcharMaxColumn nvarchar(max) NULL,
    varbinaryMaxColumn varbinary(max) NULL,
    binaryVaryingMaxColumn binary varying(max) NULL,
    charVaryingMaxColumn char varying(max) NULL,
    characterVaryingMaxColumn character varying(max) NULL,
    nationalCharVaryingMaxColumn national char varying(max) NULL,
    nationalCharacterVaryingMaxColumn national char varying(max) NULL
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                Assert.Equal("varchar(max)", columns.Single(c => c.Name == "varcharMaxColumn").StoreType);
                Assert.Equal("nvarchar(max)", columns.Single(c => c.Name == "nvarcharMaxColumn").StoreType);
                Assert.Equal("varbinary(max)", columns.Single(c => c.Name == "varbinaryMaxColumn").StoreType);
                Assert.Equal("varbinary(max)", columns.Single(c => c.Name == "binaryVaryingMaxColumn").StoreType);
                Assert.Equal("varchar(max)", columns.Single(c => c.Name == "charVaryingMaxColumn").StoreType);
                Assert.Equal("varchar(max)", columns.Single(c => c.Name == "characterVaryingMaxColumn").StoreType);
                Assert.Equal("nvarchar(max)", columns.Single(c => c.Name == "nationalCharVaryingMaxColumn").StoreType);
                Assert.Equal("nvarchar(max)", columns.Single(c => c.Name == "nationalCharacterVaryingMaxColumn").StoreType);

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Collection(
                    model.GetEntityTypes(),
                    e =>
                    {
                        Assert.Equal("MaxColumn", e.Name);
                        Assert.Collection(
                            e.GetProperties(),
                            p =>
                            {
                                Assert.Equal("BinaryVaryingMaxColumn", p.Name);
                                Assert.Same(typeof(byte[]), p.ClrType);
                                Assert.Null(p.GetMaxLength());
                            },
                            p =>
                            {
                                Assert.Equal("CharVaryingMaxColumn", p.Name);
                                Assert.Same(typeof(string), p.ClrType);
                                Assert.Null(p.GetMaxLength());
                            },
                            p =>
                            {
                                Assert.Equal("CharacterVaryingMaxColumn", p.Name);
                                Assert.Same(typeof(string), p.ClrType);
                                Assert.Null(p.GetMaxLength());
                            },
                            p => Assert.Equal("Id", p.Name),
                            p =>
                            {
                                Assert.Equal("NationalCharVaryingMaxColumn", p.Name);
                                Assert.Same(typeof(string), p.ClrType);
                                Assert.Null(p.GetMaxLength());
                            },
                            p =>
                            {
                                Assert.Equal("NationalCharacterVaryingMaxColumn", p.Name);
                                Assert.Same(typeof(string), p.ClrType);
                                Assert.Null(p.GetMaxLength());
                            },
                            p =>
                            {
                                Assert.Equal("NvarcharMaxColumn", p.Name);
                                Assert.Same(typeof(string), p.ClrType);
                                Assert.Null(p.GetMaxLength());
                            },
                            p =>
                            {
                                Assert.Equal("VarbinaryMaxColumn", p.Name);
                                Assert.Same(typeof(byte[]), p.ClrType);
                                Assert.Null(p.GetMaxLength());
                            },
                            p =>
                            {
                                Assert.Equal("VarcharMaxColumn", p.Name);
                                Assert.Same(typeof(string), p.ClrType);
                                Assert.Null(p.GetMaxLength());
                            });
                        Assert.Empty(e.GetForeignKeys());
                        Assert.Empty(e.GetSkipNavigations());
                        Assert.Empty(e.GetNavigations());
                    });
            },
            "DROP TABLE MaxColumns;");

    [ConditionalFact]
    public void Specific_max_length_are_add_to_store_type()
        => Test(
            @"
CREATE TABLE LengthColumns (
    Id int,
    char10Column char(10) NULL,
    varchar66Column varchar(66) NULL,
    nchar99Column nchar(99) NULL,
    nvarchar100Column nvarchar(100) NULL,
    binary111Column binary(111) NULL,
    varbinary123Column varbinary(123) NULL,
    binaryVarying133Column binary varying(133) NULL,
    charVarying144Column char varying(144) NULL,
    character155Column character(155) NULL,
    characterVarying166Column character varying(166) NULL,
    nationalCharacter171Column national character(171) NULL,
    nationalCharVarying177Column national char varying(177) NULL,
    nationalCharacterVarying188Column national char varying(188) NULL,
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                Assert.Equal("char(10)", columns.Single(c => c.Name == "char10Column").StoreType);
                Assert.Equal("varchar(66)", columns.Single(c => c.Name == "varchar66Column").StoreType);
                Assert.Equal("nchar(99)", columns.Single(c => c.Name == "nchar99Column").StoreType);
                Assert.Equal("nvarchar(100)", columns.Single(c => c.Name == "nvarchar100Column").StoreType);
                Assert.Equal("binary(111)", columns.Single(c => c.Name == "binary111Column").StoreType);
                Assert.Equal("varbinary(123)", columns.Single(c => c.Name == "varbinary123Column").StoreType);
                Assert.Equal("varbinary(133)", columns.Single(c => c.Name == "binaryVarying133Column").StoreType);
                Assert.Equal("varchar(144)", columns.Single(c => c.Name == "charVarying144Column").StoreType);
                Assert.Equal("char(155)", columns.Single(c => c.Name == "character155Column").StoreType);
                Assert.Equal("varchar(166)", columns.Single(c => c.Name == "characterVarying166Column").StoreType);
                Assert.Equal("nchar(171)", columns.Single(c => c.Name == "nationalCharacter171Column").StoreType);
                Assert.Equal("nvarchar(177)", columns.Single(c => c.Name == "nationalCharVarying177Column").StoreType);
                Assert.Equal("nvarchar(188)", columns.Single(c => c.Name == "nationalCharacterVarying188Column").StoreType);

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Collection(
                    model.GetEntityTypes(),
                    e =>
                    {
                        Assert.Equal("LengthColumn", e.Name);
                        Assert.Collection(
                            e.GetProperties(),
                            p =>
                            {
                                Assert.Equal("Binary111Column", p.Name);
                                Assert.Same(typeof(byte[]), p.ClrType);
                                Assert.Equal(111, p.GetMaxLength());
                                Assert.True(p.IsFixedLength());
                            },
                            p =>
                            {
                                Assert.Equal("BinaryVarying133Column", p.Name);
                                Assert.Same(typeof(byte[]), p.ClrType);
                                Assert.Equal(133, p.GetMaxLength());
                                Assert.Null(p.IsFixedLength());
                            },
                            p =>
                            {
                                Assert.Equal("Char10Column", p.Name);
                                Assert.Same(typeof(string), p.ClrType);
                                Assert.Equal(10, p.GetMaxLength());
                                Assert.False(p.IsUnicode());
                                Assert.True(p.IsFixedLength());
                            },
                            p =>
                            {
                                Assert.Equal("CharVarying144Column", p.Name);
                                Assert.Same(typeof(string), p.ClrType);
                                Assert.Equal(144, p.GetMaxLength());
                                Assert.False(p.IsUnicode());
                                Assert.Null(p.IsFixedLength());
                            },
                            p =>
                            {
                                Assert.Equal("Character155Column", p.Name);
                                Assert.Same(typeof(string), p.ClrType);
                                Assert.Equal(155, p.GetMaxLength());
                                Assert.False(p.IsUnicode());
                                Assert.True(p.IsFixedLength());
                            },
                            p =>
                            {
                                Assert.Equal("CharacterVarying166Column", p.Name);
                                Assert.Same(typeof(string), p.ClrType);
                                Assert.Equal(166, p.GetMaxLength());
                                Assert.False(p.IsUnicode());
                                Assert.Null(p.IsFixedLength());
                            },
                            p => Assert.Equal("Id", p.Name),
                            p =>
                            {
                                Assert.Equal("NationalCharVarying177Column", p.Name);
                                Assert.Same(typeof(string), p.ClrType);
                                Assert.Equal(177, p.GetMaxLength());
                                Assert.Null(p.IsUnicode());
                                Assert.Null(p.IsFixedLength());
                            },
                            p =>
                            {
                                Assert.Equal("NationalCharacter171Column", p.Name);
                                Assert.Same(typeof(string), p.ClrType);
                                Assert.Equal(171, p.GetMaxLength());
                                Assert.Null(p.IsUnicode());
                                Assert.True(p.IsFixedLength());
                            },
                            p =>
                            {
                                Assert.Equal("NationalCharacterVarying188Column", p.Name);
                                Assert.Same(typeof(string), p.ClrType);
                                Assert.Equal(188, p.GetMaxLength());
                                Assert.Null(p.IsUnicode());
                                Assert.Null(p.IsFixedLength());
                            },
                            p =>
                            {
                                Assert.Equal("Nchar99Column", p.Name);
                                Assert.Same(typeof(string), p.ClrType);
                                Assert.Equal(99, p.GetMaxLength());
                                Assert.Null(p.IsUnicode());
                                Assert.True(p.IsFixedLength());
                            },
                            p =>
                            {
                                Assert.Equal("Nvarchar100Column", p.Name);
                                Assert.Same(typeof(string), p.ClrType);
                                Assert.Equal(100, p.GetMaxLength());
                                Assert.Null(p.IsUnicode());
                                Assert.Null(p.IsFixedLength());
                            },
                            p =>
                            {
                                Assert.Equal("Varbinary123Column", p.Name);
                                Assert.Same(typeof(byte[]), p.ClrType);
                                Assert.Equal(123, p.GetMaxLength());
                                Assert.Null(p.IsFixedLength());
                            },
                            p =>
                            {
                                Assert.Equal("Varchar66Column", p.Name);
                                Assert.Same(typeof(string), p.ClrType);
                                Assert.Equal(66, p.GetMaxLength());
                                Assert.False(p.IsUnicode());
                                Assert.Null(p.IsFixedLength());
                            });
                        Assert.Empty(e.GetForeignKeys());
                        Assert.Empty(e.GetSkipNavigations());
                        Assert.Empty(e.GetNavigations());
                    });
            },
            "DROP TABLE LengthColumns;");

    [ConditionalFact]
    public void Default_max_length_are_added_to_binary_varbinary()
        => Test(
            @"
CREATE TABLE DefaultRequiredLengthBinaryColumns (
    Id int,
    binaryColumn binary(8000),
    binaryVaryingColumn binary varying(8000),
    varbinaryColumn varbinary(8000)
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                Assert.Equal("binary(8000)", columns.Single(c => c.Name == "binaryColumn").StoreType);
                Assert.Equal("varbinary(8000)", columns.Single(c => c.Name == "binaryVaryingColumn").StoreType);
                Assert.Equal("varbinary(8000)", columns.Single(c => c.Name == "varbinaryColumn").StoreType);

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Collection(
                    model.GetEntityTypes(),
                    e =>
                    {
                        Assert.Equal("DefaultRequiredLengthBinaryColumn", e.Name);
                        Assert.Collection(
                            e.GetProperties(),
                            p =>
                            {
                                Assert.Equal("BinaryColumn", p.Name);
                                Assert.Same(typeof(byte[]), p.ClrType);
                                Assert.Equal(8000, p.GetMaxLength());
                                Assert.True(p.IsFixedLength());
                            },
                            p =>
                            {
                                Assert.Equal("BinaryVaryingColumn", p.Name);
                                Assert.Same(typeof(byte[]), p.ClrType);
                                Assert.Equal(8000, p.GetMaxLength());
                                Assert.Null(p.IsFixedLength());
                            },
                            p => Assert.Equal("Id", p.Name),
                            p =>
                            {
                                Assert.Equal("VarbinaryColumn", p.Name);
                                Assert.Same(typeof(byte[]), p.ClrType);
                                Assert.Equal(8000, p.GetMaxLength());
                                Assert.Null(p.IsUnicode());
                                Assert.Null(p.IsFixedLength());
                            });
                        Assert.Empty(e.GetForeignKeys());
                        Assert.Empty(e.GetSkipNavigations());
                        Assert.Empty(e.GetNavigations());
                    });
            },
            "DROP TABLE DefaultRequiredLengthBinaryColumns;");

    [ConditionalFact]
    public void Default_max_length_are_added_to_char_1()
        => Test(
            @"
CREATE TABLE DefaultRequiredLengthCharColumns (
    Id int,
    charColumn char(8000)
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                Assert.Equal("char(8000)", columns.Single(c => c.Name == "charColumn").StoreType);

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Collection(
                    model.GetEntityTypes(),
                    e =>
                    {
                        Assert.Equal("DefaultRequiredLengthCharColumn", e.Name);
                        Assert.Collection(
                            e.GetProperties(),
                            p =>
                            {
                                Assert.Equal("CharColumn", p.Name);
                                Assert.Same(typeof(string), p.ClrType);
                                Assert.Equal(8000, p.GetMaxLength());
                                Assert.False(p.IsUnicode());
                                Assert.True(p.IsFixedLength());
                            },
                            p => Assert.Equal("Id", p.Name));
                        Assert.Empty(e.GetForeignKeys());
                        Assert.Empty(e.GetSkipNavigations());
                        Assert.Empty(e.GetNavigations());
                    });
            },
            "DROP TABLE DefaultRequiredLengthCharColumns;");

    [ConditionalFact]
    public void Default_max_length_are_added_to_char_2()
        => Test(
            @"
CREATE TABLE DefaultRequiredLengthCharColumns (
    Id int,
    characterColumn character(8000)
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                Assert.Equal("char(8000)", columns.Single(c => c.Name == "characterColumn").StoreType);

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Collection(
                    model.GetEntityTypes(),
                    e =>
                    {
                        Assert.Equal("DefaultRequiredLengthCharColumn", e.Name);
                        Assert.Collection(
                            e.GetProperties(),
                            p =>
                            {
                                Assert.Equal("CharacterColumn", p.Name);
                                Assert.Same(typeof(string), p.ClrType);
                                Assert.Equal(8000, p.GetMaxLength());
                                Assert.False(p.IsUnicode());
                                Assert.True(p.IsFixedLength());
                            },
                            p => Assert.Equal("Id", p.Name));
                        Assert.Empty(e.GetForeignKeys());
                        Assert.Empty(e.GetSkipNavigations());
                        Assert.Empty(e.GetNavigations());
                    });
            },
            "DROP TABLE DefaultRequiredLengthCharColumns;");

    [ConditionalFact]
    public void Default_max_length_are_added_to_varchar()
        => Test(
            @"
CREATE TABLE DefaultRequiredLengthVarcharColumns (
    Id int,
    charVaryingColumn char varying(8000),
    characterVaryingColumn character varying(8000),
    varcharColumn varchar(8000)
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                Assert.Equal("varchar(8000)", columns.Single(c => c.Name == "charVaryingColumn").StoreType);
                Assert.Equal("varchar(8000)", columns.Single(c => c.Name == "characterVaryingColumn").StoreType);
                Assert.Equal("varchar(8000)", columns.Single(c => c.Name == "varcharColumn").StoreType);

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Collection(
                    model.GetEntityTypes(),
                    e =>
                    {
                        Assert.Equal("DefaultRequiredLengthVarcharColumn", e.Name);
                        Assert.Collection(
                            e.GetProperties(),
                            p =>
                            {
                                Assert.Equal("CharVaryingColumn", p.Name);
                                Assert.Same(typeof(string), p.ClrType);
                                Assert.Equal(8000, p.GetMaxLength());
                                Assert.False(p.IsUnicode());
                                Assert.Null(p.IsFixedLength());
                            },
                            p =>
                            {
                                Assert.Equal("CharacterVaryingColumn", p.Name);
                                Assert.Same(typeof(string), p.ClrType);
                                Assert.Equal(8000, p.GetMaxLength());
                                Assert.False(p.IsUnicode());
                                Assert.Null(p.IsFixedLength());
                            },
                            p => Assert.Equal("Id", p.Name),
                            p =>
                            {
                                Assert.Equal("VarcharColumn", p.Name);
                                Assert.Same(typeof(string), p.ClrType);
                                Assert.Equal(8000, p.GetMaxLength());
                                Assert.False(p.IsUnicode());
                                Assert.Null(p.IsFixedLength());
                            });
                        Assert.Empty(e.GetForeignKeys());
                        Assert.Empty(e.GetSkipNavigations());
                        Assert.Empty(e.GetNavigations());
                    });
            },
            "DROP TABLE DefaultRequiredLengthVarcharColumns;");

    [ConditionalFact]
    public void Default_max_length_are_added_to_nchar_1()
        => Test(
            @"
CREATE TABLE DefaultRequiredLengthNcharColumns (
    Id int,
    nationalCharColumn national char(4000),
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                Assert.Equal("nchar(4000)", columns.Single(c => c.Name == "nationalCharColumn").StoreType);

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Collection(
                    model.GetEntityTypes(),
                    e =>
                    {
                        Assert.Equal("DefaultRequiredLengthNcharColumn", e.Name);
                        Assert.Collection(
                            e.GetProperties(),
                            p => Assert.Equal("Id", p.Name),
                            p =>
                            {
                                Assert.Equal("NationalCharColumn", p.Name);
                                Assert.Same(typeof(string), p.ClrType);
                                Assert.Equal(4000, p.GetMaxLength());
                                Assert.Null(p.IsUnicode());
                                Assert.True(p.IsFixedLength());
                            });
                        Assert.Empty(e.GetForeignKeys());
                        Assert.Empty(e.GetSkipNavigations());
                        Assert.Empty(e.GetNavigations());
                    });
            },
            "DROP TABLE DefaultRequiredLengthNcharColumns;");

    [ConditionalFact]
    public void Default_max_length_are_added_to_nchar_2()
        => Test(
            @"
CREATE TABLE DefaultRequiredLengthNcharColumns (
    Id int,
    nationalCharacterColumn national character(4000),
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                Assert.Equal("nchar(4000)", columns.Single(c => c.Name == "nationalCharacterColumn").StoreType);

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Collection(
                    model.GetEntityTypes(),
                    e =>
                    {
                        Assert.Equal("DefaultRequiredLengthNcharColumn", e.Name);
                        Assert.Collection(
                            e.GetProperties(),
                            p => Assert.Equal("Id", p.Name),
                            p =>
                            {
                                Assert.Equal("NationalCharacterColumn", p.Name);
                                Assert.Same(typeof(string), p.ClrType);
                                Assert.Equal(4000, p.GetMaxLength());
                                Assert.Null(p.IsUnicode());
                                Assert.True(p.IsFixedLength());
                            });
                        Assert.Empty(e.GetForeignKeys());
                        Assert.Empty(e.GetSkipNavigations());
                        Assert.Empty(e.GetNavigations());
                    });
            },
            "DROP TABLE DefaultRequiredLengthNcharColumns;");

    [ConditionalFact]
    public void Default_max_length_are_added_to_nchar_3()
        => Test(
            @"
CREATE TABLE DefaultRequiredLengthNcharColumns (
    Id int,
    ncharColumn nchar(4000),
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                Assert.Equal("nchar(4000)", columns.Single(c => c.Name == "ncharColumn").StoreType);

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Collection(
                    model.GetEntityTypes(),
                    e =>
                    {
                        Assert.Equal("DefaultRequiredLengthNcharColumn", e.Name);
                        Assert.Collection(
                            e.GetProperties(),
                            p => Assert.Equal("Id", p.Name),
                            p =>
                            {
                                Assert.Equal("NcharColumn", p.Name);
                                Assert.Same(typeof(string), p.ClrType);
                                Assert.Equal(4000, p.GetMaxLength());
                                Assert.Null(p.IsUnicode());
                                Assert.True(p.IsFixedLength());
                            });
                        Assert.Empty(e.GetForeignKeys());
                        Assert.Empty(e.GetSkipNavigations());
                        Assert.Empty(e.GetNavigations());
                    });
            },
            "DROP TABLE DefaultRequiredLengthNcharColumns;");

    [ConditionalFact]
    public void Default_max_length_are_added_to_nvarchar()
        => Test(
            @"
CREATE TABLE DefaultRequiredLengthNvarcharColumns (
    Id int,
    nationalCharVaryingColumn national char varying(4000),
    nationalCharacterVaryingColumn national character varying(4000),
    nvarcharColumn nvarchar(4000)
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                Assert.Equal("nvarchar(4000)", columns.Single(c => c.Name == "nationalCharVaryingColumn").StoreType);
                Assert.Equal("nvarchar(4000)", columns.Single(c => c.Name == "nationalCharacterVaryingColumn").StoreType);
                Assert.Equal("nvarchar(4000)", columns.Single(c => c.Name == "nvarcharColumn").StoreType);

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Collection(
                    model.GetEntityTypes(),
                    e =>
                    {
                        Assert.Equal("DefaultRequiredLengthNvarcharColumn", e.Name);
                        Assert.Collection(
                            e.GetProperties(),
                            p => Assert.Equal("Id", p.Name),
                            p =>
                            {
                                Assert.Equal("NationalCharVaryingColumn", p.Name);
                                Assert.Same(typeof(string), p.ClrType);
                                Assert.Equal(4000, p.GetMaxLength());
                                Assert.Null(p.IsUnicode());
                                Assert.Null(p.IsFixedLength());
                            },
                            p =>
                            {
                                Assert.Equal("NationalCharacterVaryingColumn", p.Name);
                                Assert.Same(typeof(string), p.ClrType);
                                Assert.Equal(4000, p.GetMaxLength());
                                Assert.Null(p.IsUnicode());
                                Assert.Null(p.IsFixedLength());
                            },
                            p =>
                            {
                                Assert.Equal("NvarcharColumn", p.Name);
                                Assert.Same(typeof(string), p.ClrType);
                                Assert.Equal(4000, p.GetMaxLength());
                                Assert.Null(p.IsUnicode());
                                Assert.Null(p.IsFixedLength());
                            });
                        Assert.Empty(e.GetForeignKeys());
                        Assert.Empty(e.GetSkipNavigations());
                        Assert.Empty(e.GetNavigations());
                    });
            },
            "DROP TABLE DefaultRequiredLengthNvarcharColumns;");

    [ConditionalFact]
    public void Datetime_types_have_precision_if_non_null_scale()
        => Test(
            @"
CREATE TABLE LengthColumns (
    Id int,
    time4Column time(4) NULL,
    datetime24Column datetime2(4) NULL,
    datetimeoffset5Column datetimeoffset(5) NULL,
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                Assert.Equal("time(4)", columns.Single(c => c.Name == "time4Column").StoreType);
                Assert.Equal("datetime2(4)", columns.Single(c => c.Name == "datetime24Column").StoreType);
                Assert.Equal("datetimeoffset(5)", columns.Single(c => c.Name == "datetimeoffset5Column").StoreType);

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Collection(
                    model.GetEntityTypes(),
                    e =>
                    {
                        Assert.Equal("LengthColumn", e.Name);
                        Assert.Collection(
                            e.GetProperties(),
                            p =>
                            {
                                Assert.Equal("Datetime24Column", p.Name);
                                Assert.Same(typeof(DateTime?), p.ClrType);
                                Assert.Equal(4, p.GetPrecision());
                                Assert.Null(p.GetScale());
                            },
                            p =>
                            {
                                Assert.Equal("Datetimeoffset5Column", p.Name);
                                Assert.Same(typeof(DateTimeOffset?), p.ClrType);
                                Assert.Equal(5, p.GetPrecision());
                                Assert.Null(p.GetScale());
                            },
                            p => Assert.Equal("Id", p.Name),
                            p =>
                            {
                                Assert.Equal("Time4Column", p.Name);
                                Assert.Same(typeof(TimeOnly?), p.ClrType);
                                Assert.Equal(4, p.GetPrecision());
                                Assert.Null(p.GetScale());
                            });
                        Assert.Empty(e.GetForeignKeys());
                        Assert.Empty(e.GetSkipNavigations());
                        Assert.Empty(e.GetNavigations());
                    });
            },
            "DROP TABLE LengthColumns;");

    [ConditionalFact]
    public void Types_with_required_length_uses_length_of_one()
        => Test(
            @"
CREATE TABLE OneLengthColumns (
    Id int,
    binaryColumn binary NULL,
    binaryVaryingColumn binary varying NULL,
    characterColumn character NULL,
    characterVaryingColumn character varying NULL,
    charColumn char NULL,
    charVaryingColumn char varying NULL,
    nationalCharColumn national char NULL,
    nationalCharacterColumn national character NULL,
    nationalCharacterVaryingColumn national char varying NULL,
    nationalCharVaryingColumn national char varying NULL,
    ncharColumn nchar NULL,
    nvarcharColumn nvarchar NULL,
    varbinaryColumn varbinary NULL,
    varcharColumn varchar NULL,
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                Assert.Equal("binary(1)", columns.Single(c => c.Name == "binaryColumn").StoreType);
                Assert.Equal("varbinary(1)", columns.Single(c => c.Name == "binaryVaryingColumn").StoreType);
                Assert.Equal("char(1)", columns.Single(c => c.Name == "characterColumn").StoreType);
                Assert.Equal("varchar(1)", columns.Single(c => c.Name == "characterVaryingColumn").StoreType);
                Assert.Equal("char(1)", columns.Single(c => c.Name == "charColumn").StoreType);
                Assert.Equal("varchar(1)", columns.Single(c => c.Name == "charVaryingColumn").StoreType);
                Assert.Equal("nchar(1)", columns.Single(c => c.Name == "nationalCharColumn").StoreType);
                Assert.Equal("nchar(1)", columns.Single(c => c.Name == "nationalCharacterColumn").StoreType);
                Assert.Equal("nvarchar(1)", columns.Single(c => c.Name == "nationalCharacterVaryingColumn").StoreType);
                Assert.Equal("nvarchar(1)", columns.Single(c => c.Name == "nationalCharVaryingColumn").StoreType);
                Assert.Equal("nchar(1)", columns.Single(c => c.Name == "ncharColumn").StoreType);
                Assert.Equal("nvarchar(1)", columns.Single(c => c.Name == "nvarcharColumn").StoreType);
                Assert.Equal("varbinary(1)", columns.Single(c => c.Name == "varbinaryColumn").StoreType);
                Assert.Equal("varchar(1)", columns.Single(c => c.Name == "varcharColumn").StoreType);

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Collection(
                    model.GetEntityTypes(),
                    e =>
                    {
                        Assert.Equal("OneLengthColumn", e.Name);
                        Assert.Collection(
                            e.GetProperties(),
                            p =>
                            {
                                Assert.Equal("BinaryColumn", p.Name);
                                Assert.Same(typeof(byte[]), p.ClrType);
                                Assert.Equal(1, p.GetMaxLength());
                            },
                            p =>
                            {
                                Assert.Equal("BinaryVaryingColumn", p.Name);
                                Assert.Same(typeof(byte[]), p.ClrType);
                                Assert.Equal(1, p.GetMaxLength());
                            },
                            p =>
                            {
                                Assert.Equal("CharColumn", p.Name);
                                Assert.Same(typeof(string), p.ClrType);
                                Assert.Equal(1, p.GetMaxLength());
                            },
                            p =>
                            {
                                Assert.Equal("CharVaryingColumn", p.Name);
                                Assert.Same(typeof(string), p.ClrType);
                                Assert.Equal(1, p.GetMaxLength());
                            },
                            p =>
                            {
                                Assert.Equal("CharacterColumn", p.Name);
                                Assert.Same(typeof(string), p.ClrType);
                                Assert.Equal(1, p.GetMaxLength());
                            },
                            p =>
                            {
                                Assert.Equal("CharacterVaryingColumn", p.Name);
                                Assert.Same(typeof(string), p.ClrType);
                                Assert.Equal(1, p.GetMaxLength());
                            },
                            p => Assert.Equal("Id", p.Name),
                            p =>
                            {
                                Assert.Equal("NationalCharColumn", p.Name);
                                Assert.Same(typeof(string), p.ClrType);
                                Assert.Equal(1, p.GetMaxLength());
                            },
                            p =>
                            {
                                Assert.Equal("NationalCharVaryingColumn", p.Name);
                                Assert.Same(typeof(string), p.ClrType);
                                Assert.Equal(1, p.GetMaxLength());
                            },
                            p =>
                            {
                                Assert.Equal("NationalCharacterColumn", p.Name);
                                Assert.Same(typeof(string), p.ClrType);
                                Assert.Equal(1, p.GetMaxLength());
                            },
                            p =>
                            {
                                Assert.Equal("NationalCharacterVaryingColumn", p.Name);
                                Assert.Same(typeof(string), p.ClrType);
                                Assert.Equal(1, p.GetMaxLength());
                            },
                            p =>
                            {
                                Assert.Equal("NcharColumn", p.Name);
                                Assert.Same(typeof(string), p.ClrType);
                                Assert.Equal(1, p.GetMaxLength());
                            },
                            p =>
                            {
                                Assert.Equal("NvarcharColumn", p.Name);
                                Assert.Same(typeof(string), p.ClrType);
                                Assert.Equal(1, p.GetMaxLength());
                            },
                            p =>
                            {
                                Assert.Equal("VarbinaryColumn", p.Name);
                                Assert.Same(typeof(byte[]), p.ClrType);
                                Assert.Equal(1, p.GetMaxLength());
                            },
                            p =>
                            {
                                Assert.Equal("VarcharColumn", p.Name);
                                Assert.Same(typeof(string), p.ClrType);
                                Assert.Equal(1, p.GetMaxLength());
                            });
                        Assert.Empty(e.GetForeignKeys());
                        Assert.Empty(e.GetSkipNavigations());
                        Assert.Empty(e.GetNavigations());
                    });
            },
            "DROP TABLE OneLengthColumns;");

    [ConditionalFact]
    public void Store_types_without_any_facets()
        => Test(
            @"
CREATE TABLE NoFacetTypes (
    Id int,
    bigintColumn bigint NOT NULL,
    bitColumn bit NOT NULL,
    dateColumn date NOT NULL,
    datetime2Column datetime2 NULL,
    datetimeColumn datetime NULL,
    datetimeoffsetColumn datetimeoffset NULL,
    floatColumn float NOT NULL,
    geographyColumn geography NULL,
    geometryColumn geometry NULL,
    hierarchyidColumn hierarchyid NULL,
    imageColumn image NULL,
    intColumn int NOT NULL,
    moneyColumn money NOT NULL,
    ntextColumn ntext NULL,
    realColumn real NULL,
    smalldatetimeColumn smalldatetime NULL,
    smallintColumn smallint NOT NULL,
    smallmoneyColumn smallmoney NOT NULL,
    sql_variantColumn sql_variant NULL,
    textColumn text NULL,
    timeColumn time NULL,
    timestampColumn timestamp NULL,
    tinyintColumn tinyint NOT NULL,
    uniqueidentifierColumn uniqueidentifier NULL,
    xmlColumn xml NULL,
)

CREATE TABLE RowversionType (
    Id int,
    rowversionColumn rowversion NULL,
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single(t => t.Name == "NoFacetTypes").Columns;

                Assert.Equal("bigint", columns.Single(c => c.Name == "bigintColumn").StoreType);
                Assert.Equal("bit", columns.Single(c => c.Name == "bitColumn").StoreType);
                Assert.Equal("date", columns.Single(c => c.Name == "dateColumn").StoreType);
                Assert.Equal("datetime2", columns.Single(c => c.Name == "datetime2Column").StoreType);
                Assert.Equal("datetime", columns.Single(c => c.Name == "datetimeColumn").StoreType);
                Assert.Equal("datetimeoffset", columns.Single(c => c.Name == "datetimeoffsetColumn").StoreType);
                Assert.Equal("float", columns.Single(c => c.Name == "floatColumn").StoreType);
                Assert.Equal("geography", columns.Single(c => c.Name == "geographyColumn").StoreType);
                Assert.Equal("geometry", columns.Single(c => c.Name == "geometryColumn").StoreType);
                Assert.Equal("hierarchyid", columns.Single(c => c.Name == "hierarchyidColumn").StoreType);
                Assert.Equal("image", columns.Single(c => c.Name == "imageColumn").StoreType);
                Assert.Equal("int", columns.Single(c => c.Name == "intColumn").StoreType);
                Assert.Equal("money", columns.Single(c => c.Name == "moneyColumn").StoreType);
                Assert.Equal("ntext", columns.Single(c => c.Name == "ntextColumn").StoreType);
                Assert.Equal("real", columns.Single(c => c.Name == "realColumn").StoreType);
                Assert.Equal("smalldatetime", columns.Single(c => c.Name == "smalldatetimeColumn").StoreType);
                Assert.Equal("smallint", columns.Single(c => c.Name == "smallintColumn").StoreType);
                Assert.Equal("smallmoney", columns.Single(c => c.Name == "smallmoneyColumn").StoreType);
                Assert.Equal("sql_variant", columns.Single(c => c.Name == "sql_variantColumn").StoreType);
                Assert.Equal("text", columns.Single(c => c.Name == "textColumn").StoreType);
                Assert.Equal("time", columns.Single(c => c.Name == "timeColumn").StoreType);
                Assert.Equal("tinyint", columns.Single(c => c.Name == "tinyintColumn").StoreType);
                Assert.Equal("uniqueidentifier", columns.Single(c => c.Name == "uniqueidentifierColumn").StoreType);
                Assert.Equal("xml", columns.Single(c => c.Name == "xmlColumn").StoreType);

                Assert.Equal(
                    "rowversion",
                    dbModel.Tables.Single(t => t.Name == "RowversionType").Columns.Single(c => c.Name == "rowversionColumn").StoreType);

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Collection(
                    model.GetEntityTypes(),
                    e =>
                    {
                        Assert.Equal("NoFacetType", e.Name);
                        Assert.Collection(
                            e.GetProperties(),
                            p =>
                            {
                                Assert.Equal("BigintColumn", p.Name);
                                Assert.Same(typeof(long), p.ClrType);
                            },
                            p =>
                            {
                                Assert.Equal("BitColumn", p.Name);
                                Assert.Same(typeof(bool), p.ClrType);
                            },
                            p =>
                            {
                                Assert.Equal("DateColumn", p.Name);
                                Assert.Same(typeof(DateOnly), p.ClrType);
                            },
                            p =>
                            {
                                Assert.Equal("Datetime2Column", p.Name);
                                Assert.Same(typeof(DateTime?), p.ClrType);
                            },
                            p =>
                            {
                                Assert.Equal("DatetimeColumn", p.Name);
                                Assert.Same(typeof(DateTime?), p.ClrType);
                            },
                            p =>
                            {
                                Assert.Equal("DatetimeoffsetColumn", p.Name);
                                Assert.Same(typeof(DateTimeOffset?), p.ClrType);
                            },
                            p =>
                            {
                                Assert.Equal("FloatColumn", p.Name);
                                Assert.Same(typeof(double), p.ClrType);
                            },
                            p => Assert.Equal("Id", p.Name),
                            p =>
                            {
                                Assert.Equal("ImageColumn", p.Name);
                                Assert.Same(typeof(byte[]), p.ClrType);
                            },
                            p =>
                            {
                                Assert.Equal("IntColumn", p.Name);
                                Assert.Same(typeof(int), p.ClrType);
                            },
                            p =>
                            {
                                Assert.Equal("MoneyColumn", p.Name);
                                Assert.Same(typeof(decimal), p.ClrType);
                            },
                            p =>
                            {
                                Assert.Equal("NtextColumn", p.Name);
                                Assert.Same(typeof(string), p.ClrType);
                            },
                            p =>
                            {
                                Assert.Equal("RealColumn", p.Name);
                                Assert.Same(typeof(float?), p.ClrType);
                            },
                            p =>
                            {
                                Assert.Equal("SmalldatetimeColumn", p.Name);
                                Assert.Same(typeof(DateTime?), p.ClrType);
                            },
                            p =>
                            {
                                Assert.Equal("SmallintColumn", p.Name);
                                Assert.Same(typeof(short), p.ClrType);
                            },
                            p =>
                            {
                                Assert.Equal("SmallmoneyColumn", p.Name);
                                Assert.Same(typeof(decimal), p.ClrType);
                            },
                            p =>
                            {
                                Assert.Equal("SqlVariantColumn", p.Name);
                                Assert.Same(typeof(object), p.ClrType);
                            },
                            p =>
                            {
                                Assert.Equal("TextColumn", p.Name);
                                Assert.Same(typeof(string), p.ClrType);
                            },
                            p =>
                            {
                                Assert.Equal("TimeColumn", p.Name);
                                Assert.Same(typeof(TimeOnly?), p.ClrType);
                            },
                            p =>
                            {
                                Assert.Equal("TimestampColumn", p.Name);
                                Assert.Same(typeof(byte[]), p.ClrType);
                                Assert.True(p.IsConcurrencyToken);
                            },
                            p =>
                            {
                                Assert.Equal("TinyintColumn", p.Name);
                                Assert.Same(typeof(byte), p.ClrType);
                            },
                            p =>
                            {
                                Assert.Equal("UniqueidentifierColumn", p.Name);
                                Assert.Same(typeof(Guid?), p.ClrType);
                            },
                            p =>
                            {
                                Assert.Equal("XmlColumn", p.Name);
                                Assert.Same(typeof(string), p.ClrType);
                            });
                        Assert.Empty(e.GetForeignKeys());
                        Assert.Empty(e.GetSkipNavigations());
                        Assert.Empty(e.GetNavigations());
                    },
                    e =>
                    {
                        Assert.Equal("RowversionType", e.Name);
                        Assert.Collection(
                            e.GetProperties(),
                            p => Assert.Equal("Id", p.Name),
                            p =>
                            {
                                Assert.Equal("RowversionColumn", p.Name);
                                Assert.Same(typeof(byte[]), p.ClrType);
                                Assert.True(p.IsConcurrencyToken);
                            });
                        Assert.Empty(e.GetForeignKeys());
                        Assert.Empty(e.GetSkipNavigations());
                        Assert.Empty(e.GetNavigations());
                    });
            },
            @"
DROP TABLE NoFacetTypes;
DROP TABLE RowversionType;");

    [ConditionalFact]
    public void Default_and_computed_values_are_stored()
        => Test(
            @"
CREATE TABLE DefaultComputedValues (
    Id int,
    FixedDefaultValue datetime2 NOT NULL DEFAULT ('October 20, 2015 11am'),
    ComputedValue AS GETDATE(),
    A int NOT NULL,
    B int NOT NULL,
    SumOfAAndB AS A + B,
    SumOfAAndBPersisted AS A + B PERSISTED,
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                var fixedDefaultValue = columns.Single(c => c.Name == "FixedDefaultValue");
                Assert.Equal("('October 20, 2015 11am')", fixedDefaultValue.DefaultValueSql);
                Assert.Null(fixedDefaultValue.ComputedColumnSql);

                var computedValue = columns.Single(c => c.Name == "ComputedValue");
                Assert.Null(computedValue.DefaultValueSql);
                Assert.Equal("(getdate())", computedValue.ComputedColumnSql);

                var sumOfAAndB = columns.Single(c => c.Name == "SumOfAAndB");
                Assert.Null(sumOfAAndB.DefaultValueSql);
                Assert.Equal("([A]+[B])", sumOfAAndB.ComputedColumnSql);
                Assert.False(sumOfAAndB.IsStored);

                var sumOfAAndBPersisted = columns.Single(c => c.Name == "SumOfAAndBPersisted");
                Assert.Null(sumOfAAndBPersisted.DefaultValueSql);
                Assert.Equal("([A]+[B])", sumOfAAndBPersisted.ComputedColumnSql);
                Assert.True(sumOfAAndBPersisted.IsStored);

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Collection(
                    model.GetEntityTypes(),
                    e =>
                    {
                        Assert.Equal("DefaultComputedValue", e.Name);
                        Assert.Collection(
                            e.GetProperties(),
                            p =>
                            {
                                Assert.Equal("A", p.Name);
                                Assert.Equal(0, p.GetDefaultValue());
                                Assert.Null(p.GetDefaultValueSql());
                                Assert.Null(p.GetComputedColumnSql());
                            },
                            p =>
                            {
                                Assert.Equal("B", p.Name);
                                Assert.Equal(0, p.GetDefaultValue());
                                Assert.Null(p.GetDefaultValueSql());
                                Assert.Null(p.GetComputedColumnSql());
                            },
                            p =>
                            {
                                Assert.Equal("ComputedValue", p.Name);
                                Assert.Equal(default(DateTime), p.GetDefaultValue());
                                Assert.Null(p.GetDefaultValueSql());
                                Assert.Equal("(getdate())", p.GetComputedColumnSql());
                            },
                            p =>
                            {
                                Assert.Equal("FixedDefaultValue", p.Name);
                                Assert.Equal(new DateTime(2015, 10, 20, 11, 0, 0), p.GetDefaultValue());
                                Assert.Equal("('October 20, 2015 11am')", p.GetDefaultValueSql());
                                Assert.Null(p.GetComputedColumnSql());
                            },
                            p => Assert.Equal("Id", p.Name),
                            p =>
                            {
                                Assert.Equal("SumOfAandB", p.Name);
                                Assert.Null(p.GetDefaultValue());
                                Assert.Null(p.GetDefaultValueSql());
                                Assert.Equal("([A]+[B])", p.GetComputedColumnSql());
                            },
                            p =>
                            {
                                Assert.Equal("SumOfAandBpersisted", p.Name);
                                Assert.Null(p.GetDefaultValue());
                                Assert.Null(p.GetDefaultValueSql());
                                Assert.Equal("([A]+[B])", p.GetComputedColumnSql());
                            });
                        Assert.Empty(e.GetForeignKeys());
                        Assert.Empty(e.GetSkipNavigations());
                        Assert.Empty(e.GetNavigations());
                    });
            },
            "DROP TABLE DefaultComputedValues;");

    [ConditionalFact]
    public void Non_literal_bool_default_values_are_passed_through()
        => Test(
            @"
CREATE TABLE MyTable (
    Id int,
    A bit DEFAULT (CHOOSE(1, 0, 1, 2)),
    B bit DEFAULT ((CONVERT([bit],(CHOOSE(1, 0, 1, 2))))),
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                var column = columns.Single(c => c.Name == "A");
                Assert.Equal("(choose((1),(0),(1),(2)))", column.DefaultValueSql);
                Assert.Null(column.FindAnnotation(RelationalAnnotationNames.DefaultValue));

                column = columns.Single(c => c.Name == "B");
                Assert.Equal("(CONVERT([bit],choose((1),(0),(1),(2))))", column.DefaultValueSql);
                Assert.Null(column.FindAnnotation(RelationalAnnotationNames.DefaultValue));

                var model = scaffoldingFactory.Create(dbModel, new());
                Assert.Equal(1, model.GetEntityTypes().Count());
            },
            "DROP TABLE MyTable;");

    [ConditionalFact]
    public void Simple_int_literals_are_parsed_for_HasDefaultValue()
        => Test(
            @"
CREATE TABLE MyTable (
    Id int,
    A int DEFAULT -1,
    B int DEFAULT 0,
    C int DEFAULT (0),
    D int DEFAULT (-2),
    E int DEFAULT ( 2),
    F int DEFAULT (3 ),
    G int DEFAULT ((4)),
    H int DEFAULT CONVERT([int],(6)),
    I int DEFAULT CONVERT(""int"",(-7)),
    J int DEFAULT ( ( CONVERT([int],((-8))))),
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                var column = columns.Single(c => c.Name == "A");
                Assert.Equal("((-1))", column.DefaultValueSql);
                Assert.Equal(-1, column.DefaultValue);

                column = columns.Single(c => c.Name == "B");
                Assert.Equal("((0))", column.DefaultValueSql);
                Assert.Equal(0, column.DefaultValue);

                column = columns.Single(c => c.Name == "C");
                Assert.Equal("((0))", column.DefaultValueSql);
                Assert.Equal(0, column.DefaultValue);

                column = columns.Single(c => c.Name == "D");
                Assert.Equal("((-2))", column.DefaultValueSql);
                Assert.Equal(-2, column.DefaultValue);

                column = columns.Single(c => c.Name == "E");
                Assert.Equal("((2))", column.DefaultValueSql);
                Assert.Equal(2, column.DefaultValue);

                column = columns.Single(c => c.Name == "F");
                Assert.Equal("((3))", column.DefaultValueSql);
                Assert.Equal(3, column.DefaultValue);

                column = columns.Single(c => c.Name == "G");
                Assert.Equal("((4))", column.DefaultValueSql);
                Assert.Equal(4, column.DefaultValue);

                column = columns.Single(c => c.Name == "H");
                Assert.Equal("(CONVERT([int],(6)))", column.DefaultValueSql);
                Assert.Equal(6, column.DefaultValue);

                column = columns.Single(c => c.Name == "I");
                Assert.Equal("(CONVERT([int],(-7)))", column.DefaultValueSql);
                Assert.Equal(-7, column.DefaultValue);

                column = columns.Single(c => c.Name == "J");
                Assert.Equal("(CONVERT([int],(-8)))", column.DefaultValueSql);
                Assert.Equal(-8, column.DefaultValue);

                var model = scaffoldingFactory.Create(dbModel, new());
                Assert.Equal(1, model.GetEntityTypes().Count());

            },
            "DROP TABLE MyTable;");

    [ConditionalFact]
    public void Simple_short_literals_are_parsed_for_HasDefaultValue()
        => Test(
            @"
CREATE TABLE MyTable (
    Id int,
    A smallint DEFAULT -1,
    B smallint DEFAULT (0),
    C smallint DEFAULT ((CONVERT ( ""smallint"", ( (-7) ) ))),
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                var column = columns.Single(c => c.Name == "A");
                Assert.Equal("((-1))", column.DefaultValueSql);
                Assert.Equal((short)-1, column.DefaultValue);

                column = columns.Single(c => c.Name == "B");
                Assert.Equal("((0))", column.DefaultValueSql);
                Assert.Equal((short)0, column.DefaultValue);

                column = columns.Single(c => c.Name == "C");
                Assert.Equal("(CONVERT([smallint],(-7)))", column.DefaultValueSql);
                Assert.Equal((short)-7, column.DefaultValue);

                var model = scaffoldingFactory.Create(dbModel, new());
                Assert.Equal(1, model.GetEntityTypes().Count());
            },
            "DROP TABLE MyTable;");

    [ConditionalFact]
    public void Simple_long_literals_are_parsed_for_HasDefaultValue()
        => Test(
            @"
CREATE TABLE MyTable (
    Id int,
    A bigint DEFAULT -1,
    B bigint DEFAULT (0),
    C bigint DEFAULT ((CONVERT ( ""bigint"", ( (-7) ) ))),
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                var column = columns.Single(c => c.Name == "A");
                Assert.Equal("((-1))", column.DefaultValueSql);
                Assert.Equal((long)-1, column.DefaultValue);

                column = columns.Single(c => c.Name == "B");
                Assert.Equal("((0))", column.DefaultValueSql);
                Assert.Equal((long)0, column.DefaultValue);

                column = columns.Single(c => c.Name == "C");
                Assert.Equal("(CONVERT([bigint],(-7)))", column.DefaultValueSql);
                Assert.Equal((long)-7, column.DefaultValue);

                var model = scaffoldingFactory.Create(dbModel, new());
                Assert.Equal(1, model.GetEntityTypes().Count());

            },
            "DROP TABLE MyTable;");

    [ConditionalFact]
    public void Simple_byte_literals_are_parsed_for_HasDefaultValue()
        => Test(
            @"
CREATE TABLE MyTable (
    Id int,
    A tinyint DEFAULT 1,
    B tinyint DEFAULT (0),
    C tinyint DEFAULT ((CONVERT ( ""tinyint"", ( (7) ) ))),
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                var column = columns.Single(c => c.Name == "A");
                Assert.Equal("((1))", column.DefaultValueSql);
                Assert.Equal((byte)1, column.DefaultValue);

                column = columns.Single(c => c.Name == "B");
                Assert.Equal("((0))", column.DefaultValueSql);
                Assert.Equal((byte)0, column.DefaultValue);

                column = columns.Single(c => c.Name == "C");
                Assert.Equal("(CONVERT([tinyint],(7)))", column.DefaultValueSql);
                Assert.Equal((byte)7, column.DefaultValue);

                var model = scaffoldingFactory.Create(dbModel, new());
                Assert.Equal(1, model.GetEntityTypes().Count());
            },
            "DROP TABLE MyTable;");

    [ConditionalFact]
    public void Non_literal_int_default_values_are_passed_through()
        => Test(
            @"
CREATE TABLE MyTable (
    Id int,
    A int DEFAULT (CHOOSE(1, 0, 1, 2)),
    B int DEFAULT ((CONVERT([int],(CHOOSE(1, 0, 1, 2))))),
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                var column = columns.Single(c => c.Name == "A");
                Assert.Equal("(choose((1),(0),(1),(2)))", column.DefaultValueSql);
                Assert.Null(column.FindAnnotation(RelationalAnnotationNames.DefaultValue));

                column = columns.Single(c => c.Name == "B");
                Assert.Equal("(CONVERT([int],choose((1),(0),(1),(2))))", column.DefaultValueSql);
                Assert.Null(column.FindAnnotation(RelationalAnnotationNames.DefaultValue));

                var model = scaffoldingFactory.Create(dbModel, new());
                Assert.Equal(1, model.GetEntityTypes().Count());
            },
            "DROP TABLE MyTable;");

    [ConditionalFact]
    public void Simple_double_literals_are_parsed_for_HasDefaultValue()
        => Test(
            @"
CREATE TABLE MyTable (
    Id int,
    A float DEFAULT -1.1111,
    B float DEFAULT (0.0),
    C float DEFAULT (1.1000000000000001e+000),
    D float DEFAULT ((CONVERT ( ""float"", ( (1.1234) ) ))),
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                var column = columns.Single(c => c.Name == "A");
                Assert.Equal("((-1.1111))", column.DefaultValueSql);
                Assert.Equal(-1.1111, column.DefaultValue);

                column = columns.Single(c => c.Name == "B");
                Assert.Equal("((0.0))", column.DefaultValueSql);
                Assert.Equal((double)0, column.DefaultValue);

                column = columns.Single(c => c.Name == "C");
                Assert.Equal("((1.1000000000000001e+000))", column.DefaultValueSql);
                Assert.Equal(1.1000000000000001e+000, column.DefaultValue);

                column = columns.Single(c => c.Name == "D");
                Assert.Equal("(CONVERT([float],(1.1234)))", column.DefaultValueSql);
                Assert.Equal(1.1234, column.DefaultValue);

                var model = scaffoldingFactory.Create(dbModel, new());
                Assert.Equal(1, model.GetEntityTypes().Count());
            },
            "DROP TABLE MyTable;");

    [ConditionalFact]
    public void Simple_float_literals_are_parsed_for_HasDefaultValue()
        => Test(
            @"
CREATE TABLE MyTable (
    Id int,
    A real DEFAULT -1.1111,
    B real DEFAULT (0.0),
    C real DEFAULT (1.1000000000000001e+000),
    D real DEFAULT ((CONVERT ( ""real"", ( (1.1234) ) ))),
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                var column = columns.Single(c => c.Name == "A");
                Assert.Equal("((-1.1111))", column.DefaultValueSql);
                Assert.Equal((float)-1.1111, column.DefaultValue);

                column = columns.Single(c => c.Name == "B");
                Assert.Equal("((0.0))", column.DefaultValueSql);
                Assert.Equal((float)0, column.DefaultValue);

                column = columns.Single(c => c.Name == "C");
                Assert.Equal("((1.1000000000000001e+000))", column.DefaultValueSql);
                Assert.Equal((float)1.1000000000000001e+000, column.DefaultValue);

                column = columns.Single(c => c.Name == "D");
                Assert.Equal("(CONVERT([real],(1.1234)))", column.DefaultValueSql);
                Assert.Equal((float)1.1234, column.DefaultValue);

                var model = scaffoldingFactory.Create(dbModel, new());
                Assert.Equal(1, model.GetEntityTypes().Count());
            },
            "DROP TABLE MyTable;");

    [ConditionalFact]
    public void Simple_decimal_literals_are_parsed_for_HasDefaultValue()
        => Test(
            @"
CREATE TABLE MyTable (
    Id int,
    A decimal DEFAULT -1.1111,
    B decimal DEFAULT (0.0),
    C decimal DEFAULT (0),
    D decimal DEFAULT ((CONVERT ( ""decimal"", ( (1.1234) ) ))),
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                var column = columns.Single(c => c.Name == "A");
                Assert.Equal("((-1.1111))", column.DefaultValueSql);
                Assert.Equal((decimal)-1.1111, column.DefaultValue);

                column = columns.Single(c => c.Name == "B");
                Assert.Equal("((0.0))", column.DefaultValueSql);
                Assert.Equal((decimal)0, column.DefaultValue);

                column = columns.Single(c => c.Name == "C");
                Assert.Equal("((0))", column.DefaultValueSql);
                Assert.Equal((decimal)0, column.DefaultValue);

                column = columns.Single(c => c.Name == "D");
                Assert.Equal("(CONVERT([decimal],(1.1234)))", column.DefaultValueSql);
                Assert.Equal((decimal)1.1234, column.DefaultValue);

                var model = scaffoldingFactory.Create(dbModel, new());
                Assert.Equal(1, model.GetEntityTypes().Count());
            },
            "DROP TABLE MyTable;");

    [ConditionalFact]
    public void Simple_bool_literals_are_parsed_for_HasDefaultValue()
        => Test(
            @"
CREATE TABLE MyTable (
    Id int,
    A bit DEFAULT 0,
    B bit DEFAULT 1,
    C bit DEFAULT (0),
    D bit DEFAULT (1),
    E bit DEFAULT ('FaLse'),
    F bit DEFAULT ('tRuE'),
    G bit DEFAULT ((CONVERT ( ""bit"", ( ('tRUE') ) ))),
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                var column = columns.Single(c => c.Name == "A");
                Assert.Equal("((0))", column.DefaultValueSql);
                Assert.Equal(false, column.DefaultValue);

                column = columns.Single(c => c.Name == "B");
                Assert.Equal("((1))", column.DefaultValueSql);
                Assert.Equal(true, column.DefaultValue);

                column = columns.Single(c => c.Name == "C");
                Assert.Equal("((0))", column.DefaultValueSql);
                Assert.Equal(false, column.DefaultValue);

                column = columns.Single(c => c.Name == "D");
                Assert.Equal("((1))", column.DefaultValueSql);
                Assert.Equal(true, column.DefaultValue);

                column = columns.Single(c => c.Name == "E");
                Assert.Equal("('FaLse')", column.DefaultValueSql);
                Assert.Equal(false, column.DefaultValue);

                column = columns.Single(c => c.Name == "F");
                Assert.Equal("('tRuE')", column.DefaultValueSql);
                Assert.Equal(true, column.DefaultValue);

                column = columns.Single(c => c.Name == "G");
                Assert.Equal("(CONVERT([bit],'tRUE'))", column.DefaultValueSql);
                Assert.Equal(true, column.DefaultValue);

                var model = scaffoldingFactory.Create(dbModel, new());
                Assert.Equal(1, model.GetEntityTypes().Count());
            },
            "DROP TABLE MyTable;");

    [ConditionalFact]
    public void Simple_DateTime_literals_are_parsed_for_HasDefaultValue()
        => Test(
            @"
CREATE TABLE MyTable (
    Id int,
    A datetime DEFAULT '1973-09-03T12:00:01.0020000',
    B datetime2 DEFAULT ('1968-10-23'),
    C datetime2 DEFAULT (CONVERT ([datetime2],('1973-09-03T01:02:03'))),
    D datetime DEFAULT (CONVERT(datetime,'12:12:12')),
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                var column = columns.Single(c => c.Name == "A");
                Assert.Equal("('1973-09-03T12:00:01.0020000')", column.DefaultValueSql);
                Assert.Equal(new DateTime(1973, 9, 3, 12, 0, 1, 2, DateTimeKind.Unspecified), column.DefaultValue);

                column = columns.Single(c => c.Name == "B");
                Assert.Equal("('1968-10-23')", column.DefaultValueSql);
                Assert.Equal(new DateTime(1968, 10, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), column.DefaultValue);

                column = columns.Single(c => c.Name == "C");
                Assert.Equal("(CONVERT([datetime2],'1973-09-03T01:02:03'))", column.DefaultValueSql);
                Assert.Equal(new DateTime(1973, 9, 3, 1, 2, 3, 0, DateTimeKind.Unspecified), column.DefaultValue);

                column = columns.Single(c => c.Name == "D");
                Assert.Equal("(CONVERT([datetime],'12:12:12'))", column.DefaultValueSql);
                Assert.Equal(12, ((DateTime)column.DefaultValue!).Hour);
                Assert.Equal(12, ((DateTime)column.DefaultValue!).Minute);
                Assert.Equal(12, ((DateTime)column.DefaultValue!).Second);

                var model = scaffoldingFactory.Create(dbModel, new());
                Assert.Equal(1, model.GetEntityTypes().Count());
            },
            "DROP TABLE MyTable;");

    [ConditionalFact]
    public void Non_literal_or_non_parsable_DateTime_default_values_are_passed_through()
        => Test(
            @"
CREATE TABLE MyTable (
    Id int,
    A datetime2 DEFAULT (CONVERT([datetime2],(getdate()))),
    B datetime DEFAULT getdate(),
    C datetime2 DEFAULT ((CONVERT([datetime2],('12-01-16 12:32')))),
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                var column = columns.Single(c => c.Name == "A");
                Assert.Equal("(CONVERT([datetime2],getdate()))", column.DefaultValueSql);
                Assert.Null(column.FindAnnotation(RelationalAnnotationNames.DefaultValue));

                column = columns.Single(c => c.Name == "B");
                Assert.Equal("(getdate())", column.DefaultValueSql);
                Assert.Null(column.FindAnnotation(RelationalAnnotationNames.DefaultValue));

                column = columns.Single(c => c.Name == "C");
                Assert.Equal("(CONVERT([datetime2],'12-01-16 12:32'))", column.DefaultValueSql);
                Assert.Null(column.FindAnnotation(RelationalAnnotationNames.DefaultValue));

                var model = scaffoldingFactory.Create(dbModel, new());
                Assert.Equal(1, model.GetEntityTypes().Count());
            },
            "DROP TABLE MyTable;");

    [ConditionalFact]
    public void Simple_DateOnly_literals_are_parsed_for_HasDefaultValue()
        => Test(
            @"
CREATE TABLE MyTable (
    Id int,
    A date DEFAULT ('1968-10-23'),
    B date DEFAULT (CONVERT([date],('1973-09-03T01:02:03'))),
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                var column = columns.Single(c => c.Name == "A");
                Assert.Equal("('1968-10-23')", column.DefaultValueSql);
                Assert.Equal(new DateOnly(1968, 10, 23), column.DefaultValue);

                column = columns.Single(c => c.Name == "B");
                Assert.Equal("(CONVERT([date],'1973-09-03T01:02:03'))", column.DefaultValueSql);
                Assert.Equal(new DateOnly(1973, 9, 3), column.DefaultValue);

                var model = scaffoldingFactory.Create(dbModel, new());
                Assert.Equal(1, model.GetEntityTypes().Count());
            },
            "DROP TABLE MyTable;");

    [ConditionalFact]
    public void Simple_TimeOnly_literals_are_parsed_for_HasDefaultValue()
        => Test(
            @"
CREATE TABLE MyTable (
    Id int,
    A time DEFAULT ('12:00:01.0020000'),
    B time DEFAULT (CONVERT([time],('1973-09-03T01:02:03'))),
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                var column = columns.Single(c => c.Name == "A");
                Assert.Equal("('12:00:01.0020000')", column.DefaultValueSql);
                Assert.Equal(new TimeOnly(12, 0, 1, 2), column.DefaultValue);

                column = columns.Single(c => c.Name == "B");
                Assert.Equal("(CONVERT([time],'1973-09-03T01:02:03'))", column.DefaultValueSql);
                Assert.Equal(new TimeOnly(1, 2, 3), column.DefaultValue);

                var model = scaffoldingFactory.Create(dbModel, new());
                Assert.Equal(1, model.GetEntityTypes().Count());
            },
            "DROP TABLE MyTable;");

    [ConditionalFact]
    public void Simple_DateTimeOffset_literals_are_parsed_for_HasDefaultValue()
        => Test(
            @"
CREATE TABLE MyTable (
    Id int,
    A datetimeoffset DEFAULT ('1973-09-03T12:00:01.0000000+10:00'),
    B datetimeoffset DEFAULT (CONVERT([datetimeoffset],('1973-09-03T01:02:03'))),
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                var column = columns.Single(c => c.Name == "A");
                Assert.Equal("('1973-09-03T12:00:01.0000000+10:00')", column.DefaultValueSql);
                Assert.Equal(
                    new DateTimeOffset(new DateTime(1973, 9, 3, 12, 0, 1, 0, DateTimeKind.Unspecified), new TimeSpan(0, 10, 0, 0, 0)),
                    column.DefaultValue);

                column = columns.Single(c => c.Name == "B");
                Assert.Equal("(CONVERT([datetimeoffset],'1973-09-03T01:02:03'))", column.DefaultValueSql);
                Assert.Equal(
                    new DateTime(1973, 9, 3, 1, 2, 3, 0, DateTimeKind.Unspecified),
                    ((DateTimeOffset)column.DefaultValue!).DateTime);

                var model = scaffoldingFactory.Create(dbModel, new());
                Assert.Equal(1, model.GetEntityTypes().Count());
            },
            "DROP TABLE MyTable;");

    [ConditionalFact]
    public void Simple_Guid_literals_are_parsed_for_HasDefaultValue()
        => Test(
            @"
CREATE TABLE MyTable (
    Id int,
    A uniqueidentifier DEFAULT ('0E984725-C51C-4BF4-9960-E1C80E27ABA0'),
    B uniqueidentifier DEFAULT (CONVERT([uniqueidentifier],('0E984725-C51C-4BF4-9960-E1C80E27ABA0'))),
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                var column = columns.Single(c => c.Name == "A");
                Assert.Equal("('0E984725-C51C-4BF4-9960-E1C80E27ABA0')", column.DefaultValueSql);
                Assert.Equal(new Guid("0E984725-C51C-4BF4-9960-E1C80E27ABA0"), column.DefaultValue);

                column = columns.Single(c => c.Name == "B");
                Assert.Equal("(CONVERT([uniqueidentifier],'0E984725-C51C-4BF4-9960-E1C80E27ABA0'))", column.DefaultValueSql);
                Assert.Equal(new Guid("0E984725-C51C-4BF4-9960-E1C80E27ABA0"), column.DefaultValue);

                var model = scaffoldingFactory.Create(dbModel, new());
                Assert.Equal(1, model.GetEntityTypes().Count());
            },
            "DROP TABLE MyTable;");

    [ConditionalFact]
    public void Non_literal_Guid_default_values_are_passed_through()
        => Test(
            @"
CREATE TABLE MyTable (
    Id int,
    A uniqueidentifier DEFAULT (CONVERT([uniqueidentifier],(newid()))),
    B uniqueidentifier DEFAULT NEWSEQUENTIALID(),
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                var column = columns.Single(c => c.Name == "A");
                Assert.Equal("(CONVERT([uniqueidentifier],newid()))", column.DefaultValueSql);
                Assert.Null(column.FindAnnotation(RelationalAnnotationNames.DefaultValue));

                column = columns.Single(c => c.Name == "B");
                Assert.Equal("(newsequentialid())", column.DefaultValueSql);
                Assert.Null(column.FindAnnotation(RelationalAnnotationNames.DefaultValue));

                var model = scaffoldingFactory.Create(dbModel, new());
                Assert.Equal(1, model.GetEntityTypes().Count());
            },
            "DROP TABLE MyTable;");

    [ConditionalFact]
    public void Simple_string_literals_are_parsed_for_HasDefaultValue()
        => Test(
            @"
CREATE TABLE MyTable (
    Id int,
    A nvarchar(max) DEFAULT 'Hot',
    B varchar(max) DEFAULT ('Buttered'),
    C character(100) DEFAULT (''),
    D text DEFAULT (N''),
    E nvarchar(100) DEFAULT  ( N' Toast! ') ,
    F nvarchar(20) DEFAULT  (CONVERT([nvarchar](20),('Scones'))) ,
    G varchar(max) DEFAULT (CONVERT(character varying(max),('Toasted teacakes'))),
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                var column = columns.Single(c => c.Name == "A");
                Assert.Equal("('Hot')", column.DefaultValueSql);
                Assert.Equal("Hot", column.DefaultValue);

                column = columns.Single(c => c.Name == "B");
                Assert.Equal("('Buttered')", column.DefaultValueSql);
                Assert.Equal("Buttered", column.DefaultValue);

                column = columns.Single(c => c.Name == "C");
                Assert.Equal("('')", column.DefaultValueSql);
                Assert.Equal("", column.DefaultValue);

                column = columns.Single(c => c.Name == "D");
                Assert.Equal("(N'')", column.DefaultValueSql);
                Assert.Equal("", column.DefaultValue);

                column = columns.Single(c => c.Name == "E");
                Assert.Equal("(N' Toast! ')", column.DefaultValueSql);
                Assert.Equal(" Toast! ", column.DefaultValue);

                column = columns.Single(c => c.Name == "F");
                Assert.Equal("(CONVERT([nvarchar](20),'Scones'))", column.DefaultValueSql);
                Assert.Equal("Scones", column.DefaultValue);

                column = columns.Single(c => c.Name == "G");
                Assert.Equal("(CONVERT([varchar](max),'Toasted teacakes'))", column.DefaultValueSql);
                Assert.Equal("Toasted teacakes", column.DefaultValue);

                var model = scaffoldingFactory.Create(dbModel, new());
                Assert.Equal(1, model.GetEntityTypes().Count());
            },
            "DROP TABLE MyTable;");

    [ConditionalFact]
    public void ValueGenerated_is_set_for_identity_and_computed_column()
        => Test(
            @"
CREATE TABLE ValueGeneratedProperties (
    Id int IDENTITY(1, 1),
    NoValueGenerationColumn nvarchar(max),
    FixedDefaultValue datetime2 NOT NULL DEFAULT ('October 20, 2015 11am'),
    ComputedValue AS GETDATE(),
    rowversionColumn rowversion NULL,
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                Assert.Equal(ValueGenerated.OnAdd, columns.Single(c => c.Name == "Id").ValueGenerated);
                Assert.Null(columns.Single(c => c.Name == "NoValueGenerationColumn").ValueGenerated);
                Assert.Null(columns.Single(c => c.Name == "FixedDefaultValue").ValueGenerated);
                Assert.Null(columns.Single(c => c.Name == "ComputedValue").ValueGenerated);
                Assert.Equal(ValueGenerated.OnAddOrUpdate, columns.Single(c => c.Name == "rowversionColumn").ValueGenerated);

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Collection(
                    model.GetEntityTypes(),
                    e =>
                    {
                        Assert.Equal("dbo", e.GetSchema());
                        Assert.Equal("ValueGeneratedProperty", e.Name);
                        Assert.Empty(e.GetKeys());
                        Assert.Collection(
                            e.GetProperties(),
                            p =>
                            {
                                Assert.Equal("ComputedValue", p.Name);
                                Assert.Equal(ValueGenerated.Never, p.ValueGenerated);
                            },
                            p =>
                            {
                                Assert.Equal("FixedDefaultValue", p.Name);
                                Assert.Equal(ValueGenerated.Never, p.ValueGenerated);
                            },
                            p =>
                            {
                                Assert.Equal("Id", p.Name);
                                Assert.Equal(ValueGenerated.OnAdd, p.ValueGenerated);
                            },
                            p =>
                            {
                                Assert.Equal("NoValueGenerationColumn", p.Name);
                                Assert.Equal(ValueGenerated.Never, p.ValueGenerated);
                            },
                            p =>
                            {
                                Assert.Equal("RowversionColumn", p.Name);
                                Assert.Equal(ValueGenerated.OnAddOrUpdate, p.ValueGenerated);
                                Assert.True(p.IsConcurrencyToken);
                            });
                        Assert.Empty(e.GetForeignKeys());
                        Assert.Empty(e.GetNavigations());
                        Assert.Empty(e.GetSkipNavigations());
                    });
            },
            "DROP TABLE ValueGeneratedProperties;");

    [ConditionalFact]
    public void ConcurrencyToken_is_set_for_rowVersion()
        => Test(
            @"
CREATE TABLE RowVersionTable (
    Id int,
    rowversionColumn rowversion,
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                Assert.True((bool)columns.Single(c => c.Name == "rowversionColumn")[ScaffoldingAnnotationNames.ConcurrencyToken]!);

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Collection(
                    model.GetEntityTypes(),
                    e =>
                    {
                        Assert.Equal("dbo", e.GetSchema());
                        Assert.Equal("RowVersionTable", e.Name);
                        Assert.Empty(e.GetKeys());
                        Assert.Collection(
                            e.GetProperties(),
                            p =>
                            {
                                Assert.Equal("Id", p.Name);
                                Assert.False(p.IsConcurrencyToken);
                            },
                            p =>
                            {
                                Assert.Equal("RowversionColumn", p.Name);
                                Assert.True(p.IsConcurrencyToken);
                            });
                        Assert.Empty(e.GetForeignKeys());
                        Assert.Empty(e.GetNavigations());
                        Assert.Empty(e.GetSkipNavigations());
                    });

            },
            "DROP TABLE RowVersionTable;");

    [ConditionalFact]
    public void Column_nullability_is_set()
        => Test(
            @"
CREATE TABLE NullableColumns (
    Id int,
    NullableInt int NULL,
    NonNullString nvarchar(max) NOT NULL,
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                Assert.True(columns.Single(c => c.Name == "NullableInt").IsNullable);
                Assert.False(columns.Single(c => c.Name == "NonNullString").IsNullable);

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Collection(
                    model.GetEntityTypes(),
                    e =>
                    {
                        Assert.Equal("dbo", e.GetSchema());
                        Assert.Equal("NullableColumn", e.Name);
                        Assert.Empty(e.GetKeys());
                        Assert.Collection(
                            e.GetProperties(),
                            p =>
                            {
                                Assert.Equal("Id", p.Name);
                                Assert.True(p.IsNullable);
                            },
                            p =>
                            {
                                Assert.Equal("NonNullString", p.Name);
                                Assert.False(p.IsNullable);
                            },
                            p =>
                            {
                                Assert.Equal("NullableInt", p.Name);
                                Assert.True(p.IsNullable);
                            });
                        Assert.Empty(e.GetForeignKeys());
                        Assert.Empty(e.GetNavigations());
                        Assert.Empty(e.GetSkipNavigations());
                    });
            },
            "DROP TABLE NullableColumns;");

    [ConditionalFact]
    public void Column_collation_is_set()
        => Test(
            @"
CREATE TABLE ColumnsWithCollation (
    Id int,
    DefaultCollation nvarchar(max),
    NonDefaultCollation nvarchar(max) COLLATE German_PhoneBook_CI_AS,
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                Assert.Null(columns.Single(c => c.Name == "DefaultCollation").Collation);
                Assert.Equal("German_PhoneBook_CI_AS", columns.Single(c => c.Name == "NonDefaultCollation").Collation);

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Collection(
                    model.GetEntityTypes(),
                    e =>
                    {
                        Assert.Equal("dbo", e.GetSchema());
                        Assert.Equal("ColumnsWithCollation", e.Name);
                        Assert.Empty(e.GetKeys());
                        Assert.Collection(
                            e.GetProperties(),
                            p =>
                            {
                                Assert.Equal("DefaultCollation", p.Name);
                                Assert.Null(p.GetCollation());
                            },
                            p =>
                            {
                                Assert.Equal("Id", p.Name);
                                Assert.Null(p.GetCollation());
                            },
                            p =>
                            {
                                Assert.Equal("NonDefaultCollation", p.Name);
                                Assert.Equal("German_PhoneBook_CI_AS", p.GetCollation());
                            });
                        Assert.Empty(e.GetForeignKeys());
                        Assert.Empty(e.GetNavigations());
                        Assert.Empty(e.GetSkipNavigations());
                    });
            },
            "DROP TABLE ColumnsWithCollation;");

    [ConditionalFact]
    public void Column_sparseness_is_set()
        => Test(
            @"
CREATE TABLE ColumnsWithSparseness (
    Id int,
    Sparse nvarchar(max) SPARSE NULL,
    NonSparse nvarchar(max) NULL
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                Assert.True((bool)columns.Single(c => c.Name == "Sparse")[SqlServerAnnotationNames.Sparse]!);
                Assert.Null(columns.Single(c => c.Name == "NonSparse")[SqlServerAnnotationNames.Sparse]);

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Collection(
                    model.GetEntityTypes(),
                    e =>
                    {
                        Assert.Equal("dbo", e.GetSchema());
                        Assert.Equal("ColumnsWithSparseness", e.Name);
                        Assert.Empty(e.GetKeys());
                        Assert.Collection(
                            e.GetProperties(),
                            p =>
                            {
                                Assert.Equal("Id", p.Name);
                                Assert.Null(p.IsSparse());
                            },
                            p =>
                            {
                                Assert.Equal("NonSparse", p.Name);
                                Assert.Null(p.IsSparse());
                            },
                            p =>
                            {
                                Assert.Equal("Sparse", p.Name);
                                Assert.True(p.IsSparse());
                            });
                        Assert.Empty(e.GetForeignKeys());
                        Assert.Empty(e.GetNavigations());
                        Assert.Empty(e.GetSkipNavigations());
                    });
            },
            "DROP TABLE ColumnsWithSparseness;");

    [ConditionalFact]
    [SqlServerCondition(SqlServerCondition.SupportsHiddenColumns)]
    public void Hidden_period_columns_are_not_created()
        => Test(
            @"
CREATE TABLE dbo.HiddenColumnsTable
(
     Id int NOT NULL PRIMARY KEY CLUSTERED,
     Name varchar(50) NOT NULL,
     SysStartTime datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
     SysEndTime datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
     PERIOD FOR SYSTEM_TIME(SysStartTime, SysEndTime)
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.HiddenColumnsTableHistory));
CREATE INDEX IX_HiddenColumnsTable_1 ON dbo.HiddenColumnsTable ( Name, SysStartTime);
CREATE INDEX IX_HiddenColumnsTable_2 ON dbo.HiddenColumnsTable ( SysStartTime);
CREATE INDEX IX_HiddenColumnsTable_3 ON dbo.HiddenColumnsTable ( Name );
",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                Assert.Equal(2, columns.Count);
                Assert.DoesNotContain(columns, c => c.Name == "SysStartTime");
                Assert.DoesNotContain(columns, c => c.Name == "SysEndTime");
                Assert.Equal("IX_HiddenColumnsTable_3", dbModel.Tables.Single().Indexes.Single().Name);

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Collection(
                    model.GetEntityTypes(),
                    e =>
                    {
                        Assert.Equal("dbo", e.GetSchema());
                        Assert.Equal("HiddenColumnsTable", e.Name);
                        Assert.Collection(e.GetKeys(), k => Assert.Equal("Id", k.Properties.Single().Name));
                        Assert.Collection(
                            e.GetProperties(),
                            p =>
                            {
                                Assert.Equal("Id", p.Name);
                                Assert.Equal(ValueGenerated.Never, p.ValueGenerated);
                                Assert.False(p.IsNullable);
                            },
                            p =>
                            {
                                Assert.Equal("Name", p.Name);
                                Assert.Equal(50, p.GetMaxLength());
                                Assert.Equal(ValueGenerated.Never, p.ValueGenerated);
                                Assert.False(p.IsNullable);
                            });
                        Assert.Empty(e.GetForeignKeys());
                        Assert.Collection(e.GetIndexes(), i => Assert.Collection(i.Properties, p => Assert.Equal("Name", p.Name)));
                        Assert.Empty(e.GetNavigations());
                        Assert.Empty(e.GetSkipNavigations());
                    });
            },
            @"
ALTER TABLE dbo.HiddenColumnsTable SET (SYSTEM_VERSIONING = OFF);
DROP TABLE dbo.HiddenColumnsTableHistory;
DROP TABLE dbo.HiddenColumnsTable;
");

    [ConditionalFact]
    [SqlServerCondition(SqlServerCondition.SupportsHiddenColumns)]
    public void Period_columns_are_not_created()
        => Test(
            @"
CREATE TABLE dbo.HiddenColumnsTable
(
     Id int NOT NULL PRIMARY KEY CLUSTERED,
     Name varchar(50) NOT NULL,
     SysStartTime datetime2 GENERATED ALWAYS AS ROW START NOT NULL,
     SysEndTime datetime2 GENERATED ALWAYS AS ROW END NOT NULL,
     PERIOD FOR SYSTEM_TIME(SysStartTime, SysEndTime)
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.HiddenColumnsTableHistory));
CREATE INDEX IX_HiddenColumnsTable_1 ON dbo.HiddenColumnsTable ( Name, SysStartTime);
CREATE INDEX IX_HiddenColumnsTable_2 ON dbo.HiddenColumnsTable ( SysStartTime);
CREATE INDEX IX_HiddenColumnsTable_3 ON dbo.HiddenColumnsTable ( Name );
",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                Assert.Equal(2, columns.Count);
                Assert.DoesNotContain(columns, c => c.Name == "SysStartTime");
                Assert.DoesNotContain(columns, c => c.Name == "SysEndTime");
                Assert.Equal("IX_HiddenColumnsTable_3", dbModel.Tables.Single().Indexes.Single().Name);

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Collection(
                    model.GetEntityTypes(),
                    e =>
                    {
                        Assert.Equal("dbo", e.GetSchema());
                        Assert.Equal("HiddenColumnsTable", e.Name);
                        Assert.Collection(e.GetKeys(), k => Assert.Equal("Id", k.Properties.Single().Name));
                        Assert.Collection(
                            e.GetProperties(),
                            p =>
                            {
                                Assert.Equal("Id", p.Name);
                                Assert.Equal(ValueGenerated.Never, p.ValueGenerated);
                                Assert.False(p.IsNullable);
                            },
                            p =>
                            {
                                Assert.Equal("Name", p.Name);
                                Assert.Equal(50, p.GetMaxLength());
                                Assert.Equal(ValueGenerated.Never, p.ValueGenerated);
                                Assert.False(p.IsNullable);
                            });
                        Assert.Empty(e.GetForeignKeys());
                        Assert.Collection(e.GetIndexes(), i => Assert.Collection(i.Properties, p => Assert.Equal("Name", p.Name)));
                        Assert.Empty(e.GetNavigations());
                        Assert.Empty(e.GetSkipNavigations());
                    });
            },
            @"
ALTER TABLE dbo.HiddenColumnsTable SET (SYSTEM_VERSIONING = OFF);
DROP TABLE dbo.HiddenColumnsTableHistory;
DROP TABLE dbo.HiddenColumnsTable;
");

    #endregion

    #region PrimaryKeyFacets

    [ConditionalFact]
    public void Create_composite_primary_key()
        => Test(
            @"
CREATE TABLE CompositePrimaryKeyTable (
    Id1 int,
    Id2 int,
    PRIMARY KEY (Id2, Id1)
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var pk = dbModel.Tables.Single().PrimaryKey;

                Assert.Equal("dbo", pk!.Table!.Schema);
                Assert.Equal("CompositePrimaryKeyTable", pk.Table.Name);
                Assert.StartsWith("PK__Composit", pk.Name);
                Assert.Equal(["Id2", "Id1"], pk.Columns.Select(ic => ic.Name).ToList());

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Collection(
                    model.GetEntityTypes(),
                    e =>
                    {
                        Assert.Equal("dbo", e.GetSchema());
                        Assert.Equal("CompositePrimaryKeyTable", e.Name);
                        Assert.Collection(e.GetKeys(), k => Assert.Collection(k.Properties,
                            p => Assert.Equal("Id2", p.Name),
                            p => Assert.Equal("Id1", p.Name)));
                        Assert.Collection(
                            e.GetProperties(),
                            p =>
                            {
                                Assert.Equal("Id2", p.Name);
                                Assert.Equal(ValueGenerated.Never, p.ValueGenerated);
                                Assert.False(p.IsNullable);
                            },
                            p =>
                            {
                                Assert.Equal("Id1", p.Name);
                                Assert.Equal(ValueGenerated.Never, p.ValueGenerated);
                                Assert.False(p.IsNullable);
                            });
                        Assert.Empty(e.GetForeignKeys());
                        Assert.Empty(e.GetIndexes());
                        Assert.Empty(e.GetNavigations());
                        Assert.Empty(e.GetSkipNavigations());
                    });
            },
            "DROP TABLE CompositePrimaryKeyTable;");

    [ConditionalFact]
    public void Set_clustered_false_for_non_clustered_primary_key()
        => Test(
            @"
CREATE TABLE NonClusteredPrimaryKeyTable (
    Id1 int PRIMARY KEY NONCLUSTERED,
    Id2 int,
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var pk = dbModel.Tables.Single().PrimaryKey;

                Assert.Equal("dbo", pk!.Table!.Schema);
                Assert.Equal("NonClusteredPrimaryKeyTable", pk.Table.Name);
                Assert.StartsWith("PK__NonClust", pk.Name);
                Assert.False((bool)pk[SqlServerAnnotationNames.Clustered]!);
                Assert.Equal(["Id1"], pk.Columns.Select(ic => ic.Name).ToList());

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Collection(
                    model.GetEntityTypes(),
                    e =>
                    {
                        Assert.Equal("dbo", e.GetSchema());
                        Assert.Equal("NonClusteredPrimaryKeyTable", e.Name);
                        Assert.Collection(e.GetKeys(), k =>
                        {
                            Assert.False(k.IsClustered());
                            Assert.Collection(k.Properties, p => Assert.Equal("Id1", p.Name));
                        });
                        Assert.Collection(e.GetProperties(), p => Assert.Equal("Id1", p.Name), p => Assert.Equal("Id2", p.Name));
                        Assert.Empty(e.GetForeignKeys());
                        Assert.Empty(e.GetIndexes());
                        Assert.Empty(e.GetNavigations());
                        Assert.Empty(e.GetSkipNavigations());
                    });
            },
            "DROP TABLE NonClusteredPrimaryKeyTable;");

    [ConditionalFact]
    public void Set_clustered_false_for_primary_key_if_different_clustered_index()
        => Test(
            @"
CREATE TABLE NonClusteredPrimaryKeyTableWithClusteredIndex (
    Id1 int PRIMARY KEY NONCLUSTERED,
    Id2 int,
);

CREATE CLUSTERED INDEX ClusteredIndex ON NonClusteredPrimaryKeyTableWithClusteredIndex( Id2 );",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var pk = dbModel.Tables.Single().PrimaryKey;

                Assert.Equal("dbo", pk!.Table!.Schema);
                Assert.Equal("NonClusteredPrimaryKeyTableWithClusteredIndex", pk.Table.Name);
                Assert.StartsWith("PK__NonClust", pk.Name);
                Assert.False((bool)pk[SqlServerAnnotationNames.Clustered]!);
                Assert.Equal(["Id1"], pk.Columns.Select(ic => ic.Name).ToList());

                var model = scaffoldingFactory.Create(dbModel, new());
                Assert.Equal(1, model.GetEntityTypes().Count());
            },
            "DROP TABLE NonClusteredPrimaryKeyTableWithClusteredIndex;");

    [ConditionalFact]
    public void Set_clustered_false_for_primary_key_if_different_clustered_constraint()
        => Test(
            @"
CREATE TABLE NonClusteredPrimaryKeyTableWithClusteredConstraint (
    Id1 int PRIMARY KEY,
    Id2 int,
    CONSTRAINT UK_Clustered UNIQUE CLUSTERED ( Id2 ),
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var pk = dbModel.Tables.Single().PrimaryKey;

                Assert.Equal("dbo", pk!.Table!.Schema);
                Assert.Equal("NonClusteredPrimaryKeyTableWithClusteredConstraint", pk.Table.Name);
                Assert.StartsWith("PK__NonClust", pk.Name);
                Assert.False((bool)pk[SqlServerAnnotationNames.Clustered]!);
                Assert.Equal(["Id1"], pk.Columns.Select(ic => ic.Name).ToList());

                var model = scaffoldingFactory.Create(dbModel, new());
                Assert.Equal(1, model.GetEntityTypes().Count());
            },
            "DROP TABLE NonClusteredPrimaryKeyTableWithClusteredConstraint;");

    [ConditionalFact]
    public void Set_primary_key_name_from_index()
        => Test(
            @"
CREATE TABLE PrimaryKeyName (
    Id1 int,
    Id2 int,
    CONSTRAINT MyPK PRIMARY KEY ( Id2 ),
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var pk = dbModel.Tables.Single().PrimaryKey;

                Assert.Equal("dbo", pk!.Table!.Schema);
                Assert.Equal("PrimaryKeyName", pk.Table.Name);
                Assert.StartsWith("MyPK", pk.Name);
                Assert.Null(pk[SqlServerAnnotationNames.Clustered]);
                Assert.Equal(["Id2"], pk.Columns.Select(ic => ic.Name).ToList());

                var model = scaffoldingFactory.Create(dbModel, new());
                Assert.Equal(1, model.GetEntityTypes().Count());
            },
            "DROP TABLE PrimaryKeyName;");

    [ConditionalFact]
    public void Primary_key_fill_factor()
        => Test(
            @"
CREATE TABLE PrimaryKeyFillFactor
(
    Id INT IDENTITY NOT NULL,
    Name NVARCHAR(100),
 CONSTRAINT [PK_Id] PRIMARY KEY NONCLUSTERED
(
        [Id] ASC
) WITH (FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY];",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var pk = dbModel.Tables.Single().PrimaryKey;
                Assert.NotNull(pk);
                Assert.Equal(["Id"], pk!.Columns.Select(kc => kc.Name).ToList());
                Assert.Equal(80, pk[SqlServerAnnotationNames.FillFactor]);

                var model = scaffoldingFactory.Create(dbModel, new());
                Assert.Equal(1, model.GetEntityTypes().Count());
            },
            "DROP TABLE PrimaryKeyFillFactor;");

    #endregion

    #region UniqueConstraintFacets

    [ConditionalFact]
    public void Create_composite_unique_constraint()
        => Test(
            @"
CREATE TABLE CompositeUniqueConstraintTable (
    Id1 int,
    Id2 int,
    CONSTRAINT UX UNIQUE (Id2, Id1)
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var uniqueConstraint = Assert.Single(dbModel.Tables.Single().UniqueConstraints);

                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("dbo", uniqueConstraint.Table.Schema);
                Assert.Equal("CompositeUniqueConstraintTable", uniqueConstraint.Table.Name);
                Assert.Equal("UX", uniqueConstraint.Name);
                Assert.Equal(["Id2", "Id1"], uniqueConstraint.Columns.Select(ic => ic.Name).ToList());

                var model = scaffoldingFactory.Create(dbModel, new());
                Assert.Equal(1, model.GetEntityTypes().Count());
            },
            "DROP TABLE CompositeUniqueConstraintTable;");

    [ConditionalFact]
    public void Set_clustered_true_for_clustered_unique_constraint()
        => Test(
            @"
CREATE TABLE ClusteredUniqueConstraintTable (
    Id1 int,
    Id2 int UNIQUE CLUSTERED,
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var uniqueConstraint = Assert.Single(dbModel.Tables.Single().UniqueConstraints);

                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("dbo", uniqueConstraint.Table.Schema);
                Assert.Equal("ClusteredUniqueConstraintTable", uniqueConstraint.Table.Name);
                Assert.StartsWith("UQ__Clustere", uniqueConstraint.Name);
                Assert.True((bool)uniqueConstraint[SqlServerAnnotationNames.Clustered]!);
                Assert.Equal(["Id2"], uniqueConstraint.Columns.Select(ic => ic.Name).ToList());

                var model = scaffoldingFactory.Create(dbModel, new());
                Assert.Equal(1, model.GetEntityTypes().Count());
            },
            "DROP TABLE ClusteredUniqueConstraintTable;");

    [ConditionalFact]
    public void Set_unique_constraint_name_from_index()
        => Test(
            @"
CREATE TABLE UniqueConstraintName (
    Id1 int,
    Id2 int,
    CONSTRAINT MyUC UNIQUE ( Id2 ),
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var uniqueConstraint = Assert.Single(dbModel.Tables.Single().UniqueConstraints);

                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("dbo", uniqueConstraint.Table.Schema);
                Assert.Equal("UniqueConstraintName", uniqueConstraint.Table.Name);
                Assert.Equal("MyUC", uniqueConstraint.Name);
                Assert.Equal(["Id2"], uniqueConstraint.Columns.Select(ic => ic.Name).ToList());

                var model = scaffoldingFactory.Create(dbModel, new());
                Assert.Equal(1, model.GetEntityTypes().Count());
            },
            "DROP TABLE UniqueConstraintName;");

    [ConditionalFact]
    public void Unique_constraint_fill_factor()
        => Test(
            @"
CREATE TABLE UniqueConstraintFillFactor
(
    Something NVARCHAR(100) NOT NULL,
    SomethingElse NVARCHAR(100) NOT NULL,
 CONSTRAINT [UC_Something_SomethingElse] UNIQUE NONCLUSTERED
(
    [Something] ASC,
    [SomethingElse] ASC
) WITH (FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY];",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var uniqueConstraint = Assert.Single(dbModel.Tables.Single().UniqueConstraints);
                Assert.NotNull(uniqueConstraint);
                Assert.Equal(["Something", "SomethingElse"], uniqueConstraint!.Columns.Select(kc => kc.Name).ToList());
                Assert.Equal(80, uniqueConstraint[SqlServerAnnotationNames.FillFactor]);

                var model = scaffoldingFactory.Create(dbModel, new());
                Assert.Equal(1, model.GetEntityTypes().Count());
            },
            "DROP TABLE UniqueConstraintFillFactor;");

    #endregion

    #region IndexFacets

    [ConditionalFact]
    public void Create_composite_index()
        => Test(
            @"
CREATE TABLE CompositeIndexTable (
    Id1 int,
    Id2 int,
);

CREATE INDEX IX_COMPOSITE ON CompositeIndexTable ( Id2, Id1 );",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var index = Assert.Single(dbModel.Tables.Single().Indexes);

                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("dbo", index.Table!.Schema);
                Assert.Equal("CompositeIndexTable", index.Table.Name);
                Assert.Equal("IX_COMPOSITE", index.Name);
                Assert.Equal(["Id2", "Id1"], index.Columns.Select(ic => ic.Name).ToList());

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Collection(
                    model.GetEntityTypes(),
                    e =>
                    {
                        Assert.Equal("dbo", e.GetSchema());
                        Assert.Equal("CompositeIndexTable", e.Name);
                        Assert.Empty(e.GetKeys());
                        Assert.Collection(e.GetProperties(), p => Assert.Equal("Id1", p.Name), p => Assert.Equal("Id2", p.Name));
                        Assert.Empty(e.GetForeignKeys());
                        Assert.Collection(e.GetIndexes(), k =>
                        {
                            Assert.Collection(
                                k.Properties,
                                p => Assert.Equal("Id2", p.Name),
                                p => Assert.Equal("Id1", p.Name));
                            Assert.False(k.IsUnique);
                        });
                        Assert.Empty(e.GetNavigations());
                        Assert.Empty(e.GetSkipNavigations());
                    });
            },
            "DROP TABLE CompositeIndexTable;");

    [ConditionalFact]
    public void Set_clustered_true_for_clustered_index()
        => Test(
            @"
CREATE TABLE ClusteredIndexTable (
    Id1 int,
    Id2 int,
);

CREATE CLUSTERED INDEX IX_CLUSTERED ON ClusteredIndexTable ( Id2 );",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var index = Assert.Single(dbModel.Tables.Single().Indexes);

                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("dbo", index.Table!.Schema);
                Assert.Equal("ClusteredIndexTable", index.Table.Name);
                Assert.Equal("IX_CLUSTERED", index.Name);
                Assert.True((bool)index[SqlServerAnnotationNames.Clustered]!);
                Assert.Equal(["Id2"], index.Columns.Select(ic => ic.Name).ToList());

                var model = scaffoldingFactory.Create(dbModel, new());
                Assert.Equal(1, model.GetEntityTypes().Count());
            },
            "DROP TABLE ClusteredIndexTable;");

    [ConditionalFact]
    public void Set_unique_true_for_unique_index()
        => Test(
            @"
CREATE TABLE UniqueIndexTable (
    Id1 int,
    Id2 int,
);

CREATE UNIQUE INDEX IX_UNIQUE ON UniqueIndexTable ( Id2 );",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var index = Assert.Single(dbModel.Tables.Single().Indexes);

                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("dbo", index.Table!.Schema);
                Assert.Equal("UniqueIndexTable", index.Table.Name);
                Assert.Equal("IX_UNIQUE", index.Name);
                Assert.True(index.IsUnique);
                Assert.Null(index.Filter);
                Assert.Equal(["Id2"], index.Columns.Select(ic => ic.Name).ToList());

                var model = scaffoldingFactory.Create(dbModel, new());
                Assert.Equal(1, model.GetEntityTypes().Count());
            },
            "DROP TABLE UniqueIndexTable;");

    [ConditionalFact]
    public void Set_filter_for_filtered_index()
        => Test(
            @"
CREATE TABLE FilteredIndexTable (
    Id1 int,
    Id2 int NULL,
);

CREATE UNIQUE INDEX IX_UNIQUE ON FilteredIndexTable ( Id2 ) WHERE Id2 > 10;",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var index = Assert.Single(dbModel.Tables.Single().Indexes);

                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("dbo", index.Table!.Schema);
                Assert.Equal("FilteredIndexTable", index.Table.Name);
                Assert.Equal("IX_UNIQUE", index.Name);
                Assert.Equal("([Id2]>(10))", index.Filter);
                Assert.Equal(["Id2"], index.Columns.Select(ic => ic.Name).ToList());

                var model = scaffoldingFactory.Create(dbModel, new());
                Assert.Equal(1, model.GetEntityTypes().Count());
            },
            "DROP TABLE FilteredIndexTable;");

    [ConditionalFact]
    public void Ignore_hypothetical_index()
        => Test(
            @"
CREATE TABLE HypotheticalIndexTable (
    Id1 int,
    Id2 int NULL,
);

CREATE INDEX ixHypo ON HypotheticalIndexTable ( Id1 ) WITH STATISTICS_ONLY = -1;",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                Assert.Empty(dbModel.Tables.Single().Indexes);

                var model = scaffoldingFactory.Create(dbModel, new());
                Assert.Equal(1, model.GetEntityTypes().Count());
            },
            "DROP TABLE HypotheticalIndexTable;");

    [ConditionalFact]
    public void Ignore_columnstore_index()
        => Test(
            @"
CREATE TABLE ColumnStoreIndexTable (
    Id1 int,
    Id2 int NULL,
);

CREATE NONCLUSTERED COLUMNSTORE INDEX ixColumnStore ON ColumnStoreIndexTable ( Id1, Id2 )",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                Assert.Empty(dbModel.Tables.Single().Indexes);

                var model = scaffoldingFactory.Create(dbModel, new());
                Assert.Equal(1, model.GetEntityTypes().Count());
            },
            "DROP TABLE ColumnStoreIndexTable;");

    [ConditionalFact]
    public void Set_include_for_index()
        => Test(
            @"
CREATE TABLE IncludeIndexTable (
    Id int,
    IndexProperty int,
    IncludeProperty int
);

CREATE INDEX IX_INCLUDE ON IncludeIndexTable(IndexProperty) INCLUDE (IncludeProperty);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var index = Assert.Single(dbModel.Tables.Single().Indexes);
                Assert.Equal(new[] { "IndexProperty" }, index.Columns.Select(ic => ic.Name).ToList());
                Assert.Null(index[SqlServerAnnotationNames.Include]);

                var model = scaffoldingFactory.Create(dbModel, new());
                Assert.Equal(1, model.GetEntityTypes().Count());
            },
            "DROP TABLE IncludeIndexTable;");

    [ConditionalFact]
    public void Index_fill_factor()
        => Test(
            @"
CREATE TABLE IndexFillFactor
(
    Id INT IDENTITY,
    Name NVARCHAR(100)
);

CREATE NONCLUSTERED INDEX [IX_Name] ON [dbo].[IndexFillFactor]
(
     [Name] ASC
)
WITH (FILLFACTOR = 80) ON [PRIMARY]",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var index = Assert.Single(dbModel.Tables.Single().Indexes);
                Assert.Equal(new[] { "Name" }, index.Columns.Select(ic => ic.Name).ToList());
                Assert.Equal(80, index[SqlServerAnnotationNames.FillFactor]);

                var model = scaffoldingFactory.Create(dbModel, new());
                Assert.Equal(1, model.GetEntityTypes().Count());
            },
            "DROP TABLE IndexFillFactor;");

    #endregion

    #region ForeignKeyFacets

    [ConditionalFact]
    public void Create_composite_foreign_key()
        => Test(
            @"
CREATE TABLE PrincipalTable (
    Id1 int,
    Id2 int,
    PRIMARY KEY (Id1, Id2)
);

CREATE TABLE DependentTable (
    Id int PRIMARY KEY,
    ForeignKeyId1 int,
    ForeignKeyId2 int,
    FOREIGN KEY (ForeignKeyId1, ForeignKeyId2) REFERENCES PrincipalTable(Id1, Id2) ON DELETE CASCADE,
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var fk = Assert.Single(dbModel.Tables.Single(t => t.Name == "DependentTable").ForeignKeys);

                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("dbo", fk.Table.Schema);
                Assert.Equal("DependentTable", fk.Table.Name);
                Assert.Equal("dbo", fk.PrincipalTable.Schema);
                Assert.Equal("PrincipalTable", fk.PrincipalTable.Name);
                Assert.Equal(["ForeignKeyId1", "ForeignKeyId2"], fk.Columns.Select(ic => ic.Name).ToList());
                Assert.Equal(["Id1", "Id2"], fk.PrincipalColumns.Select(ic => ic.Name).ToList());
                Assert.Equal(ReferentialAction.Cascade, fk.OnDelete);

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Collection(
                    model.GetEntityTypes(),
                    e =>
                    {
                        Assert.Equal("DependentTable", e.Name);
                        Assert.Collection(e.GetProperties(),
                            p => Assert.Equal("Id", p.Name),
                            p => Assert.Equal("ForeignKeyId1", p.Name),
                            p => Assert.Equal("ForeignKeyId2", p.Name));
                        Assert.Collection(e.GetKeys(), k => Assert.Equal("Id", k.Properties.Single().Name));
                        Assert.Collection(e.GetForeignKeys(),
                            k =>
                            {
                                Assert.Equal("PrincipalTable", k.PrincipalEntityType.Name);
                                Assert.Equal("DependentTable", k.DeclaringEntityType.Name);
                                Assert.Collection(k.Properties,
                                    p => Assert.Equal("ForeignKeyId1", p.Name),
                                    p => Assert.Equal("ForeignKeyId2", p.Name));
                                Assert.Collection(k.PrincipalKey.Properties,
                                    p => Assert.Equal("Id1", p.Name),
                                    p => Assert.Equal("Id2", p.Name));
                                Assert.False(k.IsUnique);
                            });
                        Assert.Empty(e.GetSkipNavigations());
                        Assert.Collection(e.GetNavigations(), n =>
                        {
                            Assert.Equal("PrincipalTable", n.Name);
                            Assert.False(n.IsCollection);
                        });
                    },
                    e =>
                    {
                        Assert.Equal("PrincipalTable", e.Name);
                        Assert.Collection(e.GetProperties(),
                            p => Assert.Equal("Id1", p.Name),
                            p => Assert.Equal("Id2", p.Name));
                        Assert.Collection(e.GetKeys(), k => Assert.Collection(k.Properties,
                            p => Assert.Equal("Id1", p.Name),
                            p => Assert.Equal("Id2", p.Name)));
                        Assert.Empty(e.GetForeignKeys());
                        Assert.Empty(e.GetSkipNavigations());
                        Assert.Collection(e.GetNavigations(), n =>
                        {
                            Assert.Equal("DependentTables", n.Name);
                            Assert.True(n.IsCollection);
                        });
                    });
            },
            @"
DROP TABLE DependentTable;
DROP TABLE PrincipalTable;");

    [ConditionalFact]
    public void Create_multiple_foreign_key_in_same_table()
        => Test(
            @"
CREATE TABLE PrincipalTable (
    Id int PRIMARY KEY,
);

CREATE TABLE AnotherPrincipalTable (
    Id int PRIMARY KEY,
);

CREATE TABLE DependentTable (
    Id int PRIMARY KEY,
    ForeignKeyId1 int,
    ForeignKeyId2 int,
    FOREIGN KEY (ForeignKeyId1) REFERENCES PrincipalTable(Id) ON DELETE CASCADE,
    FOREIGN KEY (ForeignKeyId2) REFERENCES AnotherPrincipalTable(Id) ON DELETE CASCADE,
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var foreignKeys = dbModel.Tables.Single(t => t.Name == "DependentTable").ForeignKeys;

                Assert.Equal(2, foreignKeys.Count);

                var principalFk = Assert.Single(foreignKeys.Where(f => f.PrincipalTable.Name == "PrincipalTable"));

                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("dbo", principalFk.Table.Schema);
                Assert.Equal("DependentTable", principalFk.Table.Name);
                Assert.Equal("dbo", principalFk.PrincipalTable.Schema);
                Assert.Equal("PrincipalTable", principalFk.PrincipalTable.Name);
                Assert.Equal(["ForeignKeyId1"], principalFk.Columns.Select(ic => ic.Name).ToList());
                Assert.Equal(["Id"], principalFk.PrincipalColumns.Select(ic => ic.Name).ToList());
                Assert.Equal(ReferentialAction.Cascade, principalFk.OnDelete);

                var anotherPrincipalFk = Assert.Single(foreignKeys.Where(f => f.PrincipalTable.Name == "AnotherPrincipalTable"));

                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("dbo", anotherPrincipalFk.Table.Schema);
                Assert.Equal("DependentTable", anotherPrincipalFk.Table.Name);
                Assert.Equal("dbo", anotherPrincipalFk.PrincipalTable.Schema);
                Assert.Equal("AnotherPrincipalTable", anotherPrincipalFk.PrincipalTable.Name);
                Assert.Equal(["ForeignKeyId2"], anotherPrincipalFk.Columns.Select(ic => ic.Name).ToList());
                Assert.Equal(["Id"], anotherPrincipalFk.PrincipalColumns.Select(ic => ic.Name).ToList());
                Assert.Equal(ReferentialAction.Cascade, anotherPrincipalFk.OnDelete);

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Collection(
                    model.GetEntityTypes(),
                    e =>
                    {
                        Assert.Equal("AnotherPrincipalTable", e.Name);
                        Assert.Collection(e.GetProperties(), p => Assert.Equal("Id", p.Name));
                        Assert.Collection(e.GetKeys(), k => Assert.Collection(k.Properties, p => Assert.Equal("Id", p.Name)));
                        Assert.Empty(e.GetForeignKeys());
                        Assert.Empty(e.GetSkipNavigations());
                        Assert.Collection(e.GetNavigations(), n =>
                        {
                            Assert.Equal("DependentTables", n.Name);
                            Assert.True(n.IsCollection);
                        });
                    },
                    e =>
                    {
                        Assert.Equal("DependentTable", e.Name);
                        Assert.Collection(e.GetProperties(),
                            p => Assert.Equal("Id", p.Name),
                            p => Assert.Equal("ForeignKeyId1", p.Name),
                            p => Assert.Equal("ForeignKeyId2", p.Name));
                        Assert.Collection(e.GetKeys(), k => Assert.Equal("Id", k.Properties.Single().Name));
                        Assert.Collection(e.GetForeignKeys(),
                            k =>
                            {
                                Assert.Equal("PrincipalTable", k.PrincipalEntityType.Name);
                                Assert.Equal("DependentTable", k.DeclaringEntityType.Name);
                                Assert.Collection(k.Properties, p => Assert.Equal("ForeignKeyId1", p.Name));
                                Assert.Collection(k.PrincipalKey.Properties, p => Assert.Equal("Id", p.Name));
                                Assert.False(k.IsUnique);
                            },
                            k =>
                            {
                                Assert.Equal("AnotherPrincipalTable", k.PrincipalEntityType.Name);
                                Assert.Equal("DependentTable", k.DeclaringEntityType.Name);
                                Assert.Collection(k.Properties, p => Assert.Equal("ForeignKeyId2", p.Name));
                                Assert.Collection(k.PrincipalKey.Properties, p => Assert.Equal("Id", p.Name));
                                Assert.False(k.IsUnique);
                            });
                        Assert.Empty(e.GetSkipNavigations());
                        Assert.Collection(e.GetNavigations(), n =>
                        {
                            Assert.Equal("ForeignKeyId1Navigation", n.Name);
                            Assert.False(n.IsCollection);
                        }, n =>
                        {
                            Assert.Equal("ForeignKeyId2Navigation", n.Name);
                            Assert.False(n.IsCollection);
                        });
                    },
                    e =>
                    {
                        Assert.Equal("PrincipalTable", e.Name);
                        Assert.Collection(e.GetProperties(), p => Assert.Equal("Id", p.Name));
                        Assert.Collection(e.GetKeys(), k => Assert.Collection(k.Properties, p => Assert.Equal("Id", p.Name)));
                        Assert.Empty(e.GetForeignKeys());
                        Assert.Empty(e.GetSkipNavigations());
                        Assert.Collection(e.GetNavigations(), n =>
                        {
                            Assert.Equal("DependentTables", n.Name);
                            Assert.True(n.IsCollection);
                        });
                    });
            },
            @"
DROP TABLE DependentTable;
DROP TABLE AnotherPrincipalTable;
DROP TABLE PrincipalTable;");

    [ConditionalFact]
    public void Create_foreign_key_referencing_unique_constraint()
        => Test(
            @"
CREATE TABLE PrincipalTable (
    Id1 int PRIMARY KEY,
    Id2 int UNIQUE,
);

CREATE TABLE DependentTable (
    Id int PRIMARY KEY,
    ForeignKeyId int,
    FOREIGN KEY (ForeignKeyId) REFERENCES PrincipalTable(Id2) ON DELETE CASCADE,
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var fk = Assert.Single(dbModel.Tables.Single(t => t.Name == "DependentTable").ForeignKeys);

                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("dbo", fk.Table.Schema);
                Assert.Equal("DependentTable", fk.Table.Name);
                Assert.Equal("dbo", fk.PrincipalTable.Schema);
                Assert.Equal("PrincipalTable", fk.PrincipalTable.Name);
                Assert.Equal(["ForeignKeyId"], fk.Columns.Select(ic => ic.Name).ToList());
                Assert.Equal(["Id2"], fk.PrincipalColumns.Select(ic => ic.Name).ToList());
                Assert.Equal(ReferentialAction.Cascade, fk.OnDelete);

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Collection(
                    model.GetEntityTypes(),
                    e =>
                    {
                        Assert.Equal("DependentTable", e.Name);
                        Assert.Collection(e.GetProperties(),
                            p => Assert.Equal("Id", p.Name),
                            p => Assert.Equal("ForeignKeyId", p.Name));
                        Assert.Collection(e.GetKeys(), k => Assert.Equal("Id", k.Properties.Single().Name));
                        Assert.Collection(e.GetForeignKeys(),
                            k =>
                            {
                                Assert.Equal("PrincipalTable", k.PrincipalEntityType.Name);
                                Assert.Equal("DependentTable", k.DeclaringEntityType.Name);
                                Assert.Collection(k.Properties, p => Assert.Equal("ForeignKeyId", p.Name));
                                Assert.Collection(k.PrincipalKey.Properties, p => Assert.Equal("Id2", p.Name));
                                Assert.False(k.IsUnique);
                            });
                        Assert.Empty(e.GetSkipNavigations());
                        Assert.Collection(e.GetNavigations(), n =>
                        {
                            Assert.Equal("ForeignKey", n.Name);
                            Assert.False(n.IsCollection);
                        });
                    },
                    e =>
                    {
                        Assert.Equal("PrincipalTable", e.Name);
                        Assert.Collection(e.GetProperties(), p => Assert.Equal("Id1", p.Name), p => Assert.Equal("Id2", p.Name));
                        Assert.Collection(e.GetKeys(),
                            k => Assert.Collection(k.Properties, p =>
                            {
                                Assert.Equal("Id1", p.Name);
                                Assert.True(p.IsPrimaryKey());
                            }),
                            k => Assert.Collection(k.Properties, p =>
                            {
                                Assert.Equal("Id2", p.Name);
                                Assert.False(p.IsPrimaryKey());
                            }));
                        Assert.Empty(e.GetForeignKeys());
                        Assert.Empty(e.GetSkipNavigations());
                        Assert.Collection(e.GetNavigations(), n =>
                        {
                            Assert.Equal("DependentTables", n.Name);
                            Assert.True(n.IsCollection);
                        });
                    });
            },
            @"
DROP TABLE DependentTable;
DROP TABLE PrincipalTable;");

    [ConditionalFact]
    public void Set_name_for_foreign_key()
        => Test(
            @"
CREATE TABLE PrincipalTable (
    Id int PRIMARY KEY,
);

CREATE TABLE DependentTable (
    Id int PRIMARY KEY,
    ForeignKeyId int,
    CONSTRAINT MYFK FOREIGN KEY (ForeignKeyId) REFERENCES PrincipalTable(Id) ON DELETE CASCADE,
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var fk = Assert.Single(dbModel.Tables.Single(t => t.Name == "DependentTable").ForeignKeys);

                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("dbo", fk.Table.Schema);
                Assert.Equal("DependentTable", fk.Table.Name);
                Assert.Equal("dbo", fk.PrincipalTable.Schema);
                Assert.Equal("PrincipalTable", fk.PrincipalTable.Name);
                Assert.Equal(["ForeignKeyId"], fk.Columns.Select(ic => ic.Name).ToList());
                Assert.Equal(["Id"], fk.PrincipalColumns.Select(ic => ic.Name).ToList());
                Assert.Equal(ReferentialAction.Cascade, fk.OnDelete);
                Assert.Equal("MYFK", fk.Name);

                var model = scaffoldingFactory.Create(dbModel, new());
                Assert.Equal(2, model.GetEntityTypes().Count());
            },
            @"
DROP TABLE DependentTable;
DROP TABLE PrincipalTable;");

    [ConditionalFact]
    public void Set_referential_action_for_foreign_key()
        => Test(
            @"
CREATE TABLE PrincipalTable (
    Id int PRIMARY KEY,
);

CREATE TABLE DependentTable (
    Id int PRIMARY KEY,
    ForeignKeyId int,
    FOREIGN KEY (ForeignKeyId) REFERENCES PrincipalTable(Id) ON DELETE SET NULL,
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var fk = Assert.Single(dbModel.Tables.Single(t => t.Name == "DependentTable").ForeignKeys);

                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("dbo", fk.Table.Schema);
                Assert.Equal("DependentTable", fk.Table.Name);
                Assert.Equal("dbo", fk.PrincipalTable.Schema);
                Assert.Equal("PrincipalTable", fk.PrincipalTable.Name);
                Assert.Equal(["ForeignKeyId"], fk.Columns.Select(ic => ic.Name).ToList());
                Assert.Equal(["Id"], fk.PrincipalColumns.Select(ic => ic.Name).ToList());
                Assert.Equal(ReferentialAction.SetNull, fk.OnDelete);

                var model = scaffoldingFactory.Create(dbModel, new());

                Assert.Collection(
                    model.GetEntityTypes(),
                    e =>
                    {
                        Assert.Equal("DependentTable", e.Name);
                        Assert.Collection(e.GetProperties(),
                            p => Assert.Equal("Id", p.Name),
                            p => Assert.Equal("ForeignKeyId", p.Name));
                        Assert.Collection(e.GetKeys(), k => Assert.Equal("Id", k.Properties.Single().Name));
                        Assert.Collection(e.GetForeignKeys(),
                            k =>
                            {
                                Assert.Equal("PrincipalTable", k.PrincipalEntityType.Name);
                                Assert.Equal("DependentTable", k.DeclaringEntityType.Name);
                                Assert.Collection(k.Properties, p => Assert.Equal("ForeignKeyId", p.Name));
                                Assert.Collection(k.PrincipalKey.Properties, p => Assert.Equal("Id", p.Name));
                                Assert.Equal(DeleteBehavior.SetNull, k.DeleteBehavior);
                                Assert.False(k.IsUnique);
                            });
                        Assert.Empty(e.GetSkipNavigations());
                        Assert.Collection(e.GetNavigations(), n =>
                        {
                            Assert.Equal("ForeignKey", n.Name);
                            Assert.False(n.IsCollection);
                        });
                    },
                    e =>
                    {
                        Assert.Equal("PrincipalTable", e.Name);
                        Assert.Collection(e.GetProperties(), p => Assert.Equal("Id", p.Name));
                        Assert.Collection(e.GetKeys(), k => Assert.Collection(k.Properties, p => Assert.Equal("Id", p.Name)));
                        Assert.Empty(e.GetForeignKeys());
                        Assert.Empty(e.GetSkipNavigations());
                        Assert.Collection(e.GetNavigations(), n =>
                        {
                            Assert.Equal("DependentTables", n.Name);
                            Assert.True(n.IsCollection);
                        });
                    });
            },
            @"
DROP TABLE DependentTable;
DROP TABLE PrincipalTable;");

    #endregion

    #region Warnings

    [ConditionalFact]
    public void Warn_missing_schema()
        => Test(
            @"
CREATE TABLE Blank (
    Id int,
);",
            Enumerable.Empty<string>(),
            new[] { "MySchema" },
            (dbModel, scaffoldingFactory) =>
            {
                Assert.Empty(dbModel.Tables);

                var message = Fixture.OperationReporter.Messages.Single(m => m.Level == LogLevel.Warning).Message;

                Assert.Equal(
                    SqlServerResources.LogMissingSchema(new TestLogger<SqlServerLoggingDefinitions>()).GenerateMessage("MySchema"),
                    message);

                var model = scaffoldingFactory.Create(dbModel, new());
                Assert.Empty(model.GetEntityTypes());
            },
            "DROP TABLE Blank;");

    [ConditionalFact]
    public void Warn_missing_table()
        => Test(
            @"
CREATE TABLE Blank (
    Id int,
);",
            new[] { "MyTable" },
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                Assert.Empty(dbModel.Tables);

                var message = Fixture.OperationReporter.Messages.Single(m => m.Level == LogLevel.Warning).Message;

                Assert.Equal(
                    SqlServerResources.LogMissingTable(new TestLogger<SqlServerLoggingDefinitions>()).GenerateMessage("MyTable"),
                    message);

                var model = scaffoldingFactory.Create(dbModel, new());
                Assert.Empty(model.GetEntityTypes());
            },
            "DROP TABLE Blank;");

    [ConditionalFact]
    public void Warn_missing_principal_table_for_foreign_key()
        => Test(
            @"
CREATE TABLE PrincipalTable (
    Id int PRIMARY KEY,
);

CREATE TABLE DependentTable (
    Id int PRIMARY KEY,
    ForeignKeyId int,
    CONSTRAINT MYFK FOREIGN KEY (ForeignKeyId) REFERENCES PrincipalTable(Id) ON DELETE CASCADE,
);",
            new[] { "DependentTable" },
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var message = Fixture.OperationReporter.Messages.Single(m => m.Level == LogLevel.Warning).Message;

                Assert.Equal(
                    SqlServerResources.LogPrincipalTableNotInSelectionSet(new TestLogger<SqlServerLoggingDefinitions>())
                        .GenerateMessage(
                            "MYFK", "dbo.DependentTable", "dbo.PrincipalTable"), message);

                var model = scaffoldingFactory.Create(dbModel, new());
                Assert.Equal(1, model.GetEntityTypes().Count());
            },
            @"
DROP TABLE DependentTable;
DROP TABLE PrincipalTable;");

    [ConditionalFact]
    public void Skip_reflexive_foreign_key()
        => Test(
            @"
CREATE TABLE PrincipalTable (
    Id int PRIMARY KEY,
    CONSTRAINT MYFK FOREIGN KEY (Id) REFERENCES PrincipalTable(Id)
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var level = Fixture.OperationReporter.Messages
                    .Single(
                        m => m.Message
                            == SqlServerResources.LogReflexiveConstraintIgnored(new TestLogger<SqlServerLoggingDefinitions>())
                                .GenerateMessage("MYFK", "dbo.PrincipalTable")).Level;

                Assert.Equal(LogLevel.Debug, level);

                var table = Assert.Single(dbModel.Tables);
                Assert.Empty(table.ForeignKeys);

                var model = scaffoldingFactory.Create(dbModel, new());
                Assert.Equal(1, model.GetEntityTypes().Count());
            },
            @"
DROP TABLE PrincipalTable;");

    [ConditionalFact]
    public void Skip_duplicate_foreign_key()
        => Test(
            @"CREATE TABLE PrincipalTable (
    Id int PRIMARY KEY,
    Value1 uniqueidentifier,
    Value2 uniqueidentifier,
	CONSTRAINT [UNIQUE_Value1] UNIQUE ([Value1] ASC),
	CONSTRAINT [UNIQUE_Value2] UNIQUE ([Value2] ASC),
);

CREATE TABLE OtherPrincipalTable (
    Id int PRIMARY KEY,
);

CREATE TABLE DependentTable (
    Id int PRIMARY KEY,
    ForeignKeyId int,
    ValueKey uniqueidentifier,
    CONSTRAINT MYFK1 FOREIGN KEY (ForeignKeyId) REFERENCES PrincipalTable(Id),
    CONSTRAINT MYFK2 FOREIGN KEY (ForeignKeyId) REFERENCES PrincipalTable(Id),
    CONSTRAINT MYFK3 FOREIGN KEY (ForeignKeyId) REFERENCES OtherPrincipalTable(Id),
    CONSTRAINT MYFK4 FOREIGN KEY (ValueKey) REFERENCES PrincipalTable(Value1),
    CONSTRAINT MYFK5 FOREIGN KEY (ValueKey) REFERENCES PrincipalTable(Value2),
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var level = Fixture.OperationReporter.Messages
                    .Single(
                        m => m.Message
                            == SqlServerResources.LogDuplicateForeignKeyConstraintIgnored(new TestLogger<SqlServerLoggingDefinitions>())
                                .GenerateMessage("MYFK2", "dbo.DependentTable", "MYFK1")).Level;

                Assert.Equal(LogLevel.Warning, level);

                var table = dbModel.Tables.Single(t => t.Name == "DependentTable");
                Assert.Equal(4, table.ForeignKeys.Count);

                var model = scaffoldingFactory.Create(dbModel, new());
                Assert.Equal(3, model.GetEntityTypes().Count());
            },
            @"
DROP TABLE DependentTable;
DROP TABLE PrincipalTable;
DROP TABLE OtherPrincipalTable;");

    [ConditionalFact]
    public void No_warning_missing_view_definition()
        => Test(
            @"CREATE TABLE TestViewDefinition (
Id int PRIMARY KEY,
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var message = Fixture.OperationReporter.Messages
                    .SingleOrDefault(
                        m => m.Message
                            == SqlServerResources.LogMissingViewDefinitionRights(new TestLogger<SqlServerLoggingDefinitions>())
                                .GenerateMessage()).Message;

                Assert.Null(message);

                var model = scaffoldingFactory.Create(dbModel, new());
                Assert.Equal(1, model.GetEntityTypes().Count());
            },
            @"
DROP TABLE TestViewDefinition;");

    #endregion

    private void Test(
        string? createSql,
        IEnumerable<string> tables,
        IEnumerable<string> schemas,
        Action<DatabaseModel, IScaffoldingModelFactory> asserter,
        string? cleanupSql)
        => Test(
            string.IsNullOrEmpty(createSql) ? [] : [createSql],
            tables,
            schemas,
            asserter,
            cleanupSql);

    private void Test(
        string[] createSqls,
        IEnumerable<string> tables,
        IEnumerable<string> schemas,
        Action<DatabaseModel, IScaffoldingModelFactory> asserter,
        string? cleanupSql)
    {
        foreach (var createSql in createSqls)
        {
            Fixture.TestStore.ExecuteNonQuery(createSql);
        }

        try
        {
            var serviceProvider = SqlServerTestHelpers.Instance.CreateDesignServiceProvider(reporter: Fixture.OperationReporter)
                .CreateScope().ServiceProvider;

            var databaseModelFactory = serviceProvider.GetRequiredService<IDatabaseModelFactory>();

            var databaseModel = databaseModelFactory.Create(
                Fixture.TestStore.ConnectionString,
                new DatabaseModelFactoryOptions(tables, schemas));

            Assert.NotNull(databaseModel);

            asserter(databaseModel, serviceProvider.GetRequiredService<IScaffoldingModelFactory>());
        }
        finally
        {
            if (!string.IsNullOrEmpty(cleanupSql))
            {
                Fixture.TestStore.ExecuteNonQuery(cleanupSql);
            }
        }
    }

    public class SqlServerDatabaseModelFixture : SharedStoreFixtureBase<PoolableDbContext>
    {
        protected override string StoreName
            => nameof(SqlServerDatabaseModelFactoryTest);

        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        public new SqlServerTestStore TestStore
            => (SqlServerTestStore)base.TestStore;

        public TestOperationReporter OperationReporter { get; } = new();

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            await TestStore.ExecuteNonQueryAsync("CREATE SCHEMA db2");
            await TestStore.ExecuteNonQueryAsync("CREATE SCHEMA [db.2]");
        }

        protected override bool ShouldLogCategory(string logCategory)
            => logCategory == DbLoggerCategory.Scaffolding.Name;
    }
}

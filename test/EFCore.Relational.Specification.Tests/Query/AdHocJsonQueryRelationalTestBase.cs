// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class AdHocJsonQueryRelationalTestBase(NonSharedFixture fixture) : AdHocJsonQueryTestBase(fixture)
{
    #region 21006

    public override async Task Project_missing_required_navigation(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Project_missing_required_navigation(async))).Message;

        Assert.Equal(RelationalStrings.JsonRequiredEntityWithNullJson(typeof(Context21006.JsonEntityNested).Name), message);
    }

    public override async Task Project_null_required_navigation(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Project_null_required_navigation(async))).Message;

        Assert.Equal(RelationalStrings.JsonRequiredEntityWithNullJson(typeof(Context21006.JsonEntityNested).Name), message);
    }

    public override async Task Project_top_level_entity_with_null_value_required_scalars(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Project_top_level_entity_with_null_value_required_scalars(async))).Message;

        Assert.Equal("Cannot get the value of a token type 'Null' as a number.", message);
    }

    protected override void OnModelCreating21006(ModelBuilder modelBuilder)
    {
        base.OnModelCreating21006(modelBuilder);

        modelBuilder.Entity<Context21006.Entity>(
            b =>
            {
                b.ToTable("Entities");
                b.OwnsOne(x => x.OptionalReference).ToJson();
                b.OwnsOne(x => x.RequiredReference).ToJson();
                b.OwnsMany(x => x.Collection).ToJson();
            });
    }

    #endregion

    #region 32310

    protected override void OnModelCreating32310(ModelBuilder modelBuilder)
    {
        base.OnModelCreating32310(modelBuilder);

        modelBuilder.Entity<Context32310.Pub>().OwnsOne(e => e.Visits).ToJson().HasColumnType(JsonColumnType);
    }

    #endregion

    #region 29219

    protected override void OnModelCreating29219(ModelBuilder modelBuilder)
    {
        base.OnModelCreating29219(modelBuilder);

        modelBuilder.Entity<Context29219.MyEntity>(
            b =>
            {
                b.ToTable("Entities");
                b.OwnsOne(x => x.Reference).ToJson().HasColumnType(JsonColumnType);
                b.OwnsMany(x => x.Collection).ToJson().HasColumnType(JsonColumnType);
            });
    }

    #endregion

    #region 30028

    protected override void OnModelCreating30028(ModelBuilder modelBuilder)
    {
        base.OnModelCreating30028(modelBuilder);

        modelBuilder.Entity<Context30028.MyEntity>(b =>
        {
            b.ToTable("Entities");
            b.OwnsOne(x => x.Json, nb =>
            {
                nb.ToJson().HasColumnType(JsonColumnType);
            });
        });
    }

    #endregion

    #region 32939

    protected override void OnModelCreating32939(ModelBuilder modelBuilder)
    {
        base.OnModelCreating32939(modelBuilder);

        modelBuilder.Entity<Context32939.Entity>().OwnsOne(x => x.Empty, b => b.ToJson().HasColumnType(JsonColumnType));
        modelBuilder.Entity<Context32939.Entity>().OwnsOne(x => x.FieldOnly, b => b.ToJson().HasColumnType(JsonColumnType));
    }

    #endregion

    #region 33046

    protected override void OnModelCreating33046(ModelBuilder modelBuilder)
    {
        base.OnModelCreating33046(modelBuilder);

        modelBuilder.Entity<Context33046.Review>(b =>
        {
            b.ToTable("Reviews");
            b.OwnsMany(x => x.Rounds, ownedBuilder =>
            {
                ownedBuilder.ToJson().HasColumnType(JsonColumnType);
            });
        });
    }

    #endregion

    #region 34293

    [ConditionalFact]
    public virtual async Task Project_entity_with_optional_json_entity_owned_by_required_json()
    {
        var contextFactory = await InitializeAsync<Context34293>(
            onModelCreating: OnModelCreating34293,
            seed: ctx => ctx.Seed());

        using var context = contextFactory.CreateContext();
        var entityProjection = await context.Set<Context34293.Entity>().ToListAsync();

        Assert.Equal(3, entityProjection.Count);
    }

    [ConditionalFact]
    public virtual async Task Project_required_json_entity()
    {
        var contextFactory = await InitializeAsync<Context34293>(
            onModelCreating: OnModelCreating34293,
            seed: ctx => ctx.Seed());

        using var context = contextFactory.CreateContext();

        var rootProjection = await context.Set<Context34293.Entity>().AsNoTracking().Where(x => x.Id != 3).Select(x => x.Json).ToListAsync();
        Assert.Equal(2, rootProjection.Count);

        var branchProjection = await context.Set<Context34293.Entity>().AsNoTracking().Where(x => x.Id != 3).Select(x => x.Json.Required).ToListAsync();
        Assert.Equal(2, rootProjection.Count);

        var badRootProjectionMessage = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => context.Set<Context34293.Entity>().AsNoTracking().Where(x => x.Id == 3).Select(x => x.Json).ToListAsync())).Message;
        Assert.Equal(RelationalStrings.JsonRequiredEntityWithNullJson(nameof(Context34293.JsonBranch)), badRootProjectionMessage);

        var badBranchProjectionMessage = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => context.Set<Context34293.Entity>().AsNoTracking().Where(x => x.Id == 3).Select(x => x.Json.Required).ToListAsync())).Message;
        Assert.Equal(RelationalStrings.JsonRequiredEntityWithNullJson(nameof(Context34293.JsonBranch)), badBranchProjectionMessage);
    }

    [ConditionalFact]
    public virtual async Task Project_optional_json_entity_owned_by_required_json_entity()
    {
        var contextFactory = await InitializeAsync<Context34293>(
            onModelCreating: OnModelCreating34293,
            seed: ctx => ctx.Seed());

        using var context = contextFactory.CreateContext();
        var leafProjection = await context.Set<Context34293.Entity>().AsNoTracking().Select(x => x.Json.Required.Optional).ToListAsync();
        Assert.Equal(3, leafProjection.Count);
    }

    protected class Context34293(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Entity> Entities { get; set; }

        public class Entity
        {
            public int Id { get; set; }
            public JsonRoot Json { get; set; }
        }

        public class JsonRoot
        {
            public DateTime Date { get; set; }

            public JsonBranch Required { get; set; }
        }

        public class JsonBranch
        {
            public int Number { get; set; }
            public JsonLeaf Optional { get; set; }
        }

        public class JsonLeaf
        {
            public string Name { get; set; }
        }

        public async Task Seed()
        {
            // everything - ok
            var e1 = new Entity
            {
                Id = 1,
                Json = new JsonRoot
                {
                    Date = new DateTime(2001, 1, 1),
                    Required = new JsonBranch
                    {
                        Number = 1,
                        Optional = new JsonLeaf { Name = "optional 1" }
                    }
                }
            };

            // null leaf - ok (optional nav)
            var e2 = new Entity
            {
                Id = 2,
                Json = new JsonRoot
                {
                    Date = new DateTime(2002, 2, 2),
                    Required = new JsonBranch
                    {
                        Number = 2,
                        Optional = null
                    }
                }
            };

            // null branch - invalid (required nav)
            var e3 = new Entity
            {
                Id = 3,
                Json = new JsonRoot
                {
                    Date = new DateTime(2003, 3, 3),
                    Required = null,
                }
            };

            Entities.AddRange(e1, e2, e3);
            await SaveChangesAsync();
        }
    }

    protected virtual void OnModelCreating34293(ModelBuilder modelBuilder)
        => modelBuilder.Entity<Context34293.Entity>(
            b =>
            {
                b.Property(x => x.Id).ValueGeneratedNever();
                b.OwnsOne(
                    x => x.Json, b =>
                    {
                        b.ToJson().HasColumnType(JsonColumnType);
                        b.OwnsOne(x => x.Required, bb =>
                        {
                            bb.OwnsOne(x => x.Optional);
                            bb.Navigation(x => x.Optional).IsRequired(false);
                        });
                        b.Navigation(x => x.Required).IsRequired(true);
                    });
                b.Navigation(x => x.Json).IsRequired(true);
            });

    #endregion

    #region 34960

    public override async Task Try_project_collection_but_JSON_is_entity()
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Try_project_collection_but_JSON_is_entity())).Message;

        Assert.Equal(
            CoreStrings.JsonReaderInvalidTokenType(nameof(JsonTokenType.StartObject)),
            message);
    }

    public override async Task Try_project_reference_but_JSON_is_collection()
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Try_project_reference_but_JSON_is_collection())).Message;

        Assert.Equal(
            CoreStrings.JsonReaderInvalidTokenType(nameof(JsonTokenType.StartArray)),
            message);
    }

    protected override void OnModelCreating34960(ModelBuilder modelBuilder)
    {
        base.OnModelCreating34960(modelBuilder);

        modelBuilder.Entity<Context34960.Entity>(b =>
        {
            b.ToTable("Entities");

            b.OwnsOne(x => x.Reference, b =>
            {
                b.ToJson().HasColumnType(JsonColumnType);
            });

            b.OwnsMany(x => x.Collection, b =>
            {
                b.ToJson().HasColumnType(JsonColumnType);
            });
        });

        modelBuilder.Entity<Context34960.JunkEntity>(b =>
        {
            b.ToTable("Junk");

            b.OwnsOne(x => x.Reference, b =>
            {
                b.ToJson().HasColumnType(JsonColumnType);
            });

            b.OwnsMany(x => x.Collection, b =>
            {
                b.ToJson().HasColumnType(JsonColumnType);
            });
        });
    }

    #endregion

    #region ArrayOfPrimitives

    protected override void OnModelCreatingArrayOfPrimitives(ModelBuilder modelBuilder)
    {
        base.OnModelCreatingArrayOfPrimitives(modelBuilder);

        modelBuilder.Entity<ContextArrayOfPrimitives.MyEntity>().OwnsOne(
            x => x.Reference, b => b.ToJson().HasColumnType(JsonColumnType));

        modelBuilder.Entity<ContextArrayOfPrimitives.MyEntity>().OwnsMany(
            x => x.Collection, b => b.ToJson().HasColumnType(JsonColumnType));
    }

    #endregion

    #region JunkInJson

    protected override void OnModelCreatingJunkInJson(ModelBuilder modelBuilder)
    {
        base.OnModelCreatingJunkInJson(modelBuilder);

        modelBuilder.Entity<ContextJunkInJson.MyEntity>(b =>
        {
            b.ToTable("Entities");

            b.OwnsOne(x => x.Reference, b =>
            {
                b.ToJson().HasColumnType(JsonColumnType);
            });

            b.OwnsOne(x => x.ReferenceWithCtor, b =>
            {
                b.ToJson().HasColumnType(JsonColumnType);
            });

            b.OwnsMany(x => x.Collection, b =>
            {
                b.ToJson().HasColumnType(JsonColumnType);
            });

            b.OwnsMany(x => x.CollectionWithCtor, b =>
            {
                b.ToJson().HasColumnType(JsonColumnType);
            });
        });
    }

    #endregion

    #region TrickyBuffering

    protected override void OnModelCreatingTrickyBuffering(ModelBuilder modelBuilder)
    {
        base.OnModelCreatingTrickyBuffering(modelBuilder);

        modelBuilder.Entity<ContextTrickyBuffering.MyEntity>(b =>
        {
            b.ToTable("Entities");
            b.OwnsOne(x => x.Reference, b =>
            {
                b.ToJson().HasColumnType(JsonColumnType);
            });
        });
    }

    #endregion

    #region ShadowProperties

    protected override void OnModelCreatingShadowProperties(ModelBuilder modelBuilder)
    {
        base.OnModelCreatingShadowProperties(modelBuilder);

        modelBuilder.Entity<ContextShadowProperties.MyEntity>(b =>
        {
            b.ToTable("Entities");

            b.OwnsOne(x => x.Reference, b =>
            {
                b.ToJson().HasColumnType(JsonColumnType);
            });

            b.OwnsOne(x => x.ReferenceWithCtor, b =>
            {
                b.ToJson().HasColumnType(JsonColumnType);
                b.Property<int>("Shadow_Int").HasJsonPropertyName("ShadowInt");
            });

            b.OwnsMany(x => x.Collection, b =>
            {
                b.ToJson().HasColumnType(JsonColumnType);
            });

            b.OwnsMany(x => x.CollectionWithCtor, b =>
            {
                b.ToJson().HasColumnType(JsonColumnType);
            });
        });
    }

    #endregion

    #region LazyLoadingProxies

    protected override void OnModelCreatingLazyLoadingProxies(ModelBuilder modelBuilder)
    {
        base.OnModelCreatingLazyLoadingProxies(modelBuilder);

        modelBuilder.Entity<ContextLazyLoadingProxies.MyEntity>().OwnsOne(x => x.Reference, b => b.ToJson().HasColumnType(JsonColumnType));
        modelBuilder.Entity<ContextLazyLoadingProxies.MyEntity>().OwnsMany(x => x.Collection, b => b.ToJson().HasColumnType(JsonColumnType));
    }

    //protected void OnConfiguringLazyLoadingProxies(DbContextOptionsBuilder optionsBuilder)
    //    => optionsBuilder.UseLazyLoadingProxies();

    //protected IServiceCollection AddServicesLazyLoadingProxies(IServiceCollection addServices)
    //    => addServices.AddEntityFrameworkProxies();

    //private Task SeedLazyLoadingProxies(DbContext ctx)
    //{
    //    var r1 = new MyJsonEntityLazyLoadingProxiesWithCtor("r1", 1);
    //    var c11 = new MyJsonEntityLazyLoadingProxies { Name = "c11", Number = 11 };
    //    var c12 = new MyJsonEntityLazyLoadingProxies { Name = "c12", Number = 12 };
    //    var c13 = new MyJsonEntityLazyLoadingProxies { Name = "c13", Number = 13 };

    //    var r2 = new MyJsonEntityLazyLoadingProxiesWithCtor("r2", 2);
    //    var c21 = new MyJsonEntityLazyLoadingProxies { Name = "c21", Number = 21 };
    //    var c22 = new MyJsonEntityLazyLoadingProxies { Name = "c22", Number = 22 };

    //    var e1 = new MyEntityLazyLoadingProxies
    //    {
    //        Id = 1,
    //        Name = "e1",
    //        Reference = r1,
    //        Collection =
    //        [
    //            c11,
    //            c12,
    //            c13
    //        ]
    //    };

    //    var e2 = new MyEntityLazyLoadingProxies
    //    {
    //        Id = 2,
    //        Name = "e2",
    //        Reference = r2,
    //        Collection = [c21, c22]
    //    };

    //    ctx.Set<MyEntityLazyLoadingProxies>().AddRange(e1, e2);
    //    return ctx.SaveChangesAsync();
    //}

    #endregion

    #region NotICollection

    protected override void OnModelCreatingNotICollection(ModelBuilder modelBuilder)
    {
        base.OnModelCreatingNotICollection(modelBuilder);

        modelBuilder.Entity<ContextNotICollection.MyEntity>(b =>
        {
            b.ToTable("Entities");
            b.OwnsOne(cr => cr.Json, nb =>
            {
                nb.ToJson().HasColumnType(JsonColumnType);
            });
        });
    }

    #endregion

    #region BadJsonProperties

    public override async Task Bad_json_properties_duplicated_navigations(bool noTracking)
    {
        // tracking returns different results - see #35807
        if (noTracking)
        {
            await base.Bad_json_properties_duplicated_navigations(noTracking);
        }
    }

    public override Task Bad_json_properties_null_navigations(bool noTracking)
        => Assert.ThrowsAnyAsync<JsonException>(
            () => base.Bad_json_properties_null_navigations(noTracking));

    public override async Task Bad_json_properties_null_scalars(bool noTracking)
    {
        var message = (await Assert.ThrowsAnyAsync<JsonException>(
            () => base.Bad_json_properties_null_scalars(noTracking))).Message;

        Assert.StartsWith("'n' is an invalid start of a property name. Expected a '\"'.", message);
    }

    protected override void OnModelCreatingBadJsonProperties(ModelBuilder modelBuilder)
    {
        base.OnModelCreatingBadJsonProperties(modelBuilder);

        modelBuilder.Entity<ContextBadJsonProperties.Entity>(b =>
        {
            b.ToTable("Entities");

            b.OwnsOne(x => x.RequiredReference, b =>
            {
                b.ToJson().HasColumnType(JsonColumnType);
            });

            b.OwnsOne(x => x.OptionalReference, b =>
            {
                b.ToJson().HasColumnType(JsonColumnType);
            });

            b.OwnsMany(x => x.Collection, b =>
            {
                b.ToJson().HasColumnType(JsonColumnType);
            });
        });
    }

    #endregion

    protected TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected virtual string JsonColumnType
        => null;
}

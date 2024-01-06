// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Design.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal;

public class CSharpEntityTypeGeneratorTest(ModelCodeGeneratorTestFixture fixture, ITestOutputHelper output) : ModelCodeGeneratorTestBase(fixture, output)
{
    [ConditionalFact]
    public Task KeylessAttribute_is_generated_for_key_less_entity()
        => TestAsync(
            modelBuilder => modelBuilder.Entity("Vista").HasNoKey(),
            new ModelCodeGenerationOptions { UseDataAnnotations = true },
            code =>
            {
                AssertFileContents(
                    """
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

[Keyless]
public partial class Vista
{
}
""",
                    code.AdditionalFiles.Single(f => f.Path == "Vista.cs"));

                AssertFileContents(
                    $$"""
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class TestDbContext : DbContext
{
    public TestDbContext()
    {
    }

    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Vista> Vista { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning {{DesignStrings.SensitiveInformationWarning}}
        => optionsBuilder.UseSqlServer("Initial Catalog=TestDatabase");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
""",
                    code.ContextFile);
            },
            model =>
            {
                var entityType = model.FindEntityType("TestNamespace.Vista");
                Assert.Null(entityType.FindPrimaryKey());
            });

    [ConditionalFact]
    public Task TableAttribute_is_generated_for_custom_name()
        => TestAsync(
            modelBuilder =>
            {
                modelBuilder.Entity(
                    "Vista",
                    b =>
                    {
                        b.ToTable("Vistas"); // Default name is "Vista" in the absence of pluralizer
                        b.HasAnnotation(ScaffoldingAnnotationNames.DbSetName, "Vista");
                        b.Property<int>("Id");
                        b.HasKey("Id");
                    });
            },
            new ModelCodeGenerationOptions { UseDataAnnotations = true },
            code =>
            {
                AssertFileContents(
                    """
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

[Table("Vistas")]
public partial class Vista
{
    [Key]
    public int Id { get; set; }
}
""",
                    code.AdditionalFiles.Single(f => f.Path == "Vista.cs"));
            },
            model =>
            {
                var entityType = model.FindEntityType("TestNamespace.Vista");
                Assert.Equal("Vistas", entityType.GetTableName());
                Assert.Null(entityType.GetSchema());
            });

    [ConditionalFact]
    public Task TableAttribute_is_not_generated_for_default_schema()
        => TestAsync(
            modelBuilder =>
            {
                modelBuilder.HasDefaultSchema("dbo");
                modelBuilder.Entity(
                    "Vista",
                    b =>
                    {
                        b.ToTable("Vista", "dbo"); // Default name is "Vista" in the absence of pluralizer
                        b.Property<int>("Id");
                        b.HasKey("Id");
                    });
            },
            new ModelCodeGenerationOptions { UseDataAnnotations = true },
            code =>
            {
                AssertFileContents(
                    """
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class Vista
{
    [Key]
    public int Id { get; set; }
}
""",
                    code.AdditionalFiles.Single(f => f.Path == "Vista.cs"));
            },
            model =>
            {
                var entityType = model.FindEntityType("TestNamespace.Vista");
                Assert.Equal("Vista", entityType.GetTableName());
                Assert.Null(entityType.GetSchema()); // Takes through model default schema
            });

    [ConditionalFact]
    public Task TableAttribute_is_generated_for_non_default_schema()
        => TestAsync(
            modelBuilder =>
            {
                modelBuilder.HasDefaultSchema("dbo");
                modelBuilder.Entity(
                    "Vista",
                    b =>
                    {
                        b.ToTable("Vista", "custom");
                        b.Property<int>("Id");
                        b.HasKey("Id");
                    });
            },
            new ModelCodeGenerationOptions { UseDataAnnotations = true },
            code =>
            {
                AssertFileContents(
                    """
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

[Table("Vista", Schema = "custom")]
public partial class Vista
{
    [Key]
    public int Id { get; set; }
}
""",
                    code.AdditionalFiles.Single(f => f.Path == "Vista.cs"));
            },
            model =>
            {
                var entityType = model.FindEntityType("TestNamespace.Vista");
                Assert.Equal("Vista", entityType.GetTableName());
                Assert.Equal("custom", entityType.GetSchema());
            });

    [ConditionalFact]
    public Task TableAttribute_is_not_generated_for_views()
        => TestAsync(
            modelBuilder => modelBuilder.Entity("Vista").ToView("Vistas", "dbo"),
            new ModelCodeGenerationOptions { UseDataAnnotations = true },
            code =>
            {
                AssertFileContents(
                    """
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

[Keyless]
public partial class Vista
{
}
""",
                    code.AdditionalFiles.Single(f => f.Path == "Vista.cs"));
            },
            model =>
            {
                var entityType = model.FindEntityType("TestNamespace.Vista");
                Assert.Equal("Vistas", entityType.GetViewName());
                Assert.Null(entityType.GetTableName());
                Assert.Equal("dbo", entityType.GetViewSchema());
                Assert.Null(entityType.GetSchema());
            });

    [ConditionalFact]
    public Task IndexAttribute_is_generated_for_multiple_indexes_with_name_unique_descending()
        => TestAsync(
            modelBuilder => modelBuilder
                .Entity(
                    "EntityWithIndexes",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<int>("A");
                        x.Property<int>("B");
                        x.Property<int>("C");
                        x.HasKey("Id");
                        x.HasIndex(["A", "B"], "IndexOnAAndB")
                            .IsUnique()
                            .IsDescending(true, false);
                        x.HasIndex(["B", "C"], "IndexOnBAndC");
                        x.HasIndex("C");
                    }),
            new ModelCodeGenerationOptions { UseDataAnnotations = true },
            code =>
            {
                AssertFileContents(
                    """
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

[Index("C")]
[Index("A", "B", Name = "IndexOnAAndB", IsUnique = true, IsDescending = new[] { true, false })]
[Index("B", "C", Name = "IndexOnBAndC")]
public partial class EntityWithIndexes
{
    [Key]
    public int Id { get; set; }

    public int A { get; set; }

    public int B { get; set; }

    public int C { get; set; }
}
""",
                    code.AdditionalFiles.Single(f => f.Path == "EntityWithIndexes.cs"));
            },
            model =>
            {
                var entityType = model.FindEntityType("TestNamespace.EntityWithIndexes");
                var indexes = entityType.GetIndexes();
                Assert.Collection(
                    indexes,
                    t => Assert.Null(t.Name),
                    t => Assert.Equal("IndexOnAAndB", t.Name),
                    t => Assert.Equal("IndexOnBAndC", t.Name));
            });

    [ConditionalFact]
    public Task IndexAttribute_is_generated_with_ascending_descending()
        => TestAsync(
            modelBuilder => modelBuilder
                .Entity(
                    "EntityWithAscendingDescendingIndexes",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<int>("A");
                        x.Property<int>("B");
                        x.HasKey("Id");
                        x.HasIndex(["A", "B"], "AllAscending");
                        x.HasIndex(["A", "B"], "PartiallyDescending").IsDescending(true, false);
                        x.HasIndex(["A", "B"], "AllDescending").IsDescending();
                    }),
            new ModelCodeGenerationOptions { UseDataAnnotations = true },
            code =>
            {
                AssertFileContents(
                    """
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

[Index("A", "B", Name = "AllAscending")]
[Index("A", "B", Name = "AllDescending", AllDescending = true)]
[Index("A", "B", Name = "PartiallyDescending", IsDescending = new[] { true, false })]
public partial class EntityWithAscendingDescendingIndexes
{
    [Key]
    public int Id { get; set; }

    public int A { get; set; }

    public int B { get; set; }
}
""",
                    code.AdditionalFiles.Single(f => f.Path == "EntityWithAscendingDescendingIndexes.cs"));
            },
            model =>
            {
                var entityType = model.FindEntityType("TestNamespace.EntityWithAscendingDescendingIndexes");
                var indexes = entityType.GetIndexes();
                Assert.Collection(
                    indexes,
                    i =>
                    {
                        Assert.Equal("AllAscending", i.Name);
                        Assert.Null(i.IsDescending);
                    },
                    i =>
                    {
                        Assert.Equal("AllDescending", i.Name);
                        Assert.Equal([], i.IsDescending);
                    },
                    i =>
                    {
                        Assert.Equal("PartiallyDescending", i.Name);
                        Assert.Equal(new[] { true, false }, i.IsDescending);
                    });
            });

    [ConditionalFact]
    public Task Entity_with_indexes_generates_IndexAttribute_only_for_indexes_without_annotations()
        => TestAsync(
            modelBuilder => modelBuilder
                .Entity(
                    "EntityWithIndexes",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<int>("A");
                        x.Property<int>("B");
                        x.Property<int>("C");
                        x.HasKey("Id");
                        x.HasIndex(["A", "B"], "IndexOnAAndB")
                            .IsUnique();
                        x.HasIndex(["B", "C"], "IndexOnBAndC")
                            .HasFilter("Filter SQL");
                    }),
            new ModelCodeGenerationOptions { UseDataAnnotations = true },
            code =>
            {
                AssertFileContents(
                    """
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

[Index("A", "B", Name = "IndexOnAAndB", IsUnique = true)]
public partial class EntityWithIndexes
{
    [Key]
    public int Id { get; set; }

    public int A { get; set; }

    public int B { get; set; }

    public int C { get; set; }
}
""",
                    code.AdditionalFiles.Single(f => f.Path == "EntityWithIndexes.cs"));

                AssertFileContents(
                    $$"""
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class TestDbContext : DbContext
{
    public TestDbContext()
    {
    }

    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<EntityWithIndexes> EntityWithIndexes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning {{DesignStrings.SensitiveInformationWarning}}
        => optionsBuilder.UseSqlServer("Initial Catalog=TestDatabase");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EntityWithIndexes>(entity =>
        {
            entity.HasIndex(e => new { e.B, e.C }, "IndexOnBAndC").HasFilter("Filter SQL");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
""",
                    code.ContextFile);
            },
            model =>
                Assert.Equal(2, model.FindEntityType("TestNamespace.EntityWithIndexes").GetIndexes().Count()));

    [ConditionalFact]
    public Task KeyAttribute_is_generated_for_single_property_and_no_fluent_api()
        => TestAsync(
            modelBuilder => modelBuilder
                .Entity(
                    "Entity",
                    x =>
                    {
                        x.Property<int>("PrimaryKey");
                        x.HasKey("PrimaryKey");
                    }),
            new ModelCodeGenerationOptions { UseDataAnnotations = true },
            code =>
            {
                AssertFileContents(
                    """
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class Entity
{
    [Key]
    public int PrimaryKey { get; set; }
}
""",
                    code.AdditionalFiles.Single(f => f.Path == "Entity.cs"));

                AssertFileContents(
                    $$"""
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class TestDbContext : DbContext
{
    public TestDbContext()
    {
    }

    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Entity> Entity { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning {{DesignStrings.SensitiveInformationWarning}}
        => optionsBuilder.UseSqlServer("Initial Catalog=TestDatabase");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
""",
                    code.ContextFile);
            },
            model =>
                Assert.Equal("PrimaryKey", model.FindEntityType("TestNamespace.Entity").FindPrimaryKey().Properties[0].Name));

    [ConditionalFact]
    public Task KeyAttribute_is_generated_on_multiple_properties_but_and_uses_PrimaryKeyAttribute_for_composite_key()
        => TestAsync(
            modelBuilder => modelBuilder
                .Entity(
                    "Post",
                    x =>
                    {
                        x.Property<int>("Key");
                        x.Property<int>("Serial");
                        x.HasKey("Key", "Serial");
                    }),
            new ModelCodeGenerationOptions { UseDataAnnotations = true },
            code =>
            {
                AssertFileContents(
                    """
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

[PrimaryKey("Key", "Serial")]
public partial class Post
{
    [Key]
    public int Key { get; set; }

    [Key]
    public int Serial { get; set; }
}
""",
                    code.AdditionalFiles.Single(f => f.Path == "Post.cs"));

                AssertFileContents(
                    $$"""
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class TestDbContext : DbContext
{
    public TestDbContext()
    {
    }

    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Post> Post { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning {{DesignStrings.SensitiveInformationWarning}}
        => optionsBuilder.UseSqlServer("Initial Catalog=TestDatabase");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
""",
                    code.ContextFile);
            },
            model =>
            {
                var postType = model.FindEntityType("TestNamespace.Post");
                Assert.Equal(new[] { "Key", "Serial" }, postType.FindPrimaryKey().Properties.Select(p => p.Name));
            });

    [ConditionalFact]
    public Task Required_and_not_required_properties_without_nrt()
        => TestAsync(
            modelBuilder => modelBuilder
                .Entity(
                    "Entity",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<string>("RequiredString").IsRequired();
                        x.Property<string>("NonRequiredString");
                        x.Property<int>("RequiredInt");
                        x.Property<int?>("NonRequiredInt");
                    }),
            new ModelCodeGenerationOptions { UseDataAnnotations = true },
            code =>
            {
                AssertFileContents(
                    """
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class Entity
{
    [Key]
    public int Id { get; set; }

    public int? NonRequiredInt { get; set; }

    public string NonRequiredString { get; set; }

    public int RequiredInt { get; set; }

    [Required]
    public string RequiredString { get; set; }
}
""",
                    code.AdditionalFiles.Single(f => f.Path == "Entity.cs"));
            },
            model =>
            {
                var entityType = model.FindEntityType("TestNamespace.Entity");
                Assert.False(entityType.GetProperty("RequiredString").IsNullable);
                Assert.True(entityType.GetProperty("NonRequiredString").IsNullable);
                Assert.False(entityType.GetProperty("RequiredInt").IsNullable);
                Assert.True(entityType.GetProperty("NonRequiredInt").IsNullable);
            });

    [ConditionalFact]
    public Task Required_and_not_required_properties_with_nrt()
        => TestAsync(
            modelBuilder => modelBuilder
                .Entity(
                    "Entity",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<string>("RequiredString").IsRequired();
                        x.Property<string>("NonRequiredString");
                        x.Property<int>("RequiredInt");
                        x.Property<int?>("NonRequiredInt");
                    }),
            new ModelCodeGenerationOptions { UseDataAnnotations = true, UseNullableReferenceTypes = true },
            code =>
            {
                AssertFileContents(
                    """
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class Entity
{
    [Key]
    public int Id { get; set; }

    public int? NonRequiredInt { get; set; }

    public string? NonRequiredString { get; set; }

    public int RequiredInt { get; set; }

    public string RequiredString { get; set; } = null!;
}
""",
                    code.AdditionalFiles.Single(f => f.Path == "Entity.cs"));
            },
            model =>
            {
                var entityType = model.FindEntityType("TestNamespace.Entity");
                Assert.False(entityType.GetProperty("RequiredString").IsNullable);
                Assert.True(entityType.GetProperty("NonRequiredString").IsNullable);
                Assert.False(entityType.GetProperty("RequiredInt").IsNullable);
                Assert.True(entityType.GetProperty("NonRequiredInt").IsNullable);
            });

    [ConditionalFact]
    public Task Required_and_not_required_navigations_without_nrt()
        => TestAsync(
            modelBuilder => modelBuilder
                .Entity(
                    "Entity",
                    x =>
                    {
                        x.Property<int>("Id");

                        x.HasOne("Dependent1", "RequiredReferenceNavigation").WithMany("Entity").IsRequired();
                        x.HasOne("Dependent2", "OptionalReferenceNavigation").WithMany("Entity");
                        x.HasOne("Dependent3", "RequiredValueNavigation").WithMany("Entity").IsRequired();
                        x.HasOne("Dependent4", "OptionalValueNavigation").WithMany("Entity");
                    })
                .Entity("Dependent1", x => x.Property<string>("Id"))
                .Entity("Dependent2", x => x.Property<string>("Id"))
                .Entity("Dependent3", x => x.Property<int>("Id"))
                .Entity("Dependent4", x => x.Property<int>("Id")),
            new ModelCodeGenerationOptions { UseDataAnnotations = true },
            code =>
            {
                AssertFileContents(
                    """
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class Entity
{
    [Key]
    public int Id { get; set; }

    public string OptionalReferenceNavigationId { get; set; }

    public int? OptionalValueNavigationId { get; set; }

    [Required]
    public string RequiredReferenceNavigationId { get; set; }

    public int RequiredValueNavigationId { get; set; }

    [ForeignKey("OptionalReferenceNavigationId")]
    [InverseProperty("Entity")]
    public virtual Dependent2 OptionalReferenceNavigation { get; set; }

    [ForeignKey("OptionalValueNavigationId")]
    [InverseProperty("Entity")]
    public virtual Dependent4 OptionalValueNavigation { get; set; }

    [ForeignKey("RequiredReferenceNavigationId")]
    [InverseProperty("Entity")]
    public virtual Dependent1 RequiredReferenceNavigation { get; set; }

    [ForeignKey("RequiredValueNavigationId")]
    [InverseProperty("Entity")]
    public virtual Dependent3 RequiredValueNavigation { get; set; }
}
""",
                    code.AdditionalFiles.Single(f => f.Path == "Entity.cs"));
            },
            model =>
            {
                var entityType = model.FindEntityType("TestNamespace.Entity");

                Assert.False(entityType.GetProperty("RequiredReferenceNavigationId").IsNullable);
                Assert.True(entityType.GetProperty("OptionalReferenceNavigationId").IsNullable);
                Assert.False(entityType.GetProperty("RequiredValueNavigationId").IsNullable);
                Assert.True(entityType.GetProperty("OptionalValueNavigationId").IsNullable);

                Assert.True(entityType.FindNavigation("RequiredReferenceNavigation")!.ForeignKey.IsRequired);
                Assert.False(entityType.FindNavigation("OptionalReferenceNavigation")!.ForeignKey.IsRequired);
                Assert.True(entityType.FindNavigation("RequiredValueNavigation")!.ForeignKey.IsRequired);
                Assert.False(entityType.FindNavigation("OptionalValueNavigation")!.ForeignKey.IsRequired);
            });

    [ConditionalFact]
    public Task Required_and_not_required_reference_navigations_with_nrt()
        => TestAsync(
            modelBuilder => modelBuilder
                .Entity(
                    "Entity",
                    x =>
                    {
                        x.Property<int>("Id");

                        x
                            .HasOne("Dependent1", "RequiredNavigationWithReferenceForeignKey")
                            .WithOne("Entity")
                            .HasForeignKey("Dependent1", "RequiredNavigationWithReferenceForeignKey")
                            .IsRequired();

                        x
                            .HasOne("Dependent2", "OptionalNavigationWithReferenceForeignKey")
                            .WithOne("Entity")
                            .HasForeignKey("Dependent2", "OptionalNavigationWithReferenceForeignKey");

                        x
                            .HasOne("Dependent3", "RequiredNavigationWithValueForeignKey")
                            .WithOne("Entity")
                            .HasForeignKey("Dependent3", "RequiredNavigationWithValueForeignKey")
                            .IsRequired();

                        x
                            .HasOne("Dependent4", "OptionalNavigationWithValueForeignKey")
                            .WithOne("Entity")
                            .HasForeignKey("Dependent4", "OptionalNavigationWithValueForeignKey");
                    })
                .Entity("Dependent1", x => x.Property<string>("Id"))
                .Entity("Dependent2", x => x.Property<string>("Id"))
                .Entity("Dependent3", x => x.Property<int>("Id"))
                .Entity("Dependent4", x => x.Property<int>("Id")),
            new ModelCodeGenerationOptions { UseDataAnnotations = true, UseNullableReferenceTypes = true },
            code =>
            {
                AssertFileContents(
                    """
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class Entity
{
    [Key]
    public int Id { get; set; }

    [InverseProperty("Entity")]
    public virtual Dependent2? OptionalNavigationWithReferenceForeignKey { get; set; }

    [InverseProperty("Entity")]
    public virtual Dependent4? OptionalNavigationWithValueForeignKey { get; set; }

    [InverseProperty("Entity")]
    public virtual Dependent1? RequiredNavigationWithReferenceForeignKey { get; set; }

    [InverseProperty("Entity")]
    public virtual Dependent3? RequiredNavigationWithValueForeignKey { get; set; }
}
""",
                    code.AdditionalFiles.Single(f => f.Path == "Entity.cs"));
            },
            model =>
            {
                var entityType = model.FindEntityType("TestNamespace.Entity");

                Assert.True(entityType.FindNavigation("RequiredNavigationWithReferenceForeignKey")!.ForeignKey.IsRequired);
                Assert.False(entityType.FindNavigation("OptionalNavigationWithReferenceForeignKey")!.ForeignKey.IsRequired);
                Assert.True(entityType.FindNavigation("RequiredNavigationWithValueForeignKey")!.ForeignKey.IsRequired);
                Assert.False(entityType.FindNavigation("OptionalNavigationWithValueForeignKey")!.ForeignKey.IsRequired);
            });

    [ConditionalFact]
    public Task Required_and_not_required_collection_navigations_with_nrt()
        => TestAsync(
            modelBuilder => modelBuilder
                .Entity(
                    "Entity",
                    x =>
                    {
                        x.Property<int>("Id");

                        x.HasOne("Dependent1", "RequiredNavigationWithReferenceForeignKey").WithMany("Entity").IsRequired();
                        x.HasOne("Dependent2", "OptionalNavigationWithReferenceForeignKey").WithMany("Entity");
                        x.HasOne("Dependent3", "RequiredNavigationWithValueForeignKey").WithMany("Entity").IsRequired();
                        x.HasOne("Dependent4", "OptionalNavigationWithValueForeignKey").WithMany("Entity");
                    })
                .Entity("Dependent1", x => x.Property<string>("Id"))
                .Entity("Dependent2", x => x.Property<string>("Id"))
                .Entity("Dependent3", x => x.Property<int>("Id"))
                .Entity("Dependent4", x => x.Property<int>("Id")),
            new ModelCodeGenerationOptions { UseDataAnnotations = true, UseNullableReferenceTypes = true },
            code =>
            {
                AssertFileContents(
                    """
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class Entity
{
    [Key]
    public int Id { get; set; }

    public string? OptionalNavigationWithReferenceForeignKeyId { get; set; }

    public int? OptionalNavigationWithValueForeignKeyId { get; set; }

    public string RequiredNavigationWithReferenceForeignKeyId { get; set; } = null!;

    public int RequiredNavigationWithValueForeignKeyId { get; set; }

    [ForeignKey("OptionalNavigationWithReferenceForeignKeyId")]
    [InverseProperty("Entity")]
    public virtual Dependent2? OptionalNavigationWithReferenceForeignKey { get; set; }

    [ForeignKey("OptionalNavigationWithValueForeignKeyId")]
    [InverseProperty("Entity")]
    public virtual Dependent4? OptionalNavigationWithValueForeignKey { get; set; }

    [ForeignKey("RequiredNavigationWithReferenceForeignKeyId")]
    [InverseProperty("Entity")]
    public virtual Dependent1 RequiredNavigationWithReferenceForeignKey { get; set; } = null!;

    [ForeignKey("RequiredNavigationWithValueForeignKeyId")]
    [InverseProperty("Entity")]
    public virtual Dependent3 RequiredNavigationWithValueForeignKey { get; set; } = null!;
}
""",
                    code.AdditionalFiles.Single(f => f.Path == "Entity.cs"));

                for (var i = 1; i <= 4; i++)
                {
                    Assert.Contains(
                        "public virtual ICollection<Entity> Entity { get; set; }",
                        code.AdditionalFiles.Single(f => f.Path == $"Dependent{i}.cs").Code);
                }
            },
            model =>
            {
                var entityType = model.FindEntityType("TestNamespace.Entity");

                Assert.False(entityType.GetProperty("RequiredNavigationWithReferenceForeignKeyId").IsNullable);
                Assert.True(entityType.GetProperty("OptionalNavigationWithReferenceForeignKeyId").IsNullable);
                Assert.False(entityType.GetProperty("RequiredNavigationWithValueForeignKeyId").IsNullable);
                Assert.True(entityType.GetProperty("OptionalNavigationWithValueForeignKeyId").IsNullable);

                Assert.True(entityType.FindNavigation("RequiredNavigationWithReferenceForeignKey")!.ForeignKey.IsRequired);
                Assert.False(entityType.FindNavigation("OptionalNavigationWithReferenceForeignKey")!.ForeignKey.IsRequired);
                Assert.True(entityType.FindNavigation("RequiredNavigationWithValueForeignKey")!.ForeignKey.IsRequired);
                Assert.False(entityType.FindNavigation("OptionalNavigationWithValueForeignKey")!.ForeignKey.IsRequired);
            });

    [ConditionalFact]
    public Task RequiredAttribute_is_not_generated_for_key_property()
        => TestAsync(
            modelBuilder => modelBuilder
                .Entity(
                    "Entity",
                    x =>
                    {
                        x.Property<string>("RequiredString");
                        x.HasKey("RequiredString");
                    }),
            new ModelCodeGenerationOptions { UseDataAnnotations = true },
            code =>
            {
                AssertFileContents(
                    """
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class Entity
{
    [Key]
    public string RequiredString { get; set; }
}
""",
                    code.AdditionalFiles.Single(f => f.Path == "Entity.cs"));
            },
            model =>
                Assert.False(model.FindEntityType("TestNamespace.Entity").GetProperty("RequiredString").IsNullable));

    [ConditionalFact]
    public Task ColumnAttribute_is_generated_for_property()
        => TestAsync(
            modelBuilder => modelBuilder
                .Entity(
                    "Entity",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<string>("A").HasColumnName("propertyA");
                        x.Property<string>("B").HasColumnType("nchar(10)");
                        x.Property<string>("C").HasColumnName("random").HasColumnType("varchar(200)");
                        x.Property<decimal>("D").HasColumnType("numeric(18, 2)");
                        x.Property<string>("E").HasMaxLength(100);
                    }),
            new ModelCodeGenerationOptions { UseDataAnnotations = true },
            code =>
            {
                AssertFileContents(
                    """
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class Entity
{
    [Key]
    public int Id { get; set; }

    [Column("propertyA")]
    public string A { get; set; }

    [Column(TypeName = "nchar(10)")]
    public string B { get; set; }

    [Column("random", TypeName = "varchar(200)")]
    public string C { get; set; }

    [Column(TypeName = "numeric(18, 2)")]
    public decimal D { get; set; }

    [StringLength(100)]
    public string E { get; set; }
}
""",
                    code.AdditionalFiles.Single(f => f.Path == "Entity.cs"));

                AssertFileContents(
                    $$"""
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class TestDbContext : DbContext
{
    public TestDbContext()
    {
    }

    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Entity> Entity { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning {{DesignStrings.SensitiveInformationWarning}}
        => optionsBuilder.UseSqlServer("Initial Catalog=TestDatabase");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
""",
                    code.ContextFile);
            },
            model =>
            {
                var entitType = model.FindEntityType("TestNamespace.Entity");
                Assert.Equal("propertyA", entitType.GetProperty("A").GetColumnName());
                Assert.Equal("nchar(10)", entitType.GetProperty("B").GetColumnType());
                Assert.Equal("random", entitType.GetProperty("C").GetColumnName());
                Assert.Equal("varchar(200)", entitType.GetProperty("C").GetColumnType());
            });

    [ConditionalFact]
    public Task MaxLengthAttribute_is_generated_for_property()
        => TestAsync(
            modelBuilder => modelBuilder
                .Entity(
                    "Entity",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<string>("A").HasMaxLength(34);
                        x.Property<byte[]>("B").HasMaxLength(10);
                    }),
            new ModelCodeGenerationOptions { UseDataAnnotations = true },
            code =>
            {
                AssertFileContents(
                    """
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class Entity
{
    [Key]
    public int Id { get; set; }

    [StringLength(34)]
    public string A { get; set; }

    [MaxLength(10)]
    public byte[] B { get; set; }
}
""",
                    code.AdditionalFiles.Single(f => f.Path == "Entity.cs"));
            },
            model =>
            {
                var entitType = model.FindEntityType("TestNamespace.Entity");
                Assert.Equal(34, entitType.GetProperty("A").GetMaxLength());
                Assert.Equal(10, entitType.GetProperty("B").GetMaxLength());
            });

    [ConditionalFact]
    public Task UnicodeAttribute_is_generated_for_property()
        => TestAsync(
            modelBuilder => modelBuilder
                .Entity(
                    "Entity",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<string>("A").HasMaxLength(34).IsUnicode();
                        x.Property<string>("B").HasMaxLength(34).IsUnicode(false);
                        x.Property<string>("C").HasMaxLength(34);
                    }),
            new ModelCodeGenerationOptions { UseDataAnnotations = true },
            code =>
            {
                AssertFileContents(
                    """
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class Entity
{
    [Key]
    public int Id { get; set; }

    [StringLength(34)]
    [Unicode]
    public string A { get; set; }

    [StringLength(34)]
    [Unicode(false)]
    public string B { get; set; }

    [StringLength(34)]
    public string C { get; set; }
}
""",
                    code.AdditionalFiles.Single(f => f.Path == "Entity.cs"));
            },
            model =>
            {
                var entitType = model.FindEntityType("TestNamespace.Entity");
                Assert.True(entitType.GetProperty("A").IsUnicode());
                Assert.False(entitType.GetProperty("B").IsUnicode());
                Assert.Null(entitType.GetProperty("C").IsUnicode());
            });

    [ConditionalFact]
    public Task PrecisionAttribute_is_generated_for_property()
        => TestAsync(
            modelBuilder => modelBuilder
                .Entity(
                    "Entity",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<decimal>("A").HasPrecision(10);
                        x.Property<decimal>("B").HasPrecision(14, 3);
                        x.Property<DateTime>("C").HasPrecision(5);
                        x.Property<DateTimeOffset>("D").HasPrecision(3);
                    }),
            new ModelCodeGenerationOptions { UseDataAnnotations = true },
            code =>
            {
                AssertFileContents(
                    """
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class Entity
{
    [Key]
    public int Id { get; set; }

    [Precision(10)]
    public decimal A { get; set; }

    [Precision(14, 3)]
    public decimal B { get; set; }

    [Precision(5)]
    public DateTime C { get; set; }

    [Precision(3)]
    public DateTimeOffset D { get; set; }
}
""",
                    code.AdditionalFiles.Single(f => f.Path == "Entity.cs"));
            },
            model =>
            {
                var entitType = model.FindEntityType("TestNamespace.Entity");
                Assert.Equal(10, entitType.GetProperty("A").GetPrecision());
                Assert.Equal(14, entitType.GetProperty("B").GetPrecision());
                Assert.Equal(3, entitType.GetProperty("B").GetScale());
                Assert.Equal(5, entitType.GetProperty("C").GetPrecision());
                Assert.Equal(3, entitType.GetProperty("D").GetPrecision());
            });

    [ConditionalFact]
    public Task Comments_are_generated()
        => TestAsync(
            modelBuilder => modelBuilder
                .Entity(
                    "Entity",
                    x =>
                    {
                        x.ToTable(tb => tb.HasComment("Entity Comment"));
                        x.Property<int>("Id").HasComment("Property Comment");
                    })
            ,
            new ModelCodeGenerationOptions { UseDataAnnotations = true },
            code =>
            {
                AssertFileContents(
                    """
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

/// <summary>
/// Entity Comment
/// </summary>
public partial class Entity
{
    /// <summary>
    /// Property Comment
    /// </summary>
    [Key]
    public int Id { get; set; }
}
""",
                    code.AdditionalFiles.Single(f => f.Path == "Entity.cs"));
            },
            model => { });

    [ConditionalFact]
    public Task Comments_complex_are_generated()
        => TestAsync(
            modelBuilder => modelBuilder
                .Entity(
                    "Entity",
                    x =>
                    {
                        x.ToTable(
                            tb => tb.HasComment(
                                """
Entity Comment
On multiple lines
With XML content <br/>
"""));
                        x.Property<int>("Id").HasComment(
                            """
Property Comment
On multiple lines
With XML content <br/>
""");
                    })
            ,
            new ModelCodeGenerationOptions { UseDataAnnotations = true },
            code =>
            {
                AssertFileContents(
                    """
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

/// <summary>
/// Entity Comment
/// On multiple lines
/// With XML content &lt;br/&gt;
/// </summary>
public partial class Entity
{
    /// <summary>
    /// Property Comment
    /// On multiple lines
    /// With XML content &lt;br/&gt;
    /// </summary>
    [Key]
    public int Id { get; set; }
}
""",
                    code.AdditionalFiles.Single(f => f.Path == "Entity.cs"));
            },
            model => { });

    [ConditionalFact]
    public Task Properties_are_sorted_in_order_of_definition_in_table()
        => TestAsync(
            modelBuilder => modelBuilder
                .Entity(
                    "Entity",
                    x =>
                    {
                        // Order would be PK first and then rest alphabetically since they are all shadow
                        x.Property<int>("Id");
                        x.Property<string>("LastProperty");
                        x.Property<string>("FirstProperty");
                    }),
            new ModelCodeGenerationOptions { UseDataAnnotations = true },
            code =>
            {
                AssertFileContents(
                    """
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class Entity
{
    [Key]
    public int Id { get; set; }

    public string FirstProperty { get; set; }

    public string LastProperty { get; set; }
}
""",
                    code.AdditionalFiles.Single(f => f.Path == "Entity.cs"));
            },
            model => { });

    [ConditionalFact]
    public Task Navigation_properties_are_sorted_after_properties_and_collection_are_initialized_in_ctor()
        => TestAsync(
            modelBuilder => modelBuilder
                .Entity(
                    "Person",
                    x => x.Property<int>("Id"))
                .Entity(
                    "Contribution",
                    x => x.Property<int>("Id"))
                .Entity(
                    "Post",
                    x =>
                    {
                        x.Property<int>("Id");

                        x.HasOne("Person", "Author").WithMany("Posts");
                        x.HasMany("Contribution", "Contributions").WithOne("Post");
                    }),
            new ModelCodeGenerationOptions { UseDataAnnotations = true },
            code =>
            {
                AssertFileContents(
                    """
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class Post
{
    [Key]
    public int Id { get; set; }

    public int? AuthorId { get; set; }

    [ForeignKey("AuthorId")]
    [InverseProperty("Posts")]
    public virtual Person Author { get; set; }

    [InverseProperty("Post")]
    public virtual ICollection<Contribution> Contributions { get; set; } = new List<Contribution>();
}
""",
                    code.AdditionalFiles.Single(f => f.Path == "Post.cs"));

                AssertFileContents(
                    """
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class Person
{
    [Key]
    public int Id { get; set; }

    [InverseProperty("Author")]
    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
}
""",
                    code.AdditionalFiles.Single(f => f.Path == "Person.cs"));
            },
            model =>
            {
                var postType = model.FindEntityType("TestNamespace.Post");
                var authorNavigation = postType.FindNavigation("Author");
                Assert.True(authorNavigation.IsOnDependent);
                Assert.Equal("TestNamespace.Person", authorNavigation.ForeignKey.PrincipalEntityType.Name);

                var contributionsNav = postType.FindNavigation("Contributions");
                Assert.False(contributionsNav.IsOnDependent);
                Assert.Equal("TestNamespace.Contribution", contributionsNav.ForeignKey.DeclaringEntityType.Name);
            });

    [ConditionalFact]
    public Task ForeignKeyAttribute_is_generated_for_composite_fk()
        => TestAsync(
            modelBuilder => modelBuilder
                .Entity(
                    "Blog",
                    x =>
                    {
                        x.Property<int>("Id1");
                        x.Property<int>("Id2");
                        x.HasKey("Id1", "Id2");
                    })
                .Entity(
                    "Post",
                    x =>
                    {
                        x.Property<int>("Id");

                        x.HasOne("Blog", "BlogNavigation").WithMany("Posts").HasForeignKey("BlogId1", "BlogId2");
                    }),
            new ModelCodeGenerationOptions { UseDataAnnotations = true },
            code =>
            {
                AssertFileContents(
                    """
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class Post
{
    [Key]
    public int Id { get; set; }

    public int? BlogId1 { get; set; }

    public int? BlogId2 { get; set; }

    [ForeignKey("BlogId1, BlogId2")]
    [InverseProperty("Posts")]
    public virtual Blog BlogNavigation { get; set; }
}
""",
                    code.AdditionalFiles.Single(f => f.Path == "Post.cs"));

                AssertFileContents(
                    $$"""
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class TestDbContext : DbContext
{
    public TestDbContext()
    {
    }

    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Blog> Blog { get; set; }

    public virtual DbSet<Post> Post { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning {{DesignStrings.SensitiveInformationWarning}}
        => optionsBuilder.UseSqlServer("Initial Catalog=TestDatabase");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
""",
                    code.ContextFile);
            },
            model =>
            {
                var postType = model.FindEntityType("TestNamespace.Post");
                var blogNavigation = postType.FindNavigation("BlogNavigation");
                Assert.Equal("TestNamespace.Blog", blogNavigation.ForeignKey.PrincipalEntityType.Name);
                Assert.Equal(new[] { "BlogId1", "BlogId2" }, blogNavigation.ForeignKey.Properties.Select(p => p.Name));
            });

    [ConditionalFact]
    public Task ForeignKeyAttribute_InversePropertyAttribute_when_composite_alternate_key()
        => TestAsync(
            modelBuilder => modelBuilder
                .Entity(
                    "Blog",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<int>("Id1");
                        x.Property<int>("Id2");
                    })
                .Entity(
                    "Post",
                    x =>
                    {
                        x.Property<int>("Id");

                        x.HasOne("Blog", "BlogNavigation").WithMany("Posts")
                            .HasPrincipalKey("Id1", "Id2")
                            .HasForeignKey("BlogId1", "BlogId2");
                    }),
            new ModelCodeGenerationOptions { UseDataAnnotations = true },
            code =>
            {
                AssertFileContents(
                    """
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class Post
{
    [Key]
    public int Id { get; set; }

    public int? BlogId1 { get; set; }

    public int? BlogId2 { get; set; }

    [ForeignKey("BlogId1, BlogId2")]
    [InverseProperty("Posts")]
    public virtual Blog BlogNavigation { get; set; }
}
""",
                    code.AdditionalFiles.Single(f => f.Path == "Post.cs"));

                AssertFileContents(
                    $$"""
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class TestDbContext : DbContext
{
    public TestDbContext()
    {
    }

    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Blog> Blog { get; set; }

    public virtual DbSet<Post> Post { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning {{DesignStrings.SensitiveInformationWarning}}
        => optionsBuilder.UseSqlServer("Initial Catalog=TestDatabase");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasOne(d => d.BlogNavigation).WithMany(p => p.Posts)
                .HasPrincipalKey(p => new { p.Id1, p.Id2 })
                .HasForeignKey(d => new { d.BlogId1, d.BlogId2 });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
""",
                    code.ContextFile);
            },
            model =>
            {
                var postType = model.FindEntityType("TestNamespace.Post");
                var blogNavigation = postType.FindNavigation("BlogNavigation");
                Assert.Equal("TestNamespace.Blog", blogNavigation.ForeignKey.PrincipalEntityType.Name);
                Assert.Equal(new[] { "BlogId1", "BlogId2" }, blogNavigation.ForeignKey.Properties.Select(p => p.Name));
                Assert.Equal(new[] { "Id1", "Id2" }, blogNavigation.ForeignKey.PrincipalKey.Properties.Select(p => p.Name));
            });

    [ConditionalFact]
    public Task ForeignKeyAttribute_is_generated_for_fk_referencing_ak()
        => TestAsync(
            modelBuilder => modelBuilder
                .Entity(
                    "Color",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<string>("ColorCode");
                    })
                .Entity(
                    "Car",
                    x =>
                    {
                        x.Property<int>("Id");

                        x.HasOne("Color", "Color").WithMany("Cars")
                            .HasPrincipalKey("ColorCode")
                            .HasForeignKey("ColorCode");
                    }),
            new ModelCodeGenerationOptions { UseDataAnnotations = true, UseNullableReferenceTypes = true },
            code =>
            {
                AssertFileContents(
                    """
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class Color
{
    [Key]
    public int Id { get; set; }

    public string ColorCode { get; set; } = null!;

    [InverseProperty("Color")]
    public virtual ICollection<Car> Cars { get; set; } = new List<Car>();
}
""",
                    code.AdditionalFiles.Single(f => f.Path == "Color.cs"));

                AssertFileContents(
                    """
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class Car
{
    [Key]
    public int Id { get; set; }

    public string? ColorCode { get; set; }

    [ForeignKey("ColorCode")]
    [InverseProperty("Cars")]
    public virtual Color? Color { get; set; }
}
""",
                    code.AdditionalFiles.Single(f => f.Path == "Car.cs"));

                AssertFileContents(
                    $$"""
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class TestDbContext : DbContext
{
    public TestDbContext()
    {
    }

    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Car> Car { get; set; }

    public virtual DbSet<Color> Color { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning {{DesignStrings.SensitiveInformationWarning}}
        => optionsBuilder.UseSqlServer("Initial Catalog=TestDatabase");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Car>(entity =>
        {
            entity.HasOne(d => d.Color).WithMany(p => p.Cars)
                .HasPrincipalKey(p => p.ColorCode)
                .HasForeignKey(d => d.ColorCode);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
""",
                    code.ContextFile);
            },
            model =>
            {
                var carType = model.FindEntityType("TestNamespace.Car");
                var colorNavigation = carType.FindNavigation("Color");
                Assert.Equal("TestNamespace.Color", colorNavigation.ForeignKey.PrincipalEntityType.Name);
                Assert.Equal(new[] { "ColorCode" }, colorNavigation.ForeignKey.Properties.Select(p => p.Name));
                Assert.Equal(new[] { "ColorCode" }, colorNavigation.ForeignKey.PrincipalKey.Properties.Select(p => p.Name));
            });

    [ConditionalFact]
    public Task Foreign_key_from_keyless_table()
        => TestAsync(
            modelBuilder => modelBuilder
                .Entity("Blog", x => x.Property<int>("Id"))
                .Entity("Post", x => x.HasOne("Blog", "Blog").WithMany()),
            new ModelCodeGenerationOptions(),
            code =>
            {
                AssertFileContents(
                    $$"""
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class TestDbContext : DbContext
{
    public TestDbContext()
    {
    }

    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Blog> Blog { get; set; }

    public virtual DbSet<Post> Post { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning {{DesignStrings.SensitiveInformationWarning}}
        => optionsBuilder.UseSqlServer("Initial Catalog=TestDatabase");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasNoKey();

            entity.HasIndex(e => e.BlogId, "IX_Post_BlogId");

            entity.HasOne(d => d.Blog).WithMany().HasForeignKey(d => d.BlogId);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
""",
                    code.ContextFile);

                AssertFileContents(
                    """
using System;
using System.Collections.Generic;

namespace TestNamespace;

public partial class Blog
{
    public int Id { get; set; }
}
""",
                    code.AdditionalFiles.First(f => f.Path == "Blog.cs"));

                AssertFileContents(
                    """
using System;
using System.Collections.Generic;

namespace TestNamespace;

public partial class Post
{
    public int? BlogId { get; set; }

    public virtual Blog Blog { get; set; }
}
""",
                    code.AdditionalFiles.First(f => f.Path == "Post.cs"));
            },
            model =>
            {
                var post = model.FindEntityType("TestNamespace.Post");
                var foreignKey = Assert.Single(post.GetForeignKeys());
                Assert.Equal("Blog", foreignKey.DependentToPrincipal.Name);
                Assert.Null(foreignKey.PrincipalToDependent);
            });

    [ConditionalFact]
    public Task InverseProperty_when_navigation_property_with_same_type_and_navigation_name()
        => TestAsync(
            modelBuilder => modelBuilder
                .Entity(
                    "Blog",
                    x => x.Property<int>("Id"))
                .Entity(
                    "Post",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.HasOne("Blog", "Blog").WithMany("Posts");
                    }),
            new ModelCodeGenerationOptions { UseDataAnnotations = true },
            code =>
            {
                AssertFileContents(
                    """
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class Post
{
    [Key]
    public int Id { get; set; }

    public int? BlogId { get; set; }

    [ForeignKey("BlogId")]
    [InverseProperty("Posts")]
    public virtual Blog Blog { get; set; }
}
""",
                    code.AdditionalFiles.Single(f => f.Path == "Post.cs"));
            },
            model =>
            {
                var postType = model.FindEntityType("TestNamespace.Post");
                var blogNavigation = postType.FindNavigation("Blog");

                var foreignKeyProperty = Assert.Single(blogNavigation.ForeignKey.Properties);
                Assert.Equal("BlogId", foreignKeyProperty.Name);

                var inverseNavigation = blogNavigation.Inverse;
                Assert.Equal("TestNamespace.Blog", inverseNavigation.DeclaringEntityType.Name);
                Assert.Equal("Posts", inverseNavigation.Name);
            });

    [ConditionalFact]
    public Task InverseProperty_when_navigation_property_with_same_type_and_property_name()
        => TestAsync(
            modelBuilder => modelBuilder
                .Entity(
                    "Blog",
                    x => x.Property<int>("Id"))
                .Entity(
                    "Post",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.HasOne("Blog", "BlogNavigation").WithMany("Posts").HasForeignKey("Blog");
                    }),
            new ModelCodeGenerationOptions { UseDataAnnotations = true },
            code =>
            {
                AssertFileContents(
                    """
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class Post
{
    [Key]
    public int Id { get; set; }

    public int? Blog { get; set; }

    [ForeignKey("Blog")]
    [InverseProperty("Posts")]
    public virtual Blog BlogNavigation { get; set; }
}
""",
                    code.AdditionalFiles.Single(f => f.Path == "Post.cs"));
            },
            model =>
            {
                var postType = model.FindEntityType("TestNamespace.Post");
                var blogNavigation = postType.FindNavigation("BlogNavigation");

                var foreignKeyProperty = Assert.Single(blogNavigation.ForeignKey.Properties);
                Assert.Equal("Blog", foreignKeyProperty.Name);

                var inverseNavigation = blogNavigation.Inverse;
                Assert.Equal("TestNamespace.Blog", inverseNavigation.DeclaringEntityType.Name);
                Assert.Equal("Posts", inverseNavigation.Name);
            });

    [ConditionalFact]
    public Task InverseProperty_when_navigation_property_with_same_type_and_other_navigation_name()
        => TestAsync(
            modelBuilder => modelBuilder
                .Entity(
                    "Blog",
                    x => x.Property<int>("Id"))
                .Entity(
                    "Post",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.HasOne("Blog", "Blog").WithMany("Posts");
                        x.HasOne("Blog", "OriginalBlog").WithMany("OriginalPosts").HasForeignKey("OriginalBlogId");
                    }),
            new ModelCodeGenerationOptions { UseDataAnnotations = true },
            code =>
            {
                AssertFileContents(
                    """
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class Post
{
    [Key]
    public int Id { get; set; }

    public int? BlogId { get; set; }

    public int? OriginalBlogId { get; set; }

    [ForeignKey("BlogId")]
    [InverseProperty("Posts")]
    public virtual Blog Blog { get; set; }

    [ForeignKey("OriginalBlogId")]
    [InverseProperty("OriginalPosts")]
    public virtual Blog OriginalBlog { get; set; }
}
""",
                    code.AdditionalFiles.Single(f => f.Path == "Post.cs"));
            },
            model =>
            {
                var postType = model.FindEntityType("TestNamespace.Post");

                var blogNavigation = postType.FindNavigation("Blog");

                var foreignKeyProperty = Assert.Single(blogNavigation.ForeignKey.Properties);
                Assert.Equal("BlogId", foreignKeyProperty.Name);

                var inverseNavigation = blogNavigation.Inverse;
                Assert.Equal("TestNamespace.Blog", inverseNavigation.DeclaringEntityType.Name);
                Assert.Equal("Posts", inverseNavigation.Name);

                var originalBlogNavigation = postType.FindNavigation("OriginalBlog");

                var originalForeignKeyProperty = Assert.Single(originalBlogNavigation.ForeignKey.Properties);
                Assert.Equal("OriginalBlogId", originalForeignKeyProperty.Name);

                var originalInverseNavigation = originalBlogNavigation.Inverse;
                Assert.Equal("TestNamespace.Blog", originalInverseNavigation.DeclaringEntityType.Name);
                Assert.Equal("OriginalPosts", originalInverseNavigation.Name);
            });

    [ConditionalFact]
    public Task InverseProperty_when_navigation_property_and_keyless()
        => TestAsync(
            modelBuilder => modelBuilder
                .Entity(
                    "Blog",
                    x => x.Property<int>("Id"))
                .Entity(
                    "Post",
                    x =>
                    {
                        x.HasNoKey();
                        x.HasOne("Blog", "Blog").WithMany();
                    }),
            new ModelCodeGenerationOptions { UseDataAnnotations = true },
            code =>
            {
                AssertFileContents(
                    """
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

[Keyless]
public partial class Post
{
    public int? BlogId { get; set; }

    [ForeignKey("BlogId")]
    public virtual Blog Blog { get; set; }
}
""",
                    code.AdditionalFiles.Single(f => f.Path == "Post.cs"));
            },
            model =>
            {
                var postType = model.FindEntityType("TestNamespace.Post");
                var blogNavigation = postType.FindNavigation("Blog");

                var foreignKeyProperty = Assert.Single(blogNavigation.ForeignKey.Properties);
                Assert.Equal("BlogId", foreignKeyProperty.Name);

                Assert.Null(blogNavigation.Inverse);
            });

    [ConditionalFact]
    public Task Entity_with_custom_annotation()
        => TestAsync(
            modelBuilder => modelBuilder
                .Entity(
                    "EntityWithAnnotation",
                    x =>
                    {
                        x.HasAnnotation("Custom:EntityAnnotation", "first argument");
                        x.Property<int>("Id");
                        x.HasKey("Id");
                    }),
            new ModelCodeGenerationOptions { UseDataAnnotations = true },
            code =>
            {
                AssertFileContents(
                    """
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

[CustomEntityDataAnnotation("first argument")]
public partial class EntityWithAnnotation
{
    [Key]
    public int Id { get; set; }
}
""",
                    code.AdditionalFiles.Single(f => f.Path == "EntityWithAnnotation.cs"));

                AssertFileContents(
                    $$"""
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class TestDbContext : DbContext
{
    public TestDbContext()
    {
    }

    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<EntityWithAnnotation> EntityWithAnnotation { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning {{DesignStrings.SensitiveInformationWarning}}
        => optionsBuilder.UseSqlServer("Initial Catalog=TestDatabase");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
""",
                    code.ContextFile);
            },
            assertModel: null,
            skipBuild: true);

    [ConditionalFact]
    public Task Entity_property_with_custom_annotation()
        => TestAsync(
            modelBuilder => modelBuilder
                .Entity(
                    "EntityWithPropertyAnnotation",
                    x =>
                    {
                        x.Property<int>("Id")
                            .HasAnnotation("Custom:PropertyAnnotation", "first argument");
                        x.HasKey("Id");
                    }),
            new ModelCodeGenerationOptions { UseDataAnnotations = true },
            code =>
            {
                AssertFileContents(
                    """
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class EntityWithPropertyAnnotation
{
    [Key]
    [CustomPropertyDataAnnotation("first argument")]
    public int Id { get; set; }
}
""",
                    code.AdditionalFiles.Single(f => f.Path == "EntityWithPropertyAnnotation.cs"));

                AssertFileContents(
                    $$"""
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class TestDbContext : DbContext
{
    public TestDbContext()
    {
    }

    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<EntityWithPropertyAnnotation> EntityWithPropertyAnnotation { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning {{DesignStrings.SensitiveInformationWarning}}
        => optionsBuilder.UseSqlServer("Initial Catalog=TestDatabase");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
""",
                    code.ContextFile);
            },
            assertModel: null,
            skipBuild: true);

    [ConditionalFact]
    public Task Scaffold_skip_navigations_default()
        => TestAsync(
            modelBuilder => modelBuilder
                .Entity(
                    "Blog",
                    x => x.Property<int>("Id"))
                .Entity(
                    "Post",
                    x => x.Property<int>("Id"))
                .Entity("BlogPost", _ => { })
                .Entity("Blog")
                .HasMany("Post", "Posts")
                .WithMany("Blogs")
                .UsingEntity("BlogPost"),
            new ModelCodeGenerationOptions { UseDataAnnotations = false },
            code =>
            {
                AssertFileContents(
                    $$"""
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class TestDbContext : DbContext
{
    public TestDbContext()
    {
    }

    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Blog> Blog { get; set; }

    public virtual DbSet<Post> Post { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning {{DesignStrings.SensitiveInformationWarning}}
        => optionsBuilder.UseSqlServer("Initial Catalog=TestDatabase");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Blog>(entity =>
        {
            entity.HasMany(d => d.Posts).WithMany(p => p.Blogs)
                .UsingEntity<Dictionary<string, object>>(
                    "BlogPost",
                    r => r.HasOne<Post>().WithMany().HasForeignKey("PostsId"),
                    l => l.HasOne<Blog>().WithMany().HasForeignKey("BlogsId"),
                    j =>
                    {
                        j.HasKey("BlogsId", "PostsId");
                        j.HasIndex(new[] { "PostsId" }, "IX_BlogPost_PostsId");
                    });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
""",
                    code.ContextFile);

                AssertFileContents(
                    """
using System;
using System.Collections.Generic;

namespace TestNamespace;

public partial class Blog
{
    public int Id { get; set; }

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
}
""",
                    code.AdditionalFiles.Single(e => e.Path == "Blog.cs"));

                AssertFileContents(
                    """
using System;
using System.Collections.Generic;

namespace TestNamespace;

public partial class Post
{
    public int Id { get; set; }

    public virtual ICollection<Blog> Blogs { get; set; } = new List<Blog>();
}
""",
                    code.AdditionalFiles.Single(e => e.Path == "Post.cs"));

                Assert.Equal(2, code.AdditionalFiles.Count);
            },
            model =>
            {
                var blogType = model.FindEntityType("TestNamespace.Blog");
                Assert.Empty(blogType.GetNavigations());
                var postsNavigation = Assert.Single(blogType.GetSkipNavigations());
                Assert.Equal("Posts", postsNavigation.Name);

                var postType = model.FindEntityType("TestNamespace.Post");
                Assert.Empty(postType.GetNavigations());
                var blogsNavigation = Assert.Single(postType.GetSkipNavigations());
                Assert.Equal("Blogs", blogsNavigation.Name);

                Assert.Equal(postsNavigation, blogsNavigation.Inverse);
                Assert.Equal(blogsNavigation, postsNavigation.Inverse);

                var joinEntityType = blogsNavigation.ForeignKey.DeclaringEntityType;
                Assert.Equal("BlogPost", joinEntityType.Name);
                Assert.Equal(typeof(Dictionary<string, object>), joinEntityType.ClrType);
                Assert.Single(joinEntityType.GetIndexes());
                Assert.Equal(2, joinEntityType.GetForeignKeys().Count());
            });

    [ConditionalFact]
    public Task Scaffold_skip_navigations_different_key_type()
        => TestAsync(
            modelBuilder => modelBuilder
                .Entity(
                    "Blog",
                    x => x.Property<int>("Id"))
                .Entity(
                    "Post",
                    x => x.Property<string>("Id"))
                .Entity("BlogPost", _ => { })
                .Entity("Blog")
                .HasMany("Post", "Posts")
                .WithMany("Blogs")
                .UsingEntity("BlogPost"),
            new ModelCodeGenerationOptions { UseDataAnnotations = false },
            code =>
            {
                AssertFileContents(
                    $$"""
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class TestDbContext : DbContext
{
    public TestDbContext()
    {
    }

    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Blog> Blog { get; set; }

    public virtual DbSet<Post> Post { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning {{DesignStrings.SensitiveInformationWarning}}
        => optionsBuilder.UseSqlServer("Initial Catalog=TestDatabase");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Blog>(entity =>
        {
            entity.HasMany(d => d.Posts).WithMany(p => p.Blogs)
                .UsingEntity<Dictionary<string, object>>(
                    "BlogPost",
                    r => r.HasOne<Post>().WithMany().HasForeignKey("PostsId"),
                    l => l.HasOne<Blog>().WithMany().HasForeignKey("BlogsId"),
                    j =>
                    {
                        j.HasKey("BlogsId", "PostsId");
                        j.HasIndex(new[] { "PostsId" }, "IX_BlogPost_PostsId");
                    });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
""",
                    code.ContextFile);

                AssertFileContents(
                    """
using System;
using System.Collections.Generic;

namespace TestNamespace;

public partial class Blog
{
    public int Id { get; set; }

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
}
""",
                    code.AdditionalFiles.Single(e => e.Path == "Blog.cs"));

                AssertFileContents(
                    """
using System;
using System.Collections.Generic;

namespace TestNamespace;

public partial class Post
{
    public string Id { get; set; }

    public virtual ICollection<Blog> Blogs { get; set; } = new List<Blog>();
}
""",
                    code.AdditionalFiles.Single(e => e.Path == "Post.cs"));

                Assert.Equal(2, code.AdditionalFiles.Count);
            },
            model =>
            {
                var blogType = model.FindEntityType("TestNamespace.Blog");
                Assert.Empty(blogType.GetNavigations());
                var postsNavigation = Assert.Single(blogType.GetSkipNavigations());
                Assert.Equal("Posts", postsNavigation.Name);

                var postType = model.FindEntityType("TestNamespace.Post");
                Assert.Empty(postType.GetNavigations());
                var blogsNavigation = Assert.Single(postType.GetSkipNavigations());
                Assert.Equal("Blogs", blogsNavigation.Name);

                Assert.Equal(postsNavigation, blogsNavigation.Inverse);
                Assert.Equal(blogsNavigation, postsNavigation.Inverse);

                var joinEntityType = blogsNavigation.ForeignKey.DeclaringEntityType;
                Assert.Equal("BlogPost", joinEntityType.Name);
                Assert.Equal(typeof(Dictionary<string, object>), joinEntityType.ClrType);
                Assert.Single(joinEntityType.GetIndexes());
                Assert.Equal(2, joinEntityType.GetForeignKeys().Count());
            });

    [ConditionalFact]
    public Task Scaffold_skip_navigations_default_data_annotations()
        => TestAsync(
            modelBuilder => modelBuilder
                .Entity(
                    "Blog",
                    x => x.Property<int>("Id"))
                .Entity(
                    "Post",
                    x => x.Property<int>("Id"))
                .Entity("BlogPost", _ => { })
                .Entity("Blog")
                .HasMany("Post", "Posts")
                .WithMany("Blogs")
                .UsingEntity("BlogPost"),
            new ModelCodeGenerationOptions { UseDataAnnotations = true },
            code =>
            {
                AssertFileContents(
                    $$"""
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class TestDbContext : DbContext
{
    public TestDbContext()
    {
    }

    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Blog> Blog { get; set; }

    public virtual DbSet<Post> Post { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning {{DesignStrings.SensitiveInformationWarning}}
        => optionsBuilder.UseSqlServer("Initial Catalog=TestDatabase");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Blog>(entity =>
        {
            entity.HasMany(d => d.Posts).WithMany(p => p.Blogs)
                .UsingEntity<Dictionary<string, object>>(
                    "BlogPost",
                    r => r.HasOne<Post>().WithMany().HasForeignKey("PostsId"),
                    l => l.HasOne<Blog>().WithMany().HasForeignKey("BlogsId"),
                    j =>
                    {
                        j.HasKey("BlogsId", "PostsId");
                        j.HasIndex(new[] { "PostsId" }, "IX_BlogPost_PostsId");
                    });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
""",
                    code.ContextFile);

                AssertFileContents(
                    """
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class Blog
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("BlogsId")]
    [InverseProperty("Blogs")]
    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
}
""",
                    code.AdditionalFiles.Single(e => e.Path == "Blog.cs"));

                AssertFileContents(
                    """
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class Post
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("PostsId")]
    [InverseProperty("Posts")]
    public virtual ICollection<Blog> Blogs { get; set; } = new List<Blog>();
}
""",
                    code.AdditionalFiles.Single(e => e.Path == "Post.cs"));

                Assert.Equal(2, code.AdditionalFiles.Count);
            },
            model =>
            {
                var blogType = model.FindEntityType("TestNamespace.Blog");
                Assert.Empty(blogType.GetNavigations());
                var postsNavigation = Assert.Single(blogType.GetSkipNavigations());
                Assert.Equal("Posts", postsNavigation.Name);

                var postType = model.FindEntityType("TestNamespace.Post");
                Assert.Empty(postType.GetNavigations());
                var blogsNavigation = Assert.Single(postType.GetSkipNavigations());
                Assert.Equal("Blogs", blogsNavigation.Name);

                Assert.Equal(postsNavigation, blogsNavigation.Inverse);
                Assert.Equal(blogsNavigation, postsNavigation.Inverse);

                var joinEntityType = blogsNavigation.ForeignKey.DeclaringEntityType;
                Assert.Equal("BlogPost", joinEntityType.Name);
                Assert.Equal(typeof(Dictionary<string, object>), joinEntityType.ClrType);
                Assert.Single(joinEntityType.GetIndexes());
                Assert.Equal(2, joinEntityType.GetForeignKeys().Count());
            });

    [ConditionalFact]
    public Task Scaffold_skip_navigations_alternate_key_data_annotations()
        => TestAsync(
            modelBuilder => modelBuilder
                .Entity(
                    "Blog",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<int>("Key");
                    })
                .Entity(
                    "Post",
                    x => x.Property<int>("Id"))
                .Entity("Blog").HasMany("Post", "Posts").WithMany("Blogs")
                .UsingEntity(
                    "BlogPost",
                    r => r.HasOne("Post").WithMany(),
                    l => l.HasOne("Blog").WithMany().HasPrincipalKey("Key")),
            new ModelCodeGenerationOptions { UseDataAnnotations = true },
            code =>
            {
                AssertFileContents(
                    $$"""
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class TestDbContext : DbContext
{
    public TestDbContext()
    {
    }

    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Blog> Blog { get; set; }

    public virtual DbSet<Post> Post { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning {{DesignStrings.SensitiveInformationWarning}}
        => optionsBuilder.UseSqlServer("Initial Catalog=TestDatabase");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Blog>(entity =>
        {
            entity.HasMany(d => d.Posts).WithMany(p => p.Blogs)
                .UsingEntity<Dictionary<string, object>>(
                    "BlogPost",
                    r => r.HasOne<Post>().WithMany().HasForeignKey("PostsId"),
                    l => l.HasOne<Blog>().WithMany()
                        .HasPrincipalKey("Key")
                        .HasForeignKey("BlogsKey"),
                    j =>
                    {
                        j.HasKey("BlogsKey", "PostsId");
                        j.HasIndex(new[] { "PostsId" }, "IX_BlogPost_PostsId");
                    });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
""",
                    code.ContextFile);

                AssertFileContents(
                    """
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class Blog
{
    [Key]
    public int Id { get; set; }

    public int Key { get; set; }

    [ForeignKey("BlogsKey")]
    [InverseProperty("Blogs")]
    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
}
""",
                    code.AdditionalFiles.Single(e => e.Path == "Blog.cs"));

                AssertFileContents(
                    """
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class Post
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("PostsId")]
    [InverseProperty("Posts")]
    public virtual ICollection<Blog> Blogs { get; set; } = new List<Blog>();
}
""",
                    code.AdditionalFiles.Single(e => e.Path == "Post.cs"));

                Assert.Equal(2, code.AdditionalFiles.Count);
            },
            model =>
            {
                var blogType = model.FindEntityType("TestNamespace.Blog");
                Assert.Empty(blogType.GetNavigations());
                var postsNavigation = Assert.Single(blogType.GetSkipNavigations());
                Assert.Equal("Posts", postsNavigation.Name);

                var postType = model.FindEntityType("TestNamespace.Post");
                Assert.Empty(postType.GetNavigations());
                var blogsNavigation = Assert.Single(postType.GetSkipNavigations());
                Assert.Equal("Blogs", blogsNavigation.Name);

                Assert.Equal(postsNavigation, blogsNavigation.Inverse);
                Assert.Equal(blogsNavigation, postsNavigation.Inverse);

                var joinEntityType = blogsNavigation.ForeignKey.DeclaringEntityType;
                Assert.Equal("BlogPost", joinEntityType.Name);
                Assert.Equal(typeof(Dictionary<string, object>), joinEntityType.ClrType);
                Assert.Single(joinEntityType.GetIndexes());
                Assert.Equal(2, joinEntityType.GetForeignKeys().Count());

                var fk = Assert.Single(joinEntityType.FindDeclaredForeignKeys(new[] { joinEntityType.GetProperty("BlogsKey") }));
                Assert.False(fk.PrincipalKey.IsPrimaryKey());
            });

    [ConditionalFact]
    public Task Many_to_many_ef6()
        => TestAsync(
            modelBuilder => modelBuilder
                .Entity(
                    "Blog",
                    x =>
                    {
                        x.ToTable("Blogs");
                        x.HasAnnotation(ScaffoldingAnnotationNames.DbSetName, "Blogs");

                        x.Property<int>("Id");
                    })
                .Entity(
                    "Post",
                    x =>
                    {
                        x.ToTable("Posts");
                        x.HasAnnotation(ScaffoldingAnnotationNames.DbSetName, "Posts");

                        x.Property<int>("Id");

                        x.HasMany("Blog", "Blogs").WithMany("Posts")
                            .UsingEntity(
                                "PostBlog",
                                r => r.HasOne("Blog", null).WithMany().HasForeignKey("BlogId").HasConstraintName("Post_Blogs_Target"),
                                l => l.HasOne("Post", null).WithMany().HasForeignKey("PostId").HasConstraintName("Post_Blogs_Source"),
                                j =>
                                {
                                    j.ToTable("PostBlogs");
                                    j.HasAnnotation(ScaffoldingAnnotationNames.DbSetName, "PostBlogs");

                                    j.Property<int>("BlogId").HasColumnName("Blog_Id");
                                    j.Property<int>("PostId").HasColumnName("Post_Id");
                                });
                    }),
            new ModelCodeGenerationOptions(),
            code =>
            {
                AssertFileContents(
                    $$"""
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class TestDbContext : DbContext
{
    public TestDbContext()
    {
    }

    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Blog> Blogs { get; set; }

    public virtual DbSet<Post> Posts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning {{DesignStrings.SensitiveInformationWarning}}
        => optionsBuilder.UseSqlServer("Initial Catalog=TestDatabase");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Blog>(entity =>
        {
            entity.HasMany(d => d.Posts).WithMany(p => p.Blogs)
                .UsingEntity<Dictionary<string, object>>(
                    "PostBlog",
                    r => r.HasOne<Post>().WithMany()
                        .HasForeignKey("PostId")
                        .HasConstraintName("Post_Blogs_Source"),
                    l => l.HasOne<Blog>().WithMany()
                        .HasForeignKey("BlogId")
                        .HasConstraintName("Post_Blogs_Target"),
                    j =>
                    {
                        j.HasKey("BlogId", "PostId");
                        j.ToTable("PostBlogs");
                        j.HasIndex(new[] { "PostId" }, "IX_PostBlogs_Post_Id");
                        j.IndexerProperty<int>("BlogId").HasColumnName("Blog_Id");
                        j.IndexerProperty<int>("PostId").HasColumnName("Post_Id");
                    });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
""",
                    code.ContextFile);
            },
            model => Assert.Collection(
                model.GetEntityTypes().OrderBy(e => e.Name),
                t1 =>
                {
                    Assert.Equal("PostBlog", t1.Name);
                    Assert.Equal("PostBlogs", t1.GetTableName());
                    Assert.Collection(
                        t1.GetForeignKeys().OrderBy(fk => fk.GetConstraintName()),
                        fk1 =>
                        {
                            Assert.Equal("Post_Blogs_Source", fk1.GetConstraintName());
                            var property = Assert.Single(fk1.Properties);
                            Assert.Equal("PostId", property.Name);
                            Assert.Equal("Post_Id", property.GetColumnName(StoreObjectIdentifier.Table(t1.GetTableName())));
                            Assert.Equal("TestNamespace.Post", fk1.PrincipalEntityType.Name);
                            Assert.Equal(DeleteBehavior.Cascade, fk1.DeleteBehavior);
                        },
                        fk2 =>
                        {
                            Assert.Equal("Post_Blogs_Target", fk2.GetConstraintName());
                            var property = Assert.Single(fk2.Properties);
                            Assert.Equal("BlogId", property.Name);
                            Assert.Equal("Blog_Id", property.GetColumnName(StoreObjectIdentifier.Table(t1.GetTableName())));
                            Assert.Equal("TestNamespace.Blog", fk2.PrincipalEntityType.Name);
                            Assert.Equal(DeleteBehavior.Cascade, fk2.DeleteBehavior);
                        });
                },
                t2 =>
                {
                    Assert.Equal("TestNamespace.Blog", t2.Name);
                    Assert.Equal("Blogs", t2.GetTableName());
                    Assert.Empty(t2.GetDeclaredForeignKeys());
                    var skipNavigation = Assert.Single(t2.GetSkipNavigations());
                    Assert.Equal("Posts", skipNavigation.Name);
                    Assert.Equal("Blogs", skipNavigation.Inverse.Name);
                    Assert.Equal("PostBlog", skipNavigation.JoinEntityType.Name);
                    Assert.Equal("Post_Blogs_Target", skipNavigation.ForeignKey.GetConstraintName());
                },
                t3 =>
                {
                    Assert.Equal("TestNamespace.Post", t3.Name);
                    Assert.Equal("Posts", t3.GetTableName());
                    Assert.Empty(t3.GetDeclaredForeignKeys());
                    var skipNavigation = Assert.Single(t3.GetSkipNavigations());
                    Assert.Equal("Blogs", skipNavigation.Name);
                    Assert.Equal("Posts", skipNavigation.Inverse.Name);
                    Assert.Equal("PostBlog", skipNavigation.JoinEntityType.Name);
                    Assert.Equal("Post_Blogs_Source", skipNavigation.ForeignKey.GetConstraintName());
                }));

    protected override IServiceCollection AddModelServices(IServiceCollection services)
        => services.Replace(ServiceDescriptor.Singleton<IRelationalAnnotationProvider, TestModelAnnotationProvider>());

    protected override IServiceCollection AddScaffoldingServices(IServiceCollection services)
        => services.Replace(ServiceDescriptor.Singleton<IAnnotationCodeGenerator, TestModelAnnotationCodeGenerator>());

    private class TestModelAnnotationProvider(RelationalAnnotationProviderDependencies dependencies) : SqlServerAnnotationProvider(dependencies)
    {
        public override IEnumerable<IAnnotation> For(ITable table, bool designTime)
        {
            foreach (var annotation in base.For(table, designTime))
            {
                yield return annotation;
            }

            var entityType = table.EntityTypeMappings.First().TypeBase;

            foreach (var annotation in entityType.GetAnnotations().Where(a => a.Name == "Custom:EntityAnnotation"))
            {
                yield return annotation;
            }
        }

        public override IEnumerable<IAnnotation> For(IColumn column, bool designTime)
        {
            foreach (var annotation in base.For(column, designTime))
            {
                yield return annotation;
            }

            var properties = column.PropertyMappings.Select(m => m.Property);
            var annotations = properties.SelectMany(p => p.GetAnnotations()).GroupBy(a => a.Name).Select(g => g.First());

            foreach (var annotation in annotations.Where(a => a.Name == "Custom:PropertyAnnotation"))
            {
                yield return annotation;
            }
        }
    }

    private class TestModelAnnotationCodeGenerator(AnnotationCodeGeneratorDependencies dependencies) : SqlServerAnnotationCodeGenerator(dependencies)
    {
        protected override AttributeCodeFragment GenerateDataAnnotation(IEntityType entityType, IAnnotation annotation)
            => annotation.Name switch
            {
                "Custom:EntityAnnotation" => new AttributeCodeFragment(
                    typeof(CustomEntityDataAnnotationAttribute), annotation.Value as string),
                _ => base.GenerateDataAnnotation(entityType, annotation)
            };

        protected override AttributeCodeFragment GenerateDataAnnotation(IProperty property, IAnnotation annotation)
            => annotation.Name switch
            {
                "Custom:PropertyAnnotation" => new AttributeCodeFragment(
                    typeof(CustomPropertyDataAnnotationAttribute), annotation.Value as string),
                _ => base.GenerateDataAnnotation(property, annotation)
            };
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class CustomEntityDataAnnotationAttribute(string argument) : Attribute
    {
        public virtual string Argument { get; } = argument;
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class CustomPropertyDataAnnotationAttribute(string argument) : Attribute
    {
        public virtual string Argument { get; } = argument;
    }
}

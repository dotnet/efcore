// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    public class CSharpEntityTypeGeneratorTest : ModelCodeGeneratorTestBase
    {
        [ConditionalFact]
        public void Navigation_properties()
        {
            Test(
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
                            x.HasMany("Contribution", "Contributions").WithOne();
                        }),
                new ModelCodeGenerationOptions { UseDataAnnotations = true },
                code =>
                {
                    var postFile = code.AdditionalFiles.First(f => f.Path == "Post.cs");
                    Assert.Equal(
                        @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace
{
    public partial class Post
    {
        public Post()
        {
            Contributions = new HashSet<Contribution>();
        }

        [Key]
        public int Id { get; set; }
        public int? AuthorId { get; set; }

        [ForeignKey(nameof(AuthorId))]
        [InverseProperty(nameof(Person.Posts))]
        public virtual Person Author { get; set; }
        public virtual ICollection<Contribution> Contributions { get; set; }
    }
}
",
                        postFile.Code, ignoreLineEndingDifferences: true);
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
        }

        [ConditionalFact]
        public void Navigation_property_with_same_type_and_navigation_name()
        {
            Test(
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
                    var postFile = code.AdditionalFiles.First(f => f.Path == "Post.cs");
                    Assert.Equal(
                        @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace
{
    public partial class Post
    {
        [Key]
        public int Id { get; set; }
        public int? BlogId { get; set; }

        [ForeignKey(nameof(BlogId))]
        [InverseProperty(""Posts"")]
        public virtual Blog Blog { get; set; }
    }
}
",
                        postFile.Code, ignoreLineEndingDifferences: true);
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
        }

        [ConditionalFact]
        public void Navigation_property_with_same_type_and_property_name()
        {
            Test(
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
                    var postFile = code.AdditionalFiles.First(f => f.Path == "Post.cs");
                    Assert.Equal(
                        @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace
{
    public partial class Post
    {
        [Key]
        public int Id { get; set; }
        public int? Blog { get; set; }

        [ForeignKey(nameof(Blog))]
        [InverseProperty(""Posts"")]
        public virtual Blog BlogNavigation { get; set; }
    }
}
",
                        postFile.Code, ignoreLineEndingDifferences: true);
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
        }

        [ConditionalFact]
        public void Navigation_property_with_same_type_and_other_navigation_name()
        {
            Test(
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
                    var postFile = code.AdditionalFiles.First(f => f.Path == "Post.cs");
                    Assert.Equal(
                        @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace
{
    public partial class Post
    {
        [Key]
        public int Id { get; set; }
        public int? BlogId { get; set; }
        public int? OriginalBlogId { get; set; }

        [ForeignKey(nameof(BlogId))]
        [InverseProperty(""Posts"")]
        public virtual Blog Blog { get; set; }
        [ForeignKey(nameof(OriginalBlogId))]
        [InverseProperty(""OriginalPosts"")]
        public virtual Blog OriginalBlog { get; set; }
    }
}
",
                        postFile.Code, ignoreLineEndingDifferences: true);
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
        }

        [ConditionalFact]
        public void Composite_key()
        {
            Test(
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
                    var postFile = code.AdditionalFiles.First(f => f.Path == "Post.cs");
                    Assert.Equal(
                        @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace
{
    public partial class Post
    {
        [Key]
        public int Key { get; set; }
        [Key]
        public int Serial { get; set; }
    }
}
",
                        postFile.Code, ignoreLineEndingDifferences: true);
                },
                model =>
                {
                    var postType = model.FindEntityType("TestNamespace.Post");
                    Assert.NotNull(postType.FindPrimaryKey());
                });
        }

        [ConditionalFact]
        public void Views_dont_generate_TableAttribute()
        {
            Test(
                modelBuilder => modelBuilder.Entity("Vista").ToView("Vistas", "dbo"),
                new ModelCodeGenerationOptions { UseDataAnnotations = true },
                code =>
                {
                    var vistaFile = code.AdditionalFiles.First(f => f.Path == "Vista.cs");
                    Assert.Equal(
                        @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace
{
    [Keyless]
    public partial class Vista
    {
    }
}
",
                        vistaFile.Code, ignoreLineEndingDifferences: true);
                },
                model =>
                {
                    var entityType = model.FindEntityType("TestNamespace.Vista");
                    Assert.Equal("Vistas", entityType.GetViewName());
                    Assert.Null(entityType.GetTableName());
                    Assert.Equal("dbo", entityType.GetViewSchema());
                    Assert.Null(entityType.GetSchema());
                });
        }

        [ConditionalFact]
        public void Keyless_entity_generates_KeylesssAttribute()
        {
            Test(
                modelBuilder => modelBuilder.Entity("Vista").HasNoKey(),
                new ModelCodeGenerationOptions { UseDataAnnotations = true },
                code =>
                {
                    var vistaFile = code.AdditionalFiles.First(f => f.Path == "Vista.cs");
                    Assert.Equal(
                        @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace
{
    [Keyless]
    public partial class Vista
    {
    }
}
",
                        vistaFile.Code, ignoreLineEndingDifferences: true);

                    Assert.Equal(
                        @"using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace TestNamespace
{
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
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer(""Initial Catalog=TestDatabase"");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
",
                        code.ContextFile.Code, ignoreLineEndingDifferences: true);
                },
                model =>
                {
                    var entityType = model.FindEntityType("TestNamespace.Vista");
                    Assert.Null(entityType.FindPrimaryKey());
                });
        }
    }
}

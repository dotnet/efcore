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
                new ModelCodeGenerationOptions
                {
                    UseDataAnnotations = true
                },
                code =>
                {
                    var postFile = code.AdditionalFiles.First(f => f.Path == "Post.cs");
                    Assert.Equal(
                        @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
                        postFile.Code);
                },
                model =>
                {
                    var postType = model.FindEntityType("TestNamespace.Post");
                    var authorNavigation = postType.FindNavigation("Author");
                    Assert.True(authorNavigation.IsDependentToPrincipal());
                    Assert.Equal("TestNamespace.Person", authorNavigation.ForeignKey.PrincipalEntityType.Name);

                    var contributionsNav = postType.FindNavigation("Contributions");
                    Assert.False(contributionsNav.IsDependentToPrincipal());
                    Assert.Equal("TestNamespace.Contribution", contributionsNav.ForeignKey.DeclaringEntityType.Name);
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
                        postFile.Code);
                },
                model =>
                {
                    var postType = model.FindEntityType("TestNamespace.Post");
                    var blogNavigation = postType.FindNavigation("Blog");

                    var foreignKeyProperty = Assert.Single(blogNavigation.ForeignKey.Properties);
                    Assert.Equal("BlogId", foreignKeyProperty.Name);

                    var inverseNavigation = blogNavigation.FindInverse();
                    Assert.Equal("TestNamespace.Blog", inverseNavigation.DeclaringEntityType.Name);
                    Assert.Equal("Posts", inverseNavigation.Name);
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
                new ModelCodeGenerationOptions
                {
                    UseDataAnnotations = true
                },
                code =>
                {
                    var postFile = code.AdditionalFiles.First(f => f.Path == "Post.cs");
                    Assert.Equal(
                        @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
                        postFile.Code);
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
                code => Assert.DoesNotContain(
                    "[Table(",
                    code.AdditionalFiles.First(f => f.Path == "Vista.cs").Code),
                model =>
                {
                    var entityType = model.FindEntityType("TestNamespace.Vista");
                    Assert.Equal("Vistas", entityType.GetTableName());
                    Assert.Equal("dbo", entityType.GetSchema());
                });
        }
    }
}

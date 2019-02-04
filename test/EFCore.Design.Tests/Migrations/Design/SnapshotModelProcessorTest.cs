// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Migrations.Internal
{
    public class SnapshotModelProcessorTest
    {
        [Fact]
        public void Updates_provider_annotations_on_model()
        {
            var builder = new ModelBuilder(new ConventionSet());

            var model = builder.Model;
            ((Model)model).SetProductVersion("1.1.2");

            var entityType = builder.Entity<Blog>().Metadata;
            var property = builder.Entity<Blog>().Property(e => e.Id).Metadata;
            var key = builder.Entity<Blog>().HasKey(e => e.Id).Metadata;

            builder.Entity<Post>().Property(e => e.BlogId);
            var foreignKey = builder.Entity<Blog>().HasMany(e => e.Posts).WithOne(e => e.Blog).HasForeignKey(e => e.BlogId).Metadata;
            var nav1 = foreignKey.DependentToPrincipal;
            var nav2 = foreignKey.PrincipalToDependent;

            var index = builder.Entity<Post>().HasIndex(e => e.BlogId).Metadata;

            AddAnnotations(model);
            AddAnnotations(entityType);
            AddAnnotations(property);
            AddAnnotations(key);
            AddAnnotations(foreignKey);
            AddAnnotations(nav1);
            AddAnnotations(nav2);
            AddAnnotations(index);

            var reporter = new TestOperationReporter();

            new SnapshotModelProcessor(reporter).Process(model);

            AssertAnnotations(model);
            AssertAnnotations(entityType);
            AssertAnnotations(property);
            AssertAnnotations(key);
            AssertAnnotations(foreignKey);
            AssertAnnotations(nav1);
            AssertAnnotations(nav2);
            AssertAnnotations(index);

            Assert.Empty(reporter.Messages);
        }

        [Fact]
        public void Warns_for_conflicting_annotations()
        {
            var model = new Model();
            model.SetProductVersion("1.1.2");
            model["Unicorn:DefaultSchema"] = "Value1";
            model["Hippo:DefaultSchema"] = "Value2";

            Assert.Equal(3, model.GetAnnotations().Count());

            var reporter = new TestOperationReporter();

            new SnapshotModelProcessor(reporter).Process(model);

            Assert.Equal("warn: " + DesignStrings.MultipleAnnotationConflict("DefaultSchema"), reporter.Messages.Single());
            Assert.Equal(2, model.GetAnnotations().Count());

            var actual = (string)model["Relational:DefaultSchema"];
            Assert.True(actual == "Value1" || actual == "Value2");
        }

        [Fact]
        public void Warns_for_conflicting_annotations_one_relational()
        {
            var model = new Model();
            model.SetProductVersion("1.1.2");
            model["Unicorn:DefaultSchema"] = "Value1";
            model["Relational:DefaultSchema"] = "Value2";

            Assert.Equal(3, model.GetAnnotations().Count());

            var reporter = new TestOperationReporter();

            new SnapshotModelProcessor(reporter).Process(model);

            Assert.Equal("warn: " + DesignStrings.MultipleAnnotationConflict("DefaultSchema"), reporter.Messages.Single());
            Assert.Equal(2, model.GetAnnotations().Count());

            var actual = (string)model["Relational:DefaultSchema"];
            Assert.True(actual == "Value1" || actual == "Value2");
        }

        [Fact]
        public void Does_not_warn_for_duplicate_non_conflicting_annotations()
        {
            var model = new Model();
            model.SetProductVersion("1.1.2");
            model["Unicorn:DefaultSchema"] = "Value";
            model["Hippo:DefaultSchema"] = "Value";

            Assert.Equal(3, model.GetAnnotations().Count());

            var reporter = new TestOperationReporter();

            new SnapshotModelProcessor(reporter).Process(model);

            Assert.Empty(reporter.Messages);

            Assert.Equal(2, model.GetAnnotations().Count());
            Assert.Equal("Value", (string)model["Relational:DefaultSchema"]);
        }

        [Fact]
        public void Does_not_process_non_v1_models()
        {
            var model = new Model();
            model.SetProductVersion("2.0.0");
            model["Unicorn:DefaultSchema"] = "Value";

            Assert.Equal(2, model.GetAnnotations().Count());

            var reporter = new TestOperationReporter();

            new SnapshotModelProcessor(reporter).Process(model);

            Assert.Empty(reporter.Messages);

            Assert.Equal(2, model.GetAnnotations().Count());
            Assert.Equal("Value", (string)model["Unicorn:DefaultSchema"]);
        }

        [Fact]
        public void Sets_owned_type_keys()
        {
            var builder = new ModelBuilder(new ConventionSet());

            var model = builder.Model;
            ((Model)model).SetProductVersion("2.1.0");

            builder.Entity<Blog>(
                b =>
                {
                    b.Property(e => e.Id);
                    b.HasKey(e => e.Id);

                    b.OwnsOne(e => e.Details).WithOwner().HasForeignKey(e => e.BlogId);
                });

            var reporter = new TestOperationReporter();
            new SnapshotModelProcessor(reporter).Process(model);

            Assert.Empty(reporter.Messages);
            Assert.Equal(
                nameof(BlogDetails.BlogId),
                model.FindEntityType(typeof(Blog)).FindNavigation(nameof(Blog.Details)).GetTargetType().FindPrimaryKey().Properties.Single()
                    .Name);
        }

        private void AddAnnotations(IMutableAnnotatable element)
        {
            foreach (var annotationName in GetAnnotationNames()
                .Select(a => "Unicorn" + a.Substring(RelationalAnnotationNames.Prefix.Length - 1)))
            {
                element[annotationName] = "Value";
            }
        }

        private void AssertAnnotations(IMutableAnnotatable element)
        {
            foreach (var annotationName in GetAnnotationNames())
            {
                Assert.Equal("Value", (string)element[annotationName]);
            }
        }

        private static IEnumerable<string> GetAnnotationNames()
            => typeof(RelationalAnnotationNames)
                .GetTypeInfo()
                .GetRuntimeFields()
                .Where(p => p.Name != nameof(RelationalAnnotationNames.Prefix))
                .Select(p => (string)p.GetValue(null));

        private class Blog
        {
            public int Id { get; set; }

            public ICollection<Post> Posts { get; set; }
            public BlogDetails Details { get; set; }
        }

        private class Post
        {
            public int BlogId { get; set; }
            public Blog Blog { get; set; }
        }

        private class BlogDetails
        {
            public int BlogId { get; set; }

            public ICollection<Post> Posts { get; set; }
        }
    }
}

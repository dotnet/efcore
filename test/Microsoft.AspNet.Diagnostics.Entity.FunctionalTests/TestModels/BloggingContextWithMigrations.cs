// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Builders;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using System;

namespace Microsoft.AspNet.Diagnostics.Entity.Tests
{
    public class BloggingContextWithMigrations : BloggingContext
    {
        public BloggingContextWithMigrations(IServiceProvider provider, DbContextOptions options)
            : base(provider, options)
        { }

        [ContextType(typeof(BloggingContextWithMigrations))]
        public class BloggingContextWithMigrationsModelSnapshot : ModelSnapshot
        {
            public override IModel Model
            {
                get
                {
                    var builder = new BasicModelBuilder();

                    builder.Entity("Blogging.Models.Blog", b =>
                    {
                        b.Property<int>("BlogId");
                        b.Property<int>("BlogId").Metadata.ValueGeneration = ValueGeneration.OnAdd;
                        b.Property<string>("Name");
                        b.Key("BlogId");
                    });

                    return builder.Model;
                }
            }
        }

        [ContextType(typeof(BloggingContextWithMigrations))]
        public class MigrationOne : Migration, IMigrationMetadata
        {
            string IMigrationMetadata.MigrationId
            {
                get { return "111111111111111_MigrationOne"; }
            }

            IModel IMigrationMetadata.TargetModel
            {
                get { return new BloggingContextWithMigrationsModelSnapshot().Model; }
            }

            public override void Up(MigrationBuilder migrationBuilder)
            {
                migrationBuilder.CreateTable("Blog",
                c => new
                {
                    BlogId = c.Int(nullable: false, identity: true),
                    Name = c.String(),
                })
                .PrimaryKey("PK_Blog", t => t.BlogId);
            }

            public override void Down(MigrationBuilder migrationBuilder)
            {
                migrationBuilder.DropTable("Blog");
            }
        }

        [ContextType(typeof(BloggingContextWithMigrations))]
        public class MigrationTwo : Migration, IMigrationMetadata
        {
            string IMigrationMetadata.MigrationId
            {
                get { return "222222222222222_MigrationTwo"; }
            }

            IModel IMigrationMetadata.TargetModel
            {
                get { return new BloggingContextWithMigrationsModelSnapshot().Model; }
            }

            public override void Up(MigrationBuilder migrationBuilder)
            { }

            public override void Down(MigrationBuilder migrationBuilder)
            { }
        }
    }
}
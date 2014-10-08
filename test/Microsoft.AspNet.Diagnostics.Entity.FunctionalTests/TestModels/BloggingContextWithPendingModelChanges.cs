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
    public class BloggingContextWithPendingModelChanges : BloggingContext
    {
        public BloggingContextWithPendingModelChanges(IServiceProvider provider, DbContextOptions options)
            : base(provider, options)
        { }

        [ContextType(typeof(BloggingContextWithPendingModelChanges))]
        public class BloggingModelSnapshot : ModelSnapshot
        {
            public override IModel Model
            {
                get { return new BasicModelBuilder().Model; }
            }
        }

        [ContextType(typeof(BloggingContextWithPendingModelChanges))]
        public partial class MigrationOne : Migration, IMigrationMetadata
        {
            protected override string MigrationId
            {
                get { return "111111111111111_MigrationOne"; }
            }

            protected override string ProductVersion
            {
                get { return CurrentProductVersion; }
            }

            protected override IModel TargetModel
            {
                get { return new BasicModelBuilder().Model; }
            }

            public override void Up(MigrationBuilder migrationBuilder)
            { }

            public override void Down(MigrationBuilder migrationBuilder)
            { }
        }
    }
}
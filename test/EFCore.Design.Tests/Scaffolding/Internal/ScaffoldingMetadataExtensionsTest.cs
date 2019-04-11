// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public class ScaffoldingMetadataExtensionsTest
    {
        [Fact]
        public void It_sets_gets_entity_type_errors()
        {
            IMutableModel model = new Model();

            model.Scaffolding().EntityTypeErrors.Add("ET", "FAIL!");
            Assert.Equal("FAIL!", model.Scaffolding().EntityTypeErrors["ET"]);

            model.Scaffolding().EntityTypeErrors = new Dictionary<string, string>();
            Assert.Empty(model.Scaffolding().EntityTypeErrors.Values);

            model.Scaffolding().EntityTypeErrors["ET"] = "FAIL 2!";
            model.Scaffolding().EntityTypeErrors.Clear();
            Assert.Empty(model.Scaffolding().EntityTypeErrors.Values);
        }

        [Fact]
        public void It_sets_DbSet_name()
        {
            IMutableModel model = new Model();
            var entity = model.AddEntityType("Blog");
            entity.Scaffolding().DbSetName = "Blogs";

            Assert.Equal("Blogs", entity.Scaffolding().DbSetName);
        }

        [Fact]
        public void It_sets_gets_database_name()
        {
            var model = new Model();
            var extensions = model.Scaffolding();

            Assert.Null(extensions.DatabaseName);

            extensions.DatabaseName = "Northwind";

            Assert.Equal("Northwind", extensions.DatabaseName);

            extensions.DatabaseName = null;

            Assert.Null(extensions.DatabaseName);
        }
    }
}

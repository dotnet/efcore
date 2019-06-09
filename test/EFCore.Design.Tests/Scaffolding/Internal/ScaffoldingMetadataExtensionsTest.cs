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
        [ConditionalFact]
        public void It_sets_gets_entity_type_errors()
        {
            IMutableModel model = new Model();

            model.GetEntityTypeErrors().Add("ET", "FAIL!");
            Assert.Equal("FAIL!", model.GetEntityTypeErrors()["ET"]);

            model.SetEntityTypeErrors(new Dictionary<string, string>());
            Assert.Empty(model.GetEntityTypeErrors().Values);

            model.GetEntityTypeErrors()["ET"] = "FAIL 2!";
            model.GetEntityTypeErrors().Clear();
            Assert.Empty(model.GetEntityTypeErrors().Values);
        }

        [ConditionalFact]
        public void It_sets_DbSet_name()
        {
            IMutableModel model = new Model();
            var entity = model.AddEntityType("Blog");
            entity.SetDbSetName("Blogs");

            Assert.Equal("Blogs", entity.GetDbSetName());
        }

        [ConditionalFact]
        public void It_sets_gets_database_name()
        {
            var model = new Model();
            var extensions = model;

            Assert.Null(extensions.GetDatabaseName());

            extensions.SetDatabaseName("Northwind");

            Assert.Equal("Northwind", extensions.GetDatabaseName());

            extensions.SetDatabaseName(null);

            Assert.Null(extensions.GetDatabaseName());
        }
    }
}

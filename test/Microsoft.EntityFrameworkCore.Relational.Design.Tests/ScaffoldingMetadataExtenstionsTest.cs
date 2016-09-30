// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Relational.Design
{
    public class ScaffoldingMetadataExtenstionsTest
    {
        [Fact]
        public void It_adds_provider_method_names()
        {
            var model = new Model();

            Assert.Null(model.Scaffolding().UseProviderMethodName);

            model.Scaffolding().UseProviderMethodName = "UsePutRelationalProviderNameHere";
            Assert.Equal("UsePutRelationalProviderNameHere", model.Scaffolding().UseProviderMethodName);

            model.Scaffolding().UseProviderMethodName = null;
            Assert.Null(model.Scaffolding().UseProviderMethodName);
        }

        [Fact]
        public void It_sets_gets_entity_type_errors()
        {
            var model = new Model();

            model.Scaffolding().EntityTypeErrors.Add("ET", "FAIL!");
            Assert.Equal("FAIL!", model.Scaffolding().EntityTypeErrors["ET"]);

            model.Scaffolding().EntityTypeErrors = new Dictionary<string, string>();
            Assert.Empty(model.Scaffolding().EntityTypeErrors.Values);

            model.Scaffolding().EntityTypeErrors["ET"] = "FAIL 2!";
            model.Scaffolding().EntityTypeErrors.Clear();
            Assert.Empty(model.Scaffolding().EntityTypeErrors.Values);
        }
    }
}

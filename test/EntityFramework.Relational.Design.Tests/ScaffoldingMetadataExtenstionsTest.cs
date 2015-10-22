// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Scaffolding.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Design
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
        public void It_adds_reads_nav_prop_names()
        {
            var modelBuilder = new ModelBuilder(new Metadata.Conventions.ConventionSet());
            IMutableForeignKey fk = null; 
            modelBuilder.Entity("A", b =>
                {
                    b.Property<int>("Id");
                    var key = b.HasKey("Id");
                    var fkProp = b.Property<int>("ParentId");
                    fk = b.Metadata.AddForeignKey(fkProp.Metadata, key.Metadata, b.Metadata);
                });

            Assert.Null(fk.Scaffolding().DependentEndNavigation);
            Assert.Null(fk.Scaffolding().PrincipalEndNavigation);

            fk.Scaffolding().PrincipalEndNavigation = "PrincipalEnd";
            fk.Scaffolding().DependentEndNavigation = "DependentEnd";
            Assert.Equal("PrincipalEnd", fk.Scaffolding().PrincipalEndNavigation);
            Assert.Equal("DependentEnd", fk.Scaffolding().DependentEndNavigation);

            fk.Scaffolding().PrincipalEndNavigation = null;
            fk.Scaffolding().DependentEndNavigation = null;
            Assert.Null(fk.Scaffolding().DependentEndNavigation);
            Assert.Null(fk.Scaffolding().PrincipalEndNavigation);
        }

        [Fact]
        public void It_sets_gets_entity_type_errors()
        { 
            var model = new Model();

            model.Scaffolding().EntityTypeErrors.Add("ET", "FAIL!");
            Assert.Equal("FAIL!", model.Scaffolding().EntityTypeErrors["ET"]);

            model.Scaffolding().EntityTypeErrors = new Dictionary<string,string>();
            Assert.Empty(model.Scaffolding().EntityTypeErrors.Values);

            model.Scaffolding().EntityTypeErrors["ET"] = "FAIL 2!";
            model.Scaffolding().EntityTypeErrors.Clear();
            Assert.Empty(model.Scaffolding().EntityTypeErrors.Values);
        }
    }
}
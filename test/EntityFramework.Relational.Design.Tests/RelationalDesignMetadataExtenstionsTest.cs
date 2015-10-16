// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Design
{
    public class RelationalDesignMetadataExtenstionsTest
    {
        [Fact]
        public void It_adds_reads_nav_prop_names()
        {
            var modelBuilder = new ModelBuilder(new Metadata.Conventions.ConventionSet());
            ForeignKey fk = null; 
            modelBuilder.Entity("A", b =>
                {
                    b.Property<int>("Id");
                    var key = b.HasKey("Id");
                    var fkProp = b.Property<int>("ParentId");
                    fk = b.Metadata.AddForeignKey(fkProp.Metadata, key.Metadata, b.Metadata);
                });

            Assert.Null(fk.RelationalDesign().DependentEndNavPropName);
            Assert.Null(fk.RelationalDesign().PrincipalEndNavPropName);

            fk.RelationalDesign().PrincipalEndNavPropName = "PrincipalEnd";
            fk.RelationalDesign().DependentEndNavPropName = "DependentEnd";
            Assert.Equal("PrincipalEnd", fk.RelationalDesign().PrincipalEndNavPropName);
            Assert.Equal("DependentEnd", fk.RelationalDesign().DependentEndNavPropName);

            fk.RelationalDesign().PrincipalEndNavPropName = null;
            fk.RelationalDesign().DependentEndNavPropName = null;
            Assert.Null(fk.RelationalDesign().DependentEndNavPropName);
            Assert.Null(fk.RelationalDesign().PrincipalEndNavPropName);
        }

        [Fact]
        public void It_sets_gets_entity_type_errors()
        {
            var et = new EntityType("ET", new Metadata.Model());

            Assert.Null(et.RelationalDesign().EntityTypeError);

            et.RelationalDesign().EntityTypeError = "FAIL!";
            Assert.Equal("FAIL!", et.RelationalDesign().EntityTypeError);

            et.RelationalDesign().EntityTypeError = null;
            Assert.Null(et.RelationalDesign().EntityTypeError);
        }

        [Fact]
        public void It_sets_gets_value_gen_never()
        {
            var prop = new Property("ET", new EntityType("A", new Metadata.Model()));

            Assert.Null(prop.RelationalDesign().ExplicitValueGeneratedNever);

            prop.RelationalDesign().ExplicitValueGeneratedNever = true;
            Assert.True(prop.RelationalDesign().ExplicitValueGeneratedNever);

            prop.RelationalDesign().ExplicitValueGeneratedNever = null;
            Assert.Null(prop.RelationalDesign().ExplicitValueGeneratedNever);
        }
    }
}
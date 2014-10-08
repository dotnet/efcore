// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Metadata
{
    public class PropertyExtensionsTest
    {
        [Fact]
        public void Annotation_found_on_property_first()
        {
            var property = CreateProperty();
            property["ArnieTheAnnotation"] = "P";
            property.EntityType["ArnieTheAnnotation"] = "E";
            property.EntityType.Model["ArnieTheAnnotation"] = "M";

            Assert.Equal("P", property.FindAnnotationInHierarchy("ArnieTheAnnotation"));
        }

        [Fact]
        public void Annotation_found_on_entity_type_second()
        {
            var property = CreateProperty();
            property.EntityType["ArnieTheAnnotation"] = "E";
            property.EntityType.Model["ArnieTheAnnotation"] = "M";

            Assert.Equal("E", property.FindAnnotationInHierarchy("ArnieTheAnnotation"));
        }

        [Fact]
        public void Annotation_found_on_model_last()
        {
            var property = CreateProperty();
            property.EntityType.Model["ArnieTheAnnotation"] = "M";

            Assert.Equal("M", property.FindAnnotationInHierarchy("ArnieTheAnnotation"));
        }

        [Fact]
        public void Null_returned_if_annotation_not_found()
        {
            Assert.Null(CreateProperty().FindAnnotationInHierarchy("ArnieTheAnnotation"));
        }

        [Fact]
        public void Null_returned_if_annotation_not_found_with_no_model()
        {
            var entityType = new Model().AddEntityType("MyType");
            var property = entityType.GetOrAddProperty("MyProperty", typeof(string), shadowProperty: true);

            Assert.Null(property.FindAnnotationInHierarchy("ArnieTheAnnotation"));
        }

        [Fact]
        public void Null_returned_if_annotation_not_found_with_no_model_entity_type()
        {
            Assert.Null(new Property("MyProperty", typeof(string), new Model().AddEntityType(typeof(object))).FindAnnotationInHierarchy("ArnieTheAnnotation"));
        }

        private static Property CreateProperty()
        {
            var entityType = new Model().AddEntityType("MyType");
            var property = entityType.GetOrAddProperty("MyProperty", typeof(string), shadowProperty: true);

            return property;
        }
    }
}

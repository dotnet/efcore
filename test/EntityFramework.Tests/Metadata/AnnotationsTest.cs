// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Metadata
{
    public class AnnotationsTest
    {
        [Fact]
        public void Can_add_annotation()
        {
            var annotations = new Annotations { new Annotation("Foo", "Bar") };

            Assert.Equal("Bar", annotations["Foo"]);
        }

        [Fact]
        public void Add_duplicate_annotation_replaces_current_annotation()
        {
            var annotations = new Annotations { new Annotation("Foo", "Bar") };

            var newAnnotation = new Annotation("Foo", "Bar");
            annotations.Add(newAnnotation);

            Assert.Same(newAnnotation, annotations.Single());
        }

        [Fact]
        public void Can_remove_annotation()
        {
            var annotations = new Annotations();
            var annotation = new Annotation("Foo", "Bar");

            annotations.Add(annotation);

            Assert.Equal("Bar", annotations["Foo"]);

            annotations.Remove(annotation);

            Assert.Null(annotations["Foo"]);

            annotations.Remove(annotation); // no throw
        }

        [Fact]
        public void Can_update_existing_annotation()
        {
            var annotations = new Annotations();
            var annotation = new Annotation("Foo", "Bar");

            annotations.Add(annotation);

            Assert.Equal("Bar", annotations["Foo"]);

            annotations["Foo"] = "Baz";

            Assert.Equal("Baz", annotations["Foo"]);
        }

        [Fact]
        public void Can_get_set_model_annotations_via_indexer()
        {
            var annotations = new Annotations();

            Assert.Null(annotations["foo"]);

            annotations["foo"] = "bar";

            Assert.Equal("bar", annotations["foo"]);

            annotations["foo"] = "horse";

            Assert.Equal("horse", annotations["foo"]);

            annotations["foo"] = null;

            Assert.Null(annotations["foo"]);

            Assert.Empty(annotations);
        }

        [Fact]
        public void Annotations_are_ordered_by_name()
        {
            var annotations = new Annotations();

            var annotation1 = new Annotation("Z", "Foo");
            var annotation2 = new Annotation("A", "Bar");

            annotations.Add(annotation1);
            annotations.Add(annotation2);

            Assert.True(new[] { annotation2, annotation1 }.SequenceEqual(annotations));
        }
    }
}

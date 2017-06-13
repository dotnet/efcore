// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    public class AnnotatableTest
    {
        [Fact]
        public void Can_add_and_remove_annotation()
        {
            var annotatable = new Annotatable();
            Assert.Null(annotatable.FindAnnotation("Foo"));
            Assert.Null(annotatable.RemoveAnnotation("Foo"));

            var annotation = annotatable.AddAnnotation("Foo", "Bar");

            Assert.NotNull(annotation);
            Assert.Equal("Bar", annotation.Value);
            Assert.Equal("Bar", annotatable["Foo"]);
            Assert.Same(annotation, annotatable.FindAnnotation("Foo"));

            Assert.Same(annotation, annotatable.GetOrAddAnnotation("Foo", "Baz"));

            Assert.Equal(new[] { annotation }, annotatable.GetAnnotations().ToArray());

            Assert.Same(annotation, annotatable.RemoveAnnotation(annotation.Name));

            Assert.Empty(annotatable.GetAnnotations());
            Assert.Null(annotatable.RemoveAnnotation(annotation.Name));
            Assert.Null(annotatable["Foo"]);
            Assert.Null(annotatable.FindAnnotation("Foo"));
        }

        [Fact]
        public void Addind_duplicate_annotation_throws()
        {
            var annotatable = new Annotatable();

            annotatable.AddAnnotation("Foo", "Bar");

            Assert.Equal(
                CoreStrings.DuplicateAnnotation("Foo"),
                Assert.Throws<InvalidOperationException>(() => annotatable.AddAnnotation("Foo", "Bar")).Message);
        }

        [Fact]
        public void Can_get_and_set_model_annotations()
        {
            var annotatable = new Annotatable();
            var annotation = annotatable.GetOrAddAnnotation("Foo", "Bar");

            Assert.NotNull(annotation);
            Assert.Same(annotation, annotatable.FindAnnotation("Foo"));
            Assert.Same(annotation, annotatable.GetAnnotation("Foo"));
            Assert.Null(annotatable["foo"]);
            Assert.Null(annotatable.FindAnnotation("foo"));

            annotatable["Foo"] = "horse";

            Assert.Equal("horse", annotatable["Foo"]);

            annotatable["Foo"] = null;

            Assert.Null(annotatable["Foo"]);
            Assert.Empty(annotatable.GetAnnotations());

            Assert.Equal(
                CoreStrings.AnnotationNotFound("Foo"),
                Assert.Throws<InvalidOperationException>(() => annotatable.GetAnnotation("Foo")).Message);
        }

        [Fact]
        public void Annotations_are_ordered_by_name()
        {
            var annotatable = new Annotatable();

            var annotation1 = annotatable.AddAnnotation("Z", "Foo");
            var annotation2 = annotatable.AddAnnotation("A", "Bar");

            Assert.True(new[] { annotation2, annotation1 }.SequenceEqual(annotatable.GetAnnotations()));
        }
    }
}

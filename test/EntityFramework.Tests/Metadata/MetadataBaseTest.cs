// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Metadata
{
    public class MetadataBaseTest
    {
        #region Fixture

        private class ConcreteMetadata : MetadataBase
        {
        }

        #endregion

        [Fact]
        public void Can_add_and_remove_annotation()
        {
            var metadataBase = new ConcreteMetadata();
            Assert.Null(metadataBase.TryGetAnnotation("Foo"));
            Assert.Null(metadataBase.RemoveAnnotation(new Annotation("Foo", "Bar")));

            var annotation = metadataBase.AddAnnotation("Foo", "Bar");

            Assert.NotNull(annotation);
            Assert.Equal("Bar", annotation.Value);
            Assert.Equal("Bar", metadataBase["Foo"]);
            Assert.Same(annotation, metadataBase.TryGetAnnotation("Foo"));

            Assert.Same(annotation, metadataBase.GetOrAddAnnotation("Foo", "Baz"));

            Assert.Equal(new[] { annotation }, metadataBase.Annotations.ToArray());

            Assert.Same(annotation, metadataBase.RemoveAnnotation(annotation));

            Assert.Empty(metadataBase.Annotations);
            Assert.Null(metadataBase.RemoveAnnotation(annotation));
            Assert.Null(metadataBase["Foo"]);
            Assert.Null(metadataBase.TryGetAnnotation("Foo"));
        }

        [Fact]
        public void Addind_duplicate_annotation_throws()
        {
            var metadataBase = new ConcreteMetadata();

            metadataBase.AddAnnotation("Foo", "Bar");

            Assert.Equal(
                Strings.DuplicateAnnotation("Foo"),
                Assert.Throws<InvalidOperationException>(() => metadataBase.AddAnnotation("Foo", "Bar")).Message);
        }

        [Fact]
        public void Can_get_and_set_model_annotations()
        {
            var metadataBase = new ConcreteMetadata();
            var annotation = metadataBase.GetOrAddAnnotation("Foo", "Bar");

            Assert.NotNull(annotation);
            Assert.Same(annotation, metadataBase.TryGetAnnotation("Foo"));
            Assert.Same(annotation, metadataBase.GetAnnotation("Foo"));
            Assert.Null(metadataBase["foo"]);
            Assert.Null(metadataBase.TryGetAnnotation("foo"));

            metadataBase["Foo"] = "horse";

            Assert.Equal("horse", metadataBase["Foo"]);

            metadataBase["Foo"] = null;

            Assert.Null(metadataBase["Foo"]);
            Assert.Empty(metadataBase.Annotations);

            Assert.Equal(
                Strings.AnnotationNotFound("Foo"),
                Assert.Throws<ModelItemNotFoundException>(() => metadataBase.GetAnnotation("Foo")).Message);
        }

        [Fact]
        public void Annotations_are_ordered_by_name()
        {
            var metadataBase = new ConcreteMetadata();

            var annotation1 = metadataBase.AddAnnotation("Z", "Foo");
            var annotation2 = metadataBase.AddAnnotation("A", "Bar");

            Assert.True(new[] { annotation2, annotation1 }.SequenceEqual(metadataBase.Annotations));
        }
    }
}

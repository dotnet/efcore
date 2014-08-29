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
        public void Members_check_arguments()
        {
            var metadataBase = new ConcreteMetadata();

            Assert.Equal(
                "annotation",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => metadataBase.Annotations.Add(null)).ParamName);

            Assert.Equal(
                "annotation",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => metadataBase.Annotations.Add(null)).ParamName);

            Assert.Equal(
                Strings.FormatArgumentIsEmpty("annotationName"),
                Assert.Throws<ArgumentException>(() => metadataBase[""]).Message);

            Assert.Equal(
                Strings.FormatArgumentIsEmpty("annotationName"),
                Assert.Throws<ArgumentException>(() => metadataBase[""] = "The kake is a lie").Message);

            Assert.Equal(
                "value",
                Assert.Throws<ArgumentNullException>(() => metadataBase["X"] = null).ParamName);
        }

        [Fact]
        public void Can_add_annotation()
        {
            var metadataBase = new ConcreteMetadata();

            metadataBase.Annotations.Add(new Annotation("Foo", "Bar"));

            Assert.Equal("Bar", metadataBase["Foo"]);
        }

        [Fact]
        public void Add_duplicate_annotation_replaces_current_annotation()
        {
            var metadataBase = new ConcreteMetadata();

            metadataBase.Annotations.Add(new Annotation("Foo", "Bar"));

            var newAnnotation = new Annotation("Foo", "Bar");
            metadataBase.Annotations.Add(newAnnotation);

            Assert.Same(newAnnotation, metadataBase.Annotations.Single());
        }

        [Fact]
        public void Can_remove_annotation()
        {
            var metadataBase = new ConcreteMetadata();
            var annotation = new Annotation("Foo", "Bar");

            metadataBase.Annotations.Add(annotation);

            Assert.Equal("Bar", metadataBase["Foo"]);

            metadataBase.Annotations.Remove(annotation);

            Assert.Null(metadataBase["Foo"]);

            metadataBase.Annotations.Remove(annotation); // no throw
        }

        [Fact]
        public void Can_update_existing_annotation()
        {
            var metadataBase = new ConcreteMetadata();
            var annotation = new Annotation("Foo", "Bar");

            metadataBase.Annotations.Add(annotation);

            Assert.Equal("Bar", metadataBase["Foo"]);

            metadataBase["Foo"] = "Baz";

            Assert.Equal("Baz", metadataBase["Foo"]);
        }

        [Fact]
        public void Can_get_set_model_annotations_via_indexer()
        {
            var metadataBase = new ConcreteMetadata();

            metadataBase["foo"] = "bar";

            Assert.Equal("bar", metadataBase["foo"]);
        }

        [Fact]
        public void Annotations_are_ordered_by_name()
        {
            var metadataBase = new ConcreteMetadata();

            var annotation1 = new Annotation("Z", "Foo");
            var annotation2 = new Annotation("A", "Bar");

            metadataBase.Annotations.Add(annotation1);
            metadataBase.Annotations.Add(annotation2);

            Assert.True(new[] { annotation2, annotation1 }.SequenceEqual(metadataBase.Annotations));
        }
    }
}

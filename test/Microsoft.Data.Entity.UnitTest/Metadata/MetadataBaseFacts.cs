// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using Xunit;

namespace Microsoft.Data.Entity.Metadata
{
    public class MetadataBaseFacts
    {
        #region Fixture

        private class ConcreteMetadata : MetadataBase
        {
            public ConcreteMetadata()
                : base("Test")
            {
            }

            public ConcreteMetadata(string name)
                : base(name)
            {
            }
        }

        #endregion

        [Fact]
        public void Can_set_name_via_ctor()
        {
            var metadataBase = new ConcreteMetadata("Foo");

            Assert.Equal("Foo", metadataBase.Name);
        }

        [Fact]
        public void StorageName_defaults_to_name()
        {
            var metadataBase = new ConcreteMetadata("Foo");

            Assert.Equal("Foo", metadataBase.StorageName);
        }

        [Fact]
        public void StorageName_can_be_different_from_name()
        {
            var metadataBase = new ConcreteMetadata("Foo") { StorageName = "Bar" };

            Assert.Equal("Foo", metadataBase.Name);
            Assert.Equal("Bar", metadataBase.StorageName);
        }

        [Fact]
        public void Can_add_annotation()
        {
            var metadataBase = new ConcreteMetadata();

            metadataBase.AddAnnotation(new Annotation("Foo", "Bar"));

            Assert.Equal("Bar", metadataBase["Foo"]);
        }

        [Fact]
        public void Can_remove_annotation()
        {
            var metadataBase = new ConcreteMetadata();
            var annotation = new Annotation("Foo", "Bar");

            metadataBase.AddAnnotation(annotation);

            Assert.Equal("Bar", metadataBase["Foo"]);

            metadataBase.RemoveAnnotation(annotation);

            Assert.Null(metadataBase["Foo"]);
        }

        [Fact]
        public void Can_update_existing_annotation()
        {
            var metadataBase = new ConcreteMetadata();
            var annotation = new Annotation("Foo", "Bar");

            metadataBase.AddAnnotation(annotation);

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

            metadataBase.AddAnnotation(annotation1);
            metadataBase.AddAnnotation(annotation2);

            Assert.True(new[] { annotation2, annotation1 }.SequenceEqual(metadataBase.Annotations));
        }
    }
}

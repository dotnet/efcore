// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Linq;
using Xunit;

namespace Microsoft.Data.Entity.Metadata
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
                Assert.Throws<ArgumentNullException>(() => metadataBase.AddAnnotation(null)).ParamName);

            Assert.Equal(
                "annotation",
                Assert.Throws<ArgumentNullException>(() => metadataBase.RemoveAnnotation(null)).ParamName);

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("annotationName"),
                Assert.Throws<ArgumentException>(() => metadataBase[""]).Message);

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("annotationName"),
                Assert.Throws<ArgumentException>(() => metadataBase[""] = "The kake is a lie").Message);

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("value"),
                Assert.Throws<ArgumentException>(() => metadataBase["X"] = null).Message);
        }

        [Fact]
        public void CanAddAnnotation()
        {
            var metadataBase = new ConcreteMetadata();

            metadataBase.AddAnnotation(new Annotation("Foo", "Bar"));

            Assert.Equal("Bar", metadataBase["Foo"]);
        }

        [Fact]
        public void CanRemoveAnnotation()
        {
            var metadataBase = new ConcreteMetadata();
            var annotation = new Annotation("Foo", "Bar");

            metadataBase.AddAnnotation(annotation);

            Assert.Equal("Bar", metadataBase["Foo"]);

            metadataBase.RemoveAnnotation(annotation);

            Assert.Null(metadataBase["Foo"]);
        }

        [Fact]
        public void CanUpdateExistingAnnotation()
        {
            var metadataBase = new ConcreteMetadata();
            var annotation = new Annotation("Foo", "Bar");

            metadataBase.AddAnnotation(annotation);

            Assert.Equal("Bar", metadataBase["Foo"]);

            metadataBase["Foo"] = "Baz";

            Assert.Equal("Baz", metadataBase["Foo"]);
        }

        [Fact]
        public void CanGetSetModelAnnotationsViaIndexer()
        {
            var metadataBase = new ConcreteMetadata();

            metadataBase["foo"] = "bar";

            Assert.Equal("bar", metadataBase["foo"]);
        }

        [Fact]
        public void AnnotationsAreOrderedByName()
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

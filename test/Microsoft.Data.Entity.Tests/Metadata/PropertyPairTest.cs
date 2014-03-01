// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Metadata
{
    public class PropertyPairTest
    {
        [Fact]
        public void Members_check_arguments()
        {
            Assert.Equal(
                "principal",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => new PropertyPair(null, new Mock<Property>().Object)).ParamName);
            Assert.Equal(
                "dependent",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => new PropertyPair(new Mock<Property>().Object, null)).ParamName);
        }

        [Fact]
        public void Can_create_property_pair()
        {
            var principal = new Mock<Property>().Object;
            var dependent = new Mock<Property>().Object;

            var pair = new PropertyPair(principal, dependent);

            Assert.Same(principal, pair.Principal);
            Assert.Same(dependent, pair.Dependent);

            Assert.Same(principal, ((IPropertyPair)pair).Principal);
            Assert.Same(dependent, ((IPropertyPair)pair).Dependent);
        }
    }
}

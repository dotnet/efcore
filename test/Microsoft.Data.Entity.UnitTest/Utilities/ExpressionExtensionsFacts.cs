// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Utilities
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Microsoft.Data.Core.Resources;
    using Xunit;

    public class ExpressionExtensionsFacts
    {
        [Fact]
        public void GetPropertyAccess_should_return_property_info_when_valid_property_access_expression()
        {
            Expression<Func<DateTime, int>> expression = d => d.Hour;

            var propertyInfo = expression.GetPropertyAccess();

            Assert.NotNull(propertyInfo);
            Assert.Equal("Hour", propertyInfo.Name);
        }

        [Fact]
        public void GetPropertyAccess_should_throw_when_not_property_access()
        {
            Expression<Func<DateTime, int>> expression = d => 123;

            Assert.Contains(
                Strings.InvalidPropertyExpression(expression),
                Assert.Throws<ArgumentException>(() => expression.GetPropertyAccess()).Message);
        }

        [Fact]
        public void GetPropertyAccess_should_throw_when_not_property_access_on_the_provided_argument()
        {
            var closure = DateTime.Now;
            Expression<Func<DateTime, int>> expression = d => closure.Hour;

            Assert.Contains(
                Strings.InvalidPropertyExpression(expression),
                Assert.Throws<ArgumentException>(() => expression.GetPropertyAccess()).Message);
        }

        [Fact]
        public void GetPropertyAccess_should_remove_convert()
        {
            Expression<Func<DateTime, long>> expression = d => d.Hour;

            var propertyInfo = expression.GetPropertyAccess();

            Assert.NotNull(propertyInfo);
            Assert.Equal("Hour", propertyInfo.Name);
        }

        [Fact]
        public void GetPropertyAccessList_should_return_property_info_collection()
        {
            Expression<Func<DateTime, object>> expression = d => new
            {
                d.Date,
                d.Day
            };

            var propertyInfos = expression.GetPropertyAccessList();

            Assert.NotNull(propertyInfos);
            Assert.Equal(2, propertyInfos.Count);
            Assert.Equal("Date", propertyInfos.First().Name);
            Assert.Equal("Day", propertyInfos.Last().Name);
        }

        [Fact]
        public void GetPropertyAccessList_should_throw_when_invalid_expression()
        {
            Expression<Func<DateTime, object>> expression = d => new
            {
                P = d.AddTicks(23)
            };

            Assert.Contains(
                Strings.InvalidPropertiesExpression(expression),
                Assert.Throws<ArgumentException>(() => expression.GetPropertyAccessList()).Message);
        }

        [Fact]
        public void GetPropertyAccessList_should_throw_when_property_access_not_on_the_provided_argument()
        {
            var closure = DateTime.Now;

            Expression<Func<DateTime, object>> expression = d => new
            {
                d.Date,
                closure.Day
            };

            Assert.Contains(
                Strings.InvalidPropertiesExpression(expression),
                Assert.Throws<ArgumentException>(() => expression.GetPropertyAccessList()).Message);
        }
    }
}

// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace Microsoft.Data.Entity.Utilities
{
    public class ExpressionExtensionsTest
    {
        [Fact]
        public void GetPropertyAccessShouldReturnPropertyInfoWhenValidPropertyAccessExpression()
        {
            Expression<Func<DateTime, int>> expression = d => d.Hour;

            var propertyInfo = expression.GetPropertyAccess();

            Assert.NotNull(propertyInfo);
            Assert.Equal("Hour", propertyInfo.Name);
        }

        [Fact]
        public void GetPropertyAccessShouldThrowWhenNotPropertyAccess()
        {
            Expression<Func<DateTime, int>> expression = d => 123;

            Assert.Contains(
                Strings.InvalidPropertyExpression(expression),
                Assert.Throws<ArgumentException>(() => expression.GetPropertyAccess()).Message);
        }

        [Fact]
        public void GetPropertyAccessShouldThrowWhenNotPropertyAccessOnTheProvidedArgument()
        {
            var closure = DateTime.Now;
            Expression<Func<DateTime, int>> expression = d => closure.Hour;

            Assert.Contains(
                Strings.InvalidPropertyExpression(expression),
                Assert.Throws<ArgumentException>(() => expression.GetPropertyAccess()).Message);
        }

        [Fact]
        public void GetPropertyAccessShouldRemoveConvert()
        {
            Expression<Func<DateTime, long>> expression = d => d.Hour;

            var propertyInfo = expression.GetPropertyAccess();

            Assert.NotNull(propertyInfo);
            Assert.Equal("Hour", propertyInfo.Name);
        }

        [Fact]
        public void GetPropertyAccessListShouldReturnPropertyInfoCollection()
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
        public void GetPropertyAccessListShouldThrowWhenInvalidExpression()
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
        public void GetPropertyAccessListShouldThrowWhenPropertyAccessNotOnTheProvidedArgument()
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

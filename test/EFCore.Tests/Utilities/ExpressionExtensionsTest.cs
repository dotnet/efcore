// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Utilities
{
    public class ExpressionExtensionsTest
    {
        [ConditionalFact]
        public void Get_property_access_should_return_property_info_when_valid_property_access_expression()
        {
            Expression<Func<DateTime, int>> expression = d => d.Hour;

            var propertyInfo = expression.GetPropertyAccess();

            Assert.NotNull(propertyInfo);
            Assert.Equal("Hour", propertyInfo.Name);
        }

        [ConditionalFact]
        public void Get_property_access_should_throw_when_not_property_access()
        {
            Expression<Func<DateTime, int>> expression = d => 123;

            Assert.Contains(
                CoreStrings.InvalidPropertyExpression(expression),
                Assert.Throws<ArgumentException>(() => expression.GetPropertyAccess()).Message);
        }

        [ConditionalFact]
        public void Get_property_access_should_throw_when_not_property_access_on_the_provided_argument()
        {
            var closure = DateTime.Now;
            Expression<Func<DateTime, int>> expression = d => closure.Hour;

            Assert.Contains(
                CoreStrings.InvalidPropertyExpression(expression),
                Assert.Throws<ArgumentException>(() => expression.GetPropertyAccess()).Message);
        }

        [ConditionalFact]
        public void Get_property_access_should_remove_convert()
        {
            Expression<Func<DateTime, long>> expression = d => d.Hour;

            var propertyInfo = expression.GetPropertyAccess();

            Assert.NotNull(propertyInfo);
            Assert.Equal("Hour", propertyInfo.Name);
        }

        [ConditionalFact]
        public void Get_property_access_list_should_return_property_info_collection()
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

        [ConditionalFact]
        public void Get_property_access_should_handle_convert()
        {
            Expression<Func<DateTime, object>> expression = d => d.Date;

            var propertyInfos = expression.GetPropertyAccess();

            Assert.NotNull(propertyInfos);
        }

        [ConditionalFact]
        public void Get_property_access_list_should_handle_convert()
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

        [ConditionalFact]
        public void Get_property_access_list_should_throw_when_invalid_expression()
        {
            Expression<Func<DateTime, object>> expression = d => new
            {
                P = d.AddTicks(23)
            };

            Assert.Contains(
                CoreStrings.InvalidPropertiesExpression(expression),
                Assert.Throws<ArgumentException>(() => expression.GetPropertyAccessList()).Message);
        }

        [ConditionalFact]
        public void Get_property_access_list_should_throw_when_property_access_not_on_the_provided_argument()
        {
            var closure = DateTime.Now;

            Expression<Func<DateTime, object>> expression = d => new
            {
                d.Date,
                closure.Day
            };

            Assert.Contains(
                CoreStrings.InvalidPropertiesExpression(expression),
                Assert.Throws<ArgumentException>(() => expression.GetPropertyAccessList()).Message);
        }
    }
}

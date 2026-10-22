// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.ModelBuilding;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore;

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
            CoreStrings.InvalidMemberExpression(expression),
            Assert.Throws<ArgumentException>(() => expression.GetPropertyAccess()).Message);
    }

    [ConditionalFact]
    public void Get_property_access_should_throw_when_not_property_access_on_the_provided_argument()
    {
        var closure = DateTime.Now;
        Expression<Func<DateTime, int>> expression = d => closure.Hour;

        Assert.Contains(
            CoreStrings.InvalidMemberExpression(expression),
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
        Expression<Func<DateTime, object>> expression = d => new { d.Date, d.Day };

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
        Expression<Func<DateTime, object>> expression = d => new { d.Date, d.Day };

        var propertyInfos = expression.GetPropertyAccessList();

        Assert.NotNull(propertyInfos);
        Assert.Equal(2, propertyInfos.Count);
        Assert.Equal("Date", propertyInfos.First().Name);
        Assert.Equal("Day", propertyInfos.Last().Name);
    }

    [ConditionalFact]
    public void Get_property_access_list_should_throw_when_invalid_expression()
    {
        Expression<Func<DateTime, object>> expression = d => new { P = d.AddTicks(23) };

        Assert.Contains(
            CoreStrings.InvalidMembersExpression(expression),
            Assert.Throws<ArgumentException>(() => expression.GetPropertyAccessList()).Message);
    }

    [ConditionalFact]
    public void Get_property_access_list_should_throw_when_property_access_not_on_the_provided_argument()
    {
        var closure = DateTime.Now;

        Expression<Func<DateTime, object>> expression = d => new { d.Date, closure.Day };

        Assert.Contains(
            CoreStrings.InvalidMembersExpression(expression),
            Assert.Throws<ArgumentException>(() => expression.GetPropertyAccessList()).Message);
    }

    [ConditionalFact]
    public void Get_member_access_should_return_property_info_when_valid_property_access_expression()
    {
        Expression<Func<DateTime, int>> propertyExpression = d => d.Hour;
        var memberInfo = propertyExpression.GetMemberAccess();

        Assert.NotNull(memberInfo);
        Assert.IsAssignableFrom<PropertyInfo>(memberInfo);
        Assert.Equal("Hour", memberInfo.Name);
    }

    [ConditionalFact]
    public void Get_member_access_should_return_field_info_when_valid_field_access_expression()
    {
        Expression<Func<ModelBuilderTest.EntityWithFields, int>> fieldExpression = e => e.CompanyId;
        var memberInfo = fieldExpression.GetMemberAccess();

        Assert.NotNull(memberInfo);
        Assert.IsAssignableFrom<FieldInfo>(memberInfo);
        Assert.Equal("CompanyId", memberInfo.Name);
    }

    [ConditionalFact]
    public void Get_member_access_should_throw_when_not_member_access()
    {
        Expression<Func<ModelBuilderTest.EntityWithFields, int>> expression = e => 123;

        Assert.Contains(
            CoreStrings.InvalidMemberExpression(expression),
            Assert.Throws<ArgumentException>(() => expression.GetMemberAccess()).Message);
    }

    [ConditionalFact]
    public void Get_member_access_should_throw_when_not_member_access_on_the_provided_argument()
    {
        var closure = new ModelBuilderTest.EntityWithFields
        {
            Id = 1,
            CompanyId = 100,
            TenantId = 200
        };

        Expression<Func<ModelBuilderTest.EntityWithFields, int>> expression = e => closure.CompanyId;

        Assert.Contains(
            CoreStrings.InvalidMemberExpression(expression),
            Assert.Throws<ArgumentException>(() => expression.GetMemberAccess()).Message);
    }

    [ConditionalFact]
    public void Get_member_access_should_handle_convert()
    {
        // Note: CompanyId is an int, so we are converting int -> long
        Expression<Func<ModelBuilderTest.EntityWithFields, long>> fieldExpression = e => e.CompanyId;

        var memberInfo = fieldExpression.GetMemberAccess();

        Assert.NotNull(memberInfo);
        Assert.Equal("CompanyId", memberInfo.Name);
    }

    [ConditionalFact]
    public void Get_member_access_list_should_handle_convert()
    {
        Expression<Func<ModelBuilderTest.EntityWithFields, object>> expression = e => new { e.Id, e.CompanyId };

        var memberInfos = expression.GetMemberAccessList();

        Assert.NotNull(memberInfos);
        Assert.Equal(2, memberInfos.Count);
        Assert.Equal("Id", memberInfos.First().Name);
        Assert.Equal("CompanyId", memberInfos.Last().Name);
    }

    [ConditionalFact]
    public void Get_member_access_list_should_throw_when_invalid_expression()
    {
        Expression<Func<ModelBuilderTest.EntityWithFields, object>> expression = e => new { P = e.Id + e.CompanyId };

        Assert.Contains(
            CoreStrings.InvalidMembersExpression(expression),
            Assert.Throws<ArgumentException>(() => expression.GetMemberAccessList()).Message);
    }

    [ConditionalFact]
    public void Get_member_access_list_should_throw_when_member_access_not_on_the_provided_argument()
    {
        var closure = new ModelBuilderTest.EntityWithFields
        {
            Id = 1,
            CompanyId = 100,
            TenantId = 200
        };

        Expression<Func<ModelBuilderTest.EntityWithFields, object>> expression = e => new { e.Id, closure.CompanyId };

        Assert.Contains(
            CoreStrings.InvalidMembersExpression(expression),
            Assert.Throws<ArgumentException>(() => expression.GetMemberAccessList()).Message);
    }
}

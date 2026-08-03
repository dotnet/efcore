// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.ModelBuilding;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore;

public class ExpressionExtensionsTest
{
    [Fact]
    public void Get_property_access_should_return_property_info_when_valid_property_access_expression()
    {
        Expression<Func<DateTime, int>> expression = d => d.Hour;

        var propertyInfo = expression.GetPropertyAccess();

        Assert.NotNull(propertyInfo);
        Assert.Equal("Hour", propertyInfo.Name);
    }

    [Fact]
    public void Get_property_access_should_throw_when_not_property_access()
    {
        Expression<Func<DateTime, int>> expression = d => 123;

        Assert.Contains(
            CoreStrings.InvalidMemberExpression(expression),
            Assert.Throws<ArgumentException>(() => expression.GetPropertyAccess()).Message);
    }

    [Fact]
    public void Get_property_access_should_throw_when_not_property_access_on_the_provided_argument()
    {
        var closure = DateTime.Now;
        Expression<Func<DateTime, int>> expression = d => closure.Hour;

        Assert.Contains(
            CoreStrings.InvalidMemberExpression(expression),
            Assert.Throws<ArgumentException>(() => expression.GetPropertyAccess()).Message);
    }

    [Fact]
    public void Get_property_access_should_remove_convert()
    {
        Expression<Func<DateTime, long>> expression = d => d.Hour;

        var propertyInfo = expression.GetPropertyAccess();

        Assert.NotNull(propertyInfo);
        Assert.Equal("Hour", propertyInfo.Name);
    }

    [Fact]
    public void Get_property_access_list_should_return_property_info_collection()
    {
        Expression<Func<DateTime, object>> expression = d => new { d.Date, d.Day };

        var propertyInfos = expression.GetPropertyAccessList();

        Assert.NotNull(propertyInfos);
        Assert.Equal(2, propertyInfos.Count);
        Assert.Equal("Date", propertyInfos.First().Name);
        Assert.Equal("Day", propertyInfos.Last().Name);
    }

    [Fact]
    public void Get_property_access_should_handle_convert()
    {
        Expression<Func<DateTime, object>> expression = d => d.Date;

        var propertyInfos = expression.GetPropertyAccess();

        Assert.NotNull(propertyInfos);
    }

    [Fact]
    public void Get_property_access_list_should_handle_convert()
    {
        Expression<Func<DateTime, object>> expression = d => new { d.Date, d.Day };

        var propertyInfos = expression.GetPropertyAccessList();

        Assert.NotNull(propertyInfos);
        Assert.Equal(2, propertyInfos.Count);
        Assert.Equal("Date", propertyInfos.First().Name);
        Assert.Equal("Day", propertyInfos.Last().Name);
    }

    [Fact]
    public void Get_property_access_list_should_throw_when_invalid_expression()
    {
        Expression<Func<DateTime, object>> expression = d => new { P = d.AddTicks(23) };

        Assert.Contains(
            CoreStrings.InvalidMembersExpression(expression),
            Assert.Throws<ArgumentException>(() => expression.GetPropertyAccessList()).Message);
    }

    [Fact]
    public void Get_property_access_list_should_throw_when_property_access_not_on_the_provided_argument()
    {
        var closure = DateTime.Now;

        Expression<Func<DateTime, object>> expression = d => new { d.Date, closure.Day };

        Assert.Contains(
            CoreStrings.InvalidMembersExpression(expression),
            Assert.Throws<ArgumentException>(() => expression.GetPropertyAccessList()).Message);
    }

    [Fact]
    public void Get_member_access_should_return_property_info_when_valid_property_access_expression()
    {
        Expression<Func<DateTime, int>> propertyExpression = d => d.Hour;
        var memberInfo = propertyExpression.GetMemberAccess();

        Assert.NotNull(memberInfo);
        Assert.IsAssignableFrom<PropertyInfo>(memberInfo);
        Assert.Equal("Hour", memberInfo.Name);
    }

    [Fact]
    public void Get_member_access_should_return_field_info_when_valid_field_access_expression()
    {
        Expression<Func<ModelBuilderTest.EntityWithFields, int>> fieldExpression = e => e.CompanyId;
        var memberInfo = fieldExpression.GetMemberAccess();

        Assert.NotNull(memberInfo);
        Assert.IsAssignableFrom<FieldInfo>(memberInfo);
        Assert.Equal("CompanyId", memberInfo.Name);
    }

    [Fact]
    public void Get_member_access_should_throw_when_not_member_access()
    {
        Expression<Func<ModelBuilderTest.EntityWithFields, int>> expression = e => 123;

        Assert.Contains(
            CoreStrings.InvalidMemberExpression(expression),
            Assert.Throws<ArgumentException>(() => expression.GetMemberAccess()).Message);
    }

    [Fact]
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

    [Fact]
    public void Get_member_access_should_handle_convert()
    {
        // Note: CompanyId is an int, so we are converting int -> long
        Expression<Func<ModelBuilderTest.EntityWithFields, long>> fieldExpression = e => e.CompanyId;

        var memberInfo = fieldExpression.GetMemberAccess();

        Assert.NotNull(memberInfo);
        Assert.Equal("CompanyId", memberInfo.Name);
    }

    [Fact]
    public void Get_member_access_list_should_handle_convert()
    {
        Expression<Func<ModelBuilderTest.EntityWithFields, object>> expression = e => new { e.Id, e.CompanyId };

        var memberInfos = expression.GetMemberAccessList();

        Assert.NotNull(memberInfos);
        Assert.Equal(2, memberInfos.Count);
        Assert.Equal("Id", memberInfos.First().Name);
        Assert.Equal("CompanyId", memberInfos.Last().Name);
    }

    [Fact]
    public void Get_member_access_list_should_throw_when_invalid_expression()
    {
        Expression<Func<ModelBuilderTest.EntityWithFields, object>> expression = e => new { P = e.Id + e.CompanyId };

        Assert.Contains(
            CoreStrings.InvalidMembersExpression(expression),
            Assert.Throws<ArgumentException>(() => expression.GetMemberAccessList()).Message);
    }

    [Fact]
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

    private sealed class ComplexBlog
    {
        public string Title { get; set; }
        public List<ComplexPost> Posts { get; set; } = [];
        public ComplexPost[] PostArray { get; set; } = [];
    }

    private sealed class ComplexPost
    {
        public string Title { get; set; }
        public List<ComplexComment> Comments { get; set; } = [];
    }

    private sealed class ComplexComment
    {
        public string Text { get; set; }
    }

    [Fact]
    public void MatchComplexMemberAccessList_handles_simple_member()
    {
        Expression<Func<ComplexBlog, object>> expression = b => b.Title;

        var (members, isCollection, collectionIndices) = expression.MatchComplexMemberAccessList(nameof(expression));

        var leaf = Assert.Single(members);
        Assert.Equal(["Title"], leaf.Select(m => m.Name));
        Assert.Null(isCollection);
        Assert.Null(collectionIndices);
    }

    [Fact]
    public void MatchComplexMemberAccessList_handles_select_over_complex_collection()
    {
        Expression<Func<ComplexBlog, object>> expression = b => b.Posts.Select(p => p.Title);

        var (members, isCollection, collectionIndices) = expression.MatchComplexMemberAccessList(nameof(expression));

        var leaf = Assert.Single(members);
        Assert.Equal(["Posts", "Title"], leaf.Select(m => m.Name));
        Assert.NotNull(isCollection);
        Assert.Equal([true, false], Assert.Single(isCollection));
        Assert.NotNull(collectionIndices);
        Assert.Equal([null], Assert.Single(collectionIndices));
    }

    [Fact]
    public void MatchComplexMemberAccessList_handles_nested_select()
    {
        Expression<Func<ComplexBlog, object>> expression =
            b => b.Posts.Select(p => p.Comments.Select(c => c.Text));

        var (members, isCollection, collectionIndices) = expression.MatchComplexMemberAccessList(nameof(expression));

        var leaf = Assert.Single(members);
        Assert.Equal(["Posts", "Comments", "Text"], leaf.Select(m => m.Name));
        Assert.NotNull(isCollection);
        Assert.Equal(new[] { true, true, false }, Assert.Single(isCollection));
        Assert.NotNull(collectionIndices);
        Assert.Equal(new int?[] { null, null }, Assert.Single(collectionIndices));
    }

    [Fact]
    public void MatchComplexMemberAccessList_handles_list_indexer()
    {
        Expression<Func<ComplexBlog, object>> expression = b => b.Posts[0].Title;

        var (members, isCollection, collectionIndices) = expression.MatchComplexMemberAccessList(nameof(expression));

        var leaf = Assert.Single(members);
        Assert.Equal(["Posts", "Title"], leaf.Select(m => m.Name));
        Assert.NotNull(isCollection);
        Assert.Equal([true, false], Assert.Single(isCollection));
        Assert.NotNull(collectionIndices);
        Assert.Equal(new int?[] { 0 }, Assert.Single(collectionIndices));
    }

    [Fact]
    public void MatchComplexMemberAccessList_handles_array_indexer()
    {
        Expression<Func<ComplexBlog, object>> expression = b => b.PostArray[2].Title;

        var (members, isCollection, collectionIndices) = expression.MatchComplexMemberAccessList(nameof(expression));

        var leaf = Assert.Single(members);
        Assert.Equal(["PostArray", "Title"], leaf.Select(m => m.Name));
        Assert.NotNull(isCollection);
        Assert.Equal([true, false], Assert.Single(isCollection));
        Assert.NotNull(collectionIndices);
        Assert.Equal(new int?[] { 2 }, Assert.Single(collectionIndices));
    }

    [Fact]
    public void MatchComplexMemberAccessList_handles_anonymous_with_mixed_leaves()
    {
        Expression<Func<ComplexBlog, object>> expression =
            b => new { b.Title, Names = b.Posts.Select(p => p.Title) };

        var (members, isCollection, collectionIndices) = expression.MatchComplexMemberAccessList(nameof(expression));

        Assert.Equal(2, members.Count);
        Assert.Equal(["Title"], members[0].Select(m => m.Name));
        Assert.Equal(["Posts", "Title"], members[1].Select(m => m.Name));
        Assert.NotNull(isCollection);
        Assert.Equal([false], isCollection[0]);
        Assert.Equal([true, false], isCollection[1]);
        Assert.NotNull(collectionIndices);
        Assert.Null(collectionIndices[0]);
        Assert.Equal([null], collectionIndices[1]);
    }

    [Fact]
    public void MatchComplexMemberAccessList_throws_for_non_constant_indexer()
    {
        var idx = 1;
        Expression<Func<ComplexBlog, object>> expression = b => b.Posts[idx].Title;

        Assert.Throws<ArgumentException>(() => expression.MatchComplexMemberAccessList(nameof(expression)));
    }

    [Fact]
    public void MatchComplexMemberAccessList_throws_for_unrelated_call()
    {
        Expression<Func<ComplexBlog, object>> expression = b => b.Posts.First().Title;

        Assert.Throws<ArgumentException>(() => expression.MatchComplexMemberAccessList(nameof(expression)));
    }
}

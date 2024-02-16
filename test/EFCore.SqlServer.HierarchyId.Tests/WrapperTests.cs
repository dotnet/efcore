// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer;

public class WrapperTests
{
    [ConditionalTheory]
    [InlineData(null, 1)]
    [InlineData("/", 1)]
    [InlineData("/0.5/", 1)]
    [InlineData("/1/", 0)]
    [InlineData("/2/", -1)]
    [InlineData("/1/1/", -1)]
    public void CompareTo_works(string value, int expected)
        => Assert.Equal(expected, HierarchyId.Parse("/1/").CompareTo(HierarchyId.Parse(value)));

    [ConditionalTheory]
    [InlineData(null, false)]
    [InlineData("/", false)]
    [InlineData("/1/", true)]
    public void Equals_works(string value, bool expected)
        => Assert.Equal(expected, HierarchyId.Parse("/1/").Equals(HierarchyId.Parse(value)));

    [ConditionalFact]
    public void GetAncestor_returns_null_when_too_high()
        => Assert.Null(HierarchyId.Parse("/1/").GetAncestor(2));

    [ConditionalFact]
    public void GetReparentedValue_returns_null_when_newRoot_is_null()
        => Assert.Null(HierarchyId.Parse("/1/").GetReparentedValue(HierarchyId.GetRoot(), newRoot: null));

    [ConditionalFact]
    public void IsDescendantOf_returns_false_when_parent_is_null()
        => Assert.False(HierarchyId.Parse("/1/").IsDescendantOf(null));

    [ConditionalFact]
    public void Parse_overloads_works_when_parentId_is_simpleId()
        => Assert.Equal(HierarchyId.Parse(_parent, 2), HierarchyId.Parse("/1/2/"));

    [ConditionalFact]
    public void Parse_overloads_works_when_parentId_is_dottedString()
        => Assert.Equal(HierarchyId.Parse(_parent, 2,1), HierarchyId.Parse("/1/2.1/"));

    [ConditionalFact]
    public void Parse_overloads_works_when_parentId_is_empty()
        => Assert.Equal(HierarchyId.Parse(_parent), HierarchyId.Parse("/1/"));

    [ConditionalFact]
    public void Parse_overloads_works_when_parentHierarchy_is_root_and_parentId_is_simple()
        => Assert.Equal(HierarchyId.Parse(HierarchyId.GetRoot(),1), HierarchyId.Parse("/1/"));

    [ConditionalFact]
    public void Parse_overloads_works_when_parentHierarchy_is_root_and_parentId_is_empty()
        => Assert.Equal(HierarchyId.Parse(HierarchyId.GetRoot()), HierarchyId.Parse("/"));

    [ConditionalFact]
    public void Parse_overloads_works_when_parentHierarchy_is_null_and_parentId_is_empty()
        => Assert.Equal(HierarchyId.Parse(null,[]), HierarchyId.Parse("/"));

    private readonly HierarchyId _parent = HierarchyId.Parse("/1/");
}

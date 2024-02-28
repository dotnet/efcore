// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.SqlServer.Types;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Represents a position in a hierarchical structure, specifying depth and breadth.
/// </summary>
[JsonConverter(typeof(HierarchyIdJsonConverter))]
public class HierarchyId : IComparable<HierarchyId>
{
    private readonly SqlHierarchyId _value;

    /// <summary>
    ///     Initializes a new instance of the<see cref="HierarchyId" /> class. Equivalent to <see cref="GetRoot" />.
    /// </summary>
    public HierarchyId()
        : this(SqlHierarchyId.GetRoot())
    {
    }

    /// <summary>
    ///     Initializes a new instance of the<see cref="HierarchyId" /> class. Equivalent to <see cref="Parse(string?)" />.
    /// </summary>
    /// <param name="value">The string representation of the node.</param>
    public HierarchyId(string value)
        : this(SqlHierarchyId.Parse(value))
    {
    }

    /// <summary>
    ///     Initializes a new instance of the<see cref="HierarchyId" /> class.
    /// </summary>
    /// <param name="value">The <see cref="SqlHierarchyId" /> representation of the node.</param>
    public HierarchyId(SqlHierarchyId value)
    {
        if (value.IsNull)
        {
            throw new ArgumentNullException(nameof(value));
        }

        _value = value;
    }

    /// <summary>
    ///     Gets the root node of the hierarchy.
    /// </summary>
    /// <returns>The root node of the hierarchy.</returns>
    public static HierarchyId GetRoot()
        => ((HierarchyId?)SqlHierarchyId.GetRoot())!;

    /// <summary>
    ///     Converts the canonical string representation of a node to a <see cref="HierarchyId" /> value.
    /// </summary>
    /// <param name="input">The string representation of a node.</param>
    /// <returns>A <see cref="HierarchyId" /> value.</returns>
    [return: NotNullIfNotNull(nameof(input))]
    public static HierarchyId? Parse(string? input)
        => (HierarchyId?)SqlHierarchyId.Parse(input);

    /// <summary>
    /// Converts the <paramref name= "parentHierarchyId" /> and <paramref name= "parentId" /> of a node to a <see cref="HierarchyId" /> value.
    /// </summary>
    /// <param name="parentHierarchyId">The parent HierarchyId of node.</param>
    /// <param name="parentId">The parent Id of current node. It can be more than one element if want have path like: "/1/2/3.1/", otherwise one element for have path like: "/1/2/3/".</param>
    /// <returns>A <see cref="HierarchyId" /> value.</returns>
    public static HierarchyId Parse(HierarchyId parentHierarchyId , params int[] parentId)
        => GenerateHierarchyIdBasedOnParent(parentHierarchyId, parentId);

    //This Method can move to "SqlHierarchyId in Microsoft.SqlServer.Types", if we don't want put it in this abstraction.
    private static HierarchyId GenerateHierarchyIdBasedOnParent(HierarchyId parent, params int[] parentId)
    {
        if (parent is null)
        {
            return HierarchyId.GetRoot();
        }

        if (parentId.Length < 1)
        {
            return parent;
        }

        var specificPath = new StringBuilder(parent.ToString());
        specificPath.Append(string.Join(".", parentId));
        specificPath.Append('/');

        return HierarchyId.Parse(specificPath.ToString());
    }

    /// <inheritdoc />
    public virtual int CompareTo(HierarchyId? other)
        => _value.CompareTo((SqlHierarchyId)other);

    /// <inheritdoc />
    public override bool Equals(object? other)
        => other is HierarchyId or null
            ? Equals((SqlHierarchyId)(HierarchyId?)other)
            : _value.Equals(other);

    /// <summary>
    ///     Gets the node <paramref name="n" /> levels up the hierarchical tree.
    /// </summary>
    /// <param name="n">The number of levels to ascend in the hierarchy.</param>
    /// <returns>
    ///     A <see cref="HierarchyId" /> value representing the <paramref name="n" />th ancestor of this node or null if <paramref name="n" />
    ///     is greater than <see cref="GetLevel" />.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="n" /> is negative.</exception>
    public virtual HierarchyId? GetAncestor(int n)
        => (HierarchyId?)_value.GetAncestor(n);

    /// <summary>
    ///     Gets a value for a new descendant node that is greater than <paramref name="child" />.
    /// </summary>
    /// <param name="child">The lower bound. Use the last descendant to ensure the new value doesn't conflict with existing children. Can be null.</param>
    /// <returns>A new <see cref="HierarchyId" /> value.</returns>
    public virtual HierarchyId GetDescendant(HierarchyId? child)
        => GetDescendant(child, null);

    /// <summary>
    ///     Gets a value for a new descendant node that is greater than <paramref name="child1" /> and less than <paramref name="child2" />. Use to
    ///     insert a new node between two existing children.
    /// </summary>
    /// <param name="child1">The lower bound.</param>
    /// <param name="child2">The upper bound.</param>
    /// <returns>A new <see cref="HierarchyId" /> value.</returns>
    public virtual HierarchyId GetDescendant(HierarchyId? child1, HierarchyId? child2)
        => ((HierarchyId?)_value.GetDescendant(child1, child2))!;

    /// <inheritdoc />
    public override int GetHashCode()
        => _value.GetHashCode();

    /// <summary>
    ///     Gets the level of this node in the hierarchical tree.
    /// </summary>
    /// <returns>The depth of this node. The root node is level 0.</returns>
    public virtual short GetLevel()
        => (short)_value.GetLevel();

    /// <summary>
    ///     Gets a value representing the location of a new node that has a path from <paramref name="newRoot" /> equal to the path from
    ///     <paramref name="oldRoot" /> to this, effectively moving this to the new location.
    /// </summary>
    /// <param name="oldRoot">An ancestor of this node specifying the endpoint of the path segment to be moved.</param>
    /// <param name="newRoot">The node that represents the new ancestor.</param>
    /// <returns>A <see cref="HierarchyId" /> value or null if <paramref name="oldRoot" /> or <paramref name="newRoot" /> is null.</returns>
    public virtual HierarchyId? GetReparentedValue(HierarchyId? oldRoot, HierarchyId? newRoot)
        => (HierarchyId?)_value.GetReparentedValue(oldRoot, newRoot);

    /// <summary>
    ///     Gets a value indicating whether this node is a descendant of <paramref name="parent" />.
    /// </summary>
    /// <param name="parent">The parent to test against.</param>
    /// <returns>True if this node is in the sub-tree rooted at <paramref name="parent" />; otherwise false.</returns>
    public virtual bool IsDescendantOf(HierarchyId? parent)
        => _value.IsDescendantOf(parent).IsTrue;

    /// <summary>
    ///     Returns the canonical string representation of a node.
    /// </summary>
    /// <returns>The string representation of a node.</returns>
    public override string ToString()
        => _value.ToString();

    /// <summary>
    ///     Evaluates whether two nodes are equal.
    /// </summary>
    /// <param name="hid1">The first node to compare.</param>
    /// <param name="hid2">The second node to compare.</param>
    /// <returns>True if <paramref name="hid1" /> and <paramref name="hid2" /> are equal; otherwise, false.</returns>
    public static bool operator ==(HierarchyId? hid1, HierarchyId? hid2)
        => ((SqlHierarchyId)hid1).CompareTo(hid2) == 0;

    /// <summary>
    ///     Evaluates whether two nodes are unequal.
    /// </summary>
    /// <param name="hid1">The first node to compare.</param>
    /// <param name="hid2">The second node to compare.</param>
    /// <returns>True if <paramref name="hid1" /> and <paramref name="hid2" /> are unequal; otherwise, false.</returns>
    public static bool operator !=(HierarchyId? hid1, HierarchyId? hid2)
        => ((SqlHierarchyId)hid1).CompareTo(hid2) != 0;

    /// <summary>
    ///     Evaluates whether one node is less than another.
    /// </summary>
    /// <param name="hid1">The first node to compare.</param>
    /// <param name="hid2">The second node to compare.</param>
    /// <returns>True if <paramref name="hid1" /> is less than <paramref name="hid2" />; otherwise, false.</returns>
    public static bool operator <(HierarchyId? hid1, HierarchyId? hid2)
        => ((SqlHierarchyId)hid1).CompareTo(hid2) < 0;

    /// <summary>
    ///     Evaluates whether one node is greater than another.
    /// </summary>
    /// <param name="hid1">The first node to compare.</param>
    /// <param name="hid2">The second node to compare.</param>
    /// <returns>True if <paramref name="hid1" /> is greater than <paramref name="hid2" />; otherwise, false.</returns>
    public static bool operator >(HierarchyId? hid1, HierarchyId? hid2)
        => ((SqlHierarchyId)hid1).CompareTo(hid2) > 0;

    /// <summary>
    ///     Evaluates whether one node is less than or equal to another.
    /// </summary>
    /// <param name="hid1">The first node to compare.</param>
    /// <param name="hid2">The second node to compare.</param>
    /// <returns>True if <paramref name="hid1" /> is less than or equal to <paramref name="hid2" />; otherwise, false.</returns>
    public static bool operator <=(HierarchyId? hid1, HierarchyId? hid2)
        => ((SqlHierarchyId)hid1).CompareTo(hid2) <= 0;

    /// <summary>
    ///     Evaluates whether one node is greater than or equal to another.
    /// </summary>
    /// <param name="hid1">The first node to compare.</param>
    /// <param name="hid2">The second node to compare.</param>
    /// <returns>True if <paramref name="hid1" /> is greater than or equal to <paramref name="hid2" />; otherwise, false.</returns>
    public static bool operator >=(HierarchyId? hid1, HierarchyId? hid2)
        => ((SqlHierarchyId)hid1).CompareTo(hid2) >= 0;

    /// <summary>
    ///     Converts a <see cref="HierarchyId" /> value to a <see cref="SqlHierarchyId" /> value.
    /// </summary>
    /// <param name="value">The <see cref="HierarchyId" /> value.</param>
    /// <returns>The underlying <see cref="SqlHierarchyId" /> value.</returns>
    public static implicit operator SqlHierarchyId(HierarchyId? value)
        => value?._value ?? SqlHierarchyId.Null;

    /// <summary>
    ///     Converts a <see cref="SqlHierarchyId" /> value to a <see cref="HierarchyId" /> value.
    /// </summary>
    /// <param name="value">The <see cref="SqlHierarchyId" /> value.</param>
    /// <returns>A new <see cref="HierarchyId" /> value.</returns>
    public static explicit operator HierarchyId?(SqlHierarchyId value)
        => value.IsNull ? null : new HierarchyId(value);
}

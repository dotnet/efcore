// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.SqlServer.Types;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
/// Represents a position in a hierarchical structure, specifying depth and breadth.
/// </summary>
public class HierarchyId : IComparable
{
    private SqlHierarchyId _value;

    private HierarchyId(SqlHierarchyId value)
    {
        if (value.IsNull)
        {
            throw new ArgumentNullException(nameof(value));
        }

        _value = value;
    }

    /// <summary>
    /// Gets the root node of the hierarchy.
    /// </summary>
    /// <returns>The root node of the hierarchy.</returns>
    public static HierarchyId GetRoot()
        => new HierarchyId(SqlHierarchyId.GetRoot());

    /// <summary>
    /// Converts the canonical string representation of a node to a <see cref="HierarchyId"/> value.
    /// </summary>
    /// <param name="input">The string representation of a node.</param>
    /// <returns>A <see cref="HierarchyId"/> value.</returns>
    [return: NotNullIfNotNull(nameof(input))]
    public static HierarchyId? Parse(string? input)
        => Wrap(SqlHierarchyId.Parse(input));

    /// <summary>
    /// Reads a <see cref="HierarchyId"/> value from the specified reader.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <returns>A <see cref="HierarchyId"/> value.</returns>
    public static HierarchyId? Read(BinaryReader reader)
    {
        var hid = new SqlHierarchyId();
        hid.Read(reader);
        return Wrap(hid);
    }

    /// <summary>
    /// Writes this <see cref="HierarchyId"/> value to the specified writer.
    /// </summary>
    /// <param name="writer">The writer.</param>
    public void Write(BinaryWriter writer)
    {
        _value.Write(writer);
    }

    /// <inheritdoc/>
    public int CompareTo(object? obj)
        => _value.CompareTo(
            obj is HierarchyId or null
                ? Unwrap((HierarchyId?)obj)
                : obj);

    /// <inheritdoc/>
    public override bool Equals(object? obj)
        => _value.Equals(
                obj is HierarchyId other
                    ? other._value
                    : obj);

    /// <summary>
    /// Gets the node <paramref name="n"/> levels up the hierarchical tree.
    /// </summary>
    /// <param name="n">The number of levels to ascend in the hierarchy.</param>
    /// <returns>A <see cref="HierarchyId"/> value representing the <paramref name="n"/>th ancestor of this node or null if <paramref name="n"/> is greater than <see cref="GetLevel"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="n"/> is negative.</exception>
    public HierarchyId GetAncestor(int n)
        => Wrap(_value.GetAncestor(n))!;

    /// <summary>
    /// Gets the value of a descendant node that is greater than <paramref name="child1"/> and less than <paramref name="child2"/>.
    /// </summary>
    /// <param name="child1">The lower bound.</param>
    /// <param name="child2">The upper bound.</param>
    /// <returns>A <see cref="HierarchyId"/> value.</returns>
    public HierarchyId GetDescendant(HierarchyId? child1, HierarchyId? child2)
        => Wrap(_value.GetDescendant(Unwrap(child1), Unwrap(child2)))!;

    /// <inheritdoc/>
    public override int GetHashCode()
        => _value.GetHashCode();

    /// <summary>
    /// Gets the level of this node in the hierarchical tree.
    /// </summary>
    /// <returns>The depth of this node. The root node is level 0.</returns>
    public short GetLevel()
        => _value.GetLevel().Value;

    /// <summary>
    /// Gets a value representing the location of a new node that has a path from <paramref name="newRoot"/> equal to the path from <paramref name="oldRoot"/> to this, effectively moving this to the new location.
    /// </summary>
    /// <param name="oldRoot">An ancestor of this node specifying the endpoint of the path segment to be moved.</param>
    /// <param name="newRoot">The node that represents the new ancestor.</param>
    /// <returns>A <see cref="HierarchyId"/> value or null if <paramref name="oldRoot"/> or <paramref name="newRoot"/> is null.</returns>
    public HierarchyId? GetReparentedValue(HierarchyId? oldRoot, HierarchyId? newRoot)
        => Wrap(_value.GetReparentedValue(Unwrap(oldRoot), Unwrap(newRoot)));

    /// <summary>
    /// Gets a value indicating whether this node is a descendant of <paramref name="parent"/>.
    /// </summary>
    /// <param name="parent">The parent to test against.</param>
    /// <returns>True if this node is in the sub-tree rooted at <paramref name="parent"/>; otherwise false.</returns>
    public bool IsDescendantOf(HierarchyId? parent)
        => _value.IsDescendantOf(Unwrap(parent)).IsTrue;

    /// <inheritdoc/>
    public override string ToString()
        => _value.ToString();

    /// <summary>
    /// Evaluates whether two nodes are equal.
    /// </summary>
    /// <param name="hid1">The first node to compare.</param>
    /// <param name="hid2">The second node to compare.</param>
    /// <returns>True if <paramref name="hid1"/> and <paramref name="hid2"/> are equal; otherwise, false.</returns>
    public static bool operator ==(HierarchyId? hid1, HierarchyId? hid2)
    {
        var sh1 = Unwrap(hid1);
        var sh2 = Unwrap(hid2);

        return sh1.IsNull == sh2.IsNull && sh1.CompareTo(sh2) == 0;
    }

    /// <summary>
    /// Evaluates whether two nodes are unequal.
    /// </summary>
    /// <param name="hid1">The first node to compare.</param>
    /// <param name="hid2">The second node to compare.</param>
    /// <returns>True if <paramref name="hid1"/> and <paramref name="hid2"/> are unequal; otherwise, false.</returns>
    public static bool operator !=(HierarchyId? hid1, HierarchyId? hid2)
    {
        var sh1 = Unwrap(hid1);
        var sh2 = Unwrap(hid2);

        return sh1.IsNull != sh2.IsNull || sh1.CompareTo(sh2) != 0;
    }

    /// <summary>
    /// Evaluates whether one node is less than another.
    /// </summary>
    /// <param name="hid1">The first node to compare.</param>
    /// <param name="hid2">The second node to compare.</param>
    /// <returns>True if <paramref name="hid1"/> is less than <paramref name="hid2"/>; otherwise, false.</returns>
    public static bool operator <(HierarchyId? hid1, HierarchyId? hid2)
    {
        var sh1 = Unwrap(hid1);
        var sh2 = Unwrap(hid2);

        return !sh1.IsNull && !sh2.IsNull && sh1.CompareTo(sh2) < 0;
    }

    /// <summary>
    /// Evaluates whether one node is greater than another.
    /// </summary>
    /// <param name="hid1">The first node to compare.</param>
    /// <param name="hid2">The second node to compare.</param>
    /// <returns>True if <paramref name="hid1"/> is greater than <paramref name="hid2"/>; otherwise, false.</returns>
    public static bool operator >(HierarchyId? hid1, HierarchyId? hid2)
    {
        var sh1 = Unwrap(hid1);
        var sh2 = Unwrap(hid2);

        return !sh1.IsNull && !sh2.IsNull && sh1.CompareTo(sh2) > 0;
    }

    /// <summary>
    /// Evaluates whether one node is less than or equal to another.
    /// </summary>
    /// <param name="hid1">The first node to compare.</param>
    /// <param name="hid2">The second node to compare.</param>
    /// <returns>True if <paramref name="hid1"/> is less than or equal to <paramref name="hid2"/>; otherwise, false.</returns>
    public static bool operator <=(HierarchyId? hid1, HierarchyId? hid2)
    {
        var sh1 = Unwrap(hid1);
        var sh2 = Unwrap(hid2);

        return !sh1.IsNull && !sh2.IsNull && sh1.CompareTo(sh2) <= 0;
    }

    /// <summary>
    /// Evaluates whether one node is greater than or equal to another.
    /// </summary>
    /// <param name="hid1">The first node to compare.</param>
    /// <param name="hid2">The second node to compare.</param>
    /// <returns>True if <paramref name="hid1"/> is greater than or equal to <paramref name="hid2"/>; otherwise, false.</returns>
    public static bool operator >=(HierarchyId? hid1, HierarchyId? hid2)
    {
        var sh1 = Unwrap(hid1);
        var sh2 = Unwrap(hid2);

        return !sh1.IsNull && !sh2.IsNull && sh1.CompareTo(sh2) >= 0;
    }

    private static SqlHierarchyId Unwrap(HierarchyId? value)
        => value?._value ?? SqlHierarchyId.Null;

    private static HierarchyId? Wrap(SqlHierarchyId value)
        => value.IsNull ? null : new HierarchyId(value);
}

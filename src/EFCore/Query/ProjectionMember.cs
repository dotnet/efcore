// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     <para>
///         A class representing a chain of CLR members to bind. Usually generated from successive Select calls in the query.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     and <see href="https://aka.ms/efcore-docs-how-query-works">How EF Core queries work</see> for more information and examples.
/// </remarks>
public sealed class ProjectionMember
{
    private readonly IList<MemberInfo> _memberChain;

    /// <summary>
    ///     Creates a new instance of the <see cref="ProjectionMember" /> class with empty MemberInfo chain.
    /// </summary>
    public ProjectionMember()
    {
        _memberChain = new List<MemberInfo>();
    }

    private ProjectionMember(IList<MemberInfo> memberChain)
    {
        _memberChain = memberChain;
    }

    /// <summary>
    ///     Append given MemberInfo to existing chain at the end.
    /// </summary>
    /// <param name="member">The MemberInfo to append.</param>
    /// <returns>A new projection member with given member info appended to existing chain.</returns>
    public ProjectionMember Append(MemberInfo member)
    {
        var existingChain = _memberChain.ToList();
        existingChain.Add(member);

        return new ProjectionMember(existingChain);
    }

    /// <summary>
    ///     Prepend given MemberInfo to existing chain at the start.
    /// </summary>
    /// <param name="member">The MemberInfo to prepend.</param>
    /// <returns>A new projection member with given member info prepended to existing chain.</returns>
    public ProjectionMember Prepend(MemberInfo member)
    {
        var existingChain = _memberChain.ToList();
        existingChain.Insert(0, member);

        return new ProjectionMember(existingChain);
    }

    /// <summary>
    ///     The last MemberInfo in the chain of MemberInfo represented by this projection member.
    /// </summary>
    /// <remarks>
    ///     This method is generally used to get last memberInfo to generate an alias for projection.
    /// </remarks>
    public MemberInfo? Last
        => _memberChain.LastOrDefault();

    /// <inheritdoc />
    [DebuggerStepThrough]
    public override int GetHashCode()
    {
        var hash = new HashCode();
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < _memberChain.Count; i++)
        {
            hash.Add(_memberChain[i]);
        }

        return hash.ToHashCode();
    }

    /// <inheritdoc />
    [DebuggerStepThrough]
    public override bool Equals(object? obj)
        => obj is ProjectionMember projectionMember && Equals(projectionMember);

    private bool Equals(ProjectionMember other)
    {
        if (_memberChain.Count != other._memberChain.Count)
        {
            return false;
        }

        for (var i = 0; i < _memberChain.Count; i++)
        {
            if (!Equals(_memberChain[i], other._memberChain[i]))
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc />
    public override string ToString()
        => _memberChain.Any()
            ? string.Join(".", _memberChain.Select(mi => mi.Name))
            : "EmptyProjectionMember";
}

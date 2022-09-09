// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents the identity of an entity type member, can be based on <see cref="MemberInfo" /> or just the name.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
[DebuggerDisplay("{DebuggerDisplay(),nq}")]
public readonly struct MemberIdentity : IEquatable<MemberIdentity>
{
    private readonly object? _nameOrMember;

    /// <summary>
    ///     Constructs a new <see cref="MemberIdentity" /> from the given member name.
    /// </summary>
    /// <param name="name">The member name.</param>
    [DebuggerStepThrough]
    public MemberIdentity(string name)
        : this((object)name)
    {
    }

    /// <summary>
    ///     Constructs a new <see cref="MemberIdentity" /> from the given <see cref="MemberInfo" />.
    /// </summary>
    /// <param name="memberInfo">The member.</param>
    [DebuggerStepThrough]
    public MemberIdentity(MemberInfo memberInfo)
        : this((object)memberInfo)
    {
    }

    [DebuggerStepThrough]
    private MemberIdentity(object? nameOrMember)
    {
        _nameOrMember = nameOrMember;
    }

    /// <summary>
    ///     A <see cref="MemberIdentity" /> instance that does not represent any member.
    /// </summary>
    public static readonly MemberIdentity None = new((object?)null);

    /// <summary>
    ///     Creates a new <see cref="MemberIdentity" /> from the given member name.
    /// </summary>
    /// <param name="name">The member name.</param>
    /// <returns>The newly created identity, or <see cref="None" /> if the given name is <see langword="null" />.</returns>
    [DebuggerStepThrough]
    public static MemberIdentity Create(string? name)
        => name == null ? None : new MemberIdentity(name);

    /// <summary>
    ///     Creates a new <see cref="MemberIdentity" /> from the given <see cref="MemberInfo" />.
    /// </summary>
    /// <param name="memberInfo">The member.</param>
    /// <returns>The newly created identity, or <see cref="None" /> if the given name is <see langword="null" />.</returns>
    [DebuggerStepThrough]
    public static MemberIdentity Create(MemberInfo? memberInfo)
        => memberInfo == null ? None : new MemberIdentity(memberInfo);

    /// <summary>
    ///     The name of the member.
    /// </summary>
    public string? Name
    {
        [DebuggerStepThrough]
        get => MemberInfo?.GetSimpleMemberName() ?? (string?)_nameOrMember;
    }

    /// <summary>
    ///     The <see cref="MemberInfo" /> representing the member, or <see langword="null" /> if not known.
    /// </summary>
    public MemberInfo? MemberInfo
    {
        [DebuggerStepThrough]
        get => _nameOrMember as MemberInfo;
    }

    private string DebuggerDisplay()
        => Name ?? "NONE";

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is MemberIdentity identity && Equals(identity);

    /// <inheritdoc />
    public bool Equals(MemberIdentity other)
        => EqualityComparer<object>.Default.Equals(_nameOrMember, other._nameOrMember);

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(_nameOrMember);

    /// <summary>
    ///     Compares one id to another id to see if they represent the same member.
    /// </summary>
    /// <param name="left">The first id.</param>
    /// <param name="right">The second id.</param>
    /// <returns><see langword="true" /> if they represent the same member; <see langword="false" /> otherwise.</returns>
    public static bool operator ==(MemberIdentity left, MemberIdentity right)
        => left.Equals(right);

    /// <summary>
    ///     Compares one id to another id to see if they represent different members.
    /// </summary>
    /// <param name="left">The first id.</param>
    /// <param name="right">The second id.</param>
    /// <returns><see langword="true" /> if they represent different members; <see langword="false" /> otherwise.</returns>
    public static bool operator !=(MemberIdentity left, MemberIdentity right)
        => !(left == right);
}

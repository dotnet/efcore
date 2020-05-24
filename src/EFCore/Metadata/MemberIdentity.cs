// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Reflection;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents the identity of an entity type member, can be based on <see cref="MemberInfo" /> or just the name.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay(),nq}")]
    public readonly struct MemberIdentity
    {
        private readonly object _nameOrMember;

        /// <summary>
        ///     Constructs a new <see cref="MemberIdentity" /> from the given member name.
        /// </summary>
        /// <param name="name"> The member name. </param>
        [DebuggerStepThrough]
        public MemberIdentity([NotNull] string name)
            : this((object)name)
        {
        }

        /// <summary>
        ///     Constructs a new <see cref="MemberIdentity" /> from the given <see cref="MemberInfo" />.
        /// </summary>
        /// <param name="memberInfo"> The member. </param>
        [DebuggerStepThrough]
        public MemberIdentity([NotNull] MemberInfo memberInfo)
            : this((object)memberInfo)
        {
        }

        [DebuggerStepThrough]
        private MemberIdentity([CanBeNull] object nameOrMember)
        {
            _nameOrMember = nameOrMember;
        }

        /// <summary>
        ///     Checks if the identity is empty, as opposed to representing a member.
        /// </summary>
        /// <returns> <see langword="true" /> if the identity is empty; <see langword="false" /> otherwise. </returns>
        public bool IsNone() => _nameOrMember == null;

        /// <summary>
        ///     A <see cref="MemberIdentity" /> instance that does not represent any member.
        /// </summary>
        public static readonly MemberIdentity None = new MemberIdentity((object)null);

        /// <summary>
        ///     Creates a new <see cref="MemberIdentity" /> from the given member name.
        /// </summary>
        /// <param name="name"> The member name. </param>
        /// <returns> The newly created identity, or <see cref="None" /> if the given name is <see langword="null" />. </returns>
        [DebuggerStepThrough]
        public static MemberIdentity Create([CanBeNull] string name)
            => name == null ? None : new MemberIdentity(name);

        /// <summary>
        ///     Creates a new <see cref="MemberIdentity" /> from the given <see cref="MemberInfo" />.
        /// </summary>
        /// <param name="memberInfo"> The member. </param>
        /// <returns> The newly created identity, or <see cref="None" /> if the given name is <see langword="null" />. </returns>
        [DebuggerStepThrough]
        public static MemberIdentity Create([CanBeNull] MemberInfo memberInfo)
            => memberInfo == null ? None : new MemberIdentity(memberInfo);

        /// <summary>
        ///     The name of the member.
        /// </summary>
        public string Name
        {
            [DebuggerStepThrough] get => MemberInfo?.GetSimpleMemberName() ?? (string)_nameOrMember;
        }

        /// <summary>
        ///     The <see cref="MemberInfo" /> representing the member, or <see langword="null" /> if not known.
        /// </summary>
        public MemberInfo MemberInfo
        {
            [DebuggerStepThrough] get => _nameOrMember as MemberInfo;
        }

        private string DebuggerDisplay()
            => Name ?? "NONE";
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    [DebuggerDisplay("{DebuggerDisplay(),nq}")]
    public readonly struct MemberIdentity
    {
        private readonly object _nameOrMember;

        [DebuggerStepThrough]
        public MemberIdentity([NotNull] string name)
            : this((object)name)
        {
        }

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

        public bool IsNone() => _nameOrMember == null;

        public static readonly MemberIdentity None = new MemberIdentity((object)null);

        [DebuggerStepThrough]
        public static MemberIdentity Create([CanBeNull] string name)
            => name == null ? None : new MemberIdentity(name);

        [DebuggerStepThrough]
        public static MemberIdentity Create([CanBeNull] MemberInfo member)
            => member == null ? None : new MemberIdentity(member);

        public string Name
        {
            [DebuggerStepThrough] get => MemberInfo?.GetSimpleMemberName() ?? (string)_nameOrMember;
        }

        public MemberInfo MemberInfo
        {
            [DebuggerStepThrough] get => _nameOrMember as MemberInfo;
        }

        private string DebuggerDisplay()
            => Name ?? "NONE";
    }
}

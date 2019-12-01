// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    [DebuggerDisplay("{ToString(), nq}")]
    public class ProjectionMember
    {
        private readonly IList<MemberInfo> _memberChain;

        public ProjectionMember()
        {
            _memberChain = new List<MemberInfo>();
        }

        private ProjectionMember([NotNull] IList<MemberInfo> memberChain)
        {
            Check.NotNull(memberChain, nameof(memberChain));

            _memberChain = memberChain;
        }

        public virtual ProjectionMember Append([NotNull] MemberInfo member)
        {
            Check.NotNull(member, nameof(member));

            var existingChain = _memberChain.ToList();
            existingChain.Add(member);

            return new ProjectionMember(existingChain);
        }

        public virtual ProjectionMember Prepend([NotNull] MemberInfo member)
        {
            Check.NotNull(member, nameof(member));

            var existingChain = _memberChain.ToList();
            existingChain.Insert(0, member);

            return new ProjectionMember(existingChain);
        }

        public virtual MemberInfo Last => _memberChain.LastOrDefault();

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

        public override bool Equals(object obj)
            => obj != null
                && (obj is ProjectionMember projectionMember
                    && Equals(projectionMember));

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

        public override string ToString()
            => _memberChain.Any()
                ? string.Join(".", _memberChain.Select(mi => mi.Name))
                : "EmptyProjectionMember";
    }
}

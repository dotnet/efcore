// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System;

namespace Microsoft.EntityFrameworkCore.Query.Pipeline
{
    public class ProjectionMember
    {
        private readonly IList<MemberInfo> _memberChain;

        public ProjectionMember()
        {
            _memberChain = new List<MemberInfo>();
        }

        private ProjectionMember(IList<MemberInfo> memberChain)
        {
            _memberChain = memberChain;
        }

        public ProjectionMember AddMember(MemberInfo member)
        {
            var existingChain = _memberChain.ToList();
            existingChain.Add(member);

            return new ProjectionMember(existingChain);
        }

        public ProjectionMember ShiftMember(MemberInfo member)
        {
            var existingChain = _memberChain.ToList();
            existingChain.Insert(0, member);

            return new ProjectionMember(existingChain);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return _memberChain.Aggregate(seed: 0, (current, value) => (current * 397) ^ value.GetHashCode());
            }
        }

        public override bool Equals(object obj)
        {
            return obj is null
                ? false
                : obj is ProjectionMember projectionMember
                    && Equals(projectionMember);
        }

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
    }
}

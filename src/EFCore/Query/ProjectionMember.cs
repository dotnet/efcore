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
    /// <summary>
    ///     <para>
    ///         A class representing a chain of CLR members to bind. Usually generated from successive Select calls in the query.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    [DebuggerDisplay("{ToString(), nq}")]
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

        private ProjectionMember([NotNull] IList<MemberInfo> memberChain)
        {
            Check.NotNull(memberChain, nameof(memberChain));

            _memberChain = memberChain;
        }

        /// <summary>
        ///     Append given MemberInfo to existing chain at the end.
        /// </summary>
        /// <param name="member"> The MemberInfo to append. </param>
        /// <returns> A new projection member with given member info appended to existing chain. </returns>
        public ProjectionMember Append([NotNull] MemberInfo member)
        {
            Check.NotNull(member, nameof(member));

            var existingChain = _memberChain.ToList();
            existingChain.Add(member);

            return new ProjectionMember(existingChain);
        }

        /// <summary>
        ///     Prepend given MemberInfo to existing chain at the start.
        /// </summary>
        /// <param name="member"> The MemberInfo to prepend. </param>
        /// <returns> A new projection member with given member info prepended to existing chain. </returns>
        public ProjectionMember Prepend([NotNull] MemberInfo member)
        {
            Check.NotNull(member, nameof(member));

            var existingChain = _memberChain.ToList();
            existingChain.Insert(0, member);

            return new ProjectionMember(existingChain);
        }

        /// <summary>
        ///     <para>
        ///         The last MemberInfo in the chain of MemberInfo represented by this projection member.
        ///     </para>
        ///     <para>
        ///         This method is generally used to get last memberInfo to generate an alias for projection.
        ///     </para>
        /// </summary>
        public MemberInfo Last
            => _memberChain.LastOrDefault();

        /// <inheritdoc />
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

        /// <inheritdoc />
        public override string ToString()
            => _memberChain.Any()
                ? string.Join(".", _memberChain.Select(mi => mi.Name))
                : "EmptyProjectionMember";
    }
}

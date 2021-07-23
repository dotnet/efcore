// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     Represents an operation that should be performed when a navigation is removed from the entity type.
    /// </summary>
    public interface INavigationRemovedConvention : IConvention
    {
        /// <summary>
        ///     Called after a navigation is removed from the entity type.
        /// </summary>
        /// <param name="sourceEntityTypeBuilder"> The builder for the entity type that contained the navigation. </param>
        /// <param name="targetEntityTypeBuilder"> The builder for the target entity type of the navigation. </param>
        /// <param name="navigationName"> The navigation name. </param>
        /// <param name="memberInfo"> The member used for by the navigation. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        void ProcessNavigationRemoved(
            IConventionEntityTypeBuilder sourceEntityTypeBuilder,
            IConventionEntityTypeBuilder targetEntityTypeBuilder,
            string navigationName,
            MemberInfo? memberInfo,
            IConventionContext<string> context);
    }
}

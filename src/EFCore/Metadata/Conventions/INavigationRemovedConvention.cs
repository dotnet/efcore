// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using JetBrains.Annotations;
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
            [NotNull] IConventionEntityTypeBuilder sourceEntityTypeBuilder,
            [NotNull] IConventionEntityTypeBuilder targetEntityTypeBuilder,
            [NotNull] string navigationName,
            [CanBeNull] MemberInfo memberInfo,
            [NotNull] IConventionContext<string> context);
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     Provides reflection objects for late-binding to relational query operations.
    /// </summary>
    public interface IQueryMethodProvider
    {
        /// <summary>
        ///     Gets the group join method.
        /// </summary>
        /// <value>
        ///     The group join method.
        /// </value>
        MethodInfo GroupJoinMethod { get; }

        /// <summary>
        ///     Gets the group by method.
        /// </summary>
        /// <value>
        ///     The group by method.
        /// </value>
        MethodInfo GroupByMethod { get; }

        /// <summary>
        ///     Gets the shaped query method.
        /// </summary>
        /// <value>
        ///     The shaped query method.
        /// </value>
        MethodInfo ShapedQueryMethod { get; }

        /// <summary>
        ///     Gets the default if empty shaped query method.
        /// </summary>
        /// <value>
        ///     The default if empty shaped query method.
        /// </value>
        MethodInfo DefaultIfEmptyShapedQueryMethod { get; }

        /// <summary>
        ///     Gets the query method.
        /// </summary>
        /// <value>
        ///     The query method.
        /// </value>
        MethodInfo QueryMethod { get; }

        /// <summary>
        ///     Gets the get result method.
        /// </summary>
        /// <value>
        ///     The get result method.
        /// </value>
        MethodInfo GetResultMethod { get; }

        /// <summary>
        ///     Gets the include method.
        /// </summary>
        /// <value>
        ///     The include method.
        /// </value>
        MethodInfo IncludeMethod { get; }

        /// <summary>
        ///     Gets the type of the related entities loader.
        /// </summary>
        /// <value>
        ///     The type of the related entities loader.
        /// </value>
        Type RelatedEntitiesLoaderType { get; }

        /// <summary>
        ///     Gets the create reference related entities loader method.
        /// </summary>
        /// <value>
        ///     The create reference related entities loader method.
        /// </value>
        MethodInfo CreateReferenceRelatedEntitiesLoaderMethod { get; }

        /// <summary>
        ///     Gets the create collection related entities loader method.
        /// </summary>
        /// <value>
        ///     The create collection related entities loader method.
        /// </value>
        MethodInfo CreateCollectionRelatedEntitiesLoaderMethod { get; }

        /// <summary>
        ///     Gets the inject parameters method.
        /// </summary>
        /// <value>
        ///     The pre execute method.
        /// </value>
        MethodInfo InjectParametersMethod { get; }

        /// <summary>
        ///     Gets the type of the group join include.
        /// </summary>
        /// <value>
        ///     The type of the group join include.
        /// </value>
        Type GroupJoinIncludeType { get; }

        /// <summary>
        ///     Creates a group join include used to describe an Include operation that should
        ///     be performed as part of a GroupJoin.
        /// </summary>
        /// <param name="navigationPath"> The included navigation path. </param>
        /// <param name="querySourceRequiresTracking"> true if this query source requires tracking. </param>
        /// <param name="existingGroupJoinInclude"> A possibly null existing group join include. </param>
        /// <param name="relatedEntitiesLoaders"> The related entities loaders. </param>
        /// <returns>
        ///     A new group join include.
        /// </returns>
        object CreateGroupJoinInclude(
            [NotNull] IReadOnlyList<INavigation> navigationPath,
            bool querySourceRequiresTracking,
            [CanBeNull] object existingGroupJoinInclude,
            [NotNull] object relatedEntitiesLoaders);
    }
}

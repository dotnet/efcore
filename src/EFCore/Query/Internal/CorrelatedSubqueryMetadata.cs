// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Remotion.Linq.Clauses;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     Structure to store metadata needed for correlated collection optimizations.
    /// </summary>
    public class CorrelatedSubqueryMetadata
    {
        /// <summary>
        ///     Creates a new <see cref="CorrelatedSubqueryMetadata" />.
        /// </summary>
        /// <param name="index"> Id associated with the collection that is being optimized. </param>
        /// <param name="trackingQuery"> Flag indicating whether query should be tracked or not. </param>
        /// <param name="firstNavigation"> First navigation in the chain leading to collection navigation that is being optimized. </param>
        /// <param name="collectionNavigation"> Collection navigation that is being optimized. </param>
        /// <param name="parentQuerySource"> Query source that is origin of the collection navigation. </param>
        public CorrelatedSubqueryMetadata(
            int index,
            bool trackingQuery,
            [NotNull] INavigation firstNavigation,
            [NotNull] INavigation collectionNavigation,
            [NotNull] IQuerySource parentQuerySource)
        {
            Index = index;
            TrackingQuery = trackingQuery;
            FirstNavigation = firstNavigation;
            CollectionNavigation = collectionNavigation;
            ParentQuerySource = parentQuerySource;
        }

        /// <summary>
        ///     Id associated with the collection that is being optimized.
        /// </summary>
        public virtual int Index { get; }

        /// <summary>
        ///     Flag indicating whether query should be tracked or not.
        /// </summary>
        public virtual bool TrackingQuery { get; }

        /// <summary>
        ///     First navigation in the chain leading to collection navigation that is being optimized.
        /// </summary>
        public virtual INavigation FirstNavigation { get; }

        /// <summary>
        ///     Collection navigation that is being optimized.
        /// </summary>
        public virtual INavigation CollectionNavigation { get; }

        /// <summary>
        ///     Query source that is origin of the collection navigation.
        /// </summary>
        public virtual IQuerySource ParentQuerySource { get; internal set; }
    }
}

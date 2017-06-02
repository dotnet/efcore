// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     Represents a single query include operation.
    /// </summary>
    public class IncludeSpecification
    {
        /// <summary>
        ///     Initializes a new instance of the Microsoft.EntityFrameworkCore.Query.IncludeResultOperator class.
        /// </summary>
        /// <param name="querySource"> The query source. </param>
        /// <param name="navigationPath"> The set of navigation properties to be included. </param>
        public IncludeSpecification(
            [NotNull] IQuerySource querySource,
            [NotNull] IReadOnlyList<INavigation> navigationPath)
        {
            Check.NotNull(querySource, nameof(querySource));
            Check.NotNull(navigationPath, nameof(navigationPath));

            QuerySource = querySource;
            NavigationPath = navigationPath;
        }

        /// <summary>
        ///     Gets the query source.
        /// </summary>
        /// <value>
        ///     The query source.
        /// </value>
        public virtual IQuerySource QuerySource { get; }

        /// <summary>
        ///     Gets the set of navigation properties to be included.
        /// </summary>
        /// <value>
        ///     The set of navigation properties to be included.
        /// </value>
        public virtual IReadOnlyList<INavigation> NavigationPath { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether this object is an enumerable target.
        /// </summary>
        /// <value>
        ///     True if this object is an enumerable target, false if not.
        /// </value>
        public virtual bool IsEnumerableTarget { get; set; }

        /// <summary>
        ///     Convert this object into a string representation.
        /// </summary>
        /// <returns>
        ///     A string that represents this object.
        /// </returns>
        public override string ToString()
            => $"{QuerySource.ItemName}.{NavigationPath.Select(n => n.Name).Join(".")}"; // Interpolation okay; strings
    }
}

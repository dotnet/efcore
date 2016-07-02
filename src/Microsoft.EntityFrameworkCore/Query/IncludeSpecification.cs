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
        ///     Initializes a new instance of the Microsoft.EntityFrameworkCore.Query.IncludeSpecification class.
        /// </summary>
        /// <param name="querySource"> The query source. </param>
        /// <param name="navigation"></param>
        /// <param name="references"></param>
        public IncludeSpecification(
            [NotNull] IQuerySource querySource,
            [NotNull] INavigation navigation,
            [CanBeNull] IReadOnlyList<IncludeSpecification> references)
        {
            Check.NotNull(querySource, nameof(querySource));
            Check.NotNull(navigation, nameof(navigation));

            QuerySource = querySource;
            Navigation = navigation;
            References = references ?? new List<IncludeSpecification>();
        }

        /// <summary>
        /// 
        /// </summary>
        public INavigation Navigation { get; }

        /// <summary>
        ///     Gets the query source.
        /// </summary>
        /// <value>
        ///     The query source.
        /// </value>
        public virtual IQuerySource QuerySource { get; }

        /// <summary>
        ///     Gets the set of navigation references to be included.
        /// </summary>
        /// <value>
        ///     The set of navigation references to be included.
        /// </value>
        public virtual IReadOnlyList<IncludeSpecification> References { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether this object is an enumerable target.
        /// </summary>
        /// <value>
        ///     True if this object is an enumerable target, false if not.
        /// </value>
        public virtual bool IsEnumerableTarget { get; set; }

        private string Format(int level)
        {
            return $"{QuerySource.ItemName}.{Navigation.Name}{(References.Count > 0 ? "->\n" + new string('\t', level + 1) : "")}" +
                   References.Select(r => r.Format(level + 1)).Join("\n" + new string('\t', level + 1));
        }

        /// <summary>
        ///     Convert this object into a string representation.
        /// </summary>
        /// <returns>
        ///     A string that represents this object.
        /// </returns>
        public override string ToString()
        {
            return Format(0);
        }
    }
}

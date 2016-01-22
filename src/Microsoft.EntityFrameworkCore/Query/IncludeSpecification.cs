// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Query
{
    public class IncludeSpecification
    {
        public IncludeSpecification(
            [NotNull] IQuerySource querySource,
            [NotNull] IReadOnlyList<INavigation> navigationPath)
        {
            Check.NotNull(querySource, nameof(querySource));
            Check.NotNull(navigationPath, nameof(navigationPath));

            QuerySource = querySource;
            NavigationPath = navigationPath;
        }

        public virtual IQuerySource QuerySource { get; }
        public virtual IReadOnlyList<INavigation> NavigationPath { get; }
    }
}

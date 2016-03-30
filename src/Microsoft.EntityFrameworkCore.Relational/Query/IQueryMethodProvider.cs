// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Query
{
    public interface IQueryMethodProvider
    {
        MethodInfo GroupJoinMethod { get; }
        MethodInfo GroupByMethod { get; }
        MethodInfo ShapedQueryMethod { get; }
        MethodInfo DefaultIfEmptyShapedQueryMethod { get; }
        MethodInfo QueryMethod { get; }
        MethodInfo GetResultMethod { get; }
        MethodInfo IncludeMethod { get; }

        Type RelatedEntitiesLoaderType { get; }

        MethodInfo CreateReferenceRelatedEntitiesLoaderMethod { get; }
        MethodInfo CreateCollectionRelatedEntitiesLoaderMethod { get; }

        Type GroupJoinIncludeType { get; }

        object CreateGroupJoinInclude(
            [NotNull] IReadOnlyList<INavigation> navigationPath,
            bool querySourceRequiresTracking,
            [CanBeNull] object existingGroupJoinInclude,
            [NotNull] object relatedEntitiesLoaders);
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public interface IEntityMaterializerSource
    {
        Expression CreateReadValueExpression([NotNull] Expression valueBuffer, [NotNull] Type type, int index);

        Expression CreateReadValueCallExpression([NotNull] Expression valueBuffer, int index);

        Expression CreateMaterializeExpression(
            [NotNull] IEntityType entityType,
            [NotNull] Expression valueBufferExpression,
            [CanBeNull] int[] indexMap = null);
    }
}

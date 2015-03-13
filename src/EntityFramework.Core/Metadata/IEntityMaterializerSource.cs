// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata
{
    public interface IEntityMaterializerSource
    {
        Expression CreateReadValueExpression(
            [NotNull] Expression valueReader, [NotNull] Type type, int index);

        Expression CreateMaterializeExpression(
            [NotNull] IEntityType entityType,
            [NotNull] Expression valueReaderExpression,
            [CanBeNull] int[] indexMap = null);
    }
}
